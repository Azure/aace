using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public class SubscriptionCustomMeterUsageService : ISubscriptionCustomMeterUsageService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<SubscriptionCustomMeterUsageService> _logger;
        private readonly ICustomMeterService _customMeterService;
        private readonly ISubscriptionService _subscriptionService;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        public SubscriptionCustomMeterUsageService(
            ISqlDbContext sqlDbContext, 
            ICustomMeterService customMeterService,
            ISubscriptionService subscriptionService,
            ILogger<SubscriptionCustomMeterUsageService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _customMeterService = customMeterService ?? throw new ArgumentException(nameof(customMeterService));
            _subscriptionService = subscriptionService ?? throw new ArgumentException(nameof(subscriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Gets all SubscriptionCustomMeterUsage.
        /// </summary>
        /// <returns>A list of SubscriptionCustomMeterUsage.</returns>
        public async Task<List<SubscriptionCustomMeterUsage>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(SubscriptionCustomMeterUsage).Name));

            // Get all customMeters from db
            var subscriptionCustomMeterUsages = await _context.SubscriptionCustomMeterUsages.ToListAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(SubscriptionCustomMeterUsage).Name, subscriptionCustomMeterUsages.Count()));

            return subscriptionCustomMeterUsages;
        }


        /// <summary>
        /// Get SubscriptionMeterUsage by meter id
        /// </summary>
        /// <param name="meterId">The meter id</param>
        /// <returns>The SubscriptionMeterUsage</returns>
        public async Task<List<SubscriptionCustomMeterUsage>> GetAllByMeterIdAsync(long meterId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(SubscriptionCustomMeterUsage).Name));

            // Get all customMeters from db
            var subscriptionCustomMeterUsages = await _context.SubscriptionCustomMeterUsages.Where(s => s.MeterId == meterId).ToListAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(SubscriptionCustomMeterUsage).Name, subscriptionCustomMeterUsages.Count()));

            return subscriptionCustomMeterUsages;
        }

        /// <summary>
        /// Enable SubscriptionCustomMeterUsage by subscription id
        /// </summary>
        /// <param name="subscriptionId">subscription id</param>
        /// <returns></returns>
        public async Task EnableSubscriptionCustomMeterUsageBySubscriptionId(Guid subscriptionId)
        {
            
            var subscriptionCustomMeterUsages = await _context.SubscriptionCustomMeterUsages.Where(s => s.SubscriptionId == subscriptionId).ToListAsync();
            foreach(var usage in subscriptionCustomMeterUsages)
            {
                _logger.LogInformation($"Enable custom meter {usage.MeterId} for subscription {subscriptionId}.");
                usage.IsEnabled = true;
                usage.LastUpdatedTime = DateTime.UtcNow;
                usage.EnabledTime = usage.LastUpdatedTime;
                _context.SubscriptionCustomMeterUsages.Update(usage);
            }

            await _context._SaveChangesAsync();
        }

        /// <summary>
        /// Disable SubscriptionCustomMeterUsage by subscription id
        /// </summary>
        /// <param name="subscriptionId">Subscription Id</param>
        /// <returns></returns>
        public async Task DisableSubscriptionCustomMeterUsageBySubscriptionId(Guid subscriptionId)
        {

            var subscriptionCustomMeterUsages = await _context.SubscriptionCustomMeterUsages.Where(s => s.SubscriptionId == subscriptionId).ToListAsync();
            foreach (var usage in subscriptionCustomMeterUsages)
            {
                _logger.LogInformation($"Disable custom meter {usage.MeterId} for subscription {subscriptionId}.");
                usage.IsEnabled = false;
                usage.DisabledTime = DateTime.UtcNow;
                _context.SubscriptionCustomMeterUsages.Update(usage);
            }

            await _context._SaveChangesAsync();

        }

        /// <summary>
        /// Get the effective start time by meter id
        /// </summary>
        /// <param name="meterid">The meter id</param>
        /// <returns>The effective start time which is the min lastUpdatedTime within all subscriptions</returns>
        public async Task<DateTime> GetEffectiveStartTimeByMeterIdAsync(long meterId)
        {
            var subscriptionCustomMeterUsage = await _context.SubscriptionCustomMeterUsages.
                Where(s => s.MeterId == meterId && s.IsEnabled).OrderBy(s => s.LastUpdatedTime).FirstOrDefaultAsync();

            return subscriptionCustomMeterUsage != null ? RoundDownDateTimeToHours(subscriptionCustomMeterUsage.LastUpdatedTime) : DateTime.MaxValue;
        }

        /// <summary>
        /// Round down DateTime to hours. For example: 19:02:04 10/10/2019 to 19:00:00 10/10/2019
        /// </summary>
        /// <param name="dateTime">The input datetime</param>
        /// <returns>The round down datetime</returns>
        private DateTime RoundDownDateTimeToHours(DateTime dateTime)
        {
            long ticks = dateTime.Ticks;
            return new DateTime(ticks - (ticks % 36000000000));
        }

        /// <summary>
        /// Gets a SubscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the SubscriptionCustomMeterUsage.</param>
        /// <returns>The SubscriptionCustomMeterUsage.</returns>
        public async Task<SubscriptionCustomMeterUsage> GetAsync(Guid subscriptionId, string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(SubscriptionCustomMeterUsage).Name, meterName));

            if (!(await ExistsAsync(subscriptionId, meterName)))
            {
                throw new LunaNotFoundUserException(
                    LoggingUtils.ComposeNotFoundErrorMessage(typeof(SubscriptionCustomMeterUsage).Name,
                    meterName,
                    subscriptionId: subscriptionId));
            }

            var subscription = await _subscriptionService.GetAsync(subscriptionId);

            // Get the subscriptionCustomMeterUsage that matches the provided subscription id and meterName
            var customMeter = await _customMeterService.GetAsync(subscription.OfferName, meterName);
            var subscriptionCustomMeterUsage = await _context.SubscriptionCustomMeterUsages
                .SingleOrDefaultAsync(c => c.MeterId == customMeter.Id && c.SubscriptionId.Equals(subscriptionId));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(
                typeof(SubscriptionCustomMeterUsage).Name,
                meterName,
                JsonSerializer.Serialize(subscriptionCustomMeterUsage)));

            return subscriptionCustomMeterUsage;
        }

        /// <summary>
        /// Creates a subscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the SubscriptionCustomMeterUsage to update.</param>
        /// <param name="subscriptionCustomMeterUsage">The subscriptionCustomMeterUsage to create.</param>
        /// <returns>The created subscriptionCustomMeterUsage.</returns>
        public async Task<SubscriptionCustomMeterUsage> CreateAsync(
            Guid subscriptionId, 
            string meterName, 
            SubscriptionCustomMeterUsage subscriptionCustomMeterUsage)
        {
            if (subscriptionCustomMeterUsage is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(SubscriptionCustomMeterUsage).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(
                typeof(SubscriptionCustomMeterUsage).Name, 
                subscriptionCustomMeterUsage.MeterId.ToString(),
                subscriptionId: subscriptionCustomMeterUsage.SubscriptionId,
                payload: JsonSerializer.Serialize(subscriptionCustomMeterUsage)));

            var subscription = await _subscriptionService.GetAsync(subscriptionId);

            var customMeter = await _customMeterService.GetAsync(subscription.OfferName, meterName);
            // Check that an SubscriptionCustomMeterUsage with the same name does not already exist
            if (await ExistsAsync(subscriptionId, meterName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(
                    typeof(SubscriptionCustomMeterUsage).Name,
                    customMeter.MeterName,
                    subscriptionId: subscriptionId));
            }

            // Add customMeter to db
            _context.SubscriptionCustomMeterUsages.Add(subscriptionCustomMeterUsage);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(SubscriptionCustomMeterUsage).Name, 
                meterName, subscriptionId: subscriptionId));

            return subscriptionCustomMeterUsage;
        }

        /// <summary>
        /// Updates a customMeter.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter.</param>
        /// <returns>The updated customMeter.</returns>
        public async Task<SubscriptionCustomMeterUsage> UpdateAsync(Guid subscriptionId, 
            string meterName, 
            SubscriptionCustomMeterUsage subscriptionCustomMeterUsage)
        {
            if (subscriptionCustomMeterUsage is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(SubscriptionCustomMeterUsage).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            var subscription = await _subscriptionService.GetAsync(subscriptionId);
            var customMeter = await _customMeterService.GetAsync(subscription.OfferName, meterName);

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(
                typeof(SubscriptionCustomMeterUsage).Name, 
                customMeter.MeterName, 
                payload: JsonSerializer.Serialize(customMeter),
                subscriptionId: subscriptionId));

            // Get the customMeter that matches the meterName provided
            var subscriptionCustomMeterUsageDb = await GetAsync(subscriptionId, meterName);

            // Check if (the meterName has been updated) && 
            //          (a customMeter with the same new name does not already exist)
            if (subscriptionCustomMeterUsageDb.SubscriptionId != subscriptionCustomMeterUsage.SubscriptionId || 
                subscriptionCustomMeterUsageDb.MeterId != subscriptionCustomMeterUsage.MeterId)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(SubscriptionCustomMeterUsage).Name),
                    UserErrorCode.NameMismatch);
            }

            // Copy over the changes
            subscriptionCustomMeterUsageDb.Copy(subscriptionCustomMeterUsage);

            // Update customMeterDb values and save changes in db
            _context.SubscriptionCustomMeterUsages.Update(subscriptionCustomMeterUsageDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(
                typeof(SubscriptionCustomMeterUsage).Name, 
                customMeter.MeterName,
                subscriptionId: subscriptionId));

            return subscriptionCustomMeterUsageDb;
        }

        /// <summary>
        /// Update the LastUpdatedTime for unreported subscriptions
        /// </summary>
        /// <param name="offerName">The offer name time</param>
        /// <param name="meterName">The meter name time</param>
        /// <param name="lastUpdatedTime">The last updated time</param>
        /// <returns></returns>
        public async Task UpdateLastUpdatedTimeForUnreportedSubscriptions(string offerName, string meterName, DateTime lastUpdatedTime)
        {
            var customMeter = await _customMeterService.GetAsync(offerName, meterName);
            var usages = await _context.SubscriptionCustomMeterUsages.Where(s => s.MeterId == customMeter.Id 
            && s.IsEnabled && s.LastUpdatedTime < lastUpdatedTime && s.LastErrorReportedTime < lastUpdatedTime).ToListAsync();

            foreach (var usage in usages)
            {
                usage.LastUpdatedTime = lastUpdatedTime;

                if (usage.UnsubscribedTime < lastUpdatedTime)
                {
                    usage.DisabledTime = lastUpdatedTime;
                    usage.IsEnabled = false;
                }
            }

            await _context._SaveChangesAsync();
            return;
        }

        /// <summary>
        /// Deletes a subscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>The deleted subscriptionCustomMeterUsage.</returns>
        public async Task<SubscriptionCustomMeterUsage> DeleteAsync(Guid subscriptionId, string meterName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(SubscriptionCustomMeterUsage).Name, meterName, subscriptionId: subscriptionId));

            // Get the customMeter that matches the meterName provide
            var subscriptionCustomMeterUsage = await GetAsync(subscriptionId, meterName);

            // Update and save changes in db
            _context.SubscriptionCustomMeterUsages.Remove(subscriptionCustomMeterUsage);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(SubscriptionCustomMeterUsage).Name, meterName, subscriptionId: subscriptionId));

            return subscriptionCustomMeterUsage;
        }

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(Guid subscriptionId, string meterName)
        {

            var subscription = await _subscriptionService.GetAsync(subscriptionId);
            var customMeter = await _customMeterService.GetAsync(subscription.OfferName, meterName);
            // Check that only one customMeter with this meterName exists
            var count = await _context.SubscriptionCustomMeterUsages
                .CountAsync(c => c.MeterId == customMeter.Id && c.SubscriptionId.Equals(subscriptionId));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(SubscriptionCustomMeterUsage).Name,
                    meterName, subscriptionId: subscriptionId));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(SubscriptionCustomMeterUsage).Name, meterName, false, subscriptionId: subscriptionId));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(SubscriptionCustomMeterUsage).Name, meterName, true, subscriptionId: subscriptionId));
                // count = 1
                return true;
            }
        }
    }
}