// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Relay;
    
    [ConfigurationCollection(
        typeof(TcpClientElement),
        AddItemName = Constants.HybridConnectionsConfigName,
        CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    [SuppressMessage("Microsoft.Design", "CA1010",
        Justification = "Generic collection interface is not needed by callers.")]
    public class TcpClientElementCollection : ConfigurationElementCollection
    {
        public new TcpClientElement this[string name]
        {
            get { return (TcpClientElement)this.BaseGet(name); }
        }

        public void Add(TcpClientElement hybridConnectionElement)
        {
            this.BaseAdd(hybridConnectionElement);
        }

        public void Remove(string portBridgeEndpointKey)
        {
            this.BaseRemove(portBridgeEndpointKey);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TcpClientElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var connectionString = ((TcpClientElement)element).RelayConnectionString;
             var manager = new RelayConnectionStringBuilder(connectionString);
            return manager.Endpoint.ToString();
        }
    }
}