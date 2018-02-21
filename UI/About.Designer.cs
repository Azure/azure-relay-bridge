// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    partial class About
    {
        CustomButton closeButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        System.Windows.Forms.Label currentVersionText;

        System.Windows.Forms.LinkLabel downloadUpdateLink;

        System.Windows.Forms.Label label2;

        System.Windows.Forms.Label label3;

        System.Windows.Forms.Label label4;

        System.Windows.Forms.Label label5;

        System.Windows.Forms.Label label6;

        System.Windows.Forms.LinkLabel moreInfoLink;

        System.Windows.Forms.PictureBox pictureBox1;

        System.Windows.Forms.Label updateAvailableText;

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
                new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.currentVersionText = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.moreInfoLink = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.closeButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.updateAvailableText = new System.Windows.Forms.Label();
            this.downloadUpdateLink = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage =
                ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.ErrorImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(29, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(328, 109);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // currentVersionText
            // 
            this.currentVersionText.AutoSize = true;
            this.currentVersionText.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentVersionText.Location = new System.Drawing.Point(32, 138);
            this.currentVersionText.Name = "currentVersionText";
            this.currentVersionText.Size = new System.Drawing.Size(62, 18);
            this.currentVersionText.TabIndex = 1;
            this.currentVersionText.Text = "Version: ";
            this.currentVersionText.Click += new System.EventHandler(this.Label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(32, 173);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(334, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "This service works with the Azure App Service Hybrid";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(32, 191);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(310, 18);
            this.label3.TabIndex = 3;
            this.label3.Text = "Connection feature to enable access to resources";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(32, 209);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(327, 18);
            this.label4.TabIndex = 4;
            this.label4.Text = "that are otherwise not accessible from the internet.";
            // 
            // moreInfoLink
            // 
            this.moreInfoLink.AutoSize = true;
            this.moreInfoLink.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moreInfoLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))),
                ((int)(((byte)(240)))));
            this.moreInfoLink.Location = new System.Drawing.Point(53, 227);
            this.moreInfoLink.Name = "moreInfoLink";
            this.moreInfoLink.Size = new System.Drawing.Size(37, 18);
            this.moreInfoLink.TabIndex = 6;
            this.moreInfoLink.TabStop = true;
            this.moreInfoLink.Text = "here";
            this.moreInfoLink.LinkClicked +=
                new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MoreInfoLink_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(32, 227);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 18);
            this.label5.TabIndex = 5;
            this.label5.Text = "Go             for more details";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(31, 293);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(316, 18);
            this.label6.TabIndex = 8;
            this.label6.Text = "© 2017 Microsoft Corporation. All rights reserved. ";
            // 
            // closeButton
            // 
            this.closeButton.ButtonText = "Close";
            this.closeButton.ClickImage = null;
            this.closeButton.DeactivatedImage = null;
            this.closeButton.DisabledTextColor = System.Drawing.Color.Black;
            this.closeButton.EnabledTextColor = System.Drawing.Color.White;
            this.closeButton.HoverImage = null;
            this.closeButton.Location = new System.Drawing.Point(306, 324);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(53, 27);
            this.closeButton.TabIndex = 9;
            this.closeButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.TextPosition = new System.Drawing.Point(0, 0);
            this.closeButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("closeButton.UnselectedImage")));
            // 
            // updateAvailableText
            // 
            this.updateAvailableText.AutoSize = true;
            this.updateAvailableText.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateAvailableText.ForeColor = System.Drawing.Color.Red;
            this.updateAvailableText.Location = new System.Drawing.Point(86, 9);
            this.updateAvailableText.Name = "updateAvailableText";
            this.updateAvailableText.Size = new System.Drawing.Size(203, 18);
            this.updateAvailableText.TabIndex = 10;
            this.updateAvailableText.Text = "There is a new update available";
            // 
            // downloadUpdateLink
            // 
            this.downloadUpdateLink.AutoSize = true;
            this.downloadUpdateLink.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.downloadUpdateLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))),
                ((int)(((byte)(176)))), ((int)(((byte)(240)))));
            this.downloadUpdateLink.Location = new System.Drawing.Point(32, 259);
            this.downloadUpdateLink.Name = "downloadUpdateLink";
            this.downloadUpdateLink.Size = new System.Drawing.Size(118, 18);
            this.downloadUpdateLink.TabIndex = 11;
            this.downloadUpdateLink.TabStop = true;
            this.downloadUpdateLink.Text = "Download update";
            this.downloadUpdateLink.LinkClicked +=
                new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DownloadUpdateLink_LinkClicked);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(385, 363);
            this.Controls.Add(this.downloadUpdateLink);
            this.Controls.Add(this.updateAvailableText);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.moreInfoLink);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.currentVersionText);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "About";
            this.Text = "About";
            this.Load += new System.EventHandler(this.About_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}