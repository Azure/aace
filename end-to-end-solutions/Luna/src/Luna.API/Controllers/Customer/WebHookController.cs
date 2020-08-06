// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Services.Utilities;
using Luna.Services.WebHook;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luna.API.Controllers.Customer
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("webhook")]
    public class WebHookController : Controller
    {
        private readonly ILogger<WebHookController> _logger;
        private readonly IWebhookProcessor _webhookProcessor;

        public WebHookController(IWebhookProcessor webhookProcessor, IOptionsMonitor<DashboardOptions> optionsMonitor, ILogger<WebHookController> logger)
        {
            _webhookProcessor = webhookProcessor ?? throw new ArgumentNullException(nameof(webhookProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] WebhookPayload payload)
        {
            _logger.LogInformation($"Received webhook request: {JsonSerializer.Serialize(payload)}");
            await _webhookProcessor.ProcessWebhookNotificationAsync(payload);
            return this.Ok();
        }
    }
}