// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.Azure.Relay;
    using Microsoft.HybridConnectionManager.Configuration;
    using Microsoft.Win32;

    public class HybridConnectionClientBaseCmdlet : PSCmdlet
    {
        protected System.Configuration.Configuration Config { get; set; }

        protected bool ConfigurationChanged { get; set; }

        protected TcpListenerConfigurationSection HybridConnectionsSection { get; set; }

        protected TcpListenerElement GetHybridConnectionElement(string connectionString)
        {
            var portBridgeEndpointKey = this.GetHybridConnectionElementKey(connectionString);
            return this.HybridConnectionsSection.HybridConnections[portBridgeEndpointKey];
        }

        protected string GetHybridConnectionElementKey(string connectionString)
        {
            RelayConnectionStringBuilder manager;
            try
            {
                manager = new RelayConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException exception)
            {
                this.ThrowTerminatingError(new ErrorRecord(exception, string.Empty, ErrorCategory.InvalidArgument, null));
                throw exception;
            }

            return manager.Endpoint.ToString();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000: DisposeObjectsBeforeLosingScope",
            Justification = "RegistryKey is auto-disposed at the end of using block")]
        static string GetInstallPath()
        {
#if WINDOWS
            using (RegistryKey registryKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(HybridConnectionConstants.HybridConnectionsRegistryPath,
                    RegistryKeyPermissionCheck.ReadSubTree))
            {
                if (registryKey != null)
                {
                    object obj = registryKey.GetValue(HybridConnectionConstants.InstallDir);
                    if (obj != null)
                    {
                        var installPath = (string)obj;
                        return installPath;
                    }
                }
            }
#endif
            return null;
        }


        void ReadConfig()
        {
            var installPath = GetInstallPath();

#if WINDOWS
        
            if (installPath == null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                    Strings.UnableToFindRegistryKey, HybridConnectionConstants.HybridConnectionsRegistryPath));
            }
#endif

            string configFilePath = Path.Combine(installPath, Constants.ConfigFileName);
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException(null, configFilePath);
            }

            this.Config = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = configFilePath, }, ConfigurationUserLevel.None);
        }

#region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.ReadConfig();
            this.HybridConnectionsSection =
                this.Config.GetSection(Constants.HybridConnectionsSectionConfigName) as
                    TcpListenerConfigurationSection;

            if (this.HybridConnectionsSection == null)
            {
                this.HybridConnectionsSection = new TcpListenerConfigurationSection();
                this.Config.Sections.Add(Constants.HybridConnectionsSectionConfigName,
                    this.HybridConnectionsSection);
                this.ConfigurationChanged = true;
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            if (this.ConfigurationChanged)
            {
                this.Config.Save(ConfigurationSaveMode.Full);
            }
        }

#endregion Overrides
    }
}