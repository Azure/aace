// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.DataContracts;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the offer resource.
    /// </summary>
    public class OfferService : IOfferService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<OfferService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="logger">The logger.</param>
        public OfferService(ISqlDbContext sqlDbContext, ILogger<OfferService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all offers.
        /// </summary>
        /// <returns>A list of offers.</returns>
        public async Task<List<Offer>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Offer).Name));

            // Get all offers that have not been deleted
            var offers = await _context.Offers.Where(o => o.DeletedTime == null).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Offer).Name, offers.Count()));

            return offers;
        }

        /// <summary>
        /// Gets an offer by name.
        /// </summary>
        /// <param name="offerName">The name of the offer to get.</param>
        /// <returns>The offer.</returns>
        public async Task<Offer> GetAsync(string offerName)
        {
            if (!await ExistsAsync(offerName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Offer).Name,
                    offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Offer).Name, offerName));

            // Get the offer that matches the provided offerName and that has not been deleted
            var offer = await _context.Offers.SingleOrDefaultAsync(o => (o.OfferName == offerName) && (o.DeletedTime == null));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Offer).Name,
               offerName,
               JsonSerializer.Serialize(offer)));

            return offer;
        }

        /// <summary>
        /// Creates an offer.
        /// </summary>
        /// <param name="offer">The offer to create.</param>
        /// <returns>The created offer.</returns>
        public async Task<Offer> CreateAsync(Offer offer)
        {
            if (offer is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Offer).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that an offer with the same name does not already exist
            if (await ExistsAsync(offer.OfferName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Offer).Name,
                        offer.OfferName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Offer).Name, offer.OfferName, payload: JsonSerializer.Serialize(offer)));

            // Generate a ContainerName for the offer
            offer.ContainerName = Guid.NewGuid();

            // Update the offer status
            offer.Status = nameof(OfferStatus.Draft);

            // Update the offer created time
            offer.CreatedTime = DateTime.UtcNow;

            // Update the offer last updated time
            offer.LastUpdatedTime = offer.CreatedTime;

            // Add offer to db
            _context.Offers.Add(offer);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Offer).Name, offer.OfferName));

            return offer;
        }

        /// <summary>
        /// Updates an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to update.</param>
        /// <param name="offer">The updated offer.</param>
        /// <returns>The updated offer.</returns>
        public async Task<Offer> UpdateAsync(string offerName, Offer offer)
        {
            if (offer is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Offer).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Offer).Name, offer.OfferName, payload: JsonSerializer.Serialize(offer)));

            // Get the offer that matches the offerName provided
            var offerDb = await GetAsync(offerName);

            // Check if (the offerName has been updated) && 
            //          (an offer with the same new name does not already exist)
            if ((offerName != offer.OfferName) && (await ExistsAsync(offer.OfferName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Offer).Name),
                    UserErrorCode.NameMismatch);
            }

            // Copy over the changes
            offerDb.Copy(offer);

            // Update the offer last updated time
            offerDb.LastUpdatedTime = DateTime.UtcNow;

            // Update offerDb values and save changes in db
            _context.Offers.Update(offerDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Offer).Name, offer.OfferName));

            return offerDb;
        }

        /// <summary>
        /// Deletes an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to delete.</param>
        /// <returns>The deleted offer.</returns>
        public async Task<Offer> DeleteAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Offer).Name, offerName));

            // Get the offer that matches the offerName provide
            var offer = await GetAsync(offerName);

            // Update the offer status
            offer.Status = nameof(OfferStatus.Deleted);

            // Update the offer deleted time (soft delete)
            offer.DeletedTime = DateTime.UtcNow;

            // Update and save changes in db
            _context.Offers.Update(offer);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Offer).Name, offerName));

            return offer;
        }

        /// <summary>
        /// Checks if an offer exists.
        /// </summary>
        /// <param name="offerName">The name of the offer to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Offer).Name, offerName));

            // Check that only one offer with this offerName exists and has not been deleted
            var count = await _context.Offers
                .CountAsync(o => (o.OfferName == offerName) && (o.DeletedTime == null));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Offer).Name, offerName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Offer).Name, offerName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Offer).Name, offerName, true));
                // count = 1
                return true;
            }
        }

        /// <summary>
        /// Get offer warnings
        /// </summary>
        /// <param name="offerName">The offer name, if not provided, will check all offers</param>
        /// <returns>The warnings</returns>
        public async Task<List<OfferWarning>> GetWarningsAsync(string offerName = null)
        {
            var offerList = _context.Offers.Where(offer => (string.IsNullOrEmpty(offerName) || offer.OfferName.Equals(offerName)));
            // TODO: compare to the info in Marketplace once added the sasclient reference
            List<OfferWarning> warnings = new List<OfferWarning>();
            return warnings;
        }

        /// <summary>
        /// Publish an offer
        /// </summary>
        /// <param name="offerName"></param>
        /// <returns></returns>
        public async Task<Offer> PublishAsync(string offerName)
        {
            _logger.LogInformation($"Publishing offer {offerName}.");
            var offer = await GetAsync(offerName);

            // Update the offer status
            offer.Status = nameof(OfferStatus.Active);

            // Update the offer last updated time 
            offer.LastUpdatedTime = DateTime.UtcNow;

            // Update and save changes in db
            _context.Offers.Update(offer);
            await _context._SaveChangesAsync();

            _logger.LogInformation($"Offer {offerName} published.");
            return offer;
        }
    }
}