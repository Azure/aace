using System;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IAPISubscriptionAPIM
    {
        public string GETAPIMRESTAPIPath(Guid subscriptionId);
        public Task<Models.Azure.APISubscription> CreateAsync(APISubscription subscription);
        public Task<Models.Azure.APISubscription> UpdateAsync(APISubscription subscription);
        public Task DeleteAsync(Data.Entities.APISubscription subscription);
        public Task<Models.Azure.APISubscription.Properties> RegenerateKey(Guid subscriptionId, string keyName);
    }
}
