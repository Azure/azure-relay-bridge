// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Commands
{
    static class HybridConnectionConstants
    {
        public const string ConfigFileName = "Microsoft.HybridConnectionManager.Listener.exe.config";

        public const string HybridConnectionManagerConfigurationNounName = "HybridConnectionManagerConfiguration";

        public const string HybridConnectionManagerDefaultParameterSetName = "HybridConnectionManagerDefaultParamSet";

        public const string HybridConnectionManagerVersion = "0.7.5";

        public const string HybridConnectionNounName = "HybridConnection";

        public const string HybridConnectionsCollectionConfigName = "connectionStrings";

        public const string HybridConnectionsConfigName = "hybridConnection";

        public const string HybridConnectionsRegistryPath =
            @"SOFTWARE\Microsoft\HybridConnectionManager\" + HybridConnectionManagerVersion;

        public const string HybridConnectionsSectionConfigName = "hybridConnections";

        public const string InstallDir = "installDir";

        public const string ManagementPort = "managementPort";
    }
}