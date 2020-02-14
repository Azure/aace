using System;
using System.Threading.Tasks;
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
        [HttpGet("customMeters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            _logger.LogInformation($"Get all custom meters");
            return Ok(await _customMeterService.GetAllAsync());
        }

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to get.</param>
        /// <returns>HTTP 200 OK with customMeter JSON object in body.</returns>
        [HttpGet("customMeters/{meterName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string meterName)
        {
            return Ok(await _customMeterService.GetAsync(meterName));
        }

        /// <summary>
        /// Create or update a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to update.</param>
        /// <param name="customMeter">The updated customMeter object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("customMeters/{meterName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string meterName, [FromBody] CustomMeter customMeter)
        {
            if (customMeter == null)
            {
                throw new ArgumentNullException(nameof(customMeter));
            }

            if (!meterName.Equals(customMeter.MeterName))
            {
                throw new ArgumentException("The meter name in url doesn't match meter name in request body.");
            }

            if(await _customMeterService.ExistsAsync(meterName))
            {
                await _customMeterService.UpdateAsync(meterName, customMeter);
                return Ok(customMeter);
            }
            else
            {
                await _customMeterService.CreateAsync(customMeter);
                return CreatedAtAction(nameof(GetAsync), new { meterName = customMeter.MeterName }, customMeter);
            }

        }

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="meterName">The name of the customMeter to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("customMeters/{meterName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string meterName)
        {
            await _customMeterService.DeleteAsync(meterName);
            return NoContent();
        }
    }
}