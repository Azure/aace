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
    /// Service class that handles basic CRUD functionality for customMeterService resource.
    /// </summary>
    public class CustomMeterService : ICustomMeterService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<CustomMeterService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        public CustomMeterService(ISqlDbContext sqlDbContext, ILogger<CustomMeterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Gets all customMeters.
        /// </summary>
        /// <returns>A list of customMeters.</returns>
        public async Task<List<CustomMeter>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(CustomMeter).Name));

            // Get all customMeters from db
            var customMeters = await _context.CustomMeters.ToListAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(CustomMeter).Name, customMeters.Count()));

            return customMeters;
        }

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter.</param>
        /// <returns>The customMeter.</returns>
        public async Task<CustomMeter> GetAsync(string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(CustomMeter).Name, meterName));

            if (!(await ExistsAsync(meterName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(CustomMeter).Name,
                        meterName));
            }

            // Get the customMeter that matches the provided meterName
            var customMeter = await _context.CustomMeters.SingleOrDefaultAsync(c => c.MeterName == meterName);

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(
                typeof(CustomMeter).Name,
                meterName,
                JsonSerializer.Serialize(customMeter)));

            return customMeter;
        }

        /// <summary>
        /// Creates a customMeter.
        /// </summary>
        /// <param name="customMeter">The customMeter to create.</param>
        /// <returns>The created customMeter.</returns>
        public async Task<CustomMeter> CreateAsync(CustomMeter customMeter)
        {
            if (customMeter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeter).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(CustomMeter).Name, customMeter.MeterName, payload: JsonSerializer.Serialize(customMeter)));

            // Check that an customMeter with the same name does not already exist
            if (await ExistsAsync(customMeter.MeterName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(CustomMeter).Name,
                    customMeter.MeterName));
            }

            // Add customMeter to db
            _context.CustomMeters.Add(customMeter);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(CustomMeter).Name, customMeter.MeterName));

            return customMeter;
        }

        /// <summary>
        /// Updates a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter.</param>
        /// <returns>The updated customMeter.</returns>
        public async Task<CustomMeter> UpdateAsync(string meterName, CustomMeter customMeter)
        {
            if (customMeter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeter).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(CustomMeter).Name, customMeter.MeterName, payload: JsonSerializer.Serialize(customMeter)));

            // Get the customMeter that matches the meterName provided
            var customMeterDb = await GetAsync(meterName);

            // Check if (the meterName has been updated) && 
            //          (a customMeter with the same new name does not already exist)
            if ((meterName != customMeter.MeterName) && (await ExistsAsync(customMeter.MeterName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(CustomMeter).Name),
                    UserErrorCode.NameMismatch);
            }

            // Copy over the changes
            customMeterDb.Copy(customMeter);

            // Update customMeterDb values and save changes in db
            _context.CustomMeters.Update(customMeterDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(CustomMeter).Name, customMeter.MeterName));

            return customMeterDb;
        }

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>The deleted customMeter.</returns>
        public async Task<CustomMeter> DeleteAsync(string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(CustomMeter).Name, meterName));

            // Get the customMeter that matches the meterName provide
            var customMeter = await GetAsync(meterName);

            // Update and save changes in db
            _context.CustomMeters.Remove(customMeter);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(CustomMeter).Name, meterName));

            return customMeter;
        }

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string meterName)
        {
            // Check that only one customMeter with this meterName exists
            var count = await _context.CustomMeters
                .CountAsync(c => c.MeterName == meterName);

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(CustomMeter).Name,
                    meterName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(CustomMeter).Name, meterName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(CustomMeter).Name, meterName, true));
                // count = 1
                return true;
            }
        }
    }
}