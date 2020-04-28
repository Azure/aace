param (
    [Parameter(Mandatory=$true)]
    [string]$uniqueName = "default", 

    [Parameter(Mandatory=$true)]
    [string]$location = "centralus",

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

    [string]$buildLocation = "https://github.com/Azure/AIPlatform/raw/master/end-to-end-solutions/Luna/Resources/Builds/latest",

    [string]$companyName = "Microsoft",

    [string]$headerBackgroundColor = "#004578",

    [string]$enableV1 = "true",

    [string]$enableV2 = "false",

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


function GetNameForAzureResources{
    param($uniqueName, $defaultName, $resourceTypeSuffix)
    if ($defaultName -ne "default"){
        return $defaultName
    }

    return $uniqueName + $resourceTypeSuffix
}

function Create-AzureADApplication{
    param($appName, $appId, $keyVaultName, $replyURLs, $secretName="none", $multiTenant=$False)
    
    $filter = "AppId eq '"+$appId+"'"
    $azureadapp = Get-AzureADApplication -Filter $filter

    ## Create new AAD application if the appId is not specified or application doesn't exist
    if ($azureadapp.Count -eq 0){
        $azureadapp = New-AzureADApplication -DisplayName $appName -AvailableToOtherTenants $multiTenant -ReplyUrls $replyURLs


        $requiredResourceAccess = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]@{
        ResourceAppId="00000003-0000-0000-c000-000000000000";
        ResourceAccess=[Microsoft.Open.AzureAD.Model.ResourceAccess]@{
            Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" ;
            Type = "Scope"
            }
        }
        Set-AzureADApplication -ObjectId $azureadapp.ObjectId -RequiredResourceAccess $requiredResourceAccess
    }
    
    $startDate = Get-Date

    if($secretName -ne "none"){
        $secret = New-AzureADApplicationPasswordCredential -ObjectId $azureadapp.ObjectId -StartDate $startDate -EndDate $startDate.AddMonths($keyExpirationByMonth) -CustomKeyIdentifier "keyfortoken"
        $secretvalue = ConvertTo-SecureString $secret.Value -AsPlainText -Force
        $secretObj = Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $secretName -SecretValue $secretvalue
    }

    return $azureadapp.AppId
}

