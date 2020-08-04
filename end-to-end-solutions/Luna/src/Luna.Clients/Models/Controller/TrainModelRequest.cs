using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller
{
    public class TrainModelRequest
    {
        public string userId { get; set; }
        public Guid subscriptionId { get; set; }
        public string userInput { get; set; }
    }
}
