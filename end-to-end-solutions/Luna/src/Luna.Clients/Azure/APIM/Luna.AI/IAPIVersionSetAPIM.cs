using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IAPIVersionSetAPIM
    {
        public string GetAPIMRESTAPIPath(string deploymentName);
        public Task CreateAsync(Deployment deployment);
        public Task UpdateAsync(Deployment deployment);
        public Task DeleteAsync(Deployment deployment);
        
    }
}
