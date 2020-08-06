// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;

namespace Luna.Clients.Azure.Auth
{
    public class AuthenticationConfiguration
    {
        public string VaultName { get; set; }
        public string AppKey { get; set; }
        public Guid ClientId { get; set; }
        public Guid TenantId { get; set; }
    }
}