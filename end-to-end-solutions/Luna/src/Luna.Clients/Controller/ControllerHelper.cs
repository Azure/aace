using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Newtonsoft.Json;

namespace Luna.Clients.Controller
{
    public static class ControllerHelper
    {
        private static IDictionary<string, IController> ControllerMap = new Dictionary<string, IController> {
            {"RTP", new PredictionController()},
        };
        private static HttpClient HttpClient = new HttpClient();

        public static IController GetController(string type)
        {
            if(!ControllerMap.ContainsKey(type))
                throw new ArgumentException("The controller type haven't support yet.");
            return ControllerMap[type];
        }

        public static async Task<object> Predict(APIVersion version, object body)
        {
            var requestUri = new Uri(version.RealTimePredictAPI);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            switch (version.AuthenticationType)
            {
                case "Token":
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", version.AuthenticationKey);
                    break;
                case "Key":
                    // TODO add an exception here
                    break;
                case "None":
                default:
                    break;
            }
            
            request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            
            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
            return responseContent.ToString();
        }
    }
}
