param (
    [string]$uniqueName = "default", 
    [string]$resourceGroup = "default",    
    [string]$subscriptionId = "default",
    [string]$location = "centralus",
    [string]$sampleCategory = "None"
)

##inputs
$subscriptionId = "a6c2a7cc-d67e-4a1a-b765-983f08c0423a"
$uniqueName = "kmaretail"

Connect-AzAccount

if($sampleCategory -notin "None", "healthcare", "oilandgas", "retail"){
    Write-Error "The -sampleCategory needs to be one of the followings: healthcare, oilandgas, retail"
    Break;
}

if($uniqueName -eq "default")
{
    Write-Error "Please specify a unique name."
    break;
}

$uniqueName = $uniqueName.ToLower();

$prefix = $uniqueName
if($resourceGroup -eq "default"){
    $resourceGroupName = $uniqueName
} else{
    $resourceGroupName = $resourceGroup
}

if($subscriptionId -ne "default"){
    $context = Get-AzSubscription -SubscriptionId $subscriptionId
    Set-AzContext @context
}


$sampleContentStorageAccountName = "cognitivesearchcontent"
$webUIAppName = $prefix+"-webui"
$webAPIAppName = $prefix + "-webapi"
$searchServiceName = $prefix + "-search"
$storageAccountName = $prefix + "storage"
$storageContainerName = "rawdata"
$facetFiltersStorageContainerName = "facetfilters"
$facetFiltersSourceStorageContainerName = $prefix + "-" + $sampleCategory
$cognitiveServiceName = $prefix + "-cogs"

$storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Value[0]
$storageAccountKey

$keys = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $searchServiceName 

$searchServiceKey = $keys.Primary 
$searchServiceKey

$appsettings = @{}

$appsettings["AllowedHosts"] = "*";
$appsettings["SearchServiceName"] = $searchServiceName;
$appsettings["SearchServiceKey"] = $searchServiceKey;
$appsettings["SearchServiceApiVersion"] = "2019-05-06";
$appsettings["SearchIndexName"] = "demoindex";
$appsettings["StorageAccountName"] = $storageAccountName;
$appsettings["StorageAccountKey"] = $storageAccountKey;
$appsettings["StorageAccountContainerName"] = $storageContainerName;
$appsettings["FacetsFilteringContainerName"] = $facetFiltersStorageContainerName;

Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $webAPIAppName -AppSettings $appsettings

$appsettings = @{}

$appsettings["ApiProtocol"] = "https"
$appsettings["ApiUrl"] = "https://"+$webAPIAppName+".azurewebsites.net";
$appsettings["OrganizationName"] = "Microsoft";
$appsettings["OrganizationWebSiteUrl"] = "https://www.microsoft.com";
$appsettings["OrganizationLogo"] = "logo.png";
$appsettings["Customizable"] = "false";

Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $webUIAppName -AppSettings $appsettings

$webUIUrl = "https://"+$prefix+"-webui.azurewebsites.net/"
Start-Process -FilePath $webUIUrl
