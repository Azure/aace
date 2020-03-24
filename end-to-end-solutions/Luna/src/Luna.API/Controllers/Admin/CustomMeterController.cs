using System;
using System.Threading.Tasks;
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
    /// API controller for the customMeter resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class CustomMeterController : ControllerBase
    {
        private readonly ICustomMeterService _customMeterService;

        private readonly ILogger<CustomMeterController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="customMeterService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public CustomMeterController(ICustomMeterService customMeterService, ILogger<CustomMeterController> logger)
        {
            _customMeterService = customMeterService ?? throw new ArgumentNullException(nameof(customMeterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all customMeters.
        /// </summary>
        /// <returns>HTTP 200 OK with customMeter JSON objects in body.</returns>
        [HttpGet("offers/{offerName}/customMeters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            _logger.LogInformation($"Get all custom meters in offer {offerName}.");
            return Ok(await _customMeterService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter to get.</param>
        /// <param name="meterName">The name of the customMeter to get.</param>
        /// <returns>HTTP 200 OK with customMeter JSON object in body.</returns>
        [HttpGet("offers/{offerName}/customMeters/{meterName}", Name = nameof(GetAsync) + nameof(CustomMeter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string meterName)
        {
            _logger.LogInformation($"Get custom meter {meterName} in offer {offerName}.");
            return Ok(await _customMeterService.GetAsync(offerName, meterName));
        }

        /// <summary>
        /// Create or update a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter to update.</param>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/customMeters/{meterName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string meterName, [FromBody] CustomMeter customMeter)
        {
            if (customMeter == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(customMeter)), UserErrorCode.PayloadNotProvided);
            }

            if (!offerName.Equals(customMeter.OfferName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(nameof(offerName)), UserErrorCode.NameMismatch);
            }

            if (!meterName.Equals(customMeter.MeterName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(nameof(meterName)), UserErrorCode.NameMismatch);
            }

            if(await _customMeterService.ExistsAsync(offerName, meterName))
            {
                await _customMeterService.UpdateAsync(offerName, meterName, customMeter);
                return Ok(customMeter);
            }
            else
            {
                await _customMeterService.CreateAsync(offerName, meterName, customMeter);
                return CreatedAtRoute(nameof(GetAsync) + nameof(CustomMeter), new { offerName, meterName }, customMeter);
            }

        }

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="offerName">The offer name of the customMeter to delete.</param>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/customMeters/{meterName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string meterName)
        {
            _logger.LogInformation($"Delete custom meter {meterName} from offer {offerName}.");
            await _customMeterService.DeleteAsync(offerName, meterName);
            return NoContent();
        }
    }
}