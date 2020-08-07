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
using Newtonsoft.Json;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for offer resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;

        private readonly ILogger<OfferController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public OfferController(IOfferService offerService, ILogger<OfferController> logger)
        {
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all offers.
        /// </summary>
        /// <returns>HTTP 200 OK with offer JSON objects in response body.</returns>
        [HttpGet("offers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all offers.");
            return Ok(await _offerService.GetAllAsync());
        }

        /// <summary>
        /// Get an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to get.</param>
        /// <returns>HTTP 200 OK with offer JSON object in response body.</returns>
        [HttpGet("offers/{offerName}", Name = nameof(GetAsync) + nameof(Offer))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get offer {offerName}");
            return Ok(await _offerService.GetAsync(offerName));
        }

        /// <summary>
        /// Creates or updates an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to update.</param>
        /// <param name="offer">The updated offer object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, [FromBody] Offer offer)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (offer == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(offer)), UserErrorCode.PayloadNotProvided);
            }

            if (!offerName.Equals(offer.OfferName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Offer).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _offerService.ExistsAsync(offerName))
            {
                _logger.LogInformation($"Update offer {offerName} with payload {JsonConvert.SerializeObject(offer)}");
                await _offerService.UpdateAsync(offerName, offer);
                return Ok(offer);
            }
            else
            {
                _logger.LogInformation($"Create offer {offerName} with payload {JsonConvert.SerializeObject(offer)}");
                await _offerService.CreateAsync(offer);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Offer), new { offerName = offer.OfferName }, offer);
            }

        }

        /// <summary>
        /// Deletes an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete offer {offerName}.");
            await _offerService.DeleteAsync(offerName);
            return NoContent();
        }

        /// <summary>
        /// Publish an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPost("offers/{offerName}/Publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Publish(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Publish offer {offerName}.");
            await _offerService.PublishAsync(offerName);
            return NoContent();
        }

        /// <summary>
        /// Get warnings for an offer
        /// </summary>
        /// <param name="offerName">The name of the offer to delete.</param>
        /// <returns>HTTP 200 Ok.</returns>
        [HttpGet("offers/{offerName}/warnings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetWarnings(string offerName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            return Ok(await _offerService.GetWarningsAsync(offerName));
        }

        /// <summary>
        /// Get warnings for all offers
        /// </summary>
        /// <returns>HTTP 200 Ok.</returns>
        [HttpGet("offers/warnings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllWarnings()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            return Ok(await _offerService.GetWarningsAsync());
        }


    }
}