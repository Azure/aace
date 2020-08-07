// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure;
using Luna.Clients.Azure.Storage;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the armTemplate resource.
    /// </summary>
    public class ArmTemplateService : IArmTemplateService
    {
        private readonly ISqlDbContext _context;
        private readonly IStorageUtility _storageUtility;
        private readonly IOfferService _offerService;
        private readonly IArmTemplateParameterService _armTemplateParameterService;
        private readonly IArmTemplateArmTemplateParameterService _armTemplateArmTemplateParameterService;
        private readonly ILogger<ArmTemplateService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public ArmTemplateService(
            ISqlDbContext sqlDbContext, 
            IStorageUtility storageUtility, 
            IOfferService offerService, 
            IArmTemplateParameterService armTemplateParameterService, 
            IArmTemplateArmTemplateParameterService armTemplateArmTemplateParameterService, 
            ILogger<ArmTemplateService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _storageUtility = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
            _armTemplateParameterService = armTemplateParameterService ?? throw new ArgumentNullException(nameof(armTemplateParameterService));
            _armTemplateArmTemplateParameterService = armTemplateArmTemplateParameterService ?? throw new ArgumentNullException(nameof(armTemplateArmTemplateParameterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        } 

        /// <summary>
        /// Gets all armTemplates within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of armTemplates.</returns>
        public async Task<List<ArmTemplate>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(ArmTemplate).Name, offerName: offerName));
            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all armTemplates with a FK to the offer
            var armTemplates = await _context.ArmTemplates.Where(a => a.OfferId == offer.Id).ToListAsync();

            foreach(var armTemplate in armTemplates)
            {
                // Generate Sas key
                armTemplate.TemplateFilePath = await _storageUtility.GetFileReferenceWithSasKeyAsync(armTemplate.TemplateFilePath);
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(ArmTemplate).Name, armTemplates.Count()));


            return armTemplates;
        }

        /// <summary>
        /// Gets an armTemplate within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to get.</param>
        /// <param name="useSaSKey">Specify if use SaS key in the uri</param>
        /// <returns>The armTemplate.</returns>
        public async Task<ArmTemplate> GetAsync(string offerName, string templateName, bool useSaSKey = true)
        {
            if (!await _offerService.ExistsAsync(offerName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Offer).Name,
                    offerName));
            }

            // Check that an armTemplate with the provided templateName exists within the given offer
            if ( !(await ExistsAsync(offerName, templateName)) )
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(ArmTemplate).Name,
                    templateName,
                    offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the armTemplate that matches the templateName provided
            var armTemplate = await _context.ArmTemplates
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.TemplateName == templateName));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(ArmTemplate).Name,
                templateName,
                JsonSerializer.Serialize(armTemplate),
                offerName: offerName));

            if (useSaSKey)
            {
                // Generate Sas key
                armTemplate.TemplateFilePath = await _storageUtility.GetFileReferenceWithSasKeyAsync(armTemplate.TemplateFilePath);
            }

            _logger.LogInformation("Sas key generated.");

            return armTemplate;
        }

        /// <summary>
        /// Uploads the given armTemplate as a JSON file in blob storage and records the URI to the
        /// created resrouce in the db.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the ARM template.</param>
        /// <param name="armTemplateJSON">The ARM Template's raw JSON data.</param>
        /// <returns>The created armTemplate db record.</returns>
        public async Task<ArmTemplate> CreateAsync(string offerName, string templateName, object armTemplateJSON)
        {
            if (armTemplateJSON is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(ArmTemplate).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an armTemplate with the same templateName
            if (await ExistsAsync(offerName, templateName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(ArmTemplate).Name,
                    templateName,
                    offerName: offerName));
            }

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get the container name associated with the offer
            var containerName = offer.ContainerName.ToString();

            // Upload the armTemplateJSON as a file in blob storage and get the URL to the created resource
            var url = await uploadToBlobStorageAsync(containerName, GetArmTemplateFileName(templateName), armTemplateJSON.ToString());

            _logger.LogInformation($"Arm template {templateName} in offer {offerName} is uploaded to {url}.");

            // Create the armTemplate to store in db
            ArmTemplate armTemplate = new ArmTemplate {
                OfferId = offer.Id,
                TemplateName = templateName,
                TemplateFilePath = url
            };

            // Add armTemplate to db
            _context.ArmTemplates.Add(armTemplate);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));

            if (!await _armTemplateParameterService.ExistsAsync(offerName, "resourceGroupLocation"))
            {
                // Add parameter for resourceGroupLocation
                ArmTemplateParameter armParameter = new ArmTemplateParameter
                {
                    OfferId = offer.Id,
                    Name = "resourceGroupLocation",
                    Type = "string",
                    // TODO: do we need to indicate an incomplete parameter?
                    Value = string.Empty
                };

                await _armTemplateParameterService.CreateAsync(offerName, armTemplate.Id, armParameter);
            }

            if (!await _armTemplateParameterService.ExistsAsync(offerName, "entryPointUrl"))
            {
                // Add parameter for entryPointLink
                ArmTemplateParameter armParameter = new ArmTemplateParameter
                {
                    OfferId = offer.Id,
                    Name = "entryPointUrl",
                    Type = "string",
                    // TODO: do we need to indicate an incomplete parameter?
                    Value = string.Empty
                };

                await _armTemplateParameterService.CreateAsync(offerName, armTemplate.Id, armParameter);
            }

            // Add arm template parameters
            await CreateArmTemplateParameters(offer, armTemplateJSON, armTemplate.Id);

            return armTemplate;
        }

        /// <summary>
        /// Parses the given JSON object for ArmTemplateParameters then creates them in the db.
        /// The join entry is also created in the armTemplateArmTemplateParameters table.
        /// </summary>
        /// <param name="offer">The name of the offer the parameters belong to.</param>
        /// <param name="armTemplateJSON">The JSON to parse.</param>
        /// <param name="armTemplateId">The ID of the armTemplate.</param>
        /// <returns></returns>
        private async Task CreateArmTemplateParameters(Offer offer, object armTemplateJSON, long armTemplateId)
        {
            var parameters = ARMTemplateHelper.GetArmTemplateParameters(armTemplateJSON.ToString());
            
            foreach (var param in parameters)
            {
                ArmTemplateParameter armParameter = new ArmTemplateParameter
                {
                    OfferId = offer.Id,
                    Name = param.Key,
                    Type = param.Value,
                    // TODO: do we need to indicate an incomplete parameter?
                    Value = string.Empty
                };

                await _armTemplateParameterService.CreateAsync(offer.OfferName, armTemplateId, armParameter);
            }
        }

        /// <summary>
        /// Parses and checks the given JSON object to see if any armTemplateParameters were added or removed.
        /// If parameters were added then they are created in the db along with a join entry in the armTemplateArmTemplateParameters table.
        /// If parameters were removed then the join entry in the armTemplateArmTemplateParameters table is removed. 
        /// </summary>
        /// <param name="offer">The name of the offer the parameters belong to</param>
        /// <param name="armTemplateJSON">The JSON object to parse.</param>
        /// <param name="armTemplateId">The ID of the armTemplate.</param>
        /// <returns></returns>
        private async Task UpdateArmTemplateParameters(Offer offer, object armTemplateJSON, long armTemplateId)
        {
            List<KeyValuePair<string, string>> incompleteParams = ARMTemplateHelper.GetArmTemplateParameters(armTemplateJSON.ToString());
            List<ArmTemplateArmTemplateParameter> joinEntries = await _armTemplateArmTemplateParameterService.GetAllJoinEntries(armTemplateId);
            Dictionary<string, ArmTemplateParameter> paramsDb = new Dictionary<string, ArmTemplateParameter>();
            HashSet<string> usedParamNames = new HashSet<string>();

            // Populate paramsDb so that it maps the ArmTemplateParameter name to the ArmTemplateParameter object
            foreach (ArmTemplateArmTemplateParameter entry in joinEntries)
            {
                ArmTemplateParameter armTemplateParameter = await _context.ArmTemplateParameters.FindAsync(entry.ArmTemplateParameterId);
                
                if (!paramsDb.ContainsKey(armTemplateParameter.Name))
                {
                    paramsDb.Add(armTemplateParameter.Name, armTemplateParameter);
                }
            }

            foreach (KeyValuePair<string, string> incompleteParam in incompleteParams)
            {
                // Check if a param with the same name as the incompleteParam already exists
                if (!paramsDb.ContainsKey(incompleteParam.Key))
                {
                    ArmTemplateParameter armParameter = new ArmTemplateParameter
                    {
                        OfferId = offer.Id,
                        Name = incompleteParam.Key,
                        Type = incompleteParam.Value,
                        // TODO: do we need to indicate an incomplete parameter?
                        Value = string.Empty
                    };

                    // A param with the same name as the incompleteParam does not exist, so create it
                    await _armTemplateParameterService.CreateAsync(offer.OfferName, armTemplateId, armParameter);                    
                }

                // Keep track of all the new parameters we are using in usedParamNames
                if (!usedParamNames.Contains(incompleteParam.Key))
                {
                    usedParamNames.Add(incompleteParam.Key);
                }
            }

            foreach (KeyValuePair<string, ArmTemplateParameter> paramDb in paramsDb)
            {
                // Check if there is a param in the db that we are no longer using
                if (!usedParamNames.Contains(paramDb.Key))
                {
                    ArmTemplateArmTemplateParameter armTemplateArmTemplateParameter = await _context.ArmTemplateArmTemplateParameters.FindAsync(armTemplateId, paramDb.Value.Id);
                    
                    // Remove the join entry for any unused params 
                    _context.ArmTemplateArmTemplateParameters.Remove(armTemplateArmTemplateParameter);
                    await _context._SaveChangesAsync();
                }
            }
        }

        private string GetArmTemplateFileName(string templateName)
        {
            return templateName + "_" + DateTime.UtcNow.ToString("MMddyyyy_hhmmss") + ".json";
        }

        /// <summary>
        /// Uploads the given armTemplate as a JSON file in blob storage and records the URI to the
        /// created resrouce in the db.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the ARM template.</param>
        /// <param name="armTemplateJSON">The ARM Template's raw JSON data.</param>
        /// <returns>The created armTemplate db record.</returns>
        public async Task<ArmTemplate> UpdateAsync(string offerName, string templateName, object armTemplateJSON)
        {
            if (armTemplateJSON is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(ArmTemplate).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does already have an armTemplate with the same templateName
            if (!await ExistsAsync(offerName, templateName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(ArmTemplate).Name,
                    templateName,
                    offerName: offerName));
            }

            ArmTemplate armTemplate = await GetAsync(offerName, templateName);

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get the container name associated with the offer
            var containerName = offer.ContainerName.ToString();

            // Upload the armTemplateJSON as a file in blob storage and get the URL to the created resource
            var url = await uploadToBlobStorageAsync(containerName, GetArmTemplateFileName(templateName), armTemplateJSON.ToString());
            _logger.LogInformation($"Arm template {templateName} in offer {offerName} is uploaded to {url}.");

            armTemplate.TemplateFilePath = url;
            
            // Add armTemplate to db
            _context.ArmTemplates.Update(armTemplate);
            await _context._SaveChangesAsync();

            await UpdateArmTemplateParameters(offer, armTemplateJSON, armTemplate.Id);

            return armTemplate;

        }

        /// <summary>
        /// Deletes an armTemplate record within an offer and removes the armTemplate file from blob storage.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTempalte to delete.</param>
        /// <returns>The deleted armTemplate db record.</returns>
        public async Task<ArmTemplate> DeleteAsync(string offerName, string templateName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));

            // Get the armTemplate that matches the templateName provided
            var armTemplate = await GetAsync(offerName, templateName, useSaSKey: false);

            // Remove the armTemplate file from blob storage
            await _storageUtility.DeleteFileAsync(new Uri(armTemplate.TemplateFilePath));

            // Remove the join entry form the db
            await _armTemplateArmTemplateParameterService.DeleteArmTemplateJoinEntriesAsync(armTemplate.Id);

            // Remove the armTemplate from the db
            _context.ArmTemplates.Remove(armTemplate);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));

            return armTemplate;
        }

        /// <summary>
        /// Checks if an armTemplate exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="templateName">The name of the armTemplate to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string templateName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));

            if (!await _offerService.ExistsAsync(offerName))
            {
                // Instead of throw NotFound exception, just return false.
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(ArmTemplate).Name, templateName, false, offerName: offerName));
                return false;
            }

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one armTemplate with this templateName exists within the offer
            var count = await _context.ArmTemplates
                .CountAsync(a => (a.OfferId == offer.Id) && (a.TemplateName == templateName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(ArmTemplate).Name, templateName, offerName: offerName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(ArmTemplate).Name, templateName, false, offerName: offerName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(ArmTemplate).Name, templateName, true, offerName: offerName));
                return true;
            }
        }

        /// <summary>
        /// Uploads a file as a blob to a container in Azure storage. If the container does not 
        /// exist then it will be created. If a file with the same name exists it will be overwritten.
        /// </summary>
        /// <param name="containerName">The name of the storage container.</param>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="content">The content of the file to upload as a string.</param>
        /// <returns>The URL to the uploaded file with a SaS key.</returns>
        private async Task<string> uploadToBlobStorageAsync(string containerName, string fileName, string content)
        {
            if (await _storageUtility.ContainerExistsAsync(containerName) is false)
            {
                // Container does not exist so create one
                await _storageUtility.CreateContainerAsync(containerName);
            }

            // TODO: get random name
            var url = await _storageUtility.UploadTextFileAsync(containerName, fileName, content, true);

            return url;
        }
    }
}