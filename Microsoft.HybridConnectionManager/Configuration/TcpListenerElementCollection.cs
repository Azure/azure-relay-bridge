// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Relay;
    using Microsoft.HybridConnectionManager;

    [ConfigurationCollection(
        typeof(TcpListenerElement),
        AddItemName = Constants.HybridConnectionsConfigName,
        CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    [SuppressMessage("Microsoft.Design", "CA1010",
        Justification = "Generic collection interface is not needed by callers.")]
    public class TcpListenerElementCollection : ConfigurationElementCollection
    {
        public new TcpListenerElement this[string name]
        {
            get { return (TcpListenerElement)this.BaseGet(name); }
        }

        public void Add(TcpListenerElement hybridConnectionElement)
        {
            this.BaseAdd(hybridConnectionElement);
        }

        public void Remove(string portBridgeEndpointKey)
        {
            this.BaseRemove(portBridgeEndpointKey);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TcpListenerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var connectionString = ((TcpListenerElement)element).RelayConnectionString;
            var manager = new RelayConnectionStringBuilder(connectionString);
            return manager.Endpoint.ToString();
        }
    }
}