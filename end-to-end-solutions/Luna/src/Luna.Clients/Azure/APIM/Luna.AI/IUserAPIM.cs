using System.Threading.Tasks;
using Luna.Clients.Models.Azure;

namespace Luna.Clients.Azure
{
    public interface IUserAPIM
    {
        public string GetAPIMRESTAPIPath(string userName);
        public Task CreateAsync(Product product);
        public Task UpdateAsync(Product product);
        public Task DeleteAsync(Product product);
       
    }
}
