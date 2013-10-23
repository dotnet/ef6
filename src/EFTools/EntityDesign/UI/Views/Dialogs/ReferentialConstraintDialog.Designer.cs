// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    partial class ReferentialConstraintDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReferentialConstraintDialog));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdPrincipalRole = new System.Windows.Forms.ComboBox();
            this.cmbDependentKey = new System.Windows.Forms.ComboBox();
            this.listMappings = new Microsoft.Data.Entity.Design.UI.Views.Dialogs.ReferentialConstraintListView();
            this.hdrPrincipal = new System.Windows.Forms.ColumnHeader();
            this.hdrDepedent = new System.Windows.Forms.ColumnHeader();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDependentRole = new System.Windows.Forms.TextBox();
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.mainTableLayout.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.OKButton_AccessibleName;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnOK.Name = "btnOK";
            this.btnOK.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.OKButton_Text;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleName = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_AccessibleName;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.CancelButton_Text;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnDelete.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.DeleteButton_Text;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.MinimumSize = new System.Drawing.Size(50, 13);
            this.label1.Name = "label1";
            // 
            // cmdPrincipalRole
            // 
            resources.ApplyResources(this.cmdPrincipalRole, "cmdPrincipalRole");
            this.cmdPrincipalRole.FormattingEnabled = true;
            this.cmdPrincipalRole.Name = "cmdPrincipalRole";
            this.cmdPrincipalRole.Sorted = true;
            this.cmdPrincipalRole.SelectedIndexChanged += new System.EventHandler(this.cmdPrincipalRole_SelectedIndexChanged);
            // 
            // cmbDependentKey
            // 
            resources.ApplyResources(this.cmbDependentKey, "cmbDependentKey");
            this.cmbDependentKey.FormattingEnabled = true;
            this.cmbDependentKey.Name = "cmbDependentKey";
            this.cmbDependentKey.Sorted = true;
            this.cmbDependentKey.SelectionChangeCommitted += new System.EventHandler(this.cmbDependentKey_SelectionChangeCommitted);
            this.cmbDependentKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbDependentKey_KeyPress);
            this.cmbDependentKey.Leave += new System.EventHandler(this.cmbDependentKey_Leave);
            // 
            // listMappings
            // 
            resources.ApplyResources(this.listMappings, "listMappings");
            this.listMappings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrPrincipal,
            this.hdrDepedent});
            this.listMappings.FullRowSelect = true;
            this.listMappings.GridLines = true;
            this.listMappings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listMappings.HideSelection = false;
            this.listMappings.MultiSelect = false;
            this.listMappings.Name = "listMappings";
            this.listMappings.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listMappings.UseCompatibleStateImageBehavior = false;
            this.listMappings.View = System.Windows.Forms.View.Details;
            this.listMappings.SelectedIndexChanged += new System.EventHandler(this.listMappings_SelectedIndexChanged);
            this.listMappings.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listMappings_KeyDown);
            this.listMappings.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listMappings_MouseUp);
            // 
            // hdrPrincipal
            // 
            this.hdrPrincipal.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.RefConstraintDialog_PrincipalKeyHeader;
            resources.ApplyResources(this.hdrPrincipal, "hdrPrincipal");
            // 
            // hdrDepedent
            // 
            this.hdrDepedent.Text = global::Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource.RefConstraintDialog_DependentKeyHeader;
            resources.ApplyResources(this.hdrDepedent, "hdrDepedent");
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.MinimumSize = new System.Drawing.Size(63, 13);
            this.label2.Name = "label2";
            // 
            // txtDependentRole
            // 
            resources.ApplyResources(this.txtDependentRole, "txtDependentRole");
            this.txtDependentRole.Name = "txtDependentRole";
            this.txtDependentRole.ReadOnly = true;
            this.txtDependentRole.TabStop = false;
            // 
            // mainTableLayout
            // 
            resources.ApplyResources(this.mainTableLayout, "mainTableLayout");
            this.mainTableLayout.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.mainTableLayout.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.mainTableLayout.Name = "mainTableLayout";
            // 
            // flowLayoutPanel2
            // 
            resources.ApplyResources(this.flowLayoutPanel2, "flowLayoutPanel2");
            this.flowLayoutPanel2.Controls.Add(this.btnOK);
            this.flowLayoutPanel2.Controls.Add(this.btnDelete);
            this.flowLayoutPanel2.Controls.Add(this.btnCancel);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.cmdPrincipalRole);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.txtDependentRole);
            this.flowLayoutPanel1.Controls.Add(this.listMappings);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // ReferentialConstraintDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.mainTableLayout);
            this.Controls.Add(this.cmbDependentKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReferentialConstraintDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.ReferentialConstraintDialog_Load);
            this.mainTableLayout.ResumeLayout(false);
            this.mainTableLayout.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmdPrincipalRole;
        private System.Windows.Forms.ComboBox cmbDependentKey;
        private System.Windows.Forms.ColumnHeader hdrPrincipal;
        private System.Windows.Forms.ColumnHeader hdrDepedent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDependentRole;
        private ReferentialConstraintListView listMappings;
        private System.Windows.Forms.TableLayoutPanel mainTableLayout;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}