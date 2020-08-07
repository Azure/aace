// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for aadSecretTmp resource.
    /// </summary>
    public class AadSecretTmpService : IAadSecretTmpService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly ILogger<AadSecretTmpService> _logger;
        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="offerService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public AadSecretTmpService(ISqlDbContext sqlDbContext, IOfferService offerService, ILogger<AadSecretTmpService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all aadSecretTmps within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of aadSecretTmp objects.</returns>
        public async Task<List<AadSecretTmp>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AadSecretTmp).Name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all aadSecretTmps with a FK to the offer
            var aadSecretTmps = await _context.AadSecretTmps.Where(a => a.OfferId == offer.Id).ToListAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AadSecretTmp).Name, aadSecretTmps.Count()));

            return aadSecretTmps;
        }

        /// <summary>
        /// Gets an aadSecretTmp within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp to get.</param>
        /// <returns>An aadSecretTmp object.</returns>
        public async Task<AadSecretTmp> GetAsync(string offerName, string name)
        {
            // Check that an aadSecretTmp with the provided name exists within the given offer
            if (!(await ExistsAsync(offerName, name)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AadSecretTmp).Name,
                        name,
                        offerName: offerName));
            }

            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AadSecretTmp).Name, name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the aadSecretTmp that matches the name provided
            var aadSecretTmp = await _context.AadSecretTmps
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AadSecretTmp).Name,
                name,
                JsonSerializer.Serialize(aadSecretTmp),
                offerName: offerName));

            return aadSecretTmp;
        }

        /// <summary>
        /// Creates an aadSecretTmp object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="aadSecretTmp">The aadSecretTmp object to create.</param>
        /// <returns>The created aadSecretTmp object.</returns>
        public async Task<AadSecretTmp> CreateAsync(string offerName, AadSecretTmp aadSecretTmp)
        {
            if (aadSecretTmp is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AadSecretTmp).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an aadSecretTmp with the same name
            if (await ExistsAsync(offerName, aadSecretTmp.Name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AadSecretTmp).Name,
                    aadSecretTmp.Name,
                    offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AadSecretTmp).Name, aadSecretTmp.Name, offerName: offerName, payload: JsonSerializer.Serialize(aadSecretTmp)));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Set the FK to offer
            aadSecretTmp.OfferId = offer.Id;

            // Add aadSecretTmp to db
            _context.AadSecretTmps.Add(aadSecretTmp);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AadSecretTmp).Name, aadSecretTmp.Name, offerName: offerName));

            return aadSecretTmp;
        }

        /// <summary>
        /// Updates an aadSecretTmp object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp object to update.</param>
        /// <param name="aadSecretTmp">The updated aadSecretTmp object.</param>
        /// <returns>The updated aadSecretTmp object.</returns>
        public async Task<AadSecretTmp> UpdateAsync(string offerName, string name, AadSecretTmp aadSecretTmp)
        {
            if (aadSecretTmp is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AadSecretTmp).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the name has been updated) && 
            //          (an aadSecretTmp with the same new name does not already exist)
            if ((name != aadSecretTmp.Name) && (await ExistsAsync(offerName, aadSecretTmp.Name)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AadSecretTmp).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AadSecretTmp).Name, name, offerName: offerName, payload: JsonSerializer.Serialize(aadSecretTmp)));

            // Get the aadSecretTmp that matches the name provided
            var aadSecretTmpDb = await GetAsync(offerName, name);

            // Copy over the changes
            aadSecretTmpDb.Copy(aadSecretTmp);

            // Update aadSecretTmpDb values and save changes in db
            _context.AadSecretTmps.Update(aadSecretTmpDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AadSecretTmp).Name, name, offerName: offerName));

            return aadSecretTmpDb;
        }

        /// <summary>
        /// Deletes an aadSecretTmp object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp to delete.</param>
        /// <returns>The deleted aadSecretTmp object.</returns>
        public async Task<AadSecretTmp> DeleteAsync(string offerName, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AadSecretTmp).Name, name, offerName: offerName));

            // Get the aadSecretTmp that matches the name provided
            var aadSecretTmp = await GetAsync(offerName, name);

            // Remove the aadSecretTmp from the db
            _context.AadSecretTmps.Remove(aadSecretTmp);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AadSecretTmp).Name, name, offerName: offerName));

            return aadSecretTmp;
        }

        /// <summary>
        /// Checks if an aadSecretTmp object exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the aadSecretTmp to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AadSecretTmp).Name, name, offerName: offerName));

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one aadSecretTmp with this name exists within the offer
            var count = await _context.AadSecretTmps
                .CountAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AadSecretTmp).Name,
                    name,
                    offerName: offerName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AadSecretTmp).Name, name, false, offerName: offerName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AadSecretTmp).Name, name, true, offerName: offerName));
                // count = 1
                return true;
            }
        }
    }
}
