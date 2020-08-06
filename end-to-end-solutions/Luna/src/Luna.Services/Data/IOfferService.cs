// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the offer resource.
    /// </summary>
    public interface IOfferService
    {
        /// <summary>
        /// Gets all offers.
        /// </summary>
        /// <returns>A list of offers.</returns>
        Task<List<Offer>> GetAllAsync();

        /// <summary>
        /// Gets an offer by name.
        /// </summary>
        /// <param name="offerName">The name of the offer to get.</param>
        /// <returns>The offer.</returns>
        Task<Offer> GetAsync(string offerName);

        /// <summary>
        /// Creates an offer.
        /// </summary>
        /// <param name="offer">The offer to create.</param>
        /// <returns>The created offer.</returns>
        Task<Offer> CreateAsync(Offer offer);

        /// <summary>
        /// Updates an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to update.</param>
        /// <param name="offer">The updated offer.</param>
        /// <returns>The updated offer.</returns>
        Task<Offer> UpdateAsync(string offerName, Offer offer);

        /// <summary>
        /// Deletes an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to delete.</param>
        /// <returns>The deleted offer.</returns>
        Task<Offer> DeleteAsync(string offerName);

        /// <summary>
        /// Checks if an offer exists.
        /// </summary>
        /// <param name="offerName">The name of the offer to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName);

        /// <summary>
        /// Get offer warnings
        /// </summary>
        /// <param name="offerName">The offer name, if not provided, will check all offers</param>
        /// <returns>The warnings</returns>
        Task<List<OfferWarning>> GetWarningsAsync(string offerName=null);

        /// <summary>
        /// publish an offer
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <returns>The offer</returns>
        Task<Offer> PublishAsync(string offerName);
    }
}