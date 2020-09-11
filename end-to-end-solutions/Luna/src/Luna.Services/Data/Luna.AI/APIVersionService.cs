using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Luna.Clients.Azure.Auth;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure.Storage;
using Luna.Clients.GitUtils;
using Luna.Clients.Azure;

namespace Luna.Services.Data.Luna.AI
{
    public class APIVersionService : IAPIVersionService
    {
        private readonly ISqlDbContext _context;
        private readonly IProductService _productService;
        private readonly IDeploymentService _deploymentService;
        private readonly IAMLWorkspaceService _amlWorkspaceService;
        private readonly ILogger<APIVersionService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IStorageUtility _storageUtillity;
        private readonly IGitUtility _gitUtility;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="productService">The service to be injected.</param>
        /// <param name="deploymentService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public APIVersionService(ISqlDbContext sqlDbContext, IProductService productService, IDeploymentService deploymentService, IAMLWorkspaceService amlWorkspaceService,
            ILogger<APIVersionService> logger, IKeyVaultHelper keyVaultHelper, IStorageUtility storageUtility, IGitUtility gitUtility, IOptionsMonitor<AzureConfigurationOption> options)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _amlWorkspaceService = amlWorkspaceService ?? throw new ArgumentNullException(nameof(amlWorkspaceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageUtillity = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
            _gitUtility = gitUtility ?? throw new ArgumentNullException(nameof(gitUtility));
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private string GetUrl(AMLWorkspace workspace, string pipelineId)
        {
            return $"https://{workspace.Region}.api.azureml.ms/pipelines/v1.0" + workspace.ResourceId + $"/PipelineRuns/PipelineSubmit/{pipelineId}";
        }

        private APIVersion UpdateUrl(APIVersion version, AMLWorkspace workspace)
        {
            version.BatchInferenceAPI = string.IsNullOrEmpty(version.BatchInferenceId) ? "" : GetUrl(workspace, version.BatchInferenceId);
            version.TrainModelAPI = string.IsNullOrEmpty(version.TrainModelId) ? "" : GetUrl(workspace, version.TrainModelId);
            version.DeployModelAPI = string.IsNullOrEmpty(version.DeployModelId) ? "" : GetUrl(workspace, version.DeployModelId);

            version.BatchInferenceId = null;
            version.TrainModelId = null;
            version.DeployModelId = null;

            return version;
        }
        private string GetPipelineIdFromUrl(string pipelineId)
        {
            return pipelineId.Substring(pipelineId.LastIndexOf('/') + 1);
        }

        private void SetPipelineIds(APIVersion version)
        {
            version.TrainModelId = string.IsNullOrEmpty(version.TrainModelAPI) ? null : GetPipelineIdFromUrl(version.TrainModelAPI);
            version.BatchInferenceId = string.IsNullOrEmpty(version.BatchInferenceAPI) ? null : GetPipelineIdFromUrl(version.BatchInferenceAPI);
            version.DeployModelId = string.IsNullOrEmpty(version.DeployModelAPI) ? null : GetPipelineIdFromUrl(version.DeployModelAPI);
        }

        /// <summary>
        /// Gets all apiVersions within a product and a deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <returns>A list of apiVersions.</returns>
        public async Task<List<APIVersion>> GetAllAsync(string productName, string deploymentName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(APIVersion).Name));

            // Get the offer associated with the productName and deploymentName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            var product = await _productService.GetAsync(productName);

            // Get all apiVersions with a FK to the deployment
            var apiVersions = await _context.APIVersions.Where(v => v.DeploymentId == deployment.Id).ToListAsync();

            foreach (var apiVersion in apiVersions)
            {
                apiVersion.DeploymentName = deployment.DeploymentName;
                apiVersion.ProductName = product.ProductName;

                //check if there is amlworkspace
                if (apiVersion.AMLWorkspaceId != 0)
                {
                    // Get the amlWorkspace associated with the Id provided
                    var amlWorkspace = await _context.AMLWorkspaces.FindAsync(apiVersion.AMLWorkspaceId);
                    apiVersion.AMLWorkspaceName = amlWorkspace.WorkspaceName;
                    SetPipelineIds(apiVersion);
                }

                if (!string.IsNullOrEmpty(apiVersion.AuthenticationKeySecretName))
                {
                    apiVersion.AuthenticationKey = "notchanged";
                }

                if (!string.IsNullOrEmpty(apiVersion.GitPersonalAccessTokenSecretName))
                {
                    apiVersion.GitPersonalAccessToken = "notchanged";
                }    
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(APIVersion).Name, apiVersions.Count()));

