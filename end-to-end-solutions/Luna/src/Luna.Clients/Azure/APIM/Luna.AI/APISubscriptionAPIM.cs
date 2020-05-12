using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM
{
    public class APISubscriptionAPIM : IAPISubscriptionAPIM
    {
        private const string REQUEST_BASE_URL_FORMAT = "https://{0}.management.azure-api.net";
        private const string BASE_URL_FORMAT = "https://{0}.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/subscriptions/{3}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _token;
        private string _apiVersion;
        private HttpClient _httpClient;
        private IProductAPIM _productAPIM;
        private IUserAPIM _userAPIM;

        private string _baseUrl;
        private string _requestBaseUrl;

        [ActivatorUtilitiesConstructor]
        public APISubscriptionAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                           HttpClient httpClient,
                           IProductAPIM productAPIM,
                           IUserAPIM userAPIM)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _subscriptionId = options.CurrentValue.Config.SubscriptionId;
            _resourceGroupName = options.CurrentValue.Config.ResourceGroupname;
            _apimServiceName = options.CurrentValue.Config.APIMServiceName;
            _token = options.CurrentValue.Config.Token;
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _baseUrl = string.Format(BASE_URL_FORMAT, _apimServiceName);
            _requestBaseUrl = string.Format(REQUEST_BASE_URL_FORMAT, _apimServiceName);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _productAPIM = productAPIM;
            _userAPIM = userAPIM;
        }

        private Uri GetSubscriptionAPIMRequestURI(Guid subscriptionId, string path = "")
        {
            var builder = new UriBuilder(_requestBaseUrl + GETAPIMRESTAPIPath(subscriptionId) + path);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        private Models.Azure.APISubscription GetSubscription(APISubscription subscription)
        {
            Models.Azure.APISubscription subscriptionAPIM = new Models.Azure.APISubscription();
            subscriptionAPIM.name = subscription.SubscriptionId.ToString();
            subscriptionAPIM.properties.scope = _productAPIM.GetAPIMRESTAPIPath(subscription.ProductName);
            subscriptionAPIM.properties.ownerId = _userAPIM.GetAPIMRESTAPIPath(subscription.UserId);
            subscriptionAPIM.properties.state = Models.Azure.SubscriptionStatus.GetState(subscription.Status);
            return subscriptionAPIM;
        }

        public string GetBaseUrl(string productName, string deploymentName)
        {
            return string.Format("{0}/{1}/{2}", _baseUrl, productName, deploymentName);
        }

        public string GETAPIMRESTAPIPath(Guid subscriptionId)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, subscriptionId.ToString());
        }

        public async Task<bool> ExistsAsync(APISubscription subscription)
        {
            Uri requestUri = GetSubscriptionAPIMRequestURI(subscription.SubscriptionId);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            var body = JsonConvert.SerializeObject(GetSubscription(subscription));
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return false;

            Models.Azure.APISubscription apiSubcriptionAPIM = (Models.Azure.APISubscription)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.APISubscription));
            if (apiSubcriptionAPIM == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            return true;
        }

        public async Task<Models.Azure.APISubscription> CreateAsync(APISubscription subscription)
        {
            Uri requestUri = GetSubscriptionAPIMRequestURI(subscription.SubscriptionId);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            var body = JsonConvert.SerializeObject(GetSubscription(subscription));
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            Models.Azure.APISubscription apiSubscription = (Models.Azure.APISubscription)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.APISubscription));
            if (apiSubscription == null || apiSubscription.properties == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Azure.APISubscription.Properties apiSubscriptionProperties = await ListSecrets(subscription.SubscriptionId);
            apiSubscription.properties.primaryKey = apiSubscriptionProperties.primaryKey;
            apiSubscription.properties.secondaryKey = apiSubscriptionProperties.secondaryKey;

            return apiSubscription;
        }

        public async Task<Models.Azure.APISubscription> UpdateAsync(APISubscription subscription)
        {
            Uri requestUri = GetSubscriptionAPIMRequestURI(subscription.SubscriptionId);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            var body = JsonConvert.SerializeObject(GetSubscription(subscription));
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            Models.Azure.APISubscription apiSubscription = (Models.Azure.APISubscription)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.APISubscription));
            if (apiSubscription == null || apiSubscription.properties == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Azure.APISubscription.Properties apiSubscriptionProperties = await ListSecrets(subscription.SubscriptionId);
            apiSubscription.properties.primaryKey = apiSubscriptionProperties.primaryKey;
            apiSubscription.properties.secondaryKey = apiSubscriptionProperties.secondaryKey;

            return apiSubscription;
        }

        public async Task DeleteAsync(Data.Entities.APISubscription subscription)
        {
            if (!(await ExistsAsync(subscription))) return;

            Uri requestUri = GetSubscriptionAPIMRequestURI(subscription.SubscriptionId);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetSubscription(subscription)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        private async Task<Models.Azure.APISubscription.Properties> ListSecrets(Guid subscriptionId)
        {
            Uri requestUri = GetSubscriptionAPIMRequestURI(subscriptionId, "/listSecrets");
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            Models.Azure.APISubscription.Properties apiSubscriptionProperties = (Models.Azure.APISubscription.Properties)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.APISubscription.Properties));
            if (apiSubscriptionProperties == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            if (apiSubscriptionProperties.primaryKey == null || apiSubscriptionProperties.secondaryKey == null)
            {
                throw new LunaServerException($"Can't find any result. The response is {responseContent}.");
            }
            return apiSubscriptionProperties;
        }

        public async Task<Models.Azure.APISubscription.Properties> RegenerateKey(Guid subscriptionId, string keyName)
        {
            Uri requestUri = GetSubscriptionAPIMRequestURI(subscriptionId, "/regenerate" + keyName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            Models.Azure.APISubscription.Properties apiSubscriptionProperties = await ListSecrets(subscriptionId);

            return apiSubscriptionProperties;
        }
    }
}
