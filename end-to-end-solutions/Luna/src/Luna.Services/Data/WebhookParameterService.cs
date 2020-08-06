// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Luna.Clients.Logging;
using Luna.Clients.Exceptions;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the webhookParameter resource.
    /// </summary>
    public class WebhookParameterService : IWebhookParameterService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly IWebhookWebhookParameterService _webhookWebhookParameterService;
        private readonly ILogger<WebhookParameterService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public WebhookParameterService(
            ISqlDbContext sqlDbContext, 
            IOfferService offerService,
            IWebhookWebhookParameterService webhookWebhookParameterService,
            ILogger<WebhookParameterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _webhookWebhookParameterService = webhookWebhookParameterService ?? throw new ArgumentNullException(nameof(webhookWebhookParameterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of webhookParameter objects.</returns>
        public async Task<List<WebhookParameter>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(WebhookParameter).Name));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all webhookParameter with a FK to the offer
            var webhookParameters = await _context.WebhookParameters.Where(a => a.OfferId == offer.Id).ToListAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(WebhookParameter).Name, webhookParameters.Count()));

            return webhookParameters;
        }

        /// <summary>
        /// Gets a webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to get.</param>
        /// <returns>The webhookParameter object.</returns>
        public async Task<WebhookParameter> GetAsync(string offerName, string name)
        {
            // Check that a webhookParameter with the provided name exists within the given offer
            if (!(await ExistsAsync(offerName, name)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(WebhookParameter).Name,
                    name, offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(WebhookParameter).Name, name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the webhookParameter that matches the name provided
            var webhookParameter = await _context.WebhookParameters
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(ArmTemplateParameter).Name,
                name,
                JsonSerializer.Serialize(webhookParameter),
                offerName: offerName));

            return webhookParameter;
        }

        /// <summary>
        /// Creates a webhookParameter object within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookId">The id of the webhook that the parameter is associated with.</param>
        /// <param name="webhookParameter">The webhookParameter to create.</param>
        /// <returns>The created webhookParameter.</returns>
        public async Task<WebhookParameter> CreateAsync(string offerName, long webhookId, WebhookParameter webhookParameter)
        {
            if (webhookParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(WebhookParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (ExpressionEvaluationUtils.ReservedParameterNames.Contains(webhookParameter.Name))
            {
                _logger.LogInformation($"Webhook {webhookId} is referencing system parameter {webhookParameter.Name}");
                return webhookParameter;
                //throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(WebhookParameter).Name,
                //    webhookParameter.Name, offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(WebhookParameter).Name, webhookParameter.Name, offerName: offerName, payload: JsonSerializer.Serialize(webhookParameter)));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check if the WebhookParameter already exists
            if (await ExistsAsync(offerName, webhookParameter.Name))
            {
                // Just create a new join entry to keep track of the fact that this Webhook is using this parameter
                WebhookParameter webhookParameterDb = await GetAsync(offerName, webhookParameter.Name);
                await _webhookWebhookParameterService.CreateJoinEntryAsync(webhookId, webhookParameterDb.Id);
                return webhookParameterDb;
            }

            // Set the FK to offer
            webhookParameter.OfferId = offer.Id;

            // Add webhookParameter to db
            _context.WebhookParameters.Add(webhookParameter);
            await _context._SaveChangesAsync();
            await _webhookWebhookParameterService.CreateJoinEntryAsync(webhookId, webhookParameter.Id);

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(WebhookParameter).Name, webhookParameter.Name, offerName: offerName));

            return webhookParameter;
        }

        /// <summary>
        /// Updates a webhookParameter within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="parameterName">The name of the webhookParameter to update.</param>
        /// <param name="webhookParameter">The updated webhookParameter.</param>
        /// <returns>The updated webhookParameter.</returns>
        public async Task<WebhookParameter> UpdateAsync(string offerName, string parameterName, WebhookParameter webhookParameter)
        {
            if (webhookParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(WebhookParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(WebhookParameter).Name, webhookParameter.Name, offerName: offerName, payload: JsonSerializer.Serialize(webhookParameter)));

            // Get the webhookParameter that matches the name provided
            var webhookParameterDb = await GetAsync(offerName, parameterName);

            // Copy over the changes
            webhookParameterDb.Copy(webhookParameter);

            // Update webhookParameterDb values and save changes in db
            _context.WebhookParameters.Update(webhookParameterDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(WebhookParameter).Name, webhookParameter.Name, offerName: offerName));

            return webhookParameterDb;
        }

        /// <summary>
        /// Removes any WebhookParameters from the db that are not associated with any Webhooks.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns></returns>
        public async Task DeleteUnusedAsync(string offerName)
        {
            List<WebhookParameter> webhookParameters = await GetAllAsync(offerName);

            foreach (WebhookParameter webhookParameter in webhookParameters)
            {
                int usages = await _context.WebhookWebhookParameters.Where(x => x.WebhookParameterId == webhookParameter.Id).CountAsync();

                if (usages == 0)
                {
                    _context.WebhookParameters.Remove(webhookParameter);
                    await _context._SaveChangesAsync();

                    _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(WebhookParameter).Name, webhookParameter.Name, offerName: offerName));
                }
            }
        }

        /// <summary>
        /// Checks if a webhookParameter exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the webhookParameter to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(WebhookParameter).Name, name, offerName: offerName));

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one webhookParameter with this name exists within the offer
            var count = await _context.WebhookParameters
                .CountAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(WebhookParameter).Name, name, offerName: offerName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(WebhookParameter).Name, name, false, offerName: offerName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(WebhookParameter).Name, name, true));

                // count = 1
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(WebhookParameter).Name, name, false, offerName: offerName));
                return true;
            }
        }
    }
}