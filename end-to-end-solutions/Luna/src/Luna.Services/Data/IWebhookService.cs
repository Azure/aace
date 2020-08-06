// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the webhook resource.
    /// </summary>
    public interface IWebhookService
    {
        /// <summary>
        /// Gets all webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of webhooks.</returns>
        Task<List<Webhook>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to get.</param>
        /// <returns>The webhook.</returns>
        Task<Webhook> GetAsync(string offerName, string webhookName);

        /// <summary>
        /// Creates a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhook">The webhook to create.</param>
        /// <returns>The created webhook.</returns>
        Task<Webhook> CreateAsync(string offerName, Webhook webhook);

        /// <summary>
        /// Updates a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to update.</param>
        /// <param name="webhook">The updated webhook.</param>
        /// <returns>The updated webhook.</returns>
        Task<Webhook> UpdateAsync(string offerName, string webhookName, Webhook webhook);

        /// <summary>
        /// Deletes a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to delete.</param>
        /// <returns>The deleted webhook.</returns>
        Task<Webhook> DeleteAsync(string offerName, string webhookName);

        /// <summary>
        /// Checks if a webhook exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string webhookName);
    }
}