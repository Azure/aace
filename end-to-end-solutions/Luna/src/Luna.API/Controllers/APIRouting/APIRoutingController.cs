using System;
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
        private readonly IProductService _productService;
        private readonly IDeploymentService _deploymentService;
        private readonly IAPIVersionService _apiVersionService;
        private readonly IAMLWorkspaceService _amlWorkspaceService;
        private readonly IAPISubscriptionService _apiSubscriptionService;
        private readonly ILogger<ProductController> _logger;
        private readonly IUserAPIM _userAPIM;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public APIRoutingController(IProductService productService, IDeploymentService deploymentService, IAPIVersionService apiVersionService, IAMLWorkspaceService amlWorkspaceService, IAPISubscriptionService apiSubscriptionService,
            ILogger<ProductController> logger,
            IUserAPIM userAPIM)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _apiVersionService = apiVersionService ?? throw new ArgumentNullException(nameof(apiVersionService));
            _amlWorkspaceService = amlWorkspaceService ?? throw new ArgumentNullException(nameof(amlWorkspaceService));
            _apiSubscriptionService = apiSubscriptionService ?? throw new ArgumentNullException(nameof(apiSubscriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userAPIM = userAPIM ?? throw new ArgumentNullException(nameof(userAPIM));
        }

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

            return this.Content((await ControllerHelper.Predict(version, workspace, request.userInput)), "application/json");
        }

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

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.BatchInferenceWithDefaultModel(product, deployment, version, workspace, apiSubcription, request.userInput));
        }

        [HttpGet("products/{productName}/deployments/{deploymentName}/operations/{operationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetABatchInferenceOperationWithDefaultModel(string productName, string deploymentName, Guid operationId, [FromQuery(Name = "api-version")] string versionName, [FromBody] BatchInferenceRequest request)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(request.subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (request.userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetABatchInferenceOperationWithDefaultModel(product, deployment, version, workspace, apiSubcription, operationId));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/operations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ListAllInferenceOperationsByUserWithDefaultModel(string productName, string deploymentName, [FromQuery(Name = "api-version")] string versionName, [FromBody] BatchInferenceRequest request)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(request.subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (request.userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.ListAllInferenceOperationsByUserWithDefaultModel(product, deployment, version, workspace, apiSubcription));
        }

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

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.TrainModel(product, deployment, version, workspace, apiSubcription, request.userInput));
        }

        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/training/{modelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetATrainingOperationsByModelIdUser(string productName, string deploymentName, Guid subscriptionId, Guid modelId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);
            

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetATrainingOperationsByModelIdUser(product, deployment, version, workspace, apiSubcription, modelId));
        }

        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/training")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ListAllTrainingOperationsByUser(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.ListAllTrainingOperationsByUser(product, deployment, version, workspace, apiSubcription));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/models/{modelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAModelByModelIdUserProductDeployment(string productName, string deploymentName, Guid subscriptionId, Guid modelId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetAModelByModelIdUserProductDeployment(product, deployment, version, workspace, apiSubcription, modelId));
        }

        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/models")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllModelsByUserProductDeployment(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetAllModelsByUserProductDeployment(product, deployment, version, workspace, apiSubcription));
        }

        [HttpDelete("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/models/{modelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteAModel(string productName, string deploymentName, Guid subscriptionId, Guid modelId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
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
            await ControllerHelper.DeleteAModel(workspace, modelId);

            return Ok();
        }

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

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.BatchInference(product, deployment, version, workspace, apiSubcription, modelId, request.userInput));
        }

        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/inference/{operationId}")]
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

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetABatchInferenceOperation(product, deployment, version, workspace, apiSubcription, operationId));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/inference")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ListAllInferenceOperationsByUser(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.ListAllInferenceOperationsByUser(product, deployment, version, workspace, apiSubcription));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpPost("products/{productName}/deployments/{deploymentName}/models/{modelId}/deploy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeployRealTimePredictionEndpoint(string productName, string deploymentName, Guid modelId, [FromQuery(Name = "api-version")] string versionName, [FromBody] BatchInferenceRequest request)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(request.subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (request.userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.DeployRealTimePredictionEndpoint(product, deployment, version, workspace, apiSubcription, modelId, request.userInput));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/deployment/{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetADeployOperationByEndpointIdUser(string productName, string deploymentName, Guid subscriptionId, Guid endpointId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetADeployOperationByEndpointIdUser(product, deployment, version, workspace, apiSubcription, endpointId));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/operations/deployment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ListAllDeployOperationsByUser(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.ListAllDeployOperationsByUser(product, deployment, version, workspace, apiSubcription));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/endpoints")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllRealTimeServiceEndpointsByUserProductAndDeployment(string productName, string deploymentName, Guid subscriptionId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetAllRealTimeServiceEndpointsByUserProductDeployment(product, deployment, version, workspace, apiSubcription));
        }

        /// <summary>
        /// Gets all apiVersions within a deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>HTTP 200 OK with apiVersion JSON objects in response body.</returns>
        [HttpGet("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/endpoints/{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetARealTimeServiceEndpointByEndpointIdUserProductAndDeployment(string productName, string deploymentName, Guid subscriptionId, Guid endpointId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
        {
            var apiSubcription = await _apiSubscriptionService.GetAsync(subscriptionId);
            if (apiSubcription == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiSubcription)), UserErrorCode.PayloadNotProvided);
            }
            if (userId != _userAPIM.GetUserName(apiSubcription.UserId))
                throw new LunaBadRequestUserException("UserId of request is not equal to apiSubscription.", UserErrorCode.InvalidParameter);

            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var version = await _apiVersionService.GetAsync(productName, deploymentName, versionName);
            var workspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);

            return Ok(await ControllerHelper.GetARealTimeServiceEndpointByEndpointIdUserProductDeployment(product, deployment, version, workspace, apiSubcription, endpointId));
        }


        [HttpDelete("products/{productName}/deployments/{deploymentName}/subscriptions/{subscriptionId}/endpoints/{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteAEndpoint(string productName, string deploymentName, Guid subscriptionId, Guid endpointId, [FromQuery(Name = "userid")] string userId, [FromQuery(Name = "api-version")] string versionName)
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
            await ControllerHelper.DeleteAEndpoint(workspace, endpointId);

            return Ok();
        }
    }
}