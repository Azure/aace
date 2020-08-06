// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Threading;
using System.Threading.Tasks;

namespace Luna.Services.WebHook
{    
    public interface IWebhookProcessor
    {
        Task ProcessWebhookNotificationAsync(WebhookPayload details, CancellationToken cancellationToken = default);
    }
}
