// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Fulfillment;
using Luna.Data.Enums;
using Luna.Services.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Luna.Services.WebHook
{
    public class WebhookProcessor : IWebhookProcessor
    {

        private readonly IFulfillmentClient fulfillmentClient;

        private readonly ILogger<WebhookProcessor> _logger;

        private readonly ISubscriptionService _subscriptionService;

        public WebhookProcessor(
            IFulfillmentClient fulfillmentClient,
            ILogger<WebhookProcessor> logger,
            ISubscriptionService subscriptionService)
        {

            this.fulfillmentClient = fulfillmentClient ?? throw new ArgumentNullException(nameof(fulfillmentClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        public async Task ProcessWebhookNotificationAsync(WebhookPayload payload,
            CancellationToken cancellationToken = default)
        {
            Guid requestId = Guid.NewGuid();
            Guid correlationId = Guid.NewGuid();

            // Get operation doesn't work. Need to confirm with marketplace team. Comment it out for now.

            // Always query the fulfillment API for the received Operation for security reasons. Webhook endpoint is not authenticated.
            //var operationDetails = await this.fulfillmentClient.GetSubscriptionOperationAsync(payload.SubscriptionId,
            //    payload.Id,
            //    requestId,
            //    correlationId,
            //    cancellationToken);

            //if (!operationDetails.Success)
            //{
            //    this.logger.LogError(
            //        $"Operation query returned {JsonConvert.SerializeObject(operationDetails)} for subscription {payload.SubscriptionId} operation {payload.Id}");
            //    return;
            //}

            this._logger.LogInformation(
                $"Received webhook notification with payload, {JsonConvert.SerializeObject(payload)}");

            var subscription = await _subscriptionService.GetAsync(payload.SubscriptionId);

            if (!subscription.ProvisioningStatus.Equals(nameof(ProvisioningState.Succeeded)))
            {
                throw new LunaConflictUserException($"Can not perform operation with type {nameof(payload.Action)}. There's another operation with type {nameof(subscription.ProvisioningType)} running.");
            }

            switch (payload.Action)
            {
                case nameof(WebhookAction.Unsubscribe):
                    await _subscriptionService.UnsubscribeAsync(payload.SubscriptionId, payload.Id);
                    break;

                case nameof(WebhookAction.ChangePlan):
                    subscription.PlanName = payload.PlanId;
                    await _subscriptionService.UpdateAsync(subscription, payload.Id);
                    break;

                case nameof(WebhookAction.ChangeQuantity):
                    subscription.Quantity = payload.Quantity;
                    await _subscriptionService.UpdateAsync(subscription, payload.Id);
                    break;

                case nameof(WebhookAction.Suspend):
                    await _subscriptionService.SuspendAsync(subscription.SubscriptionId, payload.Id);
                    break;

                case nameof(WebhookAction.Reinstate):
                    await _subscriptionService.ReinstateAsync(subscription.SubscriptionId, payload.Id);
                    break;

                default:
                    throw new ArgumentException($"The action type {payload.Action} is not supported.");
            }
        }
    }
}
