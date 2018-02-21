// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.DataLayer;
    using Microsoft.HybridConnectionManager.Commands;

    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

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
        }

        async void About_Load(object sender, EventArgs e)
        {
            currentVersionText.Text = string.Format("Version: {0}",
                HybridConnectionConstants.HybridConnectionManagerVersion);

            moreInfoLink.Links.Add(0, moreInfoLink.Text.Length, "https://go.microsoft.com/fwlink/?linkid=841334");
            moreInfoLink.LinkBehavior = LinkBehavior.NeverUnderline;

            downloadUpdateLink.Links.Add(0, downloadUpdateLink.Text.Length,
                "https://go.microsoft.com/fwlink/?linkid=864322");
            downloadUpdateLink.LinkBehavior = LinkBehavior.NeverUnderline;

            updateAvailableText.Hide();
            downloadUpdateLink.Hide();

            closeButton.OnClicked += CloseButton_OnClicked;

            if (await HybridConnectionDataManager.IsUpdateAvailable())
            {
                updateAvailableText.Show();
                downloadUpdateLink.Show();
            }
        }

        void CheckForUpdatesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        void CloseButton_OnClicked(object sender, EventArgs e)
        {
            this.Hide();
        }

        void DownloadUpdateLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        void Label1_Click(object sender, EventArgs e)
        {
        }

        void MoreInfoLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }
}