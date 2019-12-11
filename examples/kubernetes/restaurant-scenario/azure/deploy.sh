
RESOURCE_GROUP_NAME=$1
RESOURCE_GROUP_LOCATION=$2
RELAY_NAMESPACE=$3

if [ -z ${RESOURCE_GROUP_LOCATION} ]; then RESOURCE_GROUP_LOCATION='westeurope'; fi
if [ -z ${RESOURCE_GROUP_NAME} ]; then RESOURCE_GROUP_NAME='azbdemo1'$(date +%Y%m%d%H%M%S); fi
if [ -z ${RELAY_NAMESPACE} ]; then RELAY_NAMESPACE=$RESOURCE_GROUP_NAME; fi

echo $RESOURCE_GROUP_LOCATION
echo $RESOURCE_GROUP_NAME
echo $RELAY_NAMESPACE

az group create --name $RESOURCE_GROUP_NAME --location $RESOURCE_GROUP_LOCATION
if [ $? -eq 0 ]; then
    az relay namespace create --resource-group $RESOURCE_GROUP_NAME --location $RESOURCE_GROUP_LOCATION --name $RELAY_NAMESPACE
    az relay namespace authorization-rule create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name Listen --rights Listen
    az relay namespace authorization-rule create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name Send --rights Send
    az relay namespace authorization-rule create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name SendListen --rights Send Listen
    az relay hyco create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name latavola
    az relay hyco create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name pizzaboyz
    az relay hyco create --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name sidepizza
    export AZBRIDGE_DEMO_CXNSTRING=$(az relay namespace authorization-rule keys list --resource-group $RESOURCE_GROUP_NAME --namespace-name $RELAY_NAMESPACE --name SendListen --query 'primaryConnectionString' -o json)
    export AZBRIDGE_DEMO_RESOURCEGROUP=$RESOURCE_GROUP_NAME
    export AZBRIDGE_DEMO_NAMESPACE=$RELAY_NAMESPACE
fi