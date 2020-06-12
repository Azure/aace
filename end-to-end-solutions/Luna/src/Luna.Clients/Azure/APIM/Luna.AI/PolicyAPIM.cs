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
        private string _controllerBaseUrl;

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
            _controllerBaseUrl = options.CurrentValue.Config.ControllerBaseUrl;
            _apimAuthHelper = new APIMAuthHelper(options.CurrentValue.Config.UId, keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Key).Result);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private Uri GetPolicyAPIMRequestURI(string productName, string deploymentName, string versionName, string operationName, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetAPIMRESTAPIPath(productName, deploymentName, versionName, operationName));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        private Models.Azure.Policy RealTimePrediction(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName);
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"{backendUrl}\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
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

        private Models.Azure.Policy BatchInterenceWithDefaultModel(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName);
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"{backendUrl}\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
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

        private Models.Azure.Policy GetABatchInterenceOperationWithDefaultModel(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy GetAllBatchInterenceOperationsWithDefaultModel(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy TrainModel(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName);
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"{backendUrl}\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
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

        private Models.Azure.Policy GetAModel(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy GetAllModels(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy BatchInterence(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName);
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"{backendUrl}\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
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

        private Models.Azure.Policy GetABatchInterenceOperation(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy GetAllBatchInterenceOperations(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy DeployRealTimePredictionEndpoint(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName);
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"{backendUrl}\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
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

        private Models.Azure.Policy GetADeployedEndpoint(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy GetAllDeployedEndpoints(APIVersion version)
        {
            Models.Azure.Policy policy = new Models.Azure.Policy();
            string backendUrl = _controllerBaseUrl + string.Format("/api/products/{0}/deployments/{1}", version.ProductName, version.DeploymentName) + "/subscriptions/{0}";
            policy.properties.value =
                @"<policies>
                    <inbound>
                        <base />" +
                        $"<set-backend-service base-url =\"@(string.Format(&quot;{backendUrl}&quot;, context.Subscription.Id))\" />" +
                        @"<set-header name=""Content-Type"" exists-action=""override"" >
                            <value>application/json</value>
                        </set-header>
                        <set-query-parameter name=""userid"" exists-action=""override"">
                            <value>@(context.User.Id)</value>
                        </set-query-parameter>
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

        private Models.Azure.Policy GetPolicy(APIVersion version, Models.Azure.OperationTypeEnum operationType)
        {
            switch (operationType)
            {
                case Models.Azure.OperationTypeEnum.RealTimePrediction:
                    return RealTimePrediction(version);
                case Models.Azure.OperationTypeEnum.BatchInferenceWithDefaultModel:
                    return BatchInterenceWithDefaultModel(version);
                case Models.Azure.OperationTypeEnum.GetABatchInferenceOperationWithDefaultModel:
                    return GetABatchInterenceOperationWithDefaultModel(version);
                case Models.Azure.OperationTypeEnum.GetAllBatchInferenceOperationsWithDefaultModel:
                    return GetAllBatchInterenceOperationsWithDefaultModel(version);
                case Models.Azure.OperationTypeEnum.TrainModel:
                    return TrainModel(version);
                case Models.Azure.OperationTypeEnum.GetAModel:
                    return GetAModel(version);
                case Models.Azure.OperationTypeEnum.GetAllModels:
                    return GetAllModels(version);
                case Models.Azure.OperationTypeEnum.BatchInference:
                    return BatchInterence(version);
                case Models.Azure.OperationTypeEnum.GetABatchInferenceOperation:
                    return GetABatchInterenceOperation(version);
                case Models.Azure.OperationTypeEnum.GetAllBatchInferenceOperations:
                    return GetAllBatchInterenceOperations(version);
                case Models.Azure.OperationTypeEnum.DeployRealTimePredictionEndpoint:
                    return DeployRealTimePredictionEndpoint(version);
                case Models.Azure.OperationTypeEnum.GetADeployedEndpoint:
                    return GetADeployedEndpoint(version);
                case Models.Azure.OperationTypeEnum.GetAllDeployedEndpoints:
                    return GetAllDeployedEndpoints(version);
                default:
                    throw new LunaServerException($"Invalid operation type. The type is {nameof(operationType)}.");
            }
        }

        public string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName, string operationName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName + deploymentName + versionName, operationName);
        }

        public async Task<bool> ExistsAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType)
        {
            Uri requestUri = GetPolicyAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operationName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy(version, operationType)), Encoding.UTF8, "application/json");

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

        public async Task CreateAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType)
        {
            Uri requestUri = GetPolicyAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operationName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            var body = JsonConvert.SerializeObject(GetPolicy(version, operationType));
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType)
        {
            Uri requestUri = GetPolicyAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operationName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy(version, operationType)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType)
        {
            if (!(await ExistsAsync(version, operationName, operationType))) return;

            Uri requestUri = GetPolicyAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operationName);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(GetPolicy(version, operationType)), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
