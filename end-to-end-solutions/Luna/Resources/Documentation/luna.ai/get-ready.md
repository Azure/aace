# Get Ready to Start

There are few things you need to prepare before you get started to make sure you can run this tutorial end to end:

## Azure Subscription

As a part of the tutorial, you will be deploying Luna service to your own Azure subscription. You need to make sure:

- You are a contributor, owner or admin of this Azure subscription
- The region where you want to deploy Luna service to is enabled in this subscription
- The following resource providers are enabled in this subscription
  - Microsoft.Network
  - Microsoft.Compute
  - Microsoft.ContainerInstance
  - Microsoft.ContainerService
  - Microsoft.Insight
  - Microsoft.Sql
  - Microsoft.MachineLearningServices
  - Microsoft.Storage
  - Microsoft.ApiManagement
  - Microsoft.KeyVault
  - Microsoft.Web
  - Microsoft.OperationalInsights
- This subscription has enough quota for each of the resources listed above. We need at least one instance for each resource.

## Azure Active Directory

Azure Active Directory (AAD) authentication is used in Luna services and through this tutorial. You need to make sure:

- Have access to your orgnization's Azure Active Directory
- Have permission to register AAD applications

## Windows and Windows PowerShell

You can run most of the tutorial in any modern OS. But since AAD module is not supported in .netcore version of Azure PowerShell, you will need a Windows machine with Windows PowerShell installed to deploy Luna service.

The easiest way to setup this environment is to create an Windows 10 Azure VM. We will show you how to do that in the next step.

## Git repo

A Git repo allows you manage your ML project and code. It is not required but highly recommened.

The easiest way to get a Git repo is to register and create a repo in GitHub.com

## Notepad or other tools to record information

Scripts and commands we are running in this tutorial may generate information which is needed for futher steps. We recommend you saving those information using Windows Notepad or your favirate tools.

## Next Step

- [Deploy Luna service to your Azure subscription](./setup-luna.md)
