using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM.Luna.AI
{
    public class UserAPIM
    {
        private string REQUEST_BASE_URL = "https://lunav2.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/users/{3}";
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
        public UserAPIM(IOptionsMonitor<APIMConfigurationOption> options,
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

        private string GetUserName(string owner)
        {
            return owner.Replace("@", "").Replace(".", "");
        }

        private Uri GetUserAPIMRequestURI(string owner)
        {
            var userName = GetUserName(owner);
            return new Uri(REQUEST_BASE_URL + GetAPIMRESTAPIPath(userName));
        }

        private Models.Azure.User GetUser(Product product)
        {
            string[] names = product.Owner.Split('@');
            if (names.Length != 2) throw new InvalidOperationException($"user email format is invalid. email: {product.Owner}");

            Models.Azure.User user = new Models.Azure.User();
            user.name = GetUserName(product.Owner);
            user.properties.email = product.Owner;
            user.properties.firstName = names[0] ?? user.properties.firstName;
            user.properties.lastName = names[1] ?? user.properties.lastName;

            return user;
        }

        public string GetAPIMRESTAPIPath(string userName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, userName);
        }
        public async Task CreateAsync(Product product)
        {
            Uri requestUri = GetUserAPIMRequestURI(product.Owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(product)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(Product product)
        {
            Uri requestUri = GetUserAPIMRequestURI(product.Owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(product)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(Product product)
        {
            Uri requestUri = GetUserAPIMRequestURI(product.Owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(product)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
