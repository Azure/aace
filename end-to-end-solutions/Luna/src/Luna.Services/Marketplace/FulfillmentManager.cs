using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Fulfillment;
using Luna.Clients.Logging;
using Luna.Clients.Models.Fulfillment;
using Luna.Services.Utilities;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Marketplace
{
    public class FulfillmentManager : IFulfillmentManager
    {
        private readonly IFulfillmentClient _fulfillmentClient;
        private readonly ILogger<FulfillmentManager> _logger;

        public FulfillmentManager(IFulfillmentClient fulfillmentClient, ILogger<FulfillmentManager> logger)
        {
            _fulfillmentClient = fulfillmentClient ?? throw new ArgumentNullException(nameof(fulfillmentClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Activate a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="quantity">The quantity</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MarketplaceSubscription> ActivateSubscriptionAsync(
            Guid subscriptionId,
            string planId,
            int? quantity,
            CancellationToken cancellationToken = default)
        {
            Guid requestId = Guid.NewGuid();
            Guid correlationId = Guid.NewGuid();
            var subscriptionToBeActivated = new ActivatedSubscriptionResult { PlanId = planId };
            
            if (quantity.HasValue)
            {
                subscriptionToBeActivated.Quantity = quantity.Value.ToString();
            }

            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.ActivateSubscriptionAsync),
                        subscriptionId));

                var result = await _fulfillmentClient.ActivateSubscriptionAsync(
                             subscriptionId,
                             subscriptionToBeActivated,
                             requestId,
                             correlationId,
                             cancellationToken);

                _logger.LogInformation(
                        LoggingUtils.ComposeSubscriptionActionMessage(
                            "Activated",
                            subscriptionId,
                            planId,
                            quantity.ToString()));

                var returnValue = new MarketplaceSubscription
                {
                    PlanId = planId,
                    State = StatusEnum.Subscribed,
                    SubscriptionId = subscriptionId
                };

                if (quantity.HasValue)
                {
                    returnValue.Quantity = quantity.Value;
                }

                return returnValue;
            }
            catch (Exception e)
            {
                var errorMessage = LoggingUtils.ComposeSubscriptionActionMessage(
                                "Failed to activate",
                                subscriptionId,
                                planId,
                                quantity.ToString());
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }

        /// <summary>
        /// Get the operation status of the given operation
        /// </summary>
        /// <param name="receivedSubscriptionId">The subscription id</param>
        /// <param name="operationId">The operation id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FulfillmentManagerOperationResult> GetOperationResultAsync(
            Guid receivedSubscriptionId,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.GetSubscriptionOperationAsync),
                        receivedSubscriptionId));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                var operationResult = await _fulfillmentClient.GetSubscriptionOperationAsync(
                                          receivedSubscriptionId,
                                          operationId,
                                          requestId,
                                          correlationId,
                                          cancellationToken);

                return FulfillmentManagerOperationResult.Success;
            }
            catch (Exception e)
            {
                var errorMessage = $"Operation {operationId} failed for subscription {receivedSubscriptionId}.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }

        /// <summary>
        /// Get subscription information for the given subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MarketplaceSubscription> GetSubscriptionAsync(
            Guid subscriptionId, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.GetSubscriptionAsync),
                        subscriptionId));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                var subscription = await _fulfillmentClient.GetSubscriptionAsync(
                    subscriptionId, requestId, correlationId, cancellationToken);

                return MarketplaceSubscription.From(subscription);
            }
            catch (Exception e)
            {
                var errorMessage = $"Cannot get subscription {subscriptionId} from Azure Marketplace.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }

        /// <summary>
        /// Get all operation statuses for a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SubscriptionOperation>> GetSubscriptionOperationsAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.GetSubscriptionOperationsAsync),
                        subscriptionId));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                return await _fulfillmentClient.GetSubscriptionOperationsAsync(
                    subscriptionId,
                    requestId,
                    correlationId,
                    cancellationToken);
            }
            catch (Exception e)
            {
                var errorMessage = $"Cannot get subscription operations for subscription {subscriptionId}.";
                throw new LunaFulfillmentException(errorMessage, e);
            }

        }

        /// <summary>
        /// Get all available plans for a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SubscriptionPlans> GetSubscriptionPlansAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.GetSubscriptionPlansAsync),
                        subscriptionId));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                return await _fulfillmentClient.GetSubscriptionPlansAsync(
                    subscriptionId,
                    requestId,
                    correlationId,
                    cancellationToken);
            }
            catch (Exception e)
            {
                var errorMessage = $"Cannot get subscription plans for subscription {subscriptionId}.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }

        /// <summary>
        /// Get all subscriptions
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MarketplaceSubscription>> GetSubscriptionsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.GetSubscriptionsAsync)));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                var response = await _fulfillmentClient.GetSubscriptionsAsync(
                                        requestId,
                                        correlationId,
                                        cancellationToken);

                return response.Select(s => MarketplaceSubscription.From(s));
            }
            catch (Exception e)
            {
                var errorMessage = $"Cannot get subscriptions.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }

        /// <summary>
        /// Cancel a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FulfillmentManagerOperationResult> RequestCancelSubscriptionAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.DeleteSubscriptionAsync),
                        subscriptionId));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                UpdateOrDeleteSubscriptionRequestResult response = await _fulfillmentClient.DeleteSubscriptionAsync(
                                        subscriptionId,
                                        requestId,
                                        correlationId,
                                        cancellationToken);

                _logger.LogInformation($"Cancelled subscription {subscriptionId}.");
                return FulfillmentManagerOperationResult.Success;
            }
            catch (Exception e)
            {
                var errorMessage = $"Cannot cancel subscription {subscriptionId}.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }
        
        /// <summary>
        /// Update a subscription 
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FulfillmentManagerOperationResult> RequestUpdateSubscriptionAsync(
            Guid subscriptionId,
            string planId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.UpdateSubscriptionAsync),
                        subscriptionId));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();

                ActivatedSubscriptionResult activatedSubscription = new ActivatedSubscriptionResult { PlanId = planId };

                UpdateOrDeleteSubscriptionRequestResult response = await _fulfillmentClient.UpdateSubscriptionAsync(
                                         subscriptionId,
                                         activatedSubscription,
                                         requestId,
                                         correlationId,
                                         cancellationToken);

                _logger.LogInformation(
                     LoggingUtils.ComposeSubscriptionActionMessage(
                            "Updated",
                            subscriptionId,
                            activatedSubscription.PlanId,
                            activatedSubscription.Quantity));

                SubscriptionOperation operation = await _fulfillmentClient.GetSubscriptionOperationAsync(
                                    subscriptionId,
                                    response.OperationId,
                                    requestId,
                                    correlationId,
                                    cancellationToken);

                FulfillmentManagerOperationResult returnResult = FulfillmentManagerOperationResult.Success;
                returnResult.Operation = operation;

                return returnResult;
            }
            catch (Exception e)
            {
                var errorMessage = $"Cannot update subscription {subscriptionId}.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
            
        }

        /// <summary>
        /// Resolves a subscription with Azure Marketplace given a token
        /// </summary>
        /// <param name="authCode">The authentication code for the subscription</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MarketplaceSubscription> ResolveSubscriptionAsync(
            string authCode,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    LoggingUtils.ComposeHttpClientLogMessage(
                        _fulfillmentClient.GetType().Name,
                        nameof(_fulfillmentClient.ResolveSubscriptionAsync)));

                Guid requestId = Guid.NewGuid();
                Guid correlationId = Guid.NewGuid();
                ResolvedSubscriptionResult subscription = await _fulfillmentClient.ResolveSubscriptionAsync(
                                       authCode,
                                       requestId,
                                       correlationId,
                                       cancellationToken);
              
                _logger.LogInformation(
                     LoggingUtils.ComposeSubscriptionActionMessage(
                            "Resolved",
                            subscription.SubscriptionId,
                            subscription.PlanId,
                            subscription.Quantity.ToString()));

                return MarketplaceSubscription.From(subscription, StatusEnum.Provisioning);
            }
            catch (Exception e)
            {
                var errorMessage = "The token is invalid.";
                throw new LunaFulfillmentException(errorMessage, e);
            }
        }
    }
}