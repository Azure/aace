// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.CustomMetering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Luna.Clients.TelemetryDataConnectors
{
    public class LogAnalyticsTelemetryDataConnector : ITelemetryDataConnector
    {
        private const string AAD_AUTHORITY_FORMAT = "https://login.microsoftonline.com/{0}";
        private const string AAD_RESOURCE = "https://api.loganalytics.io";
        private const string REQUEST_URL_FORMAT = "https://api.loganalytics.io/v1/workspaces/{0}/query";
        private const string REQUEST_BODY_FORMAT = "{{\"query\":\"{0}\", \"timespan\":\"{1}\"}}";
        private const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private const string TIMESPAN_FORMAT = "{0}/{1}";

        private LogAnalyticsConfiguration _configuration;
        private ILogger _logger;
        private HttpClient _httpClient;
        private IKeyVaultHelper _keyVaultHelper;

        public LogAnalyticsTelemetryDataConnector(HttpClient httpClient, ILogger logger, IKeyVaultHelper keyVaultHelper, string configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _keyVaultHelper = keyVaultHelper;
            _configuration = (LogAnalyticsConfiguration)JsonSerializer.Deserialize(configuration, typeof(LogAnalyticsConfiguration));
        }

        private async Task<string> GetAuthToken()
        {
            string secret = await _keyVaultHelper.GetSecretAsync(_configuration.KeyVaultName, _configuration.AADSecretName);

            var credential = new ClientCredential(_configuration.AADClientId, secret);
            var authContext = new AuthenticationContext(string.Format(AAD_AUTHORITY_FORMAT, _configuration.TenantId), false);
            var token = await authContext.AcquireTokenAsync(AAD_RESOURCE, credential);

            return token.AccessToken;
        }

        /// <summary>
        /// Get meter event by hour
        /// </summary>
        /// <param name="startTime">The start time</param>
        /// <param name="query">The query</param>
        /// <returns></returns>
        public async Task<IEnumerable<Usage>> GetMeterEventsByHour(DateTime startTime, string query)
        {
            List<Usage> eventList = new List<Usage>();
            string token = await GetAuthToken();
            Uri requestUri = new Uri(string.Format(REQUEST_URL_FORMAT, _configuration.WorkspaceId));
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Run telemetry query in an one hour time span starting startTime
            string timespan = string.Format(TIMESPAN_FORMAT, startTime.ToString(DATETIME_FORMAT), startTime.AddHours(1).ToString(DATETIME_FORMAT));
            
            string body = string.Format(REQUEST_BODY_FORMAT, query, timespan);
            request.Content = new StringContent(body);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                QueryResponse queryResult = (QueryResponse)JsonSerializer.Deserialize(responseContent, typeof(QueryResponse));
                if (queryResult == null)
                {
                    throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
                }

                if (queryResult.tables == null || queryResult.tables.Count == 0)
                {
                    throw new LunaServerException($"Can't find any result. The response is {responseContent}.");
                }

                foreach (var table in queryResult.tables)
                {
                    if (table.name.Equals("PrimaryResult", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Check columns
                        if (CheckTableColumnNameAndType(table, 0, "resourceId", "string") &&
                         CheckTableColumnNameAndType(table, 1, "quantity", "real", "long"))
                        {
                            foreach (var row in table.rows)
                            {
                                eventList.Add(new Usage
                                {
                                    Dimension = "",
                                    PlanId = "",
                                    ResourceId = row[0].ToString(),
                                    Quantity = Double.Parse(row[1].ToString()),
                                    EffectiveStartTime = startTime.ToString()
                                });
                            }

                            return eventList;
                        }
                    }
                }

                throw new LunaServerException($"Can't find any result. The response code is {response.StatusCode} and content is {responseContent}.");
            }
            else
            {
                // TODO: error handling
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

        }

        private bool CheckTableColumnNameAndType(ResultTable table, int index, string expectedName, params string[] expectedTypes)
        {
            if (!table.columns[index].name.Equals(expectedName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new LunaServerException($"The name of column {index} {table.columns[index].name} is not {expectedName}.");
            }

            if (!expectedTypes.Contains(table.columns[index].type))
            {
                StringBuilder expectedTypeStr = new StringBuilder();
                foreach (var expectedType in expectedTypes)
                {
                    expectedTypeStr.Append(expectedType + ",");
                }

                string expectedTypeString = expectedTypeStr.ToString();

                //trim the last comma
                expectedTypeString = expectedTypeString.Length > 0 ? expectedTypeString.Substring(0, expectedTypeString.Length - 1) : expectedTypeString;

                throw new LunaServerException($"The type of column {index} {table.columns[index].type} is not in the expected types list {expectedTypeString}.");
            }

            return true;
        }

        public async Task<Usage> GetMeterEventByHourBySubscription(Guid subscriptionId, DateTime startTime, string query)
        {
            throw new NotImplementedException();
        }
    }
}
