// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public class Product
    {
        public string name { get; set; }
        public Properties properties { get; set; }
        public class Properties
        {
            public string displayName { get; set; }
            public string state { get; set; }
        }
        public Product()
        {
            this.properties = new Properties();
            this.properties.state = "Published";
        }
    }
}
