using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IProductAPIVersionAPIM
    {
        public string GetAPIMRESTAPIPath(string productName, string deploymentName);


        public Task CreateAsync(string type, APIVersion version);


        public Task UpdateAsync(string type, APIVersion version);


        public Task DeleteAsync(string type, APIVersion version);
        
    }
}
