using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
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
    /// API controller for product resource.
    /// </summary>
    // [Authorize]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class APIRoutingController : ControllerBase
    {
        private readonly IAPIVersionService _apiVersionService;
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public APIRoutingController(IAPIVersionService apiVersionService, ILogger<ProductController> logger)
        {
            _apiVersionService = apiVersionService ?? throw new ArgumentNullException(nameof(logger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/predict")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Predict(string productName, string deploymentName, [FromQuery(Name = "api-version")] string versionName, [FromBody] object body)
        {
            _logger.LogInformation($"Get all apiVersions in deployment {deploymentName} in product {productName}.");
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            
            return this.Content((await ControllerHelper.Predict(version, body)), "application/json");
        }
    }
}