// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.WebKey;

namespace Luna.Clients.Azure.Auth
{
    public interface IKeyVaultHelper
    {
        Task<string> SetSecretAsync(string vaultName, string secretName, string value);
        Task<string> GetSecretAsync(string vaultName, string secretName);
        Task<string> DeleteSecretAsync(string vaultName, string secretName);

        Task<JsonWebKey> GetKeyAsync(string vaultName, string keyName);
    
        /// <summary>
        /// Get the bearer token for an AD application.
        /// </summary>
        /// <remarks>Please see https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-registration#get-a-token-based-on-the-azure-ad-app.</remarks>
        /// <param name="options">Authentication configuration</param>
        /// <param name="resource">Authentication resource</param>
        /// <returns></returns>
        Task<string> GetBearerToken(AuthenticationConfiguration options, string resource);
    }
}
