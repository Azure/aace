using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the plan resource.
    /// </summary>
    public interface IPlanService
    {
        /// <summary>
        /// Gets all plans within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of plans.</returns>
        Task<List<Plan>> GetAllAsync(string offerName);
        
        /// <summary>
        /// Gets a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to get.</param>
        /// <returns>The plan.</returns>
        Task<Plan> GetAsync(string offerName, string planUniqueName);

        /// <summary>
        /// Gets a plan by its id.
        /// </summary>
        /// <param name="planId">The plan id</param>
        /// <returns>The plan.</returns>
        Task<Plan> GetByIdAsync(long planId);

        /// <summary>
        /// Creates a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="plan">The plan to create.</param>
        /// <returns>The created plan.</returns>
        Task<Plan> CreateAsync(string offerName, Plan plan);
        
        /// <summary>
        /// Updates a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to update.</param>
        /// <param name="plan">The updated plan.</param>
        /// <returns>The updated plan.</returns>
        Task<Plan> UpdateAsync(string offerName, string planUniqueName, Plan plan);
        
        /// <summary>
        /// Deletes a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to delete.</param>
        /// <returns>The deleted plan.</returns>
        Task<Plan> DeleteAsync(string offerName, string planUniqueName);

        /// <summary>
        /// Checks if a plan exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string planUniqueName);
    }
}