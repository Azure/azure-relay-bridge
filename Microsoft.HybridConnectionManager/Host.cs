// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.IO;

    public class Host
    {
        readonly string LinuxConfigFileName = "/etc/mshcmsvc/mshcmsvc.config";
        readonly string WindowsConfigFileName = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory),
                                                              @"\ProgramData\Microsoft\Microsoft.HybridConnectionManager\mshcm.config");

        TcpListenerHost hybridConnectionTcpListenerHost;
        TcpClientHost hybridConnectionTcpClientHost;

        public Host()
        {
        }

        public void Start(string[] args)
        {
            string configFileName;

            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                  Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                configFileName = LinuxConfigFileName;
            }
            else
            {
                configFileName = WindowsConfigFileName;
            }

            if (!File.Exists(configFileName))
            {
                EventSource.Log.HybridConnectionManagerConfigurationFileError(null, "");
                throw new FileNotFoundException(configFileName);
            }

            TcpClientSettingsCollection clientSettings = new TcpClientSettingsCollection(configFileName);
            this.hybridConnectionTcpClientHost = new TcpClientHost(clientSettings);
            TcpListenerSettingsCollection listenerSettings = new TcpListenerSettingsCollection(configFileName);
            this.hybridConnectionTcpListenerHost = new TcpListenerHost(listenerSettings);

            this.hybridConnectionTcpClientHost.Start(args);
            this.hybridConnectionTcpListenerHost.Start(args);
        }

        public void Stop()
        {
            this.hybridConnectionTcpClientHost.Stop();
            this.hybridConnectionTcpListenerHost.Stop();
        }
    }
}
