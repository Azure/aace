using Luna.Clients.Azure;
using Luna.Clients.Azure.APIM;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class DeploymentService : IDeploymentService
    {
        private readonly ISqlDbContext _context;
        private readonly IProductService _productService;
        private readonly ILogger<DeploymentService> _logger;
        private readonly IAPIVersionSetAPIM _apiVersionSetAPIM;
        private readonly IAPIVersionAPIM _apiVersionAPIM;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public DeploymentService(ISqlDbContext sqlDbContext, IProductService productService, ILogger<DeploymentService> logger,
            IAPIVersionSetAPIM apiVersionSetAPIM, IAPIVersionAPIM apiVersionAPIM)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiVersionSetAPIM = apiVersionSetAPIM ?? throw new ArgumentNullException(nameof(apiVersionSetAPIM));
            _apiVersionAPIM = apiVersionAPIM ?? throw new ArgumentNullException(nameof(apiVersionAPIM));
        }
        public async Task<List<Deployment>> GetAllAsync(string productName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Deployment).Name));

            // Get the offer associated with the productName provided
            var product = await _productService.GetAsync(productName);

            // Get all offerParameters with a FK to the offer
            var deployments = await _context.Deployments.Where(d => d.ProductId.Equals(product.Id)).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Deployment).Name, deployments.Count()));

            return deployments;
        }

        public async Task<Deployment> GetAsync(string productName, string deploymentName)
        {
            // Check that an offerParameter with the provided parameterName exists within the given offer
            if (!(await ExistsAsync(productName, deploymentName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Deployment).Name,
                        deploymentName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Deployment).Name, deploymentName));


            // Get the offer associated with the offerName provided
            var product = await _productService.GetAsync(productName);

            // Find the offerParameter that matches the parameterName provided
            var deployment = await _context.Deployments
                .SingleOrDefaultAsync(a => (a.ProductId == product.Id) && (a.DeploymentName == deploymentName));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Deployment).Name,
                deploymentName,
                JsonSerializer.Serialize(deployment)));

            return deployment;
        }

        public async Task<Deployment> CreateAsync(string productName, Deployment deployment)
        {
            if (deployment is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an offerParameter with the same parameterName
            if (await ExistsAsync(productName, deployment.DeploymentName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Deployment).Name,
                    deployment.DeploymentName));
            }

            if (ExpressionEvaluationUtils.ReservedParameterNames.Contains(deployment.DeploymentName))
            {
                throw new LunaConflictUserException($"Parameter {deployment.DeploymentName} is reserved. Please use a different name.");
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Deployment).Name, deployment.DeploymentName, payload: JsonSerializer.Serialize(deployment)));

            // Get the deployment associated with the deploymentName provided
            var product = await _productService.GetAsync(productName);

            // Set the FK to deployment
            deployment.ProductId = product.Id;
            deployment.ProductName = product.ProductName;

            // Update the deployment created time
            deployment.CreatedTime = DateTime.UtcNow;

            // Update the deployment last updated time
            deployment.LastUpdatedTime = deployment.CreatedTime;

            await _apiVersionSetAPIM.CreateAsync(deployment);
            await _apiVersionAPIM.CreateAsync(deployment);

            // Add offerParameter to db
            _context.Deployments.Add(deployment);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Deployment).Name, deployment.DeploymentName));

            return deployment;
        }

        public async Task<Deployment> UpdateAsync(string productName, string deploymentName, Deployment deployment)
        {
            if (deployment is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the parameterName has been updated) && 
            //          (an offerParameter with the same new parameterName does not already exist)
            if ((deploymentName != deployment.DeploymentName) && (await ExistsAsync(productName, deployment.DeploymentName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Deployment).Name, deploymentName, payload: JsonSerializer.Serialize(deployment)));

            // Get the offerParameter that matches the parameterName provided
            var deploymentDB = await GetAsync(productName, deploymentName);

            // Copy over the changes
            deploymentDB.Copy(deployment);

            // Update the product last updated time
            deploymentDB.LastUpdatedTime = DateTime.UtcNow;

            await _apiVersionSetAPIM.UpdateAsync(deploymentDB);
            await _apiVersionAPIM.UpdateAsync(deployment);

            // Update offerParameterDb values and save changes in db
            _context.Deployments.Update(deploymentDB);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Deployment).Name, deploymentName));

            return deploymentDB;
        }

        public async Task<Deployment> DeleteAsync(string productName, string deploymentName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Deployment).Name, deploymentName));

            // Get the offerParameter that matches the parameterName provided
            var deployment = await GetAsync(productName, deploymentName);

            await _apiVersionAPIM.DeleteAsync(deployment);

            // Remove the offerParameter from the db
            _context.Deployments.Remove(deployment);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Deployment).Name, deploymentName));

            return deployment;
        }

        public async Task<bool> ExistsAsync(string productName, string deploymentName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Deployment).Name, deploymentName));

            //Get the offer associated with the offerName provided
            var product = await _productService.GetAsync(productName);

            // Check that only one offerParameter with this parameterName exists within the offer
            var count = await _context.Deployments
                .CountAsync(d => (d.ProductId == product.Id) && (d.DeploymentName == deploymentName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Deployment).Name,
                    deploymentName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Deployment).Name, deploymentName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Deployment).Name, deploymentName, true));
                // count = 1
                return true;
            }
        }
    }
}

