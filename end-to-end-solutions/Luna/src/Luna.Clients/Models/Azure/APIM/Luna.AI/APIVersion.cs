// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public class APIVersion
    {
        public string name { get; set; }
        public Properties properties { get; set; }
        public class Properties
        {
            public string displayName { get; set; }
            public string apiVersion { get; set; }
            public string apiVersionSetId { get; set; }
            public string serviceUrl { get; set; }
            public string path { get; set; }
            public List<string> protocols { get; set; }
        }
        public APIVersion()
        {
            this.properties = new Properties();
            this.properties.serviceUrl = "https://luna-dev-controller.azurewebsites.net";
            this.properties.path = "";
            this.properties.protocols = new List<string>(new string[]
            {
                "https"
            });
        }
    }
}
