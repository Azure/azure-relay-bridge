// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.DataLayer;
    using Microsoft.Azure.Relay;
    using Microsoft.HybridConnectionManager;

    public partial class ManualAddHybridConnection : Form
    {
        public ManualAddHybridConnection()
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

            Pen lightSilverPen = new Pen(Color.FromArgb(255, 217, 217, 217));

            e.Graphics.DrawLine(lightSilverPen, new Point(ClientRectangle.Left + 3, ClientRectangle.Top + 30),
                new Point(ClientRectangle.Right - 3, ClientRectangle.Top + 30));
        }

        async void AddButton_OnClicked(object sender, EventArgs e)
        {
            addButton.Deactivate();
            savingGif.Show();
            savingText.Show();

            connectionStringTextBox.Enabled = false;

            try
            {
                await
                    HybridConnectionDataManager.ConfigureHybridConnectionFromConnectionString(
                        connectionStringTextBox.Text);
            }
            finally
            {
                connectionStringTextBox.Text = string.Empty;

                connectionStringTextBox.Enabled = true;

                savingGif.Hide();
                savingText.Hide();
                this.Hide();
            }
        }

        void CancelButton_OnClicked(object sender, EventArgs e)
        {
            // Clear the text box, hide the form
            connectionStringTextBox.Text = string.Empty;
            this.Hide();
        }

        void ConnectionStringTextBox_TextChanged(object sender, EventArgs e)
        {
            // Validate that we have a valid connection string. 
            var cb = new RelayConnectionStringBuilder(connectionStringTextBox.Text);
            if (connectionStringTextBox.Text != null && connectionStringTextBox.Enabled)
            {
                addButton.Activate();
            }
            else
            {
                addButton.Deactivate();
            }
        }

        void ManualAddHybridConnection_Load(object sender, EventArgs e)
        {
            MouseDown += ManualAddHybridConnection_MouseDown;

            addButton.Deactivate();
            addButton.OnClicked += AddButton_OnClicked;
            cancelButton.OnClicked += CancelButton_OnClicked;

            savingGif.Hide();
            savingText.Hide();

            connectionStringTextBox.TextChanged += ConnectionStringTextBox_TextChanged;
        }

        void ManualAddHybridConnection_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Main.ReleaseCapture();
                Main.SendMessage(Handle, Main.WmNclbuttondown, Main.HtCaption, 0);
            }
        }
    }
}