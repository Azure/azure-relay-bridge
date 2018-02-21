// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    partial class Main
    {
        CustomButton closeButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        IbizaStyleTable configuredConnectionsTable;

        CustomButton enterManuallyButton;

        System.Windows.Forms.Label label1;

        System.Windows.Forms.Label label2;

        System.Windows.Forms.PictureBox loadingGif;

        CustomButton minimizeButton;

        System.Windows.Forms.PictureBox oldVersionWarningIcon;

        System.Windows.Forms.Label oldVersionWarningLabel;

        System.Windows.Forms.PictureBox pictureBox1;

        CustomButton refreshButton;

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
                new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.oldVersionWarningIcon = new System.Windows.Forms.PictureBox();
            this.oldVersionWarningLabel = new System.Windows.Forms.Label();
            this.closeButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.minimizeButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.refreshButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.configuredConnectionsTable = new HybridConnectionManagerIbizaUi.IbizaStyleTable();
            this.loadingGif = new System.Windows.Forms.PictureBox();
            this.enterManuallyButton = new HybridConnectionManagerIbizaUi.CustomButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.oldVersionWarningIcon)).BeginInit();
            this.configuredConnectionsTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadingGif)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage =
                ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(23, 23);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(42, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Hybrid Connection Manager";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "About";
            this.label2.Click += new System.EventHandler(this.Label2_Click);
            // 
            // oldVersionWarningIcon
            // 
            this.oldVersionWarningIcon.Location = new System.Drawing.Point(23, 467);
            this.oldVersionWarningIcon.Name = "oldVersionWarningIcon";
            this.oldVersionWarningIcon.Size = new System.Drawing.Size(47, 43);
            this.oldVersionWarningIcon.TabIndex = 9;
            this.oldVersionWarningIcon.TabStop = false;
            this.oldVersionWarningIcon.Click += new System.EventHandler(this.PictureBox2_Click_1);
            // 
            // oldVersionWarningLabel
            // 
            this.oldVersionWarningLabel.AutoSize = true;
            this.oldVersionWarningLabel.Location = new System.Drawing.Point(58, 472);
            this.oldVersionWarningLabel.Name = "oldVersionWarningLabel";
            this.oldVersionWarningLabel.Size = new System.Drawing.Size(550, 26);
            this.oldVersionWarningLabel.TabIndex = 10;
            this.oldVersionWarningLabel.Text =
                "Your version of Windows does not support WebSockets. As such, only legacy Biztalk" +
                " Hybrid Connections will work.\r\nPlease upgrade to Windows 8+ or Windows Server 2" +
                "012+\r\n";
            // 
            // closeButton
            // 
            this.closeButton.ButtonText = null;
            this.closeButton.ClickImage = ((System.Drawing.Image)(resources.GetObject("closeButton.ClickImage")));
            this.closeButton.DeactivatedImage = null;
            this.closeButton.DisabledTextColor = System.Drawing.Color.Empty;
            this.closeButton.EnabledTextColor = System.Drawing.Color.Empty;
            this.closeButton.HoverImage = ((System.Drawing.Image)(resources.GetObject("closeButton.HoverImage")));
            this.closeButton.Location = new System.Drawing.Point(774, 4);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(55, 23);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "customButton1";
            this.closeButton.TextFont = null;
            this.closeButton.TextPosition = new System.Drawing.Point(0, 0);
            this.closeButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("closeButton.UnselectedImage")));
            // 
            // minimizeButton
            // 
            this.minimizeButton.ButtonText = null;
            this.minimizeButton.ClickImage = ((System.Drawing.Image)(resources.GetObject("minimizeButton.ClickImage")));
            this.minimizeButton.DeactivatedImage = null;
            this.minimizeButton.DisabledTextColor = System.Drawing.Color.Empty;
            this.minimizeButton.EnabledTextColor = System.Drawing.Color.Empty;
            this.minimizeButton.HoverImage = ((System.Drawing.Image)(resources.GetObject("minimizeButton.HoverImage")));
            this.minimizeButton.Location = new System.Drawing.Point(720, 4);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(56, 23);
            this.minimizeButton.TabIndex = 3;
            this.minimizeButton.Text = "customButton1";
            this.minimizeButton.TextFont = null;
            this.minimizeButton.TextPosition = new System.Drawing.Point(0, 0);
            this.minimizeButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("minimizeButton.UnselectedImage")));
            // 
            // refreshButton
            // 
            this.refreshButton.ButtonText = "Refresh";
            this.refreshButton.ClickImage = null;
            this.refreshButton.DeactivatedImage = null;
            this.refreshButton.DisabledTextColor = System.Drawing.Color.Empty;
            this.refreshButton.EnabledTextColor = System.Drawing.Color.White;
            this.refreshButton.HoverImage = null;
            this.refreshButton.Location = new System.Drawing.Point(746, 472);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(51, 27);
            this.refreshButton.TabIndex = 8;
            this.refreshButton.Text = "customButton1";
            this.refreshButton.TextFont = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refreshButton.TextPosition = new System.Drawing.Point(0, 0);
            this.refreshButton.UnselectedImage = global::HybridConnectionManagerIbizaUi.Properties.Resources.BlueButton;
            // 
            // configuredConnectionsTable
            // 
            this.configuredConnectionsTable.AddNewButtonItem = null;
            this.configuredConnectionsTable.AutoScroll = true;
            this.configuredConnectionsTable.Columns = null;
            this.configuredConnectionsTable.Controls.Add(this.loadingGif);
            this.configuredConnectionsTable.Location = new System.Drawing.Point(25, 73);
            this.configuredConnectionsTable.Name = "configuredConnectionsTable";
            this.configuredConnectionsTable.SelectedIndex = 0;
            this.configuredConnectionsTable.Size = new System.Drawing.Size(780, 383);
            this.configuredConnectionsTable.TabIndex = 7;
            this.configuredConnectionsTable.TableHeaderItem = null;
            // 
            // loadingGif
            // 
            this.loadingGif.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                       | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.loadingGif.Cursor = System.Windows.Forms.Cursors.Default;
            this.loadingGif.Image = ((System.Drawing.Image)(resources.GetObject("loadingGif.Image")));
            this.loadingGif.Location = new System.Drawing.Point(374, 216);
            this.loadingGif.Name = "loadingGif";
            this.loadingGif.Size = new System.Drawing.Size(33, 22);
            this.loadingGif.TabIndex = 1;
            this.loadingGif.TabStop = false;
            // 
            // enterManuallyButton
            // 
            this.enterManuallyButton.ButtonText = "Enter Manually";
            this.enterManuallyButton.ClickImage = null;
            this.enterManuallyButton.DeactivatedImage = null;
            this.enterManuallyButton.DisabledTextColor = System.Drawing.Color.Black;
            this.enterManuallyButton.EnabledTextColor = System.Drawing.Color.White;
            this.enterManuallyButton.HoverImage = null;
            this.enterManuallyButton.Location = new System.Drawing.Point(610, 472);
            this.enterManuallyButton.Name = "enterManuallyButton";
            this.enterManuallyButton.Size = new System.Drawing.Size(130, 27);
            this.enterManuallyButton.TabIndex = 16;
            this.enterManuallyButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enterManuallyButton.TextPosition = new System.Drawing.Point(0, 0);
            this.enterManuallyButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("enterManuallyButton.UnselectedImage")));
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(832, 517);
            this.Controls.Add(this.enterManuallyButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.minimizeButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.oldVersionWarningLabel);
            this.Controls.Add(this.oldVersionWarningIcon);
            this.Controls.Add(this.configuredConnectionsTable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Hybrid Connection Manager";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.oldVersionWarningIcon)).EndInit();
            this.configuredConnectionsTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.loadingGif)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}