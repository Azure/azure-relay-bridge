if [ ! -z $1 ]; then ResourceGroup=$1; fi

if [ -z ${ResourceGroup+x} ]; then ResourceGroup=$AZBRIDGE_TEST_RESOURCEGROUP; fi

az group delete --name $ResourceGroup --yes