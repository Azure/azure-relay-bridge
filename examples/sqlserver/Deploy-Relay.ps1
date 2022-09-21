<#
.DESCRIPTION
Deploys an Azure Relay namespace in a goiven location. The script assumes that Azure Powershell is installed and the user is logged into Powershell.

.PARAMETER NamespaceName
Unqualified namespace name. Must be globally unique. 

.PARAMETER Location
Valid Azure region identifier, eg. westeurope or eastus

.EXAMPLE
.\Deploy-Relay.ps1 myuniquename westeurope
#>

param(
    [parameter(Mandatory = $true)]
    [string] $namespaceName,
    [parameter(Mandatory = $true)]
    [string] $location
)

$ = New-AzResourceGroup -Name $NamespaceName -Location $Location
New-AzResourceGroupDeployment -ResourceGroupName $NamespaceName -TemplateFile "$PSScriptRoot\relay-resource-template.json" -namespaceName $namespaceName -location $location