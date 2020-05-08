using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IUserAPIM
    {
        public string GetAPIMRESTAPIPath(string userName);
        public Task CreateAsync(Product product);
        public Task UpdateAsync(Product product);
        public Task DeleteAsync(Product product);
       
    }
}
