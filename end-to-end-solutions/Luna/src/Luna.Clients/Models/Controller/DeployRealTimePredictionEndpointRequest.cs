using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class DeployRealTimePredictionEndpointRequest
    {
        public string userId { get; set; }
        public Guid subscriptionId { get; set; }
        public IDictionary<string, object> input { get; set; }
    }
}
