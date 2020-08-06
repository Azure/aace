// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
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
    /// API controller for the armTemplate resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class ArmTemplateController : ControllerBase
    {
        private readonly IArmTemplateService _armTemplateService;
        private readonly ILogger<ArmTemplateController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="armTemplateService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public ArmTemplateController(IArmTemplateService armTemplateService, ILogger<ArmTemplateController> logger)
        {
            _armTemplateService = armTemplateService ?? throw new ArgumentNullException(nameof(armTemplateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all armTemplates within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with armTemplate JSON objects in body.</returns>
        [HttpGet("offers/{offerName}/armTemplates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(ArmTemplate).Name, offerName: offerName));
            return Ok(await _armTemplateService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets an armTemplate within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to get.</param>
        /// <returns>HTTP 200 OK with armTemplate JSON object in body.</returns>
        [HttpGet("offers/{offerName}/armTemplates/{templateName}", Name = nameof(GetAsync) + nameof(ArmTemplate))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string templateName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));
            return Ok(await _armTemplateService.GetAsync(offerName, templateName));
        }

        /// <summary>
        /// Create or update an ARM template
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the ARM template.</param>
        /// <param name="armTemplateJSON">The ARM Template's raw JSON data.</param>
        /// <returns>HTTP 201 CREATED with a URI to created resource in response header.</returns>
        [HttpPut("offers/{offerName}/armTemplates/{templateName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string templateName, [FromBody] object armTemplateJSON)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (armTemplateJSON == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(ArmTemplate).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            ArmTemplate armTemplate = null;
            if (await _armTemplateService.ExistsAsync(offerName, templateName))
            {
                // Update. Do not log armtemplatejson
                _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(ArmTemplateParameter).Name,
                    templateName));
                armTemplate = await _armTemplateService.UpdateAsync(offerName, templateName, armTemplateJSON);
                return Ok(armTemplate);
            }
            else
            {
                // Update. Do not log armtemplatejson
                _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(ArmTemplateParameter).Name,
                    templateName));
                armTemplate = await _armTemplateService.CreateAsync(offerName, templateName, armTemplateJSON);
                return CreatedAtRoute(nameof(GetAsync) + nameof(ArmTemplate), new { OfferName = offerName, TemplateName = templateName }, armTemplate);
            }
        }

        /// <summary>
        /// Deletes an armTemplate record within an offer and removes the armTemplate file from blob storage.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/armTemplates/{templateName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string templateName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(ArmTemplateParameter).Name,
                templateName));
            await _armTemplateService.DeleteAsync(offerName, templateName);
            return NoContent();
        }
    }
}