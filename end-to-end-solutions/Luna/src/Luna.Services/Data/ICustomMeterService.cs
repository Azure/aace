using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the customMeter resource.
    /// </summary>
    public interface ICustomMeterService
    {
        /// <summary>
        /// Gets all customMeters.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <returns>A list of customMeters.</returns>
        Task<List<CustomMeter>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter.</param>
        /// <returns>The customMeter.</returns>
        Task<CustomMeter> GetAsync(string offerName, string meterName);

        /// <summary>
        /// Creates a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter.</param>
        /// <param name="customMeter">The customMeter to create.</param>
        /// <returns>The created customMeter.</returns>
        Task<CustomMeter> CreateAsync(string offerName, string meterName, CustomMeter customMeter);

        /// <summary>
        /// Updates a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter.</param>
        /// <returns>The updated customMeter.</returns>
        Task<CustomMeter> UpdateAsync(string offerName, string meterName, CustomMeter customMeter);

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>The deleted customMeter.</returns>
        Task<CustomMeter> DeleteAsync(string offerName, string meterName);

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string meterName);
    }
}