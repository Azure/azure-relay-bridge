// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System.Globalization;
    using System.Management.Automation;
    using Microsoft.HybridConnectionManager.Configuration;

    [Cmdlet(VerbsData.Update, Constants.HybridConnectionNounName,
        DefaultParameterSetName = Constants.HybridConnectionManagerDefaultParameterSetName)]
    public class UpdateHybridConnection : HybridConnectionBaseCmdlet
    {
        ConnectionTarget hybridConnectionElement;

        #region Parameters

        [Parameter(Mandatory = true,
            ParameterSetName = Constants.HybridConnectionManagerDefaultParameterSetName)]
        public string ConnectionString { get; set; }

        #endregion Parameters

        #region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            //this.hybridConnectionElement = this.GetHybridConnectionElement(this.ConnectionString);
            //if (this.hybridConnectionElement == null)
            //{
            //    var exception = new PSArgumentException(
            //        string.Format(CultureInfo.CurrentCulture, Strings.UnableToFindConfigEntry, this.ConnectionString));
            //    this.ThrowTerminatingError(new ErrorRecord(
            //        exception,
            //        string.Empty,
            //        ErrorCategory.InvalidArgument,
            //        null));
            //    throw exception;
            //}
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            //this.HybridConnectionsSection.Targets.Remove(
            //    this.GetHybridConnectionElementKey(this.ConnectionString));

            //var hybridConnection = new ConnectionTarget(this.ConnectionString, "", 0);
            //this.HybridConnectionsSection.Targets.Add(hybridConnection);
            //this.ConfigurationChanged = true;
        }

        #endregion Overrides
    }
}