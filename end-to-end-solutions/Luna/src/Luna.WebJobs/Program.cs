// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.IO;
using System.Threading.Tasks;
using Luna.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LunaWebJobsApp
{
    class Program
    {
        /// <summary>
        /// Load configuration
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true,
                             reloadOnChange: true);

            return builder.Build();
        }

        static async Task Main(string[] args)
        {

            var apiServiceUrl = Environment.GetEnvironmentVariable("WebJob:APIServiceUrl");

            if (string.IsNullOrEmpty(apiServiceUrl))
            {
                IConfigurationRoot configuration = LoadConfiguration();
                apiServiceUrl = configuration.GetValue<string>("LunaClient:BaseUri");
            }

            IServiceCollection services = new ServiceCollection();

            services.AddHttpClient("Luna", x => { x.BaseAddress = new Uri(apiServiceUrl); });
            services.AddScoped<LunaClient>();

            services
                .AddLogging()
                .AddScoped<TaskRunner>()
                .BuildServiceProvider();

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var runner = serviceProvider.GetService<TaskRunner>();

            // Entrypoint of the webjob
            await runner.Run();

        }
    }
}
