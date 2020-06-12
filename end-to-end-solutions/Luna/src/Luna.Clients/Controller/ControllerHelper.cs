using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Controller.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.Controller;
using Luna.Clients.Models.Controller.Backend;
using Luna.Data.Entities;
using Newtonsoft.Json;

namespace Luna.Clients.Controller
{
    public static class ControllerHelper
    {
        private static HttpClient HttpClient = new HttpClient();

        public static string GetLunaGeneratedUuid()
        {
            return "a" + Guid.NewGuid().ToString("N").Substring(1);
        }

        public static async Task<string> GetRegion(AMLWorkspace workspace)
        {
            var requestUri = new Uri("https://management.azure.com" + workspace.ResourceId + "?api-version=2019-05-01");
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            IDictionary<string, object> workspaceDetails = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            return workspaceDetails["location"].ToString();
        }

        public static async Task<string> Predict(APIVersion version, AMLWorkspace workspace, object body)
        {
            var requestUri = new Uri(version.RealTimePredictAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            switch (version.AuthenticationType)
            {
                case "Token":
                    var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    break;
                case "Key":
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", version.AuthenticationKey);
                    break;
                case "None":
                    break;
            }

            request.Content = new StringContent(body.ToString());
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return responseContent;
        }

        public static async Task<BatchInferenceResponse> BatchInferenceWithDefaultModel(APIVersion version, AMLWorkspace workspace, IDictionary<string, object> input)
        {
            var requestUri = new Uri(version.BatchInferenceAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var batchInferenceId = GetLunaGeneratedUuid();
            var body = new Models.Controller.Backend.BatchInferenceRequest();
            body.ExperimentName = batchInferenceId;
            var parameterAssignment = new Dictionary<string, object>() { };
            parameterAssignment["run_id"] = batchInferenceId;
            parameterAssignment.Union(input);
            body.ParameterAssignment = parameterAssignment;

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new BatchInferenceResponse { operationId = batchInferenceId };
        }

        public static async Task<TrainModelResponse> TrainModel(APIVersion version, AMLWorkspace workspace, IDictionary<string, object> input)
        {
            var requestUri = new Uri(version.TrainModelAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var modelId = GetLunaGeneratedUuid();
            var body = new Models.Controller.Backend.TrainModelRequest();
            body.ExperimentName = modelId;
            var parameterAssignment = new Dictionary<string, object>() { };
            parameterAssignment["model_key"] = modelId;
            parameterAssignment.Union(input);
            body.ParameterAssignment = parameterAssignment;

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new TrainModelResponse { modelId = modelId };
        }

        public static async Task<GetAModelResponse> GetAModel(AMLWorkspace workspace, Guid modelId, string userId)
        {
            var region = await GetRegion(workspace);
            List<Thread> workerThreads = new List<Thread>();

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/{modelId.ToString("N")}/runs";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            IDictionary<string, object> result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<ModelTrainingStatus> modelTrainingStatusList = (List<ModelTrainingStatus>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<ModelTrainingStatus>));
            if (modelTrainingStatusList == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            var getAModelResponse = new GetAModelResponse();
            foreach (var modelTrainingStatus in modelTrainingStatusList)
                if (modelTrainingStatus.runUuid == modelTrainingStatus.rootRunUuid)
                {
                    getAModelResponse.status = modelTrainingStatus.status;
                    getAModelResponse.startTimeUtc = modelTrainingStatus.startTimeUtc;
                    getAModelResponse.completeTimeUtc = modelTrainingStatus.endTimeUtc;
                    getAModelResponse.error = modelTrainingStatus.error;
                    break;
                }
               
            requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models?name={modelId.ToString("N")}";
            requestUri = new Uri(requestUrl);
            request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await HttpClient.SendAsync(request);
            responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<Model> models = (List<Model>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Model>));
            if (models == null || models.Count != 1)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            Model model = models[0];

            getAModelResponse.modelId = modelId.ToString("N");
            getAModelResponse.description = model.description;
            
            return getAModelResponse;
        }

        public static async Task<List<Model>> GetAllModels(AMLWorkspace workspace)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            var result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<Model> models = (List<Model>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Model>));
            if (models == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            return models;
        }

        public static async Task<BatchInferenceResponse> BatchInference(APIVersion version, AMLWorkspace workspace, Guid modelId, IDictionary<string, object> input)
        {
            var requestUri = new Uri(version.BatchInferenceAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var batchInferenceId = GetLunaGeneratedUuid();
            var body = new Models.Controller.Backend.BatchInferenceRequest();
            body.ExperimentName = batchInferenceId;
            var parameterAssignment = new Dictionary<string, object>() { };
            parameterAssignment["model_key"] = modelId;
            parameterAssignment["run_id"] = batchInferenceId;
            parameterAssignment.Union(input);
            body.ParameterAssignment = parameterAssignment;

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new BatchInferenceResponse { operationId = batchInferenceId };
        }

        public static async Task<GetABatchInferenceOperationResponse> GetABatchInferenceOperation(AMLWorkspace workspace, Guid operationId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/{operationId.ToString("N")}/runs";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            var result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<OperationStatus> operationStatusList = (List<OperationStatus>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<OperationStatus>));
            if (operationStatusList == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            var getABatchInferenceOperationResponse = new GetABatchInferenceOperationResponse();
            getABatchInferenceOperationResponse.operationId = operationId.ToString("N");
            foreach (var operationStatus in operationStatusList)
                if (operationStatus.runUuid == operationStatus.rootRunUuid)
                {
                    getABatchInferenceOperationResponse.status = operationStatus.status;
                    getABatchInferenceOperationResponse.startTimeUtc = operationStatus.startTimeUtc;
                    getABatchInferenceOperationResponse.completeTimeUtc = operationStatus.endTimeUtc;
                    getABatchInferenceOperationResponse.description = operationStatus.description;
                    getABatchInferenceOperationResponse.error = operationStatus.error;
                    break;
                }

            return getABatchInferenceOperationResponse;
        }

        public static async Task<List<Operation>> GetAllBatchInferenceOperations(AMLWorkspace workspace)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            var result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<Operation> operations = (List<Operation>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Operation>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            return operations;
        }

        public static async Task<DeployRealTimePredictionEndpointResponse> DeployRealTimePredictionEndpoint(APIVersion version, AMLWorkspace workspace, Guid modelId, IDictionary<string, object> input)
        {
            Guid dnsNameLabel;
            if (!input.ContainsKey("dns_name_label") || !Guid.TryParse(input["dns_name_label"].ToString(), out dnsNameLabel))
                throw new LunaServerException($"Invalid user input. Missing dns_name_label.");

            var requestUri = new Uri(version.DeployModelAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var deploymentId = GetLunaGeneratedUuid();
            var body = new Models.Controller.Backend.DeployRealTimePredictionEndpointRequest();
            body.ExperimentName = deploymentId;
            var parameterAssignment = new Dictionary<string, object>() { };
            parameterAssignment["model_key"] = modelId;
            parameterAssignment["run_id"] = deploymentId;
            parameterAssignment.Union(input);
            body.ParameterAssignment = parameterAssignment;

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new DeployRealTimePredictionEndpointResponse { deploymentId = deploymentId };
        }

        public static async Task<GetADeployedEndpointResponse> GetADeployedEndpoint(AMLWorkspace workspace, Guid deploymentId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/{deploymentId.ToString("N")}/runs";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            var result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<DeploymentStatus> deploymentStatusList = (List<DeploymentStatus>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<DeploymentStatus>));
            if (deploymentStatusList == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            var getDeployedEndpointResponse = new GetADeployedEndpointResponse();
            getDeployedEndpointResponse.deploymentId = deploymentId.ToString("N");
            foreach (var deploymentStatus in deploymentStatusList)
                if (deploymentStatus.runUuid == deploymentStatus.rootRunUuid)
                {
                    getDeployedEndpointResponse.status = deploymentStatus.status;
                    getDeployedEndpointResponse.startTimeUtc = deploymentStatus.startTimeUtc;
                    getDeployedEndpointResponse.completeTimeUtc = deploymentStatus.endTimeUtc;
                    break;
                }

            requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/services?name={deploymentId.ToString("N")}";
            requestUri = new Uri(requestUrl);
            request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await HttpClient.SendAsync(request);
            responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<Endpoint> endpoints = (List<Endpoint>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Endpoint>));
            if (endpoints == null || endpoints.Count != 1)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            getDeployedEndpointResponse.scoringUrl = endpoints[0].scoringUri;
            getDeployedEndpointResponse.key = endpoints[0].sslKey;

            return getDeployedEndpointResponse;
        }

        public static async Task<List<GetAllDeployedEndpoints>> GetAllDeployedEndpoints(AMLWorkspace workspace)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/services";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            var result = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            if (!result.ContainsKey("value"))
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            List<Endpoint> endpoints = (List<Endpoint>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Endpoint>));
            if (endpoints == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            List<GetAllDeployedEndpoints> results = new List<GetAllDeployedEndpoints>();
            foreach (var endpoint in endpoints) results.Add(new GetAllDeployedEndpoints()
            {
                deploymentId = endpoint.id,
                scoringUrl = endpoint.scoringUri,
                key = endpoint.sslKey,
                description = endpoint.description,
            });
            
            return results;
        }
    }
}
