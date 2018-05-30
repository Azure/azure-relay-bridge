// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.



namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LocalForward
    {
        public LocalForward()
        {
        }

        public LocalForward(string connectionString, string listenHostName, int listenPort)
        {
            this.ConnectionString = new RelayConnectionStringBuilder(connectionString);
            this.Host = listenHostName;
            this.HostPort = listenPort;
        }

        public RelayConnectionStringBuilder ConnectionString { get; private set; }

        public string Host { get; set; }

        public int HostPort { get; set; }

        public string RelayName { get; set; }

        public string LocalSocket { get; set; }
    }
}