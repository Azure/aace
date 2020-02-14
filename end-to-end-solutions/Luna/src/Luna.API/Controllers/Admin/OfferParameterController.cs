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
    /// API controller for offerParameter resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class OfferParameterController : ControllerBase
    {
        private readonly IOfferParameterService _offerParameterService;
        private readonly ILogger<OfferParameterController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="offerParameterService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public OfferParameterController(IOfferParameterService offerParameterService, ILogger<OfferParameterController> logger)
        {
            _offerParameterService = offerParameterService ?? throw new ArgumentNullException(nameof(offerParameterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all offerParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>HTTP 200 OK with offerParameter JSON objects in response body.</returns>
        [HttpGet("offers/{offerName}/offerParameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string offerName)
        {
            // all users can call this API
            _logger.LogInformation($"Get all offer parameters from offer {offerName}.");
            return Ok(await _offerParameterService.GetAllAsync(offerName));
        }

        /// <summary>
        /// Gets an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to get.</param>
        /// <returns>HTTP 200 OK with offerParameter JSON object in response body.</returns>
        [HttpGet("offers/{offerName}/offerParameters/{parameterName}", Name = nameof(GetAsync) + nameof(OfferParameter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string offerName, string parameterName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get offer parameter {parameterName} in offer {offerName}.");
            return Ok(await _offerParameterService.GetAsync(offerName, parameterName));
        }

        /// <summary>
        /// Create or update an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to update.</param>
        /// <param name="offerParameter">The updated offerParameter object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("offers/{offerName}/offerParameters/{parameterName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(string offerName, string parameterName, [FromBody] OfferParameter offerParameter)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (offerParameter == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(offerParameter)), UserErrorCode.PayloadNotProvided);
            }

            if (!parameterName.Equals(offerParameter.ParameterName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(OfferParameter).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _offerParameterService.ExistsAsync(offerName, parameterName))
            {
                _logger.LogInformation($"Update offer parameter {parameterName} in offer {offerName} with payload {JsonSerializer.Serialize(offerParameter)}.");
                await _offerParameterService.UpdateAsync(offerName, parameterName, offerParameter);
                return Ok(offerParameter);
            }
            else
            {
                _logger.LogInformation($"Create offer parameter {parameterName} in offer {offerName} with payload {JsonSerializer.Serialize(offerParameter)}.");
                await _offerParameterService.CreateAsync(offerName, offerParameter);
                return CreatedAtRoute(nameof(GetAsync) + nameof(OfferParameter), new { offerName = offerName, parameterName = offerParameter.ParameterName }, offerParameter);
            }

        }

        /// <summary>
        /// Deletes an offerParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the offerParameter to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("offers/{offerName}/offerParameters/{parameterName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string offerName, string parameterName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete offer parameter {parameterName} from offer {offerName}.");
            await _offerParameterService.DeleteAsync(offerName, parameterName);
            return NoContent();
        }
    }
}