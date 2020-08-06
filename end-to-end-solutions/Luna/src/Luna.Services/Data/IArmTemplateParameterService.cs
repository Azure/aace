// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the armTemplateParameter resource.
    /// </summary>
    public interface IArmTemplateParameterService
    {
        /// <summary>
        /// Gets all armTemplateParameters within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of armTemplateParameter objects.</returns>
        Task<List<ArmTemplateParameter>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets an armTemplateParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the armTemplateParameter to get.</param>
        /// <returns>The armTemplateParameter object.</returns>
        Task<ArmTemplateParameter> GetAsync(string offerName, string name);

        /// <summary>
        /// Creates an armTemplateParameter object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// /// <param name="armTemplateId">The id of the armTemplate that the parameter is associated with.</param>
        /// <param name="armTemplateParameter">The armTemplateParameter to create.</param>
        /// <returns>The created armTemplateParameter.</returns>
        Task<ArmTemplateParameter> CreateAsync(string offerName, long armTemplateId, ArmTemplateParameter armTemplateParameter);

        /// <summary>
        /// Updates an armTemplateParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the armTemplateParameter to update.</param>
        /// <param name="armTemplateParameter">The updated armTemplateParameter.</param>
        /// <returns>The updated armTemplateParameter.</returns>
        Task<ArmTemplateParameter> UpdateAsync(string offerName, string parameterName, ArmTemplateParameter armTemplateParameter);

        /// <summary>
        /// Removes any ArmTemplateParameters from the db that are not associated with any ArmTemplates.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns></returns>
        Task DeleteUnusedAsync(string offerName);

        /// <summary>
        /// Checks if an armTemplateParameter exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the armTemplateParameter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string name);
    }
}