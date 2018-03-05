// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using Microsoft.Extensions.Configuration.Json;

    public class ConnectionTarget
    {
        public ConnectionTarget()
        {
        }

        public ConnectionTarget(string connectionString, string targetHost, int targetPort)
        {
            this.RelayConnectionString = connectionString;
            this.TargetHost = targetHost;
            this.TargetPort = targetPort;
        }

        public string RelayConnectionString
        {
            get; set;
        }

        public string TargetHost
        {
            get; set;
        }

        public int TargetPort
        {
            get; set;
        }
    }
}