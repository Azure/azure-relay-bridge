// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System.Drawing;
    using System.Windows.Forms;

    public class HybridConnectionDetailItem : IbizaStyleTableItem
    {
        public HybridConnectionDetailItem(Panel parent) : base(parent)
        {
            TextColor = Color.Black;
        }

        public string DetailName { get; set; }

        public string DetailValue { get; set; }

        public Color TextColor { get; set; }

        public override void Initialize(TableColumn[] columns)
        {
            // We will create a bunch of fields here relevant to our input.

            // relay name field
            Fields.Add("detailName", new TableField()
            {
                EnabledColor = Color.Black,
                DisabledColor = Color.Gray,
                Text = DetailName
            });

            Fields.Add("detailValue", new TableField()
            {
                EnabledColor = TextColor,
                DisabledColor = TextColor,
                Text = DetailValue
            });

            TextFont = new Font("Calibri", 10, FontStyle.Regular);
            Hoverable = true;

            // Now initialize the element, this will create the fields
            base.Initialize(columns);
        }
    }
}