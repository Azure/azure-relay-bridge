namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Text.RegularExpressions;

    public class LocalForwardBinding
    {
        private string bindAddress;
        private string hostName;
        private int bindPort;
        private string bindLocalSocket = null;

        string portName;

        public LocalForwardBinding()
        {
        }

        public LocalForwardBinding(string listenHostName, int listenPort)
        {
            this.BindAddress = listenHostName;
            this.BindPort = listenPort;
        }
                                   

        public string BindAddress
        {
            get
            {
                return bindAddress;
            }

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

        public string HostName
        {
            get
            {
                return hostName;
            }

            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (Uri.CheckHostName(val) != UriHostNameType.Dns)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(BindAddress),
                        $"Invalid HostName value: {val}. Must be a valid DNS host name",
                        this);
                }
                hostName = val;
            }
        }

        public int BindPort
        {
            get => bindPort;
            set
            {
                if (value < -65536 || value == 0 || value > 65535)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(BindPort),
                        $"Invalid BindPort value: {value}. Must be in the IP port range 1..65535 (negative port numbers for UDP)",
                        this);
                }
                bindPort = value;
            }
        }

        public string PortName
        {
            get
            {
                if (portName == null)
                {
                    if (BindPort != 0)
                    {
                        return (BindPort > 0) ? 
                            BindPort.ToString() : 
                            Math.Abs(BindPort).ToString() + "U";
                    }
                }

                return portName;
            }
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                if (val != null &&
                    !new Regex("^[0-9A-Za-z_-]+$").Match(val).Success)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(PortName),
                        $"Invalid PortName value: {val}. Must be a valid port name expression",
                        this);
                }
                portName = val;
            }
        }

        public string BindLocalSocket
        {
            get => bindLocalSocket;
            set
            {
                var val = value != null ? value.Trim('\'', '\"') : value;
                Uri path;
                if (val != null &&
                    !Uri.TryCreate(val, UriKind.Absolute, out path) &&
                    !new Regex("^[0-9A-Za-z_-]+$").Match(val).Success)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(BindLocalSocket),
                        $"Invalid BindLocalSocket value: {val}. Must be a valid local socket expression",
                        this);
                }
                bindLocalSocket = val;
            }
        }
    }
}