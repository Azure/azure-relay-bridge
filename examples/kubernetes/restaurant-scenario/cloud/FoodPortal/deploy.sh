az acr login --name clemensv
docker build --tag clemensv.azurecr.io/foodportal .
docker push clemensv.azurecr.io/foodportal
kubectl --record deployment.apps/foodportal set image deployment.apps/foodportal foodportal=clemensv.azurecr.io/foodportal:latest
kubectl apply -f foodportal-cs-kubedeploy.yaml
kubectl apply -f foodportal-cs-kubeservice.yaml