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
using Luna.Data.Enums;
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
        private readonly IOfferService _offerService;
        private readonly ITelemetryDataConnectorService _telemetryDataconnectorService;
        private readonly ILogger<CustomMeterService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="offerService">The offer service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sqlDbContext">The context to inject.</param>
        public CustomMeterService(IOfferService offerService, 
            ITelemetryDataConnectorService telemetryDataConnectorService,
            ISqlDbContext sqlDbContext, 
            ILogger<CustomMeterService> logger)
        {
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _telemetryDataconnectorService = telemetryDataConnectorService ?? throw new ArgumentNullException(nameof(telemetryDataConnectorService));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Gets all customMeters.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <returns>A list of customMeters.</returns>
        public async Task<List<CustomMeter>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(CustomMeter).Name, offerName: offerName));

            var offer = await _offerService.GetAsync(offerName);

            // Get all customMeters from db
            var customMeters = await _context.CustomMeters.Where(c => c.OfferId == offer.Id).ToListAsync();

            foreach (var meter in customMeters)
            {
                meter.OfferName = offerName;
                var connector = await _context.TelemetryDataConnectors.FindAsync(meter.TelemetryDataConnectorId);
                meter.TelemetryDataConnectorName = connector.Name;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(CustomMeter).Name, customMeters.Count()));

            return customMeters;
        }

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter.</param>
        /// <returns>The customMeter.</returns>
        public async Task<CustomMeter> GetAsync(string offerName, string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(CustomMeter).Name, meterName, offerName: offerName));

            if (!(await ExistsAsync(offerName, meterName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(CustomMeter).Name,
                        meterName, offerName: offerName));
            }

            var offer = await _offerService.GetAsync(offerName);

            // Get the customMeter that matches the provided meterName
            var customMeter = await _context.CustomMeters.SingleOrDefaultAsync(c => c.OfferId == offer.Id && c.MeterName == meterName);

            customMeter.OfferName = offerName;
            var connector = await _context.TelemetryDataConnectors.FindAsync(customMeter.TelemetryDataConnectorId);
            customMeter.TelemetryDataConnectorName = connector.Name;

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(
                typeof(CustomMeter).Name,
                meterName,
                JsonSerializer.Serialize(customMeter),
                offerName: offerName));

            return customMeter;
        }

        /// <summary>
        /// Creates a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter.</param>
        /// <param name="customMeter">The customMeter to create.</param>
        /// <returns>The created customMeter.</returns>
        public async Task<CustomMeter> CreateAsync(string offerName, string meterName, CustomMeter customMeter)
        {
            if (customMeter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeter).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(CustomMeter).Name, customMeter.MeterName, offerName: offerName, payload: JsonSerializer.Serialize(customMeter)));

            // Check that an customMeter with the same name does not already exist
            if (await ExistsAsync(offerName, meterName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(CustomMeter).Name,
                    customMeter.MeterName, offerName: offerName));
            }

            var offer = await _offerService.GetAsync(offerName);
            var connector = await _telemetryDataconnectorService.GetAsync(customMeter.TelemetryDataConnectorName);
            customMeter.TelemetryDataConnectorId = connector.Id;
            customMeter.OfferId = offer.Id;


            using (var transaction = await _context.BeginTransactionAsync())
            {
                // Not using subscriptionService here to avoid circular reference
                List<Subscription> subscriptionList = _context.Subscriptions.Where(s => s.OfferId == offer.Id
                && (s.Status == FulfillmentState.Subscribed.ToString() ||
                s.Status == FulfillmentState.Suspended.ToString() ||
                s.Status == FulfillmentState.PendingFulfillmentStart.ToString())).ToList();

                _context.CustomMeters.Add(customMeter);

                await _context._SaveChangesAsync();

                // Add customMeter to db
                foreach (var sub in subscriptionList)
                {
                    bool isEnabled = sub.Status == FulfillmentState.Subscribed.ToString() || sub.Status == FulfillmentState.Suspended.ToString();
                    _context.SubscriptionCustomMeterUsages.Add(new SubscriptionCustomMeterUsage(customMeter.Id, sub.SubscriptionId, isEnabled));
                }

                await _context._SaveChangesAsync();
                transaction.Commit();
            }


            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(CustomMeter).Name, customMeter.MeterName, offerName: offerName));

            return customMeter;
        }

        /// <summary>
        /// Updates a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter.</param>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter.</param>
        /// <returns>The updated customMeter.</returns>
        public async Task<CustomMeter> UpdateAsync(string offerName, string meterName, CustomMeter customMeter)
        {
            if (customMeter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(CustomMeter).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(CustomMeter).Name, customMeter.MeterName, payload: JsonSerializer.Serialize(customMeter)));

            // Get the customMeter that matches the meterName provided
            var customMeterDb = await GetAsync(offerName, meterName);

            if ((meterName != customMeter.MeterName) || (offerName != customMeter.OfferName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(CustomMeter).Name),
                    UserErrorCode.NameMismatch);
            }

            // Copy over the changes
            customMeterDb.Copy(customMeter);

            var offer = await _offerService.GetAsync(offerName);
            var connector = await _telemetryDataconnectorService.GetAsync(customMeter.TelemetryDataConnectorName);
            customMeterDb.TelemetryDataConnectorId = connector.Id;
            customMeterDb.OfferId = offer.Id;

            // Update customMeterDb values and save changes in db
            _context.CustomMeters.Update(customMeterDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(CustomMeter).Name, customMeter.MeterName));

            return customMeterDb;
        }

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter to delete.</param>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>The deleted customMeter.</returns>
        public async Task<CustomMeter> DeleteAsync(string offerName, string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(CustomMeter).Name, meterName));

            // Get the customMeter that matches the meterName provide
            var customMeter = await GetAsync(offerName, meterName);

            // Update and save changes in db
            _context.CustomMeters.Remove(customMeter);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(CustomMeter).Name, meterName));

            return customMeter;
        }

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter to check exists.</param>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string meterName)
        {
            var offer = await _offerService.GetAsync(offerName);
            // Check that only one customMeter with this meterName exists
            var count = await _context.CustomMeters
                .CountAsync(c => c.OfferId == offer.Id && c.MeterName == meterName);

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