using System;
using System.Collections.Generic;

namespace Luna.Clients.Models.Controller
{
    public class BatchInferenceRequest
    {
        public string userId { get; set; }
        public Guid subscriptionId { get; set; }
        public IDictionary<string, object> input { get; set; }
    }
}
