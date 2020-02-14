using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Luna.Clients.Models.Fulfillment
{
    /// <summary>
    /// Response payload for getting subscriptions
    /// </summary>
    public class SubscriptionResults : FulfillmentRequestResult
    {
        public string ContinuationToken { get; set; }
        public IEnumerable<SubscriptionResult> Subscriptions { get; set; }
    }
    
    public class SubscriptionResult : FulfillmentRequestResult
    {
        /// <summary>
        /// Possible Values: Read, Update, Delete
        /// </summary>
        /// <value></value>
        public IEnumerable<AllowedCustomerOperationEnum> AllowedCustomerOperations { get; set; }

        /// <summary>
        /// Tenant for which SaaS subscription is purchased
        /// </summary>
        /// <value></value>
        public Beneficiary Beneficiary { get; set; }

        /// <summary>
        /// true – the customer subscription is currently in free trial, false – the customer subscription is not currently in free trial.
        /// </summary>
        public bool IsFreeTrial { get; set; }

        /// <summary>
        /// A friendly name provided for this resource (e.g. "Contoso Cloud Solution")
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Offer id
        /// </summary>
        /// <value></value>
        public string OfferId { get; set; }

        /// <summary>
        /// Plan id
        /// </summary>
        /// <value></value>
        public string PlanId { get; set; }

        /// <summary>
        /// Publisher id (e.g. "contoso")
        /// </summary>
        /// <value></value>
        public string PublisherId { get; set; }

        /// <summary>
        /// Tenant that purchased the SaaS subscription
        /// </summary>
        /// <value></value>
        public Purchaser Purchaser { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        /// <value></value>
        public int Quantity { get; set; }

        /// <summary>
        /// Indicates the status of the operation: [NotStarted, PendingFulfillmentStart, Subscribed, Suspended, Unsubscribed]
        /// </summary>
        /// <value></value>
        public StatusEnum SaasSubscriptionStatus { get; set; }
        
        /// <summary>
        /// Possible Values: None, DryRun
        /// </summary>
        /// <value></value>
        public SessionModeEnum SessionMode { get; set; }

        [JsonProperty("id")]
        public Guid SubscriptionId { get; set; }
    }

    public enum AllowedCustomerOperationEnum
    {
        Read,
        Update,
        Delete
    }

    public enum SessionModeEnum
    {
        None,
        DryRun
    }

    public enum StatusEnum
    {
        Provisioning,
        Subscribed,
        Suspended,
        Unsubscribed,
        NotStarted,
        PendingFulfillmentStart
    }

    public class Beneficiary
    {
        public Guid TenantId { get; set; }
    }

    public class Purchaser
    {
        public Guid TenantId { get; set; }
    }
}
