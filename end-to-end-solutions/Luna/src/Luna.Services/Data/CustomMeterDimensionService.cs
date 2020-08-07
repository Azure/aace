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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the customMeterDimension resource.
    /// </summary>
    public class CustomMeterDimensionService : ICustomMeterDimensionService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly IPlanService _planService;
        private readonly ICustomMeterService _customMeterService;
        private readonly ILogger<CustomMeterDimensionService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">A service to inject.</param>
        /// <param name="planService">A service to inject.</param>
        /// <param name="customMeterService">A service to inject.</param>
        public CustomMeterDimensionService(ISqlDbContext sqlDbContext, 
            IOfferService offerService,
            IPlanService planService, 
            ICustomMeterService customMeterService, 
            ILogger<CustomMeterDimensionService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService;
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _customMeterService = customMeterService ?? throw new ArgumentNullException(nameof(customMeterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all customMeterDimensions within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <returns>A list of customMeterDimensions.</returns>
        public async Task<List<CustomMeterDimension>> GetAllAsync(string offerName, string planName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(CustomMeterDimension).Name, offerName: offerName));

            // Get the plan associated with the offerName and planUniqueName provided
            var plan = await _planService.GetAsync(offerName, planName);

            // Get all customMeterDimensions with a FK to the plan
            var customMeterDimensions = await _context.CustomMeterDimensions.Where(c => c.PlanId == plan.Id).ToListAsync();

            // Set each customMeterDimension's meterName
            foreach (CustomMeterDimension customMeterDimension in customMeterDimensions)
            {
                customMeterDimension.MeterName = (await _context.CustomMeters.FindAsync(customMeterDimension.MeterId)).MeterName;
                customMeterDimension.PlanName = plan.PlanName;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(CustomMeterDimension).Name, customMeterDimensions.Count()));

            return customMeterDimensions;
        }

        /// <summary>
        /// Gets a customMeterDimension by id.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension.</param>
        /// <returns>The customMeterDimension.</returns>
        public async Task<CustomMeterDimension> GetAsync(string offerName, string planName, string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(CustomMeterDimension).Name, meterName, planName, offerName));

            var meter = await _customMeterService.GetAsync(offerName, meterName);
            var plan = await _planService.GetAsync(offerName, planName);
            // Find the customMeterDimension that matches the id provided
            var customMeterDimension = await _context.CustomMeterDimensions.SingleOrDefaultAsync(c => c.PlanId == plan.Id && c.MeterId == meter.Id);

            // Check that an customMeterDimension with the provided id exists 
            if (customMeterDimension is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(CustomMeterDimension).Name,
                     meterName, planName, offerName));
            }

            // Set the customMeterDimension's meterName
            customMeterDimension.MeterName = meter.MeterName;
            customMeterDimension.PlanName = plan.PlanName;

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(CustomMeterDimension).Name,
                meterName,
                JsonSerializer.Serialize(customMeterDimension),
                planName,
                offerName));

            return customMeterDimension;
        }

        /// <summary>
        /// Creates a customMeterDimension within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <param name="customMeterDimension">The customMeterDimension object to create.</param>
        /// <returns>The created customMeterDimension.</returns>
        public async Task<CustomMeterDimension> CreateAsync(string offerName, string planName, string meterName, CustomMeterDimension customMeterDimension)
        {
            if (customMeterDimension is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeterDimension).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(
                typeof(CustomMeterDimension).Name,
                meterName,
                offerName: offerName,
                planName: planName,
                payload: JsonSerializer.Serialize(customMeterDimension)));

            // Get the plan associated with the offerName and planUniqueName provided
            var plan = await _planService.GetAsync(offerName, planName);

            // Set the FK to plan
            customMeterDimension.PlanId = plan.Id;

            // Set the FK to customMeter
            customMeterDimension.MeterId = (await _customMeterService.GetAsync(offerName, meterName)).Id;

            // Reset the PK (should not be modified in request)
            customMeterDimension.Id = 0;

            // Add customMeterDimension to db
            _context.CustomMeterDimensions.Add(customMeterDimension);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(
                typeof(CustomMeterDimension).Name,
                meterName,
                offerName: offerName,
                planName: planName));

            return customMeterDimension;
        }

        /// <summary>
        /// Updates a customMeterDimension.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeterDimension</param>
        /// <param name="planName">The plan name of the customMeterDimension</param>
        /// <param name="meterName">The meter name of the customMeterDimension</param>
        /// <param name="customMeterDimension">The updated customMeterDimension.</param>
        /// <returns>The updated customMeterDimension.</returns>
        public async Task<CustomMeterDimension> UpdateAsync(string offerName, string planName, string meterName, CustomMeterDimension customMeterDimension)
        {
            if (customMeterDimension is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeterDimension).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(
                typeof(CustomMeterDimension).Name,
                meterName,
                planName: planName,
                payload: JsonSerializer.Serialize(customMeterDimension)));

            // Get the customMeterDimension that matches the id provided
            var customMeterDimensionDb = await GetAsync(offerName, planName, meterName);

            // update the FK to customMeter in case the meterName has been changed
            customMeterDimensionDb.MeterId = (await _customMeterService.GetAsync(offerName, meterName)).Id;
            customMeterDimensionDb.PlanId = (await _planService.GetAsync(offerName, planName)).Id;

            // Copy over the changes
            customMeterDimensionDb.Copy(customMeterDimension);

            // Update customMeterDimensionDb values and save changes in db
            _context.CustomMeterDimensions.Update(customMeterDimensionDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(
                typeof(CustomMeterDimension).Name,
                meterName,
                planName: planName));

            return customMeterDimensionDb;
        }

        /// <summary>
        /// Deletes a customMeterDimension.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeterDimension</param>
        /// <param name="planName">The plan name of the customMeterDimension</param>
        /// <param name="meterName">The meter name of the customMeterDimension</param>
        /// <returns>The deleted customMeterDimension.</returns>
        public async Task<CustomMeterDimension> DeleteAsync(string offerName, string planName, string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(CustomMeterDimension).Name, meterName, planName, offerName));

            // Get the customMeterDimension that matches the id provided
            var customMeterDimension = await GetAsync(offerName, planName, meterName);

            // Remove the customMeterDimension from the db
            _context.CustomMeterDimensions.Remove(customMeterDimension);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(CustomMeterDimension).Name, meterName, planName, offerName));

            return customMeterDimension;
        }

        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string planName, string meterName)
        {
            var plan = await _planService.GetAsync(offerName, planName);
            var meter = await _customMeterService.GetAsync(offerName, meterName);
            // Check that only one customMeter with this meterName exists
            var count = await _context.CustomMeterDimensions
                .CountAsync(c => c.PlanId == plan.Id && c.MeterId == meter.Id);

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(CustomMeterDimension).Name,
                    meterName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(CustomMeterDimension).Name, meterName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(CustomMeterDimension).Name, meterName, true));
                // count = 1
                return true;
            }
        }
    }
}