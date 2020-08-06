## Copyright (c) Microsoft Corporation.
## Licensed under the MIT license.
﻿param (
    [string]$resourceGroup = "default",    
    [string]$subscriptionId = "default",
    [string]$location = "centralus",
    [string]$sampleCategory = "None"
)


##inputs
$subscriptionId = "a6c2a7cc-d67e-4a1a-b765-983f08c0423a"
$uniqueName = "kmahc"


Connect-AzAccount


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

$headers = @{
'api-key' = $searchServiceKey
'Content-Type' = 'application/json' 
'Accept' = 'application/json' }


$baseSearchUrl = "https://"+$searchServiceName+".search.windows.net"

$url = $baseSearchUrl + "/indexers/demoindexer?api-version=2019-05-06"

Invoke-RestMethod -Uri $url -Headers $headers -Method Delete | ConvertTo-Json
$url = $baseSearchUrl + "/indexes/demoindex?api-version=2019-05-06"

Invoke-RestMethod -Uri $url -Headers $headers -Method Delete | ConvertTo-Json
$url = $baseSearchUrl + "/skillsets/demoskillset?api-version=2019-05-06-preview"

Invoke-RestMethod -Uri $url -Headers $headers -Method Delete | ConvertTo-Json
$url = $baseSearchUrl + "/datasources/demodata?api-version=2019-05-06"

Invoke-RestMethod -Uri $url -Headers $headers -Method Delete | ConvertTo-Json


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

