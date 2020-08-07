// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the version resource.
    /// </summary>
    public interface IAPIVersionService
    {
        /// <summary>
        /// Gets all versions.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment</param>
        /// <returns>A list of versions.</returns>
        Task<List<APIVersion>> GetAllAsync(string productName, string deploymentName);

        /// <summary>
        /// Gets an version by name.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment</param>
        /// <param name="versionName">The name of the version to get.</param>
        /// <returns>The version.</returns>
        Task<APIVersion> GetAsync(string productName, string deploymentName, string versionName);

        /// <summary>
        /// Creates an version.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment</param>
        /// <param name="version">The version to create.</param>
        /// <returns>The created version.</returns>
        Task<APIVersion> CreateAsync(string productName, string deploymentName, APIVersion version);

        /// <summary>
        /// Updates an version.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment</param>
        /// <param name="versionName">The name of the version to update.</param>
        /// <param name="version">The updated version.</param>
        /// <returns>The updated version.</returns>
        Task<APIVersion> UpdateAsync(string productName, string deploymentName, string versionName, APIVersion version);

        /// <summary>
        /// Deletes an version.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment</param>
        /// <param name="versionName">The name of the version to delete.</param>
        /// <returns>The deleted version.</returns>
        Task<APIVersion> DeleteAsync(string productName, string deploymentName, string versionName);

        /// <summary>
        /// Checks if an version exists.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment</param>
        /// <param name="versionName">The name of the version to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string productName, string deploymentName, string versionName);
    }
}