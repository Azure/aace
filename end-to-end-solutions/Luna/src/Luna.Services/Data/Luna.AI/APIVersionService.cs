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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class APIVersionService : IAPIVersionService
    {
        private readonly ISqlDbContext _context;
        private readonly IProductService _productService;
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger<APIVersionService> _logger;
        private readonly IAPIVersionAPIM _apiVersionAPIM;
        private readonly IProductAPIVersionAPIM _productAPIVersionAPIM;
        private readonly IOperationAPIM _operationAPIM;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public APIVersionService(ISqlDbContext sqlDbContext, IProductService productService, IDeploymentService deploymentService, 
            ILogger<APIVersionService> logger, 
            IAPIVersionAPIM apiVersionAPIM, IProductAPIVersionAPIM productAPIVersionAPIM, IOperationAPIM operationAPIM)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiVersionAPIM = apiVersionAPIM ?? throw new ArgumentNullException(nameof(apiVersionAPIM));
            _productAPIVersionAPIM = productAPIVersionAPIM ?? throw new ArgumentNullException(nameof(productAPIVersionAPIM));
            _operationAPIM = operationAPIM ?? throw new ArgumentNullException(nameof(operationAPIM));
        }

        public async Task<List<APIVersion>> GetAllAsync(string productName, string deploymentName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(APIVersion).Name));

            // Get the offer associated with the offerName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);

            // Get all offerParameters with a FK to the offer
            var apiVersions = await _context.APIVersions.Where(v => v.DeploymentId == deployment.Id).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(APIVersion).Name, apiVersions.Count()));

            return apiVersions;
        }

        public async Task<APIVersion> GetAsync(string productName, string deploymentName, string versionName)
        {
            // Check that an offerParameter with the provided parameterName exists within the given offer
            if (!(await ExistsAsync(productName, deploymentName, versionName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APIVersion).Name,
                        versionName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(APIVersion).Name, versionName));


            // Get the offer associated with the offerName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);

            // Find the offerParameter that matches the parameterName provided
            var apiVersion = await _context.APIVersions
                .SingleOrDefaultAsync(v => (v.DeploymentId == deployment.Id) && (v.VersionName == versionName));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(APIVersion).Name,
                versionName,
                JsonSerializer.Serialize(apiVersion)));

            return apiVersion;
        }

        public async Task<APIVersion> CreateAsync(string productName, string deploymentName, APIVersion version)
        {
            if (version is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an offerParameter with the same parameterName
            if (await ExistsAsync(productName, deploymentName, version.VersionName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(APIVersion).Name,
                    version.VersionName));
            }

            if (ExpressionEvaluationUtils.ReservedParameterNames.Contains(version.VersionName))
            {
                throw new LunaConflictUserException($"Parameter {version.VersionName} is reserved. Please use a different name.");
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(APIVersion).Name, version.VersionName, payload: JsonSerializer.Serialize(version)));

            // Get the offer associated with the offerName provided
            var product = await _productService.GetAsync(productName);
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);

            // Set the FK to apiVersion
            version.ProductName = product.ProductName;
            version.DeploymentName = deployment.DeploymentName;
            version.DeploymentId = deployment.Id;

            // Add apiVersion to APIM
            await _apiVersionAPIM.CreateAsync(product.ProductType, version);
            await _productAPIVersionAPIM.CreateAsync(product.ProductType, version);
            await _operationAPIM.CreateAsync(product.ProductType, version);

            // Add apiVersion to db
            try
            {
                _context.APIVersions.Add(version);
                await _context._SaveChangesAsync();
            }
            catch (Exception ex)
            { 
            }
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(APIVersion).Name, version.VersionName));

            return version;
        }

        public async Task<APIVersion> UpdateAsync(string productName, string deploymentName, string versionName, APIVersion version)
        {
            if (version is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the parameterName has been updated) && 
            //          (an offerParameter with the same new parameterName does not already exist)
            if ((versionName != version.VersionName) && (await ExistsAsync(productName, deploymentName, version.VersionName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(APIVersion).Name, versionName, payload: JsonSerializer.Serialize(version)));

            // Get the offerParameter that matches the parameterName provided
            var versionDb = await GetAsync(productName, deploymentName, versionName);

            // Copy over the changes
            versionDb.Copy(version);

            var product = await _productService.GetAsync(productName);

            // Add deployment to APIM
            await _apiVersionAPIM.UpdateAsync(product.ProductType, versionDb);
            await _productAPIVersionAPIM.UpdateAsync(product.ProductType, versionDb);
            await _operationAPIM.UpdateAsync(product.ProductType, versionDb);

            // Update offerParameterDb values and save changes in db
            _context.APIVersions.Update(versionDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(APIVersion).Name, versionName));

            return versionDb;
        }

        public async Task<APIVersion> DeleteAsync(string productName, string deploymentName, string versionName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(APIVersion).Name, versionName));

            // Get the offerParameter that matches the parameterName provided
            var product = await _productService.GetAsync(productName);
            var version = await GetAsync(productName, deploymentName, versionName);
            version.ProductName = productName;
            version.DeploymentName = deploymentName;

            // Add version to APIM
            await _operationAPIM.DeleteAsync(product.ProductType, version);
            await _productAPIVersionAPIM.DeleteAsync(product.ProductType, version);
            await _apiVersionAPIM.DeleteAsync(product.ProductType, version);

            // Remove the offerParameter from the db
            _context.APIVersions.Remove(version);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(APIVersion).Name, versionName));

            return version;
        }

        public async Task<bool> ExistsAsync(string productName, string deploymentName, string versionName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(APIVersion).Name, versionName));

            //Get the offer associated with the offerName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);

            // Check that only one offerParameter with this parameterName exists within the offer
            var count = await _context.APIVersions
                .CountAsync(a => (a.DeploymentId.Equals(deployment.Id)) && (a.VersionName == versionName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(APIVersion).Name,
                    versionName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(APIVersion).Name, versionName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(APIVersion).Name, versionName, true));
                // count = 1
                return true;
            }
        }
    }
}
