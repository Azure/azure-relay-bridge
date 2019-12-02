
RESOURCE_GROUP_NAME=$1
RESOURCE_GROUP_LOCATION=$2
RELAY_NAMESPACE=$3

if [ ! -v RESOURCE_GROUP_NAME ]; then
  read -p 'Azure Resource Group Name: ' RESOURCE_GROUP_NAME
fi
if [ ! -v RESOURCE_GROUP_LOCATION ]; then
  read -p 'Azure Resource Group Location: ' RESOURCE_GROUP_LOCATION
fi
if [ ! -v RELAY_NAMESPACE ]; then
  read -p 'Azure Relay Namespace: ' RELAY_NAMESPACE
fi

az group create --name $RESOURCE_GROUP_NAME --location $RESOURCE_GROUP_LOCATION
if [ $? -eq 0 ]; then
    az relay namespace create --resource-group $RESOURCE_GROUP_NAME --location $RESOURCE_GROUP_LOCATION --name $RELAY_NAMESPACE
    az relay namespace authorization-rule create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name Listen --rights Listen
    az relay namespace authorization-rule create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name Send --rights Send
    az relay namespace authorization-rule create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name SendListen --rights Send Listen
    az relay hyco create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name latavola
    az relay hyco create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name pizzaboyz
    az relay hyco create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name sidepizza
fi