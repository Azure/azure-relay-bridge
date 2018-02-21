
namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
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
                // TODO log
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
