// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Windows.Forms;

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    partial class NewAssociationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewAssociationDialog));
            this.associationNameLabel = new System.Windows.Forms.Label();
            this.associationNameTextBox = new System.Windows.Forms.TextBox();
            this.end1GroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.multiplicity1ComboBox = new System.Windows.Forms.ComboBox();
            this.navigationProperty1TextBox = new System.Windows.Forms.TextBox();
            this.entity1Label = new System.Windows.Forms.Label();
            this.multiplicity1Label = new System.Windows.Forms.Label();
            this.entity1ComboBox = new System.Windows.Forms.ComboBox();
            this.navigationPropertyCheckbox = new System.Windows.Forms.CheckBox();
            this.createForeignKeysCheckBox = new System.Windows.Forms.CheckBox();
            this.explanationTextBox = new System.Windows.Forms.TextBox();
            this.end2GroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.navigationProperty2Checkbox = new System.Windows.Forms.CheckBox();
            this.multiplicity2ComboBox = new System.Windows.Forms.ComboBox();
            this.navigationProperty2TextBox = new System.Windows.Forms.TextBox();
            this.entity2Label = new System.Windows.Forms.Label();
            this.multiplicity2Label = new System.Windows.Forms.Label();
            this.entity2ComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.end1GroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.end2GroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.buttonsTableLayout.SuspendLayout();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // associationNameLabel
            // 
            resources.ApplyResources(this.associationNameLabel, "associationNameLabel");
            this.associationNameLabel.Name = "associationNameLabel";
            // 
            // associationNameTextBox
            // 
            resources.ApplyResources(this.associationNameTextBox, "associationNameTextBox");
            this.associationNameTextBox.Name = "associationNameTextBox";
            this.associationNameTextBox.TextChanged += new System.EventHandler(this.associationNameTextBox_TextChanged);
            // 
            // end1GroupBox
            // 
            resources.ApplyResources(this.end1GroupBox, "end1GroupBox");
            this.end1GroupBox.Controls.Add(this.tableLayoutPanel1);
            this.end1GroupBox.Name = "end1GroupBox";
            this.end1GroupBox.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.multiplicity1ComboBox, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.navigationProperty1TextBox, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.entity1Label, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.multiplicity1Label, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.entity1ComboBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.navigationPropertyCheckbox, 0, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // multiplicity1ComboBox
            // 
            this.multiplicity1ComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_Multiplicity1AccessibleName;
            resources.ApplyResources(this.multiplicity1ComboBox, "multiplicity1ComboBox");
            this.multiplicity1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.multiplicity1ComboBox.FormattingEnabled = true;
            this.multiplicity1ComboBox.Name = "multiplicity1ComboBox";
            // 
            // navigationProperty1TextBox
            // 
            this.navigationProperty1TextBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_NavigationProperty1AccessibleName;
            resources.ApplyResources(this.navigationProperty1TextBox, "navigationProperty1TextBox");
            this.navigationProperty1TextBox.Name = "navigationProperty1TextBox";
            this.navigationProperty1TextBox.TextChanged += new System.EventHandler(this.navigationProperty1TextBox_TextChanged);
            // 
            // entity1Label
            // 
            resources.ApplyResources(this.entity1Label, "entity1Label");
            this.entity1Label.Name = "entity1Label";
            // 
            // multiplicity1Label
            // 
            resources.ApplyResources(this.multiplicity1Label, "multiplicity1Label");
            this.multiplicity1Label.Name = "multiplicity1Label";
            // 
            // entity1ComboBox
            // 
            this.entity1ComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_Entity1AccessibleName;
            resources.ApplyResources(this.entity1ComboBox, "entity1ComboBox");
            this.entity1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.entity1ComboBox.FormattingEnabled = true;
            this.entity1ComboBox.Name = "entity1ComboBox";
            this.entity1ComboBox.Sorted = true;
            // 
            // navigationPropertyCheckbox
            // 
            resources.ApplyResources(this.navigationPropertyCheckbox, "navigationPropertyCheckbox");
            this.navigationPropertyCheckbox.Checked = true;
            this.navigationPropertyCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.navigationPropertyCheckbox.Name = "navigationPropertyCheckbox";
            this.navigationPropertyCheckbox.UseVisualStyleBackColor = true;
            this.navigationPropertyCheckbox.CheckedChanged += new System.EventHandler(this.navigationPropertyCheckbox_CheckedChanged);
            // 
            // createForeignKeysCheckBox
            // 
            resources.ApplyResources(this.createForeignKeysCheckBox, "createForeignKeysCheckBox");
            this.createForeignKeysCheckBox.Checked = true;
            this.createForeignKeysCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.createForeignKeysCheckBox.Name = "createForeignKeysCheckBox";
            this.createForeignKeysCheckBox.UseVisualStyleBackColor = true;
            // 
            // explanationTextBox
            // 
            this.explanationTextBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_ExplanationBoxAccessibleName;
            resources.ApplyResources(this.explanationTextBox, "explanationTextBox");
            this.explanationTextBox.Name = "explanationTextBox";
            this.explanationTextBox.ReadOnly = true;
            // 
            // end2GroupBox
            // 
            resources.ApplyResources(this.end2GroupBox, "end2GroupBox");
            this.end2GroupBox.Controls.Add(this.tableLayoutPanel3);
            this.end2GroupBox.Name = "end2GroupBox";
            this.end2GroupBox.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.navigationProperty2Checkbox, 0, 6);
            this.tableLayoutPanel3.Controls.Add(this.multiplicity2ComboBox, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.navigationProperty2TextBox, 0, 7);
            this.tableLayoutPanel3.Controls.Add(this.entity2Label, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.multiplicity2Label, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.entity2ComboBox, 0, 1);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // navigationProperty2Checkbox
            // 
            resources.ApplyResources(this.navigationProperty2Checkbox, "navigationProperty2Checkbox");
            this.navigationProperty2Checkbox.Checked = true;
            this.navigationProperty2Checkbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.navigationProperty2Checkbox.Name = "navigationProperty2Checkbox";
            this.navigationProperty2Checkbox.UseVisualStyleBackColor = true;
            this.navigationProperty2Checkbox.CheckedChanged += new System.EventHandler(this.navigationProperty2Checkbox_CheckedChanged);
            // 
            // multiplicity2ComboBox
            // 
            this.multiplicity2ComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_Multiplicity2AccessibleName;
            resources.ApplyResources(this.multiplicity2ComboBox, "multiplicity2ComboBox");
            this.multiplicity2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.multiplicity2ComboBox.FormattingEnabled = true;
            this.multiplicity2ComboBox.Name = "multiplicity2ComboBox";
            // 
            // navigationProperty2TextBox
            // 
            this.navigationProperty2TextBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_NavigationProperty2AccessibleName;
            resources.ApplyResources(this.navigationProperty2TextBox, "navigationProperty2TextBox");
            this.navigationProperty2TextBox.Name = "navigationProperty2TextBox";
            this.navigationProperty2TextBox.TextChanged += new System.EventHandler(this.navigationProperty2TextBox_TextChanged);
            // 
            // entity2Label
            // 
            resources.ApplyResources(this.entity2Label, "entity2Label");
            this.entity2Label.Name = "entity2Label";
            // 
            // multiplicity2Label
            // 
            resources.ApplyResources(this.multiplicity2Label, "multiplicity2Label");
            this.multiplicity2Label.Name = "multiplicity2Label";
            // 
            // entity2ComboBox
            // 
            this.entity2ComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewAssociationDialog_Entity2AccessibleName;
            resources.ApplyResources(this.entity2ComboBox, "entity2ComboBox");
            this.entity2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.entity2ComboBox.FormattingEnabled = true;
            this.entity2ComboBox.Name = "entity2ComboBox";
            this.entity2ComboBox.Sorted = true;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.end1GroupBox, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.end2GroupBox, 1, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // buttonsTableLayout
            // 
            resources.ApplyResources(this.buttonsTableLayout, "buttonsTableLayout");
            this.buttonsTableLayout.Controls.Add(this.okButton, 0, 0);
            this.buttonsTableLayout.Controls.Add(this.cancelButton, 1, 0);
            this.buttonsTableLayout.Name = "buttonsTableLayout";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.MaximumSize = new System.Drawing.Size(75, 23);
            this.okButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.MaximumSize = new System.Drawing.Size(75, 23);
            this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_Text;
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // mainTableLayoutPanel
            // 
            resources.ApplyResources(this.mainTableLayoutPanel, "mainTableLayoutPanel");
            this.mainTableLayoutPanel.Controls.Add(this.associationNameLabel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.associationNameTextBox, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.mainTableLayoutPanel.Controls.Add(this.createForeignKeysCheckBox, 0, 4);
            this.mainTableLayoutPanel.Controls.Add(this.explanationTextBox, 0, 5);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            // 
            // NewAssociationDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.buttonsTableLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewAssociationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.end1GroupBox.ResumeLayout(false);
            this.end1GroupBox.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.end2GroupBox.ResumeLayout(false);
            this.end2GroupBox.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.buttonsTableLayout.ResumeLayout(false);
            this.buttonsTableLayout.PerformLayout();
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

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

        private System.Windows.Forms.Label associationNameLabel;
        private System.Windows.Forms.TextBox associationNameTextBox;
        private System.Windows.Forms.GroupBox end1GroupBox;
        private System.Windows.Forms.ComboBox multiplicity1ComboBox;
        private System.Windows.Forms.Label multiplicity1Label;
        private System.Windows.Forms.Label entity1Label;
        private System.Windows.Forms.TextBox navigationProperty1TextBox;
        private System.Windows.Forms.ComboBox entity1ComboBox;
        private System.Windows.Forms.TextBox explanationTextBox;
        private System.Windows.Forms.GroupBox end2GroupBox;
        private System.Windows.Forms.Label multiplicity2Label;
        private System.Windows.Forms.Label entity2Label;
        private System.Windows.Forms.ComboBox entity2ComboBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel buttonsTableLayout;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ComboBox multiplicity2ComboBox;
        private System.Windows.Forms.TextBox navigationProperty2TextBox;
        private System.Windows.Forms.CheckBox navigationPropertyCheckbox;
        private System.Windows.Forms.CheckBox navigationProperty2Checkbox;
        private System.Windows.Forms.CheckBox createForeignKeysCheckBox;
    }
}

