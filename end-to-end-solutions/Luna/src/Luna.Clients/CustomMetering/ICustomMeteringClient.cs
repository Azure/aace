// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Models.CustomMetering;

namespace Luna.Clients.CustomMetering
{
    public interface ICustomMeteringClient
    {
        Task<CustomMeteringRequestResult> RecordBatchUsageAsync(
            Guid requestId,
            Guid correlationId,
            IEnumerable<Usage> usage,
            CancellationToken cancellationToken);

        Task<CustomMeteringRequestResult> RecordUsageAsync(
                    Guid requestId,
            Guid correlationId,
            Usage usage,
            CancellationToken cancellationToken);
    }
}
