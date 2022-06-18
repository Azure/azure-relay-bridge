#Requires -Version 5.1 # Shipped w/ Windows 10 1607 and Windows Server 2016
param(
    [Parameter(Mandatory=$true)][String]$DropPath,
    [Parameter(Mandatory=$true)][String]$ConnectionString,
    [Parameter(Mandatory=$true)][String]$RelayName,
    [Parameter(Mandatory=$true)][String]$BindAddress,
    [Parameter(Mandatory=$true)][Int]$Port)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$WarningPreference = 'Continue'

# Install service non-interactively.
# Start-Process -Wait prevents msiexec from returning before it finishes.
# See https://powershellexplained.com/2016-10-21-powershell-installing-msi-files/
$logPath = $DropPath + '.log'
Start-Process msiexec -ArgumentList @('/i', $DropPath, '/qn', '/log' , $logPath) -Wait

# Set config file
$configDir = Join-Path $env:ALLUSERSPROFILE 'Microsoft\Azure Relay Bridge'
$null = mkdir $configDir
$configFile = Join-Path $configDir 'azbridge_config.svc.yml'
@"
LocalForward:
- RelayName: $RelayName
  ConnectionString: $ConnectionString
  BindAddress: $BindAddress
  BindPort: $Port
"@ |
    Set-Content `
    -LiteralPath $configFile `
    -Encoding UTF8

if ($Port -eq 80) {
  # IIS and W3SVC services might block using 80 port so we have to disable them
  # Use "SilentlyContinue" because the list of services can be different on different versions of Windows Server
  @('IISAdmin', 'W3SVC') | ForEach-Object { Stop-Service -Name $_ -ErrorAction SilentlyContinue }
}

# The installer already started the service, but StartupType is Manual.
# Restart it to reload config, and fix StartupType.
# See https://github.com/Azure/azure-relay-bridge/issues/11
Stop-Service -Name azbridgesvc
Set-Service -Name azbridgesvc -StartupType Automatic -Status Running
