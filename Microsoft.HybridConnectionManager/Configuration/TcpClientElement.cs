// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System.Configuration;

    public class TcpClientElement : ConfigurationElement
    {
        const string ResourceIdentifierStr = "resourceIdentifier";

        const string ConnectionStringStr = "connectionString";

        const string TargetHostStr = "destinationHost";

        const string TargetPortStr = "destinationPort";

        public TcpClientElement()
        {
        }
        
        public TcpClientElement(string connectionString, string targetHost, int targetPort)
        {
            this.RelayConnectionString = connectionString;
            this.TargetHost = targetHost;
            this.TargetPort = targetPort;
        }

        [ConfigurationProperty(ResourceIdentifierStr, IsRequired = false)]
        public string ResourceIdentifier
        {
            get { return (string)this[ResourceIdentifierStr]; }

            set { this[ResourceIdentifierStr] = value; }
        }

        [ConfigurationProperty(ConnectionStringStr, IsRequired = true, IsKey = true)]
        public string RelayConnectionString
        {
            get { return (string)this[ConnectionStringStr]; }

            set { this[ConnectionStringStr] = value; }
        }

        [ConfigurationProperty(TargetHostStr, IsRequired = false)]
        public string TargetHost
        {
            get { return (string)this[TargetHostStr]; }

            set { this[TargetHostStr] = value; }
        }

        [ConfigurationProperty(TargetPortStr, IsRequired = false)]
        public int TargetPort
        {
            get { return (int)this[TargetPortStr]; }

            set { this[TargetPortStr] = value; }
        }
    }
}