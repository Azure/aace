using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IProductAPIM
    {
        public string GetAPIMRESTAPIPath(string productName);

        public Task CreateAsync(Product product);

        public Task UpdateAsync(Product product);

        public Task DeleteAsync(Product product);
        
    }
}
