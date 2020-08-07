// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.TelemetryDataConnectors
{
    public class CustomMeterEvent
    {
        public Guid resourceId { get; set; }
        public double quantity { get; set; }
        public string dimension { get; set; }
        public string effectiveStartTime { get; set; }
        public string planId { get; set; }
    }
}
