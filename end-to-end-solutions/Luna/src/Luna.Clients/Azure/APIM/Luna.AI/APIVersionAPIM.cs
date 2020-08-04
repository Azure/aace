using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM
{
    public class APIVersionAPIM : IAPIVersionAPIM
    {
        private const string REQUEST_BASE_URL_FORMAT = "https://{0}.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}";
        private string APIM_PATH_FORMAT = "{0}/{1}";
        private string CONTROLLER_PATH_FORMAT = "/api/products/{0}/deployments/{1}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _apiVersion;
        private APIMAuthHelper _apimAuthHelper;
        private HttpClient _httpClient;
        private IAPIVersionSetAPIM _apiVersionSetAPIM;

        private string _requestBaseUrl;
        private string _controllerBaseUrl;

        [ActivatorUtilitiesConstructor]
        public APIVersionAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                           HttpClient httpClient,
                           IAPIVersionSetAPIM apiVersionSetAPIM,
                           IKeyVaultHelper keyVaultHelper)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _subscriptionId = options.CurrentValue.Config.SubscriptionId;
            _resourceGroupName = options.CurrentValue.Config.ResourceGroupname;
            _apimServiceName = options.CurrentValue.Config.APIMServiceName;
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _requestBaseUrl = string.Format(REQUEST_BASE_URL_FORMAT, _apimServiceName);
            _controllerBaseUrl = options.CurrentValue.Config.ControllerBaseUrl;
            _apimAuthHelper = new APIMAuthHelper(options.CurrentValue.Config.UId, keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Key).Result);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiVersionSetAPIM = apiVersionSetAPIM;
        }

        private Uri GetAPIVersionAPIMRequestURI(string productName, string deploymentName, string versionName, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetAPIMRESTAPIPath(productName, deploymentName, versionName));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        private Uri GetOriginAPIVersionAPIMRequestURI(string productName, string deploymentName, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetOriginAPIMRESTAPIPath(productName, deploymentName));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        private Models.Azure.APIVersion GetAPIVersion(APIVersion version)
        {
            Models.Azure.APIVersion api = new Models.Azure.APIVersion();
            api.name = version.ProductName + version.DeploymentName + version.GetVersionIdFormat();
            api.properties.displayName = version.ProductName + version.DeploymentName + version.GetVersionIdFormat();
            api.properties.apiVersion = version.VersionName;

            api.properties.serviceUrl = _controllerBaseUrl + GetControllerPath(version.ProductName, version.DeploymentName);
            api.properties.path = GetAPIMPath(version.ProductName, version.DeploymentName);
            api.properties.apiVersionSetId = _apiVersionSetAPIM.GetAPIMRESTAPIPath(version.ProductName, version.DeploymentName);

            return api;
        }

        private Models.Azure.APIVersion GetOriginAPIVersion(Deployment deployment)
        {
            Models.Azure.APIVersion api = new Models.Azure.APIVersion();
            api.name = deployment.ProductName + deployment.DeploymentName;
            api.properties.displayName = deployment.ProductName + deployment.DeploymentName;
            api.properties.apiVersion = deployment.ProductName + deployment.DeploymentName;

            api.properties.serviceUrl = "";
            api.properties.path = deployment.ProductName + deployment.DeploymentName;
            api.properties.apiVersionSetId = _apiVersionSetAPIM.GetAPIMRESTAPIPath(deployment.ProductName, deployment.DeploymentName);

            return api;
        }

        public string GetAPIMPath(string productName, string deploymentName)
        {
            return string.Format(APIM_PATH_FORMAT, productName, deploymentName);
        }

        public string GetControllerBaseUrl()
        {
            return _controllerBaseUrl;
        }

        public string GetControllerPath(string productName, string deploymentName)
        {
            return string.Format(CONTROLLER_PATH_FORMAT, productName, deploymentName);
        }

        public string GetOriginAPIMRESTAPIPath(string productName, string deploymentName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName + deploymentName);
        }

        public string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName + deploymentName + versionName);
        }

        public async Task<bool> ExistsAsync(APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetAPIVersion(version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return false;

            Models.Azure.APIVersion apiVersionAPIM = (Models.Azure.APIVersion)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.APIVersion));
            if (apiVersionAPIM == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            return true;
        }

        public async Task CreateAsync(APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            var body = JsonConvert.SerializeObject(GetAPIVersion(version));
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetAPIVersion(version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(APIVersion version)
        {
            if (!(await ExistsAsync(version))) return;

            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetAPIVersion(version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task CreateAsync(Deployment deployment)
        {
            Uri requestUri = GetOriginAPIVersionAPIMRequestURI(deployment.ProductName, deployment.DeploymentName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetOriginAPIVersion(deployment)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(Deployment deployment)
        {
            Uri requestUri = GetOriginAPIVersionAPIMRequestURI(deployment.ProductName, deployment.DeploymentName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetOriginAPIVersion(deployment)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(Deployment deployment)
        {
            Uri requestUri = GetOriginAPIVersionAPIMRequestURI(deployment.ProductName, deployment.DeploymentName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetOriginAPIVersion(deployment)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
