// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.TelemetryDataConnectors.SQL;
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
            if (type.Equals(TelemetryDataConnectorTypes.LogAnalytics.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                ITelemetryDataConnector connector = new LogAnalyticsTelemetryDataConnector(_httpClient, _logger, _keyVaultHelper, configuration);
                return connector;
            }
            else if (type.Equals(TelemetryDataConnectorTypes.SQL.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                ITelemetryDataConnector connector = new SqlTelemetryDataConnector(_logger, _keyVaultHelper, configuration);
                return connector;
            }
            else
            {
                throw new LunaServerException($"The specified telemetry data connector type {type} doesn't exist");
            }
        }
    }
}
