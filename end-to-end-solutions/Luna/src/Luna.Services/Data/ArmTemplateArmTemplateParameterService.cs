// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public class ArmTemplateArmTemplateParameterService : IArmTemplateArmTemplateParameterService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<ArmTemplateArmTemplateParameterService> _logger;

        public ArmTemplateArmTemplateParameterService(ISqlDbContext sqlDbContext, ILogger<ArmTemplateArmTemplateParameterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all join entries from the armTemplateArmTemplateParameters table that have a reference to the
        /// given armTemplateId.
        /// </summary>
        /// <param name="armTemplateId">The armTemplateId to filter by.</param>
        /// <returns>A list of ArmTemplateArmTemplateParameters with a reference to the given armTemplateId.</returns>
        public async Task<List<ArmTemplateArmTemplateParameter>> GetAllJoinEntries(long armTemplateId)
        {
            return await _context.ArmTemplateArmTemplateParameters.Where(x => x.ArmTemplateId == armTemplateId).ToListAsync();
        }

        /// <summary>
        /// Creates an entry in the armTemplateArmTemplateParameters table if it does not exist.
        /// </summary>
        /// <param name="armTemplateId">The ID of the armTemplate.</param>
        /// <param name="armTemplateParameterId">The ID of the armTemplateParameter.</param>
        /// <returns></returns>
        public async Task CreateJoinEntryAsync(long armTemplateId, long armTemplateParameterId)
        {
            if (await ExistsAsync(armTemplateId, armTemplateParameterId))
            {
                throw new LunaServerException("This entry cannot be created since it already exists in the DB.", false);
            }

            ArmTemplateArmTemplateParameter armTemplateArmTemplateParameter = new ArmTemplateArmTemplateParameter
            {
                ArmTemplateId = armTemplateId,
                ArmTemplateParameterId = armTemplateParameterId
            };

            _context.ArmTemplateArmTemplateParameters.Add(armTemplateArmTemplateParameter);
            await _context._SaveChangesAsync();
        }

        /// <summary>
        /// Removes all entries from the armTemplateArmTemplateParameters table with a reference to the given armTemplateId.
        /// </summary>
        /// <param name="armTemplateId">The ID to filter by.</param>
        /// <returns></returns>
        public async Task DeleteArmTemplateJoinEntriesAsync(long armTemplateId)
        {
            List<ArmTemplateArmTemplateParameter> armTemplateArmTemplateParameters = await _context.ArmTemplateArmTemplateParameters.Where(x => x.ArmTemplateId == armTemplateId).ToListAsync();

            _context.ArmTemplateArmTemplateParameters.RemoveRange(armTemplateArmTemplateParameters);
            await _context._SaveChangesAsync();
        }

        /// <summary>
        /// Checks if the given armTemplateParameterId has any associations with armTemplates that do not have the same 
        /// ID as the armTemplateId provided.
        /// </summary>
        /// <param name="armTemplateId">The armTemplateId to check against.</param>
        /// <param name="armTemplateParameterId">The armTemplateParameterId to filter by.</param>
        /// <returns>True if any other associations are found, false otherwise.</returns>
        public async Task<bool> ParameterExistsInDifferentArmTemplates(long armTemplateId, long armTemplateParameterId)
        {
            return await _context.ArmTemplateArmTemplateParameters.Where(x => x.ArmTemplateParameterId == armTemplateParameterId && x.ArmTemplateId != armTemplateId).CountAsync() > 0;
        }

        /// <summary>
        /// Checks to see if an armTemplateArmTemplateParameters entry already exists with the same given IDs.
        /// </summary>
        /// <param name="armTemplateId">The armTemplateId to check against.</param>
        /// <param name="armTemplateParameterId">The armTemplateParameterId to check against.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(long armTemplateId, long armTemplateParameterId)
        {
            var count = await _context.ArmTemplateArmTemplateParameters.Where(x => x.ArmTemplateParameterId == armTemplateParameterId && x.ArmTemplateId == armTemplateId).CountAsync();

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new LunaServerException("More than once instance of the same resource exists in the DB.", false);
            }
            else if (count == 0)
            {
                return false;
            }
            else
            {
                // count = 1
                return true;
            }
        }
    }
}