// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the ipConfig resource.
    /// </summary>
    public interface IIpConfigService
    {
        /// <summary>
        /// Gets all ipConfigs within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of offersParameters.</returns>
        Task<List<IpConfig>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to get.</param>
        /// <returns>The ipConfig.</returns>
        Task<IpConfig> GetAsync(string offerName, string name);
        
        /// <summary>
        /// Creates an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="ipConfig">The ipConfig to create.</param>
        /// <returns>The created ipConfig.</returns>
        Task<IpConfig> CreateAsync(string offerName, IpConfig ipConfig);

        /// <summary>
        /// Updates an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to update.</param>
        /// <param name="ipConfig">The updated ipConfig.</param>
        /// <returns>The updated ipConfig.</returns>
        Task<IpConfig> UpdateAsync(string offerName, string name, IpConfig ipConfig);

        /// <summary>
        /// Deletes an IpConfig within an Offer and all of the IpBlocks and IpAddresses associated 
        /// with it. The delete will only occur if all of the IpAddresses associated with the IpConfig
        /// are not being used.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to delete.</param>
        /// <returns>The deleted ipConfig.</returns>
        Task<IpConfig> DeleteAsync(string offerName, string name);
        
        /// <summary>
        /// Checks if an ipConfig exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string name);
    }
}