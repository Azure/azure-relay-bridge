# cleanup script
kubectl delete secret "relaycxn"
kubectl delete deployment foodportal
kubectl delete deployment restaurant-latavola
kubectl delete deployment restaurant-pizzaboyz
kubectl delete deployment restaurant-sidepizza
source azure/cleanup.sh