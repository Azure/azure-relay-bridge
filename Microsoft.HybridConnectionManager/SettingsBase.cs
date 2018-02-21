// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using Microsoft.Azure.Relay;

    public abstract class SettingsBase
    {
        public string RelayConnectionString { get; set; }

        public SettingsBase(string connectionString)
        {
            this.RelayConnectionString = connectionString;
        }
        public string Key
        {
            get
            {
                if (RelayConnectionString != null)
                {
                    return new RelayConnectionStringBuilder(RelayConnectionString).Endpoint?.AbsoluteUri;
                }
                else
                {
                    throw new InvalidOperationException("RealyConnectionString is null");
                }
            }
        }
    }
}
