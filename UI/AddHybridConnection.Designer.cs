// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    partial class AddHybridConnection
    {
        CustomButton cancelButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        CustomButton enterManuallyButton;

        IbizaStyleTable hybridConnectionsTable;

        System.Windows.Forms.Label label1;

        System.Windows.Forms.Label label2;

        System.Windows.Forms.PictureBox newLoadingGif;

        System.Windows.Forms.PictureBox pictureBox1;

        CustomButton saveButton;

        System.Windows.Forms.PictureBox savingGif;

        System.Windows.Forms.Label savingText;

        System.Windows.Forms.ComboBox subscriptionBox;

        System.Windows.Forms.PictureBox subscriptionLoadingGif;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(AddHybridConnection));
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.subscriptionBox = new System.Windows.Forms.ComboBox();
            this.subscriptionLoadingGif = new System.Windows.Forms.PictureBox();
            this.savingGif = new System.Windows.Forms.PictureBox();
            this.savingText = new System.Windows.Forms.Label();
            this.saveButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.cancelButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.hybridConnectionsTable = new HybridConnectionManagerIbizaUi.IbizaStyleTable();
            this.newLoadingGif = new System.Windows.Forms.PictureBox();
            this.enterManuallyButton = new HybridConnectionManagerIbizaUi.CustomButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subscriptionLoadingGif)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.savingGif)).BeginInit();
            this.hybridConnectionsTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.newLoadingGif)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(42, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Configure New Hybrid Connections";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage =
                ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(23, 23);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "Subscription";
            // 
            // subscriptionBox
            // 
            this.subscriptionBox.Items.AddRange(new object[]
            {
                "Loading Subscriptions..."
            });
            this.subscriptionBox.Location = new System.Drawing.Point(93, 45);
            this.subscriptionBox.Name = "subscriptionBox";
            this.subscriptionBox.Size = new System.Drawing.Size(391, 21);
            this.subscriptionBox.TabIndex = 9;
            this.subscriptionBox.SelectedIndexChanged +=
                new System.EventHandler(this.SubscriptionBox_SelectedIndexChanged);
            // 
            // subscriptionLoadingGif
            // 
            this.subscriptionLoadingGif.Image =
                ((System.Drawing.Image)(resources.GetObject("subscriptionLoadingGif.Image")));
            this.subscriptionLoadingGif.Location = new System.Drawing.Point(490, 48);
            this.subscriptionLoadingGif.Name = "subscriptionLoadingGif";
            this.subscriptionLoadingGif.Size = new System.Drawing.Size(17, 15);
            this.subscriptionLoadingGif.TabIndex = 11;
            this.subscriptionLoadingGif.TabStop = false;
            // 
            // savingGif
            // 
            this.savingGif.Image = ((System.Drawing.Image)(resources.GetObject("savingGif.Image")));
            this.savingGif.Location = new System.Drawing.Point(161, 396);
            this.savingGif.Name = "savingGif";
            this.savingGif.Size = new System.Drawing.Size(18, 25);
            this.savingGif.TabIndex = 13;
            this.savingGif.TabStop = false;
            // 
            // savingText
            // 
            this.savingText.AutoSize = true;
            this.savingText.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.savingText.Location = new System.Drawing.Point(180, 396);
            this.savingText.Name = "savingText";
            this.savingText.Size = new System.Drawing.Size(121, 15);
            this.savingText.TabIndex = 14;
            this.savingText.Text = "Saving... Please Wait";
            // 
            // saveButton
            // 
            this.saveButton.ButtonText = "Save";
            this.saveButton.ClickImage = null;
            this.saveButton.DeactivatedImage =
                ((System.Drawing.Image)(resources.GetObject("saveButton.DeactivatedImage")));
            this.saveButton.DisabledTextColor = System.Drawing.Color.Black;
            this.saveButton.EnabledTextColor = System.Drawing.Color.White;
            this.saveButton.HoverImage = null;
            this.saveButton.Location = new System.Drawing.Point(32, 389);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(53, 27);
            this.saveButton.TabIndex = 6;
            this.saveButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveButton.TextPosition = new System.Drawing.Point(0, 0);
            this.saveButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("saveButton.UnselectedImage")));
            // 
            // cancelButton
            // 
            this.cancelButton.ButtonText = "Cancel";
            this.cancelButton.ClickImage = null;
            this.cancelButton.DeactivatedImage =
                ((System.Drawing.Image)(resources.GetObject("cancelButton.DeactivatedImage")));
            this.cancelButton.DisabledTextColor = System.Drawing.Color.Black;
            this.cancelButton.EnabledTextColor = System.Drawing.Color.White;
            this.cancelButton.HoverImage = null;
            this.cancelButton.Location = new System.Drawing.Point(100, 389);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(53, 27);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.TextPosition = new System.Drawing.Point(0, 0);
            this.cancelButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("cancelButton.UnselectedImage")));
            // 
            // hybridConnectionsTable
            // 
            this.hybridConnectionsTable.AddNewButtonItem = null;
            this.hybridConnectionsTable.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.hybridConnectionsTable.AutoScroll = true;
            this.hybridConnectionsTable.Columns = null;
            this.hybridConnectionsTable.Controls.Add(this.newLoadingGif);
            this.hybridConnectionsTable.Location = new System.Drawing.Point(12, 75);
            this.hybridConnectionsTable.Name = "hybridConnectionsTable";
            this.hybridConnectionsTable.SelectedIndex = 0;
            this.hybridConnectionsTable.Size = new System.Drawing.Size(738, 308);
            this.hybridConnectionsTable.TabIndex = 12;
            this.hybridConnectionsTable.TableHeaderItem = null;
            // 
            // newLoadingGif
            // 
            this.newLoadingGif.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                       | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.newLoadingGif.Cursor = System.Windows.Forms.Cursors.Default;
            this.newLoadingGif.Image = ((System.Drawing.Image)(resources.GetObject("newLoadingGif.Image")));
            this.newLoadingGif.Location = new System.Drawing.Point(351, 145);
            this.newLoadingGif.Name = "newLoadingGif";
            this.newLoadingGif.Size = new System.Drawing.Size(33, 34);
            this.newLoadingGif.TabIndex = 0;
            this.newLoadingGif.TabStop = false;
            // 
            // enterManuallyButton
            // 
            this.enterManuallyButton.ButtonText = "Enter Manually";
            this.enterManuallyButton.ClickImage = null;
            this.enterManuallyButton.DeactivatedImage = null;
            this.enterManuallyButton.DisabledTextColor = System.Drawing.Color.Black;
            this.enterManuallyButton.EnabledTextColor = System.Drawing.Color.White;
            this.enterManuallyButton.HoverImage = null;
            this.enterManuallyButton.Location = new System.Drawing.Point(571, 387);
            this.enterManuallyButton.Name = "enterManuallyButton";
            this.enterManuallyButton.Size = new System.Drawing.Size(130, 27);
            this.enterManuallyButton.TabIndex = 15;
            this.enterManuallyButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enterManuallyButton.TextPosition = new System.Drawing.Point(0, 0);
            this.enterManuallyButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("enterManuallyButton.UnselectedImage")));

            // 
            // AddHybridConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(762, 426);
            this.Controls.Add(this.enterManuallyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.savingText);
            this.Controls.Add(this.savingGif);
            this.Controls.Add(this.hybridConnectionsTable);
            this.Controls.Add(this.subscriptionLoadingGif);
            this.Controls.Add(this.subscriptionBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AddHybridConnection";
            this.Load += new System.EventHandler(this.AddHybridConnection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subscriptionLoadingGif)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.savingGif)).EndInit();
            this.hybridConnectionsTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.newLoadingGif)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}