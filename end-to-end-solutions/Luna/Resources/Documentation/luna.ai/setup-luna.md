# Deploy Luna service to your Azure Subscription

In this article, we are going to show you how can you deploy Luna service in your Azure subscription

## Prepare the deployment environment

To use our deployment PowerShell script to deploy Luna service, you need to run it on a Windows machine with Windows PowerShell since the AAD module is not supported in the .netcore version of PowerShell yet.

### Create a Windows 10 Virtual Machine in Azure

The easiest way to get a Windows Machine is to create a Windows 10 VM in your Azure Subscription. You can follow this document to create your Windows 10 VM: [Create a indows Virtual Machine in Azure](https://docs.microsoft.com/en-us/learn/modules/create-windows-virtual-machine-in-azure/). Since we are only going to run a PowerShell script, you can choose the minimum configuration and ignore all advanced settings. Make sure you have RDP enabled.

### Install Windows PowerShell and modules

If Windows PowerShell is not installed on your machine, install it following this [instruction](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-windows-powershell?view=powershell-6).

Then start a Windows PowerShell window with Administrator permission and run the following command

```PowerShell
Set-ExecutionPolicy -ExecutionPolicy unrestricted

Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

Choose *A* to change the policy to Yes to All.

Then run the following commands to install PowerShell modules:

```PowerShell
Install-Module -Name Az -AllowClobber

Install-Module -Name AzureAD -AllowClobber

Install-Module -Name sqlserver -AllowClobber
```

After all these, reboot the VM or machine.

## Clone project Luna GitHub repo

You can find the repo at aka.ms/projectluna

TODO: more detailed instruction

## Collect required information for deployment

You will need to following information for the deployment:

- Tenant id: The tenant id of your organization. You can find it by going to "Azure Active Directory" in your Azure portal and look at the "Tenant Information".
- Subscription id: You can find it by going to "Subscriptions" in your Azure portal.
- Location: Which azure region you want to deploy the Luna service to
- A unique name: it is a string with only **lower case letters** and **less than 12 characters**. It will be used as prefix of all Azure and AAD resources. To avoid any failure during deployment, please make it as unique as possible.
- Admin accounts: the AAD accounts who you want to assign admin permission to your Luna service. It can be mutiple AAD accounts seperated by semicolons
- (only if you are using any kind of VPN or proxy service) The IP range of your VPN or proxy service: we can add the IP range to Azure SQL Server firewall rule so you can access your SQL database. We will detect your local ip (without VPN or proxy) and add it to the firewall rule automatically.

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

TODO: remove after private preview
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

Make sure you save this information in a Window Notepad or other tools.

## Next Step

[Create Azure Machine Learning service and configure compute resources](./create-and-configure-aml.md)
