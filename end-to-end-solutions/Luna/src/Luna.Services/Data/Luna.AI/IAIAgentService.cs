using Luna.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public interface IAIAgentService
    {
        Task<List<AIAgent>> GetAllAsync();

        Task<AIAgent> GetSaaSAgentAsync();

        Task<AIAgent> GetAsync(Guid agentId);

        Task<AIAgent> CreateAsync(Guid agentId, AIAgent agent);

        Task<AIAgent> UpdateAsync(Guid agentId, AIAgent agent);

        Task DeleteAsync(Guid agentId);

        Task<bool> ExistsAsync(Guid agentId);

        Task<List<AgentSubscription>> GetAllSubscriptionsByAgentIdAsync(Guid agentId);

        Task<List<AgentAPIVersion>> GetAllAPIVersionByAgentIdAsync(Guid agentId);

        Task<string> GetProjectFileSaSUrlAsync(Guid agentId, Guid subscriptionId, string versionName);

        Task ValidateAgentKey(Guid agentId, string key);
    }
}
