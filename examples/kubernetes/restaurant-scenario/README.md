## azbridge Kubernetes restaurant scenario sample

This is a slice of an application scenario that bridges a cloud-based portal and
ordering systems at restaurants, using the Relay to allow for the portal to
reach directly into the on-premises restaurant systems for obtaining information
about current menu availability and for placing orders.

The scenario isn't trying to be fancy or complete, but show what's required for
the integration path.

### Scenario elements

The scenario simulates a federation of four distinct Kubernetes deployments: One
is the host for the cloud service hosting the portal and backend ordering system
and three are small clusters (likely even just a single machine) that each run
at a restaurant and host the local menu service and, assumed, the local order
management system. 

For simplicity, we use a single Kubernetes cluster in these demo scripts,
however. The sample works with the Kubernetes service embedded with Docker
Desktop.

The scenario does require an Azure Relay namespace and an Azure container
registry instance for distributing the images to the conceptual sites. These
resources can be created and configured (and can be cleaned up again) using the
supplied scripts as explained below. All scripts assume use of the bash shell on
either Debian or Ubuntu, on Windows via WSL.

#### Cloud application

The [cloud](./cloud/) application is a simple ASP.NET 6.0 single page app that
uses Angular on the client. It displays a menu page where the [backing
code](./cloud/FoodPortal/ClientApp/src/app/order-menu/order-menu.component.ts)
first fetches a list of restaurants and then a list of menu items once a
restaurant is selected.

The server-side backend provides the restaurant list via the
[RestaurantController](./cloud/FoodPortal/Controllers/RestaurantController.cs) and
the menu items via the [MenuController](./cloud/FoodPortal/Controllers/MenuController.cs).

The MenuController is the element to pay special attention to. 

``` CSharp
[HttpGet]
public async Task<IEnumerable<MenuItem>> Get([FromQuery] string rid)
{
    string url;

    switch (rid)
    {
        case "latavola" :
        case "pizzaboyz" :
        case "sidepizza" : 
            url = $"http://{rid}:8000/menu";
            break;
        default:
          _logger.LogError($"Invalid Restaurant {rid}");
          return null;
    }

    try
    {
        using (var httpClient = new HttpClient())
        {
            var rsp = await httpClient.GetAsync(url);
            var bytes = await rsp.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<MenuItem[]>(bytes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
    catch(Exception e)
    {
        _logger.LogError(e, "Failed request");
        throw;
    }
}
```

Looking at the "Get" method, you will find that we select a different endpoint
based on the selected `rid` (restaurant identifier), and then call that endpoint
to retrieve the desired data. That call is realized via Azure Relay and directly
into the HTTP endpoint of the respective restaurant, and across any firewalls
that may exist there.

To make that work, we need to realize a communication tunnel through the Azure
Relay with `azbridge` running at either end, accepting and forwarding traffic to
the desired endpoints.

In
[foodportal-cs-kubedeploy.yaml](cloud/FoodPortal/foodportal-cs-kubedeploy.yaml),
we configure the required elements on the cloud side:

```yml
...
   spec:
       hostname: foodportal
       hostAliases:
       - ip: "127.0.9.1"
         hostnames:
          - "latavola"
       - ip: "127.0.9.2"
         hostnames:
          - "pizzaboyz"
       - ip: "127.0.9.3"
         hostnames:
          - "sidepizza"
...
```

For each restaurant site, we create a DNS alias with a fixed IPv4 address from
the 127.x.x.x range on the local host. That is the address that the
`MenuController` above will resolve the name to as it initiates the call. This
is how we intercept calls into the Relay. Mind that you have a full class A
network in 127.x.x.x and can therefore assign, theoretically, a few million
targets, each with distinct ports.


```yml
       containers:
        - image: clemensv.azurecr.io/foodportal
          name: foodportal
          ports:
            - containerPort: 80
        - image: clemensv.azurecr.io/azbridge
          name: azbridge
          env:
          - name: RELAYCXN
            valueFrom:
              secretKeyRef:
                name: relaycxn
                key: cxn
          args: ["-L","latavola:8000/http:latavola", "-L","pizzaboyz:8000/http:pizzaboyz", "-L","sidepizza:8000/http:sidepizza", "-x","'$(RELAYCXN)'"] 
```

We then configure the `foodportal` app container and the `azbridge` container
side-by-side into the pod so that they share the pod's local host network.

The `azbridge` container is run configuring three local listeners bound to the
previously configured local endpoints, for example `-L
latavola:8000/http:latavola`. The name and port number `latavola:8000` designate
the local endpoint and local TCP port on which the bridge will listen. The
section `http:latavola` describes the logical port name `http` that is used on
the Azure Relay `latavola`. Logical port names are a feature of `azbridge` that
allow for multiplexing different streams through the same Relay. 

We'll see the matching side of the bridge in the next section.

#### Edge application

The "edge" application that simulates a portion of the in-restaurant solution is
realized as a simple ASP.NET API application. The
[MenuController](edge/RestaurantService/Controllers/MenuController.cs)
implementation reads the available menu items for the restaurant and returns
them. That is the API called by the cloud application as shown above.

The key part for us to look at is the container section of the deployment
configuration in
[restaurant-cs-kubedeploy.yaml](edge/RestaurantService/restaurant-cs-kubedeploy.yaml):

```yml
spec:
  containers:
    - image: clemensv.azurecr.io/restaurant
      name: restaurant
      env:
      - name: RID
        value: "latavola"
    - image: clemensv.azurecr.io/azbridge
      name: azbridge
      env:
      - name: RID
        value: "latavola"
      - name: RELAYCXN
        valueFrom:
          secretKeyRef:
            name: relaycxn
            key: cxn
      args: ["-x","'$(RELAYCXN)'", "-R", "$(RID):localhost:http/80"] 
```

This is the deployment for one of the restaurants. Again, the app container and
the `azbridge` container sit side-by-side in the pod and share the same local
network. The bridge is bound to a remote listener, in effect with teh expression
`-R latavola:localhost:http/80`. `latavola` identifies the Relay name,
`localhost` the target address for forwarding, `http` the local port that is
being mapped and `80` the port at the target address. 

(Since `localhost` is the default target address, the expression could also be
shortened to `-R latavola:http/80`, and if we weren't using logical port names
further to `-R latavola:80`.)
