#Connect-AzAccount
        #$context = Get-AzSubscription -SubscriptionId 'a6c2a7cc-d67e-4a1a-b765-983f08c0423a'
        #Set-AzContext $context


function Get-PublishingProfileCredentials($resourceGroupName, $webAppName){

    $resourceType = "Microsoft.Web/sites/config"

    $resourceName = "$webAppName/publishingcredentials"
    $publishingCredentials = Invoke-AzResourceAction -ResourceGroupName $resourceGroupName -ResourceType $resourceType -ResourceName $resourceName -Action list -ApiVersion "2015-08-01" -Force
    return $publishingCredentials
}

function Get-KuduApiAuthorisationHeaderValue($resourceGroupName, $webAppName){
    $publishingCredentials = Get-PublishingProfileCredentials $resourceGroupName $webAppName
    $publishingCredentials
    return ("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $publishingCredentials.Properties.PublishingUserName, $publishingCredentials.Properties.PublishingPassword))))
}

        $webAppName = 'lunamgmtuser'

    $accessToken = (Get-KuduApiAuthorisationHeaderValue 'lunaproduction-rg' $webAppName)[-1]

    $accessToken

    $tempFileName = "config.js"
    $Header = @{
        "Authorization"=$accessToken
        "If-Match"="*"
    }
    
    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/"+$tempFileName

    $apiUrl
        Invoke-RestMethod -Uri $apiUrl `
                        -Headers $Header `
                        -Method GET  `
                        -OutFile 'c:\tmp\config.js' `
                        -ContentType "multipart/form-data"

