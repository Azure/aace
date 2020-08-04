using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Luna.Clients.Azure.Auth
{
    public class KeyVaultHelper : IKeyVaultHelper
    {   
        private readonly KeyVaultClient keyVaultClient;
        private const string authenticationEndpoint = "https://login.microsoftonline.com/";
        private readonly ILogger<KeyVaultHelper> _logger;

        public KeyVaultHelper(ILogger<KeyVaultHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback
                )
            );
        }

        public async Task<string> SetSecretAsync(string vaultName, string secretName, string value)
        {
            try
            {
                _logger.LogInformation("SetSecretAsync to Key Vault");
                var secret = await keyVaultClient.SetSecretAsync($"https://{vaultName}.vault.azure.net/", secretName, value);
                _logger.LogInformation("SetSecretAsync operation completed");
                return secret.Value;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<string> GetSecretAsync(string vaultName, string secretName)
        { 
            try {
                _logger.LogInformation("GetSecretAsync from Key Vault");
                var secret = await keyVaultClient.GetSecretAsync($"https://{vaultName}.vault.azure.net/", secretName);
                _logger.LogInformation("GetSecretAsync operation completed");
                return secret.Value;
            } catch (Exception ex) {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<string> DeleteSecretAsync(string vaultName, string secretName)
        {
            try
            {
                _logger.LogInformation("DeleteSecretAsync from Key Vault");
                var secret = await keyVaultClient.DeleteSecretAsync($"https://{vaultName}.vault.azure.net/", secretName);
                _logger.LogInformation("DeleteSecretAsync operation completed");
                return secret.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<JsonWebKey> GetKeyAsync(string vaultName, string keyName)
        {
            try {
                _logger.LogInformation("GetKeyAsync from KeyVault");
                var key = await keyVaultClient.GetKeyAsync($"https://{vaultName}.vault.azure.net/", keyName);
                _logger.LogInformation("GetKeyAsync operation completed");
                return key.Key;
            } 
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        /// <summary>
        /// Get the bearer token for an AD application.
        /// </summary>
        /// <remarks>Please see https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-registration#get-a-token-based-on-the-azure-ad-app.</remarks>
        /// <param name="options">Authentication configuration</param>
        /// <param name="resource">Authentication resource</param>
        /// <returns></returns>
        public async Task<string> GetBearerToken(AuthenticationConfiguration options, string resource)
        {
            try {
                _logger.LogInformation("GetBearerToken for authentication");
                var key = await GetSecretAsync(options.VaultName, options.AppKey);
                var credential = new ClientCredential(options.ClientId.ToString(), key);
                var authContext = new AuthenticationContext(authenticationEndpoint + options.TenantId, false);
                var token = await authContext.AcquireTokenAsync(resource, credential);
                _logger.LogInformation("GetBearerToken operation complete");
                return token.AccessToken;
            } catch (Exception ex) {
                throw new InvalidOperationException(ex.Message);
            }            
        }

    }
}
