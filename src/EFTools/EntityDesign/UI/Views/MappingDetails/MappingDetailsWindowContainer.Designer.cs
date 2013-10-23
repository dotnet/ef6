// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails
{
    partial class MappingDetailsWindowContainer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MappingDetailsWindowContainer));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolbar = new System.Windows.Forms.ToolStrip();
            this.tablesButton = new System.Windows.Forms.ToolStripButton();
            this.sprocsButton = new System.Windows.Forms.ToolStripButton();
            this.contentsPanel = new System.Windows.Forms.Panel();
            this.watermarkLabel = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolbar.SuspendLayout();
            this.contentsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolbar, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.contentsPanel, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(485, 205);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // toolbar
            // 
            this.toolbar.AccessibleName = string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.MappingDetails_Toolbar_AccessibleName);
            this.toolbar.CanOverflow = false;
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tablesButton,
            this.sprocsButton});
            this.toolbar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolbar.Location = new System.Drawing.Point(0, 0);
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(0);
            this.toolbar.Size = new System.Drawing.Size(31, 205);
            this.toolbar.TabIndex = 1;
            this.toolbar.TabStop = true;
            // 
            // tablesButton
            // 
            this.tablesButton.AutoSize = false;
            this.tablesButton.CheckOnClick = true;
            this.tablesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tablesButton.Image = ((System.Drawing.Image)(resources.GetObject("tablesButton.Image")));
            this.tablesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tablesButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 2);
            this.tablesButton.Name = "tablesButton";
            this.tablesButton.Size = new System.Drawing.Size(25, 25);
            this.tablesButton.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.MappingDetails_TablesButtonText);
            this.tablesButton.Click += new System.EventHandler(this.tablesButton_Click);
            // 
            // sprocsButton
            // 
            this.sprocsButton.AutoSize = false;
            this.sprocsButton.CheckOnClick = true;
            this.sprocsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.sprocsButton.Image = ((System.Drawing.Image)(resources.GetObject("sprocsButton.Image")));
            this.sprocsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sprocsButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 2);
            this.sprocsButton.Name = "sprocsButton";
            this.sprocsButton.Size = new System.Drawing.Size(25, 25);
            this.sprocsButton.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.MappingDetails_SProcsButtonText);
            this.sprocsButton.Click += new System.EventHandler(this.sprocsButton_Click);
            // 
            // contentsPanel
            // 
            this.contentsPanel.BackColor = System.Drawing.SystemColors.Window;
            this.contentsPanel.Controls.Add(this.watermarkLabel);
            this.contentsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentsPanel.Location = new System.Drawing.Point(32, 1);
            this.contentsPanel.Margin = new System.Windows.Forms.Padding(1);
            this.contentsPanel.Name = "contentsPanel";
            this.contentsPanel.Padding = new System.Windows.Forms.Padding(5);
            this.contentsPanel.Size = new System.Drawing.Size(452, 203);
            this.contentsPanel.TabIndex = 0;
            // 
            // watermarkLabel
            // 
            this.watermarkLabel.BackColor = System.Drawing.Color.Transparent;
            this.watermarkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.watermarkLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.watermarkLabel.Location = new System.Drawing.Point(5, 5);
            this.watermarkLabel.Name = "watermarkLabel";
            this.watermarkLabel.Size = new System.Drawing.Size(442, 193);
            this.watermarkLabel.TabIndex = 0;
            this.watermarkLabel.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.MappingDetails_WatermarkLabelText);
            this.watermarkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MappingDetailsWindowContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MappingDetailsWindowContainer";
            this.Size = new System.Drawing.Size(485, 205);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.contentsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal System.Windows.Forms.Panel contentsPanel;
        private System.Windows.Forms.ToolStrip toolbar;
        internal System.Windows.Forms.ToolStripButton tablesButton;
        internal System.Windows.Forms.ToolStripButton sprocsButton;
        private System.Windows.Forms.LinkLabel watermarkLabel;

    }
}
