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
    /// Service class that handles basic CRUD functionality for the subscriptionParameter resource.
    /// </summary>
    public class SubscriptionParameterService : ISubscriptionParameterService
    {
        private readonly ISqlDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionParameterService> _logger;

        public SubscriptionParameterService(ISqlDbContext sqlDbContext, ISubscriptionService subscriptionService, ILogger<SubscriptionParameterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<SubscriptionParameter>> GetAllAsync(Guid subscriptionId)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(SubscriptionParameter).Name));

            var subscription = await _subscriptionService.GetAsync(subscriptionId);

            var parameters = await _context.SubscriptionParameters.Where(p => p.SubscriptionId == subscriptionId).ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(SubscriptionParameter).Name, parameters.Count()));

            return parameters;
        }

        public async Task<SubscriptionParameter> GetAsync(Guid subscriptionId, string name)
        {
            // Check that an armTemplateParameter with the provided name exists within the given offer
            if (!(await ExistsAsync(subscriptionId, name)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(SubscriptionParameter).Name,
                    name));
            }

            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(SubscriptionParameter).Name, name));
            // Find the armTemplateParameter that matches the name provided
            var parameter = await _context.SubscriptionParameters
                .SingleOrDefaultAsync(p => (p.SubscriptionId == subscriptionId) && (p.Name == name));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(SubscriptionParameter).Name,
                name,
                JsonSerializer.Serialize(parameter),
                subscriptionId: subscriptionId));

            return parameter;
        }

        public async Task<SubscriptionParameter> CreateAsync(SubscriptionParameter subscriptionParameter)
        {
            if (subscriptionParameter is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(SubscriptionParameter).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an armTemplateParameter with the same name
            if (await ExistsAsync(subscriptionParameter.SubscriptionId, subscriptionParameter.Name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(SubscriptionParameter).Name,
                    subscriptionParameter.Name));
            }

            if (!await _subscriptionService.ExistsAsync(subscriptionParameter.SubscriptionId))
            {
                throw new ArgumentException("Subscription doesn't exist.");
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(SubscriptionParameter).Name, subscriptionParameter.Name, payload: JsonSerializer.Serialize(subscriptionParameter)));

            // Add armTemplateParameter to db
            _context.SubscriptionParameters.Add(subscriptionParameter);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(SubscriptionParameter).Name, subscriptionParameter.Name));

            return subscriptionParameter;
        }

        public async Task<bool> ExistsAsync(Guid subscriptionID, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(SubscriptionParameter).Name, name));

            // Check that only one armTemplateParameter with this name exists within the offer
            var count = await _context.SubscriptionParameters
                .CountAsync(p => (p.SubscriptionId == subscriptionID) && (p.Name == name));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(SubscriptionParameter).Name,
                    name,
                    subscriptionId: subscriptionID));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(SubscriptionParameter).Name, name, false));

                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(SubscriptionParameter).Name, name, true));
                // count = 1
                return true;
            }
        }
    }
}