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
        /// <param name="planName">The name of the plan.</param>
        /// <returns>A list of customMeterDimensions.</returns>
        Task<List<CustomMeterDimension>> GetAllAsync(string offerName, string planName);

        /// <summary>
        /// Gets a customMeterDimension by id.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <returns>The customMeterDimension.</returns>
        Task<CustomMeterDimension> GetAsync(string offerName, string planName, string meterName);

        /// <summary>
        /// Creates a customMeterDimension within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <param name="customMeterDimension">The customMeterDimension object to create.</param>
        /// <returns>The created customMeterDimension.</returns>
        Task<CustomMeterDimension> CreateAsync(string offerName, string planName, string meterName, CustomMeterDimension customMeterDimension);

        /// <summary>
        /// Updates a customMeterDimension.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <param name="customMeterDimension">The updated customMeterDimension.</param>
        /// <returns>The updated customMeterDimension.</returns>
        Task<CustomMeterDimension> UpdateAsync(string offerName, string planName, string meterName, CustomMeterDimension customMeterDimension);

        /// <summary>
        /// Deletes a customMeterDimension.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <returns>The deleted customMeterDimension.</returns>
        Task<CustomMeterDimension> DeleteAsync(string offerName, string planName, string meterName);

        /// <summary>
        /// Checks if a customMeterDimension exists.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeterDimension.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the customMeterDimension</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string planName, string meterName);
    }
}