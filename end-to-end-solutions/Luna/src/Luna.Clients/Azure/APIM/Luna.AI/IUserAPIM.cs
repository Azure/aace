using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IUserAPIM
    {
        public string GetAPIMRESTAPIPath(string owner);
        public Task CreateAsync(string owner);
        public Task UpdateAsync(string owner);
        public Task DeleteAsync(string owner);
    }
}
