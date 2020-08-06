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
    /// API controller for ipConfig resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class IpConfigController : ControllerBase
    {
        private readonly IIpConfigService _ipConfigService;
        private readonly ILogger<IpConfigController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="ipConfigService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public IpConfigController(IIpConfigService ipConfigService, ILogger<IpConfigController> logger)
        {
            _ipConfigService = ipConfigService ?? throw new ArgumentNullException(nameof(ipConfigService));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all ipConfigs within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with ipConfig JSON objects in response body.</returns>
        [HttpGet("offers/{offerName}/ipConfigs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all IPConfig in offer {offerName}.");
            return Ok(await _ipConfigService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to get.</param>
        /// <returns>HTTP 200 OK with ipConfig JSON object in response body.</returns>
        [HttpGet("offers/{offerName}/ipConfigs/{name}", Name = nameof(GetAsync) + nameof(IpConfig))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get IPConfig {name} in offer {offerName}.");
            return Ok(await _ipConfigService.GetAsync(offerName, name));
        }

        /// <summary>
        /// Create or update an ipConfig within an offer. Update can only be used to add new IpBlocks to an existing IpConfig.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to update.</param>
        /// <param name="ipConfig">The updated ipConfig object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/ipConfigs/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string name, [FromBody] IpConfig ipConfig)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (ipConfig == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(ipConfig)), UserErrorCode.PayloadNotProvided);
            }

            if (!name.Equals(ipConfig.Name))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(IpConfig).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _ipConfigService.ExistsAsync(offerName, name))
            {
                _logger.LogInformation($"Update IPConfig {name} in offer {offerName} with payload {JsonSerializer.Serialize(ipConfig)}");
                await _ipConfigService.UpdateAsync(offerName, name, ipConfig);
                return Ok(ipConfig);
            }
            else
            {
                _logger.LogInformation($"Create IPConfig {name} in offer {offerName} with payload {JsonSerializer.Serialize(ipConfig)}");
                await _ipConfigService.CreateAsync(offerName, ipConfig);
                return CreatedAtRoute(nameof(GetAsync) + nameof(IpConfig), new { offerName = offerName, name = name }, ipConfig);
            }

        }

        /// <summary>
        /// Deletes an IpConfig within an Offer and all of the IpBlocks and IpAddresses associated with it.
        /// The delete will only occur if all of the IpAddresses associated with the IpConfig
        /// are not being used.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the offerParameter to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/ipConfigs/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete IpConfig {name} from offer {offerName}.");
            await _ipConfigService.DeleteAsync(offerName, name);
            return NoContent();
        }
    }
}