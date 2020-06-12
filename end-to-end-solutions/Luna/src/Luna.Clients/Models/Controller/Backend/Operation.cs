using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class Operation
    {
        public string experimentId { get; set; }
        public string name { get; set; }
        public String description { get; set; }
        public string createdUtc { get; set; }
    }
}
