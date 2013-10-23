// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    partial class DeleteStorageEntitySetsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.DeleteStorageEntitySetsDialogCancelButton = new System.Windows.Forms.Button();
            this.NoButton = new System.Windows.Forms.Button();
            this.YesButton = new System.Windows.Forms.Button();
            this.StorageEntitySetsListBox = new System.Windows.Forms.ListBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DeleteStorageEntitySetsDialogCancelButton
            // 
            this.DeleteStorageEntitySetsDialogCancelButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_AccessibleName;
            this.DeleteStorageEntitySetsDialogCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteStorageEntitySetsDialogCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.DeleteStorageEntitySetsDialogCancelButton.Location = new System.Drawing.Point(350, 225);
            this.DeleteStorageEntitySetsDialogCancelButton.Name = "DeleteStorageEntitySetsDialogCancelButton";
            this.DeleteStorageEntitySetsDialogCancelButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteStorageEntitySetsDialogCancelButton.TabIndex = 4;
            this.DeleteStorageEntitySetsDialogCancelButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_Text;
            this.DeleteStorageEntitySetsDialogCancelButton.UseVisualStyleBackColor = true;
            this.DeleteStorageEntitySetsDialogCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // NoButton
            // 
            this.NoButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NoButton_AccessibleName;
            this.NoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NoButton.Location = new System.Drawing.Point(269, 225);
            this.NoButton.Name = "NoButton";
            this.NoButton.Size = new System.Drawing.Size(75, 23);
            this.NoButton.TabIndex = 3;
            this.NoButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NoButton_Text;
            this.NoButton.UseVisualStyleBackColor = true;
            this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
            // 
            // YesButton
            // 
            this.YesButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.YesButton_AccessibleName;
            this.YesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.YesButton.Location = new System.Drawing.Point(188, 225);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(75, 23);
            this.YesButton.TabIndex = 2;
            this.YesButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.YesButton_Text;
            this.YesButton.UseVisualStyleBackColor = true;
            this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
            // 
            // StorageEntitySetsListBox
            // 
            this.StorageEntitySetsListBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.DeleteStorageEntitySetsListBox_AccessibleName;
            this.StorageEntitySetsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StorageEntitySetsListBox.FormattingEnabled = true;
            this.StorageEntitySetsListBox.Location = new System.Drawing.Point(15, 50);
            this.StorageEntitySetsListBox.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.StorageEntitySetsListBox.Name = "StorageEntitySetsListBox";
            this.StorageEntitySetsListBox.Size = new System.Drawing.Size(410, 160);
            this.StorageEntitySetsListBox.TabIndex = 1;
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionLabel.Location = new System.Drawing.Point(15, 12);
            this.DescriptionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(410, 32);
            this.DescriptionLabel.TabIndex = 0;
            this.DescriptionLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.DeleteStorageEntitySetsDescription;
            // 
            // DeleteStorageEntitySetsDialog
            // 
            this.AcceptButton = this.YesButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.DeleteStorageEntitySetsDialogCancelButton;
            this.ClientSize = new System.Drawing.Size(440, 260);
            this.Controls.Add(this.DescriptionLabel);
            this.Controls.Add(this.StorageEntitySetsListBox);
            this.Controls.Add(this.DeleteStorageEntitySetsDialogCancelButton);
            this.Controls.Add(this.NoButton);
            this.Controls.Add(this.YesButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeleteStorageEntitySetsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.DeleteStorageEntitySetsDialogTitle;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button DeleteStorageEntitySetsDialogCancelButton;
        private System.Windows.Forms.Button NoButton;
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.ListBox StorageEntitySetsListBox;
        private System.Windows.Forms.Label DescriptionLabel;

    }
}