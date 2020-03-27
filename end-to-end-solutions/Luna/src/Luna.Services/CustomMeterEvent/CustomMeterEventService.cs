using Luna.Clients;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Azure.Storage;
using Luna.Clients.CustomMetering;
using Luna.Clients.Models.CustomMetering;
using Luna.Clients.TelemetryDataConnectors;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Services.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.CustomMeterEvent
{
    public class CustomMeterEventService : ICustomMeterEventService
    {
        private readonly ILogger<CustomMeterEventService> _logger;
        private readonly ICustomMeterService _customMeterService;
        private readonly ICustomMeteringClient _customMeteringClient;
        private readonly TelemetryDataConnectorManager _telemetryConnectionManager;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionCustomMeterUsageService _subscriptionCustomMeterUsageService;
        private readonly IOfferService _offerService;
        private readonly ITelemetryDataConnectorService _telemetryDataConnectorService;
        private readonly ICustomMeterDimensionService _customMeterDimensionService;
        private readonly IStorageUtility _storageUtility;

        private const string REPORTED_METER_EVENT_TABLE_NAME = "ReportedMeterEvents";
        private const string EXPIRED_METER_EVENT_TABLE_NAME = "ExpiredMeterEvents";

        public CustomMeterEventService(
            IOptionsMonitor<SecuredProvisioningClientConfiguration> optionsMonitor,
            ILogger<CustomMeterEventService> logger,
            ICustomMeterService customMeterService,
            IKeyVaultHelper keyVaultHelper,
            ICustomMeteringClient customMeteringClient,
            ISubscriptionService subscriptionService,
            ISubscriptionCustomMeterUsageService subscriptionCustomMeterUsageService,
            ICustomMeterDimensionService customMeterDimensionService,
            ITelemetryDataConnectorService telemetryDataConnectorService,
            IOfferService offerService,
            IStorageUtility storageUtility)
        {
            _logger = logger;
            _customMeterService = customMeterService;
            _customMeteringClient = customMeteringClient;
            _subscriptionService = subscriptionService;
            _subscriptionCustomMeterUsageService = subscriptionCustomMeterUsageService;
            _customMeterDimensionService = customMeterDimensionService;
            _offerService = offerService;
            _telemetryDataConnectorService = telemetryDataConnectorService;
            _storageUtility = storageUtility;
            _telemetryConnectionManager = new TelemetryDataConnectorManager(new HttpClient(), logger, keyVaultHelper);
        }
        public async Task ReportBatchMeterEvents()
        {
            List<Offer> offers = await _offerService.GetAllAsync();

            foreach (var offer in offers)
            {
                List<CustomMeter> customMeters = await _customMeterService.GetAllAsync(offer.OfferName);

                foreach (var meter in customMeters)
                {
                    DateTime effectiveStartTime = await _subscriptionCustomMeterUsageService.GetEffectiveStartTimeByMeterIdAsync(meter.Id);

                    // Give 3 hours grace period for all telemetry data being stored
                    if (effectiveStartTime > DateTime.UtcNow.AddHours(-3))
                    {
                        if (effectiveStartTime == DateTime.MaxValue)
                        {
                            _logger.LogInformation($"The meter {meter.MeterName} is not used by any subscription.");
                        }
                        else
                        {
                            _logger.LogInformation($"The events of meter {meter.MeterName} was lastly processed at {effectiveStartTime}. The current time is {DateTime.UtcNow}.");
                        }
                        
                        continue;
                    }

                    _logger.LogInformation($"Query and report meter event {meter.MeterName} starting {effectiveStartTime}.");

                    DateTime effectiveEndTime = effectiveStartTime.AddHours(1);

                    var telemetryDataConnector = await _telemetryDataConnectorService.GetAsync(meter.TelemetryDataConnectorName);

                    ITelemetryDataConnector connector = _telemetryConnectionManager.CreateTelemetryDataConnector(
                        telemetryDataConnector.Type,
                        telemetryDataConnector.Configuration);

                    IEnumerable<Usage> meterEvents = await connector.GetMeterEventsByHour(effectiveStartTime, meter.TelemetryQuery);

                    // Get the billable meter events
                    List<Usage> billableMeterEvents = new List<Usage>();

                    foreach (var meterEvent in meterEvents)
                    {
                        // Send the meter event only if:
                        // 1. Subscription exists
                        // 2. Subscription is in a plan using the current custom meter
                        // 3. Meter usage is enabled
                        // 4. The meter usage is not reported
                        Guid subscriptionId;
                        if (! Guid.TryParse(meterEvent.ResourceId, out subscriptionId))
                        {
                            _logger.LogWarning($"ResourceId {meterEvent.ResourceId} is not a valid subscription. The data type should be GUID.");
                            continue;
                        }

                        if (!await _subscriptionService.ExistsAsync(subscriptionId))
                        {
                            _logger.LogWarning($"The subscription {subscriptionId} doesn't exist. Will not report the meter event {meterEvent.Dimension}.");
                            continue;
                        }

                        var subscription = await _subscriptionService.GetAsync(subscriptionId);
                        var meterUsage = await _subscriptionCustomMeterUsageService.GetAsync(subscriptionId, meter.MeterName);

                        if (await _customMeterDimensionService.ExistsAsync(offer.OfferName, subscription.PlanName, meter.MeterName) &&
                            meterUsage.IsEnabled &&
                            meterUsage.LastUpdatedTime < effectiveEndTime)
                        {
                            meterEvent.Dimension = meter.MeterName;
                            meterEvent.PlanId = subscription.PlanName;
                            billableMeterEvents.Add(meterEvent);
                        }
                    }

                    CustomMeteringRequestResult requestResult = new CustomMeteringBatchSuccessResult();

                    if (billableMeterEvents.Count > 0)
                    {
                        requestResult = await _customMeteringClient.RecordBatchUsageAsync(
                           Guid.NewGuid(),
                           Guid.NewGuid(),
                           billableMeterEvents,
                           default);
                    }
                    else
                    {
                        // Create an empty result
                        ((CustomMeteringBatchSuccessResult)requestResult).Success = true;
                        ((CustomMeteringBatchSuccessResult)requestResult).Result = new List<CustomMeteringSuccessResult>();
                    }

                    if (requestResult.Success)
                    {
                        CustomMeteringBatchSuccessResult batchResult = (CustomMeteringBatchSuccessResult)requestResult;

                        foreach (var result in batchResult.Result)
                        {
                            var subscriptionId = Guid.Parse(result.ResourceId);
                            var subscriptionMeterUsage = await _subscriptionCustomMeterUsageService.GetAsync(subscriptionId, meter.MeterName);
                            if (result.Status.Equals(nameof(CustomMeterEventStatus.Accepted), StringComparison.InvariantCultureIgnoreCase) || 
                                result.Status.Equals(nameof(CustomMeterEventStatus.Duplicate), StringComparison.InvariantCultureIgnoreCase))
                            {
                                _logger.LogWarning($"Meter event {result.Dimension} for subscription {result.ResourceId} at {result.EffectiveStartTime} reported at {DateTime.Now}, with status {result.Status}.");
                                subscriptionMeterUsage.LastUpdatedTime = effectiveEndTime;

                                // Always record the reported meter event to Azure table if it is duplicate.
                                var tableEntity = new CustomMeteringAzureTableEntity(
                                    result.Status.Equals(nameof(CustomMeterEventStatus.Accepted), StringComparison.InvariantCultureIgnoreCase) ?
                                    result : result.Error.AdditionalInfo.AcceptedMessage);

                                await _storageUtility.InsertTableEntity(REPORTED_METER_EVENT_TABLE_NAME, tableEntity);

                                if (effectiveEndTime > subscriptionMeterUsage.UnsubscribedTime)
                                {
                                    subscriptionMeterUsage.IsEnabled = false;
                                    subscriptionMeterUsage.DisabledTime = effectiveEndTime;
                                }
                            }
                            else if (result.Status.Equals(nameof(CustomMeterEventStatus.Expired), StringComparison.InvariantCultureIgnoreCase))
                            {
                                // If the meter event is expired, record a warning and move on
                                _logger.LogWarning($"Meter event {result.Dimension} for subscription {result.ResourceId} at {result.EffectiveStartTime} expired at {DateTime.Now}.");
                                subscriptionMeterUsage.LastUpdatedTime = effectiveEndTime;

                                await _storageUtility.InsertTableEntity(EXPIRED_METER_EVENT_TABLE_NAME,
                                    new CustomMeteringAzureTableEntity(result));

                                if (effectiveEndTime > subscriptionMeterUsage.UnsubscribedTime)
                                {
                                    subscriptionMeterUsage.IsEnabled = false;
                                    subscriptionMeterUsage.DisabledTime = effectiveEndTime;
                                }
                            }
                            else
                            {
                                _logger.LogError($"Meter event {result.Dimension} for subscription {result.ResourceId} at {result.EffectiveStartTime} failed to report at {DateTime.Now}.");
                                subscriptionMeterUsage.LastErrorReportedTime = effectiveEndTime;
                                subscriptionMeterUsage.LastError = $"Meter event failed with error {result.Status}. Details: {ComposeErrorMessage(result.Error)}.";
                            }
                            await _subscriptionCustomMeterUsageService.UpdateAsync(subscriptionId, meter.MeterName, subscriptionMeterUsage);
                        }

                        await _subscriptionCustomMeterUsageService.UpdateLastUpdatedTimeForUnreportedSubscriptions(offer.OfferName, meter.MeterName, effectiveEndTime);

                        _logger.LogInformation($"Completed reporting custom meter events {meter.MeterName} starting {effectiveStartTime}.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send the batch meter event. Response code: {requestResult.Code}");
                    }
                }
            }
        }

        private string ComposeErrorMessage(CustomMeteringError error)
        {
            if (error == null)
            {
                return "unknown";
            }

            StringBuilder errorMsg = new StringBuilder();
            errorMsg.Append($"Code: {error.Code}, Message: {error.Message}, Target: {error.Target}.");

            if (error.Details != null && error.Details.Count() > 0)
            {
                errorMsg.Append("Details: ");
                foreach(var detail in error.Details)
                {
                    errorMsg.Append($"Code: {detail.Code}, Message: {detail.Message}, Target: {detail.Target};");
                }
            }

            return errorMsg.ToString();
        }
    }
}
