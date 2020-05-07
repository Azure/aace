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

namespace Luna.Clients.Azure.APIM.Luna.AI
{
    public class OperationAPIM
    {
        private string REQUEST_BASE_URL = "https://lunav2.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}/operations/{4}";
        private static IDictionary<string, string> QUERY_PARAMS = new Dictionary<string, string>
                {
                    {"api-version","2019-12-01"},
                    {"deleteSubscriptions","true"}
                };
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _token;
        private HttpClient _httpClient;

        [ActivatorUtilitiesConstructor]
        public OperationAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                           HttpClient httpClient)
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
        }

        private Uri GetAPIVersionAPIMRequestURI(string type, string versionName)
        {
            return new Uri(REQUEST_BASE_URL + GetAPIMRESTAPIPath(versionName, type));
        }

        private Models.Azure.Operation GetUser(string type)
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            IController controller = ControllerHelper.GetController(type);

            operation.name = controller.GetName();
            operation.properties.displayName = controller.GetName();
            operation.properties.method = controller.GetMethod();
            operation.properties.urlTemplate = controller.GetUrlTemplate();

            return operation;
        }

        public string GetAPIMRESTAPIPath(string type, string versionName)
        {
            IController controller = ControllerHelper.GetController(type);
            var operationName = controller.GetName();
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, versionName, operationName);
        }

        public async Task CreateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(type, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(type, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(type, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
