# setup script

if [ -z ${AZBRIDGE_DEMO_CXNSTRING} ]; then
    source ./azure/deploy.sh
fi
if [ -z ${AZBRIDGE_DEMO_CXNSTRING} ]; then
   echo No connection string found
fi
kubectl delete secret "relaycxn"
kubectl create secret generic "relaycxn" --from-literal=cxn=$AZBRIDGE_DEMO_CXNSTRING

kubectl apply -f ./cloud/FoodPortal/foodportal-cs-kubedeploy.yaml
kubectl apply -f ./cloud/FoodPortal/foodportal-cs-kubeservice.yaml
kubectl apply -f ./edge/RestaurantService/restaurant-cs-kubedeploy.yaml
