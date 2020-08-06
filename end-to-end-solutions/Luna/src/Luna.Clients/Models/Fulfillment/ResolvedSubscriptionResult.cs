// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using Newtonsoft.Json;

namespace Luna.Clients.Models.Fulfillment
{
    /// <summary>
    /// Subscription obtained by resolving a marketplace token to a persistent resource ID 
    /// </summary>
    public class ResolvedSubscriptionResult : FulfillmentRequestResult
    {
        [JsonProperty("id")]
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// A friendly name provided for this resource (e.g. "Contoso Cloud Solution")
        /// </summary>
        /// <value></value>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Offer id
        /// </summary>
        /// <value></value>
        public string OfferId { get; set; }

        public Guid OperationId { get; set; }

        /// <summary>
        /// Plan id
        /// </summary>
        /// <value></value>
        public string PlanId { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        /// <value></value>
        public int Quantity { get; set; }

        


    }
}
