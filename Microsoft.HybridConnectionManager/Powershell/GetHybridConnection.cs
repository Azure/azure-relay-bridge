// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Powershell
{
    using System.Management.Automation;
    using Microsoft.HybridConnectionManager.Configuration;

    [Cmdlet(VerbsCommon.Get, Constants.HybridConnectionNounName)]
    public class GetHybridConnection : HybridConnectionBaseCmdlet
    {
        #region Parameters

        [Parameter(Mandatory = false)]
        public string ConnectionString { get; set; }

        #endregion

        #region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            foreach (var connection in this.HybridConnectionsSection.HybridConnections)
            {
                if (ConnectionString == null ||
                    ConnectionString == ((TcpClientElement)connection).RelayConnectionString)
                {
                    WriteObject(connection);
                }
            }
        }

        #endregion Overrides
    }
}