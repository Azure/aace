using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the customMeterDimension resource.
    /// </summary>
    public interface ICustomMeterDimensionService
    {
        /// <summary>
        /// Gets all customMeterDimensions within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <returns>A list of customMeterDimensions.</returns>
        Task<List<CustomMeterDimension>> GetAllAsync(string offerName, string planUniqueName);
        
        /// <summary>
        /// Gets a customMeterDimension by id.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension.</param>
        /// <returns>The customMeterDimension.</returns>
        Task<CustomMeterDimension> GetAsync(long id);

        /// <summary>
        /// Creates a customMeterDimension within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <param name="customMeterDimension">The customMeterDimension object to create.</param>
        /// <returns>The created customMeterDimension.</returns>
        Task<CustomMeterDimension> CreateAsync(string offerName, string planUniqueName, CustomMeterDimension customMeterDimension);
        
        /// <summary>
        /// Updates a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension to update.</param>
        /// <param name="customMeterDimension">The updated customMeterDimension.</param>
        /// <returns>The updated customMeterDimension.</returns>
        Task<CustomMeterDimension> UpdateAsync(long id, CustomMeterDimension customMeterDimension);
        
        /// <summary>
        /// Deletes a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension to delete.</param>
        /// <returns>The deleted customMeterDimension.</returns>
        Task<CustomMeterDimension> DeleteAsync(long id);
    }
}