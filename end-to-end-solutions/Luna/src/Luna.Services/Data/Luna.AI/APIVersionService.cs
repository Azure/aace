using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class APIVersionService : IAPIVersionService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<APIVersionService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public APIVersionService(ISqlDbContext sqlDbContext, ILogger<APIVersionService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<APIVersion> CreateAsync(string productName, string deploymentName, APIVersion version)
        {
            throw new NotImplementedException();
        }

        public async Task<APIVersion> DeleteAsync(string productName, string deploymentName, string versionName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(string productName, string deploymentName, string versionName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<APIVersion>> GetAllAsync(string productName, string deploymentName)
        {
            throw new NotImplementedException();
        }

        public async Task<APIVersion> GetAsync(string productName, string deploymentName, string versionName)
        {
            throw new NotImplementedException();
        }

        public async Task<APIVersion> UpdateAsync(string productName, string deploymentName, string versionName, APIVersion version)
        {
            throw new NotImplementedException();
        }
    }
}
