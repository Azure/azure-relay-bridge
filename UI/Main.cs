// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.DataLayer;

    public partial class Main : Form
    {
        About aboutForm;

        MenuItem aboutMenuItem;

        static AddHybridConnection addHybridConnectionForm;

        Dictionary<string, HybridConnectionDetails> detailsForms;

        MenuItem helpMenuItem;

        MainMenu mainMenu;

        static ManualAddHybridConnection manualAddHybridConnectionForm;

        public Main()
        {
            detailsForms = new Dictionary<string, HybridConnectionDetails>();

            aboutForm = new About();
            addHybridConnectionForm = new AddHybridConnection();
            InitializeComponent();
            mainMenu = new MainMenu();
            this.Menu = mainMenu;

            helpMenuItem = new MenuItem("Help");
            helpMenuItem.Index = 0;
            aboutMenuItem = new MenuItem("About");
            aboutMenuItem.Index = 1;

            helpMenuItem.MenuItems.Add(aboutMenuItem);

            aboutMenuItem.Click += AboutMenuItem_Click;

            this.GotFocus += Main_GotFocus;
            this.MouseDown += Main_MouseDown;

            MainForm = this;

            configuredConnectionsTable.AddNewButtonItem = new AddHybridConnectionIbizaItem(configuredConnectionsTable);
            configuredConnectionsTable.AddNewButtonItem.OnClicked += AddNewButtonItem_OnClicked;
            configuredConnectionsTable.Columns = new TableColumn[]
            {
                new TableColumn()
                {
                    FieldName = "name",
                    Title = "NAME",
                    PositionPercent = 0.03f
                },
                new TableColumn()
                {
                    FieldName = "azurestatus",
                    Title = "AZURE STATUS",
                    PositionPercent = 0.25f
                },
                new TableColumn()
                {
                    FieldName = "servicetype",
                    Title = "SERVICE TYPE",
                    PositionPercent = 0.375f,
                },
                new TableColumn()
                {
                    FieldName = "servicename",
                    Title = "SERVICE NAME",
                    PositionPercent = 0.50f
                },
                new TableColumn()
                {
                    FieldName = "endpoint",
                    Title = "ENDPOINT",
                    PositionPercent = 0.75f
                }
            };
            configuredConnectionsTable.Initialize();

            refreshButton.OnClicked += RefreshButton_OnClicked;

            this.Activated += Main_Activated;

            manualAddHybridConnectionForm = new ManualAddHybridConnection();

            enterManuallyButton.OnClicked += EnterManuallyButton_OnClicked;
        }

        public static int HtCaption { get; } = 0x2;

        public static Main MainForm { get; set; }

        public static int WmNclbuttondown { get; } = 0xA1;

        public static void AddHybridConnection()
        {
            addHybridConnectionForm.Location = new Point(MainForm.Location.X + (MainForm.Width / 2) -
                                                         (addHybridConnectionForm.Width / 2),
                MainForm.Location.Y + (MainForm.Height / 2) - (addHybridConnectionForm.Height / 2));
            addHybridConnectionForm.ShowForm();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr windowHandle, int message, int wideParameter, int longParameter);

        public async Task LoadConfiguredHybridConnections()
        {
            loadingGif.Show();

            configuredConnectionsTable.ClearItems();

            var connections = await HybridConnectionDataManager.GetConfiguredHybridConnections();

            if (connections == null)
            {
                return;
            }

            loadingGif.Hide();

            foreach (var connection in connections)
            {
                var item = new HybridConnectionIbizaItem(configuredConnectionsTable)
                {
                    HybridConnection = connection,
                    Selectable = true
                };
                item.OnSelected += Item_OnSelected;
                item.OnUnselected += Item_OnUnselected;

                configuredConnectionsTable.AddItem(item);
            }
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
            e.Graphics.DrawLine(lightSilverPen, new Point(ClientRectangle.Left + 3, ClientRectangle.Top + 60),
                new Point(ClientRectangle.Right - 3, ClientRectangle.Top + 60));
        }

        void AboutMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutForm();
        }

        void AddNewButtonItem_OnClicked(EventArgs e)
        {
            AddHybridConnection();
        }

        void CloseButton_OnClicked(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void DetailsForm_OnDeleted(object sender, EventArgs e)
        {
            HybridConnectionDetails details = sender as HybridConnectionDetails;

            var key = HybridConnectionDataManager.GetHybridConnectionKey(details.HybridConnection.Namespace,
                details.HybridConnection.RelayName);

            if (detailsForms.ContainsKey(key))
            {
                detailsForms.Remove(key);
            }
        }

        void EnterManuallyButton_OnClicked(object sender, EventArgs e)
        {
            manualAddHybridConnectionForm.Show();
        }

        void HybridConnectionsListPanel_Paint(object sender, PaintEventArgs e)
        {
        }

        void Item_OnSelected(object sender, EventArgs e)
        {
            if (sender is HybridConnectionIbizaItem)
            {
                var item = sender as HybridConnectionIbizaItem;

                string hybridConnectionKey =
                    HybridConnectionDataManager.GetHybridConnectionKey(item.HybridConnection.Namespace,
                        item.HybridConnection.RelayName);

                HybridConnectionDetails detailsForm;
                if (!detailsForms.TryGetValue(hybridConnectionKey, out detailsForm))
                {
                    detailsForm = new HybridConnectionDetails();
                    detailsForm.OnDeleted += DetailsForm_OnDeleted;
                    detailsForms.Add(hybridConnectionKey, detailsForm);
                }

                detailsForm.HybridConnection = item.HybridConnection;
                detailsForm.TableItem = item;

                detailsForm.Show();
            }
        }

        void Item_OnUnselected(object sender, EventArgs e)
        {
            if (sender is HybridConnectionIbizaItem)
            {
                var item = sender as HybridConnectionIbizaItem;

                string hybridConnectionKey =
                    HybridConnectionDataManager.GetHybridConnectionKey(item.HybridConnection.Namespace,
                        item.HybridConnection.RelayName);

                HybridConnectionDetails detailsForm;
                if (detailsForms.TryGetValue(hybridConnectionKey, out detailsForm))
                {
                    detailsForm.Hide();
                }
            }
        }

        void Label2_Click(object sender, EventArgs e)
        {
            this.aboutForm.Show();
            aboutForm.Location = new Point(Location.X + (Width / 2) - (aboutForm.Width / 2),
                Location.Y + (Height / 2) - (aboutForm.Height / 2));
        }

        void Main_Activated(object sender, EventArgs e)
        {
            aboutForm.Hide();
        }

        void Main_GotFocus(object sender, EventArgs e)
        {
            aboutForm.Hide();
        }

        async void Main_Load(object sender, EventArgs e)
        {
            minimizeButton.OnClicked += MinimizeButton_OnClicked;
            closeButton.OnClicked += CloseButton_OnClicked;

            int majorVersion = Environment.OSVersion.Version.Major;
            int minorVersion = Environment.OSVersion.Version.Minor;

            if (majorVersion < 6 || (majorVersion == 6 && minorVersion < 2))
            {
                // Legacy OS. Display a warning
                oldVersionWarningIcon.Image = SystemIcons.Warning.ToBitmap();
                oldVersionWarningIcon.Show();
                oldVersionWarningLabel.Show();
            }
            else
            {
                oldVersionWarningIcon.Hide();
                oldVersionWarningLabel.Hide();
            }

            await LoadConfiguredHybridConnections();

            if (await HybridConnectionDataManager.IsUpdateAvailable())
            {
                ShowAboutForm();
            }
        }

        void Main_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
            }
        }

        void MinimizeButton_Click(object sender, EventArgs e)
        {
            // Unused, this is for regular click events - we are using a full click (with mouse up)
        }

        void MinimizeButton_OnClicked(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        void PictureBox2_Click(object sender, EventArgs e)
        {
        }

        void PictureBox2_Click_1(object sender, EventArgs e)
        {
        }

        async void RefreshButton_OnClicked(object sender, EventArgs e)
        {
            await LoadConfiguredHybridConnections();
        }

        void ShowAboutForm()
        {
            aboutForm.Show();
            aboutForm.Location = new Point(Location.X + (Width / 2) - (aboutForm.Width / 2),
                Location.Y + (Height / 2) - (aboutForm.Height / 2));
        }
    }
}