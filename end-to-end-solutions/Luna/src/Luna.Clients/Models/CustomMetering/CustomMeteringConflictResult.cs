// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace Luna.Clients.Models.CustomMetering
{
    public class AdditionalInfo
    {
        public string Dimension { get; set; }
        public string EffectiveStartTime { get; set; }
        public string MessageTime { get; set; }
        public string PlanId { get; set; }
        public double Quantity { get; set; }
        public string ResourceId { get; set; }
        public string Status { get; set; }
        public string UsageEventId { get; set; }
    }

    public class CustomMeteringConflictResult : CustomMeteringRequestResult
    {
        public AdditionalInfo AdditionalInfo { get; set; }
    }
}
