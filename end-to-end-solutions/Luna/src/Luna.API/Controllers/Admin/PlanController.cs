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
    /// API controller for plan resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly ILogger<RestrictedUserController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="planService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public PlanController(IPlanService planService, ILogger<RestrictedUserController> logger)
        {
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all plans within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with plan JSON objects in response body.</returns>
        [HttpGet("offers/{offerName}/plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            // all users can call this API.
            _logger.LogInformation($"Get all plans in offer {offerName}.");
            return Ok(await _planService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan to get.</param>
        /// <returns>HTTP 200 OK with plan JSON object in response body.</returns>
        [HttpGet("offers/{offerName}/plans/{planName}", Name = nameof(GetAsync) + nameof(Plan))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string planName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get plan {planName} in offer {offerName}.");
            return Ok(await _planService.GetAsync(offerName, planName));
        }

        /// <summary>
        /// Create or update a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan to update.</param>
        /// <param name="plan">The updated plan object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/plans/{planName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string planName, [FromBody] Plan plan)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (plan == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(plan)), UserErrorCode.PayloadNotProvided);
            }

            if (!planName.Equals(plan.PlanName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Plan).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _planService.ExistsAsync(offerName, planName))
            {
                _logger.LogInformation($"Update plan {planName} in offer {offerName} with payload {JsonSerializer.Serialize(plan)}.");
                await _planService.UpdateAsync(offerName, planName, plan);
                return Ok(plan);
            }
            else
            {
                _logger.LogInformation($"Create plan {planName} in offer {offerName} with payload {JsonSerializer.Serialize(plan)}.");
                await _planService.CreateAsync(offerName, plan);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Plan), new { offerName = offerName, planName = plan.PlanName }, plan);
            }
        }

        /// <summary>
        /// Deletes a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/plans/{planName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string planName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete plan {planName} from offer {offerName}.");
            await _planService.DeleteAsync(offerName, planName);
            return NoContent();
        }
    }
}