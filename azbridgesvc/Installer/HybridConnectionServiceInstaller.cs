// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Installer
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceProcess;

    /// <summary>
    /// Installer class for installing HybridConnectionManager as NT Service.
    /// </summary>
    [RunInstaller(true)]
    public class HybridConnectionServiceInstaller : System.Configuration.Install.Installer
    {
        public const string ServiceName = "Microsoft HybridConnectionManager";

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            this.Init();
            base.Install(stateSaver);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            this.Init();
            base.Uninstall(savedState);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification =
                "serviceInstaller and processInstaller are used internally to install the service. They live for the duration of the installation."
            )]
        void Init()
        {
            var processInstaller = new ServiceProcessInstaller { Account = ServiceAccount.NetworkService };

            var serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                DelayedAutoStart = true,
                ServiceName = HybridConnectionServiceInstaller.ServiceName,
                DisplayName = HybridConnectionServiceInstaller.ServiceName
            };

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}