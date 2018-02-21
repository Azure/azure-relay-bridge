// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System.Drawing;
    using System.Windows.Forms;

    public class IbizaStyleTableHeader : IbizaStyleTableItem
    {
        public IbizaStyleTableHeader(Panel parent) : base(parent)
        {
        }

        public override void Initialize(TableColumn[] columns)
        {
            base.Initialize(columns);

            foreach (var column in columns)
            {
                AddLabel(column.PositionPercent, column.PositionPixels, 0, column.Title, Color.Black, Color.Black);
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }
    }
}