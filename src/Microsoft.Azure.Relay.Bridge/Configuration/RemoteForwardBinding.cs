namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using System;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RemoteForwardBinding
    {
        string host;
        int hostPort;
        string localSocket = null;
        string portName;

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
                if (value < -65535 || value == 0 || value > 65535)
                {
                    throw BridgeEventSource.Log.ArgumentOutOfRange(
                        nameof(HostPort),
                        $"Invalid HostPort value: {value}. Must be in the IP port range 1..65535 (negative port numbers for UDP).",
                        this);
                }
                hostPort = value;
            }
        }

        public string PortName
        {
            get
            {
                if (portName == null)
                {
                    if (HostPort != 0)
                    {
                        return (HostPort > 0) ?
                            HostPort.ToString() :
                            Math.Abs(HostPort).ToString() + "U";
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
                portName = value;
            }
        }

        public string LocalSocket
        {
            get => localSocket;
            set
            {
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
            }
        }
    }
}