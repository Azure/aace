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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service class that handles basic CRUD functionality for the plan resource.
    /// </summary>
    public class PlanService : IPlanService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly IArmTemplateService _armTemplateService;
        private readonly IWebhookService _webhookService;
        private readonly ILogger<PlanService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">A service to inject.</param>
        /// <param name="armTemplateService">A service to inject.</param>
        /// <param name="logger">The logger.</param>
        public PlanService(ISqlDbContext sqlDbContext, IOfferService offerService, IArmTemplateService armTemplateService, IWebhookService webhookService, ILogger<PlanService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _armTemplateService = armTemplateService ?? throw new ArgumentNullException(nameof(armTemplateService));
            _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all plans within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of plans.</returns>
        public async Task<List<Plan>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Plan).Name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all plans with a FK to the offer
            var plans = await _context.Plans.Where(p => p.OfferId == offer.Id).ToListAsync();

            List<Plan> result = new List<Plan>();
            // Set each plan's ARM template Names
            foreach (Plan plan in plans)
            {
                result.Add(await SetArmTemplateAndWebhookNames(plan));
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Plan).Name, plans.Count()));

            return result;
        }

        private async Task<Plan> SetArmTemplateAndWebhookNames(Plan plan)
        {
            // Set the plan's ARM template names
            plan.SubscribeArmTemplateName = await GetArmTemplateNameById(plan.SubscribeArmTemplateId);
            plan.UnsubscribeArmTemplateName = await GetArmTemplateNameById(plan.UnsubscribeArmTemplateId);
            plan.SuspendArmTemplateName = await GetArmTemplateNameById(plan.SuspendArmTemplateId);
            plan.DeleteDataArmTemplateName = await GetArmTemplateNameById(plan.DeleteDataArmTemplateId);
            plan.SubscribeWebhookName = await GetWebhookNameById(plan.SubscribeWebhookId);
            plan.UnsubscribeWebhookName = await GetWebhookNameById(plan.UnsubscribeWebhookId);
            plan.SuspendWebhookName = await GetWebhookNameById(plan.SuspendWebhookId);
            plan.DeleteDataWebhookName = await GetWebhookNameById(plan.DeleteDataWebhookId);
            return plan;
        }

        private async Task<string> GetArmTemplateNameById(long? id)
        {
            if (id == null)
            {
                return null;
            }

            var armTemplate = await _context.ArmTemplates.FindAsync(id);

            if (armTemplate == null)
            {
                // This should never happen with FK
                throw new LunaServerException($"Specified ARM template with id {id} doesn't exists");
            }

            return armTemplate.TemplateName;
        }

        private async Task<string> GetWebhookNameById(long? id)
        {
            if (id == null)
            {
                return null;
            }

            var webhook = await _context.Webhooks.FindAsync(id);

            if (webhook == null)
            {
                // This should never happen with FK
                throw new LunaServerException($"Specified webhook with id {id} doesn't exists");
            }

            return webhook.WebhookName;
        }

        private async Task<long?> GetArmTemplateIdByName(string offerName, string name)
        {
            if (name == null)
            {
                return null;
            }

            return (await _armTemplateService.GetAsync(offerName, name)).Id;
        }

        private async Task<long?> GetWebhookIdByName(string offerName, string name)
        {
            if (name == null)
            {
                return null;
            }

            return (await _webhookService.GetAsync(offerName, name)).Id;
        }

        /// <summary>
        /// Gets a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to get.</param>
        /// <returns>The plan.</returns>
        public async Task<Plan> GetAsync(string offerName, string planUniqueName)
        {
            // Check that a plan with the provided planUniqueName exists within the given offer
            if (!(await ExistsAsync(offerName, planUniqueName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Plan).Name,
                        planUniqueName,
                        offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Plan).Name, planUniqueName, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the plan that matches the planUniqueName provided
            var plan = await _context.Plans
                .SingleOrDefaultAsync(p => (p.OfferId == offer.Id) && (p.PlanName == planUniqueName));

            plan = await SetArmTemplateAndWebhookNames(plan);
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Plan).Name,
               planUniqueName,
               JsonSerializer.Serialize(plan),
               offerName: offerName));
            return plan;
        }

        /// <summary>
        /// Gets a plan by its id.
        /// </summary>
        /// <param name="planId">The plan id</param>
        /// <returns>The plan.</returns>
        public async Task<Plan> GetByIdAsync(long planId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Plan).Name, planId.ToString()));

            // Find the plan that matches the plan id provided
            var plan = await _context.Plans
                .SingleOrDefaultAsync(p => (p.Id == planId));

            plan = await SetArmTemplateAndWebhookNames(plan);
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Plan).Name,
               planId.ToString(),
               JsonSerializer.Serialize(plan)));
            return plan;
        }

        private async Task<Plan> SetArmTemplateAndWebhookIds(string offerName, Plan plan)
        {
            // If SubscribeArmTemplateName is not specified, the other templates should not be specified.
            if (plan.SubscribeArmTemplateName == null && (plan.UnsubscribeArmTemplateName != null ||
                plan.SuspendArmTemplateName != null || plan.DeleteDataArmTemplateName != null))
            {
                throw new LunaBadRequestUserException("The Subscribe ARM template is required if any ARM template is being used in a plan.",
                    UserErrorCode.ArmTemplateNotProvided);
            }

            // Set the plan's ARM template ids
            plan.SubscribeArmTemplateId = await GetArmTemplateIdByName(offerName, plan.SubscribeArmTemplateName);
            plan.UnsubscribeArmTemplateId = await GetArmTemplateIdByName(offerName, plan.UnsubscribeArmTemplateName);
            plan.SuspendArmTemplateId = await GetArmTemplateIdByName(offerName, plan.SuspendArmTemplateName);
            plan.DeleteDataArmTemplateId = await GetArmTemplateIdByName(offerName, plan.DeleteDataArmTemplateName);
            plan.SubscribeWebhookId = await GetWebhookIdByName(offerName, plan.SubscribeWebhookName);
            plan.UnsubscribeWebhookId = await GetWebhookIdByName(offerName, plan.UnsubscribeWebhookName);
            plan.SuspendWebhookId = await GetWebhookIdByName(offerName, plan.SuspendWebhookName);
            plan.DeleteDataWebhookId = await GetWebhookIdByName(offerName, plan.DeleteDataWebhookName);
            return plan;
        }

        /// <summary>
        /// Creates a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="plan">The plan to create.</param>
        /// <returns>The created plan.</returns>
        public async Task<Plan> CreateAsync(string offerName, Plan plan)
        {
            if (plan is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Plan).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an plan with the same planUniqueName
            if (await ExistsAsync(offerName, plan.PlanName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Plan).Name,
                    plan.PlanName,
                    offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Plan).Name, plan.PlanName, offerName: offerName, payload: JsonSerializer.Serialize(plan)));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Set the FK to offer
            plan.OfferId = offer.Id;

            plan = await SetArmTemplateAndWebhookIds(offerName, plan);

            // Add plan to db
            _context.Plans.Add(plan);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Plan).Name, plan.PlanName, offerName: offerName));

            return plan;
        }

        /// <summary>
        /// Updates a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to update.</param>
        /// <param name="plan">The updated plan.</param>
        /// <returns>The updated plan.</returns>
        public async Task<Plan> UpdateAsync(string offerName, string planUniqueName, Plan plan)
        {
            if (plan is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Plan).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the planUniqueName has been updated) && 
            //          (a plan with the same new planUniqueName does not already exist)
            if ((planUniqueName != plan.PlanName) && (await ExistsAsync(offerName, plan.PlanName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Plan).Name),
                    UserErrorCode.NameMismatch);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Plan).Name, planUniqueName, offerName: offerName, payload: JsonSerializer.Serialize(plan)));

            var dbPlan = await GetAsync(offerName, planUniqueName);

            dbPlan.Copy(plan);

            dbPlan = await SetArmTemplateAndWebhookIds(offerName, dbPlan);

            _context.Plans.Update(dbPlan);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Plan).Name, planUniqueName, offerName: offerName));

            return dbPlan;
        }

        /// <summary>
        /// Deletes a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planName">The name of the plan to delete.</param>
        /// <returns>The deleted plan.</returns>
        public async Task<Plan> DeleteAsync(string offerName, string planName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Plan).Name, planName, offerName: offerName));

            // Get the plan that matches the planUniqueName provided
            var plan = await GetAsync(offerName, planName);

            // Remove restricted user for the plan
            foreach (var restrictedUser in plan.RestrictedUsers)
            {
                _context.RestrictedUsers.Remove(restrictedUser);
            }

            // Remove the plan from the db
            _context.Plans.Remove(plan);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Plan).Name, planName, offerName: offerName));

            return plan;
        }

        /// <summary>
        /// Checks if a plan exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string planName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Plan).Name, planName, offerName: offerName));

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one plan with this planUniqueName exists within the offer
            var count = await _context.Plans
                .CountAsync(p => (p.OfferId == offer.Id) && (p.PlanName == planName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Plan).Name,
                    planName,
                    offerName: offerName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Plan).Name, planName, false, offerName: offerName));

                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Plan).Name, planName, true, offerName: offerName));

                // count = 1
                return true;
            }
        }
    }
}