// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    public class IbizaStyleTable : Panel
    {
        public IbizaStyleTable()
        {
            AutoScroll = true;
            BorderStyle = BorderStyle.None;
        }

        public IbizaStyleTableItem AddNewButtonItem { get; set; }

        public TableColumn[] Columns { get; set; }

        public List<IbizaStyleTableItem> Items { get; } = new List<IbizaStyleTableItem>();

        public int SelectedIndex { get; set; }

        public IbizaStyleTableItem TableHeaderItem { get; set; }

        public void AddItem(IbizaStyleTableItem item)
        {
            item.Initialize(Columns);

            item.Index = Items.Count;
            Items.Add(item);
            Controls.Add(item);
            item.BringToFront();
            item.Click += ItemClicked;
        }

        public void ClearItems()
        {
            foreach (var item in Items)
            {
                Controls.Remove(item);
            }

            Items.Clear();

            // Re-add our special items
            if (TableHeaderItem != null)
            {
                AddItem(TableHeaderItem);
            }

            if (AddNewButtonItem != null)
            {
                AddItem(AddNewButtonItem);
            }
        }

        public void Initialize(bool showHeader = true)
        {
            if (showHeader)
            {
                TableHeaderItem = new IbizaStyleTableHeader(this);
                TableHeaderItem.TextFont = new Font("Arial", 9.0f, FontStyle.Bold);
                TableHeaderItem.Initialize(this.Columns);
            }

            // Add the table header and add new button if needed, for first time
            if (TableHeaderItem != null)
            {
                AddItem(TableHeaderItem);
            }

            if (AddNewButtonItem != null)
            {
                AddNewButtonItem.Initialize(this.Columns);
                AddItem(AddNewButtonItem);
            }
        }

        void ItemClicked(object sender, EventArgs e)
        {
        }
    }
}