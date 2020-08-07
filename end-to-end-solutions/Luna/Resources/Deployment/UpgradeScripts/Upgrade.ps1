## Copyright (c) Microsoft Corporation.
## Licensed under the MIT license.

﻿param (
    [Parameter(Mandatory=$true)]
    [string]$uniqueName = "default", 

    [string]$location = "centralus",
    
    [string]$targetVersion = "latest",
    
    [string]$tenantId = "default",

    [string]$accountId = "default",
      
    [string]$lunaServiceSubscriptionId = "default",
    
    [string]$userApplicationSubscriptionId = "default", 

    [string]$resourceGroupName = "default", 
    
    [string]$keyVaultName = "default",
    
    [string]$sqlServerName = "default",
    
    [string]$sqlDatabaseName = "default",
    
    [string]$StorageName = "default",
    
    [string]$appServicePlanName = "default",
    
    [string]$isvWebAppName = "default",
    
    [string]$enduserWebAppName = "default",
    
    [string]$apiWebAppName = "default",

    [string]$apiWebJobName = "default",
    
    [string]$apiWebAppInsightsName = "default",

    [string]$azureMarketplaceAADApplicationName = "default",

    [string]$azureMarketplaceAADApplicationId = "00000000-0000-0000-0000-000000000000",

    [string]$azureResourceManagerAADApplicationName = "default",

    [string]$azureResourceManagerAADApplicationId = "00000000-0000-0000-0000-000000000000",

    [string]$webAppAADApplicationName = "default",

    [string]$webAppAADApplicationId = "00000000-0000-0000-0000-000000000000",

    [string]$sqlServerAdminUsername = "cloudsa",

    [int]$keyExpirationByMonth = 12,

    [string]$firewallStartIpAddress = "clientIp",

    [string]$firewallEndIpAddress = "clientIp",

    [string]$headerBackgroundColor = "#004578",

    [string]$enableV1 = "true",

    [string]$enableV2 = "false",

    [string]$buildLocation = "https://github.com/Azure/AIPlatform/raw/master/end-to-end-solutions/Luna/Resources/Builds/",

    [string]$companyName = "Microsoft",

    [string]$adminTenantId = "common",

    [string]$adminAccounts = "default"

)

Clear-AzContext -Force

if($tenantId -ne "default"){
    Connect-AzureAD -TenantId $tenantId
    
    Connect-AzAccount -Tenant $tenantId
}
else{
    Connect-AzureAD
    Connect-AzAccount
}

$sqlVersionTable = @{
    "1.0" = 1;
    "1.1" = 2;
    "latest" = 2;
}


function GetNameForAzureResources{
    param($uniqueName, $defaultName, $resourceTypeSuffix)
    if ($defaultName -ne "default"){
        return $defaultName
    }

    return $uniqueName + $resourceTypeSuffix
}

function Get-PublishingProfileCredentials($resourceGroupName, $webAppName){

    $resourceType = "Microsoft.Web/sites/config"

    $resourceName = "$webAppName/publishingcredentials"
    $publishingCredentials = Invoke-AzResourceAction -ResourceGroupName $resourceGroupName -ResourceType $resourceType -ResourceName $resourceName -Action list -ApiVersion "2015-08-01" -Force
    return $publishingCredentials
}

