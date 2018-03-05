// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

    public class Host
    {
        readonly string ConfigFileName = "mshcmsvc.config";
        
        TcpListenerHost hybridConnectionTcpListenerHost;
        TcpClientHost hybridConnectionTcpClientHost;

        public Host()
        {
        }

        public void Start()
        {
            var configFileName = GetConfigFileName();
            if (!File.Exists(configFileName))
            {
                EventSource.Log.HybridConnectionManagerConfigurationFileError(null, "");
                throw new FileNotFoundException(configFileName);
            }

            TcpClientSettingsCollection clientSettings = new TcpClientSettingsCollection(configFileName);
            this.hybridConnectionTcpClientHost = new TcpClientHost(clientSettings);
            TcpListenerSettingsCollection listenerSettings = new TcpListenerSettingsCollection(configFileName);
            this.hybridConnectionTcpListenerHost = new TcpListenerHost(listenerSettings);

            this.hybridConnectionTcpClientHost.Start();
            this.hybridConnectionTcpListenerHost.Start();
        }

        public string GetConfigFileName()
        {
            // we will return the name of the config file in the current
            // directory if one is present. Otherwise we return the 
            // location of where the config file ought to be
            string configFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ConfigFileName);
            if (!File.Exists(configFileName))
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix ||
                      Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    configFileName = "/etc/mshcmsvc/" + ConfigFileName;
                }
                else
                {
                    configFileName =
                        Path.Combine(Path.GetPathRoot(Environment.SystemDirectory),
                           @"\ProgramData\Microsoft\Microsoft.HybridConnectionManager\" + ConfigFileName);
                }
            }
            return configFileName;
        }

        public void Stop()
        {
            this.hybridConnectionTcpClientHost.Stop();
            this.hybridConnectionTcpListenerHost.Stop();
        }
    }
}
