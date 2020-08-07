// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace Luna.Clients.Models.CustomMetering
{
    public class CustomMeteringAzureTableEntity: TableEntity
    {
        public CustomMeteringAzureTableEntity(CustomMeteringSuccessResult result)
        {
            this.Dimension = result.Dimension ?? "unknown";
            this.EffectiveStartTime = result.EffectiveStartTime ?? "unknown";
            this.PlanId = result.PlanId ?? "unknown";
            this.Quantity = result.Quantity;
            this.ResourceId = result.ResourceId ?? "unknown";
            this.Status = result.Status ?? "unknown";
            this.UsageEventId = result.UsageEventId ?? Guid.Empty.ToString();

            PartitionKey = ResourceId;
            RowKey = Dimension + "-" + EffectiveStartTime;
        }

        public string Dimension { get; set; }

        public string EffectiveStartTime { get; set; }

        public string PlanId { get; set; }

        public double Quantity { get; set; }

        public string ResourceId { get; set; }

        public string Status { get; set; }

        public string UsageEventId { get; set; }

    }
}
