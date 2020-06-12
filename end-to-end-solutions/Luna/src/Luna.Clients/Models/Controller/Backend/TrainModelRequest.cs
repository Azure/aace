using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class TrainModelRequest
    {
        public string ExperimentName { get; set; }
        public IDictionary<string, object> ParameterAssignment { get; set; }
        public TrainModelRequest()
        {
            this.ParameterAssignment = new Dictionary<string, object>() { };
        }
    }
}
