// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class GetAllTrainingOperationsByModelIdAndVerifyUserResponse
    {
        public List<Operation> operations { get; set; }
        public class Operation
        {
            public string modelId { get; set; }
            public string status { get; set; }
            public string startTimeUtc { get; set; }
            public string completeTimeUtc { get; set; }
            public String description { get; set; }
            public object error { get; set; }
        }
        public GetAllTrainingOperationsByModelIdAndVerifyUserResponse()
        {
            this.operations = new List<Operation>();
        }
    }
}
