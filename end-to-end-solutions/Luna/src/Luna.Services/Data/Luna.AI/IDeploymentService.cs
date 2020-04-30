using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the deployment resource.
    /// </summary>
    public interface IDeploymentService
    {
        /// <summary>
        /// Gets all deployments.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <returns>A list of deployments.</returns>
        Task<List<Deployment>> GetAllAsync(string productName);

        /// <summary>
        /// Gets an deployment by name.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to get.</param>
        /// <returns>The deployment.</returns>
        Task<Deployment> GetAsync(string productName, string deploymentName);

        /// <summary>
        /// Creates an deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deployment">The deployment to create.</param>
        /// <returns>The created deployment.</returns>
        Task<Deployment> CreateAsync(string productName, Deployment deployment);

        /// <summary>
        /// Updates an deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to update.</param>
        /// <param name="deployment">The updated deployment.</param>
        /// <returns>The updated deployment.</returns>
        Task<Deployment> UpdateAsync(string productName, string deploymentName, Deployment deployment);

        /// <summary>
        /// Deletes an deployment.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to delete.</param>
        /// <returns>The deleted deployment.</returns>
        Task<Deployment> DeleteAsync(string productName, string deploymentName);

        /// <summary>
        /// Checks if an deployment exists.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="deploymentName">The name of the deployment to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string productName, string deploymentName);
    }
}