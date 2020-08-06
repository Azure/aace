// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Services.Data
{
    /// <summary>
    /// Service clas that handles basic functionality for the IpAddress resource.
    /// </summary>
    public class IpAddressService: IIpAddressService
    {
        private readonly ISqlDbContext _context;
        private readonly IIpConfigService _ipConfigService;
        private readonly ILogger<IpAddressService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to inject.</param>
        /// <param name="ipConfigService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public IpAddressService(ISqlDbContext sqlDbContext, IIpConfigService ipConfigService, ILogger<IpAddressService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _ipConfigService = ipConfigService ?? throw new ArgumentNullException(nameof(ipConfigService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Trys to assign an IP address to the given subscriptionId. If the IP address is successfully assigned 
        /// then it will be returned. If there are no available IP addresses to assign then an exception 
        /// will be thrown. 
        /// </summary>
        /// <param name="subscriptionId">The subscriptionId to try to assign an IpAddress to.</param>
        /// <param name="offerName">The name of the offer the subscription belongs to.</param>
        /// <param name="ipConfigName">The name of the IpConfig to get an IpAdress from.</param>
        /// <returns>The assigned IpAddress.</returns>
        public async Task<IpAddress> TryAssignIpAddress(Guid subscriptionId, string offerName, string ipConfigName)
        {
            _logger.LogInformation($"Trying to assign IP addresses to subscription ID {subscriptionId} in offer {offerName} from IP Config {ipConfigName}.");

            IpConfig ipConfig = await _ipConfigService.GetAsync(offerName, ipConfigName);

            // We only want to assign IP addresses from IP blocks that belong to the given IP config 
            List<IpBlock> ipBlocks = await _context.IpBlocks.Where(x => x.IpConfigId == ipConfig.Id).ToListAsync();

            // Keep track of IpBlock IDs that belong to the IpConfig
            HashSet<long> blockIds = new HashSet<long>();
            foreach (IpBlock ipBlock in ipBlocks)
            {
                blockIds.Add(ipBlock.Id);
            }

            try
            {
                // Find an IpAddress that is available and belongs to one of the IpBlocks we are tracking
                IpAddress ipAddress = await _context.IpAddresses.Where(x => x.IsAvailable == true && blockIds.Contains(x.IpBlockId)).FirstAsync();

                ipAddress.SubscriptionId = subscriptionId;
                ipAddress.IsAvailable = false;

                _context.IpAddresses.Update(ipAddress);
                await _context._SaveChangesAsync();

                _logger.LogInformation($"The IP addresses in {ipAddress.Value} have been assigned to subscription ID {subscriptionId}.");

                return ipAddress;
            }
            catch (ArgumentNullException)
            {
                throw new LunaServerException($"There are no IP addresses available in {ipConfigName}.");
            }
        }

        /// <summary>
        /// Unassigns all IpAdresses associated with the given subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscriptionId.</param>
        /// <returns></returns>
        public async Task UnassignIpAddresses(Guid subscriptionId)
        {
            _logger.LogInformation($"Unassigning all IP addresses used by subscription ID {subscriptionId}.");

            List<IpAddress> ipAddresses = await _context.IpAddresses.Where(x => x.SubscriptionId == subscriptionId).ToListAsync();

            foreach (IpAddress ipAddress in ipAddresses)
            {
                ipAddress.SubscriptionId = null;
                ipAddress.IsAvailable = true;
            }

            _context.IpAddresses.UpdateRange(ipAddresses);
            await _context._SaveChangesAsync();

            _logger.LogInformation($"Completed unassigning all IP addresses used by subscription ID {subscriptionId}.");
        } 
    }
}