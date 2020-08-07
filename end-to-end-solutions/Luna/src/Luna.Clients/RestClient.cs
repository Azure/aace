// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Microsoft.Extensions.Logging;

namespace Luna.Clients
{
    public class RestClient<T>
    {
        protected const string DefaultApiVersionParameterName = "api-version";
        protected readonly SecuredClientConfiguration _options;
        protected readonly string _apiVersion;
        protected readonly string _baseUri;
        protected readonly ILogger<T> _logger;
        private readonly HttpClient _httpClient;

        protected RestClient(
            SecuredClientConfiguration securedClientConfiguration,
            ILogger<T> instanceLogger,
            HttpClient httpClient)
        {
            _options = securedClientConfiguration ?? throw new ArgumentNullException(nameof(securedClientConfiguration));
            _apiVersion = securedClientConfiguration.ClientService.ApiVersion;
            _baseUri = securedClientConfiguration.ClientService.BaseUri;
            _logger = instanceLogger ?? throw new ArgumentNullException(nameof(instanceLogger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Build the http request message
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <param name="requestId">The request id</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="bearerToken">The bearer token</param>
        /// <param name="content">The request content</param>
        /// <returns></returns>
        private static HttpRequestMessage BuildRequest(
            HttpMethod method,
            Uri requestUri,
            Guid requestId,
            Guid correlationId,
            string bearerToken,
            string content)
        {
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = method };

            request.Headers.Add("x-ms-requestid", requestId.ToString());
            request.Headers.Add("x-ms-correlationid", correlationId.ToString());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
           
            if (method.Equals(HttpMethod.Post) ||
                method.Equals(HttpMethod.Put) ||
                method.ToString().Equals("PATCH", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return request;
        }

        /// <summary>
        /// Send a simple request authenticated with a bearer token
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <param name="requestId">The request id</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="bearerToken">The bearer token</param>
        /// <param name="customRequestBuilder">A custom request builder</param>
        /// <param name="content">The request content</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="caller">The caller</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> SendRequest(
            HttpMethod method,
            Uri requestUri,
            Guid requestId,
            Guid correlationId,
            string bearerToken,
            Action<HttpRequestMessage> customRequestBuilder = null,
            string content = "",
            CancellationToken cancellationToken = default,
            [CallerMemberName] string caller = "")
        {
            _logger.LogInformation(LoggingUtils.ComposeHttpRequestLogMessage(
                requestId,
                correlationId,
                caller,
                method,
                requestUri,
                content));

            var request = BuildRequest(
                method,
                requestUri,
                requestId,
                correlationId,
                bearerToken,
                content);

            // Give option to modify the request for non-default settings
            customRequestBuilder?.Invoke(request);

            return await _httpClient.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Send a simple request without token
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> SendSimpleRequestWithoutToken(HttpMethod method, Uri requestUri)
        {
            _logger.LogInformation($"Sending {method} request to request uri {requestUri}.");
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = method };
            var response = await _httpClient.SendAsync(request, default);
            _logger.LogInformation($"Received response with status code {response.StatusCode} from {requestUri}.");
            return response;
        }

        /// <summary>
        /// Send a request and return the http response message
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <param name="requestId">The request id</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="bearerToken">The bearer token</param>
        /// <param name="customRequestBuilder">The custom request builder</param>
        /// <param name="content">The content</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="caller">The caller</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> SendRequestAndReturnResult(
            HttpMethod method,
            Uri requestUri,
            Guid requestId,
            Guid correlationId,
            string bearerToken,
            Action<HttpRequestMessage> customRequestBuilder = null,
            string content = "",
            CancellationToken cancellationToken = default,
            [CallerMemberName] string caller = "")
        {
            _logger.LogInformation(LoggingUtils.ComposeHttpRequestLogMessage(
                requestId,
                correlationId,
                caller,
                method,
                requestUri,
                content));

            var request = BuildRequest(
                method,
                requestUri,
                requestId,
                correlationId,
                bearerToken,
                content);

            // Give option to modify the request for non-default settings
            customRequestBuilder?.Invoke(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();
            var responseLogMessage = LoggingUtils.ComposeHttpResponseLogMessage(
                requestId,
                correlationId,
                caller,
                response.StatusCode,
                result
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(responseLogMessage);
                return response;
            }
            else
            {
                throw new LunaServerException(
                    responseLogMessage,
                    ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
            }
        }
    }
}
