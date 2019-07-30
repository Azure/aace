Setup your environment first:
If latest Az or Az.Search module is not installed, following the instruction at https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-2.4.0 to install it first. You can also run .\setup.ps1 in a PowerShell window with admin privilege.

Deployment:
1. Choose a unique name
2. If you don't want to deploy to the default subscription, get the subscription id
3. Download all files in this folder to a local folder
4. Run following command:
.\Deploy.ps1 -uniqueName <unique_name> -subscriptionId <subscription_id>

Other parameters:
-resourceGroup: The resource group name. By default it will use the -uniqueName
-location: The location of all resources. By default it will be centralus
-sampleCategory: The sample data which will be copied to the storage. Value should be one of the followings: healthcare, oilandgas, retail

In the end of deployment, the script will open the demo web UI page in your default browser. You may need to wait for 1 to 2 minutes for the indexing to finish before querying any data.