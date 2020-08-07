// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;

namespace Luna.Services.WebHook
{
    public class WebhookPayload
    {
        public string Action { get; set; }

        public Guid ActivityId { get; set; }

        public string OfferId { get; set; }

        public Guid Id { get; set; }

        public string PlanId { get; set; }
        public string PublisherId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public Guid SubscriptionId { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
