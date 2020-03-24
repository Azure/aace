using System;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Services.CustomMeterEvent;
using Luna.Services.Data;
using Luna.Services.Marketplace;
using Luna.Services.Provisoning;
using Luna.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for the subscription resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IFulfillmentManager _fulfillmentManager;
        private readonly IProvisioningService _provisioningService;
        private readonly ICustomMeterEventService _customMeterEventService;
        private readonly ILogger<SubscriptionController> _logger;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="subscriptionService">The subscription service instance</param>
        /// <param name="fulfillmentManager">The fulfillmentManager instance</param>
        /// <param name="provisioningService">The provisioning service instance</param>
        /// <param name="logger">The logger.</param>
        public SubscriptionController(ISubscriptionService subscriptionService, IFulfillmentManager fulfillmentManager,
            IProvisioningService provisioningService, ICustomMeterEventService customMeterEventService, ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _fulfillmentManager = fulfillmentManager ?? throw new ArgumentNullException(nameof(fulfillmentManager));
            _provisioningService = provisioningService ?? throw new ArgumentNullException(nameof(provisioningService));
            _customMeterEventService = customMeterEventService ?? throw new ArgumentNullException(nameof(customMeterEventService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all active subscriptions.
        /// </summary>
        /// <returns>HTTP 200 OK with subscription JSON objects in response body.</returns>
        [HttpGet("subscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Subscription).Name));

            string owner = "";
            if (Request.Query.ContainsKey("owner"))
            {
                owner = Request.Query["owner"].ToString();
                _logger.LogInformation($"Subscription owner name: {owner}.");
                AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, owner);
            }
            else
            {
                // user can only query their own subscriptions
                AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            }

            string[] statusList = null;
            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                object statusEnum;
                if (Enum.TryParse(typeof(FulfillmentState), status, true, out statusEnum))
                {
                    _logger.LogInformation($"Getting subscriptions in {status} state.");
                    statusList = new string[] { status };
                }
                else
                {
                    _logger.LogInformation($"Getting active subscriptions only");
                    statusList = new string[] {nameof(FulfillmentState.PendingFulfillmentStart),
                        nameof(FulfillmentState.Subscribed),
                        nameof(FulfillmentState.Suspended)};
                }
            }

            return Ok(await _subscriptionService.GetAllAsync(status: statusList, owner: owner));
        }

        /// <summary>
        /// Gets all deleted subscriptions.
        /// </summary>
        /// <returns>HTTP 200 OK with subscription JSON objects in response body.</returns>
        [HttpGet("deletedSubscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllDeletedAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Subscription).Name));
            _logger.LogInformation($"Deleted subscriptions only");
            string owner = "";
            if (Request.Query.ContainsKey("owner"))
            {
                owner = Request.Query["owner"].ToString();
                _logger.LogInformation($"Subscription owner name: {owner}.");
            }

            string[] status = new string[] {nameof(FulfillmentState.Purged),
                nameof(FulfillmentState.Unsubscribed)};

            return Ok(await _subscriptionService.GetAllAsync(status: status, owner: owner));
        }

        /// <summary>
        /// Gets a subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>HTTP 200 OK with subscription JSON object in response body.</returns>
        [HttpGet("subscriptions/{subscriptionId}", Name = nameof(GetAsync) + nameof(Subscription))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(Guid subscriptionId)
        {
            _logger.LogInformation($"Get subscription {subscriptionId}.");
            var subscription = await _subscriptionService.GetAsync(subscriptionId);

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, subscription.Owner);
            return Ok(subscription);
        }

        /// <summary>
        /// Create or update a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="subscription">The subscription object</param>
        /// <returns>The subscription info</returns>
        [HttpPut("subscriptions/{subscriptionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(Guid subscriptionId, [FromBody] Subscription subscription)
        {
            if (subscription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(subscription)), UserErrorCode.PayloadNotProvided);
            }

            if (!subscriptionId.Equals(subscription.SubscriptionId))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Subscription).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _subscriptionService.ExistsAsync(subscriptionId))
            {
                _logger.LogInformation($"Update subscription {subscriptionId} with payload {JsonSerializer.Serialize(subscription)}.");
                var sub = await _subscriptionService.GetAsync(subscriptionId);
                AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, sub.Owner);
                if (!sub.OfferName.Equals(subscription.OfferName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new LunaBadRequestUserException("Offer name of an existing subscription can not be changed.", UserErrorCode.InvalidParameter);
                }

                if (!string.IsNullOrEmpty(subscription.Owner) && !sub.Owner.Equals(subscription.Owner, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new LunaBadRequestUserException("Owner name of an existing subscription can not be changed.", UserErrorCode.InvalidParameter);
                }
                
                if (sub.PlanName.Equals(subscription.PlanName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new LunaConflictUserException($"The subscription {subscription.SubscriptionId} is already in plan {subscription.PlanName}.");
                }
                // Update existing subscription
                await _fulfillmentManager.RequestUpdateSubscriptionAsync(subscriptionId, subscription.PlanName);
                return Ok(await _subscriptionService.GetAsync(subscriptionId));
            }
            else
            {
                _logger.LogInformation($"Create subscription {subscriptionId} with payload {JsonSerializer.Serialize(subscription)}.");
                // Create a new subscription
                AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, subscription.Owner);
                await _subscriptionService.CreateAsync(subscription);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Subscription), new { subscriptionId = subscription.SubscriptionId }, subscription);
            }

        }

        /// <summary>
        /// Deletes a subscription.
        /// </summary>
        /// <param name="subscriptionId">The subcription id.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("subscriptions/{subscriptionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(Guid subscriptionId)
        {
            var subscription = await _subscriptionService.GetAsync(subscriptionId);

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, subscription.Owner);

            _logger.LogInformation($"Delete subscription {subscriptionId}.");
            await _fulfillmentManager.RequestCancelSubscriptionAsync(subscriptionId);
            return NoContent();
        }

        /// <summary>
        /// Resolve the token for the landing page
        /// </summary>
        /// <param name="token">the token</param>
        /// <returns>The token info</returns>
        [HttpPost("subscriptions/resolveToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ResolveToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(token)), UserErrorCode.PayloadNotProvided);
            }

            // Do not log token content!
            _logger.LogInformation($"Resolve token for a subscription.");
            MarketplaceSubscription resolvedSubscription = await _fulfillmentManager.ResolveSubscriptionAsync(token);

            return Ok(resolvedSubscription);
        }

        /// <summary>
        /// Activate a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Activate(Guid subscriptionId)
        {
            string activatedBy = this.HttpContext.User.Identity.Name;
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Activate subscription {subscriptionId}. Activated by {activatedBy}.");

            if (!await _subscriptionService.ExistsAsync(subscriptionId))
            {
                throw new LunaNotFoundUserException($"The specified subscription {subscriptionId} doesn't exist or you don't have permission to access it.");
            }

            Subscription sub = await _subscriptionService.GetAsync(subscriptionId);

            if (!sub.Status.Equals(nameof(FulfillmentState.PendingFulfillmentStart), StringComparison.InvariantCultureIgnoreCase))
            {
                throw new LunaConflictUserException($"The specified subscription is in {sub.Status} state. It can not be activated.");
            }

            if (string.IsNullOrEmpty(activatedBy))
            {
                throw new LunaBadRequestUserException("Need to specify the operation who is activating this subscription.", UserErrorCode.InvalidParameter);
            }

            return Ok(await _provisioningService.ActivateSubscriptionAsync(subscriptionId, activatedBy));
        }


        /// <summary>
        /// Delete data from a subscirption
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/deleteData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteData(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete data (purge) from subscription {subscriptionId}.");
            return Ok(await _subscriptionService.DeleteDataAsync(subscriptionId));
        }

        /// <summary>
        /// Complete a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/completeOperation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CompleteOperation(Guid subscriptionId)
        {
            string activatedBy = this.HttpContext.User.Identity.Name;
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Complete current operation for subscription {subscriptionId}. Operated by {activatedBy}.");

            if (!await _subscriptionService.ExistsAsync(subscriptionId))
            {
                throw new LunaNotFoundUserException($"The specified subscription {subscriptionId} doesn't exist or you don't have permission to access it.");
            }

            Subscription sub = await _subscriptionService.GetAsync(subscriptionId);

            if (!ProvisioningHelper.IsErrorOrWarningProvisioningState(sub.ProvisioningStatus))
            {
                throw new LunaConflictUserException($"Can not complete operation manually when provisioning in {sub.ProvisioningStatus} state.");
            }

            if (string.IsNullOrEmpty(activatedBy))
            {
                throw new LunaBadRequestUserException("Need to specify the operation who is activating this subscription.", UserErrorCode.InvalidParameter);
            }

            return Ok(await _provisioningService.UpdateOperationCompletedAsync(subscriptionId, activatedBy));
        }

        /// <summary>
        /// Get the subscription operation history
        /// </summary>
        /// <param name="subscriptionId">the subscription id</param>
        /// <returns>The operations</returns>
        [HttpGet("subscriptions/{subscriptionId}/operations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetOperations(Guid subscriptionId)
        {
            _logger.LogInformation($"Get all operations for subscription {subscriptionId}.");

            var subscription = await _subscriptionService.GetAsync(subscriptionId);
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, subscription.Owner);
            return Ok(await _fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionId));
        }

        /// <summary>
        /// Get all subscription warnings
        /// </summary>
        /// <returns></returns>
        [HttpGet("subscriptions/warnings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllWarnings()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get warnings for all subscriptions.");
            return Ok(await _subscriptionService.GetWarnings());
        }

        /// <summary>
        /// Get warnings for a specific subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        [HttpGet("subscriptions/{subscriptionId}/warnings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetWarnings(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get warnings for subscription {subscriptionId}.");
            return Ok(await _subscriptionService.GetWarnings(subscriptionId));
        }

        [HttpPost("subscriptions/processCustomMeterEvents")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult> ProcessCustomMeterEvents()
        {
            await _customMeterEventService.ReportBatchMeterEvents();
            return Accepted();
        }
    }
}