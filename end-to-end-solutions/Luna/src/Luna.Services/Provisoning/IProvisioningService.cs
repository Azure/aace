// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.DataContracts;
using Luna.Data.Entities;

namespace Luna.Services.Provisoning
{
    public interface IProvisioningService
    {
        /// <summary>
        /// Check resource group deploymenet status
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        Task<Subscription> CheckResourceGroupDeploymentStatusAsync(Guid subscriptionId);

        /// <summary>
        /// Create resource group for a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        Task<Subscription> CreateResourceGroupAsync(Guid subscriptionId);

        /// <summary>
        /// Deploy ARM template for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        Task<Subscription> DeployArmTemplateAsync(Guid subscriptionId);

        /// <summary>
        /// Check ARM template deployment status
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        Task<Subscription> CheckArmDeploymentStatusAsync(Guid subsciptionId);

        /// <summary>
        /// Execute webhook for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        Task<Subscription> ExecuteWebhookAsync(Guid subscriptionId);

        /// <summary>
        /// Activate a subscription 
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="activatedBy">AAD identity or Microsoft id of the caller</param>
        /// <returns>The subscription</returns>
        Task<Subscription> ActivateSubscriptionAsync(Guid subscriptionId, string activatedBy = "system");

        /// <summary>
        /// Update a subscription operation as completed
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="activatedBy">AAD identity or Microsoft id of the caller</param>
        /// <returns>The subscription</returns>
        Task<Subscription> UpdateOperationCompletedAsync(Guid subscriptionId, string activatedBy = "system");

        /// <summary>
        /// Get in progress provisions
        /// </summary>
        /// <returns>The in progress provisions</returns>
        Task<List<SubscriptionProvision>> GetInProgressProvisionsAsync();
    }
}
