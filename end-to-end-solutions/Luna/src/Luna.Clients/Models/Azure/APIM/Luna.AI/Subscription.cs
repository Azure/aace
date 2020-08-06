// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public class APISubscription
    {
        public string name { get; set; }
        public Properties properties { get; set; }
        public class Properties
        {
            public string ownerId { get; set; }
            public string scope { get; set; }
            public string displayName { get; set; }
            public string state { get; set; }

            public string primaryKey { get; set; }
            public string secondaryKey { get; set; }
        }
        public APISubscription()
        {
            this.properties = new Properties();
        }
    }
}

