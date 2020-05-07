using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM.Luna.AI
{
    public class ProductAPIM : IProductAPIM
    {
        private string REQUEST_BASE_URL = "https://lunav2.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/products/{3}";
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
        public ProductAPIM(IOptionsMonitor<APIMConfigurationOption> options,
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

        private Uri GetProductAPIMRequestURI(string productName)
        {
            return new Uri(REQUEST_BASE_URL + GetAPIMRESTAPIPath(productName));
        }

        private Models.Azure.Product GetProduct(Product product)
        {
            Models.Azure.Product apiProduct = new Models.Azure.Product();
            apiProduct.name = product.ProductName;
            apiProduct.properties.displayName = product.ProductName;
            return apiProduct;
        }

        public string GetAPIMRESTAPIPath(string productName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName);
        }

        public async Task CreateAsync(Product product)
        {
            Uri requestUri = GetProductAPIMRequestURI(product.ProductName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetProduct(product)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(Product product)
        {
            Uri requestUri = GetProductAPIMRequestURI(product.ProductName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetProduct(product)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(Product product)
        {
            Uri requestUri = GetProductAPIMRequestURI(product.ProductName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetProduct(product)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
