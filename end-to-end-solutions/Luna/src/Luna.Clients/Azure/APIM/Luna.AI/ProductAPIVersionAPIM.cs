﻿using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public class ProductAPIVersionAPIM : IProductAPIVersionAPIM
    {
        private string REQUEST_BASE_URL = "https://lunav2.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/products/{3}/apis/{4}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _token;
        private string _apiVersion;
        private HttpClient _httpClient;
        private IAPIVersionAPIM _apiVersionAPIM;
        private IAPIVersionSetAPIM _apiVersionSetAPIM;

        [ActivatorUtilitiesConstructor]
        public ProductAPIVersionAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                           HttpClient httpClient,
                           IAPIVersionAPIM apiVersionAPIM,
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
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiVersionAPIM = apiVersionAPIM;
            _apiVersionSetAPIM = apiVersionSetAPIM;
        }

        private Uri GetProductAPIMRequestURI(string productName, string versionNameFormat, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(REQUEST_BASE_URL + GetAPIMRESTAPIPath(productName, versionNameFormat));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        private Models.Azure.APIVersion GetProduct(string type, APIVersion version)
        {
            Models.Azure.APIVersion versionAPIM = new Models.Azure.APIVersion();
            versionAPIM.name = version.GetVersionIdFormat();
            versionAPIM.properties.displayName = version.VersionName;
            versionAPIM.properties.apiVersion = version.VersionName;

            IController controller = ControllerHelper.GetController(type);
            versionAPIM.properties.serviceUrl = controller.GetBaseUrl() + controller.GetPath(version.ProductName, version.DeploymentName);
            versionAPIM.properties.path = _apiVersionAPIM.GetAPIMPath(version.ProductName, version.DeploymentName);
            versionAPIM.properties.apiVersionSetId = _apiVersionSetAPIM.GetAPIMRESTAPIPath(version.DeploymentName);
            return versionAPIM;
        }

        public string GetAPIMRESTAPIPath(string productName, string versionNameFormat)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName, versionNameFormat);
        }

        public async Task CreateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetProductAPIMRequestURI(version.ProductName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetProduct(type, version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(string type, APIVersion version)
        {
            Uri requestUri = GetProductAPIMRequestURI(version.ProductName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetProduct(type, version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(string type, APIVersion version)
        {
            Uri requestUri = GetProductAPIMRequestURI(version.ProductName, version.GetVersionIdFormat());
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Add("Authorization", _token);
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetProduct(type, version)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}