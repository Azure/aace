using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for product resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="productService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>HTTP 200 OK with product JSON objects in response body.</returns>
        [HttpGet("products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all products.");
            return Ok(await _productService.GetAllAsync());

            Product product = new Product()
            {
                ProductName = "test",
                ProductType = "test",
                HostType = "test",
                Owner = "test"

            };

            return Ok(new Product[] { product });
        }

        /// <summary>
        /// Get an product.
        /// </summary>
        /// <param name="productName">The name of the product to get.</param>
        /// <returns>HTTP 200 OK with product JSON object in response body.</returns>
        [HttpGet("products/{productName}", Name = nameof(GetAsync) + nameof(Product))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string productName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get product {productName}");
            return Ok(await _productService.GetAsync(productName));
        }

        /// <summary>
        /// Creates or updates an product.
        /// </summary>
        /// <param name="productName">The name of the product to update.</param>
        /// <param name="product">The updated product object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("products/{productName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string productName, [FromBody] Product product)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (product == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(product)), UserErrorCode.PayloadNotProvided);
            }

            if (!productName.Equals(product.ProductName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Product).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _productService.ExistsAsync(productName))
            {
                _logger.LogInformation($"Update product {productName} with payload {JsonConvert.SerializeObject(product)}");
                await _productService.UpdateAsync(productName, product);
                return Ok(product);
            }
            else
            {
                _logger.LogInformation($"Create product {productName} with payload {JsonConvert.SerializeObject(product)}");
                await _productService.CreateAsync(product);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Product), new { productName = product.ProductName }, product);
            }
        }

        /// <summary>
        /// Deletes an product.
        /// </summary>
        /// <param name="productName">The name of the product to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("products/{productName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string productName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete product {productName}.");
            await _productService.DeleteAsync(productName);
            return NoContent();
        }
    }
}