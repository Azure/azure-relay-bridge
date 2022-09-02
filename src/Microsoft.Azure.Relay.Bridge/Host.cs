﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using global::Azure.Core;
    using global::Azure.Identity;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Host
    {
        LocalForwardHost hybridConnectionTcpListenerHost;
        RemoteForwardHost hybridConnectionTcpClientHost;
        private Config config;
        EventTraceActivity hostActivity = BridgeEventSource.NewActivity("Host");

        public Host(Config config)
        {
            this.config = config;
        }

        static Host()
        {
            DefaultAzureCredentialTokenProvider = GetDefaultAzureCredentialTokenProvider();
        }

        static TokenProvider GetDefaultAzureCredentialTokenProvider()
        {
            return TokenProvider.CreateAzureActiveDirectoryTokenProvider(
                async (audience, authority, state) =>
                {
                    var defaultAzureCredential = new DefaultAzureCredential();
                    var trc = new TokenRequestContext(new[] { authority });
                    return (await defaultAzureCredential.GetTokenAsync(trc)).Token;
                },
                "https://relay.azure.net/.default");
        }

        public static TokenProvider DefaultAzureCredentialTokenProvider 
        {
            get;
        }

        public void Start()
        {
            hostActivity.DiagnosticsActivity.Start();
            this.config.Changed += ConfigChanged;
            this.hybridConnectionTcpClientHost = new RemoteForwardHost(config);
            this.hybridConnectionTcpListenerHost = new LocalForwardHost(config);

            this.hybridConnectionTcpClientHost.Start();
            this.hybridConnectionTcpListenerHost.Start();
        }

        private void ConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            this.config = e.NewConfig;
            hybridConnectionTcpClientHost.UpdateConfig(config);
            hybridConnectionTcpListenerHost.UpdateConfig(config);
        }

        public static void SaveConfig(string configFilePath, Config connectionConfig)
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

        public void Stop()
        {
            this.hybridConnectionTcpClientHost.Stop();
            this.hybridConnectionTcpListenerHost.Stop();
            hostActivity.DiagnosticsActivity.Stop();
        }
    }
}
