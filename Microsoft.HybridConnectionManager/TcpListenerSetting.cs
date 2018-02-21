// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using Microsoft.HybridConnectionManager.Configuration;

    sealed class TcpListenerSetting : SettingsBase
    {
        public TcpListenerSetting(TcpListenerElement cfg):base(cfg.RelayConnectionString)
        {
            this.ListenHostName = cfg.ListenHostName;
            this.ListenPort = cfg.ListenPort;
        }       
        public string ListenHostName { get; set; }
        public int ListenPort { get; set; }
    }
}
