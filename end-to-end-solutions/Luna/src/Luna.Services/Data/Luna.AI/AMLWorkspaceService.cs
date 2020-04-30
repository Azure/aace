using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class AMLWorkspaceService : IAMLWorkspaceService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AMLWorkspaceService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public AMLWorkspaceService(ISqlDbContext sqlDbContext, ILogger<AMLWorkspaceService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AMLWorkspace> CreateAsync(AMLWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public async Task<AMLWorkspace> DeleteAsync(string workspaceName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(string workspaceName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AMLWorkspace>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<AMLWorkspace> GetAsync(string workspaceName)
        {
            throw new NotImplementedException();
        }

        public async Task<AMLWorkspace> UpdateAsync(string workspaceName, AMLWorkspace workspace)
        {
            throw new NotImplementedException();
        }
    }
}
