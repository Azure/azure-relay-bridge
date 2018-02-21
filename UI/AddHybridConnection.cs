// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using HybridConnectionManagerIbizaUi.ArmClient;
    using HybridConnectionManagerIbizaUi.DataLayer;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public partial class AddHybridConnection : Form
    {
        List<string> subscriptions;

        public AddHybridConnection()
        {
            InitializeComponent();

            this.MouseDown += AddHybridConnection_MouseDown;
        }

        public async void ShowForm()
        {
            this.Show();

            subscriptionLoadingGif.Show();

            IEnumerable<Subscription> armSubscriptions;
            try
            {
                armSubscriptions = await HybridConnectionDataManager.GetSubscriptions();
            }
            catch (AdalException)
            {
                Hide();
                return;
            }

            subscriptionLoadingGif.Hide();

            subscriptionBox.Items.Clear();
            subscriptions.Clear();

            // Keep a placeholder here since this is where the "Choose a subscription..." option will be.
            subscriptions.Add("unused");
            subscriptionBox.Items.Add("Choose a subscription...");

            foreach (var subscription in armSubscriptions)
            {
                subscriptionBox.Items.Add(subscription.DisplayName);
                subscriptions.Add(subscription.Id);
            }

            subscriptionBox.SelectedIndex = 0;
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            hybridConnectionsTable.Columns = new TableColumn[]
            {
                new TableColumn()
                {
                    FieldName = "name",
                    Title = "NAME",
                    PositionPercent = 0.03f
                },
                new TableColumn()
                {
                    FieldName = "region",
                    Title = "REGION",
                    PositionPercent = 0.25f
                },
                new TableColumn()
                {
                    FieldName = "servicetype",
                    Title = "SERVICE TYPE",
                    PositionPercent = 0.375f
                },
                new TableColumn()
                {
                    FieldName = "servicename",
                    Title = "SERVICE NAME",
                    PositionPercent = 0.525f
                },
                new TableColumn()
                {
                    FieldName = "endpoint",
                    Title = "ENDPOINT",
                    PositionPercent = 0.75f
                }
            };
            hybridConnectionsTable.Initialize();

            subscriptions = new List<string>();
            subscriptions.Add("unused"); // Id 0 is not used.

            saveButton.OnClicked += SaveButton_OnClicked;
            cancelButton.OnClicked += CancelButton_OnClicked;

            saveButton.Deactivate();

            subscriptionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            subscriptionBox.SelectedIndex = 0;
            HideLoading();
            subscriptionLoadingGif.Hide();

            savingGif.Hide();
            savingText.Hide();

            subscriptionBox.Items.Clear();
            subscriptionBox.Items.Add("Waiting for login...");
            subscriptionBox.SelectedIndex = 0;
        }

        void AddHybridConnection_Load(object sender, EventArgs e)
        {
        }

        void AddHybridConnection_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Main.ReleaseCapture();
                Main.SendMessage(Handle, Main.WmNclbuttondown, Main.HtCaption, 0);
            }
        }

        void CancelButton_OnClicked(object sender, EventArgs e)
        {
            this.Hide();
        }

        void DisplayRelays(IEnumerable<HybridConnectionCacheEntity> relays)
        {
            hybridConnectionsTable.ClearItems();
            HideLoading();

            foreach (var relay in relays)
            {
                var item = new HybridConnectionIbizaItem(hybridConnectionsTable)
                {
                    Selectable = true,
                    Selected = false,
                    Disabled =
                        HybridConnectionDataManager.IsHybridConnectionAlreadyConfigured(relay.Namespace, relay.RelayName) ||
                        !relay.Enableable,
                    HybridConnection = relay
                };

                item.OnSelected += HybridConnectionSelected;
                item.OnUnselected += HybridConnectionSelected;
                hybridConnectionsTable.AddItem(item);
            }
        }

        void HideLoading()
        {
            this.newLoadingGif.Hide();
        }

        void HybridConnectionSelected(object sender, EventArgs e)
        {
            UpdateSaveButtonState();
        }

        async void LoadRelaysForSubscription(int index, string subscriptionId)
        {
            ShowLoading();
            subscriptionBox.Enabled = false;

            var relays = await HybridConnectionDataManager.GetAllHybridConnectionsFromArmOrCache(subscriptionId);

            subscriptionBox.Enabled = true;

            // If relays is null, we might still be loading in another thread so just return
            if (relays == null)
            {
                return;
            }

            HideLoading();

            // Double check that we are still loading relays for this subscription
            if (subscriptionBox.SelectedIndex == index)
            {
                DisplayRelays(relays);
            }
        }

        async void SaveButton_OnClicked(object sender, EventArgs e)
        {
            // Get all of the selected hybrid connections
            List<HybridConnectionCacheEntity> selectedItems = new List<HybridConnectionCacheEntity>();

            foreach (var item in hybridConnectionsTable.Items)
            {
                if (item.Selected && item is HybridConnectionIbizaItem)
                {
                    selectedItems.Add((item as HybridConnectionIbizaItem).HybridConnection);
                }
            }

            // Disable save button, cancel button, and all items
            saveButton.Deactivate();
            cancelButton.Deactivate();
            foreach (var item in hybridConnectionsTable.Items)
            {
                item.Disabled = true;
            }

            savingGif.Show();
            savingText.Show();

            await HybridConnectionDataManager.ConfigureNewHybridConnections(selectedItems);

            savingGif.Hide();
            savingText.Hide();

            cancelButton.Activate();

            hybridConnectionsTable.ClearItems();
            subscriptionBox.SelectedIndex = 0;

            this.Hide();
        }

        void ShowLoading()
        {
            hybridConnectionsTable.ClearItems();
            this.newLoadingGif.Show();
            this.newLoadingGif.BringToFront();
        }

        void SubscriptionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = subscriptionBox.SelectedIndex;
            if (index != 0)
            {
                if (subscriptions.Count > index)
                {
                    var subscriptionId = subscriptions[index];
                    if (!string.IsNullOrEmpty(subscriptionId))
                    {
                        LoadRelaysForSubscription(index, subscriptionId);
                    }
                }
            }
        }

        void UpdateSaveButtonState()
        {
            // Check if there is a selected item in the hybrid connection list
            bool selected = false;
            foreach (var item in hybridConnectionsTable.Items)
            {
                if (item.Selected)
                {
                    selected = true;
                    break;
                }
            }

            if (selected)
            {
                saveButton.Activate();
            }
            else
            {
                saveButton.Deactivate();
            }
        }
    }
}