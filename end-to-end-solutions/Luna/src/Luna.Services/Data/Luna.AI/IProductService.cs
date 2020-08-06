// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the product resource.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>A list of products.</returns>
        Task<List<Product>> GetAllAsync();

        /// <summary>
        /// Gets an product by name.
        /// </summary>
        /// <param name="productName">The name of the product to get.</param>
        /// <returns>The product.</returns>
        Task<Product> GetAsync(string productName);

        /// <summary>
        /// Creates an product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The created product.</returns>
        Task<Product> CreateAsync(Product product);

        /// <summary>
        /// Updates an product.
        /// </summary>
        /// <param name="productName">The name of the product to update.</param>
        /// <param name="product">The updated product.</param>
        /// <returns>The updated product.</returns>
        Task<Product> UpdateAsync(string productName, Product product);

        /// <summary>
        /// Deletes an product.
        /// </summary>
        /// <param name="productName">The name of the product to delete.</param>
        /// <returns>The deleted product.</returns>
        Task<Product> DeleteAsync(string productName);

        /// <summary>
        /// Checks if an product exists.
        /// </summary>
        /// <param name="productName">The name of the product to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string productName);
    }
}