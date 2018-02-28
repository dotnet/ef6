namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    partial class WizardPageRuntimeConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardPageRuntimeConfig));
            this.promptLabel = new System.Windows.Forms.Label();
            this.versionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.notificationPictureBox = new System.Windows.Forms.PictureBox();
            this.notificationLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.notificationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.notificationLinkLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.notificationPictureBox)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.notificationPanel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // infoPanel
            // 
            resources.ApplyResources(this.infoPanel, "infoPanel");
            // 
            // promptLabel
            // 
            resources.ApplyResources(this.promptLabel, "promptLabel");
            this.promptLabel.Name = "promptLabel";
            // 
            // versionsPanel
            // 
            resources.ApplyResources(this.versionsPanel, "versionsPanel");
            this.versionsPanel.Name = "versionsPanel";
            // 
            // notificationPictureBox
            // 
            resources.ApplyResources(this.notificationPictureBox, "notificationPictureBox");
            this.notificationPictureBox.Image = global::Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources.Information;
            this.notificationPictureBox.Name = "notificationPictureBox";
            this.notificationPictureBox.TabStop = false;
            // 
            // notificationLabel
            // 
            resources.ApplyResources(this.notificationLabel, "notificationLabel");
            this.notificationLabel.Name = "notificationLabel";
            // 
            // notificationLinkLabel
            // 
            resources.ApplyResources(this.notificationLinkLabel, "notificationLinkLabel");
            this.notificationLinkLabel.Name = "notificationLinkLabel";
            this.notificationLinkLabel.TabStop = true;
            this.notificationLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleLinkClicked);
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.notificationLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.notificationLinkLabel, 0, 1);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // notificationPanel
            // 
            resources.ApplyResources(this.notificationPanel, "notificationPanel");
            this.notificationPanel.Controls.Add(this.notificationPictureBox, 0, 0);
            this.notificationPanel.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.notificationPanel.Name = "notificationPanel";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.promptLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.versionsPanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.notificationPanel, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // WizardPageRuntimeConfig
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "WizardPageRuntimeConfig";
            this.Controls.SetChildIndex(this.infoPanel, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.notificationPictureBox)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.notificationPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.FlowLayoutPanel versionsPanel;
        private System.Windows.Forms.PictureBox notificationPictureBox;
        private System.Windows.Forms.Label notificationLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel notificationPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.LinkLabel notificationLinkLabel;

    }
}
