param (
    [Parameter(Mandatory=$true)]
    [string]$uniqueName = "default", 

    [Parameter(Mandatory=$true)]
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

    [string]$buildLocation = "https://github.com/Azure/AIPlatform/raw/master/end-to-end-solutions/Luna/Resources/Builds/",

    [string]$companyName = "Microsoft",

    [string]$adminTenantId = "common",

    [string]$adminAccounts = "default"

)

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


    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/"+$tempFileName

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

    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/"+$tempFileName

    Invoke-RestMethod -Uri $apiUrl `
                        -Headers $Header `
                        -Method GET `
                        -OutFile $tempFilePath `
                        -ContentType "multipart/form-data"

}


if($lunaServiceSubscriptionId -ne "default"){
    if($tenantId -ne "default"){
        
        $context = Get-AzSubscription -SubscriptionId $lunaServiceSubscriptionId -TenantId $tenantId
        Set-AzContext $context
    }
    else{
    
        $context = Get-AzSubscription -SubscriptionId $lunaServiceSubscriptionId
        Set-AzContext $context
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

$filePath = $buildLocation + $targetVersion + "\apiApp.zip"
Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName -ArchivePath $filePath

$app = Get-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName

$currentAppSettings = $app.SiteConfig.AppSettings

$newAppsettings = @{}
ForEach ($item in $currentAppSettings) {
    $newAppsettings[$item.Name] = $item.Value
}

## Add new enter in AppSettings here. 
## For backward compatibility, you should never remove an existing entry from AppSettings
Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName -AppSettings $appsettings


$tempFilePath = "$env:temp\"+$tempFileName
DownloadScriptConfigFile($resourceGroupName, $isvWebAppName, $tempFilePath)

$filePath = $buildLocation + $targetVersion + "\isvApp.zip"
Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $isvWebAppName -ArchivePath $filePath

## Update the config.js for ISV app if needed

UpdateScriptConfigFile($resourceGroupName, $isvWebAppName, $tempFilePath)


DownloadScriptConfigFile($resourceGroupName, $enduserWebAppName, $tempFilePath)

$filePath = $buildLocation + $targetVersion + "\userApp.zip"
Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name $enduserWebAppName -ArchivePath $filePath

## Update the config.js for user app if needed

UpdateScriptConfigFile($resourceGroupName, $enduserWebAppName, $tempFilePath)

$filePath = $buildLocation + $targetVersion + "\webjob.zip"
Deploy-WebJob -resourceGroupName $resourceGroupName -webAppName $apiWebAppName -webJobName $apiWebJobName -webJobZipPath $filePath

$variables = $sqlVersionTable[$targetVersion]
Invoke-Sqlcmd -ConnectionString $sqlConnectionString -Variable $variables -InputFile .\SqlScripts\db_upgrade.sql


