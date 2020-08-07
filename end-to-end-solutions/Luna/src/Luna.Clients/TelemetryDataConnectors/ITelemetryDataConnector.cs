// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Luna.Clients.Models.CustomMetering;

namespace Luna.Clients.TelemetryDataConnectors
{
    public interface ITelemetryDataConnector
    {
        Task<IEnumerable<Usage>> GetMeterEventsByHour(DateTime startTime, string query);

        Task<Usage> GetMeterEventByHourBySubscription(Guid subscriptionId, DateTime startTime, string query);
    }
}
