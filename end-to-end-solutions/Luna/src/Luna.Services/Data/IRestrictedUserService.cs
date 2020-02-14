using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the restrictedUser resource.
    /// </summary>
    public interface IRestrictedUserService
    {
        /// <summary>
        /// Gets all restrictedUsers within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <returns>A list of restrictedUsers.</returns>
        Task<List<RestrictedUser>> GetAllAsync(string offerName, string planUniqueName);

        /// <summary>
        /// Gets a restructedUser by id.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="restrictedUser">The restrictedUser to create.</param>
        /// <returns>The restrictedUser.</returns>
        Task<RestrictedUser> GetAsync(string offerName, string planName, Guid tenantId);
        
        /// <summary>
        /// Creates a restrictedUser within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <param name="restrictedUser">The restrictedUser to create.</param>
        /// <returns>The created restrictedUser.</returns>
        Task<RestrictedUser> CreateAsync(string offerName, string planUniqueName, RestrictedUser restrictedUser);

        /// <summary>
        /// Updates a restrictedUser by id.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <param name="restrictedUser">The updated restrictedUser.</param>
        /// <returns>The updated restrictedUser.</returns>
        Task<RestrictedUser> UpdateAsync(string offerName, string planName, RestrictedUser restrictedUser);

        /// <summary>
        /// Deletes a restrictedUser by id.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="tenantId">The tenant id.</param>
        /// <returns>The deleted restrictedUser.</returns>
        Task<RestrictedUser> DeleteAsync(string offerName, string planName, Guid tenantId);

        Task<bool> ExistsAsync(string offerName, string planName, Guid tenantId);
    }
}