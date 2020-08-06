// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;

namespace Luna.Clients.Models.Fulfillment
{
    /// <summary>
    /// The response payload for subscription operations
    /// </summary>
    public class SubscriptionOperation : FulfillmentRequestResult
    {
        public string Action { get; set; }
        public Guid ActivityId { get; set; }
        public Guid Id { get; set; }
        public string OfferId { get; set; }
        public string OperationRequestSource { get; set; }
        public string PlanId { get; set; }
        public string PublisherId { get; set; }
        public string Quantity { get; set; }
        public Uri ResourceLocation { get; set; }
        public OperationStatusEnum Status { get; set; }
        public Guid SubscriptionId { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class SubscriptionOperationResult : FulfillmentRequestResult
    {
        public string ContinuationToken { get; set; }
        public IEnumerable<SubscriptionOperation> Operations { get; set; }
    }
}
