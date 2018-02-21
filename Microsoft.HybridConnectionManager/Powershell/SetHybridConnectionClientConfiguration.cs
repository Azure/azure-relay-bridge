// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System.Configuration;
    using System.Globalization;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Set, Constants.HybridConnectionManagerConfigurationNounName)]
    public class SetHybridConnectionClientConfiguration : HybridConnectionClientBaseCmdlet
    {
        string oldPort;

        #region Parameters

        [Parameter(Mandatory = true)]
        public string ManagementPort { get; set; }

        #endregion Parameters

        #region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            var oldPortKeyValue = this.Config.AppSettings.Settings[Constants.ManagementPort];
            if (oldPortKeyValue != null)
            {
                // save old port value
                this.oldPort = oldPortKeyValue.Value;
            }

            if (string.Equals(this.oldPort, this.ManagementPort))
            {
                // Do not throw an exception. Allow this operation to succeed
                this.WriteWarning(string.Format(CultureInfo.CurrentCulture, Strings.ManagementPortMatchesCurrent));
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!string.Equals(this.oldPort, this.ManagementPort))
            {
                this.Config.AppSettings.Settings.Remove(Constants.ManagementPort);
                this.Config.AppSettings.Settings.Add(
                    new KeyValueConfigurationElement(Constants.ManagementPort, this.ManagementPort));
                this.ConfigurationChanged = true;
            }
        }

        #endregion Overrides
    }
}