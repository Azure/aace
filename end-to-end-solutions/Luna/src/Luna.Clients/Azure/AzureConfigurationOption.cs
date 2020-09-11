using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Azure
{
    public class AzureConfigurationOption
    {
        public AzureConfiguration Config { get; set; }
    }

    public class AzureConfiguration
    {
        public string VaultName { get; set; }
        public string ControllerBaseUrl { get; set; }
    }
}
