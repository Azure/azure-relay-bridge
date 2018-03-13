// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System.IO;
    using System.Management.Automation;
    using Microsoft.HybridConnectionManager.Configuration;

    public class HybridConnectionClientBaseCmdlet : PSCmdlet
    {
        protected bool ConfigurationChanged { get; set; }
        public string ConfigFilePath { get; private set; }
        public ConnectionConfig Config { get; private set; }

        void ReadConfig()
        {
            this.ConfigFilePath = Microsoft.HybridConnectionManager.Host.GetConfigFileName();
            if (File.Exists(ConfigFilePath))
            {
                this.Config = Microsoft.HybridConnectionManager.Host.LoadConfig(this.ConfigFilePath);
            }
            else
            {
                this.Config = new ConnectionConfig();
            }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.ReadConfig();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            if (this.ConfigurationChanged)
            {
                Microsoft.HybridConnectionManager.Host.SaveConfig(this.ConfigFilePath, this.Config);
            }
            
        }
    }
}