// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    public class ConnectionConfig
    {
        public ConnectionListener[] Listeners
        {
            get; set;
        }

        public ConnectionTarget[] Targets
        {
            get; set;
        }
    }
}
