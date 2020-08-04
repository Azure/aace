using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.APIM;
using Luna.Services.Utilities.ExpressionEvaluation;

namespace Luna.Services.Data.Luna.AI
{
    public class AMLWorkspaceService : IAMLWorkspaceService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AMLWorkspaceService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IOptionsMonitor<APIMConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public AMLWorkspaceService(IOptionsMonitor<APIMConfigurationOption> options,
            ISqlDbContext sqlDbContext, ILogger<AMLWorkspaceService> logger, IKeyVaultHelper keyVaultHelper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper;
        }

        public async Task<List<AMLWorkspace>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AMLWorkspace).Name));

            // Get all products
            var workspaces = await _context.AMLWorkspaces.ToListAsync();
            foreach (var workspace in workspaces)
            {
                workspace.AADApplicationSecrets = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspace.AADApplicationSecretName);
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AMLWorkspace).Name, workspaces.Count()));

            return workspaces;
        }

        public async Task<AMLWorkspace> GetAsync(string workspaceName)
        {
            if (!await ExistsAsync(workspaceName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Product).Name,
                    workspaceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Product).Name, workspaceName));

            // Get the product that matches the provided productName
            var workspace = await _context.AMLWorkspaces.SingleOrDefaultAsync(o => (o.WorkspaceName == workspaceName));

            workspace.AADApplicationSecrets = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspace.AADApplicationSecretName);
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Product).Name,
               workspaceName,
               JsonSerializer.Serialize(workspace)));

            return workspace;
        }

        public async Task<bool> ExistsAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AMLWorkspace).Name, workspaceName));

            // Check that only one offer with this offerName exists and has not been deleted
            var count = await _context.AMLWorkspaces
                .CountAsync(p => (p.WorkspaceName == workspaceName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AMLWorkspace).Name, workspaceName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AMLWorkspace).Name, workspaceName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AMLWorkspace).Name, workspaceName, true));
                // count = 1
                return true;
            }
        }

        public async Task<AMLWorkspace> CreateAsync(AMLWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that an offer with the same name does not already exist
            if (await ExistsAsync(workspace.WorkspaceName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AMLWorkspace).Name,
                        workspace.WorkspaceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AMLWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            workspace.Region = await ControllerHelper.GetRegion(workspace);

            // Add secret to keyvault
            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the aml workspace", UserErrorCode.AuthKeyNotProvided);
            }
            string secretName = $"amlkey-{Context.GetRandomString(12)}";
            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, workspace.AADApplicationSecrets));

            // Add workspace to db
            workspace.AADApplicationSecretName = secretName;
            _context.AMLWorkspaces.Add(workspace);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AMLWorkspace).Name, workspace.WorkspaceName));

            return workspace;
        }

        public async Task<AMLWorkspace> UpdateAsync(string workspaceName, AMLWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Product).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            // Get the offer that matches the offerName provided
            var workspaceDb = await GetAsync(workspaceName);

            // Check if (the offerName has been updated) && 
            //          (an offer with the same new name does not already exist)
            if ((workspaceName != workspace.WorkspaceName) && (await ExistsAsync(workspace.WorkspaceName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.NameMismatch);
            }

            // Add secret to keyvault
            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the aml workspace", UserErrorCode.ArmTemplateNotProvided);
            }
            string secretName = string.IsNullOrEmpty(workspace.AADApplicationSecretName) ? $"amlkey-{Context.GetRandomString(12)}" : workspace.AADApplicationSecretName;
            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, workspace.AADApplicationSecrets));

            // Copy over the changes
            workspace.Region = await ControllerHelper.GetRegion(workspace);
            workspaceDb.Copy(workspace);

            // Update workspaceDb values and save changes in db
            _context.AMLWorkspaces.Update(workspaceDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Product).Name, workspace.WorkspaceName));

            return workspaceDb;
        }

        public async Task<AMLWorkspace> DeleteAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AMLWorkspace).Name, workspaceName));

            var workspace = await GetAsync(workspaceName);

            // Delete secret from key vault
            if (!string.IsNullOrEmpty(workspace.AADApplicationSecretName))
            {
                string secretName = workspace.AADApplicationSecretName;
                try
                {
                    await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                }
                catch { }
            }

            // Remove the workspace from the db
            _context.AMLWorkspaces.Remove(workspace);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AMLWorkspace).Name, workspaceName));

            return workspace;
        }
    }
}
