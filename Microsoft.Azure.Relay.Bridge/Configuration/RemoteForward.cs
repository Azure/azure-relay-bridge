// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RemoteForward
    {
        public RemoteForward()
        {
        }

        public RemoteForward(string connectionString, string Host, int targetPort)
        {
            this.ConnectionString = new RelayConnectionStringBuilder(connectionString);
            this.Host = Host;
            this.Port = targetPort;
        }

        public RelayConnectionStringBuilder ConnectionString { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string RelayName { get; set; }

        public string LocalSocket { get; set; }
    }
}