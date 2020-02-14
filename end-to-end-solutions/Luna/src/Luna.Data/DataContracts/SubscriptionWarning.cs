using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts
{
    public class SubscriptionWarning
    {
        public Guid SubscriptionId { get; set; }

        public string WarningMessage { get; set; }

        public string Details { get; set; }

        public SubscriptionWarning(Guid subscriptionId, string warningMessage, string details)
        {
            this.SubscriptionId = subscriptionId;
            this.WarningMessage = warningMessage;
            this.Details = details;
        }
    }
}
