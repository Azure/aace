using Luna.Clients.Azure.APIM;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Azure.Storage;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public class AIAgentService : IAIAgentService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AIAgentService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IOptionsMonitor<APIMConfigurationOption> _options;
        private readonly IStorageUtility _storageUtility;

        public AIAgentService(IOptionsMonitor<APIMConfigurationOption> options,
            ISqlDbContext sqlDbContext, ILogger<AIAgentService> logger, IKeyVaultHelper keyVaultHelper, IStorageUtility storageUtility)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _storageUtility = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
        }

        public async Task<AIAgent> CreateAsync(Guid agentId, AIAgent agent)
        {
            if (agent is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AIAgent).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (await ExistsAsync(agent.AgentId))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AIAgent).Name,
                        agent.AgentId.ToString()));
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AIAgent).Name, agent.AgentId.ToString()));

            if (agent.AgentKey == null)
            {
                throw new LunaBadRequestUserException("Agent key is required.", UserErrorCode.AuthKeyNotProvided);
            }

            string secretName = $"agentkey-{Context.GetRandomString(12)}";
            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, agent.AgentKey));

            agent.AgentKeySecretName = secretName;
            try
            {
                _context.AIAgents.Add(agent);
                await _context._SaveChangesAsync();
                _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AIAgent).Name, agent.AgentId.ToString()));
                return agent;
            }
            catch(Exception e)
            {
                _logger.LogWarning("Failed to create AIAgent. Trying to delete the agent key secret.");
                await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                _logger.LogWarning("Failed to create AIAgent. Agent key secret deleted");
                throw e;
            }

        }

        public async Task DeleteAsync(Guid agentId)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AIAgent).Name, agentId.ToString()));

            var agent = await GetAsync(agentId);

            // Remove the agent from the db
            _context.AIAgents.Remove(agent);
            await _context._SaveChangesAsync();

            // Delete secret from key vault
            if (!string.IsNullOrEmpty(agent.AgentKeySecretName))
            {
                string secretName = agent.AgentKeySecretName;
                // Try to delete the secret. Log a warning message if the deletion failed instead of failing the whole operation.
                try
                {
                    await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Cannot delete the secret with name {secretName} from key vault {_options.CurrentValue.Config.VaultName}. Error {e.Message}");
                }
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AIAgent).Name, agentId.ToString()));

            return;
        }

        public async Task<bool> ExistsAsync(Guid agentId)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AIAgent).Name, agentId.ToString()));

            var count = await _context.AIAgents
                .CountAsync(p => (p.AgentId == agentId));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AIAgent).Name, agentId.ToString()));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AIAgent).Name, agentId.ToString(), false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AIAgent).Name, agentId.ToString(), true));
                // count = 1
                return true;
            }
        }

        public async Task<List<AIAgent>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AIAgent).Name));

            // Get all products
            var agents = await _context.AIAgents.ToListAsync();
            foreach (var agent in agents)
            {
                agent.AgentKey = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, agent.AgentKeySecretName);
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AIAgent).Name, agents.Count()));

            return agents;
        }

        public async Task<AIAgent> GetAsync(Guid agentId)
        {
            if (!await ExistsAsync(agentId))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AIAgent).Name,
                    agentId.ToString()));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AIAgent).Name, agentId.ToString()));

            var agent = await _context.AIAgents.SingleOrDefaultAsync(o => (o.AgentId == agentId));

            agent.AgentKey = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, agent.AgentKeySecretName);
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AIAgent).Name,
               agentId.ToString()));

            return agent;
        }
        public async Task<AIAgent> UpdateAsync(Guid agentId, AIAgent agent)
        {
            if (agent is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AIAgent).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AIAgent).Name, agentId.ToString()));

            // The only information can be updated in an AIAgent is the key. We don't need to update the database record
            // TODO: disable the old secret

            var dbAgent = await _context.AIAgents.SingleOrDefaultAsync(o => (o.AgentId == agentId));
            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, dbAgent.AgentKeySecretName, agent.AgentKey));

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AIAgent).Name, agentId.ToString()));

            return agent;

        }

        public async Task<List<AgentSubscription>> GetAllSubscriptionsByAgentIdAsync(Guid agentId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AgentSubscription).Name));

            // Get all subscriptions by agent id
            var subs = await _context.AgentSubscriptions.Where(sub => (sub.AgentId == agentId)).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AgentSubscription).Name, subs.Count()));

            return subs;
        }

        public async Task<List<AgentAPIVersion>> GetAllAPIVersionByAgentIdAsync(Guid agentId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AgentAPIVersion).Name));

            // Get all API versions by agent id
            var versions = await _context.AgentAPIVersions.Where(ver => (ver.AgentId == agentId)).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AgentAPIVersion).Name, versions.Count()));

            return versions;
        }

        public async Task<string> GetProjectFileSaSUrlAsync(Guid agentId, Guid subscriptionId, string versionName)
        {
            _logger.LogInformation($"Get project file SaS url for AI Agent {agentId} and subscription {subscriptionId}");

            var version = await _context.AgentAPIVersions.SingleOrDefaultAsync(ver => (ver.AgentId == agentId && ver.SubscriptionId == subscriptionId && ver.VersionName == versionName));

            if (version == null)
            {
                throw new LunaNotFoundUserException("Cannot find the project file.");
            }

            string url = await _storageUtility.GetFileReferenceWithSasKeyAsync(version.ProjectFileUrl, readOnly: true);

            _logger.LogInformation($"Returning readonly SaS url for project file {version.ProjectFileUrl}.");

            return url;
        }

        public async Task ValidateAgentKey(Guid agentId, string key)
        {
            _logger.LogInformation($"Validate agent key for agent {agentId.ToString()}");
            var agent = await GetAsync(agentId);
            var secret = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, agent.AgentKeySecretName);
            if (!secret.Equals(key, StringComparison.InvariantCulture))
            {
                throw new LunaUnauthorizedUserException($"The agent key for agent {agentId.ToString()} is invalid.");
            }            
        }
    }
}
