// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.DataContracts.Luna.AI;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for the apiVersion resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class APIVersionController : ControllerBase
    {
        private readonly IAPIVersionService _apiVersionService;
        private readonly ILogger<APIVersionController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="apiVersionService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public APIVersionController(IAPIVersionService apiVersionService, ILogger<APIVersionController> logger)
        {
            _apiVersionService = apiVersionService ?? throw new ArgumentNullException(nameof(apiVersionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersions JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/apiVersions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string productName, string deploymentName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all apiVersions in deployment {deploymentName} in product {productName}.");
            return Ok(await _apiVersionService.GetAllAsync(productName, deploymentName));
        }

        /// <summary>
        /// Get an apiVersion
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <param name="versionName">The name of apiversion</param>
        /// <returns>HTTP 200 OK with one apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/apiVersions/{versionName}", Name = nameof(GetAsync) + nameof(APIVersion))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string productName, string deploymentName, string versionName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get apiVersion {versionName.ToString()} in deployment {deploymentName} in product {productName}.");
            return Ok(await _apiVersionService.GetAsync(productName, deploymentName, versionName));
        }

        /// <summary>
        /// Creates pr update a apiVersion within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <param name="versionName">The name of apiversion</param>
        /// <param name="apiVersion">The apiVersion object to create.</param>
        /// <returns>HTTP 201 CREATED with URI to created resource in response header.</returns>
        /// <returns>HTTP 200 OK with updated apiVersion JSON objects in response body.</returns>
        [HttpPut("products/{productName}/deployments/{deploymentName}/apiVersions/{versionName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string productName, string deploymentName, string versionName, [FromBody] APIVersion apiVersion)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (apiVersion == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiVersion)), UserErrorCode.PayloadNotProvided);
            }

            if (!versionName.Equals(apiVersion.VersionName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.NameMismatch);
            }

            if(await _apiVersionService.ExistsAsync(productName, deploymentName, versionName))
            {
                _logger.LogInformation($"Update apiVersion {versionName} in deployment {deploymentName} in product {productName} with payload {JsonSerializer.Serialize(apiVersion)}.");
                apiVersion = await _apiVersionService.UpdateAsync(productName, deploymentName, versionName, apiVersion);
                return Ok(apiVersion);
            }
            else
            {
                _logger.LogInformation($"Create apiVersion {versionName} in deployment {deploymentName} in product {productName} with payload {JsonSerializer.Serialize(apiVersion)}.");
                await _apiVersionService.CreateAsync(productName, deploymentName, apiVersion);
                return CreatedAtRoute(nameof(GetAsync) + nameof(APIVersion), new { productName = productName, deploymentName = deploymentName, versionName = versionName }, apiVersion);
            }

        }


        /// <summary>
        /// Delete an apiVersion
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <param name="versionName">The name of apiversion</param>
        /// <returns>HTTP 204 NO CONTENT</returns>
        [HttpDelete("products/{productName}/deployments/{deploymentName}/apiVersions/{versionName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string productName, string deploymentName, string versionName)
        {

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete apiVersion {versionName.ToString()} in deployment {deploymentName} in product {productName}.");
            await _apiVersionService.DeleteAsync(productName, deploymentName, versionName);
            return NoContent();
        }

        /// <summary>
        /// Get apiVersion source types
        /// </summary>
        /// <returns>The apiVersion</returns>
        [HttpGet("apiVersions/sourceTypes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetSourceTypes()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            List<APIVersionSourceType> list = new List<APIVersionSourceType>();

            list.Add(new APIVersionSourceType() { DisplayName = "Azure ML Pipelines", id = "amlPipeline" });
            list.Add(new APIVersionSourceType() { DisplayName = "Git repo", id = "git" });
            _logger.LogInformation($"Get apiVersion source types.");
            return Ok(list);
        }
    }
}