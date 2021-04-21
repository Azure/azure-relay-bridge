// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using YamlDotNet.Core;

    /// <summary>
    /// 
    /// </summary>
    public class Config : IDisposable
    {
        private const string AzureBridgeName = "azbridge";
        RelayConnectionStringBuilder relayConnectionStringBuilder;
        readonly List<FileSystemWatcher> fileSystemWatchers;
        List<LocalForward> localForward;
        List<RemoteForward> remoteForward;

        public Config()
        {
            relayConnectionStringBuilder = new RelayConnectionStringBuilder();
            fileSystemWatchers = new List<FileSystemWatcher>();
            localForward = new List<LocalForward>();
            remoteForward = new List<RemoteForward>();
        }

        /// <summary>
        /// Specifies which address family to use when connecting. Valid
        /// arguments are Unspecified ("any"), InterNetwork ("inet", IPv4 only), 
        /// or InterNetworkV6 ("inet6", IPv6 only).  The default is Unspecified. 
        /// </summary>
        public string AddressFamily
        {
            get => addressFamily;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (!string.IsNullOrEmpty(val) &&
                    val != "any" &&
                    val != "inet" &&
                    val != "inet6")
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(AddressFamily),
                        string.Format(Strings.MsgConfigInvalidAddressFamilyValue, val),
                        this);
                }
                addressFamily = val;
            }
        }

        /// <summary>
        /// Azure Relay connection string for a Relay namespace.
        /// </summary>
        public string AzureRelayConnectionString
        {
            get
            {
                if (relayConnectionStringBuilder.Endpoint == null)
                {
                    return null;
                }
                return relayConnectionStringBuilder.ToString();
            }
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                try
                {
                    relayConnectionStringBuilder = ((val != null) ? new RelayConnectionStringBuilder(val) : new RelayConnectionStringBuilder());
                }
                catch (ArgumentException e)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(AzureRelayConnectionString),
                        string.Format(Strings.MsgConfigInvalidAzureRelayConnectionStringValue, e.Message, val),
                        this);
                }
            }
        }

        RelayConnectionStringBuilder RelayConnectionStringBuilder
        {
            get { return relayConnectionStringBuilder; }
        }


        /// <summary>
        /// Azure Relay endpoint URI for a Relay namespace.
        /// </summary>
        public string AzureRelayEndpoint
        {
            get => RelayConnectionStringBuilder.Endpoint?.ToString();
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (string.IsNullOrWhiteSpace(val))
                {
                    RelayConnectionStringBuilder.Endpoint = new Uri("sb://undefined");
                }
                else
                {
                    try
                    {
                        RelayConnectionStringBuilder.Endpoint = new Uri(val);
                    }
                    catch (UriFormatException e)
                    {
                        throw BridgeEventSource.Log.ArgumentOutOfRange(
                            nameof(AzureRelayEndpoint),
                            string.Format(Strings.MsgConfigInvalidAzureRelayEndpointValue, e.Message, val),
                            this);
                    }
                }
            }
        }

        /// <summary>
        /// Azure Relay shared access policy name.
        /// </summary>
        public string AzureRelaySharedAccessKeyName
        {
            get => RelayConnectionStringBuilder.SharedAccessKeyName;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (val != null)
                {
                    foreach (var ch in val)
                    {
                        if (!char.IsLetterOrDigit(ch) &&
                            ch != '-' && ch != '_')
                        {
                            throw BridgeEventSource.Log.ArgumentOutOfRange(
                                nameof(AzureRelaySharedAccessKeyName),
                                $"Invalid -K/AzureRelaySharedAccessKeyName value {val}",
                                this);
                        }
                    }
                }
                RelayConnectionStringBuilder.SharedAccessKeyName = val;
            }
        }

        /// <summary>
        /// Azure Relay shared access policy key.
        /// </summary>
        public string AzureRelaySharedAccessKey
        {
            get => RelayConnectionStringBuilder.SharedAccessKey;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                try
                {
                    if (val != null)
                    {
                        Convert.FromBase64String(val);
                    }
                    RelayConnectionStringBuilder.SharedAccessKey = val;
                }
                catch (FormatException e)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(AzureRelaySharedAccessKey),
                        $"Invalid -k/AzureRelaySharedAccessKey value. {e.Message}: {val}",
                        this);
                }
            }
        }

        /// <summary>
        /// Azure Relay shared access policy signature
        /// </summary>                                
        public string AzureRelaySharedAccessSignature
        {
            get { return RelayConnectionStringBuilder.SharedAccessSignature; }
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                try
                {
                    RelayConnectionStringBuilder.SharedAccessSignature = val;
                }
                catch (ArgumentException e)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(AzureRelaySharedAccessSignature),
                        $"Invalid -s/AzureRelaySharedAccessSignature value. {e.Message}: {val}",
                        this);
                }
            }
        }

        /// <summary>
        /// Use the specified address on the local machine as the source
        /// address of the connection. Only useful on systems with more than
        /// one address. 
        /// </summary>
        public string BindAddress
        {
            get => bindAddress;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (val == null)
                {
                    bindAddress = null;
                }
                else
                {
                    // invalid characters in expression?
                    if (Uri.CheckHostName(val) == UriHostNameType.Unknown)
                    {
                        throw BridgeEventSource.Log.ArgumentOutOfRange(
                            nameof(BindAddress),
                            $"Invalid -b/BindAddress value: {val}. Must be a valid IPv4, IPv6, or DNS host name expression bindable on the local host",
                            this);
                    }

                    // "any" address?
                    if (!val.Equals("0.0.0.0") &&
                        !val.Equals("::") &&
                        !val.Equals("any", StringComparison.InvariantCultureIgnoreCase) &&
                        !val.Equals("anyv6", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            var computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                            var unicastAddresses = computerProperties.GetUnicastAddresses();
                            IList<IPAddress> ipAddresses = null;

                            ipAddresses = IPAddress.TryParse(val, out var ipAddress)
                               ? new[] { ipAddress }
                               : Dns.GetHostEntry(val).AddressList;

                            // check whether at least one of the resolved addresses is indeed
                            // locally bindable or a loopback address
                            if ((from hostAddress in ipAddresses
                                 where IPAddress.IsLoopback(hostAddress)
                                 select hostAddress).FirstOrDefault() != null ||
                                (from unicastAddress in unicastAddresses
                                 join hostAddress in ipAddresses on unicastAddress.Address equals hostAddress
                                 select hostAddress).FirstOrDefault() != null)
                            {
                                // we're only picking up the string. We do this resolution again
                                // when we bind the listener for real because we may have to 
                                // pick by family and we may run into used ports.
                                bindAddress = val;
                                return;
                            }
                        }
                        catch
                        {
                            // if the resolution fails we'll want to throw
                            // the same (below) as if the query fails
                        }
                        throw BridgeEventSource.Log.ArgumentOutOfRange(
                            nameof(BindAddress),
                            $"Invalid -b/BindAddress value: {val}. Must be a valid IPv4, IPv6, or DNS host name expression bindable on the local host",
                            this);
                    }

                    // if the address is "any", we just set it to null
                    bindAddress = null;
                }

                bindAddress = value;
            }
        }

        /// <summary>
        /// Specifies that all local, and remote port forwardings
        /// specified in the configuration files or on the command line be
        /// cleared.This option is primarily useful when used from the        
        /// command line to clear port forwardings set in configura-        
        /// tion files. The default is false.
        /// </summary>
        /// <value>
        ///   <c>true</c> to clear all forwardings; otherwise, <c>false</c>.
        /// </value>
        public bool? ClearAllForwardings { get; set; }

        /// <summary>
        /// Gets or sets the connection attempts.
        /// </summary>
        /// <value>
        /// The connection attempts.
        /// </value>
        public int? ConnectionAttempts
        {
            get => connectionAttempts;
            set
            {
                if (value.HasValue && (
                    value.Value < 1 ||
                    value.Value > 10))
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(BindAddress),
                        $"Invalid ConnectionAttempts value: {value}. Must be between 1 and 10.",
                        this);
                }
                connectionAttempts = value;
            }
        }

        /// <summary>
        /// Specifies the timeout (in seconds) used when connecting to the
        /// Relay server, instead of using the default system TCP timeout.
        /// This value is used only when the target is down or really
        /// unreachable, not when it refuses the connection.
        /// </summary>
        public int? ConnectTimeout
        {
            get => connectTimeout;
            set
            {
                if (value.HasValue && (
                        value.Value < 0 ||
                        value.Value > 120))
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(BindAddress),
                        $"Invalid ConnectTimeout value: {value}. Must be between 0 and 120.",
                        this);
                }
                connectTimeout = value;
            }
        }

        /// <summary>
        /// Specifies whether the client should terminate the 
        /// connection if it cannot set up all requested local, and remote port forwardings,
        /// (e.g. if either end is unable to bind and listen on a specified port). 
        /// The default is false.
        /// </summary>
        public bool? ExitOnForwardFailure { get; set; }

        /// <summary>
        /// Specifies whether remote hosts are allowed to connect to local
        /// forwarded ports. By default, azbridge(1) binds local port
        /// forwardings to the loopback address.This prevents other remote hosts
        /// from connecting to forwarded ports.GatewayPorts can be used to
        /// specify that azbridge should bind local port forwardings to the
        /// wildcard address, thus allowing remote hosts to connect to forwarded
        /// ports. The default is false.
        /// </summary>
        public bool? GatewayPorts { get; set; }

        /// <summary>
        /// Specifies that a (set of) TCP ports on the local machine 
        /// shall be forwarded via the Azure Relay.
        /// </summary>
        public List<LocalForward> LocalForward
        {
            get
            {
                return localForward;
            }
            set
            {
                localForward.Clear();
                if (value != null)
                {
                    localForward.AddRange(value);
                }
            }
        }

        /// <summary>
        ///  Gives the verbosity level that is used when logging messages 
        /// from azbridge(1). The possible values are: QUIET, FATAL, ERROR, INFO, VERBOSE, 
        /// DEBUG, DEBUG1, DEBUG2, and DEBUG3.The default is INFO.
        /// DEBUG and DEBUG1 are equivalent.DEBUG2 and DEBUG3 each specify
        /// higher levels of verbose output.
        /// </summary>
        public string LogLevel
        {
            get => logLevel;
            set
            {
                var s = new List<string>
                {
                    "none",
                    "quiet",
                    "fatal",
                    "error",
                    "info",
                    "verbose",
                    "debug",
                    "debug1",
                    "debug2",
                    "debug3"
                };
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (string.IsNullOrEmpty(val) || s.Contains(val.ToLower()))
                {
                    logLevel = val;
                }
                else
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(LogLevel),
                        $"Invalid LogLevel value {val}.", this);
                }
            }
        }

        /// <summary>
        ///  Log file name
        /// </summary>
        public string LogFileName
        {
            get => logFileName;
            set
            {
                logFileName = value;
            }
        }

        /// <summary>
        /// Specifies that a TCP port on the remote machine be bound to 
        /// a name on the Azure Relay.
        /// </summary>
        public List<RemoteForward> RemoteForward
        {
            get
            {
                return remoteForward;
            }
            set
            {
                remoteForward.Clear();
                if (value != null)
                {
                    remoteForward.AddRange(value);
                }
            }
        }

        internal event EventHandler<ConfigChangedEventArgs> Changed;
        void RaiseChanged(Config newConfig)
        {
            Changed?.Invoke(this, new ConfigChangedEventArgs { OldConfig = this, NewConfig = newConfig });
        }

        public static Config LoadConfig(CommandLineSettings commandLineSettings)
        {
            const string azbridge = AzureBridgeName;
            string machineConfigFileName =
                (Environment.OSVersion.Platform == PlatformID.Unix) ?
                $"/etc/{azbridge}/{azbridge}_config.machine.yml" :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), $"{azbridge}\\{azbridge}_config.machine.yml");

            Config config = LoadConfigFile(machineConfigFileName);
            Action onchange = () => { var cfg = LoadConfig(commandLineSettings); config.RaiseChanged(cfg); };

            if (Directory.Exists(Path.GetDirectoryName(machineConfigFileName)))
            {
                var fsw = new FileSystemWatcher(Path.GetDirectoryName(machineConfigFileName), Path.GetFileName(machineConfigFileName));
                fsw.Created += (o, e) => onchange();
                fsw.Deleted += (o, e) => onchange();
                fsw.Changed += (o, e) => onchange();
                config.fileSystemWatchers.Add(fsw);
            }

            if (string.IsNullOrEmpty(commandLineSettings.ConfigFile))
            {
                string userConfigFileName =
                    (Environment.OSVersion.Platform == PlatformID.Unix) ?
                    Path.Combine(Environment.GetEnvironmentVariable("HOME"), $".{azbridge}/{azbridge}_config.yml") :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{azbridge}\\{azbridge}_config.yml");

                Config userConfig = LoadConfigFile(userConfigFileName);
                config.Merge(userConfig);
                if (Directory.Exists(Path.GetDirectoryName(userConfigFileName)))
                {
                    var fsw = new FileSystemWatcher(Path.GetDirectoryName(userConfigFileName), Path.GetFileName(userConfigFileName));
                    fsw.Created += (o, e) => onchange();
                    fsw.Deleted += (o, e) => onchange();
                    fsw.Changed += (o, e) => onchange();
                    config.fileSystemWatchers.Add(fsw);
                }
            }
            else
            {
                Config overrideConfig = LoadConfigFile(commandLineSettings.ConfigFile);
                config.Merge(overrideConfig);
                if (Directory.Exists(Path.GetDirectoryName(commandLineSettings.ConfigFile)))
                {
                    var fsw = new FileSystemWatcher(Path.GetDirectoryName(commandLineSettings.ConfigFile), Path.GetFileName(commandLineSettings.ConfigFile));
                    fsw.Created += (o, e) => onchange();
                    fsw.Deleted += (o, e) => onchange();
                    fsw.Changed += (o, e) => onchange();
                    config.fileSystemWatchers.Add(fsw);
                }
            }

            if (commandLineSettings.Option != null)
            {
                MemoryStream memcfg = new MemoryStream();
                StreamWriter w = new StreamWriter(memcfg);
                foreach (var opt in commandLineSettings.Option)
                {
                    var opts = opt.Split(':');
                    w.WriteLine(opts[0] + " : " + ((opts.Length > 1) ? opts[1] : string.Empty));
                }
                w.Flush();
                memcfg.Position = 0;

                try
                {
                    config.Merge(yamlDeserializer.Deserialize<Config>(new StreamReader(memcfg)));
                }
                catch (YamlException e)
                {
                    throw MapYamlException(null, e);
                }
            }

            if (!string.IsNullOrEmpty(commandLineSettings.LogFile))
            {
                config.LogFileName = commandLineSettings.LogFile;
            }
            if (commandLineSettings.Verbose.HasValue && commandLineSettings.Verbose.Value)
            {
                config.LogLevel = "VERBOSE";
            }
            if (commandLineSettings.BindAddress != null)
            {
                config.BindAddress = commandLineSettings.BindAddress;
            }
            if (commandLineSettings.EndpointUri != null)
            {
                config.AzureRelayEndpoint = commandLineSettings.EndpointUri?.ToString();
            }
            if (commandLineSettings.GatewayPorts.HasValue)
            {
                config.GatewayPorts = commandLineSettings.GatewayPorts;
            }
            if (commandLineSettings.Quiet.HasValue && commandLineSettings.Quiet.Value)
            {
                config.LogLevel = "NONE";
            }
            if (commandLineSettings.SharedAccessKey != null)
            {
                config.AzureRelaySharedAccessKey = commandLineSettings.SharedAccessKey;
            }
            if (commandLineSettings.SharedAccessKeyName != null)
            {
                config.AzureRelaySharedAccessKeyName = commandLineSettings.SharedAccessKeyName;
            }
            if (commandLineSettings.Signature != null)
            {
                config.AzureRelaySharedAccessSignature = commandLineSettings.Signature;
            }
            if (commandLineSettings.ConnectionString != null)
            {
                config.AzureRelayConnectionString = commandLineSettings.ConnectionString;
            }

            if (commandLineSettings.LocalForward != null)
            {
                foreach (var lf in commandLineSettings.LocalForward)
                {
                    int lastColon = lf.LastIndexOf(':');
                    if (lastColon == -1)
                    {
                        throw BridgeEventSource.Log.ArgumentOutOfRange(
                            nameof(commandLineSettings.LocalForward),
                            $"Invalid -L expression: {lf}", config);
                    }

                    LocalForward localForward;
                    try
                    {
                        localForward = new LocalForward { RelayName = lf.Substring(lastColon + 1) };
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(commandLineSettings.LocalForward),
                            $"Invalid -L expression: {lf}; {e.Message}");
                    }
                    config.LocalForward.Add(localForward);
                    var bindings = lf.Substring(0, lastColon).Split(';');
                    foreach (var binding in bindings)
                    {
                        string[] lfs = binding.Split(':');
                        if (lfs.Length > 2)
                        {
                            throw BridgeEventSource.Log.ArgumentOutOfRange(
                                nameof(commandLineSettings.LocalForward),
                                $"Invalid -L expression: {lf}", config);
                        }
                        else if (lfs.Length == 1)
                        {
                            var portStrings = lfs[0].Split('/');
                            var portString = portStrings[0];
                            var portName = portStrings.Length > 1 ? portStrings[1] : portString;
                            // number suffixed by U?
                            if (new Regex("^[0-9]+U$").Match(portString).Success)
                            {
                                // UDP ports are just negative port numbers
                                portString = "-" + portString.Substring(0, portString.Length - 2);
                            }
                            // this is either -L local_socket:relay_name or -L port:relay_name
                            if (int.TryParse(portString, out var port))
                            {
                                try
                                {

                                    // port
                                    // local_socket
                                    localForward.Bindings.Add(new LocalForwardBinding
                                    {
                                        BindAddress = "localhost",
                                        BindPort = port,
                                        PortName = portName
                                    });
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(commandLineSettings.LocalForward),
                                        $"Invalid -L expression: {lf}; {e.Message}");
                                }
                            }
                            else
                            {
                                try
                                {
                                    // local_socket
                                    localForward.Bindings.Add(new LocalForwardBinding
                                    {
                                        BindLocalSocket = portString,
                                        PortName = portName
                                    });
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(commandLineSettings.LocalForward),
                                        $"Invalid -L expression: {lf}; {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            var portStrings = lfs[1].Split('/');
                            var portString = portStrings[0];
                            var portName = portStrings.Length > 1 ? portStrings[1] : portString;
                            // number suffixed by U?
                            if (new Regex("^[0-9]+U$").Match(portString).Success)
                            {
                                // UDP ports are just negative port numbers
                                portString = "-" + portString.Substring(0, portString.Length - 2);
                            }

                            // this is -L host:port:relay_name
                            if (int.TryParse(portString, out var port))
                            {
                                try
                                {
                                    // port
                                    // local_socket
                                    localForward.Bindings.Add(new LocalForwardBinding
                                    {
                                        BindAddress = lfs[0],
                                        BindPort = port,
                                        PortName = portName
                                    });
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(commandLineSettings.LocalForward),
                                        $"Invalid -L expression: {lf}; {e.Message}");
                                }
                            }
                            else
                            {
                                throw BridgeEventSource.Log.ArgumentOutOfRange(
                                    nameof(commandLineSettings.LocalForward),
                                    $"Invalid -L 'port' expression: {lfs[1]}", config);
                            }
                        }
                    }
                }
            }

            if (commandLineSettings.RemoteForward != null)
            {
                foreach (var rf in commandLineSettings.RemoteForward)
                {
                    int firstColon = rf.IndexOf(':');
                    if (firstColon == -1)
                    {
                        throw BridgeEventSource.Log.ArgumentOutOfRange(
                            nameof(commandLineSettings.RemoteForward),
                            $"Invalid -R expression: {rf}", config);
                    }

                    RemoteForward remoteForward;
                    try
                    {
                        remoteForward = new RemoteForward { RelayName = rf.Substring(0, firstColon) };
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(commandLineSettings.RemoteForward),
                            $"Invalid -R expression: {rf}; {e.Message}");
                    }
                    config.RemoteForward.Add(remoteForward);
                    var bindings = rf.Substring(firstColon + 1).Split(';');
                    foreach (var binding in bindings)
                    {
                        string[] rfs = binding.Split(':');
                        if (rfs.Length > 2)
                        {
                            throw BridgeEventSource.Log.ArgumentOutOfRange(
                                nameof(commandLineSettings.RemoteForward),
                                $"Invalid -R expression: {rf}",
                                config);
                        }
                        else if (rfs.Length == 1)
                        {
                            // this is either -R relay_name:[port_name/]port or -L relay_name:[port_name/]local_socket

                            var portStrings = rfs[0].Split('/');
                            if (portStrings.Length > 2)
                            {
                                throw BridgeEventSource.Log.ArgumentOutOfRange(
                                    nameof(commandLineSettings.RemoteForward),
                                    $"Invalid -R expression: {rf}",
                                    config);
                            }
                            var portString = portStrings.Length > 1 ? portStrings[1] : portStrings[0];
                            var portName = portStrings[0];
                            // number suffixed by U?
                            if (new Regex("^[0-9]+U$").Match(portString).Success)
                            {
                                // UDP ports are just negative port numbers
                                portString = "-" + portString.Substring(0, portString.Length - 2);
                            }

                            if (int.TryParse(portString, out var port))
                            {
                                try
                                {
                                    // port
                                    // local_socket
                                    remoteForward.Bindings.Add(new RemoteForwardBinding
                                    {
                                        Host = "localhost",
                                        HostPort = port,
                                        PortName = portName
                                    });
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(commandLineSettings.RemoteForward),
                                        $"Invalid -R expression: {rf}; {e.Message}");
                                }
                            }
                            else
                            {
                                try
                                {
                                    // local_socket
                                    remoteForward.Bindings.Add(new RemoteForwardBinding
                                    {
                                        LocalSocket = portString,
                                        PortName = portName
                                    });
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(commandLineSettings.RemoteForward),
                                        $"Invalid -R expression: {rf}; {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            // this is -L relay_name:host:[port_name/]port
                            var portStrings = rfs[1].Split('/');
                            if (portStrings.Length > 2)
                            {
                                throw BridgeEventSource.Log.ArgumentOutOfRange(
                                    nameof(commandLineSettings.RemoteForward),
                                    $"Invalid -R expression: {rf}",
                                    config);
                            }
                            var portString = portStrings.Length > 1 ? portStrings[1] : portStrings[0];
                            var portName = portStrings[0];
                            // number suffixed by U?
                            if (new Regex("^[0-9]+U$").Match(portString).Success)
                            {
                                // UDP ports are just negative port numbers
                                portString = "-" + portString.Substring(0, portString.Length - 2);
                            }

                            if (int.TryParse(portString, out var port))
                            {
                                try
                                {
                                    // port
                                    // local_socket
                                    remoteForward.Bindings.Add(new RemoteForwardBinding
                                    {
                                        Host = rfs[0],
                                        HostPort = port,
                                        PortName = portName
                                    });
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(commandLineSettings.RemoteForward),
                                        $"Invalid -R expression: {rf}; {e.Message}");
                                }
                            }
                            else
                            {
                                throw BridgeEventSource.Log.ArgumentOutOfRange(
                                    nameof(commandLineSettings.RemoteForward),
                                    $"Invalid -R 'port' expression: {rfs[2]}",
                                    config);
                            }
                        }
                    }
                }
            }
            return config;
        }

        public void SaveConfigFile(string configFileName, bool merge)
        {
            if (configFileName == null)
            {
                throw BridgeEventSource.Log.ArgumentNull(nameof(configFileName));
            }

            Config savedConfig = null;
            if (merge && File.Exists(configFileName))
            {
                try
                {
                    using (var reader = new StreamReader(configFileName))
                    {
                        savedConfig = yamlDeserializer.Deserialize<Config>(reader);
                    }
                }
                catch (YamlException e)
                {
                    throw MapYamlException(configFileName, e);
                }
                savedConfig.Merge(this);
            }
            using (var writer = new StreamWriter(configFileName, false, System.Text.Encoding.UTF8))
            {
                yamlSerializer.Serialize(writer, this);
            }
        }

        private void Merge(Config otherConfig)
        {
            if (otherConfig == null)
            {
                throw BridgeEventSource.Log.ArgumentNull(nameof(otherConfig));
            }

            if (otherConfig.AddressFamily != null)
            {
                this.AddressFamily = otherConfig.AddressFamily;
            }
            if (otherConfig.AzureRelayEndpoint != null)
            {
                this.AzureRelayEndpoint = otherConfig.AzureRelayEndpoint;
            }
            if (otherConfig.AzureRelaySharedAccessKey != null)
            {
                this.AzureRelaySharedAccessKey = otherConfig.AzureRelaySharedAccessKey;
            }
            if (otherConfig.AzureRelaySharedAccessKeyName != null)
            {
                this.AzureRelaySharedAccessKeyName = otherConfig.AzureRelaySharedAccessKeyName;
            }
            if (otherConfig.AzureRelaySharedAccessSignature != null)
            {
                this.AzureRelaySharedAccessSignature = otherConfig.AzureRelaySharedAccessSignature;
            }
            if (otherConfig.BindAddress != null)
            {
                this.BindAddress = otherConfig.BindAddress;
            }
            if (otherConfig.ClearAllForwardings.HasValue)
            {
                this.ClearAllForwardings = otherConfig.ClearAllForwardings;
            }
            if (otherConfig.ConnectionAttempts.HasValue)
            {
                this.ConnectionAttempts = otherConfig.ConnectionAttempts;
            }
            if (otherConfig.ConnectTimeout.HasValue)
            {
                this.ConnectTimeout = otherConfig.ConnectTimeout;
            }
            if (otherConfig.ExitOnForwardFailure.HasValue)
            {
                this.ExitOnForwardFailure = otherConfig.ExitOnForwardFailure;
            }
            if (otherConfig.GatewayPorts.HasValue)
            {
                this.GatewayPorts = otherConfig.GatewayPorts;
            }
            if (otherConfig.LogLevel != null)
            {
                this.LogLevel = otherConfig.LogLevel;
            }

            if (this.ClearAllForwardings.HasValue && this.ClearAllForwardings.Value)
            {
                this.LocalForward.Clear();
                this.RemoteForward.Clear();
            }

            this.LocalForward?.AddRange(otherConfig.LocalForward);
            this.RemoteForward?.AddRange(otherConfig.RemoteForward);
        }

        public static Config LoadConfigFile(string fileName)
        {
            if (fileName == null)
            {
                throw BridgeEventSource.Log.ArgumentNull(nameof(fileName));
            }

            if (File.Exists(fileName))
            {

                using (var reader = new StreamReader(fileName))
                {
                    try
                    {
                        return yamlDeserializer.Deserialize<Config>(reader);
                    }
                    catch (YamlException e)
                    {
                        throw MapYamlException(fileName, e);
                    }
                }
            }
            else
            {
                return new Config();
            }
        }

        static Exception MapYamlException(string fileName, YamlException e)
        {
            if (e.InnerException is SerializationException)
            {
                var msg = e.InnerException?.Message;
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    var propNotFound = new Regex("Property '([^']+)' not found");
                    var matchPropNotFound = propNotFound.Match(msg);
                    if (matchPropNotFound.Success && matchPropNotFound.Groups.Count > 1)
                    {
                        msg = $"Unknown configuration attribute: {matchPropNotFound.Groups[1].Value}";
                    }
                }

                return BridgeEventSource.Log.ThrowingException(new ConfigException(fileName, msg, e));
            }

            if (e.InnerException is TargetInvocationException)
            {
                return BridgeEventSource.Log.ThrowingException(new ConfigException(
                    fileName, e.InnerException.InnerException?.Message, e));
            }

            if (fileName != null)
            {
                using (var sr = new StreamReader(File.OpenRead(fileName)))
                {
                    string line = null;
                    for (int i = 0; i < e.Start.Line; i++)
                    {
                        line = sr.ReadLine();
                    }
                    return BridgeEventSource.Log.ThrowingException(new ConfigException(
                        fileName, $"Invalid value at {e.Start.Line}, {e.Start.Column}: {line} (message: {e.InnerException?.Message})", e));
                }
            }
            return BridgeEventSource.Log.ThrowingException(new ConfigException(
                fileName, e.InnerException?.Message, e));
        }

        static IDeserializer yamlDeserializer =
            new DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
            .Build();

        static ISerializer yamlSerializer =
            new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance).Build();

        private bool disposedValue = false; // To detect redundant calls
        private string bindAddress;
        private string addressFamily;
        private string logLevel;
        private int? connectionAttempts;
        private int? connectTimeout;
        private string logFileName;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var w in fileSystemWatchers)
                    {
                        w.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }

    class ConfigChangedEventArgs : EventArgs
    {
        public Config OldConfig { get; set; }
        public Config NewConfig { get; set; }
    }
}
