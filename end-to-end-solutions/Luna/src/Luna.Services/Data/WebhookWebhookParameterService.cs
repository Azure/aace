// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
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
    public class WebhookWebhookParameterService : IWebhookWebhookParameterService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<WebhookWebhookParameterService> _logger;

        public WebhookWebhookParameterService(ISqlDbContext sqlDbContext, ILogger<WebhookWebhookParameterService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all join entries from the webhookWebhookParameters table that have a reference to the
        /// given webhookId.
        /// </summary>
        /// <param name="webhookId">The webhookId to filter by.</param>
        /// <returns>A list of WebhookWebhookParameters with a reference to the given webhookId.</returns>
        public async Task<List<WebhookWebhookParameter>> GetAllJoinEntries(long webhookId)
        {
            return await _context.WebhookWebhookParameters.Where(x => x.WebhookId == webhookId).ToListAsync();
        }

        /// <summary>
        /// Creates an entry in the webhookWebhookParameters table if it does not exist.
        /// </summary>
        /// <param name="webhookId">The ID of the webhook.</param>
        /// <param name="webhookParameterId">The ID of the webhookParameter.</param>
        /// <returns></returns>
        public async Task CreateJoinEntryAsync(long webhookId, long webhookParameterId)
        {
            if (await ExistsAsync(webhookId, webhookParameterId))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(WebhookWebhookParameter).Name,
                    $"({webhookId}, {webhookParameterId})"));
            }

            WebhookWebhookParameter webhookWebhookParameter = new WebhookWebhookParameter
            {
                WebhookId = webhookId,
                WebhookParameterId = webhookParameterId
            };

            _context.WebhookWebhookParameters.Add(webhookWebhookParameter);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(WebhookWebhookParameter).Name, $"(webhookId={webhookId}, webhookParameterId={webhookParameterId})"));
        }

        /// <summary>
        /// Removes all entries from the webhookWebhookParameters table with a reference to the given webhookId.
        /// </summary>
        /// <param name="webhookId">The ID to filter by.</param>
        /// <returns></returns>
        public async Task DeleteWebhookJoinEntriesAsync(long webhookId)
        {
            List<WebhookWebhookParameter> webhookWebhookParameters = await _context.WebhookWebhookParameters.Where(x => x.WebhookId == webhookId).ToListAsync();
            
            _context.WebhookWebhookParameters.RemoveRange(webhookWebhookParameters);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(WebhookWebhookParameter).Name, $"webhookId={webhookId}"));
        }

        /// <summary>
        /// Checks if the given webhookParameterId has any associations with webhooks that do not have the same 
        /// ID as the webhookId provided.
        /// </summary>
        /// <param name="webhookId">The webhookId to check against.</param>
        /// <param name="webhookParameterId">The webhookParameterId to filter by.</param>
        /// <returns>True if any other associations are found, false otherwise.</returns>
        public async Task<bool> ParameterExistsInDifferentWebhooks(long webhookId, long webhookParameterId)
        {
            return await _context.WebhookWebhookParameters.Where(x => x.WebhookParameterId == webhookParameterId && x.WebhookId != webhookId).CountAsync() > 0;
        }

        /// <summary>
        /// Checks to see if an webhookWebhookParameters entry already exists with the same given IDs.
        /// </summary>
        /// <param name="webhookId">The webhookId to check against.</param>
        /// <param name="webhookParameterId">The webhookParameterId to check against.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(long webhookId, long webhookParameterId)
        {
            var count = await _context.WebhookWebhookParameters.Where(x => x.WebhookParameterId == webhookParameterId && x.WebhookId == webhookId).CountAsync();

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(WebhookWebhookParameterService).Name, $"(webhookId={webhookId}, webhookParameterId={webhookParameterId})"));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(WebhookWebhookParameterService).Name, $"(webhookId={webhookId}, webhookParameterId={webhookParameterId})", false));
                return false;
            }
            else
            {
                // count = 1
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(WebhookWebhookParameterService).Name, $"(webhookId={webhookId}, webhookParameterId={webhookParameterId})", false));
                return true;
            }
        }
    }
}
