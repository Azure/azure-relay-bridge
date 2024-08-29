# Azure Relay Bridge

![Build Status](https://github.com/Azure/azure-relay-bridge/actions/workflows/main-ci-build.yml/badge.svg)

The Azure Relay Bridge (`azbridge`) is a simple command line tool that allows
creating TCP, UDP, HTTP, and Unix Socket tunnels between any pair of hosts,
allowing to traverse NATs and Firewalls without requiring VPNs, only using
outbound HTTPS (443) Internet connectivity from either host. Neither of those
needs to be running in Azure; the Azure Relay helps facilitating the
connection.

Why would you want to use this? Here are a few examples:
- You have a database server or a web service somewhere in your datacenter that
  you need to reach from a cloud application, but it's too difficult or too
  expensive to set up a VPN bridge between the two.
- You have a service running in a Kubernetes cluster that you need to reach from
  a client application, but you don't want to expose it via an Ingress
  controller.
- You are developing a cloud-hosted application that needs to reach a service
  running on your local machine for testing purposes and you don't control your
  local machine's network. (You can literally bridge back into your machine
  sitting at a coffee shop table or on airplane WiFi.)
- Users install your software/devices into networks that you don't have any
  control over or that don't have stable IP addresses or are hidden behind an
  ISP's NAT, but you still need ad-hoc access (think vending machines at fuel
  stations).

