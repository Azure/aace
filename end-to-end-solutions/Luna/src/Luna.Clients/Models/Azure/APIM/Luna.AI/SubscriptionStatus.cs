// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public static class SubscriptionStatus
    {
        private static IDictionary<string, string> stateMap = new Dictionary<string, string>
        {
            {"Subscribed", "active"},
            {"Suspended", "suspended"}
        };

        public static string GetState(string status)
        {
            if(!stateMap.ContainsKey(status))
                throw new ArgumentException("The controller type haven't support yet.");
            return stateMap[status];
        }
    }
}
