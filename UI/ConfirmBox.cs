// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class ConfirmBox : Form
    {
        public ConfirmBox()
        {
            InitializeComponent();
        }

        public delegate void YesNoDelegate(object sender, EventArgs e);

        public event YesNoDelegate NoClicked;

        public event YesNoDelegate YesClicked;

        public string ConfirmText { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Paint a blue border around this window with colors 65,113,156
            Pen bluePen = new Pen(Color.FromArgb(255, 65, 113, 156), 2);

            e.Graphics.DrawLine(bluePen, new Point(ClientRectangle.Left, ClientRectangle.Top + 1),
                new Point(ClientRectangle.Right, ClientRectangle.Top));
            e.Graphics.DrawLine(bluePen, new Point(ClientRectangle.Left + 1, ClientRectangle.Top),
                new Point(ClientRectangle.Left, ClientRectangle.Bottom));
            e.Graphics.DrawLine(bluePen, new Point(ClientRectangle.Left + 1, ClientRectangle.Bottom - 1),
                new Point(ClientRectangle.Right, ClientRectangle.Bottom - 1));
            e.Graphics.DrawLine(bluePen, new Point(ClientRectangle.Right - 1, ClientRectangle.Top),
                new Point(ClientRectangle.Right - 1, ClientRectangle.Bottom));

            Pen lightSilverPen = new Pen(Color.FromArgb(255, 217, 217, 217));

            e.Graphics.DrawLine(lightSilverPen, new Point(ClientRectangle.Left + 3, ClientRectangle.Top + 30),
                new Point(ClientRectangle.Right - 3, ClientRectangle.Top + 30));
        }

        void ConfirmBox_Load(object sender, EventArgs e)
        {
            this.MouseDown += HybridConnectionDetails_MouseDown;

            confirmText.Text = ConfirmText;
            confirmText.TextAlign = ContentAlignment.MiddleCenter;
            confirmText.Location = new Point((this.Width / 2) - (confirmText.PreferredWidth / 2), confirmText.Location.Y);
            confirmText.BringToFront();

            yesButton.OnClicked += YesButton_OnClicked;
            noButton.OnClicked += NoButton_OnClicked;
        }

        void HybridConnectionDetails_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Main.ReleaseCapture();
                Main.SendMessage(Handle, Main.WmNclbuttondown, Main.HtCaption, 0);
            }
        }

        void NoButton_OnClicked(object sender, EventArgs e)
        {
            if (NoClicked != null)
            {
                NoClicked(this, EventArgs.Empty);
            }

            this.Hide();
        }

        void YesButton_OnClicked(object sender, EventArgs e)
        {
            if (YesClicked != null)
            {
                YesClicked(this, EventArgs.Empty);
            }

            this.Hide();
        }
    }
}