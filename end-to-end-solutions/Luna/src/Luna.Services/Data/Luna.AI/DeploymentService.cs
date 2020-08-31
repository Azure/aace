// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Luna.Clients.Azure.APIM;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.Controller;
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
        /// <param name="productService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="apiVersionSetAPIM">The apim service.</param>
        /// <param name="apiVersionAPIM">The apim service.</param>
        public DeploymentService(ISqlDbContext sqlDbContext, IProductService productService, ILogger<DeploymentService> logger,
            IAPIVersionSetAPIM apiVersionSetAPIM, IAPIVersionAPIM apiVersionAPIM)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiVersionSetAPIM = apiVersionSetAPIM ?? throw new ArgumentNullException(nameof(apiVersionSetAPIM));
            _apiVersionAPIM = apiVersionAPIM ?? throw new ArgumentNullException(nameof(apiVersionAPIM));
        }

        /// <summary>
        /// Gets all deployments within an product.
        /// </summary>
        /// <param name="offerName">The name of the product.</param>
        /// <returns>A list of deployments.</returns>
        public async Task<List<Deployment>> GetAllAsync(string productName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Deployment).Name));

            // Get the product associated with the productName provided
            var product = await _productService.GetAsync(productName);

            // Get all deployments with a FK to the product
            var deployments = await _context.Deployments.Where(d => d.ProductId.Equals(product.Id)).ToListAsync();

            foreach(var deployment in deployments)
            {
                deployment.ProductName = product.ProductName;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Deployment).Name, deployments.Count()));

            return deployments;
        }

        /// <summary>
        /// Gets an deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to get.</param>
        /// <returns>The deployment.</returns>
        public async Task<Deployment> GetAsync(string productName, string deploymentName)
        {
            // Check that an deployment with the provided deploymentName exists within the given product
            if (!(await ExistsAsync(productName, deploymentName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Deployment).Name,
                        deploymentName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Deployment).Name, deploymentName));


            // Get the product associated with the productName provided
            var product = await _productService.GetAsync(productName);

            // Find the deployment that matches the deploymentName provided
            var deployment = await _context.Deployments
                .SingleOrDefaultAsync(a => (a.ProductId == product.Id) && (a.DeploymentName == deploymentName));

            deployment.ProductName = product.ProductName;
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Deployment).Name,
                deploymentName,
                JsonSerializer.Serialize(deployment)));

            return deployment;
        }

        /// <summary>
        /// Creates an deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deployment">The deployment to create.</param>
        /// <returns>The created deployment.</returns>
        public async Task<Deployment> CreateAsync(string productName, Deployment deployment)
        {
            if (deployment is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if DeploymentName is valid
            if (await ControllerHelper.CheckNameValidity(deployment.DeploymentName, nameof(Deployment)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameInvalidErrorMessage(nameof(Deployment), deployment.DeploymentName),
                    UserErrorCode.PayloadNameInvalid);
            }

            // Check that the deployment does not already have an DeploymentName with the same productName
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

            // Get the product associated with the productName provided
            var product = await _productService.GetAsync(productName);

            // Set the FK to product
            deployment.ProductId = product.Id;
            deployment.ProductName = product.ProductName;

            // Update the deployment created time
            deployment.CreatedTime = DateTime.UtcNow;

            // Update the deployment last updated time
            deployment.LastUpdatedTime = deployment.CreatedTime;
            
            // Add deployment to APIM
            await _apiVersionSetAPIM.CreateAsync(deployment);
            await _apiVersionAPIM.CreateAsync(deployment);

            // Add deployment to db
            _context.Deployments.Add(deployment);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Deployment).Name, deployment.DeploymentName));

            return deployment;
        }

        /// <summary>
        /// Updates an deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to update.</param>
        /// <param name="deployment">The updated deployment.</param>
        /// <returns>The updated deployment.</returns>
        public async Task<Deployment> UpdateAsync(string productName, string deploymentName, Deployment deployment)
        {
            if (deployment is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the deploymentName has been updated) && 
            //          (an deployment with the same new productName and deploymentName does not already exist)
            if ((deploymentName != deployment.DeploymentName) && (await ExistsAsync(productName, deployment.DeploymentName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Deployment).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Deployment).Name, deploymentName, payload: JsonSerializer.Serialize(deployment)));

            // Get the deployment that matches the productName and deploymentName provided
            var deploymentDB = await GetAsync(productName, deploymentName);

            // Copy over the changes
            deploymentDB.Copy(deployment);

            // Update the deployment last updated time
            deploymentDB.LastUpdatedTime = DateTime.UtcNow;

            // Update deployment values and save changes in APIM
            await _apiVersionSetAPIM.UpdateAsync(deploymentDB);
            await _apiVersionAPIM.UpdateAsync(deployment);

            // Update deployment values and save changes in db
            _context.Deployments.Update(deploymentDB);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Deployment).Name, deploymentName));

            return deploymentDB;
        }

        /// <summary>
        /// Deletes an deployment within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to delete.</param>
        /// <returns>The deleted deployment.</returns>
        public async Task<Deployment> DeleteAsync(string productName, string deploymentName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Deployment).Name, deploymentName));

            // Get the deployment that matches the productName and deploymentName provided
            var deployment = await GetAsync(productName, deploymentName);

            // Remove the deployment from the APIM
            await _apiVersionAPIM.DeleteAsync(deployment);

            // Remove the deployment from the db
            _context.Deployments.Remove(deployment);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Deployment).Name, deploymentName));

            return deployment;
        }

        /// <summary>
        /// Checks if an deployment exists within an product.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string productName, string deploymentName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Deployment).Name, deploymentName));

            //Get the product associated with the productName provided
            var product = await _productService.GetAsync(productName);

            // Check that only one deployment with this deploymentName exists within the product
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

