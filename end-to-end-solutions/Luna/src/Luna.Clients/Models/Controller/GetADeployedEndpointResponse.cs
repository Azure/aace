using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class GetADeployedEndpointResponse
    {
        public string deploymentId { get; set; }
        public string status { get; set; }
        public string startTimeUtc { get; set; }
        public string completeTimeUtc { get; set; }
        public string description { get; set; }
        public string scoringUrl { get; set; }
        public string key { get; set; }
    }
}
