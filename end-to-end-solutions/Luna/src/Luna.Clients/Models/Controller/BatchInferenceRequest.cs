// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;

namespace Luna.Clients.Models.Controller
{
    public class BatchInferenceRequest
    {
        public string userId { get; set; }
        public Guid subscriptionId { get; set; }
        public string userInput { get; set; }
    }
}
