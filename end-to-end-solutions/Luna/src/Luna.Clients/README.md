# Azure Marketplace SaaS Fulfillment API Client

An experimental .NET client implementation for the Azure Marketplace SaaS Fulfillment API V2. 
I built this project as a part of a sample I am developing for demonstrating how SaaS applications can be integrated with Azure Marketplace.

The Azure SaaS Fulfillment API V2 reference is: [here](https://docs.microsoft.com/en-us/azure/marketplace/cloud-partner-portal/saas-app/cpp-saas-fulfillment-api-v2#update-a-subscription).

There is also a Postman collection showing the mock API.

This client is based on the mock API referenced in the article above.

The client is also available as a Nuget package at https://www.nuget.org/packages/AzureMarketplaceSaaSApiClient/

## Using the library

Register a new AAD application as described in the [documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-app-registration) and keep the secret. I recommend you to have a separate AAD application for API integration other than the one used in the landing page. This application can be single-tenant.

The library does not implement certificate authentication yet, but I love to see PRs. Please feel free to submit. So generate a key on the portal, and keep it in your favorite secret location, such as [KeyVault](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.2). I use ```dotnet user-secrets``` for my development.

If you are using dotnet dependency injection, there is an [extension method](https://github.com/Ercenk/AzureMarketplaceSaaSApiClient/blob/master/src/FulfillmentClientServiceCollectionExtensions.cs) for you. Please see the usage in the [test](https://github.com/Ercenk/AzureMarketplaceSaaSApiClient/blob/master/test/SaaSApiClientTests/WebHookTests.cs#L76) for registering the types and inject to the classes using those.

The registration call looks like following in my samples' [startup classes](https://github.com/Ercenk/ContosoAMPBasic/blob/master/src/Dashboard/Startup.cs#L84).

```csharp
            services.AddFulfillmentClient(
                options => this.configuration.Bind("FulfillmentClient", options));
```

### Webhook processing

Implement IWebhookHandler interface to your liking. 

If you are using dotnet dependency injection, again, I have an extension method. You can register the types with,

``` csharp
            services.AddWebhookProcessor().WithWebhookHandler<ContosoWebhookHandler>();
```

The [WebhookProcessor](https://github.com/Ercenk/AzureMarketplaceSaaSApiClient/blob/master/src/WebHook/WebhookProcessor.cs#L77) class takes care of validating the webhook call by the AMP commerce engine, and calls the handler's appropriate methods. Then call the ```ProcessWebhookNotificationAsync``` method in your webhook endpoint code.