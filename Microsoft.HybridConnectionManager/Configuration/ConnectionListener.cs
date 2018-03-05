// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    public class ConnectionListener
    {
        public ConnectionListener()
        {
        }

        public ConnectionListener(string connectionString, string listenHostName, int listenPort)
        {
            this.RelayConnectionString = connectionString;
            this.ListenHostName = listenHostName;
            this.ListenPort = listenPort;
        }

        public string RelayConnectionString { get; set; }

        public string ListenHostName { get; set; }

        public int ListenPort { get; set; }
    }
}