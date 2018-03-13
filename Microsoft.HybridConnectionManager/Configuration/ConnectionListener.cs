// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.



namespace Microsoft.HybridConnectionManager.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ConnectionListener
    {
        public ConnectionListener()
        {
        }

        public ConnectionListener(string connectionString, string listenHostName, int listenPort)
        {
            this.ConnectionString = connectionString;
            this.HostName = listenHostName;
            this.Port = listenPort;
        }

        public string ConnectionString { get; set; }

        public string HostName { get; set; }

        public int Port { get; set; }
    }
}