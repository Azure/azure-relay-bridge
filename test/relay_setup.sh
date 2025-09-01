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
# create hybrid connections for all platforms
# Windows platform
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a1.win
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a2.win
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a3.win --requires-client-authorization false
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name http.win

# Linux platform
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a1.linux
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a2.linux
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a3.linux --requires-client-authorization false
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name http.linux

# macOS platform
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a1.osx
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a2.osx
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a3.osx --requires-client-authorization false
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name http.osx

# Legacy connections (for backward compatibility)
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a1
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a2
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name a3 --requires-client-authorization false
az relay hyco create --resource-group $ResourceGroup --namespace-name $Namespace --name http
# get connection string and set environment variables
connectionString=$(az relay namespace authorization-rule keys list --resource-group $ResourceGroup --namespace-name $Namespace --name SendListen --query 'primaryConnectionString' -o tsv)
export AZBRIDGE_TEST_CXNSTRING=$connectionString
export AZBRIDGE_TEST_RESOURCEGROUP=$ResourceGroup
export AZBRIDGE_TEST_NAMESPACE=$Namespace

echo ""
echo "=========================================="
echo "Azure Relay Environment Setup Complete"
echo "=========================================="
echo "Connection string: $connectionString"
echo "Resource Group: $ResourceGroup"
echo "Namespace: $Namespace"
echo ""
echo "Environment variables set:"
echo "  AZBRIDGE_TEST_CXNSTRING=$AZBRIDGE_TEST_CXNSTRING"
echo "  AZBRIDGE_TEST_RESOURCEGROUP=$AZBRIDGE_TEST_RESOURCEGROUP"
echo "  AZBRIDGE_TEST_NAMESPACE=$AZBRIDGE_TEST_NAMESPACE"
echo ""
echo "Created hybrid connections:"
echo "  Windows: a1.win, a2.win, a3.win (no auth), http.win"
echo "  Linux: a1.linux, a2.linux, a3.linux (no auth), http.linux"
echo "  macOS: a1.osx, a2.osx, a3.osx (no auth), http.osx"
echo "  Legacy: a1, a2, a3 (no auth), http"
echo ""
echo "To use these in your current shell, run:"
echo "  source test/relay_setup.sh"
echo ""
