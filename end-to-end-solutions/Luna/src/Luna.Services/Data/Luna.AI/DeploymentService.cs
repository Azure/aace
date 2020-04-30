using Luna.Clients.Azure.APIM;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class DeploymentService : IDeploymentService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<DeploymentService> _logger;
        private readonly IAPIMUtility _apimUtility;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public DeploymentService(ISqlDbContext sqlDbContext, ILogger<DeploymentService> logger, IAPIMUtility apimUtility)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apimUtility = apimUtility ?? throw new ArgumentNullException(nameof(apimUtility));
        }

        public async Task<Deployment> CreateAsync(string productName, Deployment deployment)
        {
            throw new NotImplementedException();
        }

        public async Task<Deployment> DeleteAsync(string productName, string deploymentName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(string productName, string deploymentName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Deployment>> GetAllAsync(string productName)
        {
            throw new NotImplementedException();
        }

        public async Task<Deployment> GetAsync(string productName, string deploymentName)
        {
            throw new NotImplementedException();
        }

        public async Task<Deployment> UpdateAsync(string productName, string deploymentName, Deployment deployment)
        {
            throw new NotImplementedException();
        }
    }
}
