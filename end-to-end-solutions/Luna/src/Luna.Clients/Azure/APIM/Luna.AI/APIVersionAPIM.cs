using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM
{
    public class APIVersionAPIM : IAPIVersionAPIM
    {
        private string REQUEST_BASE_URL = "https://lunav2.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _token;
        private HttpClient _httpClient;
        private IAPIVersionSetAPIM _apiVersionSetAPIM;

        [ActivatorUtilitiesConstructor]
        public APIVersionAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                           HttpClient httpClient,
                           IAPIVersionSetAPIM apiVersionSetAPIM)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _subscriptionId = options.CurrentValue.Config.SubscriptionId;
            _resourceGroupName = options.CurrentValue.Config.ResourceGroupname;
            _apimServiceName = options.CurrentValue.Config.APIMServiceName;
            _token = options.CurrentValue.Config.Token;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiVersionSetAPIM = apiVersionSetAPIM;
        }

        private Uri GetAPIVersionAPIMRequestURI(string versionName)
        {
            return new Uri(REQUEST_BASE_URL + GetAPIMRESTAPIPath(versionName));
        }

        private Models.Azure.APIVersion GetUser(string type, APIVersion version)
        {
            Models.Azure.APIVersion api = new Models.Azure.APIVersion();
            api.name = version.GetVersionIdFormat();
            api.properties.displayName = version.GetVersionIdFormat();
            api.properties.apiVersion = version.VersionName;

            IController controller = ControllerHelper.GetController(type);
            api.properties.serviceUrl = controller.GetBaseUrl() + controller.GetPath(version.ProductName, version.DeploymentName);
            api.properties.path = GetAPIMPath(version.ProductName, version.DeploymentName);
            api.properties.apiVersionSetId = _apiVersionSetAPIM.GetAPIMRESTAPIPath(version.DeploymentName);

            return api;
        }

        public string GetAPIMPath(string productName, string deploymentName)
        {
            return string.Format("{0}/{1}", productName, deploymentName);
        }

        public string GetAPIMRESTAPIPath(string versionName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, versionName);
        }

        public async Task CreateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.DeploymentName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type, version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.DeploymentName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type, version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.DeploymentName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type, version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
