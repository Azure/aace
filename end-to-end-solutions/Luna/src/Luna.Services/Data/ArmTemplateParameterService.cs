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
    /// Service class that handles basic CRUD functionality for the armTemplateParameter resource.
    /// </summary>
    public class ArmTemplateParameterService : IArmTemplateParameterService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly IArmTemplateArmTemplateParameterService _armTemplateArmTemplateParameterService;
        private readonly ILogger<ArmTemplateParameterService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public ArmTemplateParameterService(
            ISqlDbContext sqlDbContext,
            IOfferService offerService,
            IArmTemplateArmTemplateParameterService armTemplateArmTemplateParameterService, 
            ILogger<ArmTemplateParameterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _armTemplateArmTemplateParameterService = armTemplateArmTemplateParameterService ?? throw new ArgumentNullException(nameof(armTemplateArmTemplateParameterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        } 

        /// <summary>
        /// Gets all armTemplateParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of armTemplateParameter objects.</returns>
        public async Task<List<ArmTemplateParameter>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(ArmTemplateParameter).Name, offerName: offerName));
            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all armTemplateParameters with a FK to the offer
            var armTemplateParameters = await _context.ArmTemplateParameters.Where(a => a.OfferId == offer.Id).ToListAsync();

            // TODO: Delete unused parameters

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(ArmTemplateParameter).Name, armTemplateParameters.Count()));

            return armTemplateParameters;
        }

        /// <summary>
        /// Gets an armTemplateParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the armTemplateParameter to get.</param>
        /// <returns>The armTemplateParameter object.</returns>
        public async Task<ArmTemplateParameter> GetAsync(string offerName, string name)
        {
            // Check that an armTemplateParameter with the provided name exists within the given offer
            if ( !(await ExistsAsync(offerName, name)) )
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(ArmTemplateParameter).Name,
                    name, offerName: offerName));
            }

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the armTemplateParameter that matches the name provided
            var armTemplateParameter = await _context.ArmTemplateParameters
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(ArmTemplateParameter).Name,
                name,
                JsonSerializer.Serialize(armTemplateParameter),
                offerName: offerName));

            return armTemplateParameter;
        }

        /// <summary>
        /// Creates an armTemplateParameter object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="armTemplateId">The id of the armTemplate that the parameter is associated with.</param>
        /// <param name="armTemplateParameter">The armTemplateParameter to create.</param>
        /// <returns>The created armTemplateParameter.</returns>
        public async Task<ArmTemplateParameter> CreateAsync(string offerName, long armTemplateId, ArmTemplateParameter armTemplateParameter)
        {
            if (armTemplateParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(ArmTemplateParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (ExpressionEvaluationUtils.ReservedParameterNames.Contains(armTemplateParameter.Name))
            {
                _logger.LogInformation($"ARM template {armTemplateId} is referencing system parameter {armTemplateParameter.Name}.");
                return armTemplateParameter;
                //throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(ArmTemplateParameter).Name, 
                //    armTemplateParameter.Name, offerName: offerName));
            }

            Offer offer = await _offerService.GetAsync(offerName);

            // Check if the ArmTemplateParameter already exists
            if (await ExistsAsync(offerName, armTemplateParameter.Name))
            {
                // Just create a new join entry to keep track of the fact that this ArmTempate is using this parameter
                ArmTemplateParameter armTemplateParameterDb = await GetAsync(offerName, armTemplateParameter.Name);
                await _armTemplateArmTemplateParameterService.CreateJoinEntryAsync(armTemplateId, armTemplateParameterDb.Id);
                return armTemplateParameterDb;
            }

            armTemplateParameter.OfferId = offer.Id;

            _context.ArmTemplateParameters.Add(armTemplateParameter);
            await _context._SaveChangesAsync();
            await _armTemplateArmTemplateParameterService.CreateJoinEntryAsync(armTemplateId, armTemplateParameter.Id);

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(ArmTemplateParameter).Name, armTemplateParameter.Name, offerName: offerName));


            return armTemplateParameter;
        }

        /// <summary>
        /// Updates an ArmTemplateParameter within an offer by name.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the ArmTemplateParameter to update.</param>
        /// <param name="armTemplateParameter">The updated ArmTemplateParameter object.</param>
        /// <returns>The updated ArmTemplateParameter object.</returns>
        public async Task<ArmTemplateParameter> UpdateAsync(string offerName, string parameterName, ArmTemplateParameter armTemplateParameter)
        {
            if (armTemplateParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(ArmTemplateParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(ArmTemplateParameter).Name, armTemplateParameter.Name, offerName: offerName, payload: JsonSerializer.Serialize(armTemplateParameter)));

            Offer offer = await _offerService.GetAsync(offerName);
            ArmTemplateParameter armTemplateParameterDb = await GetAsync(offerName, parameterName);

            armTemplateParameterDb.Copy(armTemplateParameter);
            armTemplateParameterDb.OfferId = offer.Id;

            _context.ArmTemplateParameters.Update(armTemplateParameterDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(ArmTemplateParameter).Name, armTemplateParameter.Name, offerName: offerName));

            return armTemplateParameterDb;
        }

        /// <summary>
        /// Removes any ArmTemplateParameters from the db that are not associated with any ArmTemplates.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns></returns>
        public async Task DeleteUnusedAsync(string offerName)
        {
            List<ArmTemplateParameter> armTemplateParameters = await GetAllAsync(offerName);

            foreach (ArmTemplateParameter armTemplateParameter in armTemplateParameters)
            {
                int usages = await _context.ArmTemplateArmTemplateParameters.Where(x => x.ArmTemplateParameterId == armTemplateParameter.Id).CountAsync();

                if (usages == 0)
                {
                    _context.ArmTemplateParameters.Remove(armTemplateParameter);
                    await _context._SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Checks if an armTemplateParameter exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the armTemplateParameter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(ArmTemplateParameter).Name, name, offerName: offerName));

            if(!await _offerService.ExistsAsync(offerName))
            {
                // Instead of throw NotFound exception, just return false.
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(ArmTemplateParameter).Name, name, false, offerName: offerName));
                return false;
            }

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one armTemplateParameter with this name exists within the offer
            var count = await _context.ArmTemplateParameters
                .CountAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(ArmTemplateParameter).Name, name, offerName: offerName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(ArmTemplateParameter).Name, name, false, offerName: offerName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(ArmTemplateParameter).Name, name, true, offerName: offerName));
                // count = 1
                return true;
            }
        }
    }
}