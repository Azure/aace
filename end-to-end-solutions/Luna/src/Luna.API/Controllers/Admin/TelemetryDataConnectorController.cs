using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.TelemetryDataConnectors;
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
    public class TelemetryDataConnectorController : ControllerBase
    {
        private readonly ITelemetryDataConnectorService _telemetryDataConnectorService;

        private readonly ILogger<TelemetryDataConnectorController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="telemetryDataConnectorService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public TelemetryDataConnectorController(ITelemetryDataConnectorService telemetryDataConnectorService, ILogger<TelemetryDataConnectorController> logger)
        {
            _telemetryDataConnectorService = telemetryDataConnectorService ?? throw new ArgumentNullException(nameof(telemetryDataConnectorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all customMeters.
        /// </summary>
        /// <returns>HTTP 200 OK with customMeter JSON objects in body.</returns>
        [HttpGet("telemetryDataConnectors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all telemetry connectors.");
            return Ok(await _telemetryDataConnectorService.GetAllAsync());
        }

        /// <summary>
        /// Gets a customMeter.
        /// </summary>
        /// <param name="name">The name of the telemetryDataConnector to get.</param>
        /// <returns>HTTP 200 OK with customMeter JSON object in body.</returns>
        [HttpGet("telemetryDataConnectors/{name}", Name = nameof(GetAsync) + nameof(TelemetryDataConnector))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get telemetry connector {name}.");
            return Ok(await _telemetryDataConnectorService.GetAsync(name));
        }

        /// <summary>
        /// Create or update a customMeter.
        /// </summary>
        /// <param name="name">The offer name of the customMeter to update.</param>
        /// <param name="connector">The updated customMeter object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("telemetryDataConnectors/{name}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string name, [FromBody] TelemetryDataConnector connector)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            if (connector == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(connector)), UserErrorCode.PayloadNotProvided);
            }

            if (!name.Equals(connector.Name))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(nameof(name)), UserErrorCode.NameMismatch);
            }

            if(await _telemetryDataConnectorService.ExistsAsync(name))
            {
                await _telemetryDataConnectorService.UpdateAsync(name, connector);
                return Ok(connector);
            }
            else
            {
                await _telemetryDataConnectorService.CreateAsync(name, connector);
                return CreatedAtRoute(nameof(GetAsync) + nameof(TelemetryDataConnector), new { name }, connector);
            }

        }

        /// <summary>
        /// Deletes a customMeter.
        /// </summary>
        /// <param name="name">The name of the telemetry data connector to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("telemetryDataConnectors/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            await _telemetryDataConnectorService.DeleteAsync(name);
            return NoContent();
        }

        /// <summary>
        /// Get the valid types of connectors
        /// </summary>
        /// <returns>The connector types</returns>
        [HttpGet("telemetryDataConnectors/connectorTypes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<List<string>> GetConnectorTypesAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);

            List<string> connectorTypes = new List<string>();
            foreach (TelemetryDataConnectorTypes val in Enum.GetValues(typeof(TelemetryDataConnectorTypes)))
            {
                connectorTypes.Add(val.ToString());
            }

            return connectorTypes;
        }

    }
}