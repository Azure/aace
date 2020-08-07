// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the TelemetryDataConnector resource.
    /// </summary>
    public interface ITelemetryDataConnectorService
    {
        /// <summary>
        /// Gets all TelemetryDataConnectors.
        /// </summary>
        /// <returns>A list of TelemetryDataConnectors.</returns>
        Task<List<TelemetryDataConnector>> GetAllAsync();

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <returns>The customMeter.</returns>
        Task<TelemetryDataConnector> GetAsync(string name);

        /// <summary>
        /// Creates a customMeter.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <param name="telemetryDataConnector">The TelemetryDataConnector to create.</param>
        /// <returns>The created customMeter.</returns>
        Task<TelemetryDataConnector> CreateAsync(string name, TelemetryDataConnector telemetryDataConnector);

        /// <summary>
        /// Updates a TelemetryDataConnector.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <param name="telemetryDataConnector">The updated telemetryDataConnector.</param>
        /// <returns>The updated TelemetryDataConnector.</returns>
        Task<TelemetryDataConnector> UpdateAsync(string name, TelemetryDataConnector customMeter);

        /// <summary>
        /// Deletes a TelemetryDataConnector.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <returns>The deleted TelemetryDataConnector.</returns>
        Task<TelemetryDataConnector> DeleteAsync(string name);

        /// <summary>
        /// Checks if a TelemetryDataConnector exists.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string name);
    }
}