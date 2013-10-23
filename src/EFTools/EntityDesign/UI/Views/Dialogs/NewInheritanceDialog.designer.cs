// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    partial class NewInheritanceDialog
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.baseEntityLabel = new System.Windows.Forms.Label();
            this.baseEntityComboBox = new System.Windows.Forms.ComboBox();
            this.derivedEntityLabel = new System.Windows.Forms.Label();
            this.derivedEntityComboBox = new System.Windows.Forms.ComboBox();
            this.infoLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_AccessibleName;
            this.cancelButton.AutoSize = true;
            this.cancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.Location = new System.Drawing.Point(84, 3);
            this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_Text;
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.OKButton_AccessibleName;
            this.okButton.AutoSize = true;
            this.okButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.okButton.Location = new System.Drawing.Point(3, 3);
            this.okButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.OKButton_Text;
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // baseEntityLabel
            // 
            this.baseEntityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.baseEntityLabel.AutoSize = true;
            this.baseEntityLabel.Location = new System.Drawing.Point(12, 54);
            this.baseEntityLabel.Name = "baseEntityLabel";
            this.baseEntityLabel.Size = new System.Drawing.Size(103, 13);
            this.baseEntityLabel.TabIndex = 1;
            this.baseEntityLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewInheritanceDialog_SelectBaseEntity;
            // 
            // baseEntityComboBox
            // 
            this.baseEntityComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.baseEntityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baseEntityComboBox.FormattingEnabled = true;
            this.baseEntityComboBox.Location = new System.Drawing.Point(15, 70);
            this.baseEntityComboBox.Name = "baseEntityComboBox";
            this.baseEntityComboBox.Size = new System.Drawing.Size(363, 21);
            this.baseEntityComboBox.TabIndex = 2;
            this.baseEntityComboBox.SelectedIndexChanged += new System.EventHandler(this.baseEntityComboBox_SelectedIndexChanged);
            // 
            // derivedEntityLabel
            // 
            this.derivedEntityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.derivedEntityLabel.AutoSize = true;
            this.derivedEntityLabel.Location = new System.Drawing.Point(12, 112);
            this.derivedEntityLabel.Name = "derivedEntityLabel";
            this.derivedEntityLabel.Size = new System.Drawing.Size(115, 13);
            this.derivedEntityLabel.TabIndex = 3;
            this.derivedEntityLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewInheritanceDialog_SelectDerivedEntity;
            // 
            // derivedEntityComboBox
            // 
            this.derivedEntityComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.derivedEntityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.derivedEntityComboBox.FormattingEnabled = true;
            this.derivedEntityComboBox.Location = new System.Drawing.Point(15, 128);
            this.derivedEntityComboBox.Name = "derivedEntityComboBox";
            this.derivedEntityComboBox.Size = new System.Drawing.Size(363, 21);
            this.derivedEntityComboBox.TabIndex = 4;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoLabel.Location = new System.Drawing.Point(3, 3);
            this.infoLabel.Margin = new System.Windows.Forms.Padding(3);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(367, 13);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewInheritanceDialog_InfoText;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.infoLabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(380, 35);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.okButton, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(218, 184);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(162, 29);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // NewInheritanceDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(394, 225);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.derivedEntityComboBox);
            this.Controls.Add(this.derivedEntityLabel);
            this.Controls.Add(this.baseEntityComboBox);
            this.Controls.Add(this.baseEntityLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewInheritanceDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewInheritanceDialog_Title;
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label baseEntityLabel;
        private System.Windows.Forms.ComboBox baseEntityComboBox;
        private System.Windows.Forms.Label derivedEntityLabel;
        private System.Windows.Forms.ComboBox derivedEntityComboBox;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

