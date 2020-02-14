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
        [HttpGet("offers/{offerName}/plans/{planName}/customMeterDimensions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName, string planName)
        {
            return Ok(await _customMeterDimensionService.GetAllAsync(offerName, planName));
        }

        /// <summary>
        /// Gets a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension.</param>
        /// <returns>HTTP 200 OK with customMeterDimension JSON object in body.</returns>
        [HttpGet("customMeterDimensions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(long id)
        {
            return Ok(await _customMeterDimensionService.GetAsync(id));
        }

        /// <summary>
        /// Creates a customMeterDimension within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan.</param>
        /// <param name="customMeterDimension">The customMeterDimension object to create.</param>
        /// <returns>HTTP 201 CREATED with URI to created object in response header.</returns>
        [HttpPut("offers/{offerName}/plans/{planName}/customMeterDimensions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateAsync(string offerName, string planName, [FromBody] CustomMeterDimension customMeterDimension)
        {
            await _customMeterDimensionService.CreateAsync(offerName, planName, customMeterDimension);
            return CreatedAtAction(nameof(GetAsync), new { id = customMeterDimension.Id }, customMeterDimension);
        }

        /// <summary>
        /// Updates a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension.</param>
        /// <param name="customMeterDimension">The updated customMeterDimension.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("customMeterDimensions/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateAsync(long id, [FromBody] CustomMeterDimension customMeterDimension)
        {
            await _customMeterDimensionService.UpdateAsync(id, customMeterDimension);
            return NoContent();
        }

        /// <summary>
        /// Deletes a customMeterDimension.
        /// </summary>
        /// <param name="id">The id of the customMeterDimension.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("customMeterDimensions/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(long id)
        {
            await _customMeterDimensionService.DeleteAsync(id);
            return NoContent();
        }
    }
}