1. Choose a unique name
2. If you don't want to deploy to the default subscription, get the subscription id
3. Download all files in this folder to a local folder
4. (Optional if you already installed Az and Az.Search modules) run .\setup.ps1 in an admin window to install Az and Az.Search modules
5. Run following command:
.\Deploy.ps1 -uniqueName <unique_name> -subscriptionId <subscription_id>

Other parameters:
-resourceGroup: The resource group name. By default it will use the -uniqueName
-location: The location of all resources. By default it will be centralus
-sampleCategory: The sample data which will be copied to the storage. Value should be one of the followings: healthcare, oilandgas, retail