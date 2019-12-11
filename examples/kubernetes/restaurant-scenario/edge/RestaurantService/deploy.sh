az acr login --name clemensv
docker build --tag clemensv.azurecr.io/restaurant .
docker push clemensv.azurecr.io/restaurant
kubectl --record deployment.apps/restaurant-sidepizza set image deployment.apps/restaurant-sidepizza restaurant=clemensv.azurecr.io/restaurant:latest
kubectl --record deployment.apps/restaurant-latavola set image deployment.apps/restaurant-latavola restaurant=clemensv.azurecr.io/restaurant:latest
kubectl --record deployment.apps/restaurant-pizzaboyz set image deployment.apps/restaurant-pizzaboyz restaurant=clemensv.azurecr.io/restaurant:latest
kubectl apply -f restaurant-cs-kubedeploy.yaml