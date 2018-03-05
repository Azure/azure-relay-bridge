// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System.Globalization;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Remove, Constants.HybridConnectionNounName)]
    public class RemoveHybridConnectionClient : HybridConnectionClientBaseCmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string ConnectionString { get; set; }

        #endregion Parameters

        #region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            //var hybridConnectionElement = this.GetHybridConnectionElement(this.ConnectionString);
            //if (hybridConnectionElement == null)
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

            //this.HybridConnectionsSection.HybridConnections.Remove(
            //    this.GetHybridConnectionElementKey(this.ConnectionString));
            //this.ConfigurationChanged = true;
        }

        #endregion Overrides
    }
}