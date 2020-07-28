# Deploy Luna service to your Azure Subscription

In this article, we are going to show you how can you deploy Luna service in your Azure subscription

## Prepare the deployment environment

Before you continue, you should follow [this document](./get-ready.md) to setup your development environment.

## Clone project Luna GitHub repo

You can find our GitHub repo at aka.ms/lunaai

See [here](https://docs.github.com/en/github/creating-cloning-and-archiving-repositories/cloning-a-repository) about how to clone a GitHub repo.

## Collect required information for deployment

You will need to following information for the deployment:

- Tenant id: The tenant id of your organization.
- Subscription id: The subscription id you want to deploy Luna service to
- Location: Which azure region you want to deploy the Luna service to. It should be in format like "westus2".
- A unique name: it is a string with only **lower case letters** and **less than 12 characters**. It will be used as prefix of all Azure and AAD resources. To avoid any failure during deployment, please make it as unique as possible.
- Admin accounts: the AAD accounts who you want to assign admin permission to your Luna service. It can be mutiple AAD accounts seperated by semicolons.
- (only if you are using any kind of VPN or proxy service) The IP range of your VPN or proxy service: we can add the IP range to Azure SQL Server firewall rule so you can access your SQL database. We will detect your local ip (without VPN or proxy) and add it to the firewall rule automatically. Contact your network admin for the IP range of your VPN or proxy service.

See [this document](../how-to/how-to-find-azure-info.md) for more details about how to find these information.

## Run deployment script

Open a Windows PowerShell window, locate to the local GitHub repo you just cloned. You can find the deployment script under */Resources/Deployment*.

Open a notepad and compose the deployment script:

```powershell
./Deploy.ps1 -uniqueName {unique_name} -location {location} -tenantId {tenant_id} -lunaServiceSubscriptionId {subscription_id} -adminAccounts {adminAccounts}
```

If you are using any VPN or proxy service, add the following arguments:

```powershell
-firewallStartIpAddress {start_ip_address} -firewallEndIpAddress {end_ip_address}
```

**NOTE: for private preview only**
In the end, add the following parameters to get our private preview build:

```powershell
-buildLocation "https://github.com/Azure/AIPlatform/raw/master/end-to-end-solutions/Luna/Resources/Builds/2.0" -sqlScriptFileLocation ".\SqlScripts\v2.0\db_provisioning.sql" -enableV2 true
```

Copy the whole command to the WindowsPowerShell window and run it. The AAD sign-in page will pop up twice, once for signing in to Azure and second time for AAD. The deployment may take up to 40 minutes mainly because it take quite long time to create Azure API Management service.

## Record post deployment service

If the PowerShell command completed with no error, the following information will be printed in the end:

```text
Deployment finished successfully.
You will need the following information when creating a SaaS offer in Azure Partner Center:
Landing page URL:  https://xxxxxx.azurewebsites.net/LandingPage
Connection Webhook:  https://xxxxxx.azurewebsites.net/Webhook
Azure Active Directory tenant ID:  xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Azure Active Directory application ID:  xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

Make sure you save this information in Notepad or other tools.

## Next Step

[Create a ML project using Luna.AI project template](./use-luna-ml-project-template.md)
