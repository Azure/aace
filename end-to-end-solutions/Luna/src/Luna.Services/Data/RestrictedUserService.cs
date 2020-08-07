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
    /// Service class that handles basic CRUD functionality for the restrictedUser resource.
    /// </summary>
    public class RestrictedUserService : IRestrictedUserService
    {
        private readonly ISqlDbContext _context;
        private readonly IPlanService _planService;
        private readonly ILogger<RestrictedUserService> _logger;

        /// <summary>
        /// Contructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="planService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public RestrictedUserService(ISqlDbContext sqlDbContext, IPlanService planService, ILogger<RestrictedUserService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all restrictedUsers within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <returns>A list of restrictedUsers.</returns>
        public async Task<List<RestrictedUser>> GetAllAsync(string offerName, string planUniqueName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(RestrictedUser).Name, offerName: offerName, planName: planUniqueName));

            // Get the plan associated with the offerName and planUniqueName provided
            var plan = await _planService.GetAsync(offerName, planUniqueName);

            // Get all restrictedUsers with a FK to the plan
            var restrictedUsers = await _context.RestrictedUsers.Where(r => r.PlanId == plan.Id).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(RestrictedUser).Name, restrictedUsers.Count(), offerName: offerName, planName: planUniqueName));

            return restrictedUsers;
        }

        /// <summary>
        /// Gets a restructedUser
        /// </summary>
        /// <param name="offerName">The offer name.</param>
        /// <param name="planName">The plan name.</param>
        /// <param name="tenantId">The tenant id.</param>
        /// <returns>The restrictedUser.</returns>
        public async Task<RestrictedUser> GetAsync(string offerName, string planName, Guid tenantId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(RestrictedUser).Name, tenantId.ToString(), offerName: offerName, planName: planName));

            // Check that a restrictedUser with the provided tenantId 
            var plan = await _planService.GetAsync(offerName, planName);

            // Get all restrictedUsers with a FK to the plan
            var restrictedUser = await _context.RestrictedUsers.SingleAsync(r => r.PlanId == plan.Id && r.TenantId == tenantId);

            // Check that an restrictedUser with the provided id exists 
            if (restrictedUser is null)
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(RestrictedUser).Name,
                    tenantId.ToString()));
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(RestrictedUser).Name,
               tenantId.ToString(),
               JsonSerializer.Serialize(restrictedUser),
               offerName: offerName,
               planName: planName));

            return restrictedUser;
        }

        /// <summary>
        /// Creates a restrictedUser within a plan within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="planUniqueName">The name of the plan.</param>
        /// <param name="restrictedUser">The restrictedUser to create.</param>
        /// <returns>The created restrictedUser.</returns>
        public async Task<RestrictedUser> CreateAsync(string offerName, string planUniqueName, RestrictedUser restrictedUser)
        {
            if (restrictedUser is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(RestrictedUser).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(RestrictedUser).Name, restrictedUser.TenantId.ToString(), offerName: offerName, planName: planUniqueName, payload: JsonSerializer.Serialize(restrictedUser)));

            // Get the plan associated with the offerName and planUniqueName provided
            var plan = await _planService.GetAsync(offerName, planUniqueName);

            // Set the FK to plan
            restrictedUser.PlanId = plan.Id;

            // Reset the PK (should not be modified in request)
            restrictedUser.Id = 0;

            // Add restrictedUser to db
            _context.RestrictedUsers.Add(restrictedUser);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(RestrictedUser).Name, restrictedUser.TenantId.ToString(), offerName: offerName, planName: planUniqueName));

            return restrictedUser;
        }

        /// <summary>
        /// Updates a restrictedUser 
        /// </summary>
        /// <param name="offerName">The offer name.</param>
        /// <param name="planName">The plan name.</param>
        /// <param name="restrictedUser">The updated restrictedUser.</param>
        /// <returns>The updated restrictedUser.</returns>
        public async Task<RestrictedUser> UpdateAsync(string offerName, string planName, RestrictedUser restrictedUser)
        {
            if (restrictedUser is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(RestrictedUser).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(RestrictedUser).Name, restrictedUser.TenantId.ToString(), offerName: offerName, planName: planName, payload: JsonSerializer.Serialize(restrictedUser)));

            var dbUser = await GetAsync(offerName, planName, restrictedUser.TenantId);

            dbUser.Copy(restrictedUser);

            // Update restrictedUserDb values and save changes in db
            _context.RestrictedUsers.Update(dbUser);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(RestrictedUser).Name, restrictedUser.TenantId.ToString(), offerName: offerName, planName: planName));

            return dbUser;
        }

        /// <summary>
        /// Deletes a restrictedUser.
        /// </summary>
        /// <param name="offerName">The offer name.</param>
        /// <param name="planName">The plan name.</param>
        /// <param name="tenantId">The tenant id.</param>
        /// <returns>The deleted restrictedUser.</returns>
        public async Task<RestrictedUser> DeleteAsync(string offerName, string planName, Guid tenantId)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(RestrictedUser).Name, tenantId.ToString(), offerName: offerName, planName: planName));

            var plan = await _planService.GetAsync(offerName, planName);

            // Get all restrictedUsers with a FK to the plan
            var restrictedUser = await _context.RestrictedUsers.SingleOrDefaultAsync(r => r.PlanId == plan.Id && r.TenantId == tenantId);


            // Remove the restrictedUser from the db
            _context.RestrictedUsers.Remove(restrictedUser);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(RestrictedUser).Name, tenantId.ToString(), offerName: offerName, planName: planName));

            return restrictedUser;
        }

        public async Task<bool> ExistsAsync(string offerName, string planName, Guid tenantId)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(RestrictedUser).Name, tenantId.ToString(), offerName: offerName, planName: planName));

            var plan = await _planService.GetAsync(offerName, planName);

            // Check that only one restructed user with this templateName exists within the offer
            var count = await _context.RestrictedUsers
                .CountAsync(a => (a.PlanId == plan.Id) && (a.TenantId == tenantId));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(RestrictedUser).Name,
                    tenantId.ToString(),
                    offerName: offerName,
                    planName: planName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(RestrictedUser).Name, tenantId.ToString(), false, offerName: offerName, planName: planName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(RestrictedUser).Name, tenantId.ToString(), true, offerName: offerName, planName: planName));
                // count = 1
                return true;
            }
        }
    }
}