function GrantKeyVaultAccessToWebApp{
    param($resourceGroupName, $keyVaultName, $webAppName)
    $webapp = Get-AzWebApp -ResourceGroupName $resourceGroupname -Name $webAppName
    Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $webapp.Identity.PrincipalId -PermissionsToSecrets list,get
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

function UpdateScriptConfigFile($resourceGroupName, $webAppName, $configuration){
    $accessToken = (Get-KuduApiAuthorisationHeaderValue $resourceGroupName $webAppName)[-1]

    $tempFileName = "config.js"
    $Header = @{
        "Authorization"=$accessToken
        "If-Match"="*"
    }

    $tempFilePath = "$env:temp\"+$tempFileName

    $configuration | Out-File $tempFilePath

    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/"+$tempFileName

    Invoke-RestMethod -Uri $apiUrl `
                        -Headers $Header `
                        -Method PUT `
                        -InFile $tempFilePath `
                        -ContentType "multipart/form-data"

}

Function NewAzureRoleAssignment($scope, $objectId, $retryCount) {
    $totalRetries = $retryCount
    $retryCount
    While ($True) {
        try {
        
            $assignments = Get-AzRoleAssignment -ObjectId $objectId
            if ($assignments.Count -eq 0){
                New-AzRoleAssignment -ObjectId $objectId -RoleDefinitionName 'Contributor' -Scope $scope -ErrorAction Stop
            }
            return
        }
        catch {
            Write-Host "catch catch" $retryCount
            # The principal could not be found. Maybe it was just created.
            If ($retryCount -eq 0) {
                Write-Error "An error occurred: $($_.Exception)`n$($_.ScriptStackTrace)"
                throw "The principal '$objectId' cannot be granted Contributor role on the subscription. Please make sure the principal exists and try again later."
            }
            $retryCount--
            Write-Warning "  The principal '$objectId' cannot be granted Contributor role on the subscription. Trying again (attempt $($totalRetries - $retryCount)/$totalRetries) after 30 seconds."
            Start-Sleep 30
        }
    }
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

$sqlServerAdminPasswordRaw = [System.Web.Security.Membership]::GeneratePassword(24,5)
$sqlServerAdminPassword = ConvertTo-SecureString $sqlServerAdminPasswordRaw.ToString() -AsPlainText -Force

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

Write-Host "Create resource group" $resourceGroupName
New-AzResourceGroup -Name $resourceGroupName -Location $location

Write-Host "Deploy ARM template in resource group" $resourceGroupName
New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName `
                              -TemplateFile .\main.json `
                              -keyVaultName $keyVaultName `
                              -sqlServerName $sqlServerName `
                              -sqlDatabaseName $sqlDatabaseName `
                              -storageAccountName $StorageName `
                              -appServicePlanName $appServicePlanName `
                              -isvWebAppName $isvWebAppName `
                              -enduserWebAppName $enduserWebAppName `
                              -apiWebAppName $apiWebAppName `
                              -apiWebAppInsightsName $apiWebAppInsightsName `
                              -location $location `
                              -sqlAdministratorLoginPassword $sqlServerAdminPassword `
                              -sqlAdministratorUsername $sqlServerAdminUsername `
                              -tenantId $tenantId `
                              -objectId $objectId `
                              -buildLocation $buildLocation


$filter = "AppId eq '"+$webAppAADApplicationId+"'"
$azureadapp = Get-AzureADApplication -Filter $filter
$isNewApp = $azureadapp.Count -eq 0

Write-Host "Create AAD application for webapp authentication."
$replyUrls = New-Object System.Collections.Generic.List[System.String]
#create AAD application for ISV App
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net/Offers";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net/Subscriptions";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $enduserWebAppName + ".azurewebsites.net/LandingPage";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $enduserWebAppName + ".azurewebsites.net/Subscriptions";
$replyUrls.Add($replyUrl)

#create AAD application for the API app
$appId = Create-AzureADApplication -appName $webAppAADApplicationName -appId $webAppAADApplicationId -keyVaultName $keyVaultName -secretName "api-app-key" -multiTenant $True -replyURLs $replyUrls
$webAppAADApplicationId = $appId
Write-Host "AAD application created with id" $webAppAADApplicationId

if ($isNewApp){

    Start-Sleep 15

    Write-Host "Creating Service Principal for webapp AAD application."
    New-AzureADServicePrincipal -AppId $webAppAADApplicationId

    $principalId = (Get-AzADServicePrincipal -ApplicationId $webAppAADApplicationId).id
    Write-Host "Service Principal created with id " $principalId
    
    $filter = "AppId eq '"+$webAppAADApplicationId+"'"
    $azureadapp = Get-AzureADApplication -Filter $filter
    
    $identifierUri = 'api://'+$webAppAADApplicationId
    
    Set-AzureADApplication -ObjectId $azureadapp.ObjectId `
    -Oauth2AllowImplicitFlow $True `
    -IdentifierUris $identifierUri
}

#create AAD application for Azure Marketplace auth
Write-Host "Create AAD Application for Azure Marketplace authentication."
$appId = Create-AzureADApplication -appName $azureMarketplaceAADApplicationName -appId $azureMarketplaceAADApplicationId -keyVaultName $keyVaultName -secretName "amp-app-key" -multiTenant $False
$azureMarketplaceAADApplicationId = $appId
Write-Host "AAD application created with id " $azureMarketplaceAADApplicationId

#create AAD application for ARM auth

$filter = "AppId eq '"+$azureResourceManagerAADApplicationId+"'"
$azureadapp = Get-AzureADApplication -Filter $filter
$isNewApp = $azureadapp.Count -eq 0

Write-Host "Creating AAD Application for Azure Resource Manager authentication."
$appId = Create-AzureADApplication -appName $azureResourceManagerAADApplicationName -appId $azureResourceManagerAADApplicationId -keyVaultName $keyVaultName -secretName "arm-app-key" -multiTenant $False
$azureResourceManagerAADApplicationId = $appId
Write-Host "AAD application created with id" $azureResourceManagerAADApplicationId

if ($isNewApp){
    Start-Sleep 15
    
    Write-Host "Creating Service Principal for Azure Resource Manager AAD application."
    New-AzureADServicePrincipal -AppId $azureResourceManagerAADApplicationId
    
    $principalId = (Get-AzADServicePrincipal -ApplicationId $azureResourceManagerAADApplicationId).id
    Write-Host "Service principal is created with id " $principalId
    
    Write-Host "Assign subscription contribution role to the service principal."
    $scope = '/subscriptions/'+$userApplicationSubscriptionId
    NewAzureRoleAssignment -objectId $principalId -scope $scope -retryCount 10
}

#grant key vault access to API app
Write-Host "Grant key vault access to API app"
GrantKeyVaultAccessToWebApp -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -webAppName $apiWebAppName

Write-Host "Adding client ip to the SQL Server firewall rule"
if ($firewallStartIpAddress -ne "clientIp" -or $firewallEndIpAddress -ne "clientIp"){

    $firewallRuleName = "deploymentClientVPN"+(Get-Date).ToString("yyyyMMdd-hhmmss")
    New-AzSqlServerFirewallRule -ResourceGroupName $resourceGroupName -ServerName $sqlServerName -FirewallRuleName $firewallRuleName -StartIpAddress $firewallStartIpAddress -EndIpAddress $firewallEndIpAddress

}

$clientIp = (Invoke-WebRequest -uri "http://ifconfig.me/ip").Content
$firewallStartIpAddress = $clientIp
$firewallEndIpAddress = $clientIp

$firewallRuleName = "deploymentClient"+(Get-Date).ToString("yyyyMMdd-hhmmss")

New-AzSqlServerFirewallRule -ResourceGroupName $resourceGroupName -ServerName $sqlServerName -FirewallRuleName $firewallRuleName -StartIpAddress $firewallStartIpAddress -EndIpAddress $firewallEndIpAddress

Write-Host "Execute SQL script to create database user and objects."
$sqlDatabaseUserName = "lunauser" + $uniqueName
$sqlDatabaseUsernameVar = "username='" + $sqlDatabaseUserName + "'"
$sqlDatabasePassword = ([System.Web.Security.Membership]::GeneratePassword(24,5)).Replace("=", "!")
$sqlDatabasePasswordVar = "password='" + $sqlDatabasePassword + "'"

$variables = $sqlDatabaseUsernameVar, $sqlDatabasePasswordVar

$sqlServerInstanceName = $sqlServerName + ".database.windows.net"
$sqlServerInstanceName
Invoke-Sqlcmd -ServerInstance $sqlServerInstanceName -Username $sqlServerAdminUsername -Password $sqlServerAdminPasswordRaw -Database $sqlDatabaseName -Variable $variables -InputFile .\SqlScripts\db_provisioning.sql

Write-Host "Store storage account key to Azure Key Vault."
$key = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $StorageName)| Where-Object {$_.KeyName -eq "key1"}

$secretvalue = ConvertTo-SecureString $key.Value -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $keyVaultName -Name 'storage-key' -SecretValue $secretvalue

Write-Host "Store SQL connection string to Azure key vault"
$connectionString = "Server=tcp:" + $sqlServerInstanceName + ",1433;Initial Catalog=" + $sqlDatabaseName + ";Persist Security Info=False;User ID=" + $sqlDatabaseUserName + ";Password='" + $sqlDatabasePassword + "';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$secretvalue = ConvertTo-SecureString $connectionString -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $keyVaultName -Name 'connection-string' -SecretValue $secretvalue

Write-Host "Update app settings"
$appsettings = @{}
$appsettings["SecuredCredentials:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:StorageAccount:Config:AccountName"] = $StorageName;
$appsettings["SecuredCredentials:StorageAccount:Config:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:Database:DatabaseName"] = $sqlDatabaseName;
$appsettings["SecuredCredentials:ResourceManager:AzureActiveDirectory:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:ResourceManager:AzureActiveDirectory:ClientId"] = $azureResourceManagerAADApplicationId;
$appsettings["SecuredCredentials:ResourceManager:AzureActiveDirectory:TenantId"] = $tenantId;
$appsettings["SecuredCredentials:Marketplace:AzureActiveDirectory:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:Marketplace:AzureActiveDirectory:ClientId"] = $azureMarketplaceAADApplicationId;
$appsettings["SecuredCredentials:Marketplace:AzureActiveDirectory:TenantId"] = $tenantId;
$appsettings["AzureAD:ClientId"] = $webAppAADApplicationId;
$appsettings["AzureAD:TenantId"] = $tenantId;
$appsettings["ISVPortal:AdminAccounts"] = $adminAccounts;
$appsettings["ISVPortal:AdminTenant"] = $adminTenantId;

$appInsightsApp = Get-AzApplicationInsights -ResourceGroupName $resourceGroupName -name $apiWebAppInsightsName
$appsettings["ApplicationInsights:InstrumentationKey"] = $appInsightsApp.InstrumentationKey;
$appsettings["WebJob:APIServiceUrl"] = "https://" + $apiWebAppName + ".azurewebsites.net/api";

Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName -AppSettings $appsettings

$config = 'var Configs = {
    API_ENDPOINT: "https://'+ $apiWebAppName +'.azurewebsites.net/api/",
    ISV_NAME: "'+$companyName+'",
    AAD_APPID: "'+$webAppAADApplicationId+'",
    AAD_ENDPOINT: "https://'+$isvWebAppName+'.azurewebsites.net",
    HEADER_BACKGROUND_COLOR: "'+$headerBackgroundColor+'",
    ENABLE_V1: "'+$enableV1+'",
    ENABLE_V2: "'+$enableV2+'"
}'

