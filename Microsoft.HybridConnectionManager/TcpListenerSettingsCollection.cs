using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.HybridConnectionManager.Configuration;

namespace Microsoft.HybridConnectionManager
{
    class TcpListenerSettingsCollection : SettingsCollection<TcpListenerSetting>
    {
        public TcpListenerSettingsCollection(string configFileName) : base(configFileName)
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
                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap { ExeConfigFilename = this.ConfigFileName },
                    ConfigurationUserLevel.None);

                var hybridConnectionsSection = config.GetSection(Constants.HybridConnectionsSectionConfigName) as TcpListenerConfigurationSection;
                if (hybridConnectionsSection != null)
                {
                    var keys = new HashSet<string>(this.Keys);
                    foreach (TcpListenerElement hybridConnectionElement in hybridConnectionsSection.HybridConnections)
                    {
                        TcpListenerSetting info = new TcpListenerSetting(hybridConnectionElement);
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
            catch (ConfigurationErrorsException exception)
            {
                // log 
                throw;
            }

        }
    }
}