#Pulling authorization access token :
function Get-KuduApiAuthorisationHeaderValue($resourceGroupName, $webAppName){
    $publishingCredentials = Get-PublishingProfileCredentials $resourceGroupName $webAppName
    $publishingCredentials
    return ("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $publishingCredentials.Properties.PublishingUserName, $publishingCredentials.Properties.PublishingPassword))))
}

function Deploy-WebJob($resourceGroupName, $webAppName, $webJobName, $webJobZipPath){
    $accessToken = (Get-KuduApiAuthorisationHeaderValue $resourceGroupName $webAppName)[-1]

    $tempFileName = "webjob"+(Get-Date).ToString("yyyyMMdd-hhmmss")+".zip"
    $contentDisposition = "attachment; filename="+$tempFileName
    $Header = @{
        "Authorization"=$accessToken
        "Content-Disposition"=$contentDisposition
    }

    $tempFile = "$env:temp\"+$tempFileName

    Invoke-WebRequest -Uri $webJobZipPath -OutFile $tempFile

$apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/triggeredwebjobs/" + $webJobName
$result = Invoke-RestMethod -Uri $apiUrl -Headers $Header -Method put -InFile $tempFile -ContentType 'application/zip' 

}

function UpdateScriptConfigFile($resourceGroupName, $webAppName, $tempFilePath){
    $accessToken = (Get-KuduApiAuthorisationHeaderValue $resourceGroupName $webAppName)[-1]

    $Header = @{
        "Authorization"=$accessToken
        "If-Match"="*"
    }


    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/Config.js"

    Invoke-RestMethod -Uri $apiUrl `
                        -Headers $Header `
                        -Method PUT `
                        -InFile $tempFilePath `
                        -ContentType "multipart/form-data"

}

function DownloadScriptConfigFile($resourceGroupName, $webAppName, $tempFilePath){
    $accessToken = (Get-KuduApiAuthorisationHeaderValue $resourceGroupName $webAppName)[-1]

    $Header = @{
        "Authorization"=$accessToken
        "If-Match"="*"
    }

    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/Config.js"

    Invoke-RestMethod -Uri $apiUrl `
                        -Headers $Header `
                        -Method GET `
                        -OutFile $tempFilePath `
                        -ContentType "multipart/form-data"

}

function DownloadZipFile($webLocation, $tempLocalLocation){
    
    Invoke-WebRequest -Uri $webLocation -OutFile $tempLocalLocation
}

function ExecuteUpgradeSqlScript($connectionString){
##$connectionString = "Server=tcp:lunauitest-sqlserver.database.windows.net,1433;Initial Catalog=lunauitest-sqldb;Persist Security Info=False;User ID=lunauserlunauitest;Password='|d-z$"+"("+"4T/>X!CQCNjNkop^jd';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    $vars = $connectionString.Split(";")

    foreach ($var in $vars){
        $pair = $var.Split("=")
    
        if ($pair[0] -eq 'Server'){
            $serverInstance = $pair[1].Substring(4).Replace(",1433", "")
        }
        elseif ($pair[0] -eq 'Initial Catalog'){
            $database = $pair[1]
        }
        elseif ($pair[0] -eq 'User ID'){
            $userName = $pair[1]
        }
        elseif ($pair[0] -eq 'Password'){
            $password = $pair[1].Substring(1, $pair[1].Length - 2)
        }
    }
    
    $variables = "targetVersion="+$sqlVersionTable[$targetVersion]
    $variables

    Invoke-Sqlcmd -ServerInstance $serverInstance -Username $userName -Password $password -Database $database -Variable $variables -InputFile .\SqlScripts\db_upgrade.sql
}

function AddOrUpdateEntryInScriptConfigFile($tempFilePath, $entryName, $entryValue){
    $newContent = "var Configs = {";
    $isUpdate = $false
    foreach($line in Get-Content $tempFilePath) {

        $line = $line.Trim();
        if($line -eq "var Configs = {" -or $line -eq "}"){
            continue;
        }

        $name = $line.Substring(0, $line.IndexOf(":"));

        if ($name -eq $entryName){
            $newContent = $newContent + "`r`n    " + $entryName + ": """ + $entryValue + """";
            if ($line.EndsWith(",")){
                $newContent = $newContent + ",";
            }
            $isUpdate = $true
        }
        else{
            $newContent = $newContent + "`r`n    " + $line;
        }
    }

    if (-not $isUpdate){
        $newContent = $newContent + ",`r`n    " + $entryName + ": """ + $entryValue + """";
    }

    $newContent = $newContent + "`r`n}";

    Set-Content -Path $tempFilePath -Value $newContent
}

if($lunaServiceSubscriptionId -ne "default"){
    Write-Host $lunaServiceSubscriptionId
    Write-Host $tenantId
    if($tenantId -ne "default"){
        Set-AzContext -Subscription $lunaServiceSubscriptionId -Tenant $tenantId
    }
    else{
        Set-AzContext -Subscription $lunaServiceSubscriptionId
    }
}

$resourceGroupName = GetNameForAzureResources -defaultName $resourceGroupName -resourceTypeSuffix "-rg" -uniqueName $uniqueName
$keyVaultName = GetNameForAzureResources -defaultName $keyVaultName -resourceTypeSuffix "-keyvault" -uniqueName $uniqueName
$sqlServerName = GetNameForAzureResources -defaultName $sqlServerName -resourceTypeSuffix "-sqlserver" -uniqueName $uniqueName
$sqlDatabaseName = GetNameForAzureResources -defaultName $sqlDatabaseName -resourceTypeSuffix "-sqldb" -uniqueName $uniqueName
$StorageName = GetNameForAzureResources -defaultName $StorageName -resourceTypeSuffix "storage" -uniqueName $uniqueName
$appServicePlanName = GetNameForAzureResources -defaultName $appServicePlanName -resourceTypeSuffix "-appsvrplan" -uniqueName $uniqueName
$isvWebAppName = GetNameForAzureResources -defaultName $isvWebAppName -resourceTypeSuffix "-isvapp" -uniqueName $uniqueName
$enduserWebAppName = GetNameForAzureResources -defaultName $enduserWebAppName -resourceTypeSuffix "-userapp" -uniqueName $uniqueName
$apiWebAppName = GetNameForAzureResources -defaultName $apiWebAppName -resourceTypeSuffix "-apiapp" -uniqueName $uniqueName
$apiWebJobName = GetNameForAzureResources -defaultName $apiWebJobName -resourceTypeSuffix "-apiwebjob" -uniqueName $uniqueName
$apiWebAppInsightsName = GetNameForAzureResources -defaultName $apiWebAppInsightsName -resourceTypeSuffix "-apiappinsights" -uniqueName $uniqueName

$azureMarketplaceAADApplicationName = GetNameForAzureResources -defaultName $azureMarketplaceAADApplicationName -resourceTypeSuffix "-azuremarketplace-aad" -uniqueName $uniqueName
$azureResourceManagerAADApplicationName = GetNameForAzureResources -defaultName $azureResourceManagerAADApplicationName -resourceTypeSuffix "-azureresourcemanager-aad" -uniqueName $uniqueName
$webAppAADApplicationName = GetNameForAzureResources -defaultName $webAppAADApplicationName -resourceTypeSuffix "-apiapp-aad" -uniqueName $uniqueName

$currentContext = Get-AzContext
if ($accountId -eq "default"){
    $accountId = $currentContext.Account.Id
}

if ($tenantId -eq "default"){
    $tenantId = $currentContext.Tenant.Id
}

if ($userApplicationSubscriptionId -eq "default"){
    $userApplicationSubscriptionId = $currentContext.Subscription.Id
}

$currentUser = Get-AzADUser -Mail $accountId


if ($adminAccounts -eq "default"){
    $adminAccounts = $accountId
}

$objectId = $currentUser.Id

$sqlConnectionString = Get-AzKeyVaultSecret -VaultName $keyVaultName -Name 'connection-string'

$filePath = $buildLocation + $targetVersion + "/apiApp.zip"

$tempFileName = "apiApp"+(Get-Date).ToString("yyyyMMdd-hhmmss")+".zip"
$tempFilePath = "$env:temp\"+$tempFileName
DownloadZipFile $filePath $tempFilePath

Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName -ArchivePath $tempFilePath -Force

$app = Get-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName

$currentAppSettings = $app.SiteConfig.AppSettings

$newAppsettings = @{}
ForEach ($item in $currentAppSettings) {
    $newAppsettings[$item.Name] = $item.Value
}

## Add new enter in AppSettings here. 
## For backward compatibility, you should never remove an existing entry from AppSettings
Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName -AppSettings $newAppsettings


$tempConfigFileName = "Config.js"
$tempConfigFilePath = "$env:temp\"+$tempConfigFileName

DownloadScriptConfigFile $resourceGroupName $isvWebAppName $tempConfigFilePath

$filePath = $buildLocation + $targetVersion + "/isvApp.zip"
Write-Host $filePath

$tempFileName = "isvApp"+(Get-Date).ToString("yyyyMMdd-hhmmss")+".zip"

$tempFilePath = "$env:temp\"+$tempFileName
Write-Host $tempFilePath

DownloadZipFile $filePath $tempFilePath

Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $isvWebAppName -ArchivePath $tempFilePath -Force

## Update the config.js here for ISV app if needed
AddOrUpdateEntryInScriptConfigFile $tempConfigFilePath "HEADER_BACKGROUND_COLOR" $headerBackgroundColor
AddOrUpdateEntryInScriptConfigFile $tempConfigFilePath "ENABLE_V1" $enableV1
AddOrUpdateEntryInScriptConfigFile $tempConfigFilePath "ENABLE_V2" $enableV2

UpdateScriptConfigFile $resourceGroupName $isvWebAppName $tempConfigFilePath


DownloadScriptConfigFile $resourceGroupName $enduserWebAppName $tempConfigFilePath

$filePath = $buildLocation + $targetVersion + "/userApp.zip"
Write-Host $filePath

$tempFileName = "userApp"+(Get-Date).ToString("yyyyMMdd-hhmmss")+".zip"
$tempFilePath = "$env:temp\"+$tempFileName
Write-Host $tempFilePath
DownloadZipFile $filePath $tempFilePath

Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $enduserWebAppName -ArchivePath $tempFilePath -Force

## Update the config.js here for user app if needed
AddOrUpdateEntryInScriptConfigFile $tempConfigFilePath "HEADER_BACKGROUND_COLOR" $headerBackgroundColor
AddOrUpdateEntryInScriptConfigFile $tempConfigFilePath "ENABLE_V1" $enableV1
AddOrUpdateEntryInScriptConfigFile $tempConfigFilePath "ENABLE_V2" $enableV2

UpdateScriptConfigFile $resourceGroupName $enduserWebAppName $tempConfigFilePath

$filePath = $buildLocation + $targetVersion + "/webjob.zip"
Deploy-WebJob -resourceGroupName $resourceGroupName -webAppName $apiWebAppName -webJobName $apiWebJobName -webJobZipPath $filePath

$sqlConnectionString = (Get-AzKeyVaultSecret -VaultName $keyVaultName -Name 'connection-string').SecretValueText

Write-Host $sqlConnectionString


ExecuteUpgradeSqlScript $sqlConnectionString