UpdateScriptConfigFile -resourceGroupName $resourceGroupName -webAppName $isvWebAppName -configuration $config

$config = 'var Configs = {
    API_ENDPOINT: "https://'+ $apiWebAppName +'.azurewebsites.net/api/",
    ISV_NAME: "'+$companyName+'",
    AAD_APPID: "'+$webAppAADApplicationId+'",
    AAD_ENDPOINT: "https://'+$enduserWebAppName+'.azurewebsites.net",
    HEADER_BACKGROUND_COLOR: "'+$headerBackgroundColor+'",
    ENABLE_V1: "'+$enableV1+'",
    ENABLE_V2: "'+$enableV2+'"
}'

UpdateScriptConfigFile -resourceGroupName $resourceGroupName -webAppName $enduserWebAppName -configuration $config

Write-Host "Deploy webjob."
$webjobZipPath = $buildLocation + "/webjob.zip"
Deploy-WebJob -resourceGroupName $resourceGroupName -webAppName $apiWebAppName -webJobName $apiWebJobName -webJobZipPath $webjobZipPath

Write-Host "Deployment finished successfully."

Write-Host "You will need the following information when creating a SaaS offer in Azure Partner Center:"
$landingPageUrl = "https://" + $enduserWebAppName + ".azurewebsites.net/LandingPage";
Write-Host "Landing page URL: " $landingPageUrl
$connectionWebhook = "https://"+ $apiWebAppName +".azurewebsites.net/Webhook"
Write-Host "Connection Webhook: " $connectionWebhook
Write-Host "Azure Active Directory tenant ID: " $tenantId
Write-Host "Azure Active Directory application ID: " $azureMarketplaceAADApplicationId
