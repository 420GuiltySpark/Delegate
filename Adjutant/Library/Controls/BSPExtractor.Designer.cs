namespace Adjutant.Library.Controls
{
    partial class BSPExtractor
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
            this.lblName = new System.Windows.Forms.Label();
            this.tvRegions = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportBSP = new System.Windows.Forms.Button();
            this.btnExportBitmaps = new System.Windows.Forms.Button();
            this.selectAllChildrenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoEllipsis = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(8, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(204, 23);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "bspName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tvRegions
            // 
            this.tvRegions.CheckBoxes = true;
            this.tvRegions.ContextMenuStrip = this.contextMenuStrip1;
            this.tvRegions.Location = new System.Drawing.Point(8, 32);
            this.tvRegions.Name = "tvRegions";
            this.tvRegions.Size = new System.Drawing.Size(204, 300);
            this.tvRegions.TabIndex = 1;
            this.tvRegions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvRegions_AfterCheck);
            this.tvRegions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvRegions_AfterSelect);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.selectNoneToolStripMenuItem,
            this.selectAllChildrenToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(171, 70);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // selectNoneToolStripMenuItem
            // 
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.selectNoneToolStripMenuItem.Text = "Select None";
            this.selectNoneToolStripMenuItem.Click += new System.EventHandler(this.selectNoneToolStripMenuItem_Click);
            // 
            // btnExportBSP
            // 
            this.btnExportBSP.Location = new System.Drawing.Point(113, 338);
            this.btnExportBSP.Name = "btnExportBSP";
            this.btnExportBSP.Size = new System.Drawing.Size(99, 23);
            this.btnExportBSP.TabIndex = 2;
            this.btnExportBSP.Text = "Export BSP";
            this.btnExportBSP.UseVisualStyleBackColor = true;
            this.btnExportBSP.Click += new System.EventHandler(this.btnExportBSP_Click);
            // 
            // btnExportBitmaps
            // 
            this.btnExportBitmaps.Location = new System.Drawing.Point(8, 338);
            this.btnExportBitmaps.Name = "btnExportBitmaps";
            this.btnExportBitmaps.Size = new System.Drawing.Size(99, 23);
            this.btnExportBitmaps.TabIndex = 3;
            this.btnExportBitmaps.Text = "Export Bitmaps";
            this.btnExportBitmaps.UseVisualStyleBackColor = true;
            this.btnExportBitmaps.Click += new System.EventHandler(this.btnExportBitmaps_Click);
            // 
            // selectAllChildrenToolStripMenuItem
            // 
            this.selectAllChildrenToolStripMenuItem.Name = "selectAllChildrenToolStripMenuItem";
            this.selectAllChildrenToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.selectAllChildrenToolStripMenuItem.Text = "Select All Children";
            this.selectAllChildrenToolStripMenuItem.Click += new System.EventHandler(this.selectAllChildrenToolStripMenuItem_Click);
            // 
            // BSPExtractor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnExportBitmaps);
            this.Controls.Add(this.btnExportBSP);
            this.Controls.Add(this.tvRegions);
            this.Controls.Add(this.lblName);
            this.Name = "BSPExtractor";
            this.Size = new System.Drawing.Size(220, 370);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TreeView tvRegions;
        private System.Windows.Forms.Button btnExportBSP;
        private System.Windows.Forms.Button btnExportBitmaps;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllChildrenToolStripMenuItem;
    }
}
