using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IUserAPIM
    {
        string GetAPIMRESTAPIPath(string owner);
        Task CreateAsync(string owner);
        Task UpdateAsync(string owner);
        Task DeleteAsync(string owner);
    }
}
