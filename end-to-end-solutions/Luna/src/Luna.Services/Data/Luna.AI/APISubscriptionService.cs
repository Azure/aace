using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class APISubscriptionService : IAPISubscriptionService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<APISubscriptionService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public APISubscriptionService(ISqlDbContext sqlDbContext, ILogger<APISubscriptionService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<APISubscription> CreateAsync(APISubscription apiSubscription)
        {
            throw new NotImplementedException();
        }

        public async Task<APISubscription> DeleteAsync(Guid apiSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(Guid apiSubscriptionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all subscriptions.
        /// </summary>
        /// <param name="status">The list status of the subscription.</param>
        /// <param name="owner">The owner of the subscription.</param>
        /// <returns>A list of all subsrciptions.</returns>
        public async Task<List<APISubscription>> GetAllAsync(string[] status = null, string owner = "")
        {
            throw new NotImplementedException();
        }

        public async Task<APISubscription> GetAsync(Guid apiSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public async Task<APISubscription> UpdateAsync(Guid apiSubscriptionId, APISubscription apiSubscription)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Regenerate key for the subscription
        /// </summary>
        /// <param name="apiSubscriptionId">subscription id</param>
        /// <param name="keyName">The key name</param>
        /// <returns>The subscription with regenerated key</returns>
        public async Task<APISubscription> RegenerateKey(Guid apiSubscriptionId, string keyName)
        {
            throw new NotImplementedException();
        }
    }
}
