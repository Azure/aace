// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Luna.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the apiSubscription resource.
    /// </summary>
    public interface IAPISubscriptionService
    {
        /// <summary>
        /// Gets all subscriptions.
        /// </summary>
        /// <param name="status">The list status of the subscription.</param>
        /// <param name="owner">The owner of the subscription.</param>
        /// <returns>A list of all subsrciptions.</returns>
        Task<List<APISubscription>> GetAllAsync(string[] status = null, string owner = "");

        /// <summary>
        /// Gets an apiSubscription by name.
        /// </summary>
        /// <param name="apiSubscriptionId">The id of the apiSubscription to update.</param>
        /// <returns>The apiSubscription.</returns>
        Task<APISubscription> GetAsync(Guid apiSubscriptionId);

        /// <summary>
        /// Creates an apiSubscription.
        /// </summary>
        /// <param name="apiSubscription">The apiSubscription to create.</param>
        /// <returns>The created apiSubscription.</returns>
        Task<APISubscription> CreateAsync(APISubscription apiSubscription);

        /// <summary>
        /// Updates an apiSubscription.
        /// </summary>
        /// <param name="apiSubscriptionId">The id of the apiSubscription to update.</param>
        /// <param name="apiSubscription">The updated apiSubscription.</param>
        /// <returns>The updated apiSubscription.</returns>
        Task<APISubscription> UpdateAsync(Guid apiSubscriptionId, APISubscription apiSubscription);

        /// <summary>
        /// Deletes an apiSubscription.
        /// </summary>
        /// <param name="apiSubscriptionId">The id of the apiSubscription to update.</param>
        /// <returns>The deleted apiSubscription.</returns>
        Task<APISubscription> DeleteAsync(Guid apiSubscriptionId);

        /// <summary>
        /// Checks if an apiSubscription exists.
        /// </summary>
        /// <param name="apiSubscriptionId">The id of the apiSubscription to update.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(Guid apiSubscriptionId);

        /// <summary>
        /// Regenerate key for the subscription
        /// </summary>
        /// <param name="apiSubscriptionId">subscription id</param>
        /// <param name="keyName">The key name</param>
        /// <returns>The subscription with regenerated key</returns>
        Task<APISubscription> RegenerateKey(Guid apiSubscriptionId, string keyName);
    }
}