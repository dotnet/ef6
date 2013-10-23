// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System.Diagnostics.CodeAnalysis;

    partial class WizardPageSelectTables
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
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.ComponentModel.ComponentResourceManager")]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "resources")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardPageSelectTables));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.databaseObjectTreeView = new DatabaseObjectTreeView();
            this.labelPrompt = new System.Windows.Forms.Label();
            this.modelNamespaceLabel = new System.Windows.Forms.Label();
            this.modelNamespaceTextBox = new System.Windows.Forms.TextBox();
            this.chkPluralize = new System.Windows.Forms.CheckBox();
            this.chkIncludeForeignKeys = new System.Windows.Forms.CheckBox();
            this.chkCreateFunctionImports = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip();
            this.SuspendLayout();
            // 
            // infoPanel
            // 
            this.infoPanel.Size = new System.Drawing.Size(500, 268);
            // 
            // treeView
            // 
            this.databaseObjectTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseObjectTreeView.Location = new System.Drawing.Point(0, 19);
            this.databaseObjectTreeView.Name = "treeView";
            this.databaseObjectTreeView.Size = new System.Drawing.Size(497, 160);
            this.databaseObjectTreeView.TabIndex = 1;
            // 
            // labelPrompt
            // 
            this.labelPrompt.AutoSize = true;
            this.labelPrompt.Font = new System.Drawing.Font(this.Font.FontFamily, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrompt.Location = new System.Drawing.Point(0, 0);
            this.labelPrompt.Name = "labelPrompt";
            this.labelPrompt.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.labelPrompt.Size = new System.Drawing.Size(303, 18);
            this.labelPrompt.TabIndex = 0;
            this.labelPrompt.Text = Properties.Resources.WhichDatabaseObjectsLabel;
            // 
            // modelNamespaceLabel
            // 
            this.modelNamespaceLabel.AutoSize = true;
            this.modelNamespaceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.modelNamespaceLabel.Location = new System.Drawing.Point(0, 246);
            this.modelNamespaceLabel.Name = "modelNamespaceLabel";
            this.modelNamespaceLabel.Size = new System.Drawing.Size(100, 18);
            this.modelNamespaceLabel.TabIndex = 3; 
            this.modelNamespaceLabel.Text = Properties.Resources.SelectTablesPage_ModelNamespaceLabel;
            // 
            // modelNamespaceTextBox
            // 
            this.modelNamespaceTextBox.AccessibleName = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.SelectTablesPage_ModelNamespaceAccessibleName;
            this.modelNamespaceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.modelNamespaceTextBox.Location = new System.Drawing.Point(0, 264);
            this.modelNamespaceTextBox.Name = "modelNamespaceTextBox";
            this.modelNamespaceTextBox.Size = new System.Drawing.Size(324, 20);
            this.modelNamespaceTextBox.TabIndex = 5;
            // 
            // chkPluralize
            // 
            this.chkPluralize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkPluralize.Location = new System.Drawing.Point(0,182);
            this.chkPluralize.Name = "chkPluralize";
            this.chkPluralize.Size = new System.Drawing.Size(497, 17);
            this.chkPluralize.TabIndex = 2;
            this.chkPluralize.Text = Properties.Resources.SelectTables_PluralizeCheckbox;
            this.chkPluralize.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this.chkPluralize, Properties.Resources.PluralizeCheckBoxToolTipText);
            // 
            // chkIncludeForeignKeys
            // 
            this.chkIncludeForeignKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkIncludeForeignKeys.Location = new System.Drawing.Point(0, 202);
            this.chkIncludeForeignKeys.Name = "chkIncludeForeignKeys";
            this.chkIncludeForeignKeys.Size = new System.Drawing.Size(497, 17);
            this.chkIncludeForeignKeys.TabIndex = 3;
            this.chkIncludeForeignKeys.Text = Properties.Resources.SelectTablesPage_IncludeForeignKeys;
            this.chkIncludeForeignKeys.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this.chkIncludeForeignKeys, string.Empty);
            // 
            // chkCreateFunctionImports
            // 
            this.chkCreateFunctionImports.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkCreateFunctionImports.Location = new System.Drawing.Point(0, 222);
            this.chkCreateFunctionImports.Name = "chkCreateFunctionImports";
            this.chkCreateFunctionImports.Size = new System.Drawing.Size(497, 17);
            this.chkCreateFunctionImports.TabIndex = 4;
            this.chkCreateFunctionImports.Text = Properties.Resources.SelectTablesPage_CreateFunctionImports;
            this.chkCreateFunctionImports.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this.chkCreateFunctionImports, Properties.Resources.CreateFunctionImportsCheckBoxToolTipText);
            // 
            // WizardPageSelectTables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.modelNamespaceTextBox);
            this.Controls.Add(this.modelNamespaceLabel);
            this.Controls.Add(this.databaseObjectTreeView);
            this.Controls.Add(this.labelPrompt);
            this.Controls.Add(this.chkPluralize);
            this.Controls.Add(this.chkIncludeForeignKeys);
            this.Controls.Add(this.chkCreateFunctionImports);
            this.Name = "WizardPageSelectTables";
            this.Size = new System.Drawing.Size(500, 288);
            this.Controls.SetChildIndex(this.infoPanel, 0);
            this.Controls.SetChildIndex(this.chkPluralize, 0);
            this.Controls.SetChildIndex(this.chkIncludeForeignKeys, 0);
            this.Controls.SetChildIndex(this.chkCreateFunctionImports, 0);
            this.Controls.SetChildIndex(this.labelPrompt, 0);
            this.Controls.SetChildIndex(this.databaseObjectTreeView, 0);
            this.Controls.SetChildIndex(this.modelNamespaceLabel, 0);
            this.Controls.SetChildIndex(this.modelNamespaceTextBox, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Forms.ImageList imageList;
        private DatabaseObjectTreeView databaseObjectTreeView;
        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.Label modelNamespaceLabel;
        private System.Windows.Forms.TextBox modelNamespaceTextBox;
        private System.Windows.Forms.CheckBox chkPluralize;
        private System.Windows.Forms.CheckBox chkIncludeForeignKeys;
        private System.Windows.Forms.CheckBox chkCreateFunctionImports;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
