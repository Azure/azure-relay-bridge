// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    partial class ManualAddHybridConnection
    {
        CustomButton addButton;

        CustomButton cancelButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        System.Windows.Forms.TextBox connectionStringTextBox;

        System.Windows.Forms.Label label1;

        System.Windows.Forms.Label label2;

        System.Windows.Forms.PictureBox pictureBox1;

        System.Windows.Forms.PictureBox savingGif;

        System.Windows.Forms.Label savingText;

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
                new System.ComponentModel.ComponentResourceManager(typeof(ManualAddHybridConnection));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.connectionStringTextBox = new System.Windows.Forms.TextBox();
            this.addButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.cancelButton = new HybridConnectionManagerIbizaUi.CustomButton();
            this.savingText = new System.Windows.Forms.Label();
            this.savingGif = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.savingGif)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage =
                ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(23, 23);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(37, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(262, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Add Hybrid Connection From Connection String";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Connection String: ";
            // 
            // connectionStringTextBox
            // 
            this.connectionStringTextBox.Font = new System.Drawing.Font("Calibri", 8.25F,
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectionStringTextBox.Location = new System.Drawing.Point(127, 55);
            this.connectionStringTextBox.Name = "connectionStringTextBox";
            this.connectionStringTextBox.Size = new System.Drawing.Size(360, 21);
            this.connectionStringTextBox.TabIndex = 10;
            // 
            // addButton
            // 
            this.addButton.ButtonText = "Add";
            this.addButton.ClickImage = null;
            this.addButton.DeactivatedImage =
                ((System.Drawing.Image)(resources.GetObject("addButton.DeactivatedImage")));
            this.addButton.DisabledTextColor = System.Drawing.Color.Black;
            this.addButton.EnabledTextColor = System.Drawing.Color.White;
            this.addButton.HoverImage = null;
            this.addButton.Location = new System.Drawing.Point(359, 96);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(53, 28);
            this.addButton.TabIndex = 11;
            this.addButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addButton.TextPosition = new System.Drawing.Point(10, 5);
            this.addButton.UnselectedImage = ((System.Drawing.Image)(resources.GetObject("addButton.UnselectedImage")));
            // 
            // cancelButton
            // 
            this.cancelButton.ButtonText = "Cancel";
            this.cancelButton.ClickImage = null;
            this.cancelButton.DeactivatedImage = null;
            this.cancelButton.DisabledTextColor = System.Drawing.Color.Black;
            this.cancelButton.EnabledTextColor = System.Drawing.Color.White;
            this.cancelButton.HoverImage = null;
            this.cancelButton.Location = new System.Drawing.Point(441, 96);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(53, 28);
            this.cancelButton.TabIndex = 12;
            this.cancelButton.TextFont = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.TextPosition = new System.Drawing.Point(10, 5);
            this.cancelButton.UnselectedImage =
                ((System.Drawing.Image)(resources.GetObject("cancelButton.UnselectedImage")));
            // 
            // savingText
            // 
            this.savingText.AutoSize = true;
            this.savingText.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.savingText.Location = new System.Drawing.Point(166, 102);
            this.savingText.Name = "savingText";
            this.savingText.Size = new System.Drawing.Size(121, 15);
            this.savingText.TabIndex = 16;
            this.savingText.Text = "Saving... Please Wait";
            // 
            // savingGif
            // 
            this.savingGif.Image = ((System.Drawing.Image)(resources.GetObject("savingGif.Image")));
            this.savingGif.Location = new System.Drawing.Point(147, 101);
            this.savingGif.Name = "savingGif";
            this.savingGif.Size = new System.Drawing.Size(18, 25);
            this.savingGif.TabIndex = 15;
            this.savingGif.TabStop = false;
            // 
            // ManualAddHybridConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(517, 133);
            this.Controls.Add(this.savingText);
            this.Controls.Add(this.savingGif);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.connectionStringTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManualAddHybridConnection";
            this.Text = "ManualAddHybridConnection";
            this.Load += new System.EventHandler(this.ManualAddHybridConnection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.savingGif)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}