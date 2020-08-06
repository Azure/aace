// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Models.Fulfillment;

namespace Luna.Clients.Fulfillment
{
    public interface IFulfillmentClient
    {
        /// <summary>
        /// Activate a subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="subscriptionDetails"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FulfillmentRequestResult> ActivateSubscriptionAsync(
            Guid subscriptionId,
            ActivatedSubscriptionResult subscriptionDetails, 
            Guid requestId, 
            Guid correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribe and delete the specified subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<UpdateOrDeleteSubscriptionRequestResult> DeleteSubscriptionAsync(
            Guid subscriptionId, 
            Guid requestId,
            Guid correlationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// List outstanding operations for the current publisher
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<SubscriptionOperation>> GetOperationsAsync(
            Guid requestId, 
            Guid correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the specified SaaS subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SubscriptionResult> GetSubscriptionAsync(
            Guid subscriptionId, 
            Guid requestId, 
            Guid correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the operation status of the specified triggered async operation such as subscribe, unsubscribe, changePlan, or changeQuantity
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="operationId"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SubscriptionOperation> GetSubscriptionOperationAsync(
            Guid subscriptionId, 
            Guid operationId, 
            Guid requestId,
            Guid correlationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all operation statuses
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<SubscriptionOperation>> GetSubscriptionOperationsAsync(
            Guid subscriptionId, 
            Guid requestId,
            Guid correlationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// List all available plans
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SubscriptionPlans> GetSubscriptionPlansAsync(
            Guid subscriptionId, 
            Guid requestId, 
            Guid correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// List all the SaaS subscriptions for a publisher
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<SubscriptionResult>> GetSubscriptionsAsync(
            Guid requestId, 
            Guid correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve a subscription
        /// </summary>
        /// <param name="marketplaceToken"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ResolvedSubscriptionResult> ResolveSubscriptionAsync(
            string marketplaceToken, 
            Guid requestId, 
            Guid correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="update"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<UpdateOrDeleteSubscriptionRequestResult> UpdateSubscriptionAsync(
            Guid subscriptionId,
            ActivatedSubscriptionResult update, 
            Guid requestId, 
            Guid correlationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the status of an operation to indicate success or failure with the provided values
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="operationId"></param>
        /// <param name="update"></param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FulfillmentRequestResult> UpdateSubscriptionOperationAsync(
            Guid subscriptionId, 
            Guid operationId, 
            OperationUpdate update, 
            Guid requestId,
            Guid correlationId, 
            CancellationToken cancellationToken = default);
    }
}
