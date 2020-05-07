using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure
{
    public interface ISubscription
    {
        public Task CreateAsync(Subscription subscription);
        public Task UpdateAsync(Subscription subscription);
        public Task DeleteAsync(Subscription subscription);
    }
}
