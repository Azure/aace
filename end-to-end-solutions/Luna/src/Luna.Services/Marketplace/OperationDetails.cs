// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;

namespace Luna.Services.Marketplace
{
    public class OperationDetails
    {
        public Guid OperationId { get; set; }

        public TimeSpan RetryInterval { get; set; }

        public Guid SubscriptionId { get; set; }
    }
}