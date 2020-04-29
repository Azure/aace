using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Fulfillment;
using Luna.Clients.Logging;
using Luna.Data.DataContracts;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Luna.Services.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the subscription resource.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly IPlanService _planService;
        private readonly IOfferParameterService _offerParameterService;
        private readonly ICustomMeterService _customMeterService;
        private readonly ICustomMeterDimensionService _customMeterDimensionService;
        private readonly ILogger<SubscriptionService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">A service to inject.</param>
        /// <param name="planService">A service to inject.</param>
        /// <param name="customMeterDimensionService">A service to inject.</param>
        /// <param name="customMeterService">A service to inject.</param>
        /// <param name="offerParameterService">A service to inject.</param>
        /// <param name="logger">The logger.</param>
        public SubscriptionService(ISqlDbContext sqlDbContext,
            IOfferService offerService,
            IPlanService planService,
            IOfferParameterService offerParameterService,
            ICustomMeterDimensionService customMeterDimensionService,
            ICustomMeterService customMeterService,
            ILogger<SubscriptionService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _customMeterDimensionService = customMeterDimensionService ?? throw new ArgumentNullException(nameof(customMeterDimensionService));
            _customMeterService = customMeterService ?? throw new ArgumentNullException(nameof(customMeterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerParameterService = offerParameterService ?? throw new ArgumentNullException(nameof(offerParameterService));
        }

        /// <summary>
        /// Gets all subscriptions.
        /// </summary>
        /// <param name="status">The list of status of the subscription.</param>
        /// <param name="owner">The owner of the subscription.</param>
        /// <returns>A list of all subsrciptions.</returns>
        public async Task<List<Subscription>> GetAllAsync(string[] status = null, string owner = "")
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Subscription).Name));

            // Gets all subscriptions that have the provided status.
            List<Subscription> allSub = await _context.Subscriptions.ToListAsync();

            List<Subscription> subscriptionList = allSub.Where(s => (status == null || status.Contains(s.Status)) &&
                    (string.IsNullOrEmpty(owner) || s.Owner.Equals(owner, StringComparison.InvariantCultureIgnoreCase))).ToList();

            foreach (var sub in subscriptionList)
            {
                sub.PlanName = (await _context.Plans.FindAsync(sub.PlanId)).PlanName;
                sub.OfferName = (await _context.Offers.FindAsync(sub.OfferId)).OfferName;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Subscription).Name, subscriptionList.Count()));

            return subscriptionList;
        }


        /// <summary>
        /// Get all active subscription by offer name
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <returns>The list of subscriptions</returns>
        public async Task<List<Subscription>> GetAllActiveByOfferName(string offerName)
        {
            var offer = await _offerService.GetAsync(offerName);
            //TODO: error handling

            List<Subscription> allSub = await _context.Subscriptions.ToListAsync();

            List<Subscription> subscriptionList = allSub.Where(s => s.OfferId == offer.Id).ToList();
            foreach (var sub in subscriptionList)
            {
                sub.PlanName = (await _context.Plans.FindAsync(sub.PlanId)).PlanName;
                sub.OfferName = offerName;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Subscription).Name, subscriptionList.Count()));

            return subscriptionList;

        }

        /// <summary>
        /// Gets a subscription by id.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription.</param>
        /// <returns>The subscription.</returns>
        public async Task<Subscription> GetAsync(Guid subscriptionId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Subscription).Name, subscriptionId.ToString()));

            // Find the subscription that matches the subscriptionId provided
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);

            // Check if subscription exists
            if (subscription is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Subscription).Name,
                    subscriptionId.ToString()));
            }

            subscription.OfferName = (await _context.Offers.FindAsync(subscription.OfferId)).OfferName;
            subscription.PlanName = (await _context.Plans.FindAsync(subscription.PlanId)).PlanName;
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Subscription).Name,
                subscriptionId.ToString(),
                JsonSerializer.Serialize(subscription)));

            return subscription; //Task.FromResult(subscription);
        }

        /// <summary>
        /// Creates a subscription within a plan within an offer.
        /// </summary>
        /// <param name="subscription">The subscription to create.</param>
        /// <returns>The created subscription.</returns>
        public async Task<Subscription> CreateAsync(Subscription subscription)
        {
            if (subscription is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Subscription).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (await ExistsAsync(subscription.SubscriptionId))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Subscription).Name,
                    subscription.SubscriptionId.ToString()));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Subscription).Name, subscription.Name, offerName: subscription.OfferName, planName: subscription.PlanName, payload: JsonSerializer.Serialize(subscription)));

            var offerParameters = await _offerParameterService.GetAllAsync(subscription.OfferName);

            foreach (var param in offerParameters)
            {
                // Check if value of all offer parameters are provided with correct type
                if (subscription.InputParameters.Where(x => x.Name.Equals(param.ParameterName) && x.Type.Equals(param.ValueType)).Count() < 1)
                {
                    throw new LunaBadRequestUserException($"Value of parameter {param.ParameterName} is not provided, or the type doesn't match.", UserErrorCode.ParameterNotProvided);
                }
            }

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(subscription.OfferName);


            // Get the plan associated with the planUniqueName provided
            var plan = await _planService.GetAsync(subscription.OfferName, subscription.PlanName);

            // Set the FK to offer
            subscription.OfferId = offer.Id;

            // Set the FK to plan
            subscription.PlanId = plan.Id;

            // Always set quantity to 1 to walkaround a marketplace service bug
            subscription.Quantity = 1;

            // Set the created time
            subscription.CreatedTime = DateTime.UtcNow;

            subscription.Status = nameof(FulfillmentState.PendingFulfillmentStart);

            subscription.ProvisioningStatus = nameof(ProvisioningState.ProvisioningPending);

            subscription.ProvisioningType = nameof(ProvisioningType.Subscribe);

            subscription.RetryCount = 0;

            List<CustomMeter> customMeterList = await _customMeterService.GetAllAsync(offer.OfferName);

            using (var transaction = await _context.BeginTransactionAsync())
            {
                // Add subscription to db
                _context.Subscriptions.Add(subscription);
                await _context._SaveChangesAsync();

                // Add subscription parameters
                foreach (var param in subscription.InputParameters)
                {
                    param.SubscriptionId = subscription.SubscriptionId;
                    _context.SubscriptionParameters.Add(param);
                }
                await _context._SaveChangesAsync();

                foreach (var meter in customMeterList)
                {
                    _context.SubscriptionCustomMeterUsages.Add(new SubscriptionCustomMeterUsage(meter.Id, subscription.SubscriptionId));
                }

                await _context._SaveChangesAsync();

                transaction.Commit();
            }
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Subscription).Name, subscription.Name, offerName: subscription.OfferName, planName: subscription.PlanName));

            return subscription;
        }

        public void CheckSubscriptionInReadyState(Subscription subscription)
        {
            if (!subscription.Status.Equals(nameof(FulfillmentState.Subscribed)))
            {
                throw new NotSupportedException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscription.SubscriptionId, SubscriptionAction.Update.ToVerb(), invalidFulfillmentState: nameof(subscription.Status)));
            }

            if (!subscription.ProvisioningStatus.Equals(nameof(ProvisioningState.Succeeded)))
            {
                throw new NotSupportedException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscription.SubscriptionId, SubscriptionAction.Update.ToVerb(), invalidProvisioningState: true));
            }

            _logger.LogInformation($"Subscription {subscription.SubscriptionId} is in the Subscribed fulfillment state and Succeeded provisioining state.");
        }

        /// <summary>
        /// Updates a subscription.
        /// </summary>
        /// <param name="subscription">The updated subscription.</param>
        /// <returns>The updated subscription.</returns>
        public async Task<Subscription> UpdateAsync(Subscription subscription, Guid operationId)
        {
            if (subscription is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Subscription).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Subscription).Name, subscription.Name, offerName: subscription.OfferName, planName: subscription.PlanName, payload: JsonSerializer.Serialize(subscription)));
            var newPlanName = subscription.PlanName;
            var newQuantity = subscription.Quantity;

            Subscription subscriptionDb;
            try
            {
                // Get the subscription that matches the subscriptionId provided
                subscriptionDb = await GetAsync(subscription.SubscriptionId);
            }
            catch (Exception)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscription.SubscriptionId, SubscriptionAction.Update.ToVerb()), UserErrorCode.ResourceNotFound);
            }

            CheckSubscriptionInReadyState(subscriptionDb);

            // Get the offer and plan associated with the subscriptionId provided
            var offer = await _context.Offers.FindAsync(subscriptionDb.OfferId);
            var plan = await _context.Plans.FindAsync(subscriptionDb.PlanId);

            if (newPlanName != plan.PlanName && subscriptionDb.Quantity != newQuantity)
            {
                throw new ArgumentException("Cannot update plan and quantity at the same time.");
            }

            // Check if the plan has been upgraded or downgraded
            if (newPlanName != plan.PlanName)
            {
                _logger.LogInformation($"Updating subscription {subscription.SubscriptionId} from plan {plan.PlanName} to {newPlanName}.");
                // Get the new plan to change to 
                var newPlan = await _planService.GetAsync(offer.OfferName, newPlanName);

                // Update the FK to the new plan
                subscription.OperationId = operationId;
                subscriptionDb.PlanId = newPlan.Id;
                subscriptionDb.ProvisioningStatus = nameof(ProvisioningState.ArmTemplatePending);
                subscriptionDb.ProvisioningType = nameof(ProvisioningType.Update);
            }
            else if (subscriptionDb.Quantity != subscription.Quantity)
            {
                _logger.LogInformation($"Updating subscription {subscription.SubscriptionId} from quantity {subscriptionDb.Quantity} to {subscription.Quantity}");
                subscriptionDb.Quantity = newQuantity;
                //TODO: what to do?
            }

            // Set the updated time
            subscriptionDb.LastUpdatedTime = DateTime.UtcNow;

            // Update subscriptionDb values and save changes in db
            _context.Subscriptions.Update(subscriptionDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Subscription).Name, subscription.Name, offerName: subscription.OfferName, planName: subscription.PlanName));

            return subscriptionDb;
        }

        /// <summary>
        /// Soft delete a subscription.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to soft delete.</param>
        /// <returns>The subscription with updated status and unsubscribed_time.</returns>
        public async Task<Subscription> UnsubscribeAsync(Guid subscriptionId, Guid operationId)
        {
            Subscription subscription;
            try
            {
                // Get the subscription that matches the subscriptionId provided
                subscription = await GetAsync(subscriptionId);
            }
            catch (Exception)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.Unsubscribe.ToVerb()), UserErrorCode.ResourceNotFound);
            }

            CheckSubscriptionInReadyState(subscription);
            _logger.LogInformation($"Operation {operationId}: Unsubscribe subscription {subscriptionId} with offer {subscription.OfferName} and plan {subscription.PlanName}.");
            // Soft delete the subscription from db
            subscription.OperationId = operationId;
            subscription.ProvisioningStatus = nameof(ProvisioningState.ArmTemplatePending);
            subscription.ProvisioningType = nameof(ProvisioningType.Unsubscribe);
            subscription.LastUpdatedTime = DateTime.UtcNow;

            using (var transaction = await _context.BeginTransactionAsync())
            {
                var subscriptionMeterUsages = await _context.SubscriptionCustomMeterUsages.Where(s => s.IsEnabled && s.SubscriptionId == subscriptionId).ToListAsync();

                foreach(var usage in subscriptionMeterUsages)
                {
                    usage.UnsubscribedTime = subscription.LastUpdatedTime.Value;
                    _context.SubscriptionCustomMeterUsages.Update(usage);
                }

                await _context._SaveChangesAsync();

                _context.Subscriptions.Update(subscription);
                await _context._SaveChangesAsync();

                transaction.Commit();
            }
            _logger.LogInformation($"Operation {operationId}: Subscription {subscriptionId} with offer {subscription.OfferName} and plan {subscription.PlanName} is unsubscribed.");

            return subscription;
        }

        /// <summary>
        /// Suspend the subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<Subscription> SuspendAsync(Guid subscriptionId, Guid operationId)
        {
            Subscription subscription;
            try
            {
                // Get the subscription that matches the subscriptionId provided
                subscription = await GetAsync(subscriptionId);
            }
            catch (Exception)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.Suspend.ToVerb()), UserErrorCode.ResourceNotFound);
            }

            CheckSubscriptionInReadyState(subscription);
            _logger.LogInformation($"Operation {operationId}: Suspend subscription {subscriptionId} with offer {subscription.OfferName} and plan {subscription.PlanName}.");

            // Soft delete the subscription from db
            subscription.OperationId = operationId;
            subscription.ProvisioningStatus = nameof(ProvisioningState.ArmTemplatePending);
            subscription.ProvisioningType = nameof(ProvisioningType.Suspend);
            subscription.LastUpdatedTime = DateTime.UtcNow;

            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
            _logger.LogInformation($"Operation {operationId}: Subscription {subscriptionId} with offer {subscription.OfferName} and plan {subscription.PlanName} is suspended.");

            return subscription;
        }

        /// <summary>
        /// Reinstate the subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<Subscription> ReinstateAsync(Guid subscriptionId, Guid operationId)
        {
            Subscription subscription;
            try
            {
                // Get the subscription that matches the subscriptionId provided
                subscription = await GetAsync(subscriptionId);
            }
            catch (Exception)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.Reinstate.ToVerb()), UserErrorCode.ResourceNotFound);
            }

            if (!subscription.ProvisioningStatus.Equals(nameof(ProvisioningState.Succeeded)))
            {
                throw new NotSupportedException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.Reinstate.ToVerb(), invalidProvisioningState: true));
            }

            if (!subscription.Status.Equals(nameof(FulfillmentState.Suspended)))
            {
                throw new NotSupportedException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.Reinstate.ToVerb(), requiredFulfillmentState: nameof(FulfillmentState.Suspended)));
            }
            _logger.LogInformation($"Operation {operationId}: Reinstate subscription {subscriptionId} with offer {subscription.OfferName} and plan {subscription.PlanName}.");

            // Soft delete the subscription from db
            subscription.OperationId = operationId;
            subscription.ProvisioningStatus = nameof(ProvisioningState.ArmTemplatePending);
            subscription.ProvisioningType = nameof(ProvisioningType.Reinstate);
            subscription.LastUpdatedTime = DateTime.UtcNow;

            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
            _logger.LogInformation($"Operation {operationId}: Reinstate {subscriptionId} with offer {subscription.OfferName} and plan {subscription.PlanName} is suspended.");

            return subscription;
        }

        /// <summary>
        /// Delete data from a subscription
        /// </summary>
        /// <param name="subscriptionId">the subscription id</param>
        /// <returns>Purged subscription</returns>
        public async Task<Subscription> DeleteDataAsync(Guid subscriptionId)
        {
            Subscription subscription;
            try
            {
                // Get the subscription that matches the subscriptionId provided
                subscription = await GetAsync(subscriptionId);
            }
            catch (Exception)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.DeleteData.ToVerb()), UserErrorCode.ResourceNotFound);
            }

            if (subscription == null)
            {
                throw new NotSupportedException($"Subscription {subscriptionId} doesn't exist or you don't have permission to access it.");
            }

            if (!subscription.ProvisioningStatus.Equals(nameof(ProvisioningState.Succeeded)))
            {
                throw new NotSupportedException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.DeleteData.ToVerb(), invalidProvisioningState: true));
            }

            if (!subscription.Status.Equals(nameof(FulfillmentState.Unsubscribed)))
            {
                throw new NotSupportedException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.DeleteData.ToVerb(), requiredFulfillmentState: nameof(FulfillmentState.Unsubscribed)));
            }

            _logger.LogInformation($"Delete data for subscription {subscriptionId}.");
            subscription.ProvisioningStatus = nameof(ProvisioningState.ArmTemplatePending);
            subscription.ProvisioningType = nameof(ProvisioningType.DeleteData);
            subscription.LastUpdatedTime = DateTime.UtcNow;

            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
            _logger.LogInformation($"Data deleted for subscription {subscriptionId}.");
            return subscription;
        }

        /// <summary>
        /// Activate a subscription.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to activate.</param>
        /// <param name="activatedBy">The id of the user who activated this subscription.</param>
        /// <returns>The activated subscription.</returns>
        public async Task<Subscription> ActivateAsync(Guid subscriptionId, string activatedBy = "system")
        {
            Subscription subscription;
            try
            {
                // Get the subscription that matches the subscriptionId provided
                subscription = await GetAsync(subscriptionId);
            }
            catch (Exception)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeSubscriptionActionErrorMessage(subscriptionId, SubscriptionAction.Activate.ToVerb()), UserErrorCode.ResourceNotFound);
            }

            _logger.LogInformation($"Activate subscription {subscriptionId}.");
            subscription.Status = nameof(FulfillmentState.Subscribed);
            subscription.ActivatedTime = DateTime.UtcNow;
            subscription.ActivatedBy = activatedBy;

            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
            _logger.LogInformation($"Activated subscription {subscriptionId}. Activated by: {activatedBy}.");
            return subscription;
        }

        /// <summary>
        /// Checks if a subscription exists.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(Guid subscriptionId)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Subscription).Name, subscriptionId.ToString()));
            // Check that only one subscription with this subscriptionId exists 
            var count = await _context.Subscriptions
                .CountAsync(s => s.SubscriptionId == subscriptionId);

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Subscription).Name, subscriptionId.ToString()));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Subscription).Name, subscriptionId.ToString(), false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Subscription).Name, subscriptionId.ToString(), true));
                // count = 1
                return true;
            }
        }

        /// <summary>
        /// Get warnings from subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription id. Get all warnings if not specified</param>
        /// <returns>warnings</returns>
        public async Task<List<SubscriptionWarning>> GetWarnings(Guid? subscriptionId = null)
        {
            var subList = _context.Subscriptions.ToList().Where(s => ProvisioningHelper.IsErrorOrWarningProvisioningState(s.ProvisioningStatus) &&
                (subscriptionId == null || s.SubscriptionId == subscriptionId)).ToList();

            List<SubscriptionWarning> warnings = new List<SubscriptionWarning>();
            foreach (var sub in subList)
            {
                warnings.Add(new SubscriptionWarning(sub.SubscriptionId,
                    string.Format("Subscription in error state {0} since {1}.", sub.ProvisioningStatus, sub.LastUpdatedTime),
                    string.Format("Last exception: {0}.", sub.LastException)));
            }

            return warnings;
        }
    }
}