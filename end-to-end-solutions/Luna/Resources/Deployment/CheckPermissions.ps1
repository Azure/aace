## Copyright (c) Microsoft Corporation.
## Licensed under the MIT license.

﻿param (
    [Parameter(Mandatory=$true)]
    [string]$tenantId = "default", 

    [Parameter(Mandatory=$true)]
    [string]$subscriptionId = "default",

    [Parameter(Mandatory=$true)]
    [string]$userId = "default", 

    [Parameter(Mandatory=$true)]
    [string]$location = "default"
)

$rpList = @('Microsoft.Network','Microsoft.Compute','Microsoft.ContainerInstance','Microsoft.ContainerService','Microsoft.Insights','Microsoft.Sql','Microsoft.MachineLearningServices','Microsoft.Storage','Microsoft.ApiManagement','Microsoft.KeyVault','Microsoft.Web','Microsoft.OperationalInsights')

Connect-AzAccount -Tenant $tenantId

Set-AzContext -Subscription $subscriptionId

Write-Host "Enabling required Resource Providers and check the region availability"

$rpList | ForEach-Object -Process {
            $rp = Register-AzResourceProvider -ProviderNamespace $_;
            If (-not $rp.Locations.Contains($location)){Write-Error "Resource Provider" $_ "is not enabled in region " $location}}

Write-Host "Check if you are owner or contributor of the Azure subscription"

$scope = "/subscriptions/"+$subscriptionId
$assignment = Get-AzRoleAssignment -Scope $scope | Where-Object {$_.Scope -eq $scope -and $_.SignInName -eq $userId -and ($_.RoleDefinitionName -eq "Owner" -or $_.RoleDefinitionName -eq "Contributor")}

If ($assignment.Length -le 0){
    Write-Error "You are neither owner nor contributor of Azure subscription " $subscriptionId
}

