// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the webhookParameter resource.
    /// </summary>
    public interface IWebhookParameterService
    {
        /// <summary>
        /// Gets all webhookParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of webhookParameters objects.</returns>
        Task<List<WebhookParameter>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets a webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to get.</param>
        /// <returns>The webhookParameter object.</returns>
        Task<WebhookParameter> GetAsync(string offerName, string name);

        /// <summary>
        /// Creates a webhookParameter object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookId">The id of the webhook that the given WebhookParameter is associated with.</param>
        /// <param name="webhookParameter">The webhookParameter to create.</param>
        /// <returns>The created webhookParameter.</returns>
        Task<WebhookParameter> CreateAsync(string offerName, long webhookId, WebhookParameter webhookParameter);

        /// <summary>
        /// Updates a webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the webhookParameter to update.</param>
        /// <param name="webhookParameter">The updated webhookParameter.</param>
        /// <returns>The updated webhookParameter.</returns>
        Task<WebhookParameter> UpdateAsync(string offerName, string parameterName, WebhookParameter webhookParameter);

        /// <summary>
        /// Removes any WebhookParameters from the db that are not associated with any Webhooks.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to delete.</param>
        /// <returns></returns>
        Task DeleteUnusedAsync(string offerName);

        /// <summary>
        /// Checks if a webhookParameter exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string name);
    }
}