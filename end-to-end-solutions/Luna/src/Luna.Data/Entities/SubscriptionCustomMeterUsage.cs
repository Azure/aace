using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class SubscriptionCustomMeterUsage
    {
        public SubscriptionCustomMeterUsage(long meterId, Guid subscriptionId, bool isEnabled = false)
        {
            DateTime currentTime = DateTime.UtcNow;
            MeterId = meterId;
            SubscriptionId = subscriptionId;
            CreatedTime = currentTime;
            LastUpdatedTime = currentTime;
            LastErrorReportedTime = DateTime.MinValue;
            DisabledTime = DateTime.MinValue;
            EnabledTime = DateTime.MinValue;
            UnsubscribedTime = DateTime.MaxValue;
            IsEnabled = isEnabled;
        }

        public void Copy(SubscriptionCustomMeterUsage subscriptionCustomMeterUsage)
        {
            this.Id = subscriptionCustomMeterUsage.Id;
            this.MeterId = subscriptionCustomMeterUsage.MeterId;
            this.SubscriptionId = subscriptionCustomMeterUsage.SubscriptionId;
            this.CreatedTime = subscriptionCustomMeterUsage.CreatedTime;
            this.LastUpdatedTime = subscriptionCustomMeterUsage.LastUpdatedTime;
            this.LastErrorReportedTime = subscriptionCustomMeterUsage.LastErrorReportedTime;
            this.LastError = subscriptionCustomMeterUsage.LastError;
            this.IsEnabled = subscriptionCustomMeterUsage.IsEnabled;
            this.DisabledTime = subscriptionCustomMeterUsage.DisabledTime;
            this.UnsubscribedTime = subscriptionCustomMeterUsage.UnsubscribedTime;
        }

        [JsonIgnore]
        public long Id { get; set; }

        public long MeterId { get; set; }

        public Guid SubscriptionId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime LastErrorReportedTime { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime EnabledTime { get; set; }

        public DateTime DisabledTime { get; set; }

        public DateTime UnsubscribedTime { get; set; }

        public string LastError { get; set; }
    }
}
