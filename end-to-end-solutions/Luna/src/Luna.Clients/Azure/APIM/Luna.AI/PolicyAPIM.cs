using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM.Luna.AI
{
    public class PolicyAPIM : IPolicyAPIM
    {
        private const string REQUEST_BASE_URL_FORMAT = "https://{0}.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}/operations/{4}/policies/policy";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _token;
        private string _apiVersion;
        private APIMAuthHelper _apimAuthHelper;
        private HttpClient _httpClient;

        private string _requestBaseUrl;

        [ActivatorUtilitiesConstructor]
        public PolicyAPIM(IOptionsMonitor<APIMConfigurationOption> options,
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
            _token = keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Token).Result;
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _requestBaseUrl = string.Format(REQUEST_BASE_URL_FORMAT, _apimServiceName);
            _apimAuthHelper = new APIMAuthHelper(options.CurrentValue.Config.UId, keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Key).Result);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private Uri GetPolicyAPIMRequestURI(string type, string productName, string deploymentName, string versionName, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetAPIMRESTAPIPath(type, productName, deploymentName, versionName));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        private Models.Azure.Policy GetPolicy()
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />
                        <set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-variable name = ""subscriptionId"" value =""@(context.Subscription.Id)"" />
                        <set-variable name = ""userId"" value =""@(context.User.Id)"" />
                        <set-variable name = ""input"" value=""@(context.Request.Body.As&lt;string&gt;())"" />
                        <set-body template = ""liquid"" >
                        {
                            ""subscriptionId"": ""{{context.Variables[""subscriptionId""]}}"",
                            ""userId"": ""{{context.Variables[""userId""]}}"",
                            ""input"": {{context.Variables[""input""]}}
                        }
                        </set-body>
                    </inbound>
                    <backend>
                        <base />
                    </backend>
                    <outbound>
                        <base />
                    </outbound>
                    <on-error>
                        <base />
                    </on-error>
                </policies>";
            return policy;
        }

        public string GetAPIMRESTAPIPath(string type, string productName, string deploymentName, string versionName)
        {
            IController controller = ControllerHelper.GetController(type);
            var operationName = controller.GetName();
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName + deploymentName + versionName, operationName);
        }

        public async Task<bool> ExistsAsync(string type, APIVersion version)
        {
            Uri requestUri = GetPolicyAPIMRequestURI(type, version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy()), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return false;

            Models.Azure.Operation operationAPIM = (Models.Azure.Operation)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.Operation));
            if (operationAPIM == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            return true;
        }

        public async Task CreateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetPolicyAPIMRequestURI(type, version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy()), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetPolicyAPIMRequestURI(type, version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy()), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(string type, APIVersion version)
        {
            if (!(await ExistsAsync(type, version))) return;

            Uri requestUri = GetPolicyAPIMRequestURI(type, version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy()), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
