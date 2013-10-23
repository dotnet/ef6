// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    partial class VsIdeHostAdapterRunConfigControl
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
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.m_hiveCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_additionalCommandLineArgumentsEdit = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.m_additionalTestDataEdit = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = Resources.VSRegistryHive;
            // 
            // m_hiveCombo
            // 
            this.m_hiveCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_hiveCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_hiveCombo.FormattingEnabled = true;
            this.m_hiveCombo.Location = new System.Drawing.Point(0, 16);
            this.m_hiveCombo.Name = "m_hiveCombo";
            this.m_hiveCombo.Size = new System.Drawing.Size(324, 21);
            this.m_hiveCombo.TabIndex = 1;
            this.m_hiveCombo.SelectedIndexChanged += new System.EventHandler(this.HiveCombo_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = Resources.AdditionalCommandLine;
            // 
            // m_additionalCommandLineArgumentsEdit
            // 
            this.m_additionalCommandLineArgumentsEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_additionalCommandLineArgumentsEdit.Location = new System.Drawing.Point(0, 56);
            this.m_additionalCommandLineArgumentsEdit.Name = "m_additionalCommandLineArgumentsEdit";
            this.m_additionalCommandLineArgumentsEdit.Size = new System.Drawing.Size(324, 20);
            this.m_additionalCommandLineArgumentsEdit.TabIndex = 3;
            this.m_additionalCommandLineArgumentsEdit.TextChanged += new System.EventHandler(this.AdditionalCommandLineArgumentsEdit_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = Resources.AdditionalTestData;
            // 
            // m_additionalTestDataEdit
            // 
            this.m_additionalTestDataEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_additionalTestDataEdit.Location = new System.Drawing.Point(0, 95);
            this.m_additionalTestDataEdit.Name = "m_additionalTestDataEdit";
            this.m_additionalTestDataEdit.Size = new System.Drawing.Size(324, 20);
            this.m_additionalTestDataEdit.TabIndex = 5;
            this.m_additionalTestDataEdit.TextChanged += new System.EventHandler(this.m_additionalTestDataEdit_TextChanged);
            // 
            // VsIdeHostAdapterRunConfigControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.m_additionalTestDataEdit);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_additionalCommandLineArgumentsEdit);
            this.Controls.Add(this.m_hiveCombo);
            this.Controls.Add(this.label1);
            this.Name = "VsIdeHostAdapterRunConfigControl";
            this.Size = new System.Drawing.Size(324, 163);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox m_hiveCombo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_additionalCommandLineArgumentsEdit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_additionalTestDataEdit;
    }
}
