// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RemoteForward
    {
        private RelayConnectionStringBuilder relayConnectionStringBuilder;

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
            set { relayConnectionStringBuilder = value != null ? new RelayConnectionStringBuilder(value) : new RelayConnectionStringBuilder(); }
        }

        internal RelayConnectionStringBuilder RelayConnectionStringBuilder
        {
            get { return relayConnectionStringBuilder; }
        }

        public string Host { get; set; }

        public int HostPort { get; set; }

        public string RelayName { get; set; }

        public string LocalSocket { get; set; }
    }
}