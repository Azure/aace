// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class GetRealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse
    {
        public string name { get; set; }
        public string createdTime { get; set; }
        public string updatedTime { get; set; }
        public string scoringUri { get; set; }
        public String description { get; set; }
        public GetRealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse()
        {

        }
    }
}
