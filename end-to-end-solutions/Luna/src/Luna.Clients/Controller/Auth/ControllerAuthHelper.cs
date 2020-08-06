// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Luna.Clients.Controller.Auth
{
    public class ControllerAuthHelper
    {
        public async static Task<string> GetToken(string tenantId, string clientId, string clientSecret)
        {
            string resourceUri = "https://management.azure.com/";
            string authorityUri = String.Format("https://login.microsoftonline.com/{0}", tenantId);

            AuthenticationContext authenticationContext = new AuthenticationContext(authorityUri, new TokenCache());
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

            return (await authenticationContext.AcquireTokenAsync(resourceUri, clientCredential)).AccessToken;
        }
    }
}
