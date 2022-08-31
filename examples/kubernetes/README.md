## azbridge Kubernetes Samples

The samples in these subfolders illustrate how to use the Azure Relay Bridge in a
sidecar container for endpoint integration in Kubernetes.

* [Restaurant Scenario](./restaurant-scenario/) is a slice of an application scenario
  that bridges a cloud-based portal and ordering system at restaurants, using the
  Relay to allow for the portal to reach directly into the on-premises restaurant system
  for obtaining information about menu availability and placing orders.