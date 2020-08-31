// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for deployment resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class DeploymentController : ControllerBase
    {
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger<RestrictedUserController> _logger;
        private readonly IAPISubscriptionService _apiSubscriptionService;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="deploymentService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public DeploymentController(IDeploymentService deploymentService, ILogger<RestrictedUserController> logger, IAPISubscriptionService apiSubscriptionService)
        {
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiSubscriptionService = apiSubscriptionService ?? throw new ArgumentNullException(nameof(apiSubscriptionService));
        }

        /// <summary>
        /// Gets all deployments within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <returns>HTTP 200 OK with deployment JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string productName)
        {
            // all users can call this API.
            _logger.LogInformation($"Get all deployments in product {productName}.");
            return Ok(await _deploymentService.GetAllAsync(productName));
        }

        /// <summary>
        /// Gets a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to get.</param>
        /// <returns>HTTP 200 OK with deployment JSON object in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}", Name = nameof(GetAsync) + nameof(Deployment))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string productName, string deploymentName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get deployment {deploymentName} in product {productName}.");
            return Ok(await _deploymentService.GetAsync(productName, deploymentName));
        }

        /// <summary>
        /// Create or update a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to update.</param>
        /// <param name="deployment">The updated deployment object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("products/{productName}/deployments/{deploymentName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(string productName, string deploymentName, [FromBody] Deployment deployment)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (deployment == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(deployment)), UserErrorCode.PayloadNotProvided);
            }

            if (!deploymentName.Equals(deployment.DeploymentName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _deploymentService.ExistsAsync(productName, deploymentName))
            {
                _logger.LogInformation($"Update deployment {deploymentName} in product {productName} with payload {JsonSerializer.Serialize(deployment)}.");
                deployment = await _deploymentService.UpdateAsync(productName, deploymentName, deployment);
                return Ok(deployment);
            }
            else
            {
                _logger.LogInformation($"Create deployment {deploymentName} in product {productName} with payload {JsonSerializer.Serialize(deployment)}.");
                await _deploymentService.CreateAsync(productName, deployment);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Deployment), new { productName = productName, deploymentName = deployment.DeploymentName }, deployment);
            }
        }

        /// <summary>
        /// Deletes a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("products/{productName}/deployments/{deploymentName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string productName, string deploymentName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete deployment {deploymentName} from product {productName}.");

            // check if there exist api subscriptions
            var apiSubscriptions = await _apiSubscriptionService.GetAllAsync();
            if (apiSubscriptions.Count != 0)
            {
                foreach (var apiSubscription in apiSubscriptions)
                {
                    if (apiSubscription.ProductName.Equals(productName, StringComparison.InvariantCultureIgnoreCase) && apiSubscription.DeploymentName.Equals(deploymentName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new LunaConflictUserException($"Unable to delete {deploymentName} with subscription");
                    }
                }               
            }

            await _deploymentService.DeleteAsync(productName, deploymentName);
            return NoContent();
        }
    }
}