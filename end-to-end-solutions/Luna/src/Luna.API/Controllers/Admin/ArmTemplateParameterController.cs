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
    /// API controller for the armTemplateParameterController resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class ArmTemplateParameterController : ControllerBase
    {
        private readonly IArmTemplateParameterService _armTemplateParameterService;
        private readonly ILogger<ArmTemplateParameterController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="armTemplateParameterService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public ArmTemplateParameterController(IArmTemplateParameterService armTemplateParameterService, ILogger<ArmTemplateParameterController> logger)
        {
            _armTemplateParameterService = armTemplateParameterService ?? throw new ArgumentNullException(nameof(armTemplateParameterService));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all armTemplateParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with armTemplateParameter JSON objects in body.</returns>
        [HttpGet("offers/{offerName}/armTemplateParameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(ArmTemplateParameter).Name, offerName: offerName));
            return Ok(await _armTemplateParameterService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets an armTemplateParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the armTemplateParameter to get.</param>
        /// <returns>HTTP 200 OK with armTemplateparameter JSON object in body.</returns>
        [HttpGet("offers/{offerName}/armTemplateParameters/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(ArmTemplateParameter).Name, name, offerName: offerName));
            return Ok(await _armTemplateParameterService.GetAsync(offerName, name));
        }

        /// <summary>
        /// Updates an armTemplateParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the armTemplateParameter to update.</param>
        /// <param name="armTemplateParameter">The updated armTemplateParameter object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/armTemplateParameters/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateAsync(string offerName, string name, [FromBody] ArmTemplateParameter armTemplateParameter)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (armTemplateParameter == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(ArmTemplateParameter).Name), 
                    UserErrorCode.PayloadNotProvided);
            }

            if (!name.Equals(armTemplateParameter.Name))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(ArmTemplateParameter).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(ArmTemplateParameter).Name,
                name,
                JsonSerializer.Serialize(armTemplateParameter)));

            await _armTemplateParameterService.UpdateAsync(offerName, name, armTemplateParameter);
            return Ok(armTemplateParameter);
        }

        /// <summary>
        /// Removes any ArmTemplateParameters from the db that are not associated with any ArmTemplates.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/armTemplateParameters")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteUnusedAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Attempting to delete all unused ArmTemplateParameters in offer {offerName}.");
            await _armTemplateParameterService.DeleteUnusedAsync(offerName);
            return NoContent();
        }
    }
}