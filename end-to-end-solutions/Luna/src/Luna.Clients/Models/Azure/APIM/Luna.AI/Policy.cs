using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public class Policy
    {
        public Properties properties { get; set; }
        public class Properties
        {
            public string format { get; set; }
            public string value { get; set; }
        }
        public Policy()
        {
            this.properties = new Properties();
            this.properties.format = "xml";
        }
    }
}
