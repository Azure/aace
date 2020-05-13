using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM
{
    public class OperationAPIM : IOperationAPIM
    {
        private string REQUEST_BASE_URL = "https://lunav2.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}/operations/{4}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _sharedAccessSignature;
        private string _apiVersion;
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
            _sharedAccessSignature = APIMAuthHelper.CreateSharedAccessToken(options.CurrentValue.Config.PrimaryKey, options.CurrentValue.Config.SecondaryKey);
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private Uri GetAPIVersionAPIMRequestURI(string type, string versionName, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(REQUEST_BASE_URL + GetAPIMRESTAPIPath(type, versionName));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
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

        public async Task<bool> ExistsAsync(string type, APIVersion version)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(type, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _sharedAccessSignature);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(type)), Encoding.UTF8, "application/json");

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
            Uri requestUri = GetAPIVersionAPIMRequestURI(type, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _sharedAccessSignature);
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

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _sharedAccessSignature);
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
            if (!(await ExistsAsync(type, version))) return;

            Uri requestUri = GetAPIVersionAPIMRequestURI(type, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _sharedAccessSignature);
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
