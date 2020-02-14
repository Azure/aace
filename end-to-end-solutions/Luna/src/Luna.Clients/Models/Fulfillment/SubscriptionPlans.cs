using System.Collections.Generic;

namespace Luna.Clients.Models.Fulfillment
{
    /// <summary>
    /// Response payload for listing available plans
    /// </summary>
    public class SubscriptionPlan
    {
        public string DisplayName { get; set; }
        public bool IsPrivate { get; set; }
        public string PlanId { get; set; }
    }

    public class SubscriptionPlans : FulfillmentRequestResult
    {
        public IEnumerable<SubscriptionPlan> Plans { get; set; }
    }
}
