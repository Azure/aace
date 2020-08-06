// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class GetABatchInferenceOperationResponse
    {
        public string operationType { get; set; }
        public string operationId { get; set; }
        public string status { get; set; }
        public string startTimeUtc { get; set; }
        public string completeTimeUtc { get; set; }
        public String description { get; set; }
        public Object error { get; set; }
    }
}
