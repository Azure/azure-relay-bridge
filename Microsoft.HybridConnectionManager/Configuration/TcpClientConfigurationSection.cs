// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System.Configuration;

    public class TcpClientConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty(Constants.TcpClientConfigName,
            IsDefaultCollection = true)]
        public TcpClientElementCollection HybridConnections
        {
            get
            {
                return
                    (TcpClientElementCollection)
                        this[Constants.TcpClientConfigName];
            }
        }
    }
}