            return apiVersions;
        }

        /// <summary>
        /// Gets an apiVersion within a product and a deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to get.</param>
        /// <param name="versionName">The name of the apiVersion to get.</param>
        /// <returns>The apiVersion.</returns>
        public async Task<APIVersion> GetAsync(string productName, string deploymentName, string versionName)
        {
            // Check that an apiVersion with the provided versionName exists within the given product and deployment
            if (!(await ExistsAsync(productName, deploymentName, versionName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APIVersion).Name,
                        versionName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(APIVersion).Name, versionName));

            // Get the deployment associated with the productName and deploymentName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);

            // Find the apiVersion that matches the productName and deploymentName provided
            var apiVersion = await _context.APIVersions
                .SingleOrDefaultAsync(v => (v.DeploymentId == deployment.Id) && (v.VersionName == versionName));
            apiVersion.DeploymentName = deployment.DeploymentName;

            // Get the product associated with the productName provided
            var product = await _productService.GetAsync(productName);
            apiVersion.ProductName = product.ProductName;

            //check if there is amlworkspace
            if (apiVersion.AMLWorkspaceId != 0)
            {
                // Get the amlWorkspace associated with the Id provided
                var amlWorkspace = await _context.AMLWorkspaces.FindAsync(apiVersion.AMLWorkspaceId);
                apiVersion.AMLWorkspaceName = amlWorkspace.WorkspaceName;
                SetPipelineIds(apiVersion);
            }

            if (!string.IsNullOrEmpty(apiVersion.AuthenticationKeySecretName))
            {
                apiVersion.AuthenticationKey = "notchanged";
            }

            if (apiVersion.VersionSourceType.Equals("git"))
            {
                apiVersion.GitPersonalAccessToken = "notchanged";
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(APIVersion).Name,
                versionName,
                JsonSerializer.Serialize(apiVersion)));

            return apiVersion;
        }

        /// <summary>
        /// Creates an apiVersion within a product and a deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <param name="version">The apiVersion to create.</param>
        /// <returns>The created apiVersion.</returns>
        public async Task<APIVersion> CreateAsync(string productName, string deploymentName, APIVersion version)
        {
            if (version is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the product and the deployment does not already have an apiVersion with the same versionName
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

            // Get the product associated with the productName provided
            var product = await _productService.GetAsync(productName);

            // Get the deployment associated with the productName and the deploymentName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);
            
            // Set the FK to apiVersion
            version.ProductName = product.ProductName;
            version.DeploymentName = deployment.DeploymentName;
            version.DeploymentId = deployment.Id;            

            // Update the apiVersion created time
            version.CreatedTime = DateTime.UtcNow;

            // Update the apiVersion last updated time
            version.LastUpdatedTime = version.CreatedTime;

            //check if amlworkspace is required
            if (!string.IsNullOrEmpty(version.AMLWorkspaceName))
            {
                // Get the amlWorkspace associated with the AMLWorkspaceName provided
                var amlWorkspace = await _amlWorkspaceService.GetWithSecretsAsync(version.AMLWorkspaceName);

                version.AMLWorkspaceName = amlWorkspace.WorkspaceName;
                version.AMLWorkspaceId = amlWorkspace.Id;

                // Update the apiVersion API
                version = UpdateUrl(version, amlWorkspace);
            }
            
            // add athentication key to keyVault if authentication type is key
            if (version.AuthenticationType.Equals("Key", StringComparison.InvariantCultureIgnoreCase))
            {
                if (version.AuthenticationKey == null)
                {
                    throw new LunaBadRequestUserException("Authentication key is needed with the key authentication type", UserErrorCode.AuthKeyNotProvided);
                }
                string secretName = $"authkey-{Context.GetRandomString(12)}";
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, version.AuthenticationKey));
                version.AuthenticationKeySecretName = secretName;
            }

            if (!string.IsNullOrEmpty(version.GitPersonalAccessToken))
            {
                string secretName = $"gitpat-{Context.GetRandomString(12)}";
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, version.GitPersonalAccessToken));
                version.GitPersonalAccessTokenSecretName = secretName;
            }

            if (version.VersionSourceType.Equals("git"))
            {
                if (!await _storageUtillity.ContainerExistsAsync("mlprojects"))
                {
                    await _storageUtillity.CreateContainerAsync("mlprojects");
                }
                string fileName = string.Format(@"{0}/{1}/{2}.zip", version.ProductName, version.DeploymentName, version.VersionName);

                version.ProjectFileUrl = await _gitUtility.DownloadProjectAsZipToAzureStorageAsync(version.GitUrl, version.GitVersion, version.GitPersonalAccessToken, "mlprojects", fileName);
            }

            // Add apiVersion to db
            _context.APIVersions.Add(version);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(APIVersion).Name, version.VersionName));

            return version;
        }

        /// <summary>
        /// Updates an apiVersion within a product and a deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to update.</param>
        /// <param name="versionName">The name of the apiVersion to update.</param>
        /// <param name="version">The updated apiVersion.</param>
        /// <returns>The updated apiVersion.</returns>
        public async Task<APIVersion> UpdateAsync(string productName, string deploymentName, string versionName, APIVersion version)
        {
            if (version is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the versionName has been updated) && 
            //          (an APIVersion with the same new versionName does not already exist)
            if ((versionName != version.VersionName) && (await ExistsAsync(productName, deploymentName, version.VersionName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(APIVersion).Name, versionName, payload: JsonSerializer.Serialize(version)));
            
            // Get the apiVersion that matches the productName, deploymentName and versionName provided
            var versionDb = await GetAsync(productName, deploymentName, versionName);

            if (!versionDb.VersionSourceType.Equals(version.VersionSourceType, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new LunaBadRequestUserException("Cannot change the version source type of an existing API version.", UserErrorCode.InvalidParameter);
            }
            //check if amlworkspace is required
            if (!string.IsNullOrEmpty(version.AMLWorkspaceName))
            {
                // Get the amlWorkspace associated with the AMLWorkspaceName provided
                var amlWorkspace = await _amlWorkspaceService.GetWithSecretsAsync(version.AMLWorkspaceName);

                // Update the apiVersion API
                version = UpdateUrl(version, amlWorkspace);
            }

            // update athentication key to keyVault if authentication type is key
            if (version.AuthenticationType.Equals("Key", StringComparison.InvariantCultureIgnoreCase) 
                && version.AuthenticationKey != null 
                && !version.AuthenticationKey.Equals("notchanged", StringComparison.InvariantCultureIgnoreCase))
            {
                string secretName = string.IsNullOrEmpty(versionDb.AuthenticationKeySecretName) ? $"authkey-{Context.GetRandomString(12)}" : versionDb.AuthenticationKeySecretName;
                
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, version.AuthenticationKey));
                versionDb.AuthenticationKeySecretName = secretName;
            }

            if (!string.IsNullOrEmpty(version.GitPersonalAccessToken))
            {
                string secretName = string.IsNullOrEmpty(versionDb.GitPersonalAccessTokenSecretName) ? $"gitpat-{Context.GetRandomString(12)}" : versionDb.GitPersonalAccessTokenSecretName;
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, version.GitPersonalAccessToken));
                versionDb.GitPersonalAccessTokenSecretName = secretName;
            }

            
            // Copy over the changes
            versionDb.Copy(version);
            versionDb.LastUpdatedTime = DateTime.UtcNow;

            // Update version values and save changes in db
            _context.APIVersions.Update(versionDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(APIVersion).Name, versionName));

            return versionDb;
        }

        /// <summary>
        /// Deletes an apiVersion within a product and a deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <param name="versionName">The name of the apiVersion to delete.</param>
        /// <returns>The deleted apiVersion.</returns>
        public async Task<APIVersion> DeleteAsync(string productName, string deploymentName, string versionName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(APIVersion).Name, versionName));

            // Get the product that matches the productName provided
            var product = await _productService.GetAsync(productName);

            // Get the apiVersion that matches the productName, the deploymentName and the versionName provided
            var version = await GetAsync(productName, deploymentName, versionName);
            version.ProductName = productName;
            version.DeploymentName = deploymentName;

            // delete athentication key from keyVault if authentication type is key
            if (version.AuthenticationType.Equals("Key", StringComparison.InvariantCultureIgnoreCase))
            {
                if (version.AuthenticationKey != null)
                {
                    string secretName = version.AuthenticationKeySecretName;
                    try
                    {
                        await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                    }
                    catch 
                    { 
                    }
                }
            }

            // Remove the apiVersion from the db
            _context.APIVersions.Remove(version);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(APIVersion).Name, versionName));

            return version;
        }

        /// <summary>
        /// Checks if an apiVersion exists within a product and a deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to check exists.</param>
        /// <param name="versionName">The name of the apiVersion to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string productName, string deploymentName, string versionName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(APIVersion).Name, versionName));

            //Get the deployment associated with the productName and the deploymentName provided
            var deployment = await _deploymentService.GetAsync(productName, deploymentName);

            // Check that only one apiVersion with this versionName exists within the deployment
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
