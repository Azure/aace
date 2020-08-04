using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;
using Luna.Clients.Azure.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Luna.Clients.Exceptions;
using System.Threading.Tasks;

namespace Luna.Clients.Azure.APIM
{
    public class ClientCertAPIM : IClientCertAPIM
    {
        private const string REQUEST_BASE_URL_FORMAT = "https://{0}.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/certificates/testCert";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _requestBaseUrl;
        private APIMAuthHelper _apimAuthHelper;
        private HttpClient _httpClient;
        private string _apiVersion;

        [ActivatorUtilitiesConstructor]
        public ClientCertAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                                    HttpClient httpClient,
                                    IKeyVaultHelper keyVaultHelper)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _subscriptionId = options.CurrentValue.Config.SubscriptionId;
            _resourceGroupName = options.CurrentValue.Config.ResourceGroupname;
            _apimServiceName = options.CurrentValue.Config.APIMServiceName;
            _requestBaseUrl = string.Format(REQUEST_BASE_URL_FORMAT, _apimServiceName);
            _apimAuthHelper = new APIMAuthHelper(options.CurrentValue.Config.UId, keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Key).Result);
            _httpClient = httpClient ?? throw new ArgumentException(nameof(httpClient));
            _apiVersion = options.CurrentValue.Config.APIVersion;
        }


        public string GetAPIMRESTAPIPath(string owner)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName);
        }

        private Uri GetCertificateAPIMRequestURI(string owner, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetAPIMRESTAPIPath(owner));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();
            builder.Query = queryString;

            return new Uri(builder.ToString());
        }


        public async Task<ClientCertConfiguration> GetCert(string owner)
        {
            Uri requestUri = GetCertificateAPIMRequestURI(owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(owner), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            ClientCertConfiguration cert = JsonConvert.DeserializeObject<ClientCertConfiguration>(responseContent);

            return cert;
        }
    }
}
