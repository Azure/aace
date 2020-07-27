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
    public class UserAPIM : IUserAPIM
    {
        private const string REQUEST_BASE_URL_FORMAT = "https://{0}.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/users/{3}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _apiVersion;
        private APIMAuthHelper _apimAuthHelper;
        private HttpClient _httpClient;

        private string _requestBaseUrl;

        [ActivatorUtilitiesConstructor]
        public UserAPIM(IOptionsMonitor<APIMConfigurationOption> options,
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
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _requestBaseUrl = string.Format(REQUEST_BASE_URL_FORMAT, _apimServiceName);
            _apimAuthHelper = new APIMAuthHelper(options.CurrentValue.Config.UId, keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Key).Result);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public string GetUserName(string owner)
        {
            return owner.Replace("@", "").Replace(".", "");
        }

        private Uri GetUserAPIMRequestURI(string owner, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetAPIMRESTAPIPath(owner));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = queryString;

            return new Uri(builder.ToString());
        }

        private Models.Azure.User GetUser(string owner)
        {
            string[] names = owner.Split('@');
            if (names.Length != 2) throw new InvalidOperationException($"user email format is invalid. email: {owner}");

            Models.Azure.User user = new Models.Azure.User();
            user.name = GetUserName(owner);
            user.properties.email = owner;
            user.properties.firstName = names[0] ?? user.properties.firstName;
            user.properties.lastName = names[1] ?? user.properties.lastName;

            return user;
        }

        public string GetAPIMRESTAPIPath(string owner)
        {
            string userName = GetUserName(owner);
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, userName);
        }

        public async Task<bool> ExistsAsync(string owner)
        {
            Uri requestUri = GetUserAPIMRequestURI(owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(owner)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            Models.Azure.User userAPIM = (Models.Azure.User)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.User));
            if (userAPIM == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            return true;
        }

        public async Task CreateAsync(string owner)
        {
            Uri requestUri = GetUserAPIMRequestURI(owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(owner)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(string owner)
        {
            Uri requestUri = GetUserAPIMRequestURI(owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(owner)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(string owner)
        {
            if (!(await ExistsAsync(owner))) return;

            Uri requestUri = GetUserAPIMRequestURI(owner);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetUser(owner)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
