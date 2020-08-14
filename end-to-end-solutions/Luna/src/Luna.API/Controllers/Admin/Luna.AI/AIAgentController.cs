using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Entities;
using Luna.Services.Data;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
    public class AIAgentController : ControllerBase
    {
        private readonly IAIAgentService _aiAgentService;

        private readonly ILogger<AMLWorkspaceController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="aiAgentService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public AIAgentController(IAIAgentService aiAgentService, ILogger<AMLWorkspaceController> logger)
        {
            _aiAgentService = aiAgentService ?? throw new ArgumentNullException(nameof(aiAgentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all ai agents.
        /// </summary>
        /// <returns>HTTP 200 OK with aiagent JSON objects in response body.</returns>
        [HttpGet("aiagents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all AI agents.");
            return Ok(await _aiAgentService.GetAllAsync());
        }

        /// <summary>
        /// Get an AI agent.
        /// </summary>
        /// <param name="agentId">The id of the agent to get.</param>
        /// <returns>HTTP 200 OK with ai agent JSON object in response body.</returns>
        [HttpGet("aiagents/{agentId}", Name = nameof(GetAsync) + nameof(AIAgent))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(Guid agentId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get AI agent {agentId.ToString()}");
            return Ok(await _aiAgentService.GetAsync(agentId));
        }

        [HttpGet("aiagents/{agentId}/subscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult> GetAllAgentSubscriptionsAsync(Guid agentId)
        {
            var receivedKey = Request.Headers["Authorization"].ToString();
            await _aiAgentService.ValidateAgentKey(agentId, receivedKey);
            _logger.LogInformation($"Get all subscriptions for AI Agent {agentId.ToString()}");
            return Ok(await _aiAgentService.GetAllSubscriptionsByAgentIdAsync(agentId));
        }

        [HttpGet("aiagents/{agentId}/apiVersions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult> GetAllAPIVersionsAsync(Guid agentId)
        {
            var receivedKey = Request.Headers["Authorization"].ToString();
            await _aiAgentService.ValidateAgentKey(agentId, receivedKey);
            _logger.LogInformation($"Get all API versions for AI Agent {agentId.ToString()}");
            return Ok(await _aiAgentService.GetAllAPIVersionByAgentIdAsync(agentId));
        }

        [HttpGet("aiagents/{agentId}/subscriptions/{subscriptionId}/projectFileUrl/{versionName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        [Produces("text/plain")]
        public async Task<ActionResult> GetProjectFileUrlAsync(Guid agentId, Guid subscriptionId, string versionName)
        {
            var receivedKey = Request.Headers["Authorization"].ToString();
            await _aiAgentService.ValidateAgentKey(agentId, receivedKey);
            _logger.LogInformation($"Get project file url for AI Agent {agentId.ToString()}, subscription {subscriptionId} and version {versionName}");
            return Ok(await _aiAgentService.GetProjectFileSaSUrlAsync(agentId, subscriptionId, versionName));
        }

        /// <summary>
        /// Creates or updates an AI agent.
        /// </summary>
        /// <param name="agentId">The id of the AI agent to update.</param>
        /// <param name="agent">The updated AI agent object.</param>
        /// <returns>HTTP 201 created or 200 for update.</returns>
        [HttpPut("aiagents/{agentId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(Guid agentId, [FromBody] AIAgent agent)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false);
            if (agent == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(agent)), UserErrorCode.PayloadNotProvided);
            }

            if (!agentId.Equals(agent.AgentId))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AIAgent).Name),
                    UserErrorCode.NameMismatch);
            }
            agent.CreatedTime = DateTime.UtcNow;
            if (await _aiAgentService.ExistsAsync(agentId))
            {
                _logger.LogInformation($"Update AI agent {agentId.ToString()}");
                await _aiAgentService.UpdateAsync(agentId, agent);
                return Ok(agent);
            }
            else
            {
                _logger.LogInformation($"Create AI Agent {agentId.ToString()}");
                await _aiAgentService.CreateAsync(agentId, agent);
                return CreatedAtRoute(nameof(GetAsync) + nameof(AIAgent), new { agentId = agentId }, agent);
            }
        }

        /// <summary>
        /// Deletes an AI agent.
        /// </summary>
        /// <param name="agentId">The id of the AI agent to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("aiagents/{agentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(Guid agentId)
        {
            _logger.LogInformation($"Delete AI agent {agentId.ToString()}.");
            await _aiAgentService.DeleteAsync(agentId);
            return NoContent();
        }
    }
}