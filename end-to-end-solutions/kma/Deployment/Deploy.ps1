## Copyright (c) Microsoft Corporation.
## Licensed under the MIT license.
param (
    [Parameter(Mandatory=$true)]
    [string]$uniqueName = "default", 
    [string]$resourceGroup = "default",    
    [string]$subscriptionId = "default",
    [string]$location = "centralus",
    [string]$sampleCategory = "none"
)

Connect-AzAccount

if($sampleCategory -notin "none", "healthcare", "oilandgas", "retail"){
    Write-Error "The -sampleCategory needs to be one of the followings: healthcare, oilandgas, retail"
    Break;
}

if($uniqueName -eq "default")
{
    Write-Error "Please specify a unique name."
    break;
}
 
$uniqueName = $uniqueName.Replace('[^a-z0-9]',"").ToLower()
$lenght = $uniqueName.Length
if ($lenght -ge 16){
    $uniqueName = $uniqueName.Substring(0,16)
} 

$prefix = $uniqueName
if($resourceGroup -eq "default"){
    $resourceGroupName = $uniqueName
} else{
    $resourceGroupName = $resourceGroup
}

if($subscriptionId -ne "default"){
    $context = Get-AzSubscription -SubscriptionId $subscriptionId
    Set-AzContext $context
}

New-AzResourceGroup -Name $resourceGroupName -Location $location

$result = New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile .\main.json -prefix $prefix


$storageAccountKey = $result.Outputs["storageAccountKey"].value
$searchServiceKey = $result.Outputs["searchServiceKey"].Value

$sampleContentStorageAccountName = "cognitivesearchcontent"
$webUIAppName = $prefix+"-webui"
$webAPIAppName = $prefix + "-webapi"
$searchServiceName = $prefix + "-search"
$storageAccountName = $prefix + "storage"
$storageContainerName = "rawdata"
$facetFiltersStorageContainerName = "facetfilters"
$facetFiltersSourceContainerName = "facetfilters-" + $sampleCategory
$facetFiltersSourceStorageContainerName = $prefix + "-" + $sampleCategory
$cognitiveServiceName = $prefix + "-cogs"

$headers = @{
'api-key' = $searchServiceKey
'Content-Type' = 'application/json' 
'Accept' = 'application/json' }

$baseSearchUrl = "https://"+$searchServiceName+".search.windows.net"

$storageContext = New-AzStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey

New-AzStorageContainer -Name $storageContainerName -Context $storageContext
New-AzStorageContainer -Name $facetFiltersStorageContainerName -Context $storageContext

$sampleContentStorageContext = New-AzStorageContext -StorageAccountName $sampleContentStorageAccountName -Anonymous

Get-AzStorageBlob -Container $facetFiltersSourceContainerName -Context $sampleContentStorageContext | Start-AzStorageBlobCopy -DestContainer $facetFiltersStorageContainerName -DestContext $storageContext

if ($sampleCategory -ne "none"){
    Get-AzStorageBlob -Container $sampleCategory -Context $sampleContentStorageContext | Start-AzStorageBlobCopy -DestContainer $storageContainerName -DestContext $storageContext
}

$dataSourceBody = Get-Content -Path .\base-datasource.json

$dataSourceBody = $dataSourceBody -replace "%%storageAccountName%%", $storageAccountName
$dataSourceBody = $dataSourceBody -replace "%%storageAccountKey%%", $storageAccountKey
$dataSourceBody = $dataSourceBody -replace "%%storageContainerName%%", $storageContainerName


$url = $baseSearchUrl + "/datasources?api-version=2019-05-06"

Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $dataSourceBody | ConvertTo-Json

$url = $baseSearchUrl + "/skillsets/demoskillset?api-version=2019-05-06-Preview"

$skillBody = Get-Content -Path .\base-skills.json

$cognitiveServiceKeys = Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -Name $cognitiveServiceName

$skillBody = $skillBody -replace "%%cognitiveServiceKey%%", $cognitiveServiceKeys.Key1
$skillBody = $skillBody -replace "%%azure_webapi_name%%", $webAPIAppName
$skillBody = $skillBody -replace "%%storageAccountName%%", $storageAccountName
$skillBody = $skillBody -replace "%%storageAccountKey%%", $storageAccountKey

Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $skillBody | ConvertTo-Json

$url = $baseSearchUrl + "/indexes/demoindex?api-version=2019-05-06"

$indexBody = Get-Content -Path .\base-index.json

Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $indexBody | ConvertTo-Json

$url = $baseSearchUrl + "/indexers/demoindexer?api-version=2019-05-06"

$indexerBody = Get-Content -Path .\base-indexer.json

Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $indexerBody | ConvertTo-Json


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
$appsettings["Customizable"] = "true";

Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $webUIAppName -AppSettings $appsettings

$webUIUrl = "https://"+$prefix+"-webui.azurewebsites.net/"
Start-Process -FilePath $webUIUrl

Write-Host "For Power BI Report:"
Write-Host "Storage Account Name:" $storageAccountName
Write-Host "Storage Account Key:" $storageAccountKey 
