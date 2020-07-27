using System.Collections.Generic;

namespace Luna.Clients.Models.Controller.Backend
{
    public class BatchInferenceRequest
    {
        public string experimentName { get; set; }
        public ParameterAssignments parameterAssignments { get; set; }
        public Tags tags { get; set; }

        public class ParameterAssignments
        {
            public string userInput { get; set; }
            public string operationId { get; set; }
            public string modelId { get; set; }
        }
        public class Tags
        {
            public string userId { get; set; }
            public string productName { get; set; }
            public string deploymentName { get; set; }
            public string apiVersion { get; set; }
            public string modelId { get; set; }
            public string operationId { get; set; }
            public string operationType { get; set; }
            public string subscriptionId { get; set; }
        }
        public BatchInferenceRequest()
        {
            this.parameterAssignments = new ParameterAssignments()
            {

            };
            this.tags = new Tags()
            {
                operationType = "training",
            };
        }
    }
}
