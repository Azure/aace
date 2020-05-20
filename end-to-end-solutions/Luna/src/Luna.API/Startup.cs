using System;
using System.IO;
using System.Net;
using System.Reflection;
using Luna.API.Controllers.Admin;
using Luna.Clients;
using Luna.Clients.Azure.APIM;
using Luna.Clients.Azure.APIM.Luna.AI;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Azure.Storage;
using Luna.Clients.CustomMetering;
using Luna.Clients.Exceptions;
using Luna.Clients.Fulfillment;
using Luna.Clients.Models;
using Luna.Clients.Provisioning;
using Luna.Data.Repository;
using Luna.Services;
using Luna.Services.CustomMeterEvent;
using Luna.Services.Data;
using Luna.Services.Data.Luna.AI;
using Luna.Services.Marketplace;
using Luna.Services.Provisoning;
using Luna.Services.Utilities;
using Luna.Services.WebHook;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace Luna.API
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly string apiVersion;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.apiVersion = configuration.GetSection("ApiVersion").Value;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/" + apiVersion + "/swagger.json", "Luna Dashboard API");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseCors(
                options => options
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );

            app.UseExceptionHandler(x =>
            {
                x.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();

                    if (ex != null)
                    {
                        if (ex.Error.GetType().Name.Equals(nameof(LunaUserException)) || ex.Error.GetType().BaseType.Name.Equals(nameof(LunaUserException)))
                        {
                            context.Response.StatusCode = (int)((LunaUserException)ex.Error).HttpStatusCode;
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }

                        string errorMessage = new ErrorModel(ex.Error).ToJson();
                        logger.LogError(ex.Error, ExceptionUtils.GetFormattedDetails(ex.Error));
                        await context.Response.WriteAsync(errorMessage).ConfigureAwait(false);
                    }
                });
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(name: "default", pattern: "api/{route}");
            });

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    });

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => this.configuration.Bind("AzureAd", options));

            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                            .AddAzureADBearer(options => this.configuration.Bind("AzureAd", options));

            AADAuthHelper.AdminList = this.configuration["ISVPortal:AdminAccounts"].Split(';', StringSplitOptions.RemoveEmptyEntries);

            AADAuthHelper.AdminTenantId = this.configuration["ISVPortal:AdminTenant"];

            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // This is a Microsoft identity platform web API.
                options.Authority += "/v2.0";

                // The web API accepts as audiences both the Client ID (options.Audience) and api://{ClientID}.
                options.TokenValidationParameters.ValidAudiences = new[]
                {
     options.Audience,
     $"api://{options.Audience}",
     this.configuration["AzureAD:ClientId"],
     $"api://{this.configuration["AzureAD:ClientId"]}"

    };
                options.ClaimsIssuer = @"https://sts.windows.net/{tenantid}/";

                // Instead of using the default validation (validating against a single tenant,
                // as we do in line-of-business apps),
                // we inject our own multitenant validation logic (which even accepts both v1 and v2 tokens).
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
            });

            services.Configure<CookieAuthenticationOptions>(
                AzureADDefaults.CookieScheme,
                options => options.AccessDeniedPath = "/Subscriptions/NotAuthorized");

            services.Configure<OpenIdConnectOptions>(
                AzureADDefaults.OpenIdScheme,
                options =>
                    {
                        //options.Authority = options.Authority + "/v2.0/"; // Azure AD v2.0

                        options.TokenValidationParameters.ValidateIssuer =
                            false; // accept several tenants (here simplified)
                    });


            services.Configure<DashboardOptions>(this.configuration.GetSection("Dashboard"));

            services.Configure<AzureOptions>(this.configuration.GetSection("Azure"));

            services.Configure<ApiBehaviorOptions>(x =>
            {
                x.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ErrorModel(context);
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });

            services.AddOptions<SecuredFulfillmentClientConfiguration>().Configure(
                options => {
                    this.configuration.Bind("FulfillmentClient", options);
                    this.configuration.Bind("SecuredCredentials:Marketplace", options);
                }
            );
            services.AddHttpClient<IFulfillmentClient, FulfillmentClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                .AddPolicyHandler(
                    (services, request) => HttpPolicyExtensions.HandleTransientHttpError()
                        .OrResult(msg => ExceptionUtils.IsHttpErrorCodeRetryable(msg.StatusCode))
                        .WaitAndRetryAsync(
                            3,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (result, timeSpan, retryCount, context) =>
                            {
                                if (result.Exception != null)
                                {
                                    services.GetService<ILogger<IFulfillmentClient>>()?
                                        .LogWarning($"An exception occurred on retry {retryCount} at {context.OperationKey} with message {result.Exception.Message}");
                                }
                                else
                                {
                                    services.GetService<ILogger<IFulfillmentClient>>()?
                                        .LogError($"An unsuccessful status code {result.Result.StatusCode} was received on retry {retryCount} at {context.OperationKey}");
                                }
                            }
                    ));

            // Hack to save the host name and port during the handling the request. Please see the WebhookController and ContosoWebhookHandler implementations
            services.AddSingleton<ContosoWebhookHandlerOptions>();


            services.TryAddScoped<IWebhookProcessor, WebhookProcessor>();

            services.TryAddScoped<IFulfillmentManager, FulfillmentManager>();

            // Register the provisioning client
            services.AddOptions<SecuredProvisioningClientConfiguration>().Configure(
                options => {
                    this.configuration.Bind("ProvisioningClient", options);
                    this.configuration.Bind("SecuredCredentials:ResourceManager", options);
                }
            );
            //services.TryAddScoped<IProvisioningClient, ProvisioningClient>();
            services.AddHttpClient<IProvisioningClient, ProvisioningClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));


            services.AddHttpClient<ICustomMeteringClient, CustomMeteringClient>();

            // Get the connection string from appsettings.json 
            string connectionString = this.configuration.GetValue<string>(this.configuration["SecuredCredentials:Database:ConnectionString"]);

            // Database context must be registered with the dependency injection (DI) container
            services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register the db context interface
            services.TryAddScoped<ISqlDbContext, SqlDbContext>();

            services.AddOptions<APIMConfigurationOption>().Configure(
                options =>
                {
                    this.configuration.Bind("SecuredCredentials:APIM", options);
                });

            services.AddHttpClient<IProductAPIM, ProductAPIM>();
            services.AddHttpClient<IAPIVersionSetAPIM, APIVersionSetAPIM>();
            services.AddHttpClient<IAPIVersionAPIM, APIVersionAPIM>();
            services.AddHttpClient<IProductAPIVersionAPIM, ProductAPIVersionAPIM>();
            services.AddHttpClient<IOperationAPIM, OperationAPIM>();
            services.AddHttpClient<IPolicyAPIM, PolicyAPIM>();
            services.AddHttpClient<IAPISubscriptionAPIM, APISubscriptionAPIM>();
            services.AddHttpClient<IUserAPIM, UserAPIM>();

            services.AddOptions<StorageAccountConfigurationOption>().Configure(
               options => {
                   this.configuration.Bind("SecuredCredentials:StorageAccount", options);
               }
           );

            // Register the storage utility interface
            services.TryAddScoped<IStorageUtility, StorageUtility>();
            services.TryAddScoped<IKeyVaultHelper, KeyVaultHelper>();
            // Register the provisioning helper
            services.TryAddScoped<IMarketplaceNotificationHandler, ProvisioningHelper>();

            // Register the entity services
            services.TryAddScoped<IOfferService, OfferService>();
            services.TryAddScoped<IArmTemplateService, ArmTemplateService>(); 
            services.TryAddScoped<IPlanService, PlanService>();
            services.TryAddScoped<ISubscriptionService, SubscriptionService>();
            services.TryAddScoped<IAadSecretTmpService, AadSecretTmpService>();
            services.TryAddScoped<IArmTemplateParameterService, ArmTemplateParameterService>();
            services.TryAddScoped<ICustomMeterService, CustomMeterService>();
            services.TryAddScoped<ICustomMeterDimensionService, CustomMeterDimensionService>();
            services.TryAddScoped<IIpConfigService, IpConfigService>();
            services.TryAddScoped<IOfferParameterService, OfferParameterService>();
            services.TryAddScoped<IRestrictedUserService, RestrictedUserService>();
            services.TryAddScoped<IWebhookService, WebhookService>();
            services.TryAddScoped<IWebhookParameterService, WebhookParameterService>();
            services.TryAddScoped<IProvisioningService, ProvisioningService>();
            services.TryAddScoped<IIpAddressService, IpAddressService>();
            services.TryAddScoped<ISubscriptionParameterService, SubscriptionParameterService>();
            services.TryAddScoped<IWebhookWebhookParameterService, WebhookWebhookParameterService>();
            services.TryAddScoped<IArmTemplateArmTemplateParameterService, ArmTemplateArmTemplateParameterService>();
            services.TryAddScoped<ITelemetryDataConnectorService, TelemetryDataConnectorService>();
            services.TryAddScoped<ISubscriptionCustomMeterUsageService, SubscriptionCustomMeterUsageService>();

            services.TryAddScoped<ICustomMeterEventService, CustomMeterEventService>();
            // Register luna db client
            services.AddHttpClient("Luna", x => { x.BaseAddress = new Uri(configuration.GetValue<string>("LunaClient:BaseUri")); });
            services.TryAddScoped<LunaClient>();

            // Register Luna.AI services
            services.TryAddScoped<IProductService, ProductService>();
            services.TryAddScoped<IDeploymentService, DeploymentService>();
            services.TryAddScoped<IAPIVersionService, APIVersionService>();
            services.TryAddScoped<IAPISubscriptionService, APISubscriptionService>();
            services.TryAddScoped<IAMLWorkspaceService, AMLWorkspaceService>();

            services.AddCors();

            services.AddRazorPages();
            
            services.AddApiVersioning(o => {
               DateTime groupVersion = DateTime.ParseExact(apiVersion, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
               ApiVersion latest = new ApiVersion(groupVersion);
               
               o.ReportApiVersions = true;
               o.AssumeDefaultVersionWhenUnspecified = true;
               o.DefaultApiVersion = latest;
               
               o.Conventions.Controller<OfferController>().HasApiVersion(latest);
               o.Conventions.Controller<ArmTemplateController>().HasApiVersion(latest);
               o.Conventions.Controller<PlanController>().HasApiVersion(latest);
               o.Conventions.Controller<SubscriptionController>().HasApiVersion(latest);
               o.Conventions.Controller<ArmTemplateParameterController>().HasApiVersion(latest);
               o.Conventions.Controller<CustomMeterController>().HasApiVersion(latest);
               o.Conventions.Controller<CustomMeterDimensionController>().HasApiVersion(latest);
               o.Conventions.Controller<IpConfigController>().HasApiVersion(latest);
               o.Conventions.Controller<OfferParameterController>().HasApiVersion(latest);
               o.Conventions.Controller<RestrictedUserController>().HasApiVersion(latest);
               o.Conventions.Controller<WebhookController>().HasApiVersion(latest);
               o.Conventions.Controller<WebhookParameterController>().HasApiVersion(latest);
                o.Conventions.Controller<AMLWorkspaceController>().HasApiVersion(latest);
                o.Conventions.Controller<APISubscriptionController>().HasApiVersion(latest);
                o.Conventions.Controller<APIVersionController>().HasApiVersion(latest);
                o.Conventions.Controller<DeploymentController>().HasApiVersion(latest);
                o.Conventions.Controller<ProductController>().HasApiVersion(latest);
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = "Luna Dashboard API", Version = apiVersion });
            
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddApplicationInsightsTelemetry();
        }
    }
}