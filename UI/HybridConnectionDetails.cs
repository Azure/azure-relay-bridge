// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.DataLayer;

    public partial class HybridConnectionDetails : Form
    {
        float defaultWidth;

        ConfirmBox removeConfirmBox;

        public HybridConnectionDetails()
        {
            InitializeComponent();

            this.closeButton.OnClicked += CloseButton_OnClicked;
            this.closeBlueButton.OnClicked += CloseButton_OnClicked;

            this.MouseDown += HybridConnectionDetails_MouseDown;
            this.Shown += HybridConnectionDetails_Shown;

            hybridConnectionDetailsTable.Columns = new TableColumn[]
            {
                new TableColumn()
                {
                    FieldName = "detailName",
                    Title = "unused",
                    PositionPercent = 0.03f
                },
                new TableColumn()
                {
                    FieldName = "detailValue",
                    Title = "unused",
                    PositionPixels = 160
                }
            };

            hybridConnectionDetailsTable.Initialize(showHeader: false);

            defaultWidth = this.Size.Width;
        }

        public event EventHandler OnDeleted;

        public HybridConnectionCacheEntity HybridConnection { get; set; }

        public HybridConnectionIbizaItem TableItem { get; set; }

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
            e.Graphics.DrawLine(lightSilverPen, new Point(ClientRectangle.Left + 3, ClientRectangle.Top + 60),
                new Point(ClientRectangle.Right - 3, ClientRectangle.Top + 60));
        }

        async void _removeConfirmBox_YesClicked(object sender, EventArgs e)
        {
            removingText.Show();
            removingGif.Show();
            closeBlueButton.Deactivate();

            // Remove this connection
            HybridConnectionDataManager.RemoveHybridConnection(HybridConnection);

            // Wait 2 seconds then refresh the view and close.
            await Task.Delay(2000);

            await Main.MainForm.LoadConfiguredHybridConnections();

            // And then close this view and report that it is closed.
            Hide();

            if (OnDeleted != null)
            {
                OnDeleted(this, EventArgs.Empty);
            }
        }

        void CloseButton_OnClicked(object sender, EventArgs e)
        {
            this.Hide();

            TableItem.Selected = false;
        }

        void CustomButton1_Click(object sender, EventArgs e)
        {
        }

        void HybridConnectionDetails_Load(object sender, EventArgs e)
        {
        }

        void HybridConnectionDetails_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Main.ReleaseCapture();
                Main.SendMessage(Handle, Main.WmNclbuttondown, Main.HtCaption, 0);
            }
        }

        async void HybridConnectionDetails_Shown(object sender, EventArgs e)
        {
            removingGif.Hide();
            removingText.Hide();

            if (HybridConnection != null)
            {
                hybridConnectionDetailsTable.ClearItems();
                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Name",
                        DetailValue = HybridConnection.RelayName
                    });

                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Namespace",
                        DetailValue = HybridConnection.GetNamespaceDisplayName()
                    });

                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Endpoint",
                        DetailValue = HybridConnection.Endpoint
                    });

                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Status",
                        DetailValue = HybridConnection.GetStateText(),
                        TextColor = HybridConnection.GetStateTextColor()
                    });

                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Service Bus Endpoint",
                        DetailValue = HybridConnection.ServiceBusEndpoint
                    });

                // We need to resize window according to the biggest element - the service bus endpoint
                // Calculate the size of the label now
                using (Graphics g = CreateGraphics())
                {
                    SizeF size = g.MeasureString(HybridConnection.ServiceBusEndpoint,
                        hybridConnectionDetailsTable.Items.Last().TextFont, 495);
                    this.Size = new Size(200 + (int)Math.Max(size.Width, defaultWidth), this.Size.Height);
                }

                IPAddress[] addresses;
                StringBuilder addressesString = new StringBuilder();
                try
                {
                    addresses = await Dns.GetHostAddressesAsync(HybridConnection.ServiceBusEndpoint);

                    for (int i = 0; i < addresses.Length; i++)
                    {
                        addressesString.Append(addresses[i]);
                        if (i != addresses.Length - 1)
                        {
                            addressesString.Append(",");
                        }
                    }
                }
                catch (Exception)
                {
                    addressesString.Append("Name could not be resolved.");
                }

                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Azure IP Address",
                        DetailValue = addressesString.ToString()
                    });

                    hybridConnectionDetailsTable.AddItem(
                        new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                        {
                            DetailName = "Azure Ports",
                            DetailValue = "80, 443"
                        });
                
                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Created On",
                        DetailValue =
                            HybridConnection.CreatedDate != DateTime.MinValue
                                ? HybridConnection.CreatedDate.ToString()
                                : string.Empty
                    });

                hybridConnectionDetailsTable.AddItem(
                    new HybridConnectionDetailItem(hybridConnectionDetailsTable)
                    {
                        DetailName = "Last Updated",
                        DetailValue =
                            HybridConnection.LastUpdatedDate != DateTime.MinValue
                                ? HybridConnection.LastUpdatedDate.ToString()
                                : string.Empty
                    });
            }
        }

        void RemoveMenuButton_Click(object sender, EventArgs e)
        {
            removeConfirmBox = new ConfirmBox()
            {
                ConfirmText =
                    string.Format("Are you sure you want to remove the connection\n{0}", HybridConnection.RelayName)
            };

            removeConfirmBox.YesClicked += _removeConfirmBox_YesClicked;

            removeConfirmBox.Show();
            removeConfirmBox.Location = new Point(Location.X + (Width / 2) - (removeConfirmBox.Width / 2),
                Location.Y + (Height / 2) - (removeConfirmBox.Height / 2));
        }
    }
}