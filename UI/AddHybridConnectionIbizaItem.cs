// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System.Drawing;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.Properties;

    public class AddHybridConnectionIbizaItem : IbizaStyleTableItem
    {
        public AddHybridConnectionIbizaItem(Panel parent) : base(parent)
        {
        }

        public override void Initialize(TableColumn[] columns)
        {
            base.Initialize(columns);

            this.Size = new Size(808, 37);

            this.Image = Resources.AddNewButton;

            AddLabel(0.06f, 0, 0.25f, "Add a new Hybrid Connection", Color.FromArgb(255, 0, 191, 248),
                Color.FromArgb(255, 0, 191, 248));

            Hoverable = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Image != null)
            {
                e.Graphics.DrawImage(Image, new Rectangle(new Point(10, 5), ImageSize));
            }
        }
    }
}