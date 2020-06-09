using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Luna.Clients.Azure.APIM;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.Models.Controller;
using Luna.Services.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            var apiSubcription = await _apiSubscriptionService.GetAsync(request.subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (request.userId != _userAPIM.GetUserName(apiSubcription.UserId))
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
        public async Task<ActionResult> BatchInferenceWithDefaultModel(string productName, string deploymentName, [FromQuery(Name = "api-version")] string versionName, [FromBody] BatchInferenceRequest request)
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

            return Ok(await ControllerHelper.BatchInferenceWithDefaultModel(version, workspace, request.input));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/train")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> TrainModel(string productName, string deploymentName, [FromQuery(Name = "api-version")] string versionName, [FromBody] TrainModelRequest request)
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

            return Ok(await ControllerHelper.TrainModel(version, workspace, request.input));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/models/{modelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAModel(string productName, string deploymentName, Guid subscriptionId, Guid modelId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetAModel(workspace, modelId, userId));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/models")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllModels(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetAllModels(workspace));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/models/{modelId}/batchinference")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> BatchInference(string productName, string deploymentName, Guid modelId, [FromQuery(Name = "api-version")] string versionName, [FromBody] BatchInferenceRequest request)
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

            return Ok(await ControllerHelper.BatchInference(version, workspace, modelId, request.input));
        }


        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/{operationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetABatchInferenceOperation(string productName, string deploymentName, Guid subscriptionId, Guid operationId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok((await ControllerHelper.GetABatchInferenceOperation(workspace, operationId)));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllBatchInferenceOperations(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok((await ControllerHelper.GetAllBatchInferenceOperations(workspace)));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/models/{model_id}/deploy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeployRealTimePredictionEndpoint(string productName, string deploymentName, Guid modelId, [FromQuery(Name = "api-version")] string versionName, [FromBody] DeployRealTimePredictionEndpointRequest request)
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

            return Ok(await ControllerHelper.DeployRealTimePredictionEndpoint(version, workspace, modelId, request.input));
        }


        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/endpoints/{deploymentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetADeployedEndpoint(string productName, string deploymentName, Guid subscriptionId, Guid deploymentId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok((await ControllerHelper.GetADeployedEndpoint(workspace, deploymentId)));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/endpoints")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllDeployedEndpoints(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok((await ControllerHelper.GetAllDeployedEndpoints(workspace)));
        }
    }
}