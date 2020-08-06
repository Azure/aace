// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the armTemplate resource.
    /// </summary>
    public interface IArmTemplateService
    {
        /// <summary>
        /// Gets all armTemplates within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of armTemplates.</returns>
        Task<List<ArmTemplate>> GetAllAsync(string offerName);

        /// <summary>
        /// Gets an armTemplate within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to get.</param>
        /// <param name="useSaSKey">Specify if use SaS key.</param>
        /// <returns>The armTemplate.</returns>
        Task<ArmTemplate> GetAsync(string offerName, string templateName, bool useSaSKey = true);

        /// <summary>
        /// Uploads the given armTemplate as a JSON file in blob storage and records the URI to the
        /// created resrouce in the db.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="armTemplate">The name of the armTemplate to create.</param>
        /// <param name="armTemplateJSON">The ARM Template's raw JSON data.</param>
        /// <returns>The created armTemplate db record.</returns>
        Task<ArmTemplate> CreateAsync(string offerName, string templateName, object armTemplateJSON);

        /// <summary>
        /// Uploads the given armTemplate as a JSON file in blob storage and records the URI to the
        /// update resrouce in the db.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to create.</param>
        /// <param name="armTemplateJSON">The ARM Template's raw JSON data.</param>
        /// <returns>The created armTemplate db record.</returns>
        Task<ArmTemplate> UpdateAsync(string offerName, string templateName, object armTemplateJSON);

        /// <summary>
        /// Deletes an armTemplate record within an offer and removes the armTemplate file from blob storage.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTempalte to delete.</param>
        /// <returns>The deleted armTemplate db record.</returns>
        Task<ArmTemplate> DeleteAsync(string offerName, string templateName);

        /// <summary>
        /// Checks if an armTemplate exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string offerName, string templateName);
    }
}