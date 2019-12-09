if [ ! -z $1 ]; then Region=$1; fi
if [ ! -z $2 ]; then ResourceGroup=$2; fi
if [ ! -z $3 ]; then Namespace=$3; fi

d=$(date +%Y%m%d%H%M%S)

if [ -z ${Region+x} ]; then Region='westeurope'; fi
if [ -z ${ResourceGroup+x} ]; then ResourceGroup='azbridge'$d; fi
if [ -z ${Namespace+x} ]; then Namespace=$ResourceGroup; fi

echo $ResourceGroup

az group create --name $ResourceGroup --location $Region
az relay namespace create --resource-group $ResourceGroup --name $Namespace
az relay namespace authorization-rule create --resource-group $ResourceGroup --namespace-name $Namespace --name SendListen --rights Send Listen
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a1
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a2
export AZBRIDGE_TEST_CXNSTRING=$(az relay namespace authorization-rule keys list --resource-group $ResourceGroup --namespace-name $Namespace --name SendListen --query 'primaryConnectionString' -o json)
export AZBRIDGE_TEST_RESOURCEGROUP=$ResourceGroup
export AZBRIDGE_TEST_NAMESPACE=$Namespace