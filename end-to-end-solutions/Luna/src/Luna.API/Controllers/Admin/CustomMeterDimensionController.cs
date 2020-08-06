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
    /// API controller for customMeterDimension resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class CustomMeterDimensionController : ControllerBase
    {
        private readonly ICustomMeterDimensionService _customMeterDimensionService;
        private readonly ILogger<CustomMeterDimensionController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="customMeterDimensionService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public CustomMeterDimensionController(ICustomMeterDimensionService customMeterDimensionService, ILogger<CustomMeterDimensionController> logger)
        {
            _customMeterDimensionService = customMeterDimensionService ?? throw new ArgumentNullException(nameof(customMeterDimensionService));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all customMeterDimensions within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <returns>HTTP 200 OK with customMeterDimension JSON objects in body.</returns>
        [HttpGet("offers/{offerName}/plans/{planName}/customMeterDimensions", Name = nameof(GetAsync) + nameof(CustomMeterDimension))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName, string planName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            _logger.LogInformation($"Get all custom meters dimensions from offer {offerName} and plan {planName}.");
            return Ok(await _customMeterDimensionService.GetAllAsync(offerName, planName));
        }

        /// <summary>
        /// Gets a customMeterDimension.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The meterName of the customMeterDimension.</param>
        /// <returns>HTTP 200 OK with customMeterDimension JSON object in body.</returns>
        [HttpGet("offers/{offerName}/plans/{planName}/customMeterDimensions/{meterName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string planName, string meterName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            _logger.LogInformation($"Get custom meter {meterName} dimensions from offer {offerName} and plan {planName}.");
            return Ok(await _customMeterDimensionService.GetAsync(offerName, planName, meterName));
        }

        /// <summary>
        /// Creates a customMeterDimension within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <param name="customMeterDimension">The customMeterDimension object to create.</param>
        /// <returns>HTTP 201 CREATED with URI to created object in response header.</returns>
        [HttpPut("offers/{offerName}/plans/{planName}/customMeterDimensions/{metername}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string planName, string meterName, [FromBody] CustomMeterDimension customMeterDimension)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            if (customMeterDimension == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(customMeterDimension)), UserErrorCode.PayloadNotProvided);
            }

            if (!planName.Equals(customMeterDimension.PlanName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(nameof(offerName)), UserErrorCode.NameMismatch);
            }

            if (!meterName.Equals(customMeterDimension.MeterName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(nameof(meterName)), UserErrorCode.NameMismatch);
            }

            if (await _customMeterDimensionService.ExistsAsync(offerName, planName, meterName))
            {
                await _customMeterDimensionService.UpdateAsync(offerName, planName, meterName, customMeterDimension);
                return Ok(customMeterDimension);
            }
            else
            {
                await _customMeterDimensionService.CreateAsync(offerName, planName, meterName, customMeterDimension);
                return CreatedAtRoute(nameof(GetAsync) + nameof(CustomMeterDimension), 
                    new { offerName = offerName, planName = planName, meterName = meterName }, 
                    customMeterDimension);;
            }
        }

        /// <summary>
        /// Deletes a customMeterDimension.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/plans/{planName}/customMeterDimensions/{metername}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string planName, string meterName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            _logger.LogInformation($"Delete custom meter dimenstion {meterName} in offer {offerName} and plan {planName}.");
            await _customMeterDimensionService.DeleteAsync(offerName, planName, meterName);
            return NoContent();
        }
    }
}