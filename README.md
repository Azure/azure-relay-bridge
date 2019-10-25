# Azure Relay Bridge

![img](https://ci.appveyor.com/api/projects/status/github/clemensv/azure-relay-bridge)

The Azure Relay Bridge is a tool that allows creating TCP tunnels between any
pair of hosts, as long as those hosts each have outbound Internet connectivity on
port 443 (HTTPS) to the Azure Relay service.

The Relay Bridge is designed for reaching networked assets in any environment
where it is impractical or impossible for those assets to be directly reachable
through a public IP address.

For instance, if you need to reach an on-premises database or application from
a cloud-based solution, the on-premises assets are typically not accessible
from the public network. The Relay Bridge can help establishing a TCP tunnel
socket exclusive to a particular endpoint in such an on-premises environment,
and without the complexity of a VPN solution.

Another example are on-premises applications that run behind network gateways
with dynamically assigned IP addresses, like in most residential and small-business
environments. An Azure Relay endpoint provides a stable network destination for
such application endpoints, without VPN, and without the complexity of a dynamic
DNS registration.

Inside cloud and data-center environments, reaching into and bridging between
containerized workloads can also be tricky. The Relay Bridge can provide
every service inside a container instance with a stable and externally reachable
network address, and with the Relay's built-in load balancing support, you can
even bind multiple services inside separate container instances to the same name.
And you can do all that without configuring any kind of inbound network access 
to the containers.

Practically all TCP-based services, including HTTP(S), are compatible with
the Azure Relay Bridge. For services that require connections to be made from
both parties, the bridge can concurrently act as local and remote bridge.

All Azure Relay endpoints are secure, requiring TLS 1.2+ (aka SSL) WebSocket
connections for all connections through the Relay, and both communicating
parties must provide an authorization token to establish a connection via the
Relay.

The Azure Relay Bridge builds on this foundation and creates the illusion of a
local connection to the target service by ways of a *local forwarder* that listens
on a configured IP address and port, and that then forwards all incoming TCP
connections to a *remote forwarder* via the Azure Relay. The *remote forwarder*
connects each incoming connection to the target service.


## azbridge

The Relay Bridge is a command line utility ("azbridge") with binary distributions
for Windows, macOS, and several Linux distributions. It can optionally also be
configured and run as a background service on Windows and Linux.

Since the tool helps with scenarios not dissimilar to SSH tunnels (but without
requiring peer-to-peer connectivity) the command line syntax of *azbridge* uses 
elements that resemble SSH's equivalent tunnel functionality, especially the -L 
and -R arguments. The key difference to SSH is that *azbridge* always binds sockets
to an Azure Relay name, and that Azure Relay acts as the identifier for the
tunnel and as network rendezvous point.

The bridge can either be used directly on the machines where a client or a server
resides, or it can be used as a gateway solution. When used as *local forwarder 
gateway* (*-g* *-L*) and with externally resolvable listener addresses, the bridge
resides on a host in the network and allows connections from clients across the
network. The *remote forwarder* (*-R*) can always reach out to off-machine targets
within its network scope.

When the bridge is used locally, the client can configure DNS names of the target
services in the local *hosts* file, picking a unique IP address out of the 127.x.x.x
range for each service, and then configuring a local forwarder for the respective
target address. Those addresses can only be reached on that local machine, shielding
the client from exposing TCP bridges to others. For instance, for reaching the remote
SQL Server "sql.corp.example.com", you would add an IP address like `127.1.2.3` to
the "hosts" file as `127.1.2.3 sql.corp.example.com`, and then use a local forwarder
configuration that refers to the `127.1.2.3` address, for example
`azbridge -L 127.1.2.3:1433:relay`.

When used as a *local forwarder gateway*, you will need to use addresses that can be
reached by the clients in your network, and ideally have a multi-homed setup where
the gateway node has a network address, e.g. from the `10.x.x.x` range, per remote 
target service host. For naming support, those network addresses should be registered
in a DNS service reachable and used by the clients in your network. A DNS service is
also required for resolving wildcard addresses, even for the local scenario.

With a local configuration, when using *azbridge* to reach a Microsoft SQL Server
instance endpoint (port 1433) on a different network, you would use the following
constellation:

* SQL Client connects to sql.corp.example.com:1433, whereby the local "hosts" file
  re-maps the server name to a local address with the entry
  `127.0.5.1 sql.corp.example.com`
* Local bridge on the same machine the client runs as
  `azclient -L 127.0.5.1:1433:sql-corp-example-com -x {cxnstring}`
* Azure Relay has a configured endpoint
  `wss://mynamespace.servicebus.windows.net/$hc/sql-corp-example-com`
* Remote Bridge on or near the server runs as
  `azclient -R sql-corp-example-com:sql.corp.example.com:1433 -x {cxnstring}`
* SQL Server runs as `sql.corp.example.com:1433`

The `{cxnstring}` represents the connection string for the configured
Azure Relay endpoint with appropriate send and/or listen permissions.

The connection string can be obtained from the portal.

Further details about how to use the tool and how to configure it can be found in
the [Configuration and Command Line Options](CONFIG.md) document.

## Downloads

This is an early preview. Unsigned (!) binaries are available for direct download
from the [Github Releases](../../releases) page for evaluation. Signed binaries will eventually
be available for download with common package managers.

## Installation 

The tool has installation packages for a variety of platforms. All packages are
self-contained distributions, meaning they do not rely on a centrally installed 
runtime. However, depending on the package type and platform, the installation
of some prerequisites may be required.

### Windows 

The easiest way to install the bridge on Windows is by using the appropriate
*.msi package. The installer adds the tool to the PATH and also registers
the "azbridge" Windows service. The service is configured for on-demand
(manual) start at installation time.

> **KNOWN ISSUE:** These early builds are not signed. Download the MSI file,
unblock it, and then install. Otherwise the application may not work as
expected.

### Linux

### Debian, Ubuntu, Linuxmint

For Debian 8+ and all Debian-based distributions, like Ubuntu 16.04+ and Linuxmint 16+,
you can install the tool from the respective *.deb package with

`sudo apt-get install ./{package-name}.deb`

Using `apt-get` will automatically install the distribution prerequisites. The
.NET Core platform required by the tool is private and not installed machine-wide.

The package install will put the tool into `/usr/share/azbridge`, place a machine-wide
configuration file into `/etc/azbridge`, add the tool to the PATH, and register two
BASH extensions for adding and removing entries from the `/etc/hosts` file:

* `addhost {ipaddress} {name}` - adds an IP address with the given hostname to "hosts"
* `removehost {name}` - removes the entry for the given hostname

### Fedora, CentOS, Red Hat Enterprise Linux

For Fedora, CentOS, and Red Hat Enterprise Linux, you can install the tool from the
respective *.rpm package with

`sudo yum install {package-name}.rpm`

Using `yum` will automatically install the distribution prerequisites. The
.NET Core platform required by the tool is private and not installed machine-wide.

The package install will put the tool into `/usr/share/azbridge`.

> **KNOWN ISSUE 1:** The package will presently not install correctly if the install of
> documentation files is suppressed for `yum` and/or `dnf`. The is the case for many
> container base images. On CentOS, you should drop the respective configuration with
> `sed -i '/tsflags=nodocs/d' /etc/yum.conf` (RUN in a Dockerfile before installing
> the rpm) and on Fedora use `sed -i '/tsflags=nodocs/d' /etc/dnf/dnf.conf`.
> **KNOWN ISSUE 2:** The package does not yet perform any of the post-install tasks that
> the Debian package performs, meaning the tool is not added to the PATH.

### Other distributions and platforms

You can also install the tool from respective platform *.tar.gz archive. For Linux,
you need to [explicitly install Linux prerequisites for .NET Core](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x)
for your respective distribution. For macOS, you need to [install prerequisites from
this list](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x).

## Building the code

The repo contains a complete build and verification structure for all platforms.

The Windows version MUST be built on Windows because the service integration requires
the full .NET Framework and the installer can only be built on Windows. You will at least
need the "Build Tools for Visual Studio 2017", and ideally a local install of
Visual Studio 2017 with desktop C# support.

All other versions are built with the .NET Core 2.0 or .NET Core 2.1 SDK. The DEB and
RPM packages are only created when building on a Unix (i.e. Linux or macOS) host.

The ideal build environment is a Windows 10/Windows Server 2016 host with Docker for
Windows installed. The `package-all.cmd` script will first build and package all Windows
targets, and then launch a docker-based build with the official Microsoft .NET Core 2.1
SDK image for the remaining targets. The `package.sh` script will only build and package
the Unix targets, the `package.cmd` script only Windows targets.

The latter two scripts are used with the AppVeyor build as well.

All build output is placed into `./artifacts/build`

## Tests

Running the Unit tests and the Integration tests both require an Azure Relay namespace
to be available for use and configured. Before running any of the test scenarios, the
environment variable `AZBRIDGE_TEST_CXNSTRING` must be set to the Relay namespace
connection string (enclosed in quotes) on the build platform.

An [Azure Resource Manager template](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-template-deploy-cli) 
to deploy a namespace with the definitions required for testing resides in
`./src/tools/azure/test-resource-template.json`. The template expects the name of a new namespace
and a region location as inputs.

Once the template has been deployed using either Powershell or the Azure CLI, you can find
the "sendListenConnectionString" value (starts with "Endpoint...") in the returned output. 
Copy and save that value for use in the `AZBRIDGE_TEST_CXNSTRING` environment variable.

The Unit tests can be run from the command line with `dotnet test` with an .NET Core build.

For integration testing that installs and executes the emitted packages, run `verify-build.cmd`/`verify-build.sh`. Expect for Windows, the integration tests depend on a
local Docker installation: The script cleans any existing images, builds CentOS, Debian, Fedora, and Ubuntu images with the newly built binaries, and then executes a series of tests on each image.

## Contributions

We're gladly accepting contributions. Please review the [contribution rules](CONTRIBUTING.md).
