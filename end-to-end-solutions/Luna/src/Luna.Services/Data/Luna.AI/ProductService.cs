using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class ProductService : IProductService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<ProductService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public ProductService(ISqlDbContext sqlDbContext, ILogger<ProductService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Product> CreateAsync(Product product)
        {
            throw new NotImplementedException();
        }

        public async Task<Product> DeleteAsync(string productName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(string productName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Product>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Product> GetAsync(string productName)
        {
            throw new NotImplementedException();
        }

        public async Task<Product> UpdateAsync(string productName, Product product)
        {
            throw new NotImplementedException();
        }
    }
}
