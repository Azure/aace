// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public class Operation
    {
        public string name { get; set; }
        public Properties properties { get; set; }
        public class Properties
        {
            public string displayName { get; set; }
            public string method { get; set; }
            public string urlTemplate { get; set; }
            public List<templateParameter> templateParameters { get; set; }
        }
        public class templateParameter
        {
            public string name { get; set; }
        }
        public Operation()
        {
            this.properties = new Properties()
            {
                templateParameters = new List<templateParameter>()
            };
        }
    }
}
