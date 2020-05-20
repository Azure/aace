using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.APIM;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.Models.Controller;
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
        private readonly IAMLWorkspaceService _amlWorkspaceService;
        private readonly IAPISubscriptionService _apiSubscriptionService;
        private readonly ILogger<ProductController> _logger;
        private readonly IUserAPIM _userAPIM;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public APIRoutingController(IAPIVersionService apiVersionService, IAMLWorkspaceService amlWorkspaceService, IAPISubscriptionService apiSubscriptionService,
            ILogger<ProductController> logger,
            IUserAPIM userAPIM)
        {
            _apiVersionService = apiVersionService ?? throw new ArgumentNullException(nameof(apiVersionService));
            _amlWorkspaceService = amlWorkspaceService ?? throw new ArgumentNullException(nameof(amlWorkspaceService));
            _apiSubscriptionService = apiSubscriptionService ?? throw new ArgumentNullException(nameof(apiSubscriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userAPIM = userAPIM ?? throw new ArgumentNullException(nameof(userAPIM));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/predict")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Predict(string productName, string deploymentName, [FromQuery(Name = "api-version")] string versionName, [FromBody] PredictRequest request)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(request.apiSubscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (request.userId != apiSubcription.UserId)
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return this.Content((await ControllerHelper.Predict(version, workspace, request.input)), "application/json");
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/batchinference")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> BatchInference(string productName, string deploymentName, [FromQuery(Name = "api-version")] string versionName, [FromBody] BatchInferenceRequest request)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(request.subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (request.userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.BatchInference(version, workspace, request.input));
        }
    }
}