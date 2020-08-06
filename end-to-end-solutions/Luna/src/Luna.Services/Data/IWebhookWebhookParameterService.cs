// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using Luna.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public interface IWebhookWebhookParameterService
    {
        Task CreateJoinEntryAsync(long webhookId, long webhookParameterId);
        Task DeleteWebhookJoinEntriesAsync(long webhookId);
        Task<bool> ParameterExistsInDifferentWebhooks(long webhookId, long webhookParameterId);
        Task<List<WebhookWebhookParameter>> GetAllJoinEntries(long webhookId);
    }
}