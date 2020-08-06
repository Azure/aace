// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class Endpoint
    {
        public string id { get; set; }
        public string scoringUri { get; set; }
        public string sslKey { get; set; }
        public String description { get; set; }
    }
}
