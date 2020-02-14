using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Clients;
using Luna.Data.DataContracts;
using Luna.Data.Enums;
using Microsoft.Extensions.Logging;

namespace LunaWebJobsApp
{
    public class TaskRunner
    {
        private readonly LunaClient _lunaClient;
        private readonly ILogger<TaskRunner> _logger;

        /// <summary>
        /// constructor for Task runner
        /// </summary>
        /// <param name="lunaClient"></param>
        public TaskRunner(LunaClient lunaClient, ILogger<TaskRunner> logger)
        {
            _lunaClient = lunaClient ?? throw new ArgumentNullException(nameof(lunaClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// The function is responsible to take actions when webjob is run.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            await _lunaClient.ProcessActiveProvisions();
        }
    }
}
