using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IProductAPIVersionAPIM
    {
        string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName);
        Task<bool> ExistsAsync(string type, APIVersion version);
        Task CreateAsync(string type, APIVersion version);
        Task UpdateAsync(string type, APIVersion version);
        Task DeleteAsync(string type, APIVersion version);
    }
}
