using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for aadSecretTmp resource.
    /// </summary>
    public interface IAadSecretTmpService
    {
        /// <summary>
        /// Gets all aadSecretTmps within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of aadSecretTmp objects.</returns>
        Task<List<AadSecretTmp>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets an aadSecretTmp within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp to get.</param>
        /// <returns>An aadSecretTmp object.</returns>
        Task<AadSecretTmp> GetAsync(string offerName, string name);

        /// <summary>
        /// Creates an aadSecretTmp object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="aadSecretTmp">The aadSecretTmp object to create.</param>
        /// <returns>The created aadSecretTmp object.</returns>
        Task<AadSecretTmp> CreateAsync(string offerName, AadSecretTmp aadSecretTmp);

        /// <summary>
        /// Updates an aadSecretTmp object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp object to update.</param>
        /// <param name="aadSecretTmp">The updated aadSecretTmp object.</param>
        /// <returns>The updated aadSecretTmp object.</returns>
        Task<AadSecretTmp> UpdateAsync(string offerName, string name, AadSecretTmp aadSecretTmp);

        /// <summary>
        /// Deletes an aadSecretTmp object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp to delete.</param>
        /// <returns>The deleted aadSecretTmp object.</returns>
        Task<AadSecretTmp> DeleteAsync(string offerName, string name);

        /// <summary>
        /// Checks if an aadSecretTmp object exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string name);
    }
}