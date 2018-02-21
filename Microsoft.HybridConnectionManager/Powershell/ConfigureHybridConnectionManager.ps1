import-module $env:ProgramW6432'\Microsoft\HybridConnectionManager\HybridConnectionManager' 

Set-HybridConnectionManagerConfiguration -ManagementPort 9352

echo "Set-HybridConnectionManagerConfiguration returned $? $LASTEXITCODE"

& $env:ProgramW6432'\Microsoft\HybridConnectionManager\HCMConfigWizard.exe'

echo "HCMConfigWizard.exe returned $? $LASTEXITCODE"
