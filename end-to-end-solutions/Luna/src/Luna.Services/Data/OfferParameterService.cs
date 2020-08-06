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
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service clas that handles basic CRUD functionality for the offerParameter resource.
    /// </summary>
    public class OfferParameterService : IOfferParameterService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly ILogger<OfferParameterService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public OfferParameterService(ISqlDbContext sqlDbContext, IOfferService offerService, ILogger<OfferParameterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all offerParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of offersParameters.</returns>
        public async Task<List<OfferParameter>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(OfferParameter).Name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all offerParameters with a FK to the offer
            var offerParameters = await _context.OfferParameters.Where(o => o.OfferId == offer.Id).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(OfferParameter).Name, offerParameters.Count()));

            return offerParameters;
        }

        /// <summary>
        /// Gets an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to get.</param>
        /// <returns>The offerParameter.</returns>
        public async Task<OfferParameter> GetAsync(string offerName, string parameterName)
        {
            // Check that an offerParameter with the provided parameterName exists within the given offer
            if (!(await ExistsAsync(offerName, parameterName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(OfferParameter).Name,
                        parameterName,
                        offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(OfferParameter).Name, parameterName, offerName: offerName));


            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the offerParameter that matches the parameterName provided
            var offerParameter = await _context.OfferParameters
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.ParameterName == parameterName));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(OfferParameter).Name,
                parameterName,
                JsonSerializer.Serialize(offerParameter),
                offerName: offerName));

            return offerParameter;
        }

        /// <summary>
        /// Creates an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="offerParameter">The offerParameter to create.</param>
        /// <returns>The created offerParameter.</returns>
        public async Task<OfferParameter> CreateAsync(string offerName, OfferParameter offerParameter)
        {
            if (offerParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(OfferParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an offerParameter with the same parameterName
            if (await ExistsAsync(offerName, offerParameter.ParameterName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(OfferParameter).Name,
                    offerParameter.ParameterName,
                    offerName: offerName));
            }

            if (ExpressionEvaluationUtils.ReservedParameterNames.Contains(offerParameter.ParameterName))
            {
                throw new LunaConflictUserException($"Parameter {offerParameter.ParameterName} is reserved. Please use a different name.");
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(OfferParameter).Name, offerParameter.ParameterName, offerName: offerName, payload: JsonSerializer.Serialize(offerParameter)));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Set the FK to offer
            offerParameter.OfferId = offer.Id;

            // Add offerParameter to db
            _context.OfferParameters.Add(offerParameter);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(OfferParameter).Name, offerParameter.ParameterName, offerName: offerName));

            return offerParameter;
        }

        /// <summary>
        /// Updates an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to update.</param>
        /// <param name="offerParameter">The updated offerParameter.</param>
        /// <returns>The updated offerParameter.</returns>
        public async Task<OfferParameter> UpdateAsync(string offerName, string parameterName, OfferParameter offerParameter)
        {
            if (offerParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(OfferParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the parameterName has been updated) && 
            //          (an offerParameter with the same new parameterName does not already exist)
            if ((parameterName != offerParameter.ParameterName) && (await ExistsAsync(offerName, offerParameter.ParameterName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(OfferParameter).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(OfferParameter).Name, parameterName, offerName: offerName, payload: JsonSerializer.Serialize(offerParameter)));

            // Get the offerParameter that matches the parameterName provided
            var offerParameterDb = await GetAsync(offerName, parameterName);

            // Copy over the changes
            offerParameterDb.Copy(offerParameter);

            // Update offerParameterDb values and save changes in db
            _context.OfferParameters.Update(offerParameterDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(OfferParameter).Name, parameterName, offerName: offerName));

            return offerParameterDb;
        }

        /// <summary>
        /// Deletes an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to delete.</param>
        /// <returns>The deleted offerParameter.</returns>
        public async Task<OfferParameter> DeleteAsync(string offerName, string parameterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(OfferParameter).Name, parameterName, offerName: offerName));

            // Get the offerParameter that matches the parameterName provided
            var offerParameter = await GetAsync(offerName, parameterName);

            // Remove the offerParameter from the db
            _context.OfferParameters.Remove(offerParameter);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(OfferParameter).Name, parameterName, offerName: offerName));

            return offerParameter;
        }

        /// <summary>
        /// Checks if an offerParameter exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string parameterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(OfferParameter).Name, parameterName, offerName: offerName));

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one offerParameter with this parameterName exists within the offer
            var count = await _context.OfferParameters
                .CountAsync(a => (a.OfferId == offer.Id) && (a.ParameterName == parameterName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(OfferParameter).Name,
                    parameterName,
                    offerName: offerName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(OfferParameter).Name, parameterName, false, offerName: offerName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(OfferParameter).Name, parameterName, true, offerName: offerName));
                // count = 1
                return true;
            }
        }
    }
}