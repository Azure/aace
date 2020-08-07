// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class ListAllTrainingOperationsByUserResponse
    {
        public List<Operation> operations { get; set; }
        public class Operation
        {
            public string operationType { get; set; }
            public string modelId { get; set; }
            public string status { get; set; }
            public string startTimeUtc { get; set; }
            public string completeTimeUtc { get; set; }
            public String description { get; set; }
            public object error { get; set; }
        }
        public ListAllTrainingOperationsByUserResponse()
        {
            this.operations = new List<Operation>();
        }
    }
}
