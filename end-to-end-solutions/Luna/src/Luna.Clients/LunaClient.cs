// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Data.DataContracts;
using Luna.Data.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Luna.Clients
{
    public class LunaClient
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger<LunaClient> _logger;
        protected readonly HttpMessageHandler _httpMessageHandler;
        protected readonly IKeyVaultHelper _keyVaultHelper;

        /// <summary>
        /// The LunaClient constructor
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="instanceLogger"></param>
        public LunaClient(
            IHttpClientFactory httpClientFactory,
            ILogger<LunaClient> instanceLogger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = instanceLogger ?? throw new ArgumentNullException(nameof(instanceLogger));
        }

        /// <summary>
        /// Get an offer by name from the database
        /// </summary>
        /// <param name="offerName"></param>
        /// <returns></returns>
        public async Task<Offer> GetOfferAsync(string offerName)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var requestUri = $"{client.BaseAddress}/offers/{offerName}";
            var content = await client.GetStringAsync(requestUri).ConfigureAwait(false);
            _logger.LogInformation($"Received response GetOfferAsync. Response content: {content}");
            return JsonConvert.DeserializeObject<Offer>(content);

        }

        /// <summary>
        /// Get a plan by name from the database
        /// </summary>
        /// <param name="offerName"></param>
        /// <param name="planUniqueName"></param>
        /// <returns></returns>
        public async Task<Plan> GetPlanAsync(
            string offerName,
            string planUniqueName)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var content = await client.GetStringAsync($"{client.BaseAddress}/offers/{offerName}/plans/{planUniqueName}").ConfigureAwait(false);
            _logger.LogInformation($"Received response GetPlanAsync. Response content: {content}");
            return JsonConvert.DeserializeObject<Plan>(content);
        }

        /// <summary>
        /// Get a subscription by id from the database
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<Subscription> GetSubscriptionAsync(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var content = await client.GetStringAsync($"{client.BaseAddress}/subscriptions/{subscriptionId}").ConfigureAwait(false);
            _logger.LogInformation($"Received response GetSubscriptionAsync. Response content: {content}");
            return JsonConvert.DeserializeObject<Subscription>(content);
        }

        /// <summary>
        /// Add a subscription to the database
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> AddSubscriptionAsync(
            Subscription subscription)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var content = new StringContent(JsonConvert.SerializeObject(subscription));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await client.PutAsync(
                $"{client.BaseAddress}/subscriptions/{subscription.SubscriptionId}",
                content
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response AddSubscriptionAsync. Response content: {content}");
            return result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Update the given subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="planUniqueName"></param>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> UpdateSubscriptionAsync(
            Guid subscriptionId,
            string planUniqueName,
            Subscription subscription)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var content = new StringContent(JsonConvert.SerializeObject(subscription));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await client.PutAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/plans/{planUniqueName}",
                content
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response UpdateSubscriptionAsync. Response content: {result}");
            return result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Unsubscribe from the given subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteSubscriptionAsync(
            Guid subscriptionId
        )
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PutAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}",
                null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response DeleteSubscriptionAsync. Response content: {result}");
            return result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Delete data from the given subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteDataAsync(
            Guid subscriptionId
        )
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
            $"{client.BaseAddress}/subscriptions/{subscriptionId}/deleteData",
            null
        ).ConfigureAwait(false);
            _logger.LogInformation($"Received response DeleteDataAsync. Response content: {result}");
            return result.EnsureSuccessStatusCode();
        }
        /// <summary>
        /// Process all active provisions
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ProcessActiveProvisions()
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
            $"{client.BaseAddress}/subscriptions/ProcessActiveProvisions",
            null
        ).ConfigureAwait(false);
            _logger.LogInformation($"Received response ProcessActiveProvisions. Response content: {result}");
            return result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Process all active provisions
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ProcessCustomMeterEvents()
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
            $"{client.BaseAddress}/subscriptions/processCustomMeterEvents",
            null
        ).ConfigureAwait(false);
            _logger.LogInformation($"Received response ProcessCustomMeterEvents. Response content: {result}");
            return result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Get arm template by name
        /// </summary>
        /// <param name="offerName"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public async Task<ArmTemplate> GetArmTemplate(
            string offerName,
            string templateName)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var content = await client.GetStringAsync($"{client.BaseAddress}/offers/{offerName}/armTemplates/{templateName}").ConfigureAwait(false);
            _logger.LogInformation($"Received response GetArmTemplate. Response content: {content}");
            return JsonConvert.DeserializeObject<ArmTemplate>(content);
        }

        /// <summary>
        /// Get Arm template parameter
        /// </summary>
        /// <param name="offerName">the offer name</param>
        /// <param name="parameterName">the parameter name</param>
        /// <returns></returns>
        public async Task<ArmTemplateParameter> GetArmTemplateParameter(
            string offerName,
            string parameterName
        )
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var content = await client.GetStringAsync($"{client.BaseAddress}/offers/{offerName}/armTemplateParameters/{parameterName}").ConfigureAwait(false);
            _logger.LogInformation($"Received response GetArmTemplateParameter. Response content: {content}");
            return JsonConvert.DeserializeObject<ArmTemplateParameter>(content);
        }

        /// <summary>
        /// Unsubscribe a subscription
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> UnsubscribeSubscriptionAsync(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.DeleteAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}"
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response UnsubscribeSubscriptionAsync. Response content: {result}");
            return result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Get active provisions
        /// </summary>
        /// <returns>The active provisions</returns>
        public async Task<List<SubscriptionProvision>> GetActiveProvisionsAsync()
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.GetStringAsync(
                $"{client.BaseAddress}/subscriptions/ActiveProvisions"
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response GetActiveProvisionsAsync. Response content: {result}");
            return JsonConvert.DeserializeObject<List<SubscriptionProvision>>(result);
        }

        /// <summary>
        /// Create resource group for a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> CreateResourceGroup(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/CreateResourceGroup", null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response CreateResourceGroup. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Check resource group deploymenet status
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> CheckResourceGroupDeploymentStatus(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/CheckResourceGroupDeploymentStatus", null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response CheckResourceGroupDeploymentStatus. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Deploy ARM template for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> DeployArmTemplate(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/DeployArmTemplate", null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response DeployArmTemplate. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Check ARM template deployment status
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> CheckArmDeploymentStatus(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/CheckArmDeploymentStatus", null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response CheckArmDeploymentStatus. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Execute webhook for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> ExecuteWebhook(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/ExecuteWebhook", null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response ExecuteWebhook. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Activate a subscription 
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> ActivateSubscription(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/ActivateSubscription", null
            ).ConfigureAwait(false);
            _logger.LogInformation($"Received response ActivateSubscription. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Update a subscription operation as completed
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        public async Task<Subscription> UpdateOperationCompleted(Guid subscriptionId)
        {
            var client = _httpClientFactory.CreateClient("Luna");
            var result = await client.PostAsync(
                $"{client.BaseAddress}/subscriptions/{subscriptionId}/UpdateOperationCompleted", null
            ).ConfigureAwait(false);

            _logger.LogInformation($"Received response UpdateOperationCompleted. Response content: {result.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<Subscription>(await result.Content.ReadAsStringAsync());
        }
    }
}
