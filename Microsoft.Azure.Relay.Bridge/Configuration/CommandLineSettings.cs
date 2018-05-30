// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class CommandLineSettings
    {
        /*
         * **--addhosts** / **--cleanhosts**

        When `azbridge` is run with the `--addhosts` option, which
        requires running with administrative privileges, all hostname values from the 
        configuration files' `LocalForward` section are added to the local machine's "hosts"
        file. The `--cleanhosts` option removes all entries added by `--addhosts`. 

        These options cannot be combined with use of the -L or -R options below.

        **-b bind_address**

        Use bind_address on the local machine as the source address of
        forwarding connections.  Only useful on systems with more than one
        address.

        **-C**    

        Requests compression for the tunnel. The compression algorithm is the
        same used by gzip(1), and the "level" can be controlled by the 
        CompressionLevel configuration option. Compression is desirable on slow
        connections, but may cause significant throughput and other performance 
        issues on fast networks. The default value can be set on a host-by-host
        basis in the configuration files; see the Compression configuration option.

        **-D**   

        Reserved. Not presently supported

        **-E endpoint_uri**

        Azure Relay endpoint URI (see -x).

        **-F configfile**

        Specifies an alternative per-user configuration file.  If a configuration 
        file is given on the command line, the system-wide configuration file 
        (Linux: /etc/azbridge/azbridge_config, 
        Windows: %ALLUSERSPROFILE%\Microsoft\AzureBridge\azbridge_config) will be 
        ignored. 

        The default for the per-user configuration file is ~/.azurebridge/config
        on Linux and %USERPROFILE%\.azurebridge\config on Windows.

        **-g**    

        Allows remote hosts to connect to local forwarded ports.

        **-K policy_name**

        Azure Relay shared access policy name to use (see -x).

        **-k policy_key**

        Azure Relay shared access policy key to use (see -x).

        **-L [bind_address:]port:relay_name**<br/>
        **-L local_socket:relay_name**<br/>

        Specifies that connections to the given TCP port or Unix socket
        on the local (client) host are to be bound (forwarded) to the given 
        Azure Relay name. This works by allocating a socket to listen 
        to either a TCP port on the local side, optionally bound to the 
        specified bind_address, or to a Unix socket*. Whenever a connection
        is made to the local port or socket, the connection is forwarded
        to a connected remote bridge via the chosen Relay entity.

        Port forwardings can also be specified in the configuration file.
        Only the superuser can forward privileged ports.  IPv6 addresses
        can be specified by enclosing the address in square brackets.

        By default, the local port is bound in accordance with the
        *GatewayPorts* configuration setting.  However, an explicit bind_address
        may be used to bind the connection to a specific address.  The
        bind_address of ``localhost'' indicates that the listening port
        be bound for local use only, while an empty address or '*' indi-
        cates that the port should be available from all interfaces.

        The -L option can be used multiple times on a single command line.

        **-o option**

        Can be used to give options in the format used in the configura-
        tion file.  This is useful for specifying options for which there
        is no separate command-line flag.

        **-q**    

        Quiet mode.  Causes most warning and diagnostic messages to be
        suppressed.

        **-R relay_name:port**<br/>
        **-R relay_name:host:hostport**<br/>
        **-R relay_name:local_socket**

        Specifies that connections to the given Azure Relay name
        are to be forwarded to the given host and port, or Unix socket*, 
        on the local side. Whenever a connection is made to the Relay, 
        the connection is forwarded to this listener (or a concurrently 
        connected listener in a random load distribution fashion), and a 
        then a forwarding connection is made to either port, host:hostport,
        or local_socket, from the local machine.

        Port forwardings can also be specified in the configuration file.
        Privileged ports can be forwarded only when logging in as root.  
        IPv6 addresses can be specified by enclosing the address in square
        brackets.

        The -R option can be used multiple times on a single command line.

        **-S signature**

        Azure Relay shared access signature (previously issued access token)
        to use (see -x)

        **-V**

        Display the version number and exit.

        **-v** 

        Verbose mode.  Causes ssh to print debugging messages about its
        progress. This is helpful in debugging connection, authentica-
        tion, and configuration problems. Multiple -v options increase
        the verbosity.  The maximum is 3.

        **-x connection_string**

        Connection String. Azure Relay connection string for the namespace
        or for a specific Azure Relay. The Connection String properties
        can be overriden by the -E (Endpoint), -K (SharedAccessKeyName),
        -k (SharedAccessKey), -S (SharedAccessSignature) arguments.

        If an EntityPath is specified in the connection string, that name 
        is the only valid option for the relay_name expressions in the
        -L and -R options or for expressions in the effective configuration
        file.

        The connection string can be set via the AZURE_BRIDGE_CONNECTIONSTRING
        environment variable. 

         * 
         */

        [Option(CommandOptionType.NoValue, LongName = "addhosts")]
        public bool? AddHosts { get; set; }
        [Option(CommandOptionType.NoValue, LongName = "cleanhosts")]
        public bool? CleanHosts { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "b")]
        public string BindAddress { get; set; }
        [Option(CommandOptionType.NoValue, ShortName = "C")]
        public bool? Compression { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "E")]
        public Uri EndpointUri { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "F")]
        public string ConfigFile { get; set; }
        [Option(CommandOptionType.NoValue, ShortName = "g")]
        public bool? GatewayPorts { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "K")]
        public string SharedAccessKeyName { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "k")]
        public string SharedAccessKey { get; set; }
        [Option(CommandOptionType.MultipleValue, ShortName = "L")]
        public IEnumerable<string> LocalForward { get; set; }
        [Option(CommandOptionType.MultipleValue, ShortName = "o")]
        public IEnumerable<string> Option { get; set; }
        [Option(CommandOptionType.NoValue, ShortName = "q")]
        public bool? Quiet { get; set; }
        [Option(CommandOptionType.MultipleValue, ShortName = "R")]
        public IEnumerable<string> RemoteForward { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "S")]
        public string Signature { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "x")]
        public string ConnectionString { get; internal set; }

        async Task<int> OnExecute()
        {
            if (Run != null)
            {
                return await Run(this);
            }
            return -1;
        }

        public static Func<CommandLineSettings, Task<int>> Run { get; set; }
        
    }
}
