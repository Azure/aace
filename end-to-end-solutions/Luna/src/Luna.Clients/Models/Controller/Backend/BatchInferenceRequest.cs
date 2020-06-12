using System.Collections.Generic;

namespace Luna.Clients.Models.Controller.Backend
{
    public class BatchInferenceRequest
    {
        public string ExperimentName { get; set; }
        public IDictionary<string, object> ParameterAssignment { get; set; }
        public BatchInferenceRequest()
        {
            this.ParameterAssignment = new Dictionary<string, object>() { };
        }
    }
}
