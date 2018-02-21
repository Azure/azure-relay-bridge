// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System.Drawing;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.DataLayer;
    using Microsoft.HybridConnectionManager;

    public class HybridConnectionIbizaItem : IbizaStyleTableItem
    {
        public HybridConnectionIbizaItem(Panel parent) : base(parent)
        {
        }

        public HybridConnectionCacheEntity HybridConnection { get; set; }

        public override void Initialize(TableColumn[] columns)
        {
            // We will create a bunch of fields here relevant to our input.

            // relay name field
            Fields.Add("name", new TableField()
            {
                EnabledColor = Color.Black,
                DisabledColor = Color.Gray,
                Text = HybridConnection.RelayName
            });

            Fields.Add("azurestatus", new TableField()
            {
                EnabledColor = HybridConnection.GetStateTextColor(),
                DisabledColor = HybridConnection.GetStateTextColor(),
                Text = HybridConnection.GetStateText()
            });

            Fields.Add("servicetype", new TableField()
            {
                EnabledColor = Color.Black,
                DisabledColor = Color.Gray,
                Text = "Relay"
            });

            Fields.Add("servicename", new TableField()
            {
                EnabledColor = Color.Black,
                DisabledColor = Color.Gray,
                Text = HybridConnection.GetNamespaceDisplayName()
            });

            Fields.Add("endpoint", new TableField()
            {
                EnabledColor = Color.Black,
                DisabledColor = Color.Gray,
                Text = HybridConnection.Endpoint
            });

            Fields.Add("region", new TableField()
            {
                EnabledColor = Color.Black,
                DisabledColor = Color.Gray,
                Text = HybridConnection.Region
            });

            TextFont = new Font("Calibri", 10, FontStyle.Regular);
            Hoverable = true;

            // Now initialize the element, this will create the fields
            base.Initialize(columns);
        }
    }
}