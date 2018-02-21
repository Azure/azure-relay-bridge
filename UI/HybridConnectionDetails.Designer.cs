// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    partial class HybridConnectionDetails
    {
        CustomButton closeBlueButton;

        CustomButton closeButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        IbizaStyleTable hybridConnectionDetailsTable;

        System.Windows.Forms.Label label1;

        System.Windows.Forms.PictureBox pictureBox1;

        System.Windows.Forms.Label removeMenuButton;

        System.Windows.Forms.PictureBox removingGif;

        System.Windows.Forms.Label removingText;

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
                new System.ComponentModel.ComponentResourceManager(typeof(HybridConnectionDetails));
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.closeBlueButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.closeButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.hybridConnectionDetailsTable = new HybridConnectionManagerIbizaUi.IbizaStyleTable();
            this.removeMenuButton = new System.Windows.Forms.Label();
            this.removingText = new System.Windows.Forms.Label();
            this.removingGif = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.removingGif)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(42, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Hybrid Connection Details";
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
            // closeBlueButton
            // 
            this.closeBlueButton.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBlueButton.ButtonText = "Close";
            this.closeBlueButton.ClickImage = null;
            this.closeBlueButton.DeactivatedImage =
                global::HybridConnectionManagerIbizaUi.Properties.Resources.BlueButton_Disabled;
            this.closeBlueButton.DisabledTextColor = System.Drawing.Color.Black;
            this.closeBlueButton.EnabledTextColor = System.Drawing.Color.White;
            this.closeBlueButton.HoverImage = null;
            this.closeBlueButton.Location = new System.Drawing.Point(457, 362);
            this.closeBlueButton.Name = "closeBlueButton";
            this.closeBlueButton.Size = new System.Drawing.Size(53, 28);
            this.closeBlueButton.TabIndex = 7;
            this.closeBlueButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeBlueButton.TextPosition = new System.Drawing.Point(10, 5);
            this.closeBlueButton.UnselectedImage =
                global::HybridConnectionManagerIbizaUi.Properties.Resources.BlueButton;
            // 
            // closeButton
            // 
            this.closeButton.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.ButtonText = null;
            this.closeButton.ClickImage = ((System.Drawing.Image)(resources.GetObject("closeButton.ClickImage")));
            this.closeButton.DeactivatedImage =
                ((System.Drawing.Image)(resources.GetObject("closeButton.DeactivatedImage")));
            this.closeButton.DisabledTextColor = System.Drawing.Color.Empty;
            this.closeButton.EnabledTextColor = System.Drawing.Color.Empty;
            this.closeButton.HoverImage = ((System.Drawing.Image)(resources.GetObject("closeButton.HoverImage")));
            this.closeButton.Location = new System.Drawing.Point(479, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(52, 25);
            this.closeButton.TabIndex = 6;
            this.closeButton.TextFont = null;
            this.closeButton.TextPosition = new System.Drawing.Point(0, 0);
            this.closeButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("closeButton.UnselectedImage")));
            this.closeButton.Click += new System.EventHandler(this.CustomButton1_Click);
            // 
            // hybridConnectionDetailsTable
            // 
            this.hybridConnectionDetailsTable.AddNewButtonItem = null;
            this.hybridConnectionDetailsTable.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                       | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.hybridConnectionDetailsTable.AutoScroll = true;
            this.hybridConnectionDetailsTable.Columns = null;
            this.hybridConnectionDetailsTable.Location = new System.Drawing.Point(15, 65);
            this.hybridConnectionDetailsTable.Name = "hybridConnectionDetailsTable";
            this.hybridConnectionDetailsTable.SelectedIndex = 0;
            this.hybridConnectionDetailsTable.Size = new System.Drawing.Size(508, 283);
            this.hybridConnectionDetailsTable.TabIndex = 8;
            this.hybridConnectionDetailsTable.TableHeaderItem = null;
            // 
            // removeMenuButton
            // 
            this.removeMenuButton.AutoSize = true;
            this.removeMenuButton.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.removeMenuButton.Location = new System.Drawing.Point(12, 38);
            this.removeMenuButton.Name = "removeMenuButton";
            this.removeMenuButton.Size = new System.Drawing.Size(49, 15);
            this.removeMenuButton.TabIndex = 6;
            this.removeMenuButton.Text = "Remove";
            this.removeMenuButton.Click += new System.EventHandler(this.RemoveMenuButton_Click);
            // 
            // removingText
            // 
            this.removingText.AutoSize = true;
            this.removingText.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.removingText.Location = new System.Drawing.Point(65, 366);
            this.removingText.Name = "removingText";
            this.removingText.Size = new System.Drawing.Size(138, 15);
            this.removingText.TabIndex = 9;
            this.removingText.Text = "Removing, please wait...";
            // 
            // removingGif
            // 
            this.removingGif.Image = ((System.Drawing.Image)(resources.GetObject("removingGif.Image")));
            this.removingGif.Location = new System.Drawing.Point(44, 366);
            this.removingGif.Name = "removingGif";
            this.removingGif.Size = new System.Drawing.Size(18, 22);
            this.removingGif.TabIndex = 10;
            this.removingGif.TabStop = false;
            // 
            // HybridConnectionDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(535, 400);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.closeBlueButton);
            this.Controls.Add(this.removingGif);
            this.Controls.Add(this.removingText);
            this.Controls.Add(this.removeMenuButton);
            this.Controls.Add(this.hybridConnectionDetailsTable);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HybridConnectionDetails";
            this.Text = "HybridConnectionInfo";
            this.Load += new System.EventHandler(this.HybridConnectionDetails_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.removingGif)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}