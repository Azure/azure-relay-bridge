// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.



namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LocalForward
    {
        private RelayConnectionStringBuilder relayConnectionStringBuilder;
        private string relayName;
        List<LocalForwardBinding> bindings = new List<LocalForwardBinding>();


        public LocalForward()
        {
        }

        public LocalForward(string connectionString, string listenHostName, int listenPort)
        {
            this.relayConnectionStringBuilder = new RelayConnectionStringBuilder(connectionString);
            this.BindAddress = listenHostName;
            this.BindPort = listenPort;
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

        public string BindAddress
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].BindAddress;
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
                    bindings.Add(new LocalForwardBinding { BindAddress = value });
                }
                else
                {
                    bindings[0].BindAddress = value;
                }
            }
        }

        public string HostName
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].HostName;
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
                    bindings.Add(new LocalForwardBinding { HostName = value });
                }
                else
                {
                    bindings[0].HostName = value;
                }
            }
        }

        public int BindPort
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].BindPort;
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
                    bindings.Add(new LocalForwardBinding { BindPort = value });
                }
                else
                {
                    bindings[0].BindPort = value;
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
                    bindings.Add(new LocalForwardBinding { PortName = value });
                }
                else
                {
                    bindings[0].PortName = value;
                }
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

        public string BindLocalSocket
        {
            get
            {
                if (bindings.Count == 1)
                {
                    return bindings[0].BindLocalSocket;
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
                    bindings.Add(new LocalForwardBinding { BindLocalSocket = value });
                }
                else
                {
                    bindings[0].BindLocalSocket = value;
                }
            }
        }

        public List<LocalForwardBinding> Bindings
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