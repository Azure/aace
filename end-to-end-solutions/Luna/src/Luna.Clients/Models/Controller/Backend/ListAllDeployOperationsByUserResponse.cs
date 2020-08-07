// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class ListAllDeployOperationsByUserResponse
    {
        public string status { get; set; }
        public string startTimeUtc { get; set; }
        public string endTimeUtc { get; set; }
        public Object error { get; set; }
        public Tags tags { get; set; }
        public class Tags
        {
            public string operationType { get; set; }
            public string endpointId { get; set; }
        }
        public ListAllDeployOperationsByUserResponse()
        {

        }
    }
}
