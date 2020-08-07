// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace Luna.Clients.Models.CustomMetering
{
    public class Usage
    {
        public string Dimension { get; set; }

        public string EffectiveStartTime { get; set; }

        public string PlanId { get; set; }

        public double Quantity { get; set; }

        public string ResourceId { get; set; }
    }
}
