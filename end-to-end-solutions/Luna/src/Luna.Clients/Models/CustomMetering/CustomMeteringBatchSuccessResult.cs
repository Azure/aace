// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Luna.Clients.Models.CustomMetering
{
    public class CustomMeteringBatchSuccessResult : CustomMeteringRequestResult
    {
        public int Count { get; set; }

        public IEnumerable<CustomMeteringSuccessResult> Result { get; set; }
    }
}
