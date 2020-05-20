using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class PredictRequest
    {
        public string userId { get; set; }
        public Guid apiSubscriptionId { get; set; }
        public object input { get; set; }
    }
}
