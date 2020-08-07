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
    /// API controller for the restrictedUser resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class RestrictedUserController : ControllerBase
    {
        private readonly IRestrictedUserService _restrictedUserService;
        private readonly ILogger<RestrictedUserController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="restrictedUserService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public RestrictedUserController(IRestrictedUserService restrictedUserService, ILogger<RestrictedUserController> logger)
        {
            _restrictedUserService = restrictedUserService ?? throw new ArgumentNullException(nameof(restrictedUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all restrictedUsers within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <returns>HTTP 200 OK with restrictedUser JSON objects in response body.</returns>
        [HttpGet("offers/{offerName}/plans/{planName}/restrictedUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName, string planName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all restricted users in plan {planName} in offer {offerName}.");
            return Ok(await _restrictedUserService.GetAllAsync(offerName, planName));
        }

        /// <summary>
        /// Get a restricted user
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="tenantId">The tenant Id</param>
        /// <returns>The restricted user</returns>
        [HttpGet("offers/{offerName}/plans/{planName}/restrictedUsers/{tenantId}", Name = nameof(GetAsync) + nameof(RestrictedUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string planName, Guid tenantId)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get restricted user {tenantId.ToString()} in plan {planName} in offer {offerName}.");
            return Ok(await _restrictedUserService.GetAsync(offerName, planName, tenantId));
        }

        /// <summary>
        /// Creates pr update a restrictedUser within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="tenantId">The tenant id</param>
        /// <param name="restrictedUser">The restrictedUser object to create.</param>
        /// <returns>HTTP 201 CREATED with URI to created resource in response header.</returns>
        [HttpPut("offers/{offerName}/plans/{planName}/restrictedUsers/{tenantId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string planName, Guid tenantId, [FromBody] RestrictedUser restrictedUser)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (restrictedUser == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(restrictedUser)), UserErrorCode.PayloadNotProvided);
            }

            if (!tenantId.Equals(restrictedUser.TenantId))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(RestrictedUser).Name),
                    UserErrorCode.NameMismatch);
            }

            if(await _restrictedUserService.ExistsAsync(offerName, planName, tenantId))
            {
                _logger.LogInformation($"Update resticted user {tenantId} in plan {planName} in offer {offerName} with payload {JsonSerializer.Serialize(restrictedUser)}.");
                await _restrictedUserService.UpdateAsync(offerName, planName, restrictedUser);
                return Ok(restrictedUser);
            }
            else
            {
                _logger.LogInformation($"Create resticted user {tenantId} in plan {planName} in offer {offerName} with payload {JsonSerializer.Serialize(restrictedUser)}.");
                await _restrictedUserService.CreateAsync(offerName, planName, restrictedUser);
                return CreatedAtRoute(nameof(GetAsync) + nameof(RestrictedUser), new { offerName = offerName, planName = planName, tenantId = tenantId }, restrictedUser);
            }

        }


        /// <summary>
        /// Delete an restricted user
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="tenantId">The tenant id</param>
        /// <returns>no content</returns>
        [HttpDelete("offers/{offerName}/plans/{planName}/restrictedUsers/{tenantId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string planName, Guid tenantId)
        {

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete restricted user {tenantId.ToString()} in plan {planName} in offer {offerName}.");
            await _restrictedUserService.DeleteAsync(offerName, planName, tenantId);
            return NoContent();
        }
    }
}