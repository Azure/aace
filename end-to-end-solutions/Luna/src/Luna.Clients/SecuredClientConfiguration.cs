// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Luna.Clients.Azure.Auth;

namespace Luna.Clients
{
    public class SecuredClientConfiguration
    {
        public AuthenticationConfiguration AzureActiveDirectory { get; set; }

        public ClientConfiguration ClientService { get; set; }
    }

    public class ClientConfiguration
    {
        public string ApiVersion { get; set; }
        public string BaseUri { get; set; }
        public string AuthenticationResourceId { get; set; }
    }

    public class SecuredFulfillmentClientConfiguration : SecuredClientConfiguration 
    {

    }

    public class SecuredProvisioningClientConfiguration : SecuredClientConfiguration
    {
        
    }
}