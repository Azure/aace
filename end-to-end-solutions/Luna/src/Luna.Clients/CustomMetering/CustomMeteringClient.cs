using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models;
using Luna.Clients.Models.CustomMetering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.CustomMetering
{
    public class CustomMeteringClient : RestClient<CustomMeteringClient>, ICustomMeteringClient
    {
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly ILogger<CustomMeteringClient> _logger;

        public CustomMeteringClient(IOptionsMonitor<SecuredFulfillmentClientConfiguration> optionsMonitor,
                                 ILogger<CustomMeteringClient> logger,
                                 IKeyVaultHelper keyVaultHelper,
                                 HttpClient httpClient) : this(
            optionsMonitor.CurrentValue,
            logger,
            keyVaultHelper,
            httpClient)
        {
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private CustomMeteringClient(
            SecuredFulfillmentClientConfiguration options,
            ILogger<CustomMeteringClient> logger,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient) : base(options, logger, httpClient)
        {
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomMeteringRequestResult> RecordBatchUsageAsync(Guid requestId, Guid correlationId, IEnumerable<Usage> usage, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start("https://marketplaceapi.microsoft.com/api")
                .AddPath("batchUsageEvent")
                .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
            var bearerToken = await _keyVaultHelper.GetBearerToken(
                _options.AzureActiveDirectory, 
                _options.ClientService.AuthenticationResourceId);
            var response = await this.SendRequestAndReturnResult(
                               HttpMethod.Post,
                               requestUrl,
                               requestId,
                               correlationId,
                               bearerToken,
                               null,
                               JsonConvert.SerializeObject(new BatchUsageEvent(usage)),
                               cancellationToken);

            _logger.LogInformation($"RecordBatchUsageAsync response: {response}");

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await HttpRequestResult.ParseAsync<CustomMeteringBatchSuccessResult>(response);

                case HttpStatusCode.Forbidden:
                    return await HttpRequestResult.ParseAsync<CustomMeteringForbiddenResult>(response);

                case HttpStatusCode.Conflict:
                    return await HttpRequestResult.ParseAsync<CustomMeteringConflictResult>(response);

                case HttpStatusCode.BadRequest:
                    return await HttpRequestResult.ParseAsync<CustomMeteringBadRequestResult>(response);

                default:
                    throw new ApplicationException($"Unknown response from the API {await response.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<CustomMeteringRequestResult> RecordUsageAsync(Guid requestId, Guid correlationId, Usage usage, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(_baseUri)
                .AddPath("usageEvent")
                .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
            var bearerToken = await _keyVaultHelper.GetBearerToken(
                _options.AzureActiveDirectory, 
                _options.ClientService.AuthenticationResourceId);
            var response = await this.SendRequestAndReturnResult(
                               HttpMethod.Post,
                               requestUrl,
                               requestId,
                               correlationId,
                               bearerToken,
                               null,
                               JsonConvert.SerializeObject(usage),
                               cancellationToken);

            _logger.LogInformation($"RecordUsageAsync response: {response}");

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await HttpRequestResult.ParseAsync<CustomMeteringSuccessResult>(response);

                case HttpStatusCode.Forbidden:
                    return await HttpRequestResult.ParseAsync<CustomMeteringForbiddenResult>(response);

                case HttpStatusCode.Conflict:
                    return await HttpRequestResult.ParseAsync<CustomMeteringConflictResult>(response);

                case HttpStatusCode.BadRequest:
                    return await HttpRequestResult.ParseAsync<CustomMeteringBadRequestResult>(response);

                default:
                    throw new LunaServerException($"Unknown response from the API {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
