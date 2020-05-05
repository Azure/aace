using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.TelemetryDataConnectors.SQL
{
    public class SqlConfiguration
    {
        public string KeyVaultName { get; set; }
        public string ConnectionStringSecretName { get; set; }
    }
}
