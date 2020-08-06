// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Data.DataContracts;
using Luna.Data.Enums;
using Luna.Services.CustomMeterEvent;
using Luna.Services.Data;
using Luna.Services.Provisoning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Provisioning
{
    [ApiController]
    [AllowAnonymous]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class ProvisioningController : ControllerBase
    {
        private readonly IProvisioningService _provisioningService;

        private readonly ILogger<ProvisioningController> _logger;

        private readonly ISubscriptionService _subscriptionService;

        private readonly ICustomMeterEventService _customMeterEventService;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="provisioningService">The provisioning service.</param>
        /// <param name="subscriptionService">The subscriptionService.</param>
        /// <param name="logger">The logger.</param>
        public ProvisioningController(IProvisioningService provisioningService, 
            ISubscriptionService subscriptionService, ICustomMeterEventService customMeterEventService, 
            ILogger<ProvisioningController> logger)
        {
            _provisioningService = provisioningService ?? throw new ArgumentNullException(nameof(provisioningService));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _customMeterEventService = customMeterEventService ?? throw new ArgumentNullException(nameof(customMeterEventService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create resource group for a new subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/CreateResourceGroup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateResourceGroup(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Create resource group for subscription {subscriptionId}.");
            return Ok(await _provisioningService.CreateResourceGroupAsync(subscriptionId));
        }

        /// <summary>
        /// Deploy ARM template for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/DeployArmTemplate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeployArmTemplate(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Deploy ARM template for subscription {subscriptionId}.");
            return Ok(await _provisioningService.DeployArmTemplateAsync(subscriptionId));
        }

        /// <summary>
        /// Check ARM template deployment status for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/CheckArmDeploymentStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CheckArmDeploymentStatus(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Check ARM template deployment status for subscription {subscriptionId}.");
            return Ok(await _provisioningService.CheckArmDeploymentStatusAsync(subscriptionId));
        }

        /// <summary>
        /// Execute webhook for a subscription operation
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/ExecuteWebhook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ExecuteWebhook(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Execute webhook for subscription {subscriptionId}.");
            return Ok(await _provisioningService.ExecuteWebhookAsync(subscriptionId));
        }

        /// <summary>
        /// Activate a subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/ActivateSubscription")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ActivateSubscription(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Activate subscription {subscriptionId}");
            return Ok(await _provisioningService.ActivateSubscriptionAsync(subscriptionId));
        }

        /// <summary>
        /// Update a subscription operation as completed
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/UpdateOperationCompleted")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateOperationCompleted(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"PATCH operation in marketplace for subscription {subscriptionId}.");
            return Ok(await _provisioningService.UpdateOperationCompletedAsync(subscriptionId));
        }

        /// <summary>
        /// Check resource group deployment status
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>The subscription</returns>
        [HttpPost("subscriptions/{subscriptionId}/CheckResourceGroupDeploymentStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CheckResourceGroupDeploymentStatus(Guid subscriptionId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Check resource group deployment status for subscription {subscriptionId}.");
            return Ok(await _provisioningService.CheckResourceGroupDeploymentStatusAsync(subscriptionId));
        }

        /// <summary>
        /// Get provisioning status
        /// </summary>
        /// <returns></returns>
        [HttpGet("subscriptions/ActiveProvisions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetActiveProvisions()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get active provisions.");
            return Ok(await _provisioningService.GetInProgressProvisionsAsync());
        }

        /// <summary>
        /// Process custom meter events
        /// </summary>
        /// <returns></returns>
        [HttpPost("subscriptions/processCustomMeterEvents")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult> ProcessCustomMeterEvents()
        {
            await _customMeterEventService.ReportBatchMeterEvents();
            return Accepted();
        }

        /// <summary>
        /// Process active provisions
        /// </summary>
        /// <returns></returns>
        [HttpPost("subscriptions/ProcessActiveProvisions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ProcessActiveProvisions()
        {
            List<SubscriptionProvision> provisionList = await _provisioningService.GetInProgressProvisionsAsync();
            foreach (var provision in provisionList)
            {
                try
                {
                    _logger.LogInformation($"{ provision.SubscriptionId} is in {nameof(provision.ProvisioningStatus)} state");

                    switch (provision.ProvisioningStatus)
                    {
                        case nameof(ProvisioningState.ProvisioningPending):
                            await _provisioningService.CreateResourceGroupAsync(provision.SubscriptionId);
                            break;
                        case nameof(ProvisioningState.DeployResourceGroupRunning):
                            await _provisioningService.CheckResourceGroupDeploymentStatusAsync(provision.SubscriptionId);
                            break;
                        case nameof(ProvisioningState.ArmTemplatePending):
                            await _provisioningService.DeployArmTemplateAsync(provision.SubscriptionId);
                            break;
                        case nameof(ProvisioningState.ArmTemplateRunning):
                            await _provisioningService.CheckArmDeploymentStatusAsync(provision.SubscriptionId);
                            break;
                        case nameof(ProvisioningState.WebhookPending):
                            await _provisioningService.ExecuteWebhookAsync(provision.SubscriptionId);
                            break;
                        case nameof(ProvisioningState.NotificationPending):
                            if (provision.ProvisioningType.Equals(nameof(ProvisioningType.Subscribe)))
                            {
                                _logger.LogInformation($"Activate subscription {provision.SubscriptionId}");
                                await _provisioningService.ActivateSubscriptionAsync(provision.SubscriptionId);
                            }
                            else
                            {
                                _logger.LogInformation($"Update operation complete for {provision.SubscriptionId}");
                                await _provisioningService.UpdateOperationCompletedAsync(provision.SubscriptionId);
                            }
                            break;
                        default:
                            if (provision.SubscriptionStatus.Equals(nameof(FulfillmentState.Unsubscribed)))
                            {
                                _logger.LogInformation($"Delete data for {provision.SubscriptionId}");
                                await _subscriptionService.DeleteDataAsync(provision.SubscriptionId);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            return Ok();
        }
    }
}