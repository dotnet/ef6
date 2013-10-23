// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    partial class WizardPageUpdateFromDatabase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardPageUpdateFromDatabase));
            this.components = new System.ComponentModel.Container();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.DescriptionTextBox = new System.Windows.Forms.TextBox();
            this.AddUpdateDeleteTabControl = new System.Windows.Forms.TabControl();
            this.AddTabPage = new System.Windows.Forms.TabPage();
            this.tabTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.AddTreeView = new DatabaseObjectTreeView();
            this.chkPluralize = new System.Windows.Forms.CheckBox();
            this.chkIncludeForeignKeys = new System.Windows.Forms.CheckBox();
            this.chkCreateFunctionImports = new System.Windows.Forms.CheckBox();
            this.RefreshTabPage = new System.Windows.Forms.TabPage();
            this.RefreshTreeView = new DatabaseObjectTreeView();
            this.DeleteTabPage = new System.Windows.Forms.TabPage();
            this.DeleteTreeView = new DatabaseObjectTreeView();
            this.toolTip = new System.Windows.Forms.ToolTip();

            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.AddUpdateDeleteTabControl.SuspendLayout();
            this.AddTabPage.SuspendLayout();
            this.tabTableLayoutPanel.SuspendLayout();
            this.RefreshTabPage.SuspendLayout();
            this.DeleteTabPage.SuspendLayout();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList.Images.SetKeyName(0, "DbTables.bmp");
            this.imageList.Images.SetKeyName(1, "Table.bmp");
            this.imageList.Images.SetKeyName(2, "DbViews.bmp");
            this.imageList.Images.SetKeyName(3, "View.bmp");
            this.imageList.Images.SetKeyName(4, "DBStoredProcs.bmp");
            this.imageList.Images.SetKeyName(5, "StoredProc.bmp");
            this.imageList.Images.SetKeyName(6, "DbDeletedItems.bmp");
            this.imageList.Images.SetKeyName(7, "DeletedItem.bmp");
            this.imageList.Images.SetKeyName(8, "DbAddedItems.bmp");
            this.imageList.Images.SetKeyName(9, "DbUpdatedItems.bmp");
            this.imageList.Images.SetKeyName(10, "database_schema.bmp");
            // 
            // DescriptionTextBox
            // 
            this.DescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DescriptionTextBox.Location = new System.Drawing.Point(3, 281);
            this.DescriptionTextBox.Multiline = true;
            this.DescriptionTextBox.Name = "DescriptionTextBox";
            this.DescriptionTextBox.ReadOnly = true;
            this.DescriptionTextBox.Size = new System.Drawing.Size(496, 24);
            this.DescriptionTextBox.TabIndex = 7;
            this.DescriptionTextBox.Text = string.Empty;
            // 
            // AddUpdateDeleteTabControl
            // 
            this.AddUpdateDeleteTabControl.Controls.Add(this.AddTabPage);
            this.AddUpdateDeleteTabControl.Controls.Add(this.RefreshTabPage);
            this.AddUpdateDeleteTabControl.Controls.Add(this.DeleteTabPage);
            this.AddUpdateDeleteTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddUpdateDeleteTabControl.Location = new System.Drawing.Point(3, 3);
            this.AddUpdateDeleteTabControl.Name = "AddUpdateDeleteTabControl";
            this.AddUpdateDeleteTabControl.SelectedIndex = 0;
            this.AddUpdateDeleteTabControl.Size = new System.Drawing.Size(496, 222);
            this.AddUpdateDeleteTabControl.TabIndex = 2;
            // 
            // AddTabPage
            // 
            this.AddTabPage.Controls.Add(this.tabTableLayoutPanel);
            this.AddTabPage.Location = new System.Drawing.Point(4, 22);
            this.AddTabPage.Name = "AddTabPage";
            this.AddTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.AddTabPage.Size = new System.Drawing.Size(488, 196);
            this.AddTabPage.TabIndex = 0;
            this.AddTabPage.Text = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.UpdateFromDatabase_AddTabPageTitle;
            this.AddTabPage.UseVisualStyleBackColor = true;
            this.AddTabPage.Enter += new System.EventHandler(this.AddTabPage_Enter);
            // 
            // tabTableLayoutPanel
            // 
            this.tabTableLayoutPanel.ColumnCount = 1;
            this.tabTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabTableLayoutPanel.Controls.Add(this.AddTreeView, 0, 0);
            this.tabTableLayoutPanel.Controls.Add(this.chkPluralize, 0, 1);
            this.tabTableLayoutPanel.Controls.Add(this.chkIncludeForeignKeys, 0, 2);
            this.tabTableLayoutPanel.Controls.Add(this.chkCreateFunctionImports, 0, 3);
            this.tabTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.tabTableLayoutPanel.Name = "tabTableLayoutPanel";
            this.tabTableLayoutPanel.RowCount = 4;
            this.tabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabTableLayoutPanel.Size = new System.Drawing.Size(482, 210);
                        
            this.tabTableLayoutPanel.TabIndex = 2;
            // 
            // AddTreeView
            // 
            this.AddTreeView.AccessibleName = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.UpdateFromDatabase_AddTreeViewAccessibleName;
            this.AddTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AddTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddTreeView.Location = new System.Drawing.Point(3, 3);
            this.AddTreeView.Name = "AddTreeView";
            this.AddTreeView.Size = new System.Drawing.Size(476, 141);
            this.AddTreeView.TabIndex = 0;
            // 
            // chkPluralize
            //
            this.chkPluralize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkPluralize.Location = new System.Drawing.Point(3, 150);
            this.chkPluralize.Name = "chkPluralize";
            this.chkPluralize.Size = new System.Drawing.Size(476, 19);
            this.chkPluralize.TabIndex = 1;
            this.chkPluralize.Text = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.SelectTables_PluralizeCheckbox;
            this.chkPluralize.UseVisualStyleBackColor = true;
            //
            // chkIncludeForeignKeys
            //
            this.chkIncludeForeignKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkIncludeForeignKeys.Location = new System.Drawing.Point(3, 170);
            this.chkIncludeForeignKeys.Name = "chkIncludeForeignKeys";
            this.chkIncludeForeignKeys.Size = new System.Drawing.Size(476, 19);
            this.chkIncludeForeignKeys.TabIndex = 3;
            this.chkIncludeForeignKeys.Text = Properties.Resources.SelectTablesPage_IncludeForeignKeys;
            this.chkIncludeForeignKeys.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this.chkIncludeForeignKeys, string.Empty);
            //
            // chkCreateFunctionImports
            //
            this.chkCreateFunctionImports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCreateFunctionImports.Location = new System.Drawing.Point(3, 190);
            this.chkCreateFunctionImports.Name = "chkCreateFunctionImports";
            this.chkCreateFunctionImports.Size = new System.Drawing.Size(476, 19);
            this.chkCreateFunctionImports.TabIndex = 4;
            this.chkCreateFunctionImports.Text = Properties.Resources.SelectTablesPage_CreateFunctionImports;
            this.chkCreateFunctionImports.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this.chkCreateFunctionImports, Properties.Resources.CreateFunctionImportsCheckBoxToolTipText);
            // 
            // RefreshTabPage
            // 
            this.RefreshTabPage.Controls.Add(this.RefreshTreeView);
            this.RefreshTabPage.Location = new System.Drawing.Point(4, 22);
            this.RefreshTabPage.Name = "RefreshTabPage";
            this.RefreshTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RefreshTabPage.Size = new System.Drawing.Size(488, 214);
            this.RefreshTabPage.TabIndex = 1;
            this.RefreshTabPage.Text = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.UpdateFromDatabase_RefreshTabPageTitle;
            this.RefreshTabPage.UseVisualStyleBackColor = true;
            this.RefreshTabPage.Enter += new System.EventHandler(this.RefreshTabPage_Enter);
            // 
            // RefreshTreeView
            // 
            this.RefreshTreeView.AccessibleName = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.UpdateFromDatabase_RefreshTreeViewAccessibleName;
            this.RefreshTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RefreshTreeView.Location = new System.Drawing.Point(-5, 6);
            this.RefreshTreeView.Name = "RefreshTreeView";
            this.RefreshTreeView.Size = new System.Drawing.Size(490, 206);
            this.RefreshTreeView.TabIndex = 0;
            // 
            // DeleteTabPage
            // 
            this.DeleteTabPage.Controls.Add(this.DeleteTreeView);
            this.DeleteTabPage.Location = new System.Drawing.Point(4, 22);
            this.DeleteTabPage.Name = "DeleteTabPage";
            this.DeleteTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DeleteTabPage.Size = new System.Drawing.Size(488, 214);
            this.DeleteTabPage.TabIndex = 2;
            this.DeleteTabPage.Text = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.UpdateFromDatabase_DeleteTabPageTitle;
            this.DeleteTabPage.UseVisualStyleBackColor = true;
            this.DeleteTabPage.Enter += new System.EventHandler(this.DeleteTabPage_Enter);
            // 
            // DeleteTreeView
            // 
            this.DeleteTreeView.AccessibleName = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.UpdateFromDatabase_DeleteTreeViewAccessibleName;
            this.DeleteTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DeleteTreeView.Location = new System.Drawing.Point(-5, 6);
            this.DeleteTreeView.Name = "DeleteTreeView";
            this.DeleteTreeView.Size = new System.Drawing.Size(493, 206);
            this.DeleteTreeView.TabIndex = 0;
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainTableLayoutPanel.Controls.Add(this.AddUpdateDeleteTabControl, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.DescriptionTextBox, 0, 2);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 3;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 7F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(501, 324);
            this.mainTableLayoutPanel.TabIndex = 8;
            // 
            // WizardPageUpdateFromDatabase
            // 

            //Below two values are added by designer by default.
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "WizardPageUpdateFromDatabase";
            this.Size = new System.Drawing.Size(501, 324);
            this.AddUpdateDeleteTabControl.ResumeLayout(false);
            this.AddTabPage.ResumeLayout(false);
            this.tabTableLayoutPanel.ResumeLayout(false);
            this.RefreshTabPage.ResumeLayout(false);
            this.DeleteTabPage.ResumeLayout(false);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TextBox DescriptionTextBox;
        private System.Windows.Forms.TabControl AddUpdateDeleteTabControl;
        private System.Windows.Forms.TabPage AddTabPage;
        private System.Windows.Forms.TabPage RefreshTabPage;
        private System.Windows.Forms.TabPage DeleteTabPage;
        private DatabaseObjectTreeView RefreshTreeView;
        private DatabaseObjectTreeView DeleteTreeView;
        private System.Windows.Forms.TableLayoutPanel tabTableLayoutPanel;
        private DatabaseObjectTreeView AddTreeView;
        private System.Windows.Forms.CheckBox chkPluralize;
        private System.Windows.Forms.CheckBox chkIncludeForeignKeys;
        private System.Windows.Forms.CheckBox chkCreateFunctionImports;
        private System.Windows.Forms.ToolTip toolTip;

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
    }
}
