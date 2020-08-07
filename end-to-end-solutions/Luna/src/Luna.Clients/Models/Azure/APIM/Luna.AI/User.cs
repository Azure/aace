// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace Luna.Clients.Models.Azure
{
    public class User
    {
        public string name { get; set; }
        public Properties properties { get; set; }
        public class Properties
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string confirmation { get; set; }
        }
        public User()
        {
            this.properties = new Properties();
            this.properties.confirmation = "signup";
        }
    }
}
