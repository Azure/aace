using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the customMeter resource.
    /// </summary>
    public interface ISubscriptionCustomMeterUsageService
    {
        /// <summary>
        /// Gets all SubscriptionCustomMeterUsage.
        /// </summary>
        /// <returns>A list of SubscriptionCustomMeterUsage.</returns>
        Task<List<SubscriptionCustomMeterUsage>> GetAllAsync();

        /// <summary>
        /// Gets a SubscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the SubscriptionCustomMeterUsage.</param>
        /// <returns>The SubscriptionMeterUsage.</returns>
        Task<SubscriptionCustomMeterUsage> GetAsync(Guid subscriptionId, string meterName);

        /// <summary>
        /// Get SubscriptionMeterUsage by meter id
        /// </summary>
        /// <param name="meterId">The meter id</param>
        /// <returns>The SubscriptionMeterUsage</returns>
        Task<List<SubscriptionCustomMeterUsage>> GetAllByMeterIdAsync(long meterId);

        /// <summary>
        /// Get the effective start time by meter id
        /// </summary>
        /// <param name="meterid">The meter id</param>
        /// <returns>The effective start time which is the min lastUpdatedTime within all subscriptions</returns>
        Task<DateTime> GetEffectiveStartTimeByMeterIdAsync(long meterId);

        /// <summary>
        /// Enable SubscriptionCustomMeterUsage by subscription id
        /// </summary>
        /// <param name="subscriptionId">subscription id</param>
        /// <returns></returns>
        Task EnableSubscriptionCustomMeterUsageBySubscriptionId(Guid subscriptionId);

        /// <summary>
        /// Disable SubscriptionCustomMeterUsage by subscription id
        /// </summary>
        /// <param name="subscriptionId">Subscription Id</param>
        /// <returns></returns>
        Task DisableSubscriptionCustomMeterUsageBySubscriptionId(Guid subscriptionId);

        /// <summary>
        /// Creates a SubscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the SubscriptionCustomMeterUsage to update.</param>
        /// <param name="customMeter">The SubscriptionCustomMeterUsage to create.</param>
        /// <returns>The created SubscriptionCustomMeterUsage.</returns>
        Task<SubscriptionCustomMeterUsage> CreateAsync(Guid subscriptionId, string meterName, SubscriptionCustomMeterUsage subscriptionCustomMeterUsage);

        /// <summary>
        /// Updates a SubscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the SubscriptionCustomMeterUsage to update.</param>
        /// <param name="customMeter">The updated SubscriptionCustomMeterUsage.</param>
        /// <returns>The updated customMeter.</returns>
        Task<SubscriptionCustomMeterUsage> UpdateAsync(Guid subscriptionId, string meterName, SubscriptionCustomMeterUsage customMeter);

        /// <summary>
        /// Update the LastUpdatedTime for unreported subscriptions
        /// </summary>
        /// <param name="offerName">The offer name time</param>
        /// <param name="meterName">The meter name time</param>
        /// <param name="lastUpdatedTime">The last updated time</param>
        /// <returns></returns>
        Task UpdateLastUpdatedTimeForUnreportedSubscriptions(string offerName, string meterName, DateTime lastUpdatedTime);

        /// <summary>
        /// Deletes a SubscriptionCustomMeterUsage.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the SubscriptionCustomMeterUsage to delete.</param>
        /// <returns>The deleted customMeter.</returns>
        Task<SubscriptionCustomMeterUsage> DeleteAsync(Guid subscriptionId, string meterName);

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="meterName">The name of the customMeter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(Guid subscriptionId, string meterName);
    }
}