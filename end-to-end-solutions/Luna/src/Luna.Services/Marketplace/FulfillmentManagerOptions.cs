// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using Luna.Clients;
using Luna.Clients.Azure.Auth;

namespace Luna.Services.Marketplace
{
    public class FulfillmentManagerOptions
    {
        public AuthenticationConfiguration AzureActiveDirectory { get; set; }

        public ClientConfiguration FulfillmentService { get; set; }
    }
}