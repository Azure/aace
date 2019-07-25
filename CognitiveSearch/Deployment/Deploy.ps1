param (
    [Parameter(Mandatory=$true)]
    [string]$uniqueName = "default", 
    [string]$resourceGroup = "default",    
    [string]$subscriptionId = "default",
    [string]$location = "centralus",
    [string]$sampleCategory = "None"
)

Connect-AzAccount

if($sampleCategory -notin "None", "healthcare", "oilandgas", "retail"){
    Write-Error "The -sampleCategory needs to be one of the followings: healthcare, oilandgas, retail"
    Break;
}

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

New-AzResourceGroup -Name $resourceGroupName -Location $location

New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile .\main.json -prefix $prefix


$sampleContentStorageAccountName = "cognitivesearchcontent"
$webUIAppName = $prefix+"-webui"
$webAPIAppName = $prefix + "-webapi"
$searchServiceName = $prefix + "-search"
$storageAccountName = $prefix + "storage"
$storageContainerName = $prefix + "rawdata"
$cognitiveServiceName = $prefix + "-cogs"

$keys = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $searchServiceName

$searchServiceKey = $keys.Primary

$headers = @{
'api-key' = $searchServiceKey
'Content-Type' = 'application/json' 
'Accept' = 'application/json' }

$baseSearchUrl = "https://"+$searchServiceName+".search.windows.net"


$storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Value[0]

$storageContext = New-AzStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey

New-AzStorageContainer -Name $storageContainerName -Context $storageContext

if ($sampleCategory -ne "None"){
    $sampleContentStorageContext = New-AzStorageContext -StorageAccountName $sampleContentStorageAccountName -Anonymous

    Get-AzStorageBlob -Container $sampleCategory -Context $sampleContentStorageContext | Start-AzStorageBlobCopy -DestContainer $storageContainerName -DestContext $storageContext
}

$dataSourceBody = Get-Content -Path .\base-datasource.json

$dataSourceBody = $dataSourceBody -replace "%%storageAccountName%%", $storageAccountName
$dataSourceBody = $dataSourceBody -replace "%%storageAccountKey%%", $storageAccountKey
$dataSourceBody = $dataSourceBody -replace "%%storageContainerName%%", $storageContainerName


$url = $baseSearchUrl + "/datasources?api-version=2019-05-06"

Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $dataSourceBody | ConvertTo-Json

$url = $baseSearchUrl + "/skillsets/demoskillset?api-version=2019-05-06"

$skillBody = Get-Content -Path .\base-skills.json

$cognitiveServiceKeys = Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -Name $cognitiveServiceName

$skillBody = $skillBody -replace "%%cognitiveServiceKey%%", $cognitiveServiceKeys.Key1

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


Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $webAPIAppName -AppSettings $appsettings

$appsettings = @{}

$appsettings["ApiProtocol"] = "https"
$appsettings["ApiUrl"] = "https://"+$webAPIAppName+".azurewebsites.net";
$appsettings["OrganizationName"] = "Microsoft";
$appsettings["OrganizationWebSiteUrl"] = "https://www.microsoft.com";
$appsettings["OrganizationLogo"] = "org-logo.svg";

Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $webUIAppName -AppSettings $appsettings

