using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Controller
{
    public class BatchInferenceController : IController
    {
        public BatchInferenceController()
        {
        }

        public string GetName()
        {
            return "batchinference";
        }

        public string GetUrlTemplate()
        {
            return "/batchinference";
        }

        public string GetMethod()
        {
            return "POST";
        }

        public string GetPath(string productName, string deploymentName)
        {
            return $"/api/products/{productName}/deployments/{deploymentName}";
        }

        public string GetBaseUrl()
        {
            return "https://lunaaitest-apiapp.azurewebsites.net";
        }
    }
}
