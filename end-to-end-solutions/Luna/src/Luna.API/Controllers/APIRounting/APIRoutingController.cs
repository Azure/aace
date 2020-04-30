using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for product resource.
    /// </summary>
    // [Authorize]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class APIRoutingController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public APIRoutingController(ILogger<ProductController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

    }
}