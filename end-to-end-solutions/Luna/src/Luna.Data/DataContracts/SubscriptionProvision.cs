using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts
{
    public class SubscriptionProvision
    {
        public Guid SubscriptionId { get; set; }

        public string ProvisioningStatus { get; set; }

        public string ProvisioningType { get; set; }

        public string SubscriptionStatus { get; set; }

        public string LastException { get; set; }

        public int RetryCount { get; set; } 
    }
}
