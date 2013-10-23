// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System.Diagnostics.CodeAnalysis;

    partial class WizardPageDbConfig
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardPageDbConfig));
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
            System.Windows.Forms.Panel panel;
            this.checkBoxSaveInAppConfig = new Microsoft.Data.Entity.Design.UI.Views.Controls.AutoWrapCheckBox();
            this.disallowSensitiveInfoButton = new Microsoft.Data.Entity.Design.UI.Views.Controls.AutoWrapRadioButton();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.allowSensitiveInfoButton = new Microsoft.Data.Entity.Design.UI.Views.Controls.AutoWrapRadioButton();
            this.lblPagePrompt = new System.Windows.Forms.Label();
            this.dataSourceComboBox = new System.Windows.Forms.ComboBox();
            this.newDBConnectionButton = new System.Windows.Forms.Button();
            this.textBoxAppConfigConnectionName = new System.Windows.Forms.TextBox();
            this.sensitiveInfoTextBox = new System.Windows.Forms.Label();
            this.lblEntityConnectionString = new System.Windows.Forms.Label();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            panel = new System.Windows.Forms.Panel();
            tableLayoutPanel.SuspendLayout();
            panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // infoPanel
            // 
            resources.ApplyResources(this.infoPanel, "infoPanel");
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(tableLayoutPanel, "tableLayoutPanel");
            tableLayoutPanel.Controls.Add(this.checkBoxSaveInAppConfig, 0, 7);
            tableLayoutPanel.Controls.Add(this.disallowSensitiveInfoButton, 0, 3);
            tableLayoutPanel.Controls.Add(this.textBoxConnectionString, 0, 6);
            tableLayoutPanel.Controls.Add(this.allowSensitiveInfoButton, 0, 4);
            tableLayoutPanel.Controls.Add(this.lblPagePrompt, 0, 0);
            tableLayoutPanel.Controls.Add(panel, 0, 1);
            tableLayoutPanel.Controls.Add(this.textBoxAppConfigConnectionName, 0, 8);
            tableLayoutPanel.Controls.Add(this.sensitiveInfoTextBox, 0, 2);
            tableLayoutPanel.Controls.Add(this.lblEntityConnectionString, 0, 5);
            tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // checkBoxSaveInAppConfig
            // 
            resources.ApplyResources(this.checkBoxSaveInAppConfig, "checkBoxSaveInAppConfig");
            this.checkBoxSaveInAppConfig.Checked = true;
            this.checkBoxSaveInAppConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSaveInAppConfig.Name = "checkBoxSaveInAppConfig";
            this.checkBoxSaveInAppConfig.Tag = "";
            this.checkBoxSaveInAppConfig.UseVisualStyleBackColor = true;
            this.checkBoxSaveInAppConfig.CheckedChanged += new System.EventHandler(this.checkBoxSaveInAppConfig_CheckedChanged);
            // 
            // disallowSensitiveInfoButton
            // 
            resources.ApplyResources(this.disallowSensitiveInfoButton, "disallowSensitiveInfoButton");
            this.disallowSensitiveInfoButton.Name = "disallowSensitiveInfoButton";
            this.disallowSensitiveInfoButton.TabStop = true;
            this.disallowSensitiveInfoButton.UseVisualStyleBackColor = true;
            this.disallowSensitiveInfoButton.CheckedChanged += new System.EventHandler(this.disallowSensitiveInfoButton_CheckedChanged);
            // 
            // textBoxConnectionString
            // 
            resources.ApplyResources(this.textBoxConnectionString, "textBoxConnectionString");
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.ReadOnly = true;
            this.textBoxConnectionString.TabStop = true;
            // 
            // allowSensitiveInfoButton
            // 
            resources.ApplyResources(this.allowSensitiveInfoButton, "allowSensitiveInfoButton");
            this.allowSensitiveInfoButton.Name = "allowSensitiveInfoButton";
            this.allowSensitiveInfoButton.TabStop = true;
            this.allowSensitiveInfoButton.UseVisualStyleBackColor = true;
            this.allowSensitiveInfoButton.CheckedChanged += new System.EventHandler(this.allowSensitiveInfoButton_CheckedChanged);
            // 
            // lblPagePrompt
            // 
            resources.ApplyResources(this.lblPagePrompt, "lblPagePrompt");
            this.lblPagePrompt.Name = "lblPagePrompt";
            // 
            // panel
            // 
            resources.ApplyResources(panel, "panel");
            panel.Controls.Add(this.dataSourceComboBox);
            panel.Controls.Add(this.newDBConnectionButton);
            panel.Name = "panel";
            // 
            // dataSourceComboBox
            // 
            resources.ApplyResources(this.dataSourceComboBox, "dataSourceComboBox");
            this.dataSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataSourceComboBox.FormattingEnabled = true;
            this.dataSourceComboBox.Name = "dataSourceComboBox";
            this.dataSourceComboBox.SelectedIndexChanged += new System.EventHandler(this.dataSourceComboBox_SelectedIndexChanged);
            // 
            // newDBConnectionButton
            // 
            resources.ApplyResources(this.newDBConnectionButton, "newDBConnectionButton");
            this.newDBConnectionButton.Name = "newDBConnectionButton";
            this.newDBConnectionButton.UseVisualStyleBackColor = true;
            this.newDBConnectionButton.Click += new System.EventHandler(this.newDBConnectionButton_Click);
            // 
            // textBoxAppConfigConnectionName
            // 
            resources.ApplyResources(this.textBoxAppConfigConnectionName, "textBoxAppConfigConnectionName");
            this.textBoxAppConfigConnectionName.Name = "textBoxAppConfigConnectionName";
            // 
            // sensitiveInfoTextBox
            // 
            resources.ApplyResources(this.sensitiveInfoTextBox, "sensitiveInfoTextBox");
            this.sensitiveInfoTextBox.Name = "sensitiveInfoTextBox";
            // 
            // lblEntityConnectionString
            // 
            resources.ApplyResources(this.lblEntityConnectionString, "lblEntityConnectionString");
            this.lblEntityConnectionString.Name = "lblEntityConnectionString";
            // 
            // WizardPageDbConfig
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(tableLayoutPanel);
            this.Name = "WizardPageDbConfig";
            this.Resize += new System.EventHandler(this.WizardPageDbConfig_Resize);
            this.Controls.SetChildIndex(this.infoPanel, 0);
            this.Controls.SetChildIndex(tableLayoutPanel, 0);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            panel.ResumeLayout(false);
            panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblEntityConnectionString;
        private System.Windows.Forms.Label sensitiveInfoTextBox;
        private Microsoft.Data.Entity.Design.UI.Views.Controls.AutoWrapRadioButton allowSensitiveInfoButton;
        private Microsoft.Data.Entity.Design.UI.Views.Controls.AutoWrapRadioButton disallowSensitiveInfoButton;
        private System.Windows.Forms.TextBox textBoxAppConfigConnectionName;
        private Microsoft.Data.Entity.Design.UI.Views.Controls.AutoWrapCheckBox checkBoxSaveInAppConfig;
        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Label lblPagePrompt;
        private System.Windows.Forms.Button newDBConnectionButton;
        private System.Windows.Forms.ComboBox dataSourceComboBox;
    }
}
