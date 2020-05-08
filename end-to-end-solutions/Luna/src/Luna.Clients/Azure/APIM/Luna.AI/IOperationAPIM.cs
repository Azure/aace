using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IOperationAPIM
    {
        public string GetAPIMRESTAPIPath(string type, string versionName);
        public Task CreateAsync(string type, APIVersion version);
        public Task UpdateAsync(string type, APIVersion version);
        public Task DeleteAsync(string type, APIVersion version);
    }
}
