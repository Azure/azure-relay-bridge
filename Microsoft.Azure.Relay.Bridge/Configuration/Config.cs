// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class Config
    {
        RelayConnectionStringBuilder relayConnectionStringBuilder;

        public Config()
        {
            relayConnectionStringBuilder = new RelayConnectionStringBuilder();
        }

        /// <summary>
        /// Specifies which address family to use when connecting.Valid
        /// arguments are Unspecified ("any"), InterNetwork ("inet", IPv4 only), 
        /// or InterNetworkV6 ("inet6", IPv6 only).  The default is Unspecified. 
        /// </summary>
        public string AddressFamily { get; set; }

        /// <summary>
        /// Azure Relay connection string for a Relay namespace.
        /// </summary>
        public string AzureRelayConnectionString
        {
            get { return relayConnectionStringBuilder.ToString(); }
            set { relayConnectionStringBuilder = value != null ? new RelayConnectionStringBuilder(value) : new RelayConnectionStringBuilder(); }
        }

        RelayConnectionStringBuilder RelayConnectionStringBuilder
        {
            get { return relayConnectionStringBuilder; }
        }


        /// <summary>
        /// Azure Relay endpoint URI for a Relay namespace.
        /// </summary>
        public string AzureRelayEndpoint { get => RelayConnectionStringBuilder.Endpoint.ToString(); set => RelayConnectionStringBuilder.Endpoint = new Uri(value); }

        /// <summary>
        /// Azure Relay shared access policy name.
        /// </summary>
        public string AzureRelaySharedAccessKeyName { get => RelayConnectionStringBuilder.SharedAccessKeyName; set => RelayConnectionStringBuilder.SharedAccessKeyName = value; }

        /// <summary>
        /// Azure Relay shared access policy key.
        /// </summary>
        public string AzureRelaySharedAccessKey { get => RelayConnectionStringBuilder.SharedAccessKey; set => RelayConnectionStringBuilder.SharedAccessKey = value; }

        /// <summary>
        /// Azure Relay shared access policy signature
        /// </summary>                                
        public string AzureRelaySharedAccessSignature { get => RelayConnectionStringBuilder.SharedAccessSignature; set => RelayConnectionStringBuilder.SharedAccessSignature = value; }

        /// <summary>
        /// Use the specified address on the local machine as the source
        /// address of the connection. Only useful on systems with more than
        /// one address. 
        /// </summary>
        public string BindAddress { get; set; }

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
        /// Specifies whether to use compression.
        /// </summary>
        /// <value>
        ///   <c>true</c> if compression is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool? Compression { get; set; }

        /// <summary>
        /// Gets or sets the compression level.
        /// </summary>
        /// <value>
        /// The compression level.
        /// </value>
        public int? CompressionLevel { get; set; }

        /// <summary>
        /// Gets or sets the connection attempts.
        /// </summary>
        /// <value>
        /// The connection attempts.
        /// </value>
        public int? ConnectionAttempts { get; set; }

        /// <summary>
        /// Specifies the timeout (in seconds) used when connecting to the
        /// Relay server, instead of using the default system TCP timeout.
        /// This value is used only when the target is down or really
        /// unreachable, not when it refuses the connection.
        /// </summary>
        public int? ConnectTimeout { get; set; }

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
        public List<LocalForward> LocalForward { get; set; } = new List<LocalForward>();

        /// <summary>
        ///  Gives the verbosity level that is used when logging messages 
        /// from azbridge(1). The possible values are: QUIET, FATAL, ERROR, INFO, VERBOSE, 
        /// DEBUG, DEBUG1, DEBUG2, and DEBUG3.The default is INFO.
        /// DEBUG and DEBUG1 are equivalent.DEBUG2 and DEBUG3 each specify
        /// higher levels of verbose output.
        /// </summary>
        public string LogLevel { get; set; }
        /// <summary>
        /// Specifies that a TCP port on the remote machine be bound to 
        /// a name on the Azure Relay.
        /// </summary>
        public List<RemoteForward> RemoteForward { get; set;  } = new List<RemoteForward>();


        public static Config LoadConfig(CommandLineSettings commandLineSettings)
        {
            const string azurebridge = "azurebridge";
            string machineConfigFileName =
                (Environment.OSVersion.Platform == PlatformID.Unix) ?
                $"/etc/{azurebridge}/{azurebridge}_config.yml" :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), $"{azurebridge}\\{azurebridge}_config.yml");

            Config config = LoadConfigFile(machineConfigFileName);

            if (commandLineSettings.ConfigFile == null)
            {
                string userConfigFileName =
                    (Environment.OSVersion.Platform == PlatformID.Unix) ?
                    Path.Combine(Environment.GetEnvironmentVariable("HOME"), $".{azurebridge}/{azurebridge}_config.yml") :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{azurebridge}\\{azurebridge}_config.yml");

                Config userConfig = LoadConfigFile(userConfigFileName);
                config.Merge(userConfig);
            }
            else
            {
                Config overrideConfig = LoadConfigFile(commandLineSettings.ConfigFile);
                config.Merge(overrideConfig);
            }

            if (commandLineSettings.Option != null)
            {
                foreach (var opt in commandLineSettings.Option)
                {
                    config.Merge(yamlDeserializer.Deserialize<Config>(new StringReader(opt)));
                }
            }

            if (commandLineSettings.BindAddress != null)
            {
                config.BindAddress = commandLineSettings.BindAddress;
            }
            if ( commandLineSettings.CleanHosts.HasValue)
            {
                config.ClearAllForwardings = commandLineSettings.CleanHosts;
            }
            if (commandLineSettings.Compression.HasValue)
            {
                config.Compression = commandLineSettings.Compression;
            }
            if (commandLineSettings.EndpointUri != null)
            {
                config.AzureRelayEndpoint = commandLineSettings.EndpointUri?.ToString();
            }
            if (commandLineSettings.GatewayPorts.HasValue)
            {
                config.GatewayPorts = commandLineSettings.GatewayPorts;
            }
            if (commandLineSettings.Quiet.HasValue)
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
            return config;
        }

        public void SaveConfigFile(string configFileName, bool merge)
        {
            if (configFileName == null)
            {
                throw new ArgumentNullException(nameof(configFileName));
            }

            Config savedConfig = null;
            if (merge && File.Exists(configFileName))
            {
                using (var reader = new StreamReader(configFileName))
                {
                    savedConfig = yamlDeserializer.Deserialize<Config>(reader);
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
            if ( otherConfig == null)
            {
                throw new ArgumentNullException(nameof(otherConfig));
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
            if (otherConfig.Compression.HasValue)
            {
                this.Compression = otherConfig.Compression;
            }
            if (otherConfig.CompressionLevel.HasValue)
            {
                this.CompressionLevel = otherConfig.CompressionLevel;
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

            this.LocalForward.AddRange(otherConfig.LocalForward);
            this.RemoteForward.AddRange(otherConfig.RemoteForward);
        }

        public static Config LoadConfigFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    return yamlDeserializer.Deserialize<Config>(reader);
                }
            }
            else
            {
                return new Config();
            }
        }

        static Deserializer yamlDeserializer = 
            new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(new PascalCaseNamingConvention()).Build();

        static Serializer yamlSerializer =
            new SerializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention()).Build();

    }
}
