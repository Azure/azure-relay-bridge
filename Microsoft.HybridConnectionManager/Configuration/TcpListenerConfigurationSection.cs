// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System.Configuration;

    public class TcpListenerConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty(Constants.TcpListenerConfigName,
            IsDefaultCollection = true)]
        public TcpListenerElementCollection HybridConnections
        {
            get
            {
                return
                    (TcpListenerElementCollection)
                        this[Constants.TcpListenerConfigName];
            }
        }
    }
}