// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.



namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LocalForward
    {
        private RelayConnectionStringBuilder relayConnectionStringBuilder;

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
            set { relayConnectionStringBuilder = value != null ? new RelayConnectionStringBuilder(value) : new RelayConnectionStringBuilder(); }
        }

        internal RelayConnectionStringBuilder RelayConnectionStringBuilder
        {
            get { return relayConnectionStringBuilder; }
        }

        public string BindAddress { get; set; }

        public string HostName { get; set; }

        public int BindPort { get; set; }

        public string RelayName { get; set; }

        public string BindLocalSocket { get; set; }
        
    }
}