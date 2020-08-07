// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class GetAllTrainingOperationsByModelIdAndVerifyUserResponse
    {
        public string status { get; set; }
        public string startTimeUtc { get; set; }
        public string endTimeUtc { get; set; }
        public String description { get; set; }
        public Object error { get; set; }
        public Tags tags { get; set; }
        public class Tags
        {
            public string modelId { get; set; }
        }
        public GetAllTrainingOperationsByModelIdAndVerifyUserResponse()
        {

        }
    }
}
