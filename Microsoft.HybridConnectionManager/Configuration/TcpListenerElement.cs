// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System.Configuration;

    public class TcpListenerElement : ConfigurationElement
    {
        const string ConnectionStringStr = "connectionString";
        const string ListenHostNameStr = "listenHostName";
        const string ListenPortStr = "listenPort";

        public TcpListenerElement()
        {
        }

        public TcpListenerElement(string connectionString)
        {
            this.RelayConnectionString = connectionString;
        }

        public TcpListenerElement(TcpListenerElement hybridConnectionClientElement)
        {
            this.RelayConnectionString = hybridConnectionClientElement.RelayConnectionString;
            this.ListenHostName = hybridConnectionClientElement.ListenHostName;
            this.ListenPort = hybridConnectionClientElement.ListenPort;
        }

        [ConfigurationProperty(ConnectionStringStr, IsRequired = true, IsKey = true)]
        public string RelayConnectionString
        {
            get { return (string)this[ConnectionStringStr]; }

            set { this[ConnectionStringStr] = value; }
        }

        [ConfigurationProperty(ListenHostNameStr, IsRequired = true, IsKey = false)]
        public string ListenHostName { get; set; }

        [ConfigurationProperty(ListenPortStr, IsRequired = true, IsKey = false)]
        public int ListenPort { get; set; }
    }
}