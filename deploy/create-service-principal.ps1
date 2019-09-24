<#
 .SYNOPSIS
    Configure a service account for VM and ACI management within a subscription

 .DESCRIPTION
    Configure a service account for VM and ACI management within a subscription

 .EXAMPLE
    ./create-service-principal.ps1 -subscription "Azure Subscription"

 .PARAMETER Subscription
    The subscription name or id.

#>

param(
 [Parameter(Mandatory=$True)]
 [string]
 $Subscription,

 [Parameter(Mandatory=$True)]
 [string]
 $ClientSecret
)

$ErrorActionPreference="Stop"
$AzModuleVersion = "2.0.0"

# Verify that the Az module is installed 
if (!(Get-InstalledModule -Name Az -MinimumVersion $AzModuleVersion -ErrorAction SilentlyContinue)) {
    Write-Host "This script requires to have Az Module version $AzModuleVersion installed..
It was not found, please install from: https://docs.microsoft.com/en-us/powershell/azure/install-az-ps"
    exit
} 

if ([string]::IsNullOrEmpty($(Get-AzContext).Account)) {
    # sign in
    Write-Host "Logging in...";
    Connect-AzAccount; 
}

Write-Host "Selecting subscription '$Subscription'";
$sub = Select-AzSubscription -Subscription $Subscription;

#Then add an App to the Azure AD.
Write-Host "Creating AD App"

$SecureStringPassword = ConvertTo-SecureString -String "$ClientSecret" -AsPlainText -Force
$app = New-AzADApplication -DisplayName vm-aci-scheduler -IdentifierUris vm-aci-scheduler -Password $SecureStringPassword

# Note the _applicationId_ in the output. It's a GUID and you'll need it in the next step and to configure the Azure function.
# Next, create a service principal in the AAD.

Write-Host "Creating Service Principal"

$sp = New-AzADServicePrincipal -DisplayName vm-aci-scheduler -ApplicationId $app.ApplicationId -Scope /subscriptions/$Subscription -Role Reader

Write-Host "Assign VM mamangement role"

New-AzRoleAssignment -ObjectId $sp.Id -RoleDefinitionName "Virtual Machine Contributor" -Scope /subscriptions/$Subscription

# Create a custom role for ACI management. Make sure you set the correct subscription id in that file

$role = Get-AzRoleDefinition -Name "Container Instance Manager"

if( !($role) ){
    ((Get-Content -path "ACI-manager-role.json" -Raw) -replace '<subscription-id>',$Subscription) | Set-Content -Path role-temp.json

    Write-Host "Create ACI mamangement role"
    New-AzRoleDefinition -InputFile role-temp.json 

    Remove-Item ".\role-temp.json"
}

# Assign the custom role: 
Write-Host "Assign ACI mamangement role"
New-AzRoleAssignment -ObjectId $sp.Id -RoleDefinitionName "Container Instance Manager" -Scope /subscriptions/$Subscription

# Output the details needed for app configuration
Write-Host " --- App config settigns --- "

Write-Host "AAD Tenant ID : $($sub.Tenant.Id)"
Write-Host "Client ID: $($app.ApplicationId)"
Write-Host "Client secret: $ClientSecret"

