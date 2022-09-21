## Microsoft SQL Server example

This directory contains a set of sample files illustrating how to bridge to a
SQL Server instance.


## Relay Setup

First, create an Azure Relay namespace with a Hybrid Connection named "sql". 

The included [Azure
Powershell](https://learn.microsoft.com/en-us/powershell/azure/) script
`Deploy-Relay.ps1`can be called with the name of the namespace and the Azure
region, for instance, and deploys the included resource template:

```Powershell
$result=./Deploy-Relay.ps1 mynamespacename westeurope
echo $result.Outputs.sendListenConnectionString.Value
```

An equivalent, explicit script using [Azure
CLI](https://learn.microsoft.com/en-us/cli/azure/) looks like this:

```azurecli
export _NS=mynamespacename
az group create --name $_NS --location westeurope
az relay namespace create -g $_NS --name $_NS
az relay hyco create -g $_NS --namespace-name $_NS --name sql
az relay namespace authorization-rule create -g $_NS --namespace-name $_NS -n sendlisten --rights Send Listen
az relay namespace authorization-rule keys list -g $_NS --namespace-name $_NS -n sendlisten --out tsv --query "primaryConnectionString"
```

## Customizing the config files

The template deployment returns a connection string from the
`sendListenConnectionString` value. The last line of the CLI script yields a
connection string as well.

These connection strings are associated with a namespace-wide [shared access
signature
rule](https://learn.microsoft.com/en-us/azure/azure-relay/relay-authentication-and-authorization#shared-access-signature)
called "sendlisten" that confers both the "Listen" and "Send" permission at
once.

The `client_config.yml` and `server_config.yml` files each have a line as
follows. Replace the placeholder with the connection string in those files.

```yml
AzureRelayConnectionString : <<insert connection string>>
```

The remaining content of `client_config.yml` sets up a local forwarder bound to
address 127.0.0.2 with TCP port 1433 mapped to hybrid connection "sql". The
logical port name is set to "tds", which allows for the TCP port number here to
differ from that on the server. 

```yml
LocalForward :
   - BindAddress: 127.0.0.2
     BindPort: 1433
     PortName: tds
     RelayName: sql

LogLevel: INFO
```

Using the `add-hostname` PowerShell command (Windows) or the `addhost` bash
function (Linux) that are installed with `azbridge`, you can easily map that
address to a local host name alias. You must run that command as administrator.

Bash:

```bash
addhost 127.0.0.2 localsql
```

Powershell:

```powershell
add-hostname 127.0.0.2 localsql
```

The remaining `server_config.yml` file sets up a remote forwarder that binds the hybrid connection "sql" with logical port "tds" to the SQL server endpoint on "localhost", port 1433.

```yml
RemoteForward :
   - RelayName: sql
     Host: localhost
     PortName: tds
     HostPort: 1433

LogLevel: INFO
```

## Running the bridge

To run the bridge, you can now run 

```azurecli
azbridge -f ./client_config.yml
```

on the client side where the SQL client will run.

You run 

```azurecli
azbridge -f ./server_config.yml
```

on the server side where SQL server runs.

To verify the bridge, you can now connect through it from the client side, for
instance with `sqlcmd`:

```azurecli
sqlcmd -S tcp:127.0.0.2,1433 -P <<password>> -U <<username>>
```

With the host name alias use

```azurecli
sqlcmd -S tcp:localsql,1433 -P <<password>> -U <<username>>
```

Mind that if you enable TLS (Encryption) for SQL Server (as you should), the
host name alias you configure must match the remote SQL server's host name in
order for the certificate validation on the client to function correctly. Concretely, if the SQL Server's host name on its local network is "sql.corp.example.com", that exact name must be used for the host name alias.


The Azure Relay tunnel is *always* TLS protected, independent of the SQL server
configuration.
