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
    /// Service class that handles basic CRUD functionality for TelemetryDataConnectorService resource.
    /// </summary>
    public class TelemetryDataConnectorService : ITelemetryDataConnectorService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<CustomMeterService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="sqlDbContext">The context to inject.</param>
        public TelemetryDataConnectorService(ISqlDbContext sqlDbContext, 
            ILogger<CustomMeterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Gets all TelemetryDataConnector.
        /// </summary>
        /// <returns>A list of TelemetryDataConnector.</returns>
        public async Task<List<TelemetryDataConnector>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(TelemetryDataConnector).Name));

            // Get all TelemetryDataConnectors from db
            var connectors = _context.TelemetryDataConnectors;

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(TelemetryDataConnector).Name, connectors.Count()));

            return connectors.ToList();
        }

        /// <summary>
        /// Gets a TelemetryDataConnector.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <returns>The TelemetryDataConnector.</returns>
        public async Task<TelemetryDataConnector> GetAsync(string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(TelemetryDataConnector).Name, name));

            if (!(await ExistsAsync(name)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(TelemetryDataConnector).Name,
                        name));
            }


            // Get the TelemetryDataConnector that matches the provided meterName
            var connector = await _context.TelemetryDataConnectors.SingleOrDefaultAsync(c => c.Name == name);

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(
                typeof(TelemetryDataConnector).Name,
                name,
                JsonSerializer.Serialize(connector)));

            return connector;
        }

        /// <summary>
        /// Creates a TelemetryDataConnector.
        /// </summary>
        /// <param name="offerName">The name of the TelemetryDataConnector.</param>
        /// <param name="connector">The TelemetryDataConnector to create.</param>
        /// <returns>The created TelemetryDataConnector.</returns>
        public async Task<TelemetryDataConnector> CreateAsync(string name, TelemetryDataConnector connector)
        {
            if (connector is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(TelemetryDataConnector).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(TelemetryDataConnector).Name, name, payload: JsonSerializer.Serialize(connector)));

            // Check that an TelemetryDataConnector with the same name does not already exist
            if (await ExistsAsync(name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(TelemetryDataConnector).Name,
                    name));
            }

            _context.TelemetryDataConnectors.Add(connector);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(TelemetryDataConnector).Name, name));

            return connector;
        }

        /// <summary>
        /// Updates a TelemetryDataConnector.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <param name="connector">The updated TelemetryDataConnector.</param>
        /// <returns>The updated TelemetryDataConnector.</returns>
        public async Task<TelemetryDataConnector> UpdateAsync(string name, TelemetryDataConnector connector)
        {
            if (connector is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(TelemetryDataConnector).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(TelemetryDataConnector).Name, 
                name, payload: JsonSerializer.Serialize(connector)));

            if (!await ExistsAsync(name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(TelemetryDataConnector).Name,
                    name));
            }

            // Get the customMeter that matches the meterName provided
            var connectorDb = await GetAsync(name);
            
            // Copy over the changes
            connectorDb.Copy(connector);

            // Update connector values and save changes in db
            _context.TelemetryDataConnectors.Update(connectorDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(TelemetryDataConnector).Name, name));

            return connector;
        }

        /// <summary>
        /// Deletes a TelemetryDataConnector.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector to delete.</param>
        /// <returns>The deleted TelemetryDataConnector.</returns>
        public async Task<TelemetryDataConnector> DeleteAsync(string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(TelemetryDataConnector).Name, name));

            if (!await ExistsAsync(name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(TelemetryDataConnector).Name,
                    name));
            }

            // Get the TelemetryDataConnector that matches the name provide
            var connector = await GetAsync(name);

            // Update and save changes in db
            _context.TelemetryDataConnectors.Remove(connector);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(TelemetryDataConnector).Name, name));

            return connector;
        }

        /// <summary>
        /// Checks if a customMeter exists.
        /// </summary>
        /// <param name="name">The name of the TelemetryDataConnector.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string name)
        {
            // Check that only one customMeter with this meterName exists
            var count = await _context.TelemetryDataConnectors
                .CountAsync(c => c.Name == name);

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(TelemetryDataConnector).Name,
                    name));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(TelemetryDataConnector).Name, 
                    name, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(TelemetryDataConnector).Name, 
                    name, true));
                // count = 1
                return true;
            }
        }
    }
}