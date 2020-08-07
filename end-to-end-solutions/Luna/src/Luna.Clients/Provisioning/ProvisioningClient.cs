// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.Models.Provisioning;
using Luna.Data.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Provisioning
{
    /// <summary>
    /// An HTTP client for deploying resources with ARM templates and the resource manager API.
    /// </summary>
    public class ProvisioningClient : RestClient<ProvisioningClient>, IProvisioningClient
    {
        private readonly IKeyVaultHelper _keyVaultHelper;

        [ActivatorUtilitiesConstructor]
        public ProvisioningClient(
            IOptionsMonitor<SecuredProvisioningClientConfiguration> optionsMonitor,
            ILogger<ProvisioningClient> logger,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient) : this(
            optionsMonitor.CurrentValue,
            logger,
            keyVaultHelper,
            httpClient)
        {
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
        }

        public ProvisioningClient(
            SecuredProvisioningClientConfiguration options,
            ILogger<ProvisioningClient> logger,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient
        ) : base(
            options,
            logger,
            httpClient) 
        {
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
        }

        /// <summary>
        /// Deploy to a resource group
        /// </summary>
        /// <remarks>Default to "Complete" deployment</remarks>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
         /// <param name="subscriptionId">ISV Azure subscription id</param>
        /// <param name="resourceGroup">ISV Azure resource group</param>
        /// <param name="deploymentName">Deployment name</param>
        /// <param name="templatePath">Link to template file</param>
        /// <param name="parametersPath">Link to parameter file</param>
        /// <param name="rollbackToLastSuccessful">If true, the rolllback property will be set in the request body</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DeploymentExtendedResult> PutDeploymentAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId, 
            string resourceGroup,
            string deploymentName, 
            string templatePath = default,
            object template = default,
            string parametersPath = default,
            object parameters = default,
            bool rollbackToLastSuccessful = default,
            CancellationToken cancellationToken = default) 
        {
            try
            {
                if ((templatePath == null) == (template == null))
                {
                    throw new LunaBadRequestUserException(LoggingUtils.ComposeBadXorArgumentMessage("templatePath", "template"), UserErrorCode.InvalidParameter);
                }
                if ((parametersPath == null) == (parameters == null))
                {
                    throw new LunaBadRequestUserException(LoggingUtils.ComposeBadXorArgumentMessage("parametersPath", "parameters"), UserErrorCode.InvalidParameter);
                }

                var requestUrl = GetRequestUri(subscriptionId, resourceGroup, deploymentName);

                var body = rollbackToLastSuccessful ?
                    new DeploymentRequestBody
                    {
                        Properties = new DeploymentProperties
                        {
                            Mode = nameof(DeploymentMode.Complete),
                            TemplateLink = new TemplateLink { Uri = templatePath },
                            Template = template,
                            Parameters = parameters,
                            OnErrorDeployment = new OnErrorDeployment { Type = nameof(DeploymentRollback.LastSuccessful) }
                        }
                    } :
                    new DeploymentRequestBody
                    {
                        Properties = new DeploymentProperties
                        {
                            Mode = nameof(DeploymentMode.Complete),
                            TemplateLink = new TemplateLink { Uri = templatePath },
                            Template = template,
                            Parameters = parameters
                        }
                    }
                    ;

                var requestBody = JsonConvert.SerializeObject(body);
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);

                var response = await SendRequestAndReturnResult(
                    HttpMethod.Put,
                    requestUrl,
                    requestId,
                    correlationId,
                    bearerToken,
                    null,
                    requestBody,
                    cancellationToken
                );

                return await DeploymentRequestResult.ParseAsync<DeploymentExtendedResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaProvisioningException("Cannot deploy template.", e.IsRetryable, ProvisioningState.ArmTemplatePending, e);
            }
        }

        /// <summary>
        /// Get the status of a deployment.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="subscriptionId">ISV Azure subscription id</param>
        /// <param name="resourceGroup">ISV Azure resource group</param>
        /// <param name="deploymentName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DeploymentExtendedResult> GetDeploymentAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup, 
            string deploymentName,
            CancellationToken cancellationToken = default) 
        {
            try
            {
                var requestUrl = GetRequestUri(subscriptionId, resourceGroup, deploymentName);

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

                return await DeploymentRequestResult.ParseAsync<DeploymentExtendedResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaProvisioningException($"Cannot get deployment information for {deploymentName}.", e.IsRetryable, ProvisioningState.ArmTemplatePending, e);
            }
        }

        /// <summary>
        /// Validates whether the specified template is syntactically correct and will be accepted by Azure Resource Manager
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="deploymentName"></param>
        /// <param name="deploymentMode"></param>
        /// <param name="templatePath"></param>
        /// <param name="parameterPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DeploymentValidateResult> ValidateTemplateAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup,
            string deploymentName,
            string deploymentMode,
            string templatePath,
            string parameterPath,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var requestUrl = GetRequestUri(subscriptionId, resourceGroup, deploymentName);

                var body = new DeploymentRequestBody
                {
                    Properties = new DeploymentProperties
                    {
                        Mode = deploymentMode,
                        TemplateLink = new TemplateLink { Uri = templatePath },
                        ParametersLink = new ParametersLink { Uri = parameterPath },
                    }
                };

                var requestBody = JsonConvert.SerializeObject(body);
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
                    requestBody,
                    cancellationToken
                );

                return await DeploymentRequestResult.ParseAsync<DeploymentValidateResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaProvisioningException($"Cannot validate template for deployment {deploymentName}.", e.IsRetryable, ProvisioningState.NotSpecified, e);
            }
        }

        /// <summary>
        /// Create a resource group under the given host subscription in the specified location with the specified name
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="subscriptionId">The ISV's host subscription id</param>
        /// <param name="resourceGroup">The name for the resource group</param>
        /// <param name="location">The location for the resource group</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResourceGroupResult> CreateOrUpdateResourceGroupAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup,
            string location,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var requestUrl = GetRequestUri(subscriptionId, resourceGroup);

                var body = new ResourceGroupRequestBody
                {
                    Location = location
                };

                var requestBody = JsonConvert.SerializeObject(body);
                var bearerToken = await _keyVaultHelper.GetBearerToken(
                    _options.AzureActiveDirectory,
                    _options.ClientService.AuthenticationResourceId);

                var response = await SendRequestAndReturnResult(
                    HttpMethod.Put,
                    requestUrl,
                    requestId,
                    correlationId,
                    bearerToken,
                    null,
                    requestBody,
                    cancellationToken
                );

                return await DeploymentRequestResult.ParseAsync<ResourceGroupResult>(response);
            }
            catch (LunaServerException e)
            {
                throw new LunaProvisioningException($"Cannot create or update resource group {resourceGroup}.", e.IsRetryable, ProvisioningState.DeployResourceGroupFailed, e);
            }
        }

        /// <summary>
        /// Check if the resource group exists in the subscription
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="subscriptionId">The ISV's host subscription id</param>
        /// <param name="resourceGroup">The name of the resource group</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ResourceGroupExistsAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup,
            CancellationToken cancellationToken = default
        )
        {
        var requestUrl = GetRequestUri(subscriptionId, resourceGroup);
        var bearerToken = await _keyVaultHelper.GetBearerToken(
            _options.AzureActiveDirectory,
            _options.ClientService.AuthenticationResourceId);

        var response = await SendRequest(
            HttpMethod.Head,
            requestUrl,
            requestId,
            correlationId,
            bearerToken,
            null,
            null,
            cancellationToken
        );

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        throw new LunaProvisioningException(
            $"Request failed to check if resource group {resourceGroup} exists.",
            ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
        }

        /// <summary>
        /// Execute a webhook
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteWebhook(Uri Uri)
        {
            var response = await SendSimpleRequestWithoutToken(HttpMethod.Post, Uri);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            throw new LunaProvisioningException(
                $"Failed to execute webhook. " +
                $"Status Code: {response.StatusCode}. Error: " +
                $"{await response.Content.ReadAsStringAsync()}",
                ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
        }

        /// <summary>
        /// Build the request URI
        /// </summary>
        /// <param name="subscriptionId">ISV Azure subscription id</param>
        /// <param name="resourceGroup">ISV Azure resource group</param>
        /// <param name="deploymentName"></param>
        /// <returns></returns>
        private Uri GetRequestUri(
            string subscriptionId, 
            string resourceGroup, 
            string deploymentName = default
        ) 
        {
            var requestUri = String.IsNullOrEmpty(deploymentName) ?
                FluentUriBuilder.Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId)
                    .AddPath("resourceGroups")
                    .AddPath(resourceGroup)
                    .AddQuery("api-version", _apiVersion)
                    : 
                FluentUriBuilder.Start(_baseUri)
                    .AddPath("subscriptions")
                    .AddPath(subscriptionId)
                    .AddPath("resourcegroups")
                    .AddPath(resourceGroup)
                    .AddPath("providers/Microsoft.Resources/deployments")
                    .AddPath(deploymentName)
                    .AddQuery("api-version", _apiVersion);

            return requestUri.Uri;
        }
    }
}
