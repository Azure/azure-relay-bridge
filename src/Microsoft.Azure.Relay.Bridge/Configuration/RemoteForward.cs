// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using System;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RemoteForward
    {
        private RelayConnectionStringBuilder relayConnectionStringBuilder;
        private string relayName;
        private string host;
        private int hostPort;
        private string localSocket = null;

        public RemoteForward()
        {
        }

        public RemoteForward(string connectionString, string Host, int targetPort)
        {
            this.relayConnectionStringBuilder = new RelayConnectionStringBuilder(connectionString);
            this.Host = Host;
            this.HostPort = targetPort;
        }

        public string ConnectionString
        {
            get { return relayConnectionStringBuilder?.ToString(); }
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
                        nameof(ConnectionString),
                        string.Format(Strings.MsgConfigInvalidConnectionStringValue, e.Message, val),
                        this);
                }
            }
        }

        internal RelayConnectionStringBuilder RelayConnectionStringBuilder
        {
            get { return relayConnectionStringBuilder; }
        }

        public string Host
        {
            get => host;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (Uri.CheckHostName(val) == UriHostNameType.Unknown)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(Host),
                        $"Invalid Host value: {val}. Must be a valid host name",
                        this);
                }
                host = val;
            }
        }

        public int HostPort
        {
            get => hostPort;
            set
            {
                if (value < 0 || value > 65535)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(HostPort),
                        $"Invalid HostPort value: {value}. Must be in the IP port range 0..65535.",
                        this);
                }
                hostPort = value;
            }
        }

        public string RelayName
        {
            get => relayName;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (val != null &&
                    !new Regex("^[0-9A-Za-z_-]+$").Match(val).Success)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(RelayName),
                        $"Invalid RelayName value: {val}. Must be a valid relay name expression",
                        this);
                }
                relayName = val;
            }
        }

        public string LocalSocket
        {
            get => localSocket;
            set
            {
#if NETFRAMEWORK
                throw BridgeEventSource.Log.ThrowingException(
                    new PlatformNotSupportedException($"Unix sockets are only supported in the .NET Core version"));
#else
                var val = value != null ? value.Trim('\'', '\"') : value;
                Uri path;
                if (val != null && 
                    !Uri.TryCreate(val, UriKind.Absolute, out path) &&
                    !new Regex("^[0-9A-Za-z_-]+$").Match(val).Success)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(LocalSocket),
                        $"Invalid LocalSocket value: {val}. Must be a valid local socket expression",
                        this);
                }
                localSocket = val;
#endif
            }
        }
    }
}