Using this tool requires a [Microsoft Azure subscription](https://azure.com) and
an [Azure Relay](https://docs.microsoft.com/en-us/azure/azure-relay/) namespace.
Active Azure Relay ("Hybrid Connections") listeners are charged hourly.
Namespaces and relays are not charged while not in use; please review the
[pricing FAQ](https://azure.microsoft.com/en-us/pricing/details/service-bus/#faq).

> NOTE: Azure Relay is a fully supported cloud service that has been around for
> well over a decade, but this `azbridge` is not covered by Azure product
> support. The underlying client SDK is fully covered by product support.
> Therefore you can ask for Relay service and SDK product support as if this was
> your own code, but you can't demand fixes in this code under the support SLA.
> Issues that arise from the code in this repo must be filed here and there is
> no guaranteed reaction time for addressing any such issues. That said, the
> tool is used by several teams within Microsoft and maintained by the Azure
> Relay team.

This README document illustrates some simple usage scenarios. For further
details, including installation instructions, please read the
[Installation](INSTALLATION.md) document. The various configuration options are
detailed even further in the [Config](CONFIG.md) document.

## Installation

See the [Installation](INSTALLATION.md) document for installation instructions for
Windows, Linux, MacOS and Docker.

## Command Line Arguments Quick Reference

The `azbridge` command line is structured as follows:

``` bash
azbridge [options] [forwarding rules]
```

When no shared access policy is specified either in the connection string or via
the `-K` and `-k` options, the tool will use the "default Azure credentials"
method just like the `az` CLI tool, meaning that it will automatically pick up
the current user's Azure CLI login context. That is the preferred way to use the
tool, as it allows for seamless integration with Entra ID and is the most secure
way to authenticate with Azure Relay.The examples in this document show that
usage, only referring to the Relay service with the `-e` option.

| Option                                                | Description                                                 |
| ----------------------------------------------------- | ----------------------------------------------------------- |
| -f, --config-file <CONFIG_FILE>                       | Configuration file to use. See [Configuration File](CONFIG.md#configuration-file) |
| -e, --endpoint-uri <ENDPOINT_URI>                     | Relay endpoint base URI, formed as `sb://{namespace-name}.servicebus.windows.net/`. |
| -x, --connection-string <CONNECTION_STRING>           | Azure Relay connection string (overridden with -s -K -k -e) |
| -K, --shared-access-key-name <SHARED_ACCESS_KEY_NAME> | Shared access policy name (optional) |
| -k, --shared-access-key <SHARED_ACCESS_KEY>           | Shared access policy key (optional) |
| -s, --signature <SIGNATURE>                           | Shared access signature token, alternate to -K and -k that does not use the key directly, see [SAS Authentication](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-sas#generate-a-shared-access-signature-token)  |
| -l, --log-file <LOG_FILE>                             | Log file (default is output to the console) |
| -q, --quiet                                           | No log output to stdout/stderr |
| -v, --verbose                                         | Verbose log output with more details. |
| -o, --option <OPTION>                                 | Configuration file option override key:value, see [Configuration File](CONFIG.md#configuration-file) |
| -g, --gateway-ports                                   | By default, the bridge restricts connections to local listeners (`-L`) to the local machine. This option allows remote hosts to connect to these listeners. |
| -b, --bind-address <BIND_ADDRESS>                     | Source address of outbound, forwarding connections (`-T`, `-H`). |
| -I, --svcinstall                                      | Windows only: Install as Windows Service. Must run as admin. |
| -U, --svcuninstall                                    | Windows only: Uninstall Windows Service. Must run as admin. |
| -svc, --svc                                           | Reserved for background service invocation |
| -?, -h, --help                                        | Show help information. |

Forwarding rules are specified as shown in the following table. Hereby, a "local
forwarder" is a listener that is bound to a local port and forwards to a relay,
and a "remote forwarder" is a listener that is bound to a relay and forwards to
a local port. A "remote HTTP forwarder" is a listener that is bound to a relay
and forwards to a local HTTP(S) endpoint, with the relay providing the HTTPS
endpoint; the bridge thus acts as a reverse proxy.

| Forwarding Rule                                       | Description                                                  |
| ----------------------------------------------------- | ------------------------------------------------------------ |
| -L, --local-forward <LOCAL_FORWARD>                   | Expression: [`[hostname:]port[/port_name]:relay_name`](CONFIG.md#-l) |
| -T, --remote-forward <REMOTE_FORWARD>                 | Expression: [`relay_name:[port_name/][hostname:]port`](CONFIG.md#-t)     |
| -H, --remote-http-forward <REMOTE_HTTP_FORWARD>       | Expression: [`relay_name:{http or https}/[hostname:]port`](CONFIG.md#-h) |

Whereby:
- `hostname` is the DNS name or IP address of the local binding address (`-L`) or the remote target host (`-T`, `-H`)
- `port` is the TCP port number
- `port_name` is an optional logical port name that allows multiplexing multiple connections over a single relay. This *must* be `http` or `https` for the `-H` option.
- `relay_name` is the name of the *Hybrid Connection* relay in the Azure Relay namespace

The [Config](CONFIG.md) explains the options in more detail.

You can specify multiple forwarding rules on the command line, and you can also
use a configuration file to specify them. The configuration file can also contain
additional options that are not specified on the command line.

## Basic Scenario

```
            Private Network A│          │Private Network B
                             │          │
                             │          │
                             │          │
                   TCP:16161 │          │                    TCP:16161
┌──────────┐      ┌────────┐ │          │   ┌────────┐    ┌────────────┐
│          │      │        │ │          │   │        │    │            │
│  Client  ├──────►azbridge├─┼──────┐ ┌─┼───►azbridge├────► Database   │
│          │      │        │ │      │ │ │   │        │    │            │
└──────────┘      └────────┘        │ │     └────────┘    └────────────┘
                                 ┌──▼─┴───┐
                                 │   db   │
                                 │        │
                                 │        │
                                 └────────┘
                                 Azure Relay
                                  Namespace

azbridge \                                 azbridge \
  -L 16161:db \                               -T db:16161 \
  -e sb://mydemo.servicebus.windows.net       -e sb://mydemo.servicebus.windows.net

```

If you run a database server somewhere in your own datacenter that you need to
reach from a cloud application, `azbridge` can make that database server
securely reachable from the cloud application without you having to make any
changes on your on-premises network, as long as `azbridge` running on or near
the database server machine can establish an outbound HTTPS/WebSocket connection
to the Azure Relay namespace.

In the example above, there's a bridge running on the same machine as the
client, listening on local TCP port 16161 and binding that port to the "db"
*Hybrid Connection* relay on the "mydemo" namespace with the `-L`option.

On the database machine, the bridge is bound in the reverse, mapping "db" to the
local TCP port 16161 as a client connecting to the database with the `-T`
option. All traffic through the bridge is forwarded end-to-end.

## Hybrid Connection Setup

The required Azure Relay resource (the *Hybrid Connection*) can be set up with a
few lines of script (showing BASH syntax):

``` bash
export _NS=mydemo
# create resource group
az group create --name $_NS --location westeurope
# create relay namespace
az relay namespace create -g $_NS --name $_NS
# create the hybrid connection endpoint 'db'
az relay hyco create -g $_NS --namespace-name $_NS --name db
# grant the current user "owner" permission
az role assignment create \
    --assignee $(az ad signed-in-user show --query "id" --output tsv) \
    --role "Azure Relay Owner" \
    --scope $(az relay namespace show -g $_NS --name $_NS --query "id" --output tsv)
```

In a real deployment, you can authenticate with Entra ID (formerly Azure Active
Directory) using the `az login` command and then use the that security context
with the ´azbridge` tool. You can also use an [environment
credential](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet)
or managed identities.

If using Entra ID is not an option, you can use the native
authentication/authorization scheme of the Azure Relay. You can create specific
access rights with an associated token credential for each relay or you can do
so globally at the namespace level. The following two script lines create a
"listen" and a "send" rule that can be used to either run a listener or a sender
on the "db" *Hybrid Connection*. You could also combine those rights into one
rule by specfying `--rights Listen Send`

``` bash
az relay hyco authorization-rule create -g mydemo --hybrid-connection-name db \
                                --namespace-name mydemo -n send --rights Send
az relay hyco authorization-rule create -g mydemo --hybrid-connection-name db \
                               --namespace-name mydemo -n listen --rights Listen
```

The created rule's keys can then be obtained by running

``` bash
az relay hyco authorization-rule keys list --hybrid-connection-name db \
                               --namespace-name mydemo -g mydemo -n send
```

The command will return the primary and secondary signing keys and connection
strings:

``` json
{
  "keyName": "send",
  "primaryConnectionString": "Endpoint=sb://mydemo.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey={base64 key};EntityPath=db",
  "primaryKey": "{base64 key}",
  "secondaryConnectionString": "Endpoint=sb://mydemo.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey={base64 key};EntityPath=db",
  "secondaryKey": "{base64 key}"
}
```

You can either use a connection string with the `azbridge -x "{connection
string}"` option and omit the `-e` endpoint option, or you can specify the name
of the rule with `-K` and the value of the key with `-k`. Specifying an
authorization rule skips the Entra ID authentication.

### Hostnames and Addresses

For most scenarios involving TLS, it is important for the hostnames to match the
subject name of the presented certificates during the TLS handshake.

When the bridge is used on the client side, you can configure DNS names of the
target services in the local *hosts* file, picking a unique IPv4 address out of
the 127.x.x.x range for each service, and then configuring a local forwarder for
the respective target address.

It's not widely understood (oddly) that the 127.x.x.x network is fully available
for local endpoints, meaning you can bind a TCP port 443 listener simultaneously
to 127.0.0.2 and 127.0.0.3 without conflicts. On MacOS only, you must explicitly
enable those addresses using, for instance,
`sudo ifconfig lo0 alias 127.0.0.2 up`.

Addresses in the 127.x.x.x range can only be reached on that local machine,
shielding the client from exposing TCP bridges to others. For instance, for
reaching the remote SQL Server `sql.corp.example.com`, you would add an IP
address like `127.1.2.3` to the "hosts" file as `127.1.2.3
sql.corp.example.com`, and then use a local forwarder configuration that refers
to the `127.1.2.3` address, for example `azbridge -L 127.1.2.3:1433:relay`.

Any client connections from that machine to `sql.corp.example.com` will then be
redirected to the `azbridge` exposed endpoint. The machine connected via the
remote `azbridge` which has this actual DNS name on its own network and
therefore presents a matching TLS certificate through the tunnel.

To make that easy, the package install in Linux will register two BASH
extensions for adding and removing entries from the `/etc/hosts` file:

* `addhost {ipaddress} {name}` - adds an IP address with the given hostname to
  "hosts"
* `removehost {name}` - removes the entry for the given hostname

On Windows, the installation path includes three scripts:
* `Add-Hostnames.ps1 {ipaddress} {name}` - adds an IP address with the given
  hostname to "hosts"
* `Remove-Hostnames.ps1 {name}` - removes the entry for the given hostname
* `Get-Hostnames.ps` - lists all hostnames

All these scripts must be run with admin privileges (sudo on Linux) because the
`hosts` file is protected.

You can obviously also bind to a private network address of the host on which
`azbridge` runs, like `azbridge -L 192.168.0.6:1433:relay`, and enter that
address into the private network's DNS. By default, the bridge will not allow
connections from remote hosts to the local listeners, but you can enable that
with the `-g` option.

The remote forwarder end of the `azbridge` tunnel in the above example is
configured as `-T db:16161`, which forwards to TCP port 16161 on the local
machine.


```
            Private Network A│          │Private Network B
                             │          │
                             │          │
                   127.0.0.8 │          │                 sql.corp.example.com
                   TCP:16161 │          │               │     TCP:16161
┌──────────┐      ┌────────┐ │          │   ┌────────┐  │  ┌────────────┐
│          │      │        │ │          │   │        │  │  │            │
│  Client  ├──────►azbridge├─┼──────┐ ┌─┼───►azbridge├──│──► Database   │
│          │      │        │ │      │ │ │   │        │  │  │            │
└──────────┘      └────────┘        │ │     └────────┘  │  └────────────┘
                                 ┌──▼─┴───┐             │
                                 │   db   │
                                 │        │
                                 │        │
                                 └────────┘

azbridge \                                  azbridge \
  -L sql.corp.example.com:16161:db            -T db:sql.corp.example.com:16161

(hosts file: sql.corp.example.com 127.0.0.8)
```

The bridge's remote forwarders can also be configured to point to external hosts
within the bridge's network scope, specifiying their hostname in the binding
expression, like `-T db:somewhere.corp.example.com:16161`.

### Multiplexing

Occasionally, you will need to connect to multiple targets behind the same
network boundary. `azbridge` can realize this using a single listener (which
saves you money). The established connections remain completely independent.

```
            Private Network A│          │Private Network B
                             │          │                    sql1.corp.example.com
                             │          │                        TCP:16161
                             │          │                      ┌────────────┐
                             │          │               │      │            │
┌──────────┐      ┌────────┐ │          │   ┌────────┐  │  ┌───► Database   │
│          │      │        │ │          │   │        │  │  │   │            │
│  Client  ├──────►azbridge├─┼──────┐ ┌─┼───►azbridge├──│──┤   └────────────┘
│          │      │        │ │      │ │ │   │        │  │  │   ┌────────────┐
└──────────┘      └────────┘        │ │     └────────┘  │  │   │            │
                                 ┌──▼─┴───┐             │  └───► Database   │
                                 │   db   │                    │            │
                                 │        │                    └────────────┘
                                 │        │                      TCP:16161
                                 └────────┘                  sql2.corp.example.com

hosts file:
   sql1.corp.example.com 127.0.0.8
   sql2.corp.example.com 127.0.0.9

```

To multiplex connections to multiple targets, you can use "logical ports" with
`azbridge`. Logical ports are used to distinguish connections through the Relay
tunnel and they are specified on the command line.

The command `azbridge -L "127.0.0.8:16161/sql1;127.0.0.9:16161/sql2:db" -e
[...]` binds a listener on 127.0.0.8:16161 to logical port "sql1" and a listener
on 127.0.0.9:16161 to logical port "sql2"; both listeners are bound to the
*Hybrid Connection* `db` specified after the last `:`.

On the remote side, and given the scenario above, the targets are bound with the
command `azbridge -T db:sql1/sql1.corp.example.com:16161;sql2/sql2.corp.example.com:16161`.

Logical ports are construct of `azbridge` and there's no limit on how many you
can have.

## HTTP Scenario


```
   Cloud App Network │      │Restaurant Network
                     │      │
                     │      │
                     │      │
                     │      │                    HTTPS:443
┌─────────┐          │      │   ┌────────┐    ┌────────────┐
│         │ HTTPS:443│      │   │        │    │ "La Tavola"│
│ Cloud   ├──────────┼──┐ ┌─┼───►azbridge├────► Restaurant │
│  App    │          │  │ │ │   │        │    │ Order Sys  │
└─────────┘             │ │     └────────┘    └────────────┘
                     ┌──▼─┴───┐
                     │latavola│
                     │        │
                     │        │
                     └────────┘
                     Azure Relay
                      Namespace

                                 azbridge \
                                   -H latavola:https/443 \
                                   -e sb://mydemo.servicebus.windows.net
```

If you run an order management app inside of a restaurant using a common DSL or
Cable connection that does not have a stable IP address and is firewalled, but
you need to be able to reach that app from a web-based portal or even from a
client device attached to a public wireless network to submit orders, `azbridge`
can help, either in the same way as shown above or as an HTTP proxy that only
needs to run on one side.

The diagram shows `azbridge` functioning as a reverse proxy for HTTPS, making an
HTTP or HTTPS endpoint behind a firewall or even inside a container reachable
via the Azure Relay namespace's network and DNS integration and secured with an
optional (on-by-default) access control rule protecting the endpoint.

The azbridge only needs to be run on the receiver side since the Azure Relay
provides the HTTPS endpoint. Similar to the prior scenario, we need to set up
and configure *Hybrid Connection*, which is named "latavola" (name of the
assumed restaurant). The `azbridge -H` command then binds to that relay with an
HTTP reverse proxy, which forwards incoming requests to the HTTPS listener on
the local target port 443. All HTTP headers, including the "Authorization"
header, are forwarded end-to-end.

The client in this example will connect to
`https://mydemo.servicebus.windows.net/latavola`. By default, the endpoint
requires an extra layer of authorization at the relay, similar to HTTP proxy
authentication. The required parameters/headers are [described in the protocol
guide](https://docs.microsoft.com/en-us/azure/azure-relay/relay-hybrid-connections-protocol#http-request-protocol),
and the token model is described in the [authorization documentation](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-sas#generate-a-shared-access-signature-token).

You can turn off this protection at the relay endpoint and only rely on
end-to-end authorization, even though it's not recommended to expose services
behind firewalls that are not designed to be used as public endpoints in this way.

``` bash
az relay hyco update -g mydemo --namespace-name mydemo --name latavola \
              --requires-client-authorization false
```

The command `azbridge -H latavola:https/443 -e sb://mydemo.servicebus.windows.net`
will bind the external address to
`https://mydemo.servicebus.windows.net/latavola` and will forward arriving
requests to `https://localhost:443`

## Kubernetes scenario

```

                        │       │  Kubernetes Pod
┌─────────┐             │       │
│  Admin  │  HTTPS:443  │       │
│  Tool A ├─────────────┼───┐   │
│         │             │   │   │
└─────────┘             │   │   │   ┌─────────┬───────────┐
             TCP:17171  │   │   │   │         │           │
┌─────────┐ ┌────────┐  │   │   │   │         │ ┌───────┐ │
│ Admin   │ │        │  │   │   │   │    ┌────┼─►Svc A  │ │
│ Tool B  ├─►azbridge├──┼───►   │   │    │    │ └───────┘ │
│         │ │        │  │   │ ┌─┼───►azbridge │           │
└─────────┘ └────────┘  │   │ │ │   │    │    │ ┌───────┐ │
                            │ │     │    └────┼─►Svc B  │ │
                         ┌──▼─┴───┐ │         │ └───────┘ │
                         │ myctr  │ │sidecar  │           │
                         │        │ └─────────┴───────────┘
                         │        │
                         └────────┘
                         Azure Relay
                          Namespace

```

Using `azbridge` in a Kubernetes sidecar container, you can reach APIs and
services running inside of containers in a cluster without having them exposed
via an Ingress controller.

See the [Installation](INSTALLATION.md) document for further details.

