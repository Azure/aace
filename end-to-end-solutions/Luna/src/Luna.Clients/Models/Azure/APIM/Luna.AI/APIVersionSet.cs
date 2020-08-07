// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace Luna.Clients.Models.Azure
{
    public class APIVersionSet
    {
        public string name { get; set; }
        public Properties properties { get; set; }
        public class Properties
        {
            public string displayName { get; set; }
            public string versioningScheme { get; set; }
            public string versionQueryName { get; set; }
        }

        public APIVersionSet()
        {
            this.properties = new Properties();
            this.properties.versioningScheme = "Query";
            this.properties.versionQueryName = "api-version";
        }
    }
}
