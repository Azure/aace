using System;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.DataContracts;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Services.CustomMeterEvent;
using Luna.Services.Data;
using Luna.Services.Marketplace;
using Luna.Services.Provisoning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for the apiSubscription resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class APISubscriptionController : ControllerBase
    {
        private readonly IAPISubscriptionService _apiSubscriptionService;
        private readonly ILogger<APISubscriptionController> _logger;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="apiSubscriptionService">The apiSubscription service instance</param>
        /// <param name="logger">The logger.</param>
        public APISubscriptionController(IAPISubscriptionService apiSubscriptionService, 
            ILogger<APISubscriptionController> logger)
        {
            _apiSubscriptionService = apiSubscriptionService ?? throw new ArgumentNullException(nameof(apiSubscriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all active apiSubscriptions.
        /// </summary>
        /// <returns>HTTP 200 OK with apiSubscription JSON objects in response body.</returns>
        [HttpGet("apiSubscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(APISubscription).Name));

            string owner = "";
            if (Request.Query.ContainsKey("owner"))
            {
                owner = Request.Query["owner"].ToString();
                _logger.LogInformation($"APISubscription owner name: {owner}.");
                AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, owner);
            }
            else
            {
                // user can only query their own apiSubscriptions
                AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            }

            string[] statusList = null;
            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                object statusEnum;
                if (Enum.TryParse(typeof(FulfillmentState), status, true, out statusEnum))
                {
                    _logger.LogInformation($"Getting apiSubscriptions in {status} state.");
                    statusList = new string[] { status };
                }
                else
                {
                    _logger.LogInformation($"Getting active apiSubscriptions only");
                    statusList = new string[] {nameof(FulfillmentState.PendingFulfillmentStart),
                        nameof(FulfillmentState.Subscribed),
                        nameof(FulfillmentState.Suspended)};
                }
            }

            return Ok(await _apiSubscriptionService.GetAllAsync(status: statusList, owner: owner));
        }

        /// <summary>
        /// Gets a apiSubscription.
        /// </summary>
        /// <param name="apiSubscriptionId">The apiSubscription id.</param>
        /// <returns>HTTP 200 OK with apiSubscription JSON object in response body.</returns>
        [HttpGet("apiSubscriptions/{apiSubscriptionId}", Name = nameof(GetAsync) + nameof(APISubscription))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(Guid apiSubscriptionId)
        {
            _logger.LogInformation($"Get apiSubscription {apiSubscriptionId}.");
            var apiSubscription = await _apiSubscriptionService.GetAsync(apiSubscriptionId);
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, apiSubscription.UserId);
            return Ok(apiSubscription);
        }

        [HttpPost("apiSubscriptions/CreateWithId")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateAsync([FromQuery] string subscriptionName, 
            [FromQuery] Guid subscriptionId,
            [FromQuery] string productName,
            [FromQuery] string deploymentName,
            [FromQuery] string userId)
        {
            APISubscription apiSubscription = new APISubscription()
            {
                SubscriptionName = subscriptionName,
                SubscriptionId = subscriptionId,
                ProductName = productName,
                DeploymentName = deploymentName,
                UserId = userId
            };

            return await CreateInternal(apiSubscription);
        }

        [HttpPost("apiSubscriptions/Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateAsync([FromBody] APISubscription apiSubscription)
        {
            if (apiSubscription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubscription)), UserErrorCode.PayloadNotProvided);
            }

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, apiSubscription.UserId);

            return await CreateInternal(apiSubscription);
        }

        private async Task<ActionResult> CreateInternal(APISubscription apiSubscription)
        {
            _logger.LogInformation($"Create apiSubscription {apiSubscription.SubscriptionName} with payload {JsonSerializer.Serialize(apiSubscription)}.");
            // Create a new apiSubscription
            await _apiSubscriptionService.CreateAsync(apiSubscription);
            return CreatedAtRoute(nameof(GetAsync) + nameof(APISubscription), new
            {
                apiSubscriptionId = apiSubscription.SubscriptionId
            }, apiSubscription);
        }

        /// <summary>
        /// Update a apiSubscription
        /// </summary>
        /// <param name="apiSubscriptionId">The apiSubscription id.</param>
        /// <param name="apiSubscription">The apiSubscription object</param>
        /// <returns>The apiSubscription info</returns>
        [HttpPut("apiSubscriptions/{apiSubscriptionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateAsync(Guid apiSubscriptionId, [FromBody] APISubscription apiSubscription)
        {
            if (apiSubscription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubscription)), UserErrorCode.PayloadNotProvided);
            }

            if (!apiSubscriptionId.Equals(apiSubscription.SubscriptionId))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APISubscription).Name),
                    UserErrorCode.NameMismatch);
            }
            _logger.LogInformation($"Update apiSubscription {apiSubscriptionId} with payload {JsonSerializer.Serialize(apiSubscription)}.");
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, apiSubscription.UserId);

            await _apiSubscriptionService.UpdateAsync(apiSubscriptionId, apiSubscription);
            return Ok(await _apiSubscriptionService.GetAsync(apiSubscriptionId));

        }

        /// <summary>
        /// Deletes a apiSubscription.
        /// </summary>
        /// <param name="apiSubscriptionId">The subcription id.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("apiSubscriptions/{apiSubscriptionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(Guid apiSubscriptionId)
        {
            var apiSubscription = await _apiSubscriptionService.GetAsync(apiSubscriptionId);

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false, apiSubscription.UserId);

            _logger.LogInformation($"Delete apiSubscription {apiSubscriptionId}.");
            await _apiSubscriptionService.DeleteAsync(apiSubscriptionId);
            return NoContent();
        }

        [HttpPost("apiSubscriptions/Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AllowAnonymous]
        public async Task<ActionResult> DeleteByPostAsync()
        {
            Guid apiSubscriptionId;
            if (Request.Query.ContainsKey("SubscriptionId") && Guid.TryParse(Request.Query["SubscriptionId"].ToString(), out apiSubscriptionId))
            {
                _logger.LogInformation($"Delete apiSubscription {apiSubscriptionId}.");
                await _apiSubscriptionService.DeleteAsync(apiSubscriptionId);
                return NoContent();
            }
            else
            {
                throw new LunaBadRequestUserException("The query parameter SubscriptionId is not found.", UserErrorCode.InvalidParameter);
            }
        }

        /// <summary>
        /// Regenerate key for a apiSubscription
        /// </summary>
        /// <param name="apiSubscriptionId">The apiSubscription id</param>
        /// <returns>The apiSubscription</returns>
        [HttpPost("apiSubscriptions/{apiSubscriptionId}/regenerateKey")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RegenerateKey(Guid apiSubscriptionId, [FromBody] APISubscriptionKeyName keyName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Regenerate {keyName.KeyName} key for apiSubscription {apiSubscriptionId}.");

            if (!await _apiSubscriptionService.ExistsAsync(apiSubscriptionId))
            {
                throw new LunaNotFoundUserException($"The specified apiSubscription {apiSubscriptionId} doesn't exist or you don't have permission to access it.");
            }

            return Ok(await _apiSubscriptionService.RegenerateKey(apiSubscriptionId, keyName.KeyName));
        }
    }
}