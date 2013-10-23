// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Windows.Forms;

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    partial class NewEntityDialog
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
            this.entityNameLabel = new System.Windows.Forms.Label();
            this.entityNameTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.baseTypeLabel = new System.Windows.Forms.Label();
            this.baseTypeComboBox = new System.Windows.Forms.ComboBox();
            this.keyPropertyCheckBox = new System.Windows.Forms.CheckBox();
            this.propertyNameLabel = new System.Windows.Forms.Label();
            this.propertyTypeLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.entitySetLabel = new System.Windows.Forms.Label();
            this.entitySetTextBox = new System.Windows.Forms.TextBox();
            this.propertyTypeComboBox = new System.Windows.Forms.ComboBox();
            this.propertyNameTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // entityNameLabel
            // 
            this.entityNameLabel.Location = new System.Drawing.Point(6, 24);
            this.entityNameLabel.Name = "entityNameLabel";
            this.entityNameLabel.Size = new System.Drawing.Size(315, 13);
            this.entityNameLabel.TabIndex = 0;
            this.entityNameLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_EntityNameLabel;
            // 
            // entityNameTextBox
            // 
            this.entityNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.entityNameTextBox.Location = new System.Drawing.Point(9, 40);
            this.entityNameTextBox.Name = "entityNameTextBox";
            this.entityNameTextBox.Size = new System.Drawing.Size(314, 20);
            this.entityNameTextBox.TabIndex = 1;
            this.entityNameTextBox.TextChanged += new System.EventHandler(this.entityNameTextBox_TextChanged);
            // 
            // cancelButton
            // 
            this.cancelButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_AccessibleName;
            this.cancelButton.AutoSize = true;
            this.cancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.Location = new System.Drawing.Point(84, 3);
            this.cancelButton.MaximumSize = new System.Drawing.Size(75, 23);
            this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 14;
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
            this.okButton.MaximumSize = new System.Drawing.Size(75, 23);
            this.okButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 13;
            this.okButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.OKButton_Text;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // baseTypeLabel
            // 
            this.baseTypeLabel.Location = new System.Drawing.Point(6, 72);
            this.baseTypeLabel.Name = "baseTypeLabel";
            this.baseTypeLabel.Size = new System.Drawing.Size(315, 13);
            this.baseTypeLabel.TabIndex = 2;
            this.baseTypeLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_BaseTypeLabel;
            // 
            // baseTypeComboBox
            // 
            this.baseTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baseTypeComboBox.FormattingEnabled = true;
            this.baseTypeComboBox.Location = new System.Drawing.Point(9, 88);
            this.baseTypeComboBox.Name = "baseTypeComboBox";
            this.baseTypeComboBox.Size = new System.Drawing.Size(313, 21);
            this.baseTypeComboBox.Sorted = true;
            this.baseTypeComboBox.TabIndex = 3;
            this.baseTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.baseTypeComboBox_SelectedIndexChanged);
            // 
            // keyPropertyCheckBox
            // 
            this.keyPropertyCheckBox.Location = new System.Drawing.Point(10, 20);
            this.keyPropertyCheckBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.keyPropertyCheckBox.Name = "keyPropertyCheckBox";
            this.keyPropertyCheckBox.Size = new System.Drawing.Size(314, 26);
            this.keyPropertyCheckBox.TabIndex = 7;
            this.keyPropertyCheckBox.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_CreateKeyLabel;
            this.keyPropertyCheckBox.UseVisualStyleBackColor = true;
            this.keyPropertyCheckBox.CheckedChanged += new System.EventHandler(this.keyPropertyCheckBox_CheckedChanged);
            // 
            // propertyNameLabel
            // 
            this.propertyNameLabel.Location = new System.Drawing.Point(6, 50);
            this.propertyNameLabel.Name = "propertyNameLabel";
            this.propertyNameLabel.Size = new System.Drawing.Size(315, 13);
            this.propertyNameLabel.TabIndex = 8;
            this.propertyNameLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_PropertyNameLabel;
            // 
            // propertyTypeLabel
            // 
            this.propertyTypeLabel.Location = new System.Drawing.Point(6, 98);
            this.propertyTypeLabel.Name = "propertyTypeLabel";
            this.propertyTypeLabel.Size = new System.Drawing.Size(315, 13);
            this.propertyTypeLabel.TabIndex = 10;
            this.propertyTypeLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_PropertyTypeLabel;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.entitySetLabel);
            this.groupBox1.Controls.Add(this.entitySetTextBox);
            this.groupBox1.Controls.Add(this.entityNameLabel);
            this.groupBox1.Controls.Add(this.entityNameTextBox);
            this.groupBox1.Controls.Add(this.baseTypeComboBox);
            this.groupBox1.Controls.Add(this.baseTypeLabel);
            this.groupBox1.Location = new System.Drawing.Point(13, 19);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(334, 175);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_PropertiesLabel;
            // 
            // entitySetLabel
            // 
            this.entitySetLabel.Location = new System.Drawing.Point(6, 122);
            this.entitySetLabel.Name = "entitySetLabel";
            this.entitySetLabel.Size = new System.Drawing.Size(315, 13);
            this.entitySetLabel.TabIndex = 4;
            this.entitySetLabel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_EntitySetLabel;
            // 
            // entitySetTextBox
            // 
            this.entitySetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.entitySetTextBox.Location = new System.Drawing.Point(9, 138);
            this.entitySetTextBox.Name = "entitySetTextBox";
            this.entitySetTextBox.Size = new System.Drawing.Size(314, 20);
            this.entitySetTextBox.TabIndex = 5;
            // 
            // propertyTypeComboBox
            // 
            this.propertyTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.propertyTypeComboBox.FormattingEnabled = true;
            this.propertyTypeComboBox.Location = new System.Drawing.Point(9, 114);
            this.propertyTypeComboBox.Name = "propertyTypeComboBox";
            this.propertyTypeComboBox.Size = new System.Drawing.Size(313, 21);
            this.propertyTypeComboBox.Sorted = true;
            this.propertyTypeComboBox.TabIndex = 11;
            // 
            // propertyNameTextBox
            // 
            this.propertyNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyNameTextBox.Location = new System.Drawing.Point(9, 66);
            this.propertyNameTextBox.Name = "propertyNameTextBox";
            this.propertyNameTextBox.Size = new System.Drawing.Size(314, 20);
            this.propertyNameTextBox.TabIndex = 9;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.propertyNameTextBox);
            this.groupBox2.Controls.Add(this.propertyTypeComboBox);
            this.groupBox2.Controls.Add(this.keyPropertyCheckBox);
            this.groupBox2.Controls.Add(this.propertyNameLabel);
            this.groupBox2.Controls.Add(this.propertyTypeLabel);
            this.groupBox2.Location = new System.Drawing.Point(12, 205);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(334, 152);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_KeyPropertyLabel;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.okButton, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(184, 379);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(162, 29);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // NewEntityDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(358, 420);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewEntityDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewEntityDialog_Title;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            int resolutionWidth = Screen.PrimaryScreen.Bounds.Width;
            int resolutionHeight = Screen.PrimaryScreen.Bounds.Height;

            //Check if the form does not fit in the window anymore, if it does not, add a scrollbar
            if (resolutionHeight < this.Size.Height ||
               resolutionWidth < this.Size.Width)
            {
                this.AutoScroll = true;
                this.AutoScrollMinSize = new System.Drawing.Size(this.Size.Width, this.Size.Height + Screen.PrimaryScreen.Bounds.Height);
            }
            else // handling the case where we first change the resolution to be small and add a scroll bar, and then make it big again. we should not show the scroll bar.
            {
                this.AutoScroll = false;
            }

            base.OnSizeChanged(e);
        }

        #endregion

        private System.Windows.Forms.Label entityNameLabel;
        private System.Windows.Forms.TextBox entityNameTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label baseTypeLabel;
        private System.Windows.Forms.ComboBox baseTypeComboBox;
        private System.Windows.Forms.CheckBox keyPropertyCheckBox;
        private System.Windows.Forms.Label propertyNameLabel;
        private System.Windows.Forms.Label propertyTypeLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox propertyTypeComboBox;
        private System.Windows.Forms.TextBox propertyNameTextBox;
        private System.Windows.Forms.Label entitySetLabel;
        private System.Windows.Forms.TextBox entitySetTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

