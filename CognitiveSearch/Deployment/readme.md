# Cognitive Search sales accelarator deployment guidance

## Setup environment:
If latest Az or Az.Search module is not installed, following the instruction at https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-2.4.0 to install it first. You can also run .\setup.ps1 in a PowerShell window with admin privilege.

## Prepare:
1. Choose a unique name. It will be used as prefix of name of all Azure resources. If the name is already used, the deployment will fail.
2. If you don't want to deploy to the default subscription, get the subscription id.

## Deploy:
1. Sync the current repo or download the [deployment package.zip](deployment package.zip)
2. Run following command:
.\Deploy.ps1 -uniqueName <unique_name> -subscriptionId <subscription_id>

In the end of deployment, the script will open the demo web UI page in your default browser. You may need to wait for 1 to 2 minutes for the indexing to finish before querying any data. It will also print the storage account name and key for Power BI report.

### Additional parameters:
-resourceGroup: The resource group name. By default it will use the -uniqueName
-location: The location of all resources. By default it will be centralus
-sampleCategory: The sample data which will be copied to the storage. Value should be one of the followings: healthcare, oilandgas, retail

## Power BI Report:
You can update the Power BI report to load data from your own knowledge store:

1. The deployment script will print the storage account name and storage account key in the end of deployment.
2. Open PowerBI desktop
3. Click Edit Queries dropdown and choose "Data source settings"
4. Select the first data source, then click "Change source"
5. Enter the name of the storage account
6. Click OK
7. Click "Edit permissions"
8. Click Edit under Credentials
9. Update the account key with the primary or secondary key
10. Click Save
11. Close all windows and refresh your data sources
