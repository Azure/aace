// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the offerParameter resource.
    /// </summary>
    public interface IOfferParameterService
    {
        /// <summary>
        /// Gets all offerParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of offersParameters.</returns>
        Task<List<OfferParameter>> GetAllAsync(string offerName);
        
        /// <summary>
        /// Gets an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to get.</param>
        /// <returns>The offerParameter.</returns>
        Task<OfferParameter> GetAsync(string offerName, string parameterName);
        
        /// <summary>
        /// Creates an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="offerParameter">The offerParameter to create.</param>
        /// <returns>The created offerParameter.</returns>
        Task<OfferParameter> CreateAsync(string offerName, OfferParameter offerParameter);
        
        /// <summary>
        /// Updates an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to update.</param>
        /// <param name="offerParameter">The updated offerParameter.</param>
        /// <returns>The updated offerParameter.</returns>
        Task<OfferParameter> UpdateAsync(string offerName, string parameterName, OfferParameter offerParameter);
        
        /// <summary>
        /// Deletes an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to delete.</param>
        /// <returns>The deleted offerParameter.</returns>
        Task<OfferParameter> DeleteAsync(string offerName, string parameterName);
        
        /// <summary>
        /// Checks if an offerParameter exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string parameterName);
    }
}