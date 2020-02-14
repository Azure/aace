using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.Fulfillment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Fulfillment
{
    public class FulfillmentClient : RestClient<FulfillmentClient>, IFulfillmentClient
    {
        private const string mockApiVersion = "2018-09-15";
        private readonly IKeyVaultHelper _keyVaultHelper;
        private new readonly ILogger<FulfillmentClient> _logger;

        [ActivatorUtilitiesConstructor]
        public FulfillmentClient(
            IOptionsMonitor<SecuredFulfillmentClientConfiguration> optionsMonitor,
            ILogger<FulfillmentClient> logger,
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

        public FulfillmentClient(
            SecuredFulfillmentClientConfiguration options,
            ILogger<FulfillmentClient> logger,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient) : base(
                options,
                logger,
                httpClient)
        {
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FulfillmentRequestResult> ActivateSubscriptionAsync(
            Guid subscriptionId,
            ActivatedSubscriptionResult subscriptionDetails,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddPath("activate")
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Post,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        JsonConvert.SerializeObject(subscriptionDetails),
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<FulfillmentRequestResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot activate subscription {subscriptionId}.", e);
            }
        }

        public async Task<UpdateOrDeleteSubscriptionRequestResult> DeleteSubscriptionAsync(
            Guid subscriptionId,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Delete,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<UpdateOrDeleteSubscriptionRequestResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot delete subscription {subscriptionId}.", e);
            }
        }

        public async Task<IEnumerable<SubscriptionOperation>> GetOperationsAsync(
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("operations")
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Get,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                return await FulfillmentRequestResult.ParseMultipleAsync<SubscriptionOperation>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException("Cannot get all subscription operations.", e);
            }
        }

        public async Task<SubscriptionResult> GetSubscriptionAsync(
            Guid subscriptionId,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Get,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<SubscriptionResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot get subscription {subscriptionId}.", e);
            }
        }

        public async Task<SubscriptionOperation> GetSubscriptionOperationAsync(
            Guid subscriptionId,
            Guid operationId,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddPath("operations")
                    .AddPath(operationId.ToString())
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Get,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<SubscriptionOperation>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot get operation {operationId} for subscription {subscriptionId}.", e);
            }
        }

        public async Task<IEnumerable<SubscriptionOperation>> GetSubscriptionOperationsAsync(
            Guid subscriptionId,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddPath("operations")
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Get,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                if (_apiVersion == mockApiVersion)
                {
                    return await FulfillmentRequestResult.ParseMultipleAsync<SubscriptionOperation>(response);
                }

                return (await FulfillmentRequestResult.ParseAsync<SubscriptionOperationResult>(response)).Operations;
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot get operations for subscription {subscriptionId}.", e);
            }
        }

        public async Task<SubscriptionPlans> GetSubscriptionPlansAsync(
            Guid subscriptionId,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddPath("listAvailablePlans")
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Get,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<SubscriptionPlans>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot get all available plans for subscription {subscriptionId}.", e);
            }
        }

        public async Task<IEnumerable<SubscriptionResult>> GetSubscriptionsAsync(
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Get,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        "",
                        cancellationToken);

                if (_apiVersion == mockApiVersion)
                {
                    return await FulfillmentRequestResult.ParseMultipleNestedAsync<SubscriptionResult>(response, "subscriptions");
                }

                return await FulfillmentRequestResult.ParseMultipleAsync<SubscriptionResult>(response); // TODO revisit
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException("Cannot get all subscriptions.", e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="marketplaceToken">
        /// Token query parameter in the URL when the user is redirected to the SaaS ISV’s website from Azure.
        /// Note: The URL decodes the token value from the browser before using it.
        /// This token is valid only for 1 hour
        /// </param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="bearerToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResolvedSubscriptionResult> ResolveSubscriptionAsync(
            string marketplaceToken,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath("resolve")
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        HttpMethod.Post,
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        r =>
                        {
                            r.Headers.Add("x-ms-marketplace-token", marketplaceToken);
                        },
                        "",
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<ResolvedSubscriptionResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException("Cannot resolve subscription.", e);
            }
        }

        public async Task<UpdateOrDeleteSubscriptionRequestResult> UpdateSubscriptionAsync(
            Guid subscriptionId,
            ActivatedSubscriptionResult update,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var updateContent = JsonConvert.SerializeObject(update);

                if (!string.IsNullOrEmpty(update.PlanId) && !string.IsNullOrEmpty(update.Quantity))
                {
                    string fulfillmentRestrictionMessage = "Only a plan or quantity can be patched at one time, not both";
                    throw new LunaConflictUserException(fulfillmentRestrictionMessage);
                }
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);
                var response = await SendRequestAndReturnResult(
                        new HttpMethod("PATCH"),
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        updateContent,
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<UpdateOrDeleteSubscriptionRequestResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot update subscription {subscriptionId}.", e);
            }
        }

        public async Task<FulfillmentRequestResult> UpdateSubscriptionOperationAsync(
            Guid subscriptionId,
            Guid operationId,
            OperationUpdate update,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = FluentUriBuilder
                    .Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId.ToString())
                    .AddPath("operations")
                    .AddPath(operationId.ToString())
                    .AddQuery(DefaultApiVersionParameterName, _apiVersion)
                    .Uri;

                requestId = requestId == default ? Guid.NewGuid() : requestId;
                correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);

                var response = await SendRequestAndReturnResult(
                        new HttpMethod("PATCH"),
                        requestUrl,
                        requestId,
                        correlationId,
                        bearerToken,
                        null,
                        JsonConvert.SerializeObject(update),
                        cancellationToken);

                return await FulfillmentRequestResult.ParseAsync<ResolvedSubscriptionResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaFulfillmentException($"Cannot update operation {operationId} for subscription {subscriptionId}.", e);
            }

        }
    }
}
