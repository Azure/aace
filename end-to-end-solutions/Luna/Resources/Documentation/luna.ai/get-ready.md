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

## Python environment and conda

You will need to prepare your python environment for local testing.

You can install conda following [this instruction](https://docs.conda.io/projects/conda/en/latest/user-guide/install/).

## Visual Studio Code or other IDE

If you don't have any IDE installed on your machine to develop you ML project yet, we'd recommend you install [Visual Studio Code](https://code.visualstudio.com/).

## Git repo

A Git repo allows you manage your ML project and code. It is not required but highly recommened.

The easiest way to get a Git repo is to register and create a repo in GitHub.com

## Azure Storage Account

In this tutorial, the AI service will read and write data from a Azure storage account. If you don't have a Azure storage account, you can follow [the instruction here](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) to create one.

We also highly recommend downloading and installing the [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/). The Azure Storage Explorer can help you easily create, manage and browse blob files in your Azure storage.

## Install Postman

We will be calling REST APIs during this tutorial. For you convinience, we recommed you [install Postman](https://www.postman.com) on your dev machine.

## Notepad or other tools to record information

Scripts and commands we are running in this tutorial may generate information which is needed for futher steps. We recommend you saving those information using Windows Notepad or your favirate tools.

## Next Step

- [Deploy Luna service to your Azure subscription](./setup-luna.md)
