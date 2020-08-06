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
    /// API controller for the webhook resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhookController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="webhookService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
        {
            _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with webhook JSON objects in body.</returns>
        [HttpGet("offers/{offerName}/webhooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all webhooks in offer {offerName}.");
            return Ok(await _webhookService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets an webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to get.</param>
        /// <returns>HTTP 200 OK with webhook JSON object in body.</returns>
        [HttpGet("offers/{offerName}/webhooks/{webhookName}", Name = nameof(GetAsync) + nameof(Webhook))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string webhookName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get webhook {webhookName} in offer {offerName}.");
            return Ok(await _webhookService.GetAsync(offerName, webhookName));
        }

        /// <summary>
        /// Create or update an webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to update.</param>
        /// <param name="webhook">The updated webhook object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/webhooks/{webhookName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string webhookName, [FromBody] Webhook webhook)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (webhook == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(webhook)), UserErrorCode.PayloadNotProvided);
            }

            if (!webhookName.Equals(webhook.WebhookName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Webhook).Name),
                    UserErrorCode.NameMismatch);
            }

            if(await _webhookService.ExistsAsync(offerName, webhookName))
            {
                _logger.LogInformation($"Update webhook {webhookName} in offer {offerName} with payload {JsonSerializer.Serialize(webhook)}.");
                await _webhookService.UpdateAsync(offerName, webhookName, webhook);
                return Ok(webhook);
            }
            else
            {
                _logger.LogInformation($"Create webhook {webhookName} in offer {offerName} with payload {JsonSerializer.Serialize(webhook)}.");
                await _webhookService.CreateAsync(offerName, webhook);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Webhook), new { offerName = offerName, webhookName = webhook.WebhookName }, webhook);
            }
        }

        /// <summary>
        /// Deletes an webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/webhooks/{webhookName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string webhookName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete webhook {webhookName} from offer {offerName}.");
            await _webhookService.DeleteAsync(offerName, webhookName);
            return NoContent();
        }
    }
}