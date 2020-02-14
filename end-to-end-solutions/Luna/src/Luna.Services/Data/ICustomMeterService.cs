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
        /// <returns>A list of customMeters.</returns>
        Task<List<CustomMeter>> GetAllAsync();

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter.</param>
        /// <returns>The customMeter.</returns>
        Task<CustomMeter> GetAsync(string meterName);

        /// <summary>
        /// Creates a customMeter.
        /// </summary>
        /// <param name="customMeter">The customMeter to create.</param>
        /// <returns>The created customMeter.</returns>
        Task<CustomMeter> CreateAsync(CustomMeter customMeter);

        /// <summary>
        /// Updates a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter.</param>
        /// <returns>The updated customMeter.</returns>
        Task<CustomMeter> UpdateAsync(string meterName, CustomMeter customMeter);

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>The deleted customMeter.</returns>
        Task<CustomMeter> DeleteAsync(string meterName);

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string meterName);
    }
}