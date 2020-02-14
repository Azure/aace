using Luna.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public interface IArmTemplateArmTemplateParameterService
    {
        Task CreateJoinEntryAsync(long armTemplateId, long armTemplateParameterId);
        Task DeleteArmTemplateJoinEntriesAsync(long armTemplateId);
        Task<bool> ParameterExistsInDifferentArmTemplates(long armTemplateId, long armTemplateParameterId);
        Task<List<ArmTemplateArmTemplateParameter>> GetAllJoinEntries(long armTemplateId);
    }
}