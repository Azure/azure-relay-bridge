// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    static class Constants
    {
        public const string ConfigFileName = "Microsoft.HybridConnectionManager.Listener.exe.config";

        public const string HybridConnectionManagerConfigurationNounName = "HybridConnectionManagerConfiguration";

        public const string HybridConnectionManagerDefaultParameterSetName = "HybridConnectionManagerDefaultParamSet";

        public const string HybridConnectionNounName = "HybridConnection";

        public const string TcpClientConfigName = "tcpClients";
        public const string TcpListenerConfigName = "tcpListeners";

        public const string HybridConnectionsConfigName = "hybridConnection";

        public const string HybridConnectionsRegistryPath = @"SOFTWARE\Microsoft\HybridConnectionManager\0.7.5";

        public const string HybridConnectionsSectionConfigName = "hybridConnections";

        public const string InstallDir = "installDir";

        public const string ManagementPort = "managementPort";

        public const string AddHostNameScriptFile = "add-hostnames.ps1";

        public const string ConfigSetting = ConfigSettingPrefix + "ConnectionStrings";

        public const string ConfigSettingPrefix = "Microsoft.WindowsAzure.Plugins.HybridConnectionClient.";

        public const string ConnectionString = "ConnectionString";

        public const string GetHostNamesScriptFile = "get-hostnames.ps1";

        public const string RemoveHostNameScriptFile = "remove-hostnames.ps1";
        
    }
}