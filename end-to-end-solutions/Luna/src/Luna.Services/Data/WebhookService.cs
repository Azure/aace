// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the webhook resource.
    /// </summary>
    public class WebhookService : IWebhookService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly IWebhookParameterService _webhookParameterService;
        private readonly IWebhookWebhookParameterService _webhookWebhookParameterService;
        private readonly ILogger<WebhookService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public WebhookService(
            ISqlDbContext sqlDbContext, 
            IOfferService offerService, 
            IWebhookParameterService webhookParameterService,
            IWebhookWebhookParameterService webhookWebhookParameterService ,
            ILogger<WebhookService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _webhookParameterService = webhookParameterService ?? throw new ArgumentNullException(nameof(webhookParameterService));
            _webhookWebhookParameterService = webhookWebhookParameterService ?? throw new ArgumentNullException(nameof(webhookWebhookParameterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of webhook.</returns>
        public async Task<List<Webhook>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Webhook).Name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all webhooks with a FK to the offer
            var webhooks = await _context.Webhooks.Where(a => a.OfferId == offer.Id).ToListAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Webhook).Name, webhooks.Count()));

            return webhooks;
        }

        /// <summary>
        /// Gets a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to get.</param>
        /// <returns>The webhook.</returns>
        public async Task<Webhook> GetAsync(string offerName, string webhookName)
        {
            // Check that a webhook with the provided webhook exists within the given offer
            if (!(await ExistsAsync(offerName, webhookName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Webhook).Name,
                    webhookName, offerName: offerName));
            }

            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Webhook).Name, webhookName, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the webhook that matches the webhookName provided
            var webhook = await _context.Webhooks
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.WebhookName == webhookName));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Webhook).Name,
                webhookName,
                JsonSerializer.Serialize(webhook),
                offerName: offerName));

            return webhook;
        }

        /// <summary>
        /// Creates a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhook">The webhook to create.</param>
        /// <returns>The created webhook.</returns>
        public async Task<Webhook> CreateAsync(string offerName, Webhook webhook)
        {
            if (webhook is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Webhook).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have a webhook with the same webhookName
            if (await ExistsAsync(offerName, webhook.WebhookName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Webhook).Name,
                    webhook.WebhookName, offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Webhook).Name, webhook.WebhookName, offerName: offerName, payload: JsonSerializer.Serialize(webhook)));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Set the FK to offer
            webhook.OfferId = offer.Id;

            // Add webhook to db
            _context.Webhooks.Add(webhook);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Webhook).Name, webhook.WebhookName, offerName: offerName));

            await CreateWebhookParameters(offerName, webhook);

            return webhook;
        }

        private async Task CreateWebhookParameters(string offerName, Webhook webhook)
        {
            List<WebhookParameter> webhookParameters = ParseWebhookParameters(webhook);

            foreach(WebhookParameter webhookParameter in webhookParameters)
            {
                await _webhookParameterService.CreateAsync(offerName, webhook.Id, webhookParameter);
            }
        }

        private List<WebhookParameter> ParseWebhookParameters(Webhook webhook)
        {
            List<WebhookParameter> webhookParameters = new List<WebhookParameter>();
            Uri webhookUri = new Uri(webhook.WebhookUrl);
            _logger.LogInformation($"Parsing query string for uri {webhookUri}.");
            var parameters = HttpUtility.ParseQueryString(webhookUri.Query);
            foreach (var key in parameters.Keys)
            {
                string value = parameters[key.ToString()];
                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    //trim the brankets
                    string parameterName = value.Substring(1, value.Length - 2);
                    _logger.LogInformation($"Creating webhook parameter {parameterName} for webhook {webhook.WebhookName}.");
                    WebhookParameter webhookParam = new WebhookParameter
                    {
                        OfferId = webhook.OfferId,
                        Name = parameterName,
                        // TODO: do we need to indicate an incomplete parameter?
                        Value = string.Empty
                    };

                    webhookParameters.Add(webhookParam);
                }
            }

            return webhookParameters;
        }

        private async Task UpdateWebhookParameters(Offer offer, Webhook webhook, long webhookId)
        {
            List<WebhookParameter> incompleteParams = ParseWebhookParameters(webhook);
            List<WebhookWebhookParameter> joinEntries = await _webhookWebhookParameterService.GetAllJoinEntries(webhookId);
            Dictionary<string, WebhookParameter> paramsDb = new Dictionary<string, WebhookParameter>();
            HashSet<string> usedParamNames = new HashSet<string>();

            // Populate paramsDb so that it maps the WebhookParameter name to the WebhookParameter object
            foreach (WebhookWebhookParameter entry in joinEntries)
            {
                WebhookParameter webhookParameter = await _context.WebhookParameters.FindAsync(entry.WebhookParameterId);

                if (!paramsDb.ContainsKey(webhookParameter.Name))
                {
                    paramsDb.Add(webhookParameter.Name, webhookParameter);
                }
            }

            foreach (WebhookParameter incompleteParam in incompleteParams)
            {
                // Check if a param with the same name as the incompleteParam already exists
                if (!paramsDb.ContainsKey(incompleteParam.Name))
                {
                    // A param with the same name as the incompleteParam does not exist, so create it
                    await _webhookParameterService.CreateAsync(offer.OfferName, webhookId, incompleteParam);
                }

                // Keep track of all the new parameters we are using in usedParamNames
                if (!usedParamNames.Contains(incompleteParam.Name))
                {
                    usedParamNames.Add(incompleteParam.Name);
                }
            }

            foreach (KeyValuePair<string, WebhookParameter> paramDb in paramsDb)
            {
                // Check if there is a param in the db that we are no longer using
                if (!usedParamNames.Contains(paramDb.Key))
                {
                    WebhookWebhookParameter webhookWebhookParameter = await _context.WebhookWebhookParameters.FindAsync(webhookId, paramDb.Value.Id);

                    // Remove the join entry for any unused params 
                    _context.WebhookWebhookParameters.Remove(webhookWebhookParameter);
                    await _context._SaveChangesAsync();

                    _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(
                        nameof(WebhookWebhookParameter),
                        $"(webhookId={webhookId}, webhookParameterId={webhookWebhookParameter.WebhookParameter})",
                        offerName: offer.OfferName));
                }
            }
        }


        /// <summary>
        /// Updates a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to update.</param>
        /// <param name="webhook">The new webhook.</param>
        /// <returns>The updated webhook.</returns>
        public async Task<Webhook> UpdateAsync(string offerName, string webhookName, Webhook webhook)
        {
            if (webhook is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Webhook).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the webhookName has been updated) && 
            //          (a webhook with the same new webhookName does not already exist)
            if ((webhookName != webhook.WebhookName) && (await ExistsAsync(offerName, webhook.WebhookName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Webhook).Name,
                    webhook.WebhookName, offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Webhook).Name, webhook.WebhookName, offerName: offerName, payload: JsonSerializer.Serialize(webhook)));

            // Get the webhook that matches the webhookName provided
            var webhookDb = await GetAsync(offerName, webhookName);

            // Copy over the changes
            webhookDb.Copy(webhook);

            // Update webhook values and save changes in db
            _context.Webhooks.Update(webhookDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(nameof(Webhook), webhookName, offerName: offerName));

            await UpdateWebhookParameters(await _offerService.GetAsync(offerName), webhookDb, webhookDb.Id);

            return webhook;
        }

        /// <summary>
        /// Deletes a webhook within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to delete.</param>
        /// <returns>The deleted webhook.</returns>
        public async Task<Webhook> DeleteAsync(string offerName, string webhookName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Webhook).Name, webhookName, offerName: offerName));

            // Get the webhook that matches the webhook provided
            var webhook = await GetAsync(offerName, webhookName);

            // Remove the webhook from the db
            _context.Webhooks.Remove(webhook);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(nameof(Webhook), webhookName, offerName: offerName));

            return webhook;
        }

        /// <summary>
        /// Checks if a webhook exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="webhookName">The name of the webhook to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string webhookName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Webhook).Name, webhookName, offerName: offerName));

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one webhook with this webhookName exists within the offer
            var count = await _context.Webhooks
                .CountAsync(a => (a.OfferId == offer.Id) && (a.WebhookName == webhookName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Webhook).Name, webhookName, offerName: offerName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Webhook).Name, webhookName, false, offerName: offerName));
                return false;
            }
            else
            {
                // count = 1
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Webhook).Name, webhookName, false, offerName: offerName));
                return true;
            }
        }
    }
}