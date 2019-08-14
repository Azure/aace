using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
using CognitiveSearch.Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveSearch.WebApi
{
    public class Startup
    {
        private const string AllowCorsPolicy = "AllowAllCorsPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AllowCorsPolicy, builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var appInsightsConfig = new AppInsightsConfig
            {
                InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]
            };
            services.AddSingleton(appInsightsConfig);
            services.AddApplicationInsightsTelemetry(appInsightsConfig.InstrumentationKey);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var searchConfig = new SearchConfig
            {
                ServiceName = Configuration["SearchServiceName"],
                Key = Configuration["SearchServiceKey"],
                ApiVersion = Configuration["SearchServiceApiVersion"],
                IndexName = Configuration["SearchIndexName"]
            };
            services.AddSingleton(searchConfig);

            var storageConfig = new BlobStorageConfig
            {
                AccountName = Configuration["StorageAccountName"],
                Key = Configuration["StorageAccountKey"],
                ContainerName = Configuration["StorageAccountContainerName"],
                FacetsFilteringContainerName = Configuration["FacetsFilteringContainerName"]
            };
            services.AddSingleton(storageConfig);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(AllowCorsPolicy);

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}