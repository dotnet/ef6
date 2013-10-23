// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System.Windows.Forms;
using Microsoft.Data.Entity.Design.UI.Views.Controls;

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    partial class NewFunctionImportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewFunctionImportDialog));
            this.functionImportNameTextBox = new System.Windows.Forms.TextBox();
            this.functionImportLabel = new System.Windows.Forms.Label();
            this.functionImportComposableCheckBox = new System.Windows.Forms.CheckBox();
            this.storedProcComboBox = new System.Windows.Forms.ComboBox();
            this.storedProcNameLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.buttonsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.returnsCollectionGroupBox = new System.Windows.Forms.GroupBox();
            this.returnsCollectionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.emptyReturnTypeButton = new ArrowOverrideRadioButton();
            this.entityTypeReturnButton = new ArrowOverrideRadioButton();
            this.scalarTypeReturnButton = new ArrowOverrideRadioButton();
            this.scalarTypeReturnComboBox = new System.Windows.Forms.ComboBox();
            this.entityTypeReturnComboBox = new System.Windows.Forms.ComboBox();
            this.complexTypeReturnButton = new ArrowOverrideRadioButton();
            this.complexTypeReturnComboBox = new System.Windows.Forms.ComboBox();
            this.updateComplexTypeButton = new System.Windows.Forms.Button();
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.returnTypeShapeGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.returnTypeShapePanel = new System.Windows.Forms.Panel();
            this.returnTypeShapeTabControl = new Microsoft.Data.Entity.Design.UI.Views.Dialogs.NoHeaderTabControl();
            this.getColumnInformationButton = new System.Windows.Forms.Button();
            this.createNewComplexTypeButton = new System.Windows.Forms.Button();
            this.selectTabPage = new System.Windows.Forms.TabPage();
            this.selectTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.selectLabel = new System.Windows.Forms.Label();
            this.detectingTabPage = new System.Windows.Forms.TabPage();
            this.detectingTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.detectingLabel = new System.Windows.Forms.Label();
            this.detectTabPage = new System.Windows.Forms.TabPage();
            this.detectTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.detectLabel = new System.Windows.Forms.Label();
            this.detectErrorTabPage = new System.Windows.Forms.TabPage();
            this.detectErrorTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.detectErrorLabel = new System.Windows.Forms.Label();
            this.connectionTabPage = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.returnTypeShapeListView = new System.Windows.Forms.ListView();
            this.columnAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEdmType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDbType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnNullable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnMaxLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnPrecision = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnScale = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonsTableLayout.SuspendLayout();
            this.returnsCollectionGroupBox.SuspendLayout();
            this.returnsCollectionTableLayoutPanel.SuspendLayout();
            this.mainTableLayout.SuspendLayout();
            this.returnTypeShapeGroup.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.returnTypeShapePanel.SuspendLayout();
            this.returnTypeShapeTabControl.SuspendLayout();
            this.selectTabPage.SuspendLayout();
            this.selectTableLayoutPanel.SuspendLayout();
            this.detectingTabPage.SuspendLayout();
            this.detectingTableLayoutPanel.SuspendLayout();
            this.detectTabPage.SuspendLayout();
            this.detectTableLayoutPanel.SuspendLayout();
            this.detectErrorTabPage.SuspendLayout();
            this.detectErrorTableLayoutPanel.SuspendLayout();
            this.connectionTabPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // functionImportNameTextBox
            // 
            resources.ApplyResources(this.functionImportNameTextBox, "functionImportNameTextBox");
            this.functionImportNameTextBox.Name = "functionImportNameTextBox";
            this.functionImportNameTextBox.TextChanged += new System.EventHandler(this.functionImportNameTextBox_TextChanged);
            // 
            // functionImportLabel
            // 
            resources.ApplyResources(this.functionImportLabel, "functionImportLabel");
            this.functionImportLabel.Name = "functionImportLabel";
            // 
            // functionImportComposableCheckBox
            // 
            resources.ApplyResources(this.functionImportComposableCheckBox, "functionImportComposableCheckBox");
            this.functionImportComposableCheckBox.Name = "functionImportComposableCheckBox";
            this.functionImportComposableCheckBox.Click += new System.EventHandler(this.functionImportComposableCheckBox_Click);
            // 
            // storedProcComboBox
            // 
            resources.ApplyResources(this.storedProcComboBox, "storedProcComboBox");
            this.storedProcComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.storedProcComboBox.FormattingEnabled = true;
            this.storedProcComboBox.Name = "storedProcComboBox";
            this.storedProcComboBox.Sorted = true;
            this.storedProcComboBox.SelectedIndexChanged += new System.EventHandler(this.storedProcComboBox_SelectedIndexChanged);
            // 
            // storedProcNameLabel
            // 
            resources.ApplyResources(this.storedProcNameLabel, "storedProcNameLabel");
            this.storedProcNameLabel.Name = "storedProcNameLabel";
            // 
            // cancelButton
            // 
            this.cancelButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_AccessibleName;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.OKButton_AccessibleName;
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // buttonsTableLayout
            // 
            resources.ApplyResources(this.buttonsTableLayout, "buttonsTableLayout");
            this.buttonsTableLayout.Controls.Add(this.okButton, 0, 0);
            this.buttonsTableLayout.Controls.Add(this.cancelButton, 1, 0);
            this.buttonsTableLayout.Name = "buttonsTableLayout";
            // 
            // returnsCollectionGroupBox
            // 
            resources.ApplyResources(this.returnsCollectionGroupBox, "returnsCollectionGroupBox");
            this.returnsCollectionGroupBox.Controls.Add(this.returnsCollectionTableLayoutPanel);
            this.returnsCollectionGroupBox.Name = "returnsCollectionGroupBox";
            this.returnsCollectionGroupBox.TabStop = false;
            // 
            // returnsCollectionTableLayoutPanel
            // 
            resources.ApplyResources(this.returnsCollectionTableLayoutPanel, "returnsCollectionTableLayoutPanel");
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.emptyReturnTypeButton, 0, 0);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.scalarTypeReturnButton, 0, 1);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.scalarTypeReturnComboBox, 1, 1);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.complexTypeReturnButton, 0, 2);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.complexTypeReturnComboBox, 1, 2);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.updateComplexTypeButton, 2, 2);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.entityTypeReturnButton, 0, 3);
            this.returnsCollectionTableLayoutPanel.Controls.Add(this.entityTypeReturnComboBox, 1, 3);
            this.returnsCollectionTableLayoutPanel.Name = "returnsCollectionTableLayoutPanel";
            // 
            // emptyReturnTypeButton
            // 
            resources.ApplyResources(this.emptyReturnTypeButton, "emptyReturnTypeButton");
            this.emptyReturnTypeButton.Name = "emptyReturnTypeButton";
            this.emptyReturnTypeButton.TabStop = true;
            this.emptyReturnTypeButton.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewFunctionImportDialog_EmptyReturnTypeLabel;
            this.emptyReturnTypeButton.UseVisualStyleBackColor = true;
            this.emptyReturnTypeButton.CheckedChanged += new System.EventHandler(this.returnTypeButton_CheckedChanged);
            this.emptyReturnTypeButton.ArrowPressed += this.returnTypeButton_ArrowPressed;
            // 
            // scalarTypeReturnButton
            // 
            resources.ApplyResources(this.scalarTypeReturnButton, "scalarTypeReturnButton");
            this.scalarTypeReturnButton.Name = "scalarTypeReturnButton";
            this.scalarTypeReturnButton.TabStop = true;
            this.scalarTypeReturnButton.UseVisualStyleBackColor = true;
            this.scalarTypeReturnButton.CheckedChanged += new System.EventHandler(this.returnTypeButton_CheckedChanged);
            this.scalarTypeReturnButton.ArrowPressed += this.returnTypeButton_ArrowPressed;
            // 
            // scalarTypeReturnComboBox
            // 
            this.scalarTypeReturnComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewFunctionImportDialog_ScalarsTypeReturnAccessibleName;
            resources.ApplyResources(this.scalarTypeReturnComboBox, "scalarTypeReturnComboBox");
            this.scalarTypeReturnComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scalarTypeReturnComboBox.FormattingEnabled = true;
            this.scalarTypeReturnComboBox.Name = "scalarTypeReturnComboBox";
            this.scalarTypeReturnComboBox.Sorted = true;
            this.scalarTypeReturnComboBox.SelectedIndexChanged += new System.EventHandler(this.scalarTypeReturnComboBox_SelectedIndexChanged);
            // 
            // complexTypeReturnButton
            // 
            resources.ApplyResources(this.complexTypeReturnButton, "complexTypeReturnButton");
            this.complexTypeReturnButton.Name = "complexTypeReturnButton";
            this.complexTypeReturnButton.TabStop = true;
            this.complexTypeReturnButton.UseVisualStyleBackColor = true;
            this.complexTypeReturnButton.ArrowPressed += this.returnTypeButton_ArrowPressed;
            // 
            // complexTypeReturnComboBox
            // 
            this.complexTypeReturnComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewFunctionImportDialog_ComplexTypeReturnAccessibleName;
            resources.ApplyResources(this.complexTypeReturnComboBox, "complexTypeReturnComboBox");
            this.complexTypeReturnComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.complexTypeReturnComboBox.FormattingEnabled = true;
            this.complexTypeReturnComboBox.Name = "complexTypeReturnComboBox";
            this.complexTypeReturnComboBox.SelectedIndexChanged += new System.EventHandler(this.complexTypeReturnComboBox_SelectedIndexChanged);
            // 
            // updateComplexTypeButton
            // 
            resources.ApplyResources(this.updateComplexTypeButton, "updateComplexTypeButton");
            this.updateComplexTypeButton.Name = "updateComplexTypeButton";
            this.updateComplexTypeButton.UseVisualStyleBackColor = true;
            this.updateComplexTypeButton.Click += new System.EventHandler(this.updateComplexTypeButton_Click);
            // 
            // entityTypeReturnButton
            // 
            resources.ApplyResources(this.entityTypeReturnButton, "entityTypeReturnButton");
            this.entityTypeReturnButton.Name = "entityTypeReturnButton";
            this.entityTypeReturnButton.TabStop = true;
            this.entityTypeReturnButton.UseVisualStyleBackColor = true;
            this.entityTypeReturnButton.CheckedChanged += new System.EventHandler(this.returnTypeButton_CheckedChanged);
            this.entityTypeReturnButton.ArrowPressed += this.returnTypeButton_ArrowPressed;
            // 
            // entityTypeReturnComboBox
            // 
            this.entityTypeReturnComboBox.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.NewFunctionImportDialog_EntityTypeReturnAccessibleName;
            resources.ApplyResources(this.entityTypeReturnComboBox, "entityTypeReturnComboBox");
            this.entityTypeReturnComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.entityTypeReturnComboBox.FormattingEnabled = true;
            this.entityTypeReturnComboBox.Name = "entityTypeReturnComboBox";
            this.entityTypeReturnComboBox.SelectedIndexChanged += new System.EventHandler(this.entityTypeReturnComboBox_SelectedIndexChanged);
            // 
            // mainTableLayout
            // 
            resources.ApplyResources(this.mainTableLayout, "mainTableLayout");
            this.mainTableLayout.Controls.Add(this.functionImportLabel, 0, 0);
            this.mainTableLayout.Controls.Add(this.functionImportNameTextBox, 0, 1);
            this.mainTableLayout.Controls.Add(this.functionImportComposableCheckBox, 0, 2);
            this.mainTableLayout.Controls.Add(this.storedProcNameLabel, 0, 4);
            this.mainTableLayout.Controls.Add(this.storedProcComboBox, 0, 5);
            this.mainTableLayout.Controls.Add(this.returnsCollectionGroupBox, 0, 7);
            this.mainTableLayout.Controls.Add(this.returnTypeShapeGroup, 0, 9);
            this.mainTableLayout.Name = "mainTableLayout";
            // 
            // returnTypeShapeGroup
            // 
            resources.ApplyResources(this.returnTypeShapeGroup, "returnTypeShapeGroup");
            this.returnTypeShapeGroup.Controls.Add(this.tableLayoutPanel2);
            this.returnTypeShapeGroup.Name = "returnTypeShapeGroup";
            this.returnTypeShapeGroup.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.getColumnInformationButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.returnTypeShapePanel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.createNewComplexTypeButton, 0, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // returnTypeShapePanel
            // 
            resources.ApplyResources(this.returnTypeShapePanel, "returnTypeShapePanel");
            this.returnTypeShapePanel.BackColor = System.Drawing.SystemColors.Window;
            this.returnTypeShapePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.returnTypeShapePanel.Controls.Add(this.returnTypeShapeTabControl);
            this.returnTypeShapePanel.Controls.Add(this.returnTypeShapeListView);
            this.returnTypeShapePanel.Name = "returnTypeShapePanel";
            //
            // returnTypeShapeTabControl
            //
            this.returnTypeShapeTabControl.Controls.Add(this.selectTabPage);
            this.returnTypeShapeTabControl.Controls.Add(this.detectingTabPage);
            this.returnTypeShapeTabControl.Controls.Add(this.detectTabPage);
            this.returnTypeShapeTabControl.Controls.Add(this.detectErrorTabPage);
            this.returnTypeShapeTabControl.Controls.Add(this.connectionTabPage);
            resources.ApplyResources(this.returnTypeShapeTabControl, "returnTypeShapeTabControl");
            this.returnTypeShapeTabControl.Name = "returnTypeShapeTabControl";
            this.returnTypeShapeTabControl.SelectedIndex = 0;
            this.returnTypeShapeTabControl.SimpleMode = true;
            this.returnTypeShapeTabControl.SimpleModeInDesign = true;
            this.returnTypeShapeTabControl.TabStop = false;
            // 
            // getColumnInformationButton
            // 
            resources.ApplyResources(this.getColumnInformationButton, "getColumnInformationButton");
            this.getColumnInformationButton.Name = "getColumnInformationButton";
            this.getColumnInformationButton.UseVisualStyleBackColor = true;
            this.getColumnInformationButton.Click += new System.EventHandler(this.getColumnInformationButton_Click);
            // 
            // createNewComplexTypeButton
            // 
            resources.ApplyResources(this.createNewComplexTypeButton, "createNewComplexTypeButton");
            this.createNewComplexTypeButton.Name = "createNewComplexTypeButton";
            this.createNewComplexTypeButton.UseVisualStyleBackColor = true;
            this.createNewComplexTypeButton.Click += new System.EventHandler(this.createNewComplexTypeButton_Click);
            // 
            // selectTabPage
            // 
            this.selectTabPage.Controls.Add(this.selectTableLayoutPanel);
            resources.ApplyResources(this.selectTabPage, "selectTabPage");
            this.selectTabPage.Name = "selectTabPage";
            this.selectTabPage.UseVisualStyleBackColor = true;
            // 
            // selectTableLayoutPanel
            // 
            resources.ApplyResources(this.selectTableLayoutPanel, "selectTableLayoutPanel");
            this.selectTableLayoutPanel.Controls.Add(this.selectLabel, 0, 0);
            this.selectTableLayoutPanel.Name = "selectTableLayoutPanel";
            // 
            // selectLabel
            // 
            resources.ApplyResources(this.selectLabel, "selectLabel");
            this.selectLabel.Name = "selectLabel";
            // 
            // detectingTabPage
            // 
            this.detectingTabPage.Controls.Add(this.detectingTableLayoutPanel);
            resources.ApplyResources(this.detectingTabPage, "detectingTabPage");
            this.detectingTabPage.Name = "detectingTabPage";
            this.detectingTabPage.UseVisualStyleBackColor = true;
            // 
            // detectingTableLayoutPanel
            // 
            resources.ApplyResources(this.detectingTableLayoutPanel, "detectingTableLayoutPanel");
            this.detectingTableLayoutPanel.Controls.Add(this.detectingLabel, 0, 0);
            this.detectingTableLayoutPanel.Name = "detectingTableLayoutPanel";
            // 
            // detectingLabel
            // 
            resources.ApplyResources(this.detectingLabel, "detectingLabel");
            this.detectingLabel.Name = "detectingLabel";
            // 
            // detectTabPage
            // 
            this.detectTabPage.Controls.Add(this.detectTableLayoutPanel);
            resources.ApplyResources(this.detectTabPage, "detectTabPage");
            this.detectTabPage.Name = "detectTabPage";
            this.detectTabPage.UseVisualStyleBackColor = true;
            // 
            // detectTableLayoutPanel
            // 
            resources.ApplyResources(this.detectTableLayoutPanel, "detectTableLayoutPanel");
            this.detectTableLayoutPanel.Controls.Add(this.detectLabel, 0, 0);
            this.detectTableLayoutPanel.Name = "detectTableLayoutPanel";
            // 
            // detectLabel
            // 
            this.detectLabel.AutoEllipsis = true;
            resources.ApplyResources(this.detectLabel, "detectLabel");
            this.detectLabel.Name = "detectLabel";
            // 
            // detectErrorTabPage
            // 
            this.detectErrorTabPage.Controls.Add(this.detectErrorTableLayoutPanel);
            resources.ApplyResources(this.detectErrorTabPage, "detectErrorTabPage");
            this.detectErrorTabPage.Name = "detectErrorTabPage";
            this.detectErrorTabPage.UseVisualStyleBackColor = true;
            // 
            // detectErrorTableLayoutPanel
            // 
            resources.ApplyResources(this.detectErrorTableLayoutPanel, "detectErrorTableLayoutPanel");
            this.detectErrorTableLayoutPanel.Controls.Add(this.detectErrorLabel, 0, 0);
            this.detectErrorTableLayoutPanel.Name = "detectErrorTableLayoutPanel";
            // 
            // detectErrorLabel
            // 
            this.detectErrorLabel.AutoEllipsis = true;
            resources.ApplyResources(this.detectErrorLabel, "detectErrorLabel");
            this.detectErrorLabel.Name = "detectErrorLabel";
            // 
            // connectionTabPage
            // 
            this.connectionTabPage.Controls.Add(this.tableLayoutPanel1);
            resources.ApplyResources(this.connectionTabPage, "connectionTabPage");
            this.connectionTabPage.Name = "connectionTabPage";
            this.connectionTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // returnTypeShapeListView
            // 
            this.returnTypeShapeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnAction,
            this.columnName,
            this.columnEdmType,
            this.columnDbType,
            this.columnNullable,
            this.columnMaxLength,
            this.columnPrecision,
            this.columnScale});
            resources.ApplyResources(this.returnTypeShapeListView, "returnTypeShapeListView");
            this.returnTypeShapeListView.FullRowSelect = true;
            this.returnTypeShapeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.returnTypeShapeListView.MultiSelect = false;
            this.returnTypeShapeListView.Name = "returnTypeShapeListView";
            this.returnTypeShapeListView.UseCompatibleStateImageBehavior = false;
            this.returnTypeShapeListView.View = System.Windows.Forms.View.Details;
            // 
            // columnAction
            // 
            resources.ApplyResources(this.columnAction, "columnAction");
            // 
            // columnName
            // 
            resources.ApplyResources(this.columnName, "columnName");
            // 
            // columnEdmType
            // 
            resources.ApplyResources(this.columnEdmType, "columnEdmType");
            // 
            // columnDbType
            // 
            resources.ApplyResources(this.columnDbType, "columnDbType");
            // 
            // columnNullable
            // 
            resources.ApplyResources(this.columnNullable, "columnNullable");
            // 
            // columnMaxLength
            // 
            resources.ApplyResources(this.columnMaxLength, "columnMaxLength");
            // 
            // columnPrecision
            // 
            resources.ApplyResources(this.columnPrecision, "columnPrecision");
            // 
            // columnScale
            // 
            resources.ApplyResources(this.columnScale, "columnScale");
            // 
            // NewFunctionImportDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.mainTableLayout);
            this.Controls.Add(this.buttonsTableLayout);
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewFunctionImportDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.buttonsTableLayout.ResumeLayout(false);
            this.buttonsTableLayout.PerformLayout();
            this.returnsCollectionGroupBox.ResumeLayout(false);
            this.returnsCollectionGroupBox.PerformLayout();
            this.returnsCollectionTableLayoutPanel.ResumeLayout(false);
            this.returnsCollectionTableLayoutPanel.PerformLayout();
            this.mainTableLayout.ResumeLayout(false);
            this.mainTableLayout.PerformLayout();
            this.returnTypeShapeGroup.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.returnTypeShapePanel.ResumeLayout(false);
            this.returnTypeShapeTabControl.ResumeLayout(false);
            this.selectTabPage.ResumeLayout(false);
            this.selectTableLayoutPanel.ResumeLayout(false);
            this.selectTableLayoutPanel.PerformLayout();
            this.detectingTabPage.ResumeLayout(false);
            this.detectingTableLayoutPanel.ResumeLayout(false);
            this.detectingTableLayoutPanel.PerformLayout();
            this.detectTabPage.ResumeLayout(false);
            this.detectTableLayoutPanel.ResumeLayout(false);
            this.detectTableLayoutPanel.PerformLayout();
            this.detectErrorTabPage.ResumeLayout(false);
            this.detectErrorTableLayoutPanel.ResumeLayout(false);
            this.detectErrorTableLayoutPanel.PerformLayout();
            this.connectionTabPage.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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

        private System.Windows.Forms.TextBox functionImportNameTextBox;
        private System.Windows.Forms.Label functionImportLabel;
        private System.Windows.Forms.CheckBox functionImportComposableCheckBox;
        private System.Windows.Forms.ComboBox storedProcComboBox;
        private System.Windows.Forms.Label storedProcNameLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TableLayoutPanel buttonsTableLayout;
        private System.Windows.Forms.GroupBox returnsCollectionGroupBox;
        private System.Windows.Forms.TableLayoutPanel returnsCollectionTableLayoutPanel;
        private System.Windows.Forms.ComboBox entityTypeReturnComboBox;
        private ArrowOverrideRadioButton entityTypeReturnButton;
        private ArrowOverrideRadioButton scalarTypeReturnButton;
        private ArrowOverrideRadioButton emptyReturnTypeButton;
        private System.Windows.Forms.ComboBox scalarTypeReturnComboBox;
        private ArrowOverrideRadioButton complexTypeReturnButton;
        private System.Windows.Forms.ComboBox complexTypeReturnComboBox;
        private System.Windows.Forms.TableLayoutPanel mainTableLayout;
        private System.Windows.Forms.GroupBox returnTypeShapeGroup;
        private System.Windows.Forms.Panel returnTypeShapePanel;
        private System.Windows.Forms.ListView returnTypeShapeListView;
        private System.Windows.Forms.ColumnHeader columnAction;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnEdmType;
        private System.Windows.Forms.ColumnHeader columnDbType;
        private System.Windows.Forms.ColumnHeader columnNullable;
        private System.Windows.Forms.ColumnHeader columnMaxLength;
        private System.Windows.Forms.ColumnHeader columnPrecision;
        private System.Windows.Forms.ColumnHeader columnScale;
        private NoHeaderTabControl returnTypeShapeTabControl;
        private System.Windows.Forms.TabPage selectTabPage;
        private System.Windows.Forms.TabPage detectingTabPage;
        private System.Windows.Forms.TableLayoutPanel selectTableLayoutPanel;
        private System.Windows.Forms.Label selectLabel;
        private System.Windows.Forms.TabPage detectTabPage;
        private System.Windows.Forms.TableLayoutPanel detectTableLayoutPanel;
        private System.Windows.Forms.Label detectLabel;
        private System.Windows.Forms.TabPage detectErrorTabPage;
        private System.Windows.Forms.TableLayoutPanel detectErrorTableLayoutPanel;
        private System.Windows.Forms.Label detectErrorLabel;
        private System.Windows.Forms.TableLayoutPanel detectingTableLayoutPanel;
        private System.Windows.Forms.Label detectingLabel;
        private System.Windows.Forms.TabPage connectionTabPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button updateComplexTypeButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button getColumnInformationButton;
        private System.Windows.Forms.Button createNewComplexTypeButton;
    }
}
