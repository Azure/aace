// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using Newtonsoft.Json;

namespace Luna.Clients.Models.Fulfillment
{
    public enum ActionEnum
    {
        Activate,
        Delete,
        Suspend,
        Reinstate,
        Change
    }

    public class WebhookContent
    {
        public ActionEnum Action { get; set; }
        public string ActivityId { get; set; }
        public string OfferId { get; set; }
        public string OperationId { get; set; }
        public string PlanId { get; set; }
        public string PublisherId { get; set; }
        public string Quantity { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime TimeStamp { get; set; }

        public static WebhookContent Parse(string content)
        {
            return JsonConvert.DeserializeObject<WebhookContent>(content);
        }
    }
}
