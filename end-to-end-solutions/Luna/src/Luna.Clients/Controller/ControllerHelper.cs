using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Luna.Clients.Controller.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.Controller;
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



        public static async Task<BatchInferenceResponse> BatchInferenceWithDefaultModel(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, string userInput)
        {
            var requestUri = new Uri(version.BatchInferenceAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var batchInferenceId = GetLunaGeneratedUuid();
            var batchInferenceWithDefaultModelRequest = new Models.Controller.Backend.BatchInferenceWithDefaultModelRequest();
            batchInferenceWithDefaultModelRequest.experimentName = $"p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.SubscriptionId}_infer";
            batchInferenceWithDefaultModelRequest.parameterAssignments.userInput = userInput;
            batchInferenceWithDefaultModelRequest.parameterAssignments.operationId = batchInferenceId;
            batchInferenceWithDefaultModelRequest.tags.userId = apiSubscription.UserId;
            batchInferenceWithDefaultModelRequest.tags.productName = product.ProductName;
            batchInferenceWithDefaultModelRequest.tags.deploymentName = deployment.DeploymentName;
            batchInferenceWithDefaultModelRequest.tags.apiVersion = version.VersionName;
            batchInferenceWithDefaultModelRequest.tags.operationId = batchInferenceId;
            batchInferenceWithDefaultModelRequest.tags.operationType = "inference";
            batchInferenceWithDefaultModelRequest.tags.subscriptionId = apiSubscription.SubscriptionId.ToString();

            request.Content = new StringContent(JsonConvert.SerializeObject(batchInferenceWithDefaultModelRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new BatchInferenceResponse { operationId = batchInferenceId };
        }

        public static async Task<Models.Controller.GetABatchInferenceOperationResponse> GetABatchInferenceOperationWithDefaultModel(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid operationId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_infer/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var getABatchInferenceOperationRequest = new Models.Controller.Backend.GetABatchInferenceOperationRequest();
            getABatchInferenceOperationRequest.filter = $"runType eq azureml.PipelineRun and tags/operationType eq inference and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId} and tags/operationId eq {operationId.ToString("N")}";

            request.Content = new StringContent(JsonConvert.SerializeObject(getABatchInferenceOperationRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.GetABatchInferenceOperationResponse> operations = (List<Models.Controller.Backend.GetABatchInferenceOperationResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetABatchInferenceOperationResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.GetABatchInferenceOperationResponse getABatchInferenceOperationResponse = new Models.Controller.GetABatchInferenceOperationResponse();
            foreach (var operation in operations)
            {
                getABatchInferenceOperationResponse.operations.Add(new Models.Controller.GetABatchInferenceOperationResponse.Operation()
                {
                    operationId = operation.tags.operationId,
                    operationType = operation.tags.operationType,
                    startTimeUtc = operation.startTimeUtc,
                    completeTimeUtc = operation.endTimeUtc,
                    description = operation.description,
                    error = operation.error,
                });
            }
            return getABatchInferenceOperationResponse;
        }

        public static async Task<Models.Controller.ListAllInferenceOperationsByUserResponse> ListAllInferenceOperationsByUserWithDefaultModel(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_infer/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var listAllInferenceOperationsByUserRequest = new Models.Controller.Backend.ListAllInferenceOperationsByUserRequest();
            listAllInferenceOperationsByUserRequest.filter = $"runType eq azureml.PipelineRun and tags/operationType eq inference and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId}";

            request.Content = new StringContent(JsonConvert.SerializeObject(listAllInferenceOperationsByUserRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.ListAllInferenceOperationsByUserResponse> operations = (List<Models.Controller.Backend.ListAllInferenceOperationsByUserResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.ListAllInferenceOperationsByUserResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.ListAllInferenceOperationsByUserResponse listAllInferenceOperationsByUserResponse = new Models.Controller.ListAllInferenceOperationsByUserResponse();
            foreach (var operation in operations)
            {
                listAllInferenceOperationsByUserResponse.operations.Add(new Models.Controller.ListAllInferenceOperationsByUserResponse.Operation()
                {
                    operationId = operation.tags.operationId,
                    operationType = operation.tags.operationType,
                    startTimeUtc = operation.startTimeUtc,
                    completeTimeUtc = operation.endTimeUtc,
                    description = operation.description,
                    error = operation.error,
                });
            }
            return listAllInferenceOperationsByUserResponse;
        }

        public static async Task<TrainModelResponse> TrainModel(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, string userInput)
        {
            var requestUri = new Uri(version.TrainModelAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var modelId = GetLunaGeneratedUuid();
            var trainModelRequest = new Models.Controller.Backend.TrainModelRequest();
            trainModelRequest.experimentName = $"p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_train";
            trainModelRequest.parameterAssignments.userInput = userInput;
            trainModelRequest.parameterAssignments.modelId = modelId;
            trainModelRequest.parameterAssignments.userId = apiSubscription.UserId;
            trainModelRequest.parameterAssignments.productName = product.ProductName;
            trainModelRequest.parameterAssignments.deploymentName = deployment.DeploymentName;
            trainModelRequest.parameterAssignments.apiVersion = version.VersionName;
            //trainModelRequest.parameterAssignments.subscriptionId = apiSubscription.SubscriptionId.ToString();
            trainModelRequest.tags.userId = apiSubscription.UserId;
            trainModelRequest.tags.productName = product.ProductName;
            trainModelRequest.tags.deploymentName = deployment.DeploymentName;
            trainModelRequest.tags.apiVersion = version.VersionName;
            trainModelRequest.tags.modelId = modelId;
            trainModelRequest.tags.operationId = modelId;
            trainModelRequest.tags.operationType = "training";
            trainModelRequest.tags.subscriptionId = apiSubscription.SubscriptionId.ToString();

            var body = JsonConvert.SerializeObject(trainModelRequest);
            request.Content = new StringContent(body);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new TrainModelResponse { modelId = modelId };
        }

        public static async Task<List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>> ListAllTrainingOperationsByUser(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_train/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var getAllTrainingOperationsByModelIdAndVerifyUserRequest = new Models.Controller.Backend.GetAllTrainingOperationsByModelIdAndVerifyUserRequest();
            getAllTrainingOperationsByModelIdAndVerifyUserRequest.filter = $"tags/operationType eq training and runType eq azureml.PipelineRun and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId}";

            request.Content = new StringContent(JsonConvert.SerializeObject(getAllTrainingOperationsByModelIdAndVerifyUserRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.GetAllTrainingOperationsByModelIdAndVerifyUserResponse> operations = (List<Models.Controller.Backend.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse> results = new List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>();
            foreach (var operation in operations)
            {
                results.Add(new Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse() { 
                    modelId = operation.tags.modelId,
                    status = operation.status,
                    startTimeUtc = operation.startTimeUtc,
                    completeTimeUtc = operation.endTimeUtc,
                    description = operation.description,
                    error = operation.error
                });
            }
            return results;
        }

        public static async Task<ListAllTrainingOperationsByUserResponse> GetAllTrainingOperationsByModelIdUser(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid modelId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_train/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var listAllTrainingOperationsByAUserRequest = new Models.Controller.Backend.ListAllTrainingOperationsByAUserRequest();
            listAllTrainingOperationsByAUserRequest.filter = $"tags/operationType eq training and runType eq azureml.PipelineRun and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId} and tags/modelId eq {modelId.ToString("N")}";

            request.Content = new StringContent(JsonConvert.SerializeObject(listAllTrainingOperationsByAUserRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.ListAllTrainingOperationsByAUserResponse> operations = (List<Models.Controller.Backend.ListAllTrainingOperationsByAUserResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.ListAllTrainingOperationsByAUserResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.ListAllTrainingOperationsByUserResponse listAllTrainingOperationsByUserResponse = new Models.Controller.ListAllTrainingOperationsByUserResponse();
            foreach (var operation in operations)
            {
                listAllTrainingOperationsByUserResponse.operations.Add(new Models.Controller.ListAllTrainingOperationsByUserResponse.Operation()
                {
                    operationType = operation.tags.operationType,
                    modelId = operation.tags.modelId,
                    status = operation.status,
                    startTimeUtc = operation.startTimeUtc,
                    completeTimeUtc = operation.endTimeUtc,
                    description = operation.description,
                    error = operation.error
                });
            }
            return listAllTrainingOperationsByUserResponse;
        }

        public static async Task<List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>> GetAModelByModelIdUserProductDeployment(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid modelId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models?tags=userId={apiSubscription.UserId},productName={product.ProductName},deploymentName={deployment.DeploymentName},subscriptionId={apiSubscription.SubscriptionId}&name={modelId.ToString("N")}";
            //var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models?tags=userId=xiwu@microsoft.com,productName=eddi,deploymentName=westus,subscriptionId=a6c2a7cc-d67e-4a1a-b765-983f08c0423a&name={modelId.ToString("N")}";
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

            List<Models.Controller.Backend.GetAllModelsByUserProductDeploymentResponse> operations = (List<Models.Controller.Backend.GetAllModelsByUserProductDeploymentResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetAllModelsByUserProductDeploymentResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse> results = new List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>();
            foreach (var operation in operations)
            {
                results.Add(new Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse()
                {
                    modelId = operation.name,
                    startTimeUtc = operation.createdTime,
                    completeTimeUtc = operation.modifiedTime,
                    description = operation.description,
                });
            }
            return results;
        }

        public static async Task<List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>> GetAllModelsByUserProductDeployment(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models?tags=userId={apiSubscription.UserId},productName={product.ProductName},deploymentName={deployment.DeploymentName},subscriptionId={apiSubscription.SubscriptionId}";
            //var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models?tags=userId=xiwu@microsoft.com,productName=eddi,deploymentName=westus,subscriptionId=a6c2a7cc-d67e-4a1a-b765-983f08c0423a";
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

            List<Models.Controller.Backend.GetAllModelsByUserProductDeploymentResponse> operations = (List<Models.Controller.Backend.GetAllModelsByUserProductDeploymentResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetAllModelsByUserProductDeploymentResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse> results = new List<Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse>();
            foreach (var operation in operations)
            {
                results.Add(new Models.Controller.GetAllTrainingOperationsByModelIdAndVerifyUserResponse()
                {
                    modelId = operation.name,
                    startTimeUtc = operation.createdTime,
                    completeTimeUtc = operation.modifiedTime,
                    description = operation.description,
                });
            }
            return results;
        }
        public static async Task DeleteAModel(AMLWorkspace workspace, Guid modelId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/models/{modelId.ToString("N")}";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
        public static async Task<BatchInferenceResponse> BatchInference(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid modelId, string userInput)
        {
            var requestUri = new Uri(version.BatchInferenceAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var operationId = GetLunaGeneratedUuid();
            var batchInferenceRequest = new Models.Controller.Backend.BatchInferenceRequest();
            batchInferenceRequest.experimentName = $"p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_infer";
            batchInferenceRequest.parameterAssignments.userInput = userInput;
            batchInferenceRequest.parameterAssignments.modelId = modelId.ToString("N");
            batchInferenceRequest.parameterAssignments.operationId = operationId;
            batchInferenceRequest.tags.userId = apiSubscription.UserId;
            batchInferenceRequest.tags.productName = product.ProductName;
            batchInferenceRequest.tags.deploymentName = deployment.DeploymentName;
            batchInferenceRequest.tags.apiVersion = version.VersionName;
            batchInferenceRequest.tags.modelId = modelId.ToString("N");
            batchInferenceRequest.tags.operationId = operationId;
            batchInferenceRequest.tags.operationType = "inference";
            batchInferenceRequest.tags.subscriptionId = apiSubscription.SubscriptionId.ToString();

            request.Content = new StringContent(JsonConvert.SerializeObject(batchInferenceRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return new BatchInferenceResponse { operationId = operationId };
        }

        public static async Task<Models.Controller.GetABatchInferenceOperationResponse> GetABatchInferenceOperation(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid operationId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_infer/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var getABatchInferenceOperationRequest = new Models.Controller.Backend.GetABatchInferenceOperationRequest();
            getABatchInferenceOperationRequest.filter = $"runType eq azureml.PipelineRun and tags/operationType eq inference and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId} and tags/operationId eq {operationId.ToString("N")}";

            request.Content = new StringContent(JsonConvert.SerializeObject(getABatchInferenceOperationRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.GetABatchInferenceOperationResponse> operations = (List<Models.Controller.Backend.GetABatchInferenceOperationResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetABatchInferenceOperationResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.GetABatchInferenceOperationResponse getABatchInferenceOperationResponse = new Models.Controller.GetABatchInferenceOperationResponse();
            foreach (var operation in operations)
            {
                getABatchInferenceOperationResponse.operations.Add(new Models.Controller.GetABatchInferenceOperationResponse.Operation()
                {
                    operationId = operation.tags.operationId,
                    operationType = operation.tags.operationType,
                    startTimeUtc = operation.startTimeUtc,
                    completeTimeUtc = operation.endTimeUtc,
                    description = operation.description,
                    status = operation.status,
                    error = operation.error,
                });
            }
            return getABatchInferenceOperationResponse;
        }

        public static async Task<Models.Controller.ListAllInferenceOperationsByUserResponse> ListAllInferenceOperationsByUser(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_infer/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var listAllInferenceOperationsByUserRequest = new Models.Controller.Backend.ListAllInferenceOperationsByUserRequest();
            listAllInferenceOperationsByUserRequest.filter = $"runType eq azureml.PipelineRun and tags/operationType eq inference and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId}";

            request.Content = new StringContent(JsonConvert.SerializeObject(listAllInferenceOperationsByUserRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.ListAllInferenceOperationsByUserResponse> operations = (List<Models.Controller.Backend.ListAllInferenceOperationsByUserResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.ListAllInferenceOperationsByUserResponse>));
            if (operations == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.ListAllInferenceOperationsByUserResponse listAllInferenceOperationsByUserResponse = new Models.Controller.ListAllInferenceOperationsByUserResponse();
            foreach (var operation in operations)
            {
                listAllInferenceOperationsByUserResponse.operations.Add(new Models.Controller.ListAllInferenceOperationsByUserResponse.Operation()
                {
                    operationId = operation.tags.operationId,
                    operationType = operation.tags.operationType,
                    startTimeUtc = operation.startTimeUtc,
                    completeTimeUtc = operation.endTimeUtc,
                    description = operation.description,
                    status = operation.status,
                    error = operation.error,
                });
            }
            return listAllInferenceOperationsByUserResponse;
        }

        public static async Task<Models.Controller.DeployRealTimePredictionEndpointResponse> DeployRealTimePredictionEndpoint(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid modelId, string userInput)
        {
            var requestUri = new Uri(version.DeployModelAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpointId = GetLunaGeneratedUuid();
            var deployRealTimePredictionEndpointRequest = new Models.Controller.Backend.DeployRealTimePredictionEndpointRequest();
            deployRealTimePredictionEndpointRequest.experimentName = $"p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_deploy";
            deployRealTimePredictionEndpointRequest.parameterAssignments.userInput = userInput;
            deployRealTimePredictionEndpointRequest.parameterAssignments.endpointId = endpointId;
            deployRealTimePredictionEndpointRequest.parameterAssignments.modelId = modelId.ToString("N");
            deployRealTimePredictionEndpointRequest.parameterAssignments.userId = apiSubscription.UserId;
            deployRealTimePredictionEndpointRequest.parameterAssignments.productName = product.ProductName;
            deployRealTimePredictionEndpointRequest.parameterAssignments.deploymentName = deployment.DeploymentName;
            deployRealTimePredictionEndpointRequest.parameterAssignments.apiVersion = version.VersionName;
            deployRealTimePredictionEndpointRequest.parameterAssignments.subscriptionId = apiSubscription.SubscriptionId.ToString();
            deployRealTimePredictionEndpointRequest.tags.userId = apiSubscription.UserId;
            deployRealTimePredictionEndpointRequest.tags.productName = product.ProductName;
            deployRealTimePredictionEndpointRequest.tags.deploymentName = deployment.DeploymentName;
            deployRealTimePredictionEndpointRequest.tags.apiVersion = version.VersionName;
            deployRealTimePredictionEndpointRequest.tags.modelId = modelId.ToString("N");
            deployRealTimePredictionEndpointRequest.tags.endpointId = endpointId;
            deployRealTimePredictionEndpointRequest.tags.operationType = "deployment";
            deployRealTimePredictionEndpointRequest.tags.subscriptionId = apiSubscription.SubscriptionId.ToString();

            request.Content = new StringContent(JsonConvert.SerializeObject(deployRealTimePredictionEndpointRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            return new Models.Controller.DeployRealTimePredictionEndpointResponse { endpointId = endpointId };
        }
        
        public static async Task<Models.Controller.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse> GetAllDeployOperationsByEndpointIdUser(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid endpointId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_deploy/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var getAllDeployOperationsByEndpointIdAndVerifyUserRequest = new Models.Controller.Backend.GetAllDeployOperationsByEndpointIdAndVerifyUserRequest();
            getAllDeployOperationsByEndpointIdAndVerifyUserRequest.filter = $"runType eq azureml.PipelineRun and tags/operationType eq deployment and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId} and tags/endpointId eq {endpointId.ToString("N")}";

            request.Content = new StringContent(JsonConvert.SerializeObject(getAllDeployOperationsByEndpointIdAndVerifyUserRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse> endpoints = (List<Models.Controller.Backend.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse>));
            if (endpoints == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse getAllDeployOperationsByEndpointIdAndVerifyUserResponse = new Models.Controller.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse();
            foreach (var endpoint in endpoints)
            {
                getAllDeployOperationsByEndpointIdAndVerifyUserResponse.operations.Add(new Models.Controller.GetAllDeployOperationsByEndpointIdAndVerifyUserResponse.Operation()
                {
                    operationType = endpoint.tags.operationType,
                    endpointId = endpoint.tags.endpointId,
                    startTimeUtc = endpoint.startTimeUtc,
                    completeTimeUtc = endpoint.endTimeUtc,
                    status = endpoint.status,
                    error = endpoint.error,
                });
            }
            return getAllDeployOperationsByEndpointIdAndVerifyUserResponse;
        }

        public static async Task<Models.Controller.ListAllDeployOperationsByUserResponse> ListAllDeployOperationsByUser(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/history/v1.0" + workspace.ResourceId + $"/experiments/p_{product.Id}_d_{deployment.Id}_s_{apiSubscription.Id}_deploy/runs:query";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var listAllDeployOperationsByUserRequest = new Models.Controller.Backend.ListAllDeployOperationsByUserRequest();
            listAllDeployOperationsByUserRequest.filter = $"runType eq azureml.PipelineRun and tags/operationType eq deployment and tags/userId eq {apiSubscription.UserId} and tags/subscriptionId eq {apiSubscription.SubscriptionId}";

            request.Content = new StringContent(JsonConvert.SerializeObject(listAllDeployOperationsByUserRequest));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            List<Models.Controller.Backend.ListAllDeployOperationsByUserResponse> endpoints = (List<Models.Controller.Backend.ListAllDeployOperationsByUserResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.ListAllDeployOperationsByUserResponse>));
            if (endpoints == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.ListAllDeployOperationsByUserResponse listAllDeployOperationsByUserResponse = new Models.Controller.ListAllDeployOperationsByUserResponse();
            foreach (var endpoint in endpoints)
            {
                listAllDeployOperationsByUserResponse.operations.Add(new Models.Controller.ListAllDeployOperationsByUserResponse.Operation()
                {
                    operationType = endpoint.tags.operationType,
                    endpointId = endpoint.tags.endpointId,
                    startTimeUtc = endpoint.startTimeUtc,
                    completeTimeUtc = endpoint.endTimeUtc,
                    status = endpoint.status,
                    error = endpoint.error,
                });
            }
            return listAllDeployOperationsByUserResponse;
        }

        public static async Task<Models.Controller.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse> GetAllRealTimeServiceEndpointsByUserProductDeployment(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/services?tags=userId={apiSubscription.UserId},productName={product.ProductName},deploymentName={deployment.DeploymentName},subscriptionId={apiSubscription.SubscriptionId}";
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

            List<Models.Controller.Backend.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse> endpoints = (List<Models.Controller.Backend.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse>));
            if (endpoints == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse getAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse = new Models.Controller.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse();
            foreach (var endpoint in endpoints)
            {
                getAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse.operations.Add(new Models.Controller.GetAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse.Operation()
                {
                    endpointId = endpoint.name,
                    startTimeUtc = endpoint.createdTime,
                    completeTimeUtc = endpoint.updatedTime,
                    scoringUrl = endpoint.scoringUri,
                    description = endpoint.description,
                });
            }
            return getAllRealTimeServiceEndpointsByUserProductAndDeploymentResponse;
        }
            
        public static async Task<Models.Controller.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse> GetARealTimeServiceEndpointByEndpointIdUserProductDeployment(Product product, Deployment deployment, APIVersion version, AMLWorkspace workspace, APISubscription apiSubscription, Guid endpointId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/services?tags=userId={apiSubscription.UserId},productName={product.ProductName},deploymentName={deployment.DeploymentName},subscriptionId={apiSubscription.SubscriptionId}&name={endpointId.ToString("N")}";
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

            List<Models.Controller.Backend.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse> endpoints = (List<Models.Controller.Backend.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse>)System.Text.Json.JsonSerializer.Deserialize(result["value"].ToString(), typeof(List<Models.Controller.Backend.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse>));
            if (endpoints == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }

            Models.Controller.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse getARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse = new Models.Controller.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse();
            foreach (var endpoint in endpoints)
            {
                getARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse.operations.Add(new Models.Controller.GetARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse.Operation()
                {
                    endpointId = endpoint.name,
                    startTimeUtc = endpoint.createdTime,
                    completeTimeUtc = endpoint.updatedTime,
                    scoringUrl = endpoint.scoringUri,
                    description = endpoint.description,
                });
            }
            return getARealTimeServiceEndpointByEndpointIdUserProductAndDeploymentResponse;
        }

        public static async Task DeleteAEndpoint(AMLWorkspace workspace, Guid endpointId)
        {
            var region = await GetRegion(workspace);

            var requestUrl = $"https://{region}.api.azureml.ms/modelmanagement/v1.0" + workspace.ResourceId + $"/services/{endpointId.ToString("N")}";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
