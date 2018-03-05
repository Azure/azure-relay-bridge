// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.HybridConnectionManager.Configuration;

    sealed class TcpClientSettingsCollection : SettingsCollection<TcpClientSetting>
    {
        public TcpClientSettingsCollection(string configFileName): base(configFileName)
        {

        }

        protected override void LoadConfiguration()
        {
            if (!File.Exists(this.ConfigFileName))
            {
                return;
            }

            try
            {
                var connectionConfig = new ConnectionConfig();
                var builder = new ConfigurationBuilder()
                   .SetBasePath(Path.GetDirectoryName(this.ConfigFileName))
                   .AddJsonFile(Path.GetFileName(this.ConfigFileName));

                var config = builder.Build();
                config.GetSection("Connections").Bind(connectionConfig);

                var hybridConnectionsSection = connectionConfig.Targets;
                if (hybridConnectionsSection != null)
                {
                    var keys = new HashSet<string>(this.Keys);
                    foreach (ConnectionTarget hybridConnectionElement in hybridConnectionsSection)
                    {
                        TcpClientSetting info = new TcpClientSetting(hybridConnectionElement);
                        if (this.ContainsKey(info.Key))
                        {
                            keys.Remove(info.Key);
                            this[info.Key] = info;
                        }
                        else
                        {
                            this.Add(info.Key, info);
                        }
                    }
                    foreach (var key in keys)
                    {
                        this.Remove(key);
                    }
                }
            }
            catch (Exception exception)
            {
                // log 
                throw;
            }

        }
    }
}
