using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients;
using Luna.Clients.Azure;
using Luna.Clients.Azure.Storage;
using Luna.Clients.Provisioning;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Luna.Services.Utilities;

namespace Luna.Services.Marketplace
{
    /// <summary>
    /// Manage marketplace notifications and make HTTP calls via the provisioning client.
    /// </summary>
    public class ProvisioningHelper : IMarketplaceNotificationHandler
    {
        private readonly IProvisioningClient _provisioningClient;
        private readonly LunaClient _lunaClient;
        private readonly IStorageUtility _storage;
        private readonly ISqlDbContext _context;

        public ProvisioningHelper(
            IProvisioningClient provisioningClient,
            LunaClient lunaClient,
            IStorageUtility storageUtility,
            ISqlDbContext context
            )
        {
            _provisioningClient = provisioningClient ?? throw new ArgumentNullException(nameof(provisioningClient));
            _lunaClient = lunaClient ?? throw new ArgumentNullException(nameof(lunaClient));
            _storage = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public static bool isFinalProvisioningState(string provisionState)
        {
            List<string> finalStates = new List<string>(new string[] {
                nameof(ProvisioningState.NotificationFailed),
                nameof(ProvisioningState.ArmTemplateFailed),
                nameof(ProvisioningState.WebhookFailed),
                nameof(ProvisioningState.DeployResourceGroupFailed),
                nameof(ProvisioningState.Succeeded),
            });

            return finalStates.Contains(provisionState);
        }

        public static bool IsErrorOrWarningProvisioningState(string status)
        {
            return status.Equals(nameof(ProvisioningState.ArmTemplateFailed)) ||
                status.Equals(nameof(ProvisioningState.WebhookFailed)) ||
                status.Equals(nameof(ProvisioningState.NotificationFailed)) ||
                status.Equals(nameof(ProvisioningState.DeployResourceGroupFailed)) ||
                // Warning states
                status.Equals(nameof(ProvisioningState.ManualActivationPending)) ||
                status.Equals(nameof(ProvisioningState.ManualCompleteOperationPending));
        }

        /// <summary>
        /// Upon notification of activation of a subscription from AMP, deploy the ARM template.
        /// </summary>
        /// <param name="provisionModel"></param>
        /// <param name="azureLocation"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessStartProvisioningAsync(
            Subscription subscription,
            string azureLocation,
            CancellationToken cancellationToken = default)
        {
            Offer offer = await _context.Offers.FindAsync(subscription.OfferId);
            Plan plan = await _context.Plans.FindAsync(subscription.OfferId, subscription.PlanId);
            string deploymentName = $"{plan.PlanName}{offer.OfferName}{new Random().Next(0, 9999).ToString("D4")}"; // deployment name cannot exceed 64 characters, otherwise returns 400
            string isvSubscriptionId = offer.HostSubscription.ToString();
 
            // get the subscribe template for this plan
            string templatePath = await GetTemplatePath(
                offer, 
                plan, 
                FulfillmentAction.Activate
            );
            object parameters = await GetTemplateParameters(
                offer,
                plan,
                FulfillmentAction.Activate
            );

            // create a resource group for the subscription
            var resourceGroup = (await _provisioningClient.CreateOrUpdateResourceGroupAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                isvSubscriptionId,
                $"luna-{subscription.SubscriptionId}",
                azureLocation,
                cancellationToken
            )).Name;

            // deploy the ARM template
            // note that the rollback property is set to false here because this is a fresh resource group with nothing to fallback on if this deployment fails
            await _provisioningClient.PutDeploymentAsync(
                requestId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(), 
                subscriptionId: isvSubscriptionId,
                resourceGroup: resourceGroup,
                deploymentName: deploymentName,
                templatePath: templatePath,
                parameters: parameters,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Upon notification of an update of the plan from AMP, deploy the ARM template.
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="operationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessChangePlanAsync(
            Subscription subscription,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            Offer offer = await _context.Offers.FindAsync(subscription.OfferId);
            Plan plan = await _context.Plans.FindAsync(subscription.OfferId, subscription.PlanId);

            string deploymentName = $"{plan.PlanName}{offer.OfferName}{new Random().Next(0, 9999).ToString("D4")}"; // deployment name cannot exceed 64 characters, otherwise returns 400
            string isvSubscriptionId = offer.HostSubscription.ToString();

            // get the subscribe template for the new plan
            string templatePath = await GetTemplatePath(
                offer, 
                plan, 
                FulfillmentAction.Update
            );
            object parameters = await GetTemplateParameters(
                offer,
                plan,
                FulfillmentAction.Update
            );

            // deploy the ARM template. rollback to the last successful deployment if this one fails
            await _provisioningClient.PutDeploymentAsync(
                requestId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                subscriptionId: isvSubscriptionId,
                resourceGroup: subscription.ResourceGroup,
                deploymentName: deploymentName,
                templatePath: templatePath,
                parameters: parameters,
                rollbackToLastSuccessful: true,
                cancellationToken: cancellationToken
            );

            // update the subscriptions table
            subscription.PlanId = plan.Id;
            subscription.Status = nameof(FulfillmentState.PendingFulfillmentStart);
            subscription.DeploymentName = deploymentName;
            subscription.OperationId = operationId;
            _context.Subscriptions.Update(subscription);
            await _context._SaveChangesAsync();
        }

