// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    /// Service clas that handles basic CRUD functionality for the ipConfig resource.
    /// </summary>
    public class IpConfigService : IIpConfigService
    {
        private readonly ISqlDbContext _context;
        private readonly IOfferService _offerService;
        private readonly ILogger<IpConfigService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="offerService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public IpConfigService(ISqlDbContext sqlDbContext, IOfferService offerService, ILogger<IpConfigService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all ipConfigs within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <returns>A list of ipConfigs.</returns>
        public async Task<List<IpConfig>> GetAllAsync(string offerName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(IpConfig).Name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Get all ipConfigs with a FK to the offer
            var ipConfigs = await _context.IpConfigs.Where(o => o.OfferId == offer.Id).ToListAsync();

            // Populate the IpBlocks for each IpConfig
            foreach (IpConfig ipConfig in ipConfigs)
            {
                ipConfig.IpBlocks = await GetIpBlocks(ipConfig.Id);
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(IpConfig).Name, ipConfigs.Count()));

            return ipConfigs;
        }

        /// <summary>
        /// Gets an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to get.</param>
        /// <returns>The ipConfig.</returns>
        public async Task<IpConfig> GetAsync(string offerName, string name)
        {
            // Check that an ipConfig with the provided name exists within the given offer
            if (!(await ExistsAsync(offerName, name)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(IpConfig).Name,
                        name,
                        offerName: offerName));
            }

            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(IpConfig).Name, name, offerName: offerName));

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Find the ipConfig that matches the name provided
            var ipConfig = await _context.IpConfigs
                .SingleOrDefaultAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            // Populate the IpBlocks this IpConfig has
            ipConfig.IpBlocks = await GetIpBlocks(ipConfig.Id);

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(IpConfig).Name,
                name,
                JsonSerializer.Serialize(ipConfig),
                offerName: offerName));

            return ipConfig;
        }

        /// <summary>
        /// Creates an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="ipConfig">The ipConfig to create.</param>
        /// <returns>The created ipConfig.</returns>
        public async Task<IpConfig> CreateAsync(string offerName, IpConfig ipConfig)
        {
            if (ipConfig is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(IpConfig).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the offer does not already have an ipConfig with the same name
            if (await ExistsAsync(offerName, ipConfig.Name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(IpConfig).Name,
                    ipConfig.Name,
                    offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(IpConfig).Name, ipConfig.Name, offerName: offerName, payload: JsonSerializer.Serialize(ipConfig)));

            // Validate that the values provided for the IpConfig are syntactically and logically correct
            ipConfig = validateIpConfig(ipConfig);

            // Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Set the FK to offer
            ipConfig.OfferId = offer.Id;

            // Add ipConfig to db
            _context.IpConfigs.Add(ipConfig);
            await _context._SaveChangesAsync();

            // Process the IpBlocks
            await ProcessIpBlocks(ipConfig.IpBlocks, ipConfig.Id);

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(IpConfig).Name, ipConfig.Name, offerName: offerName));

            return ipConfig;
        }

        /// <summary>
        /// Updates an ipConfig within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to update.</param>
        /// <param name="ipConfig">The updated ipConfig.</param>
        /// <returns>The updated ipConfig.</returns>
        public async Task<IpConfig> UpdateAsync(string offerName, string name, IpConfig ipConfig)
        {
            if (ipConfig is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Plan).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that that an IpConfig with the given name exists within the Offer
            if (!(await ExistsAsync(offerName, name)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(IpConfig).Name,
                        name,
                        offerName: offerName));
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(IpConfig).Name, name, offerName: offerName, payload: JsonSerializer.Serialize(ipConfig)));

            // Validate that the values provided for the IpConfig are syntactically and logically correct
            ipConfig = validateIpConfig(ipConfig);

            // Get the ipConfig that matches the name provided
            var ipConfigDb = await GetAsync(offerName, name);

            // Verify that the only change is the addition of new IpBlocks
            List<string> addedBlocks = OnlyIpBlocksAdded(ipConfigDb, ipConfig);

            // Process the added IpBlocks
            await ProcessIpBlocks(addedBlocks, ipConfigDb.Id);

            // Get the ipConfig from the db again to repopulate the IpBlocks
            ipConfigDb = await GetAsync(offerName, name);
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(IpConfig).Name, name, offerName: offerName));

            return ipConfigDb;
        }

        /// <summary>
        /// Deletes an IpConfig within an Offer and all of the IpBlocks and IpAddresses associated 
        /// with it. The delete will only occur if all of the IpAddresses associated with the IpConfig
        /// are not being used.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to delete.</param>
        /// <returns>The deleted ipConfig.</returns>
        public async Task<IpConfig> DeleteAsync(string offerName, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(IpConfig).Name, name, offerName: offerName));

            // Get the ipConfig that matches the name provided
            var ipConfig = await GetAsync(offerName, name);

            // Get the ipBlocks associated with the ipConfig
            var ipBlocks = await _context.IpBlocks.Where(x => x.IpConfigId == ipConfig.Id).ToListAsync();

            foreach (IpBlock block in ipBlocks)
            {
                // Check if there are any IpAddresses in use
                if (await _context.IpAddresses.AnyAsync(x => x.IpBlockId == block.Id && !x.IsAvailable))
                {
                    // TODO - at least one IpAddress is in use so the delete cannot be performed
                    throw new NotSupportedException();
                }
            }

            // Remove entities from db
            foreach (IpBlock block in ipBlocks)
            {
                // Get all IpAddresses in this IpBlock
                var ipAddresses = await _context.IpAddresses.Where(x => x.IpBlockId == block.Id).ToListAsync();

                // Remove all IpAddresses in this IpBlock from the db
                _context.IpAddresses.RemoveRange(ipAddresses);
                await _context._SaveChangesAsync();

                // Remove the IpBlock from the db
                _context.IpBlocks.Remove(block);
                await _context._SaveChangesAsync();
            }

            // Remove the ipConfig from the db
            _context.IpConfigs.Remove(ipConfig);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(IpConfig).Name, name, offerName: offerName));

            return ipConfig;
        }

        /// <summary>
        /// Checks if an ipConfig exists within an offer.
        /// </summary>
        /// <param name="offerName">The name of the offer.</param>
        /// <param name="name">The name of the ipConfig to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string offerName, string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(IpConfig).Name, name, offerName: offerName));

            //Get the offer associated with the offerName provided
            var offer = await _offerService.GetAsync(offerName);

            // Check that only one ipConfig with this name exists within the offer
            var count = await _context.IpConfigs
                .CountAsync(a => (a.OfferId == offer.Id) && (a.Name == name));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(IpConfig).Name,
                    name,
                    offerName: offerName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(IpConfig).Name, name, false, offerName: offerName));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(IpConfig).Name, name, true, offerName: offerName));
                // count = 1
                return true;
            }
        }

        /// <summary>
        /// Parses a comma delimited list of IPv4 addresses in CIDR notation and stores them 
        /// in the IpBlocks db table. Each IpBlock is then enumerated and all of the possible
        /// IP addresses in the range are stored in the IpAddresses db table.
        /// </summary>
        /// <param name="ipBlocks">A comma delimited list of IPv4 addresses in CIDR notation.</param>
        /// <param name="ipConfigId">The Id of the IpConfig that these IpBlocks belong to.</param>
        /// <returns></returns>
        private async Task ProcessIpBlocks(List<string> ipBlocks, long ipConfigId)
        {
            HashSet<string> blocks = new HashSet<string>();

            for (int i = 0; i < ipBlocks.Count; i++)
            {
                // Remove whitespace from string
                ipBlocks.ElementAt(i).Replace(ipBlocks.ElementAt(i), Regex.Replace(ipBlocks.ElementAt(i), @"\s+", ""));

                // Check for duplicate IpBlocks
                if (!blocks.Add(ipBlocks.ElementAt(i)))
                {
                    // TODO - One of the IpBlocks is a duplicate
                    throw new NotSupportedException();
                }
            }

            // For each Ipv4 address in CIDR notation, enumerate all the IPs in the range then add them to ipAddresses
            foreach (string cidr in ipBlocks)
            {
                IpBlock ipBlock = new IpBlock
                {
                    CIDR = cidr,
                    IpConfigId = ipConfigId
                };

                // Store each CIDR IPv4 address in the IpBlocks db table
                _context.IpBlocks.Add(ipBlock);
                await _context._SaveChangesAsync();

                IPNetwork block = IPNetwork.Parse(cidr);

                // Adjust the prefix relative to the IPsPerSub
                byte prefix = (byte)(32 - Math.Log((await _context.IpConfigs.FindAsync(ipConfigId)).IPsPerSub, 2));

                // Break the IpBlock into smaller subnets in CIDR IPv4 notation
                IPNetworkCollection subnets = block.Subnet(prefix);

                // This list will contain all of the enumerated IP address entities for the ipBlock
                List<IpAddress> ipAddressEntities = new List<IpAddress>();

                foreach (IPNetwork network in subnets)
                {
                    IpAddress ip = new IpAddress
                    {
                        Value = network.Value,
                        IsAvailable = true,
                        IpBlockId = ipBlock.Id
                    };

                    ipAddressEntities.Add(ip);
                }

                // Store all of the enumerated IP address entities in the IpAddresses db table
                _context.IpAddresses.AddRange(ipAddressEntities);
                await _context._SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets all the IpBlock CIDR values associated with an IpConfig. 
        /// </summary>
        /// <param name="ipConfigId">The Id of the IpConfig.</param>
        /// <returns>A list of IPv4 CIDR values.</returns>
        private async Task<List<string>> GetIpBlocks(long ipConfigId)
        {
            List<IpBlock> ipBlocks = await _context.IpBlocks.Where(x => x.IpConfigId == ipConfigId).ToListAsync();
            List<string> blocks = new List<string>();

            foreach (IpBlock ipBlock in ipBlocks)
            {
                blocks.Add(ipBlock.CIDR);
            }

            return blocks;
        }

        /// <summary>
        /// Checks to see if the only difference between two IpConfigs is that one
        /// has additional IpBlocks. If this is not the only difference then an error will
        /// be thrown.
        /// </summary>
        /// <param name="original">The original IpConfig that serves as the baseline.</param>
        /// <param name="updated">The updated IpConfig that should only have additional IpBlocks.</param>
        /// <returns>A list of the added IpBlocks.</returns>
        private List<string> OnlyIpBlocksAdded(IpConfig original, IpConfig updated)
        {
            bool same = true;

            same &= original.Name == updated.Name &&
                    original.IPsPerSub == updated.IPsPerSub;

            for (int i = 0; i < original.IpBlocks.Count; i++)
            {
                same &= original.IpBlocks.ElementAt(i) == updated.IpBlocks.ElementAt(i);
            }

            if (!same)
            {
                // TODO - Some other change was made besides adding IPBlocks
                throw new NotSupportedException();
            }

            int firstBlockAddedIndex = original.IpBlocks.Count;
            int numBlocksAdded = updated.IpBlocks.Count - original.IpBlocks.Count;

            return updated.IpBlocks.GetRange(firstBlockAddedIndex, numBlocksAdded);
        }

        /// <summary>
        /// Validates that the properties of the given IpConfig make sense logically.
        /// </summary>
        /// <param name="ipConfig">The IpConfig to validate.</param>
        /// <returns>The validated IpConfig.</returns>
        private IpConfig validateIpConfig(IpConfig ipConfig)
        {
            HashSet<string> blocks = new HashSet<string>();

            // Check that the IPsPerSub is a power of two
            if (!((ipConfig.IPsPerSub != 0) && ((ipConfig.IPsPerSub & (ipConfig.IPsPerSub - 1)) == 0)))
            {
                // TODO
                throw new ArgumentException();
            }

            for (int i = 0; i < ipConfig.IpBlocks.Count; i++)
            {
                // Remove whitespace from string
                ipConfig.IpBlocks[i] = Regex.Replace(ipConfig.IpBlocks.ElementAt(i), @"\s+", "");

                // Check for duplicate IpBlocks
                if (!blocks.Add(ipConfig.IpBlocks.ElementAt(i)))
                {
                    // TODO - One of the IpBlocks is a duplicate
                    throw new ArgumentException();
                }

                // Check that each IpBlock is in valid IPv4 CIDR notation
                if (!Regex.IsMatch(ipConfig.IpBlocks.ElementAt(i), @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\/(3[0-2]|[1-2][0-9]|[0-9]))$"))
                {
                    // TODO
                    throw new ArgumentException();
                }

                int prefix = Int32.Parse(ipConfig.IpBlocks.ElementAt(i).Split('/').ElementAt(1));

                // Check to see that there are not more IPsPerSub than available IP addresses
                if (ipConfig.IPsPerSub > Math.Pow(2, 32 - prefix))
                {
                    // TODO
                    throw new ArgumentException();
                }
            }

            return ipConfig;
        }
    }
}