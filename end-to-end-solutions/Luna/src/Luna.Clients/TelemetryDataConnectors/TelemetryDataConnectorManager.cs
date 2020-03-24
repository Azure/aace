using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Luna.Clients.Azure.Auth;
using Microsoft.Extensions.Logging;

namespace Luna.Clients.TelemetryDataConnectors
{

    public class TelemetryDataConnectorManager
    {
        private ILogger _logger;
        private HttpClient _httpClient;
        private IKeyVaultHelper _keyVaultHelper;

        public TelemetryDataConnectorManager(HttpClient httpClient, ILogger logger, IKeyVaultHelper keyVaultHelper)
        {
            _logger = logger;
            _httpClient = httpClient;
            _keyVaultHelper = keyVaultHelper;
        }

        public ITelemetryDataConnector CreateTelemetryDataConnector(string type, string configuration)
        {
            if (type.Equals("LogAnalytics", StringComparison.InvariantCultureIgnoreCase))
            {
                ITelemetryDataConnector connector = new LogAnalyticsTelemetryDataConnector(_httpClient, _logger, _keyVaultHelper, configuration);
                return connector;
            }
            else
            {
                //TODO: error handling
                throw new Exception("Type doesnt exist");
            }
        }
    }
}
