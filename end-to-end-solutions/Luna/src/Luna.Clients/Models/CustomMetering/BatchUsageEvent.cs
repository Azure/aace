// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.CustomMetering
{
    public class BatchUsageEvent
    {
        public BatchUsageEvent(IEnumerable<Usage> usage)
        {
            request = usage;
        }

        public IEnumerable<Usage> request { get; set; }
    }
}
