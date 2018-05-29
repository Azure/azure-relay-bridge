// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Management.Automation;
    
    [Cmdlet(VerbsCommon.Set, Constants.HybridConnectionManagerConfigurationNounName)]
    public class SetHybridConnectionManagerConfiguration : HybridConnectionBaseCmdlet
    {
        string oldPort;

        #region Parameters

        [Parameter(Mandatory = true)]
        public string ManagementPort { get; set; }

        [Parameter(Mandatory = false)]
        public bool RemoveOldPortUrlReservation { get; set; }

        #endregion Parameters

        #region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            //var oldPortKeyValue = this.Config.AppSettings.Settings[Constants.ManagementPort];
            //if (oldPortKeyValue != null)
            //{
            //    // save old port value
            //    this.oldPort = oldPortKeyValue.Value;
            //}

            //if (string.Equals(this.oldPort, this.ManagementPort))
            //{
            //    // Do not throw an exception. Allow this operation to succeed
            //    this.WriteWarning(string.Format(CultureInfo.CurrentCulture, Strings.ManagementPortMatchesCurrent));

            //    // Ensure that we do not remove the old port reservation
            //    this.RemoveOldPortUrlReservation = false;
            //}
            //else
            //{
            //    Collection<PSObject> results =
            //        ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture,
            //            "netsh http add urlacl url=http://+:{0}/ user=\"NT AUTHORITY\\NETWORK SERVICE\"",
            //            this.ManagementPort)).Invoke();
            //    this.WriteObject(results, true);
            //}
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            //if (!string.Equals(this.oldPort, this.ManagementPort))
            //{
            //    this.Config.AppSettings.Settings.Remove(Constants.ManagementPort);
            //    this.Config.AppSettings.Settings.Add(
            //        new KeyValueConfigurationElement(Constants.ManagementPort, this.ManagementPort));
            //    this.ConfigurationChanged = true;
            //}
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            //if (this.RemoveOldPortUrlReservation)
            //{
            //    Collection<PSObject> results =
            //        ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture,
            //            "netsh http delete urlacl url=http://+:{0}/", this.oldPort)).Invoke();
            //    this.WriteObject(results, true);
            //}
        }

        #endregion Overrides
    }
}