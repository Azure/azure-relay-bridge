if "%ACR_NAME%"=="" set ACR_NAME=azbridgetests
if "%RES_GROUP%"=="" set RES_GROUP=%ACR_NAME%

call az group create --resource-group %RES_GROUP% --location eastus
call az acr create --resource-group %RES_GROUP% --name %ACR_NAME% --sku Standard --location eastus

set ACR_NAME=
set RES_GROUP=