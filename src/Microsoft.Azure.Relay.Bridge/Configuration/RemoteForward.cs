// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RemoteForward
    {
        private RelayConnectionStringBuilder relayConnectionStringBuilder;
        private string relayName;
        List<RemoteForwardBinding> bindings = new List<RemoteForwardBinding>();

        public RemoteForward()
        {
        }

        public RemoteForward(string connectionString)
        {
            this.relayConnectionStringBuilder = new RelayConnectionStringBuilder(connectionString);
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

        public string Host
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].Host;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (bindings.Count == 0)
                {
                    bindings.Add(new RemoteForwardBinding { Host = value });
                }
                else 
                {
                    bindings[0].Host = value;
                }
            }
        }

        public string PortName
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].PortName;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (bindings.Count == 0)
                {
                    bindings.Add(new RemoteForwardBinding { PortName = value });
                }
                else
                {
                    bindings[0].PortName = value;
                }
            }
        }

        public int HostPort
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].HostPort;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (bindings.Count == 0)
                {
                    bindings.Add(new RemoteForwardBinding { HostPort = value });
                }
                else 
                {
                    bindings[0].HostPort = value;
                }
            }
        }                               


        public string LocalSocket
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].LocalSocket;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (bindings.Count == 0)
                {
                    bindings.Add(new RemoteForwardBinding { LocalSocket = value });
                }
                else 
                {
                    bindings[0].LocalSocket = value;
                }
            }
        }
    

        public List<RemoteForwardBinding> Bindings
        {
            get
            {
                return bindings;
            }
            set
            {
                bindings.Clear();
                if (value != null)
                {
                    bindings.AddRange(value);
                }
            }
        }
    }
}