        /// <summary>
        /// Upon notification of an update of the plan quantity from AMP, deploy the ARM template.
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessChangeQuantityAsync(
            Subscription subscription,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

        /// <summary>
        /// Upon notification to unsubscribe from AMP, deploy the unsubscribe ARM template.
        /// </summary>
        /// <param name="unsubscribeModel"></param>
        /// <param name="operationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessUnsubscribeAsync(
            Subscription subscription,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            Offer offer = await _context.Offers.FindAsync(subscription.OfferId);
            Plan plan = await _context.Plans.FindAsync(subscription.OfferId, subscription.PlanId);

            string deploymentName = $"{plan.PlanName}{offer.OfferName}{new Random().Next(0, 9999).ToString("D4")}"; // deployment name cannot exceed 64 characters, otherwise returns 400
            string isvSubscriptionId = offer.HostSubscription.ToString();

            // get the unsubscribe template for the plan
            string templatePath = await GetTemplatePath(
                offer, 
                plan, 
                FulfillmentAction.Unsubscribe
            );
            object parameters = await GetTemplateParameters(
                offer,
                plan,
                FulfillmentAction.Unsubscribe
            );

            // deploy the ARM template. rollback to the last successful deployment if this one fails
            await _provisioningClient.PutDeploymentAsync(
                requestId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                subscriptionId: isvSubscriptionId,
                resourceGroup: subscription.ResourceGroup,
                deploymentName: deploymentName,
                templatePath: templatePath,
                parameters: parameters,
                rollbackToLastSuccessful: true,
                cancellationToken: cancellationToken
            );

            // update the subscriptions table
            subscription.PlanId = plan.Id;
            subscription.Status = nameof(FulfillmentState.PendingFulfillmentStart);
            subscription.DeploymentName = deploymentName;
            subscription.OperationId = operationId;
            _context.Subscriptions.Remove(subscription);
        }

        /// <summary>
        /// Upon notification of an update of the plan from the SaaS webhook, deploy the ARM template, 
        /// </summary>
        /// <param name="notificationModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessChangePlanAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

        /// <summary>
        /// Upon notification of an update of the plan quantity from the SaaS webhook, deploy the ARM template.
        /// </summary>
        /// <param name="notificationModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessChangeQuantityAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

        /// <summary>
        /// Upon notification to unsubscribe from the SaaS webhook, deploy the unsubscribe ARM template.
        /// </summary>
        /// <param name="notificationModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessUnsubscribeAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

        /// <summary>
        /// TODO Upon notification of an operation failure or conflict from the SaaS webhook, rollback deployment and/or email.
        /// </summary>
        /// <param name="notificationModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessOperationFailOrConflictAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

        public async Task ProcessReinstatedAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

        public async Task ProcessSuspendedAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            // pass
            await Task.FromResult(0);
        }

       /// <summary>
       /// Get the path for the arm template file on Azure Storage
       /// </summary>
       /// <param name="offer"></param>
       /// <param name="plan"></param>
       /// <param name="subscriptionAction"></param>
       /// <returns>File path for the template file on blob storage</returns>
        public async Task<string> GetTemplatePath(
            Offer offer,
            Plan plan,
            FulfillmentAction subscriptionAction
        )
        {
            ArmTemplate template = null;
            
            switch(subscriptionAction)
            {
                case FulfillmentAction.Activate:
                    template = await _lunaClient.GetArmTemplate(offer.OfferName, plan.SubscribeArmTemplateName);
                    break;
                case FulfillmentAction.Update:
                    template = await _lunaClient.GetArmTemplate(offer.OfferName, plan.SubscribeArmTemplateName);
                    break;
                case FulfillmentAction.Unsubscribe:
                    template = await _lunaClient.GetArmTemplate(offer.OfferName, plan.UnsubscribeArmTemplateName);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            return template.TemplateFilePath;
        }

        /// <summary>
        /// Parse the ARM template to get the parameter names and build an object containing the parameter names and values
        /// </summary>
        /// <param name="offer"></param>
        /// <param name="plan"></param>
        /// <param name="subscriptionAction"></param>
        /// <returns>An object containing the parameter names and values</returns>
        public async Task<Dictionary<string, object>> GetTemplateParameters(
            Offer offer,
            Plan plan,
            FulfillmentAction subscriptionAction
        )
        {
            ArmTemplate template = null;
            switch(subscriptionAction)
            {
                case FulfillmentAction.Activate:
                    template = await _lunaClient.GetArmTemplate(offer.OfferName, plan.SubscribeArmTemplateName);
                    break;
                case FulfillmentAction.Update:
                    template = await _lunaClient.GetArmTemplate(offer.OfferName, plan.SubscribeArmTemplateName);
                    break;
                case FulfillmentAction.Unsubscribe:
                    template = await _lunaClient.GetArmTemplate(offer.OfferName, plan.UnsubscribeArmTemplateName);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            string templateContent = await _storage.DownloadToTextAsync(template.TemplateFilePath);
            var parameters = ARMTemplateHelper.GetArmTemplateParameters(templateContent);
            var parameterList = new Dictionary<string, object>();
            foreach (var parameter in parameters)
            {
                ArmTemplateParameter atp = await _lunaClient.GetArmTemplateParameter(offer.OfferName, parameter.Key);
                
                parameterList.Add(
                    atp.Name,
                    new { Value = atp.Value }    
                );
            }
            return parameterList;
        }
    }
}