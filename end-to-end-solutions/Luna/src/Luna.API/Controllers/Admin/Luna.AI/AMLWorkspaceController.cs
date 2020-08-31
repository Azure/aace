// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for workspace resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class AMLWorkspaceController : ControllerBase
    {
        private readonly IAMLWorkspaceService _workspaceService;

        private readonly ILogger<AMLWorkspaceController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="workspaceService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public AMLWorkspaceController(IAMLWorkspaceService workspaceService, ILogger<AMLWorkspaceController> logger)
        {
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get deployed pipelines from a workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to get.</param>
        /// <returns>HTTP 200 OK with workspace JSON object in response body.</returns>
        [HttpGet("amlworkspaces/{workspaceName}/pipelines")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllPipelinesAsync(string workspaceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get workspace {workspaceName}");
            var workspace = await _workspaceService.GetAsync(workspaceName);
            return Ok(await ControllerHelper.GetAllPipelines(workspace));
        }

        /// <summary>
        /// Gets all workspaces.
        /// </summary>
        /// <returns>HTTP 200 OK with workspace JSON objects in response body.</returns>
        [HttpGet("amlworkspaces")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all workspaces.");
            return Ok(await _workspaceService.GetAllAsync());
        }

        /// <summary>
        /// Get an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to get.</param>
        /// <returns>HTTP 200 OK with workspace JSON object in response body.</returns>
        [HttpGet("amlworkspaces/{workspaceName}", Name = nameof(GetAsync) + nameof(AMLWorkspace))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string workspaceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get workspace {workspaceName}");
            return Ok(await _workspaceService.GetAsync(workspaceName));
        }

        public async Task<string> Test()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string test = azureServiceTokenProvider.KeyVaultTokenCallback.ToString();
            return test;
        }

        /// <summary>
        /// Creates or updates an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to update.</param>
        /// <param name="workspace">The updated workspace object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("amlworkspaces/{workspaceName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string workspaceName, [FromBody] AMLWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(workspace)), UserErrorCode.PayloadNotProvided);
            }

            if (!workspaceName.Equals(workspace.WorkspaceName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _workspaceService.ExistsAsync(workspaceName))
            {
                _logger.LogInformation($"Update workspace with {workspaceName}");
                await _workspaceService.UpdateAsync(workspaceName, workspace);
                return Ok(workspace);
            }
            else
            {
                _logger.LogInformation($"Create workspace with {workspaceName}");
                await _workspaceService.CreateAsync(workspace);
                return CreatedAtRoute(nameof(GetAsync) + nameof(AMLWorkspace), new { workspaceName = workspace.WorkspaceName }, workspace);
            }
        }

        /// <summary>
        /// Deletes an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("amlworkspaces/{workspaceName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string workspaceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete workspace {workspaceName}.");
            await _workspaceService.DeleteAsync(workspaceName);
            return NoContent();
        }
    }
}