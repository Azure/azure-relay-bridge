// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Microsoft.HybridConnectionManager.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Host
    {
        readonly static string ConfigFileName = "mshcmsvc.config";
        
        TcpListenerHost hybridConnectionTcpListenerHost;
        TcpClientHost hybridConnectionTcpClientHost;
        FileSystemWatcher fsw;
        string configurationFile;
        private string configFileName;

        public Host(string configurationFile)
        {
            this.configurationFile = configurationFile;
        }

        public void Start()
        {
            this.configFileName = configurationFile ?? GetConfigFileName();
            if (!File.Exists(configFileName))
            {
                EventSource.Log.HybridConnectionManagerConfigurationFileError(null, "");
                throw new FileNotFoundException(configFileName);
            }

            fsw = new FileSystemWatcher(configFileName);
            fsw.Changed += ConfigFileChanged;
            var config = LoadConfig(configFileName);
            this.hybridConnectionTcpClientHost = new TcpClientHost(config.Targets);
            this.hybridConnectionTcpListenerHost = new TcpListenerHost(config.Listeners);

            this.hybridConnectionTcpClientHost.Start();
            this.hybridConnectionTcpListenerHost.Start();
        }

        private void ConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            var config = LoadConfig(configFileName);
            hybridConnectionTcpClientHost.UpdateConfig(config.Targets);
            hybridConnectionTcpListenerHost.UpdateConfig(config.Listeners);
        }

        public static void SaveConfig(string configFilePath, ConnectionConfig connectionConfig)
        {
            JObject config = null;
            if (File.Exists(configFilePath))
            {
                using (var reader = new StreamReader(configFilePath, true))
                {
                    var jr = new JsonTextReader(reader);
                    config = JObject.Load(jr);
                }
            }
            else
            {
                config = new JObject();
            }
            config["connections"] = JObject.FromObject(connectionConfig);
            using (var writer = new StreamWriter(configFilePath, false, Encoding.UTF8))
            {
                using (var jwriter = new JsonTextWriter(writer))
                {
                    jwriter.Formatting = Formatting.Indented;
                    config.WriteTo(jwriter);
                    jwriter.Flush();
                }                    
            }
        }

        public static string GetConfigFileName()
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

        public static ConnectionConfig LoadConfig(string configFileName)
        {   
            using (var reader = new StreamReader(configFileName, true))
            {
                var jr = new JsonTextReader(reader);
                JObject config = JObject.Load(jr);
                if (config.ContainsKey("connections"))
                {
                    return config["connections"].ToObject<ConnectionConfig>();
                }
            }
            return new ConnectionConfig();
        }

        public void Stop()
        {
            this.hybridConnectionTcpClientHost.Stop();
            this.hybridConnectionTcpListenerHost.Stop();
        }
    }
}
