using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.DataContracts;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the subscription resource.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Gets all subscriptions.
        /// </summary>
        /// <param name="status">The list status of the subscription.</param>
        /// <param name="owner">The owner of the subscription.</param>
        /// <returns>A list of all subsrciptions.</returns>
        Task<List<Subscription>> GetAllAsync(string[] status = null, string owner = "");
        
        /// <summary>
        /// Gets a subscription by id.
        /// </summary>
        /// <param name="subscription_id">The id of the subscription.</param>
        /// <returns>The subscription.</returns>
        Task<Subscription> GetAsync(Guid subscription_id);
        
        /// <summary>
        /// Creates a subscription within a plan within an offer.
        /// </summary>
        /// <param name="subscription">The subscription to create.</param>
        /// <returns>The created subscription.</returns>
        Task<Subscription> CreateAsync(Subscription subscription);

        /// <summary>
        /// Updates a subscription.
        /// </summary>
        /// <param name="subscription">The updated subscription.</param>
        /// <param name="operationId">The operation id.</param>
        /// <returns>The updated subscription.</returns>
        Task<Subscription> UpdateAsync(Subscription subscription, Guid operationId);

        /// <summary>
        /// Soft deletes a subscription.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to soft delete.</param>
        /// <param name="operationId">The operation id.</param>
        /// <returns>The subscription with updated status and unsubscribed_time.</returns>
        Task<Subscription> UnsubscribeAsync(Guid subscriptionId, Guid operationId);

        /// <summary>
        /// Suspend a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="operationId">The operation id.</param>
        /// <returns>Suspended subscription</returns>
        Task<Subscription> SuspendAsync(Guid subscriptionId, Guid operationId);

        /// <summary>
        /// Reinstate a subscription
        /// </summary>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="operationId">The operation id</param>
        /// <returns>Reinstated subscription</returns>
        Task<Subscription> ReinstateAsync(Guid subscriptionId, Guid operationId);

        /// <summary>
        /// Delete data from a subscription
        /// </summary>
        /// <param name="subscriptionId">the subscription id</param>
        /// <returns>Purged subscription</returns>
        Task<Subscription> DeleteDataAsync(Guid subscriptionId);

        /// <summary>
        /// Activate a subscription.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to activate.</param>
        /// <param name="activatedBy">The id of the user who activated this subscription.</param>
        /// <returns>The activated subscription.</returns>
        Task<Subscription> ActivateAsync(Guid subscriptionId, string activatedBy = "system");

        /// <summary>
        /// Checks if a subscription exists.
        /// </summary>
        /// <param name="subscription_id">The id of the subscription to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(Guid subscription_id);

        /// <summary>
        /// Get warnings from subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription id. Get all warnings if not specified</param>
        /// <returns>warnings</returns>
        Task<List<SubscriptionWarning>> GetWarnings(Guid? subscriptionId = null);
    }
}