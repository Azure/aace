# Get Ready to Start

You need to get a few things ready before you can start this tutorial:

## Windows and Windows PowerShell

You can run most of the tutorial in any modern OS. But since AAD PowerShell module is not supported in .netcore version of Azure PowerShell, you will need a Windows machine with Windows PowerShell installed to deploy Luna service.

The easiest way to get a Windows Machine is to create a Windows 10 VM in your Azure Subscription. You can follow this document to create your Windows 10 VM: [Create a Windows Virtual Machine in Azure](https://docs.microsoft.com/en-us/learn/modules/create-windows-virtual-machine-in-azure/). Since we are only going to run a PowerShell script, you can choose the minimum configuration and ignore all advanced settings. Make sure you have RDP enabled.

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

## Azure Subscription

In this tutorial, the Luna service and your models will all be deployed to an Azure subscription. You need to make sure:

- You are a contributor or owner of this Azure subscription
- The following resource providers are enabled in this subscription
  - Microsoft.Network
  - Microsoft.Compute
  - Microsoft.ContainerInstance
  - Microsoft.ContainerService
  - Microsoft.Insights
  - Microsoft.Sql
  - Microsoft.MachineLearningServices
  - Microsoft.Storage
  - Microsoft.ApiManagement
  - Microsoft.KeyVault
  - Microsoft.Web
  - Microsoft.OperationalInsights
- The region where you want to deploy Luna service to is enabled in this subscription

You can find a PowerShell script [CheckPermissions.ps1](../../../Deployment/CheckPermissions.ps1) under *Resources/Deployment* folder of the this repo. You can enable all resource providers, check your permission and region availability by running

```powershell
./CheckPermissions.ps1 -tenantId <your-tenant-id> -subscriptionId <your-subscription-id> -userId <your-aad-user-id> -location <azure-region>
```

where

- the tenant-id is the tenant id of your organization
- the subscription-id is the id of your Azure subscription
- the user-id is your AAD id
- the azure-region is in a format like "West US 2".

See [this document](../how-to/how-to-find-azure-info.md) about how to find those information in Azure portal.

## Azure Active Directory

Azure Active Directory (AAD) authentication is used in Luna services and through this tutorial. You need to make sure:

- Have access to your orgnization's Azure Active Directory
- Have permission to register AAD applications

You can test your permission by running the following PowerShell command

```powershell
Connect-AzureAD -TenantId $tenantId

$app = New-AzureADApplication -DisplayName 'testAADApp'

Remove-AzureADApplication -ObjectId $app.ObjectId
```

It will try to create an AAD application and then delete it.

## Python environment and conda

You will need to prepare your python environment for local testing.

You can install conda following [this instruction](https://docs.conda.io/projects/conda/en/latest/user-guide/install/).

## Visual Studio Code or other IDE

If you don't have any IDE installed on your machine to develop you ML project yet, we'd recommend you install [Visual Studio Code](https://code.visualstudio.com/).

## Git repo

A Git repo allows you manage your ML project and code. It is not required but highly recommened.

The easiest way to get a Git repo is to register and create a repo in GitHub.com. Later in this tutorial, we will show you how to create a GitHub repo using Luna ML project template.

## Azure Storage Account

In this tutorial, the AI service will read and write data from a Azure storage account. If you don't have a Azure storage account, you can follow [the instruction here](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) to create one.

We also highly recommend downloading and installing the [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/). The Azure Storage Explorer can help you easily create, manage and browse blob files in your Azure storage.

## Postman

In this tutorial, we will be testing the AI services by calling REST APIs. For you convinience, we recommed you [install Postman](https://www.postman.com) on your dev machine.

## Notepad or other tools to record information

Scripts and commands we are running in this tutorial may generate information which is needed for futher steps. We recommend you saving those information using Windows Notepad or your favirate text editing tools.

## Next Step

- [Deploy Luna service to your Azure subscription](./setup-luna.md)
