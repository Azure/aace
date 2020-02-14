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
    /// Service class that handles basic CRUD functionality for the customMeterDimension resource.
    /// </summary>
    public class CustomMeterDimensionService : ICustomMeterDimensionService
    {
        private readonly ISqlDbContext _context;
        private readonly IPlanService _planService;
        private readonly ICustomMeterService _customMeterService;
        private readonly ILogger<CustomMeterDimensionService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="planService">A service to inject.</param>
        /// <param name="customMeterService">A service to inject.</param>
        public CustomMeterDimensionService(ISqlDbContext sqlDbContext, IPlanService planService, ICustomMeterService customMeterService, ILogger<CustomMeterDimensionService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _customMeterService = customMeterService ?? throw new ArgumentNullException(nameof(customMeterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all customMeterDimensions within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <returns>A list of customMeterDimensions.</returns>
        public async Task<List<CustomMeterDimension>> GetAllAsync(string offerName, string planUniqueName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(CustomMeterDimension).Name, offerName: offerName));

            // Get the plan associated with the offerName and planUniqueName provided
            var plan = await _planService.GetAsync(offerName, planUniqueName);

            // Get all customMeterDimensions with a FK to the plan
            var customMeterDimensions = await _context.CustomMeterDimensions.Where(c => c.PlanId == plan.Id).ToListAsync();

            // Set each customMeterDimension's meterName
            foreach (CustomMeterDimension customMeterDimension in customMeterDimensions)
            {
                customMeterDimension.MeterName = (await _context.CustomMeters.FindAsync(customMeterDimension.MeterId)).MeterName;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(CustomMeterDimension).Name, customMeterDimensions.Count()));

            return customMeterDimensions;
        }

        /// <summary>
        /// Gets a customMeterDimension by id.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension.</param>
        /// <returns>The customMeterDimension.</returns>
        public async Task<CustomMeterDimension> GetAsync(long id)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(CustomMeterDimension).Name, id.ToString()));

            // Find the customMeterDimension that matches the id provided
            var customMeterDimension = await _context.CustomMeterDimensions.FindAsync(id);

            // Check that an customMeterDimension with the provided id exists 
            if (customMeterDimension is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(CustomMeterDimension).Name,
                     id.ToString()));
            }

            // Set the customMeterDimension's meterName
            customMeterDimension.MeterName = (await _context.CustomMeters.FindAsync(customMeterDimension.MeterId)).MeterName;

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(CustomMeterDimension).Name,
                id.ToString(),
                JsonSerializer.Serialize(customMeterDimension)));

            return customMeterDimension;
        }

        /// <summary>
        /// Creates a customMeterDimension within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <param name="customMeterDimension">The customMeterDimension object to create.</param>
        /// <returns>The created customMeterDimension.</returns>
        public async Task<CustomMeterDimension> CreateAsync(string offerName, string planUniqueName, CustomMeterDimension customMeterDimension)
        {
            if (customMeterDimension is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeterDimension).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(
                typeof(CustomMeterDimension).Name,
                customMeterDimension.Id.ToString(),
                offerName: offerName,
                planName: planUniqueName,
                payload: JsonSerializer.Serialize(customMeterDimension)));

            // Get the plan associated with the offerName and planUniqueName provided
            var plan = await _planService.GetAsync(offerName, planUniqueName);

            // Set the FK to plan
            customMeterDimension.PlanId = plan.Id;

            // Set the FK to customMeter
            customMeterDimension.MeterId = (await _customMeterService.GetAsync(customMeterDimension.MeterName)).Id;

            // Reset the PK (should not be modified in request)
            customMeterDimension.Id = 0;

            // Add customMeterDimension to db
            _context.CustomMeterDimensions.Add(customMeterDimension);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(
                typeof(ArmTemplateParameter).Name,
                customMeterDimension.Id.ToString(),
                offerName: offerName,
                planName: planUniqueName));

            return customMeterDimension;
        }

        /// <summary>
        /// Updates a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension to update.</param>
        /// <param name="customMeterDimension">The updated customMeterDimension.</param>
        /// <returns>The updated customMeterDimension.</returns>
        public async Task<CustomMeterDimension> UpdateAsync(long id, CustomMeterDimension customMeterDimension)
        {
            if (customMeterDimension is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeterDimension).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(
                typeof(CustomMeterDimension).Name,
                customMeterDimension.Id.ToString(),
                planName: customMeterDimension.Plan.PlanName,
                payload: JsonSerializer.Serialize(customMeterDimension)));

            // Get the customMeterDimension that matches the id provided
            var customMeterDimensionDb = await GetAsync(id);

            // update the FK to customMeter in case the meterName has been changed
            customMeterDimensionDb.MeterId = (await _customMeterService.GetAsync(customMeterDimension.MeterName)).Id;

            // Copy over the changes
            customMeterDimensionDb.Copy(customMeterDimension);

            // Update customMeterDimensionDb values and save changes in db
            _context.CustomMeterDimensions.Update(customMeterDimensionDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(
                typeof(CustomMeterDimension).Name,
                customMeterDimension.Id.ToString(),
                planName: customMeterDimension.Plan.PlanName));

            return customMeterDimensionDb;
        }

        /// <summary>
        /// Deletes a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension to delete.</param>
        /// <returns>The deleted customMeterDimension.</returns>
        public async Task<CustomMeterDimension> DeleteAsync(long id)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(CustomMeterDimension).Name, id.ToString()));

            // Get the customMeterDimension that matches the id provided
            var customMeterDimension = await GetAsync(id);

            // Remove the customMeterDimension from the db
            _context.CustomMeterDimensions.Remove(customMeterDimension);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(CustomMeterDimension).Name, id.ToString()));

            return customMeterDimension;
        }
    }
}