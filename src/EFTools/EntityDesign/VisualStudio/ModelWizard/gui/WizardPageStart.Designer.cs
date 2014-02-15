// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    partial class WizardPageStart
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
            this.textboxListViewSelectionInfo = new System.Windows.Forms.TextBox();
            this.listViewModelContents = new System.Windows.Forms.ListView();
            this.labelPrompt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // infoPanel
            // 
            this.infoPanel.Size = new System.Drawing.Size(500, 304);
            // 
            // textboxListViewSelectionInfo
            // 
            this.textboxListViewSelectionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxListViewSelectionInfo.Location = new System.Drawing.Point(0, 116);
            this.textboxListViewSelectionInfo.Multiline = true;
            this.textboxListViewSelectionInfo.Name = "textboxListViewSelectionInfo";
            this.textboxListViewSelectionInfo.ReadOnly = true;
            this.textboxListViewSelectionInfo.Size = new System.Drawing.Size(500, 90);
            this.textboxListViewSelectionInfo.TabIndex = 2;
            // 
            // listViewModelContents
            // 
            this.listViewModelContents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewModelContents.GridLines = true;
            this.listViewModelContents.HideSelection = false;
            this.listViewModelContents.Location = new System.Drawing.Point(0, 18);
            this.listViewModelContents.MultiSelect = false;
            this.listViewModelContents.Name = "listViewModelContents";
            this.listViewModelContents.OwnerDraw = true;
            this.listViewModelContents.Size = new System.Drawing.Size(500, 93);
            this.listViewModelContents.TabIndex = 1;
            this.listViewModelContents.UseCompatibleStateImageBehavior = false;
            this.textboxListViewSelectionInfo.AccessibleName = ModelWizard.Properties.Resources.StartPage_AccessibleSelectionExplanation;
            this.listViewModelContents.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewModelContents_DrawItem);
            this.listViewModelContents.SelectedIndexChanged += new System.EventHandler(this.listViewModelContents_SelectedIndexChanged);
            this.listViewModelContents.DoubleClick += new System.EventHandler(this.listViewModelContents_DoubleClick);
            // 
            // labelPrompt
            // 
            this.labelPrompt.AutoSize = true;
            this.labelPrompt.Font = new System.Drawing.Font(this.Font.FontFamily, 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrompt.Location = new System.Drawing.Point(0, 0);
            this.labelPrompt.Name = "labelPrompt";
            this.labelPrompt.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.labelPrompt.Size = new System.Drawing.Size(190, 18);
            this.labelPrompt.TabIndex = 0;
            this.labelPrompt.Text = ModelWizard.Properties.Resources.StartPage_PromptLabelText;
            // 
            // WizardPageStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listViewModelContents);
            this.Controls.Add(this.textboxListViewSelectionInfo);
            this.Controls.Add(this.labelPrompt);
            this.Name = "WizardPageStart";
            this.Size = new System.Drawing.Size(500, 304);
            this.Controls.SetChildIndex(this.infoPanel, 0);
            this.Controls.SetChildIndex(this.labelPrompt, 0);
            this.Controls.SetChildIndex(this.textboxListViewSelectionInfo, 0);
            this.Controls.SetChildIndex(this.listViewModelContents, 0);
            this.ResumeLayout(false);
            this.PerformLayout();
        
        }

        #endregion

        private System.Windows.Forms.TextBox textboxListViewSelectionInfo;
        private System.Windows.Forms.ListView listViewModelContents;
        private System.Windows.Forms.Label labelPrompt;
    }
}
