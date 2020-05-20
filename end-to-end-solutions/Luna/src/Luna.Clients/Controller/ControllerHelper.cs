using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Controller.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.Controller;
using Luna.Clients.Models.Controller.Backend;
using Luna.Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luna.Clients.Controller
{
    public static class ControllerHelper
    {
        private static IDictionary<string, IController> ControllerMap = new Dictionary<string, IController> {
            {"RTP", new PredictionController()},
            {"BI", new BatchInferenceController()}
        };
        private static HttpClient HttpClient = new HttpClient();

        public static IController GetController(string type)
        {
            if(!ControllerMap.ContainsKey(type))
                throw new ArgumentException("The controller type haven't support yet.");
            return ControllerMap[type];
        }

        public static string GetLunaGeneratedUuid()
        {
            return "a" + Guid.NewGuid().ToString("N").Substring(1);
        }

        public static async Task<BatchInferenceResponse> BatchInference(APIVersion version, AMLWorkspace workspace, IDictionary<string, object> input)
        {
            var requestUri = new Uri(version.BatchInferenceAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var batchInferenceId = GetLunaGeneratedUuid();
            var body = new Models.Controller.Backend.BatchInferenceRequest();
            body.ExperimentName = batchInferenceId;
            var parameterAssignment = new Dictionary<string, object>() { };
            parameterAssignment["operationId"] = batchInferenceId;
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

        public static async Task<string> Predict(APIVersion version, AMLWorkspace workspace, object body)
        {
            var requestUri = new Uri(version.RealTimePredictAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
    }
}
