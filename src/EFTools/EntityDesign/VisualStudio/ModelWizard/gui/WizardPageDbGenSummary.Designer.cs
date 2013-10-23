// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System.Diagnostics.CodeAnalysis;

    partial class WizardPageDbGenSummary
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardPageDbGenSummary));
            System.Windows.Forms.TabPage CreateDatabaseTabDDL;
            System.Windows.Forms.Label lblSaveDdlAs;
            this.txtDDL = new System.Windows.Forms.TextBox();
            this.SummaryTabs = new System.Windows.Forms.TabControl();
            this.txtSaveDdlAs = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            CreateDatabaseTabDDL = new System.Windows.Forms.TabPage();
            lblSaveDdlAs = new System.Windows.Forms.Label();
            CreateDatabaseTabDDL.SuspendLayout();
            this.SummaryTabs.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // infoPanel
            // 
            resources.ApplyResources(this.infoPanel, "infoPanel");
            // 
            // CreateDatabaseTabDDL
            // 
            CreateDatabaseTabDDL.Controls.Add(this.txtDDL);
            resources.ApplyResources(CreateDatabaseTabDDL, "CreateDatabaseTabDDL");
            CreateDatabaseTabDDL.Name = "CreateDatabaseTabDDL";
            CreateDatabaseTabDDL.UseVisualStyleBackColor = true;
            // 
            // txtDDL
            // 
            resources.ApplyResources(this.txtDDL, "txtDDL");
            this.txtDDL.BackColor = System.Drawing.SystemColors.Window;
            this.txtDDL.Name = "txtDDL";
            this.txtDDL.ReadOnly = true;
            // 
            // lblSaveDdlAs
            // 
            resources.ApplyResources(lblSaveDdlAs, "lblSaveDdlAs");
            lblSaveDdlAs.Name = "lblSaveDdlAs";
            // 
            // SummaryTabs
            // 
            resources.ApplyResources(this.SummaryTabs, "SummaryTabs");
            this.SummaryTabs.Controls.Add(CreateDatabaseTabDDL);
            this.SummaryTabs.Name = "SummaryTabs";
            this.SummaryTabs.SelectedIndex = 0;
            // 
            // txtSaveDdlAs
            // 
            resources.ApplyResources(this.txtSaveDdlAs, "txtSaveDdlAs");
            this.txtSaveDdlAs.Name = "txtSaveDdlAs";
            this.txtSaveDdlAs.TextChanged += new System.EventHandler(this.txtSaveDdlAs_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(lblSaveDdlAs, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSaveDdlAs, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // WizardPageDbGenSummary
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.SummaryTabs);
            this.Name = "WizardPageDbGenSummary";
            this.Controls.SetChildIndex(this.infoPanel, 0);
            this.Controls.SetChildIndex(this.SummaryTabs, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            CreateDatabaseTabDDL.ResumeLayout(false);
            CreateDatabaseTabDDL.PerformLayout();
            this.SummaryTabs.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl SummaryTabs;
        private System.Windows.Forms.TextBox txtDDL;
        private System.Windows.Forms.TextBox txtSaveDdlAs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
