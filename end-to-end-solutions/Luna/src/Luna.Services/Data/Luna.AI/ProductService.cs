using Luna.Clients.Azure.APIM;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class ProductService : IProductService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IProductAPIM _productAPIM;
        private readonly IUserAPIM _userAPIM;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public ProductService(ISqlDbContext sqlDbContext, ILogger<ProductService> logger, 
            IProductAPIM productAPIM, IUserAPIM userAPIM)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productAPIM = productAPIM ?? throw new ArgumentNullException(nameof(productAPIM));
            _userAPIM = userAPIM ?? throw new ArgumentNullException(nameof(userAPIM));
        }
        public async Task<List<Product>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Product).Name));

            // Get all products
            var products = await _context.Products.ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Product).Name, products.Count()));

            return products;
        }

        public async Task<Product> GetAsync(string productName)
        {
            if (!await ExistsAsync(productName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Product).Name,
                    productName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Product).Name, productName));

            // Get the product that matches the provided productName
            var product = await _context.Products.SingleOrDefaultAsync(o => (o.ProductName == productName));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Product).Name,
               productName,
               JsonSerializer.Serialize(product)));

            return product;
        }

        public async Task<Product> CreateAsync(Product product)
        {
            if (product is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Product).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that an offer with the same name does not already exist
            if (await ExistsAsync(product.ProductName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Product).Name,
                        product.ProductName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Product).Name, product.ProductName, payload: JsonSerializer.Serialize(product)));

            // Update the product created time
            product.CreatedTime = DateTime.UtcNow;

            // Update the product last updated time
            product.LastUpdatedTime = product.CreatedTime;

            await _productAPIM.CreateAsync(product);
            await _userAPIM.CreateAsync(product);

            // Add product to db
            _context.Products.Add(product);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Offer).Name, product.ProductName));

            return product;
        }

        public async Task<Product> UpdateAsync(string productName, Product product)
        {
            if (product is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Product).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Product).Name, product.ProductName, payload: JsonSerializer.Serialize(product)));

            // Get the offer that matches the offerName provided
            var productDb = await GetAsync(productName);

            // Check if (the offerName has been updated) && 
            //          (an offer with the same new name does not already exist)
            if ((productName != product.ProductName) && (await ExistsAsync(product.ProductName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Product).Name),
                    UserErrorCode.NameMismatch);
            }

            // Update the product owner
            productDb.Owner = product.Owner;

            // Update the product last updated time
            productDb.LastUpdatedTime = DateTime.UtcNow;

            // Update productDb values and save changes in APIM
            await _productAPIM.UpdateAsync(productDb);
            await _userAPIM.CreateAsync(productDb);

            // Update productDb values and save changes in db
            _context.Products.Update(productDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Product).Name, product.ProductName));

            return productDb;
        }

        public async Task<Product> DeleteAsync(string productName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Product).Name, productName));

            // Get the offer that matches the offerName provide
            var product = await GetAsync(productName);

            // remove the product from the APIM
            await _productAPIM.DeleteAsync(product);

            // Remove the product from the db
            _context.Products.Remove(product);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Product).Name, productName));

            return product;
        }

        public async Task<bool> ExistsAsync(string productName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Product).Name, productName));

            // Check that only one offer with this offerName exists and has not been deleted
            var count = await _context.Products
                .CountAsync(p => (p.ProductName == productName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Product).Name, productName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Product).Name, productName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Product).Name, productName, true));
                // count = 1
                return true;
            }
        }
    }
}
