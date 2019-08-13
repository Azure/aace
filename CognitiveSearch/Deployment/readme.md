# Cognitive Search sales accelerator deployment guidance

This repo contains all code, instructions, and data required to deploy the ACE Team's Knowledge Mining Accelerator: [KMA](http://aka.ms/kma).

This solution was build to help KM demos, POCs, MVPs, and etc.

## Pre Requisites

These are the key pre-requisites to deploy this solution:

1. You need a Microsoft Azure account to create the services used in this solution. You can create a [free account](https://azure.microsoft.com/en-us/free/), use your MSDN account, or any other subscription where you have permission to create Azure services.

2. PowerShell: The one-command deployment process uses PowerShell to execute all the required activities to get the solution up and running. If you don't have PowerShell, install it from [here](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-windows-powershell?view=powershell-6)

## Environment Setup

Follow these steps to prepare the deployment:

### Step 1 - Download the required code to you computer

The deployment runs locally in your computer. You have 2 alternative methods to do it:

1. Download and extract the file **deployment package.zip** located [here](https://github.com/Azure/AIPlatform/tree/master/CognitiveSearch/Deployment)
1. Clone the repo, using [Git for Windows](https://gitforwindows.org/) or any other git app you want. The command is ```git clone https://github.com/Azure/https://github.com/Azure/AIPlatform.git```

### Step 2 - Install Azure PowerShell Module

The deployment process is based on this PowerShell module. If you don't have it on your computer, choose one of these two methods to install it:

1. Follow [these](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-2.4.0) instructions

1. Using `cd` commands, navigate to the cloned or extracted folder and run ```.\setup.ps1``` in a PowerShell terminal. If you get a permission related error, run the terminal as an administrator and run command again.

### Step 3 - PowerShell Privileges

You need to change PowerShell permissions to run all necessary commands:

+ Run a PowerShell terminal
+ Connect to Azure running ```Connect-AzAccount```
+ Set the priorities running ```Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass```. Choose  ```"A"```, to change the policy to **Yes to All**. If you get a permission error, you can try:

     1. Run the PowerShell terminal as an Administrator
     1. Set the priorities running ```Set-ExecutionPolicy -ExecutionPolicy unrestricted```.

## Deployment

1. Choose a unique name. It will be used as prefix of name of all Azure resources. If the name is already used, the deployment will fail.
1. If you don't want to deploy to the default subscription, get the subscription id from the Azure Portal.
1. Using the information collected in the 2 previous items, run following command: `.\Deploy.ps1 -uniqueName <unique_name> -subscriptionId <subscription_id>`
1. If you are not logged in, the script will ask you to do it.
In the end of deployment, the script will open the demo web UI page in your default browser. There will be no data. Use the **Upload Files** link to upload your data. You may need to wait for 1 to 4 minutes for the indexing to finish before querying any data. It will also print the storage account name and key for Power BI report.

### Additional parameters

+ ```-resourceGroup```: The resource group name. By default it will use the informed **-uniqueName**
+ ```-location```: The location of all resources. By default it will be **centralus**
+ ```-sampleCategory```: The sample data which will be copied to the storage. Value should be one of the followings: healthcare, oilandgas, retail

## Power BI Report

You can update the Power BI report to load data from your own knowledge store:

1. The deployment script will print the storage account name and storage account key in the end of deployment.
2. Open Power BI desktop
3. Click Edit Queries dropdown and choose "Data source settings"
4. Select the first data source, then click "Change source"
5. Enter the name of the storage account
6. Click OK
7. Click "Edit permissions"
8. Click Edit under Credentials
9. Update the account key with the primary or secondary key
10. Click Save
11. Close all windows and refresh your data sources

## Architecture Diagram

The solution architecture diagram is shared not only as the image below, but also its Visio file, located in the **Architecture** folder of the repo. You are free do use it or customize it as you want.

![Diagram](./architecture/diagram.PNG)

## Pricing and SLA

The final price depends on multiple factors like contract type, account type, partner, taxes, discounts, and etc. These factors are beyond the scope of this section. You can use the [Azure Calculator](https://azure.microsoft.com/en-us/pricing/calculator/) for pricing **estimation**. The used Azure Services are:

+ Storage Account
+ Azure Search - Standard tier
+ Azure Cognitive Services
+ App Service (x2) with one instance for Service plan.
Actual prices may vary depending upon the date of purchase, currency of payment, and type of agreement you have with Microsoft. Contact a Microsoft sales representative for additional information on pricing. The size of your dataset also will have influence in the final price. Your dataset size is a factor beyond our control.

The order of magnitude of the estimated cost to keep this solution running is close to **US$ 350.50** per month. Please check [this](https://docs.microsoft.com/en-us/azure/search/search-limits-quotas-capacity) link to understand the standard sku tier limits.

If you want to reduce the costs, you can change the **searchSku / defaultValue** property of the **main.json** file, located in the **Deployment** folder. The provided value is **standard**, and you can replace it with the **basic** or **free** sku tiers. However, by doing this, we can't predict/support the errors and the performance of your deployment. Use [this](https://docs.microsoft.com/en-us/azure/search/search-sku-tier) link to guide you on how to choose an Azure Search sku tier.

This solution was not designed to provide SLA. If you need 99.9% of uptime, click [here](https://azure.microsoft.com/en-us/support/legal/sla/search/v1_0/) to learn more about Azure Search SLA.

> If you want to eliminate the costs at any moment after the deployment, we recommend you delete the created **resource group**.

## Additional Content - Knowledge Mining

+ [ACE Team Blog](http://aka.ms/ACE-Blog)
+ [ACE Team KMA Demo](http://aka.ms/kma)
+ [Learn how to use Cognitive Search on Audio Files](https://techcommunity.microsoft.com/t5/AI-Customer-Engineering-Team/Mine-knowledge-from-audio-files-with-Microsoft-AI/ba-p/781957)
+ [Knowledge Mining Bootcamp](http://aka.ms/kmb)
