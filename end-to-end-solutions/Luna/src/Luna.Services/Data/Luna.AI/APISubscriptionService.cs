// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using Luna.Clients.Azure.APIM;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class APISubscriptionService : IAPISubscriptionService
    {
        private readonly ISqlDbContext _context;
        private readonly IProductService _productService;
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger<APISubscriptionService> _logger;
        private readonly IAPISubscriptionAPIM _apiSubscriptionAPIM;
        private readonly IUserAPIM _userAPIM;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="productService">The service to be injected.</param>
        /// <param name="deploymentService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="apiSubscriptionAPIM">The apim service.</param>
        /// <param name="userAPIM">The apim service.</param>
        public APISubscriptionService(ISqlDbContext sqlDbContext, IProductService productService, IDeploymentService deploymentService, 
            ILogger<APISubscriptionService> logger, IAPISubscriptionAPIM apiSubscriptionAPIM, IUserAPIM userAPIM)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiSubscriptionAPIM = apiSubscriptionAPIM ?? throw new ArgumentNullException(nameof(apiSubscriptionAPIM));
            _userAPIM = userAPIM ?? throw new ArgumentNullException(nameof(userAPIM));
        }

        /// <summary>
        /// Gets all apiSubscriptions.
        /// </summary>
        /// <param name="status">The list of status of the apiSubscriptions.</param>
        /// <param name="owner">The owner of the apiSubscriptions.</param>
        /// <returns>A list of all apiSubscriptions.</returns>
        public async Task<List<APISubscription>> GetAllAsync(string[] status = null, string owner = "")
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(APISubscription).Name));

            // Get all apiSubscriptions
            var subscriptionList = await _context.APISubscriptions.ToListAsync();

            List<APISubscription> apiSubscriptions = subscriptionList.Where(s => (status == null || status.Contains(s.Status)) &&
        (string.IsNullOrEmpty(owner) || s.UserId.Equals(owner, StringComparison.InvariantCultureIgnoreCase))).ToList();


            foreach (var apiSubscription in apiSubscriptions)
            {
                var deployment = await _context.Deployments.FindAsync(apiSubscription.DeploymentId);
                var product = await _context.Products.FindAsync(deployment.ProductId);

                apiSubscription.ProductName = product.ProductName;
                apiSubscription.DeploymentName = deployment.DeploymentName;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(APISubscription).Name, apiSubscriptions.Count()));

            return apiSubscriptions;
        }

        /// <summary>
        /// Get all active apiSubscription by Id
        /// </summary>
        /// <param name="apiSubscriptionId">The apiSubscription Id</param>
        /// <returns>The list of apiSubscription</returns>
        public async Task<APISubscription> GetAsync(Guid apiSubscriptionId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(APISubscription).Name, apiSubscriptionId.ToString()));

            // Find the apiSubscription that matches the subscriptionId provided
            var apiSubscription = await _context.APISubscriptions.SingleOrDefaultAsync(o => (o.SubscriptionId.Equals(apiSubscriptionId)));

            // Check if apiSubscription exists
            if (apiSubscription is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APISubscription).Name,
                    apiSubscriptionId.ToString()));
            }

            var deployment = await _context.Deployments.FindAsync(apiSubscription.DeploymentId);
            // Check if deployment exists
            if (deployment is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APISubscription).Name,
                    apiSubscriptionId.ToString()));
            }
            apiSubscription.DeploymentName = deployment.DeploymentName;

            var product = await _context.Products.FindAsync(deployment.ProductId);
            // Check if product exists
            if (product is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APISubscription).Name,
                    apiSubscriptionId.ToString()));
            }
            apiSubscription.ProductName = product.ProductName;

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(APISubscription).Name,
                apiSubscriptionId.ToString(),
                JsonSerializer.Serialize(apiSubscription)));

            return apiSubscription;
        }

        /// <summary>
        /// Creates a apiSubscription within a product and a deployment.
        /// </summary>
        /// <param name="apiSubscription">The apiSubscription to create.</param>
        /// <returns>The created apiSubscription.</returns>
        public async Task<APISubscription> CreateAsync(APISubscription apiSubscription)
        {
            if (apiSubscription is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APISubscription).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that an apiSubscription with the same Id does not already exist
            if (await ExistsAsync(apiSubscription.SubscriptionId))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(APISubscription).Name,
                        apiSubscription.SubscriptionId.ToString()));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(APISubscription).Name, apiSubscription.SubscriptionId.ToString(), payload: JsonSerializer.Serialize(apiSubscription)));

            apiSubscription.Status = "Subscribed";

            if (apiSubscription.SubscriptionId == null)
            {
                apiSubscription.SubscriptionId = Guid.NewGuid();
            }

            apiSubscription.BaseUrl = _apiSubscriptionAPIM.GetBaseUrl(apiSubscription.ProductName, apiSubscription.DeploymentName);

            var deployment = await _deploymentService.GetAsync(apiSubscription.ProductName, apiSubscription.DeploymentName);
            // Check if deployment exists
            if (deployment is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APISubscription).Name,
                    apiSubscription.SubscriptionId.ToString()));
            }
            apiSubscription.DeploymentId = deployment.Id;
            apiSubscription.DeploymentName = deployment.DeploymentName;

            var product = await _productService.GetAsync(apiSubscription.ProductName);
            // Check if product exists
            if (product is null || product.Id != deployment.ProductId)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APISubscription).Name,
                    apiSubscription.SubscriptionId.ToString()));
            }
            apiSubscription.ProductName = product.ProductName;

            // Update the apiSubscription created time
            apiSubscription.CreatedTime = DateTime.UtcNow;

            // Update the apiSubscription last updated time
            apiSubscription.LastUpdatedTime = apiSubscription.CreatedTime;

            // Add apiSubscription to APIM
            await _userAPIM.CreateAsync(apiSubscription.UserId);
            var apiSubscriptionAPIM = await _apiSubscriptionAPIM.CreateAsync(apiSubscription);

            // Update the apiSubscription primary key and secondary key
            apiSubscription.PrimaryKey = apiSubscriptionAPIM.properties.primaryKey;
            apiSubscription.SecondaryKey = apiSubscriptionAPIM.properties.secondaryKey;

            // Add apiSubscription to db
            _context.APISubscriptions.Add(apiSubscription);
            await _context._SaveChangesAsync();
            
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(APISubscription).Name, apiSubscription.SubscriptionId.ToString()));

            return apiSubscription;
        }

        /// <summary>
        /// Updates a apiSubscription.
        /// </summary>
        /// <param name="apiSubscriptionId">The id of the apiSubscription to delete.</param>
        /// <param name="apiSubscription">The updated apiSubscription.</param>
        /// <returns>The updated apiSubscriptionId.</returns>
        public async Task<APISubscription> UpdateAsync(Guid apiSubscriptionId, APISubscription apiSubscription)
        {
            if (apiSubscription is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APISubscription).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(APISubscription).Name, apiSubscription.SubscriptionId.ToString(), payload: JsonSerializer.Serialize(apiSubscription)));

            // Get the apiSubscription that matches the apiSubscriptionId provided
            var apiSubscriptionDb = await GetAsync(apiSubscriptionId);
            if (!string.IsNullOrEmpty(apiSubscription.UserId) && !apiSubscriptionDb.UserId.Equals(apiSubscription.UserId, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new LunaBadRequestUserException("Owner name of an existing apiSubscription can not be changed.", UserErrorCode.InvalidParameter);
            }
            // Check if (the apiSubscription has been updated) && 
            //          (an apiSubscription with the same new Id does not already exist)
            if ((!apiSubscriptionId.Equals(apiSubscription.SubscriptionId)) && (await ExistsAsync(apiSubscription.SubscriptionId)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APISubscription).Name),
                    UserErrorCode.NameMismatch);
            }

            // Copy over the changes
            apiSubscriptionDb.Copy(apiSubscription);

            // Update the apiSubscription last updated time
            apiSubscriptionDb.LastUpdatedTime = DateTime.UtcNow;

            // Update apiSubscription values and save changes in APIM
            await _userAPIM.CreateAsync(apiSubscription.UserId);
            var apiSubscriptionAPIM = await _apiSubscriptionAPIM.UpdateAsync(apiSubscriptionDb);

            // Update the apiSubscription primary key and secondry key
            apiSubscriptionDb.PrimaryKey = apiSubscriptionAPIM.properties.primaryKey;
            apiSubscriptionDb.SecondaryKey = apiSubscriptionAPIM.properties.secondaryKey;

            // Update apiSubscription values and save changes in db
            _context.APISubscriptions.Update(apiSubscriptionDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(APISubscription).Name, apiSubscription.SubscriptionId.ToString()));

            return apiSubscriptionDb;
        }

        /// <summary>
        /// Delete a apiSubscription.
        /// </summary>
        /// <param name="apiSubscriptionId">The id of the apiSubscription to delete.</param>
        /// <returns>The subscription.</returns>
        public async Task<APISubscription> DeleteAsync(Guid apiSubscriptionId)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Product).Name, apiSubscriptionId.ToString()));

            // Get the offer that matches the offerName provide
            var apiSubscription = await GetAsync(apiSubscriptionId);

            // remove the product from the APIM
            await _apiSubscriptionAPIM.DeleteAsync(apiSubscription);

            // Remove the product from the db
            _context.APISubscriptions.Remove(apiSubscription);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Product).Name, apiSubscriptionId.ToString()));

            return apiSubscription;
        }

        /// <summary>
        /// Regenerate key for the apiSubscription
        /// </summary>
        /// <param name="apiSubscriptionId">apiSubscription id</param>
        /// <param name="keyName">The key name</param>
        /// <returns>The apiSubscription with regenerated key</returns>
        public async Task<APISubscription> RegenerateKey(Guid apiSubscriptionId, string keyName)
        {
            if (!await ExistsAsync(apiSubscriptionId))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APISubscription).Name,
                    apiSubscriptionId.ToString()));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(APISubscription).Name, apiSubscriptionId.ToString()));

            // Get the apiSubscription that matches the provided apiSubscriptionId
            var apiSubscription = await _context.APISubscriptions.SingleOrDefaultAsync(o => (o.SubscriptionId.Equals(apiSubscriptionId)));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(APISubscription).Name,
               apiSubscriptionId.ToString(),
               JsonSerializer.Serialize(apiSubscription)));

            // Update apiSubscription primary key and secondary key and save changes in APIM
            var apiSubscriptionProperties = await _apiSubscriptionAPIM.RegenerateKey(apiSubscriptionId, keyName);
            apiSubscription.PrimaryKey = apiSubscriptionProperties.primaryKey;
            apiSubscription.SecondaryKey = apiSubscriptionProperties.secondaryKey;

            // Update apiSubscription primary key and secondary key and save changes in db
            _context.APISubscriptions.Update(apiSubscription);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(APISubscription).Name, apiSubscription.SubscriptionId.ToString()));

            return apiSubscription;
        }

        public async Task<bool> ExistsAsync(Guid apiSubscriptionId)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(APISubscription).Name, apiSubscriptionId.ToString()));

            // Check that only one apiSubscription with this apiSubscriptionId exists and has not been deleted
            var count = await _context.APISubscriptions
                .CountAsync(s => (s.SubscriptionId.Equals(apiSubscriptionId)));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(APISubscription).Name, apiSubscriptionId.ToString()));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(APISubscription).Name, apiSubscriptionId.ToString(), false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Product).Name, apiSubscriptionId.ToString(), true));
                // count = 1
                return true;
            }
        }        

    }
}
