using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Azure;
using Luna.Clients.Azure.Storage;
using Luna.Clients.Exceptions;
using Luna.Clients.Fulfillment;
using Luna.Clients.Logging;
using Luna.Clients.Models.Fulfillment;
using Luna.Clients.Provisioning;
using Luna.Data.DataContracts;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Luna.Services.Data;
using Luna.Services.Marketplace;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Luna.Services.Provisoning
{
    public class ProvisioningService : IProvisioningService
    {

        private readonly ISqlDbContext _context;
        private readonly IProvisioningClient _provisioningClient;
        private readonly IFulfillmentClient _fulfillmentClient;
        private readonly IIpAddressService _ipAddressService;
        private readonly ISubscriptionParameterService _subscriptionParameterService;
        private readonly IArmTemplateParameterService _armTemplateParameterService;
        private readonly IWebhookParameterService _webhookParameterService;
        private readonly IStorageUtility _storageUtility;
        private readonly int _maxRetry;
        private readonly ILogger<ProvisioningService> _logger;


        public ProvisioningService(ISqlDbContext sqlDbContext, IProvisioningClient provisioningClient, 
            IFulfillmentClient fulfillmentclient, IIpAddressService ipAddressService,
            ISubscriptionParameterService subscriptionParameterService, IArmTemplateParameterService armTemplateParameterService,
            IWebhookParameterService webhookParameterService, IStorageUtility storageUtility, ILogger<ProvisioningService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _provisioningClient = provisioningClient ?? throw new ArgumentNullException(nameof(provisioningClient));
            _fulfillmentClient = fulfillmentclient ?? throw new ArgumentNullException(nameof(fulfillmentclient));
            _ipAddressService = ipAddressService ?? throw new ArgumentNullException(nameof(ipAddressService));
            _subscriptionParameterService = subscriptionParameterService ?? throw new ArgumentNullException(nameof(subscriptionParameterService));
            _armTemplateParameterService = armTemplateParameterService ?? throw new ArgumentNullException(nameof(armTemplateParameterService));
            _webhookParameterService = webhookParameterService ?? throw new ArgumentNullException(nameof(webhookParameterService));
            _storageUtility = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            //TODO: add the app settings.
            _maxRetry = 3;
        }

        /// <summary>
        /// Activate a subscription 
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="activatedBy">AAD identity or Microsoft id of the caller</param>
        [InputStates(ProvisioningState.NotificationPending, 
            ProvisioningState.DeployResourceGroupFailed, 
            ProvisioningState.ArmTemplateFailed,
            ProvisioningState.WebhookFailed,
            ProvisioningState.NotificationFailed,
            ProvisioningState.ManualActivationPending)]
        [OutputStates(ProvisioningState.Succeeded, ProvisioningState.NotificationFailed)]
        public async Task<Subscription> ActivateSubscriptionAsync(Guid subscriptionId, string activatedBy = "system")
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);


            try 
            {

                Offer offer = await FindOfferById(subscription.OfferId);
                if (subscription.ProvisioningStatus.Equals(ProvisioningState.NotificationPending.ToString(), StringComparison.InvariantCultureIgnoreCase)
                    && offer.ManualActivation)
                {
                    _logger.LogInformation($"ManualActivation of offer {offer.OfferName} is set to true. Will not activate the subscription.");

                    return await TransitToNextState(subscription, ProvisioningState.ManualActivationPending);
                }

                Plan plan = await FindPlanById(subscription.PlanId);
                ActivatedSubscriptionResult activatedResult = new ActivatedSubscriptionResult
                {
                    PlanId = plan.PlanName,
                    Quantity = subscription.Quantity.ToString()
                };

                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.ActivateSubscriptionAsync),
                        subscriptionId));

                var result = await _fulfillmentClient.ActivateSubscriptionAsync(
                    subscriptionId,
                    activatedResult,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    default);

                _logger.LogInformation(
                    LoggingUtils.ComposeSubscriptionActionMessage(
                        "Activated",
                        subscriptionId,
                        activatedResult.PlanId,
                        activatedResult.Quantity,
                        activatedBy));

                subscription.Status = nameof(FulfillmentState.Subscribed);
                subscription.ActivatedTime = DateTime.UtcNow;
                subscription.ActivatedBy = activatedBy;
                return await TransitToNextState(subscription, ProvisioningState.Succeeded);
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }

        }

        /// <summary>
        /// Check ARM template deployment status
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [InputStates(ProvisioningState.ArmTemplateRunning)]
        [OutputStates(ProvisioningState.WebhookPending, ProvisioningState.ArmTemplateFailed)]
        public async Task<Subscription> CheckArmDeploymentStatusAsync(Guid subscriptionId)
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);
            
            try
            {
                Offer offer = await FindOfferById(subscription.OfferId);

                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _provisioningClient.GetType().Name,
                        nameof(_provisioningClient.GetDeploymentAsync),
                        subscriptionId));

                var result = await _provisioningClient.GetDeploymentAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    offer.HostSubscription.ToString(),
                    subscription.ResourceGroup,
                    subscription.DeploymentName,
                    default);

                if (result.Properties.ProvisioningState.Equals(nameof(ArmProvisioningState.Succeeded)))
                {
                    _logger.LogInformation("ARM deployment {subscription.DeploymentName} succeeded.");
                    return await TransitToNextState(subscription, ProvisioningState.WebhookPending);
                }
                if (result.Properties.ProvisioningState.Equals(nameof(ArmProvisioningState.Updating)))
                {
                    _logger.LogInformation("ARM deployment {subscription.DeploymentName} in progress.");
                    return await TransitToNextState(subscription, ProvisioningState.ArmTemplateRunning);
                }

                throw new LunaProvisioningException(
                    "ARM deployment failed.",
                    ExceptionUtils.IsHttpErrorCodeRetryable(result.StatusCode),
                    ProvisioningState.ArmTemplatePending);
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }
        }

        /// <summary>
        /// Check resource group deployment status
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [InputStates(ProvisioningState.DeployResourceGroupRunning)]
        [OutputStates(ProvisioningState.DeployResourceGroupRunning, 
            ProvisioningState.ArmTemplatePending, 
            ProvisioningState.DeployResourceGroupFailed)]
        public async Task<Subscription> CheckResourceGroupDeploymentStatusAsync(Guid subscriptionId)
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);

            try
            {
                Offer offer = await FindOfferById(subscription.OfferId);

                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _provisioningClient.GetType().Name,
                        nameof(_provisioningClient.ResourceGroupExistsAsync),
                        subscriptionId));

                //TODO: how to check if it is being deployed or deployment failed?
                bool resourceGroupExists = await _provisioningClient.ResourceGroupExistsAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    offer.HostSubscription.ToString(),
                    subscription.ResourceGroup,
                    default);

                if (resourceGroupExists)
                {
                    _logger.LogInformation($"Resource group deployment for {subscription.ResourceGroup} succeeded.");
                    return await TransitToNextState(subscription, ProvisioningState.ArmTemplatePending);
                }

                _logger.LogInformation($"Resource group deployment for {subscription.ResourceGroup} in progress.");
                return await TransitToNextState(subscription, ProvisioningState.DeployResourceGroupRunning);
                
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }
        }

        /// <summary>
        /// Create resource group for a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [InputStates(ProvisioningState.ProvisioningPending)]
        [OutputStates(ProvisioningState.DeployResourceGroupRunning, ProvisioningState.WebhookPending, ProvisioningState.DeployResourceGroupFailed)]
        public async Task<Subscription> CreateResourceGroupAsync(Guid subscriptionId)
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);

            try
            {
                Offer offer = await FindOfferById(subscription.OfferId);
                Plan plan = await FindPlanById(subscription.PlanId);
                string isvSubscriptionId = offer.HostSubscription.ToString();

                var parameters = await EvaluateParameters(offer, plan, subscription);

                subscription.EntryPointUrl = parameters.ContainsKey("entryPointUrl") ? parameters["entryPointUrl"].ToString() : "";

                if (plan.SubscribeArmTemplateId == null)
                {
                    // Don't need to deploy anything, transit to WebhookPending state
                    return await TransitToNextState(subscription, ProvisioningState.WebhookPending);
                }

                string azureLocation = null;
                string resourceGroupName = $"{offer.OfferName}-{subscription.SubscriptionId}";

                if (parameters.ContainsKey("resourceGroupLocation"))
                {
                    azureLocation = parameters["resourceGroupLocation"].ToString();
                }
                else
                {
                    throw new LunaBadRequestUserException("The ResourceGroupLocation parameter is not specified.", UserErrorCode.ParameterNotProvided);
                }

                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _provisioningClient.GetType().Name,
                        nameof(_provisioningClient.ResourceGroupExistsAsync),
                        subscriptionId));

                bool resourceGroupExists = await _provisioningClient.ResourceGroupExistsAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    isvSubscriptionId,
                    resourceGroupName,
                    default);

                if (!resourceGroupExists)
                {
                    _logger.LogInformation(
                        LoggingUtils.ComposeHttpClientLogMessage(
                            _provisioningClient.GetType().Name,
                            nameof(_provisioningClient.CreateOrUpdateResourceGroupAsync),
                            subscriptionId));

                    var result = await _provisioningClient.CreateOrUpdateResourceGroupAsync(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        isvSubscriptionId,
                        resourceGroupName,
                        azureLocation,
                        default
                    );

                    _logger.LogInformation($"Deploying resource group {resourceGroupName} in location {azureLocation}.");
                }
                else
                {
                    throw new LunaConflictUserException($"Resource group with name {resourceGroupName} already exist.");
                }

                subscription.ResourceGroup = resourceGroupName;

                return await TransitToNextState(subscription, ProvisioningState.DeployResourceGroupRunning);
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }
        }

        /// <summary>
        /// Deploy ARM template for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [InputStates(ProvisioningState.ArmTemplatePending)]
        [OutputStates(ProvisioningState.ArmTemplateRunning, 
            ProvisioningState.WebhookPending, 
            ProvisioningState.ProvisioningPending,
            ProvisioningState.ArmTemplateFailed)]
        public async Task<Subscription> DeployArmTemplateAsync(Guid subscriptionId)
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);

            try
            {
                if (subscription.ResourceGroup == null)
                {
                    // If resource group is not created, transit to ProvisioningPending state to create resource group
                    return await TransitToNextState(subscription, ProvisioningState.ProvisioningPending);
                }

                Offer offer = await FindOfferById(subscription.OfferId);
                Plan plan = await FindPlanById(subscription.PlanId);
                string deploymentName = $"{plan.PlanName}{offer.OfferName}{new Random().Next(0, 9999).ToString("D4")}"; // deployment name cannot exceed 64 characters, otherwise returns 400
                string isvSubscriptionId = offer.HostSubscription.ToString();

                string templatePath = await GetTemplatePath(plan, subscription.ProvisioningType);

                if (templatePath == null)
                {
                    // If template is not specified, do nothing and transit to WebhookPending state
                    return await TransitToNextState(subscription, ProvisioningState.WebhookPending);
                }

                // Reevaluate parameters if not subscribe. If it is subscribe, the parameters are evaluated when creating resource group
                if (!subscription.ProvisioningType.Equals(nameof(ProvisioningType.Subscribe)))
                {
                    await EvaluateParameters(offer, plan, subscription);
                }

                templatePath = await _storageUtility.GetFileReferenceWithSasKeyAsync(templatePath);

                JObject parameters = new JObject();
                using (WebClient client = new WebClient())
                {
                    string content = client.DownloadString(templatePath);
                    Context context = await SetContext(offer.OfferName, subscription.Owner, subscriptionId, plan.PlanName, subscription.ProvisioningType);
                    var paramList = ARMTemplateHelper.GetArmTemplateParameters(content);
                    foreach (var param in paramList)
                    {
                        JProperty value = new JProperty("value", context.Parameters[param.Key]);
                        parameters.Add(param.Key, new JObject(value));
                    }
                }

                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _provisioningClient.GetType().Name,
                        nameof(_provisioningClient.PutDeploymentAsync),
                        subscriptionId));

                var result = await _provisioningClient.PutDeploymentAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    isvSubscriptionId,
                    subscription.ResourceGroup,
                    deploymentName,
                    parameters: parameters,
                    templatePath: templatePath);
               
                subscription.DeploymentName = result.Name;

                _logger.LogInformation($"Running ARM deployment {deploymentName} for subscription {isvSubscriptionId} in resource group {subscription.ResourceGroup}.");

                return await TransitToNextState(subscription, ProvisioningState.ArmTemplateRunning);
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }
        }


        /// <summary>
        /// Execute webhook for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [InputStates(ProvisioningState.WebhookPending)]
        [OutputStates(ProvisioningState.NotificationPending, ProvisioningState.WebhookFailed)]
        public async Task<Subscription> ExecuteWebhookAsync(Guid subscriptionId)
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);

            try
            {
                Offer offer = await FindOfferById(subscription.OfferId);
                Plan plan = await FindPlanById(subscription.PlanId);
                string urlString = await GetWebhookUrl(plan, subscription.ProvisioningType);

                // Only run the webhook if it is specified.
                if(urlString != null)
                {
                    Context context = await SetContext(offer.OfferName, subscription.Owner, subscriptionId, plan.PlanName, subscription.ProvisioningType);
                    UriBuilder webhookUri = new UriBuilder(urlString);
                    var query = HttpUtility.ParseQueryString(webhookUri.Query);

                    foreach (var key in query.AllKeys)
                    {
                        if (query[key].StartsWith("{", StringComparison.InvariantCultureIgnoreCase) && query[key].EndsWith("}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string parameterName = query[key].Substring(1, query[key].Length - 2);
                            if (context.Parameters.ContainsKey(parameterName))
                            {
                                query[key] = context.Parameters[parameterName].ToString();
                            }
                            else
                            {
                                //re-evaluate the parameter and reset the context
                                await EvaluateParameters(offer, plan, subscription);
                                context = await SetContext(offer.OfferName, subscription.Owner, subscriptionId, plan.PlanName, subscription.ProvisioningType);

                                if (context.Parameters.ContainsKey(parameterName))
                                {
                                    query[key] = context.Parameters[parameterName].ToString();
                                }
                                else
                                {
                                    // There's a bug if this happens.
                                    throw new LunaServerException($"Webhook parameter {parameterName} doesn't exist.");
                                }
                            }
                        }
                    }

                    webhookUri.Query = query.ToString();

                    _logger.LogInformation(
                        LoggingUtils.ComposeHttpClientLogMessage(
                            _provisioningClient.GetType().Name,
                            nameof(_provisioningClient.ExecuteWebhook),
                            subscriptionId));

                    await _provisioningClient.ExecuteWebhook(webhookUri.Uri);

                    _logger.LogInformation($"Running webhook {webhookUri.Uri} for subscription {subscriptionId}.");
                }

                return await TransitToNextState(subscription, ProvisioningState.NotificationPending);
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }
        }

        /// <summary>
        /// Update a subscription operation as completed
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="activatedBy">AAD identity or Microsoft id of the caller</param>
        /// <returns>The subscription</returns>
        [InputStates(ProvisioningState.NotificationPending,
            ProvisioningState.DeployResourceGroupFailed,
            ProvisioningState.ArmTemplateFailed,
            ProvisioningState.WebhookFailed,
            ProvisioningState.NotificationFailed,
            ProvisioningState.ManualCompleteOperationPending)]
        [OutputStates(ProvisioningState.Succeeded, ProvisioningState.NotificationFailed, ProvisioningState.ManualCompleteOperationPending)]
        public async Task<Subscription> UpdateOperationCompletedAsync(Guid subscriptionId, string activatedBy = "system")
        {
            Subscription subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            ValidateSubscriptionAndInputState(subscription);

            try
            {
                Offer offer = await FindOfferById(subscription.OfferId);

                if (subscription.ProvisioningStatus.Equals(ProvisioningState.NotificationPending.ToString(), StringComparison.InvariantCultureIgnoreCase)
                    && offer.ManualCompleteOperation)
                {
                    _logger.LogInformation($"ManualCompleteOperation of offer {offer.OfferName} is set to true. Will not complete the operation automatically.");

                    return await TransitToNextState(subscription, ProvisioningState.ManualCompleteOperationPending);
                }

                // Don't need to update marketplace operation for delete data
                if (!subscription.ProvisioningType.Equals(nameof(ProvisioningType.DeleteData)))
                {
                    Plan plan = await FindPlanById(subscription.PlanId);

                    OperationUpdate update = new OperationUpdate
                    {
                        PlanId = plan.PlanName,
                        Quantity = subscription.Quantity,
                        Status = OperationUpdateStatusEnum.Success
                    };

                    _logger.LogInformation(
                        LoggingUtils.ComposeHttpClientLogMessage(
                            _fulfillmentClient.GetType().Name,
                            nameof(_fulfillmentClient.UpdateSubscriptionOperationAsync),
                            subscriptionId));

                    var result = await _fulfillmentClient.UpdateSubscriptionOperationAsync(
                        subscriptionId,
                        subscription.OperationId ?? Guid.Empty,
                        update,
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        default);
                }

                switch (subscription.ProvisioningType)
                {
                    case nameof(ProvisioningType.Update):
                    case nameof(ProvisioningType.Reinstate):
                        subscription.Status = nameof(FulfillmentState.Subscribed);
                        break;
                    case nameof(ProvisioningType.DeleteData):
                        subscription.Status = nameof(FulfillmentState.Purged);
                        break;
                    case nameof(ProvisioningType.Suspend):
                        subscription.LastSuspendedTime = DateTime.UtcNow;
                        subscription.Status = nameof(FulfillmentState.Suspended);
                        break;
                    case nameof(ProvisioningType.Unsubscribe):
                        subscription.UnsubscribedTime = DateTime.UtcNow;
                        subscription.Status = nameof(FulfillmentState.Unsubscribed);
                        break;
                    default:
                        throw new ArgumentException($"Provisioning type {subscription.ProvisioningType} is not supported.");
                }
                subscription.ActivatedBy = activatedBy;
                return await TransitToNextState(subscription, ProvisioningState.Succeeded);
            }
            catch (Exception e)
            {
                return await HandleExceptions(subscription, e);
            }
        }

        /// <summary>
        /// Get active provisions
        /// </summary>
        /// <returns>The active provisions</returns>
        public async Task<List<SubscriptionProvision>> GetInProgressProvisionsAsync()
        {
            List<SubscriptionProvision> statusList = new List<SubscriptionProvision>();
            List<Subscription> subList = await _context.Subscriptions.ToListAsync();

            foreach (var sub in subList)
            {
                if (sub.Status.Equals(nameof(FulfillmentState.Unsubscribed)))
                {
                    Plan plan = await FindPlanById(sub.PlanId);
                    if (sub.UnsubscribedTime.Value.AddDays(plan.DataRetentionInDays) > DateTime.UtcNow)
                    {
                        continue;
                    }
                }
                else if (ProvisioningHelper.isFinalProvisioningState(sub.ProvisioningStatus))
                {
                    continue;
                }

                statusList.Add(new SubscriptionProvision
                {
                    SubscriptionId = sub.SubscriptionId,
                    ProvisioningStatus = sub.ProvisioningStatus,
                    ProvisioningType = sub.ProvisioningType,
                    RetryCount = sub.RetryCount,
                    LastException = sub.LastException,
                    SubscriptionStatus = sub.Status
                });
            }

            return statusList;

        }

        #region private methods
        /// <summary>
        /// Get ARM template path
        /// </summary>
        /// <param name="plan">The plan</param>
        /// <param name="subscriptionAction">The subscription action</param>
        /// <returns></returns>
        private async Task<string> GetTemplatePath(Plan plan, string subscriptionAction)
        {
            ArmTemplate template;
            switch (subscriptionAction)
            {
                case nameof(ProvisioningType.Subscribe):
                case nameof(ProvisioningType.Update):
                case nameof(ProvisioningType.Reinstate):
                    template = await _context.ArmTemplates.FindAsync(plan.SubscribeArmTemplateId);
                    break;
                case nameof(ProvisioningType.Unsubscribe):
                    template = await _context.ArmTemplates.FindAsync(plan.UnsubscribeArmTemplateId);
                    break;
                case nameof(ProvisioningType.Suspend):
                    template = await _context.ArmTemplates.FindAsync(plan.SuspendArmTemplateId);
                    break;
                case nameof(ProvisioningType.DeleteData):
                    template = await _context.ArmTemplates.FindAsync(plan.DeleteDataArmTemplateId);
                    break;
                default:
                    throw new ArgumentException();
            }

            return template == null ? null : template.TemplateFilePath;
        }

        /// <summary>
        /// Get the webhook url
        /// </summary>
        /// <param name="plan">The plan</param>
        /// <param name="subscriptionAction">The subscription action</param>
        /// <returns></returns>
        private async Task<string> GetWebhookUrl(Plan plan, string subscriptionAction)
        {
            Webhook webhook;
            switch (subscriptionAction)
            {
                case nameof(ProvisioningType.Subscribe):
                case nameof(ProvisioningType.Update):
                case nameof(ProvisioningType.Reinstate):
                    webhook = await _context.Webhooks.FindAsync(plan.SubscribeWebhookId);
                    break;
                case nameof(ProvisioningType.Unsubscribe):
                    webhook = await _context.Webhooks.FindAsync(plan.UnsubscribeWebhookId);
                    break;
                case nameof(ProvisioningType.Suspend):
                    webhook = await _context.Webhooks.FindAsync(plan.SuspendWebhookId);
                    break;
                case nameof(ProvisioningType.DeleteData):
                    webhook = await _context.Webhooks.FindAsync(plan.DeleteDataWebhookId);
                    break;
                default:
                    throw new ArgumentException();
            }

            return webhook == null? null: webhook.WebhookUrl;
        }

        /// <summary>
        /// Set the context for expression evaluation
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionOwner">The subscription owner</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="planName">The plan name</param>
        /// <param name="operationType">The operation type</param>
        /// <returns>The context</returns>
        private async Task<Context> SetContext(string offerName, string subscriptionOwner, Guid subscriptionId, string planName, string operationType)
        {
            Context context = new Context(offerName, subscriptionOwner, subscriptionId, planName, operationType);
            var subParams = await _subscriptionParameterService.GetAllAsync(subscriptionId);
            foreach (var param in subParams)
            {
                if (!context.Parameters.ContainsKey(param.Name))
                {
                    context.Parameters.Add(param.Name, param.Value);
                }
            }
            Context._ipAddressService = _ipAddressService;

            return context;
        }

        /// <summary>
        /// Transition to next state and update the subscription in database.
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="targetState">The target state</param>
        /// <param name="callerName">The caller name</param>
        /// <returns></returns>
        private async Task<Subscription> TransitToNextState(Subscription subscription,
            ProvisioningState targetState,
            [CallerMemberName] string callerName = "")
        {
            MethodInfo method = typeof(ProvisioningService).GetMethod(callerName);

            OutputStatesAttribute attribute = (OutputStatesAttribute)Attribute.GetCustomAttribute(method, typeof(OutputStatesAttribute));

            if (!attribute.InputStates.Contains(targetState.ToString()))
            {
                throw new LunaProvisioningException(
                    $"Cannot transit to ${targetState.ToString()} state from method {callerName}.",
                    false);
            }

            //reset retry count
            subscription.RetryCount = 0;
            subscription.ProvisioningStatus = targetState.ToString();
            subscription.LastUpdatedTime = DateTime.UtcNow;
            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
            return subscription;
        }

        /// <summary>
        /// Handle the provisioning exceptions
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="ex">The exception</param>
        /// <param name="callerName">The caller name</param>
        /// <returns>The updated subscription</returns>
        private async Task<Subscription> HandleExceptions(Subscription subscription, Exception ex,
            [CallerMemberName] string callerName = "")
        {
            _logger.LogError(ex, ex.Message);

            // Transit to error state if:
            // 1. It is not a LunaException
            // 2. It is not retry-able
            // 3. The retry count exceeded the threshold
            if (ex.GetType() != typeof(LunaServerException) || !((LunaServerException)ex).IsRetryable || subscription.RetryCount >= _maxRetry)
            {
                ProvisioningState errorState;
                switch (subscription.ProvisioningStatus)
                {
                    case nameof(ProvisioningState.NotificationPending):
                        errorState = ProvisioningState.NotificationFailed;
                        break;
                    case nameof(ProvisioningState.ArmTemplatePending):
                    case nameof(ProvisioningState.ArmTemplateRunning):
                        errorState = ProvisioningState.ArmTemplateFailed;
                        break;
                    case nameof(ProvisioningState.ProvisioningPending):
                    case nameof(ProvisioningState.DeployResourceGroupRunning):
                        errorState = ProvisioningState.DeployResourceGroupFailed;
                        break;
                    case nameof(ProvisioningState.WebhookPending):
                        errorState = ProvisioningState.WebhookFailed;
                        break;
                    default:
                        errorState = ProvisioningState.NotSpecified;
                        break;
                }

                // Transit to error state and reset retry count
                subscription.ProvisioningStatus = errorState.ToString();
                subscription.RetryCount = 0;
            }
            else
            {
                // Failback to an earlier state if it is specified in the LunaProvisioningException. 
                // Otherwise, stay in the same state and retry

                if (ex.GetType() == typeof(LunaProvisioningException) &&
                    ((LunaProvisioningException)ex).FailbackState != ProvisioningState.NotSpecified)
                {
                    var failbackState = ((LunaProvisioningException)ex).FailbackState;
                    subscription.ProvisioningStatus = failbackState.ToString();
                }
            }

            MethodInfo method = typeof(ProvisioningService).GetMethod(callerName);

            OutputStatesAttribute attribute = (OutputStatesAttribute)Attribute.GetCustomAttribute(method, typeof(OutputStatesAttribute));

            if (!attribute.InputStates.Contains(subscription.ProvisioningStatus.ToString()))
            {
                _logger.LogError(ex, $"Can not transit to ${subscription.ProvisioningStatus.ToString()} state from method {callerName}.");
                subscription.ProvisioningStatus = nameof(ProvisioningState.NotSpecified);
            }

            subscription.LastUpdatedTime = DateTime.UtcNow;
            subscription.LastException = ex.Message;
            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
            return subscription;
        }

        /// <summary>
        /// Evaluate parameters for a given subscription
        /// </summary>
        /// <param name="offer"></param>
        /// <param name="plan"></param>
        /// <param name="subscription"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, object>> EvaluateParameters(Offer offer, Plan plan, Subscription subscription)
        {
            ExpressionEvaluationUtils util = new ExpressionEvaluationUtils(
                await SetContext(offer.OfferName, subscription.Owner, subscription.SubscriptionId, plan.PlanName, subscription.ProvisioningType));

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var param in await _armTemplateParameterService.GetAllAsync(offer.OfferName))
            {
                parameters.Add(param.Name, param.Value);
            }
            foreach (var param in await _webhookParameterService.GetAllAsync(offer.OfferName))
            {
                parameters.Add(param.Name, param.Value);
            }
            await util.EvaluateAll(parameters);

            foreach (var param in util.Context.Parameters.Keys)
            {
                if (!await _subscriptionParameterService.ExistsAsync(subscription.SubscriptionId, param))
                {
                    await _subscriptionParameterService.CreateAsync(new SubscriptionParameter
                    {
                        SubscriptionId = subscription.SubscriptionId,
                        Name = param,
                        Value = util.Context.Parameters[param].ToString(),
                        Type = util.Context.Parameters[param].GetType().Name
                    });
                }
            }

            return util.Context.Parameters;
        }

        /// <summary>
        /// Validate the input provisioning state
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="callerName">The caller name</param>
        /// 
        private void ValidateSubscriptionAndInputState(Subscription subscription, [CallerMemberName] string callerName = "")
        {
            if (subscription == null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Subscription).Name, subscription.SubscriptionId.ToString()));
            }

            MethodInfo method = typeof(ProvisioningService).GetMethod(callerName);

            InputStatesAttribute attribute = (InputStatesAttribute)Attribute.GetCustomAttribute(method, typeof(InputStatesAttribute));

            if (!attribute.InputStates.Contains(subscription.ProvisioningStatus))
            {
                throw new LunaConflictUserException($"Cannot call function {method.Name} when subscription provisioning state is {subscription.ProvisioningStatus}.");
            }
        }

        /// <summary>
        /// Find offer by the offer id
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns></returns>
        private async Task<Offer> FindOfferById(long offerId)
        {
            Offer offer = await _context.Offers.FindAsync(offerId);
            if (offer == null)
            {
                throw new LunaProvisioningException(
                    LoggingUtils.ComposeNotFoundErrorMessage("Offer", "id", offerName: offerId.ToString()),
                    false);
            }
            return offer;
        }

        /// <summary>
        /// Find plan by the plan id
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        private async Task<Plan> FindPlanById(long planId)
        {
            Plan plan = await _context.Plans.FindAsync(planId);
            if (plan == null)
            {
                throw new LunaServerException(
                    LoggingUtils.ComposeNotFoundErrorMessage("Plan", "id", planName: planId.ToString()),
                    false);
            }
            return plan;
        }
        #endregion

    }
}
