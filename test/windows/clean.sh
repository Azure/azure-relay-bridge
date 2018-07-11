if [ -z $ACR_NAME ]; then ACR_NAME=azbridge_test; fi
if [ -z $RES_GROU ]; then RES_GROUP=$ACR_NAME; fi

az acr delete --resource-group $RES_GROUP --name $ACR_NAME 
az group delete --resource-group $RES_GROUP --location eastus
