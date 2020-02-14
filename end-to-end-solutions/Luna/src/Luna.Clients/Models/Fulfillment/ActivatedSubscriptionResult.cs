namespace Luna.Clients.Models.Fulfillment
{
    /// <summary>
    /// The response payload for activating a subscription
    /// </summary>
    public class ActivatedSubscriptionResult : FulfillmentRequestResult
    {
        public string PlanId { get; set; }
        public string Quantity { get; set; }
    }
}
