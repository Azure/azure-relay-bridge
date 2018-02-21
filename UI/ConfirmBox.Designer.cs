// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    partial class ConfirmBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        System.Windows.Forms.Label confirmText;

        System.Windows.Forms.Label label1;

        CustomButton noButton;

        System.Windows.Forms.PictureBox pictureBox1;

        CustomButton yesButton;

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
                new System.ComponentModel.ComponentResourceManager(typeof(ConfirmBox));
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.confirmText = new System.Windows.Forms.Label();
            this.yesButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.noButton = new HybridConnectionManagerIbizaUi.CustomButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(38, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Confirm";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage =
                ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Location = new System.Drawing.Point(9, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(23, 23);
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // confirmText
            // 
            this.confirmText.AutoSize = true;
            this.confirmText.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.confirmText.Location = new System.Drawing.Point(109, 44);
            this.confirmText.Name = "confirmText";
            this.confirmText.Size = new System.Drawing.Size(50, 15);
            this.confirmText.TabIndex = 7;
            this.confirmText.Text = "Confirm";
            // 
            // yesButton
            // 
            this.yesButton.ButtonText = "Yes";
            this.yesButton.ClickImage = null;
            this.yesButton.DeactivatedImage = null;
            this.yesButton.DisabledTextColor = System.Drawing.Color.Black;
            this.yesButton.EnabledTextColor = System.Drawing.Color.White;
            this.yesButton.HoverImage = null;
            this.yesButton.Location = new System.Drawing.Point(151, 89);
            this.yesButton.Name = "yesButton";
            this.yesButton.Size = new System.Drawing.Size(53, 28);
            this.yesButton.TabIndex = 8;
            this.yesButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.yesButton.TextPosition = new System.Drawing.Point(10, 5);
            this.yesButton.UnselectedImage = ((System.Drawing.Image)(resources.GetObject("yesButton.UnselectedImage")));
            // 
            // noButton
            // 
            this.noButton.ButtonText = "No";
            this.noButton.ClickImage = null;
            this.noButton.DeactivatedImage = null;
            this.noButton.DisabledTextColor = System.Drawing.Color.Black;
            this.noButton.EnabledTextColor = System.Drawing.Color.White;
            this.noButton.HoverImage = null;
            this.noButton.Location = new System.Drawing.Point(218, 89);
            this.noButton.Name = "noButton";
            this.noButton.Size = new System.Drawing.Size(53, 28);
            this.noButton.TabIndex = 9;
            this.noButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noButton.TextPosition = new System.Drawing.Point(10, 5);
            this.noButton.UnselectedImage = ((System.Drawing.Image)(resources.GetObject("noButton.UnselectedImage")));
            // 
            // ConfirmBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 129);
            this.Controls.Add(this.yesButton);
            this.Controls.Add(this.noButton);
            this.Controls.Add(this.confirmText);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConfirmBox";
            this.Text = "ConfirmBox";
            this.Load += new System.EventHandler(this.ConfirmBox_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}