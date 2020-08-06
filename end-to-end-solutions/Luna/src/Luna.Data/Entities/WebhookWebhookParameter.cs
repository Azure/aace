// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the WebhookWebhookParameters table in the db.
    /// </summary>
    public partial class WebhookWebhookParameter
    {
        public long WebhookId { get; set; }
        public Webhook Webhook { get; set; }

        public long WebhookParameterId { get; set; }
        public WebhookParameter WebhookParameter { get; set; }
    }
}
