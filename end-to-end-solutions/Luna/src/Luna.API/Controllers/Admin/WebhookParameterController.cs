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
    /// API controller for the webhookParameterController resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class WebhookParameterController : ControllerBase
    {
        private readonly IWebhookParameterService _webhookParameterService;
        private readonly ILogger<WebhookParameterController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="webhookParameterService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public WebhookParameterController(IWebhookParameterService webhookParameterService, ILogger<WebhookParameterController> logger)
        {
            _webhookParameterService = webhookParameterService ?? throw new ArgumentNullException(nameof(webhookParameterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all webhookParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with webhookParameter JSON objects in body.</returns>
        [HttpGet("offers/{offerName}/webhookParameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all webhook parameters in offer {offerName}.");
            return Ok(await _webhookParameterService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets an webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to get.</param>
        /// <returns>HTTP 200 OK with webhookParameter JSON object in body.</returns>
        [HttpGet("offers/{offerName}/webhookParameters/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get webhook parameter {name} in offer {offerName}.");
            return Ok(await _webhookParameterService.GetAsync(offerName, name));
        }

        /// <summary>
        /// Updates an webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to update.</param>
        /// <param name="webhookParameter">The updated webhookParameter object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/webhookParameters/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateAsync(string offerName, string name, [FromBody] WebhookParameter webhookParameter)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (webhookParameter == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(webhookParameter)), UserErrorCode.PayloadNotProvided);
            }

            if (!name.Equals(webhookParameter.Name))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(WebhookParameter).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation($"Update webhook parameter {name} in offer {offerName} with payload {JsonSerializer.Serialize(webhookParameter)}.");
            await _webhookParameterService.UpdateAsync(offerName, name, webhookParameter);
            return Ok(webhookParameter);
        }

        /// <summary>
        /// Removes any WebhookParameters from the db that are not associated with any Webhooks.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/webhookParameters")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteUnusedAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Attempting to delete all unused WebhookParameters in offer {offerName}.");
            await _webhookParameterService.DeleteUnusedAsync(offerName);
            return NoContent();
        }
    }
}