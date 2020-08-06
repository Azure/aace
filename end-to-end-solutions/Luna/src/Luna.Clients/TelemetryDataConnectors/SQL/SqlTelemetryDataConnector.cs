// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.CustomMetering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Luna.Clients.TelemetryDataConnectors.SQL
{
    public class SqlTelemetryDataConnector:ITelemetryDataConnector
    {
        private SqlConfiguration _configuration;
        private ILogger _logger;
        private IKeyVaultHelper _keyVaultHelper;

        public SqlTelemetryDataConnector(ILogger logger, IKeyVaultHelper keyVaultHelper, string configuration)
        {
            _logger = logger;
            _keyVaultHelper = keyVaultHelper;
            _configuration = (SqlConfiguration)JsonSerializer.Deserialize(configuration, typeof(SqlConfiguration));
        }

        public async Task<Usage> GetMeterEventByHourBySubscription(Guid subscriptionId, DateTime startTime, string query)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Usage>> GetMeterEventsByHour(DateTime startTime, string query)
        {
            List<Usage> usageEventList = new List<Usage>();

            try
            {
                _logger.LogInformation($"Query sql database to get telemetry data from {startTime} to {startTime.AddHours(1)} with query {query}.");
                _logger.LogInformation($"Key vault name {_configuration.KeyVaultName}.");
                _logger.LogInformation($"ConnectionStringSecretName {_configuration.ConnectionStringSecretName}");
                string connectionString = await _keyVaultHelper.GetSecretAsync(_configuration.KeyVaultName, _configuration.ConnectionStringSecretName);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand command = conn.CreateCommand();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@effectiveStartTime", startTime);
                    conn.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        usageEventList.Add(new Usage()
                        {
                            EffectiveStartTime = startTime.ToString(),
                            ResourceId = reader["resourceId"].ToString(),
                            Quantity = (float)reader["quantity"]
                        });
                    }
                }

                _logger.LogInformation($"{usageEventList.Count} event(s) returned.");

                return usageEventList;
            }
            catch (Exception e)
            {
                throw new LunaServerException($"SQL query failed with error {e.Message}", innerException: e);
            }

        }
    }
}
