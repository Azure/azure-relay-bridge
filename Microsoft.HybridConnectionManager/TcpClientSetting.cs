// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using Microsoft.HybridConnectionManager.Configuration;

    sealed class TcpClientSetting : SettingsBase
    {
        public TcpClientSetting(TcpClientElement cfg):base(cfg.RelayConnectionString)
        {
            this.ResourceIdentifier = cfg.ResourceIdentifier;
            this.TargetHostName = cfg.TargetHost;
            this.TargetPort = cfg.TargetPort;
        }
        public string ResourceIdentifier { get; set; }
        public string TargetHostName { get; set; }
        public int TargetPort { get; set; }
    }
}
