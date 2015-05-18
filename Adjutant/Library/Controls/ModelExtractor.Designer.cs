namespace Adjutant.Library.Controls
{
    partial class ModelExtractor
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
            this.selectBDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPermutationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectPermutationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportModel = new System.Windows.Forms.Button();
            this.btnExportBitmaps = new System.Windows.Forms.Button();
            this.chkSplit = new System.Windows.Forms.CheckBox();
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
            this.lblName.Text = "modelName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tvRegions
            // 
            this.tvRegions.CheckBoxes = true;
            this.tvRegions.ContextMenuStrip = this.contextMenuStrip1;
            this.tvRegions.Location = new System.Drawing.Point(8, 55);
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
            this.selectBDSToolStripMenuItem,
            this.selectPermutationToolStripMenuItem,
            this.deselectPermutationToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(188, 114);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // selectNoneToolStripMenuItem
            // 
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.selectNoneToolStripMenuItem.Text = "Select None";
            this.selectNoneToolStripMenuItem.Click += new System.EventHandler(this.selectNoneToolStripMenuItem_Click);
            // 
            // selectBDSToolStripMenuItem
            // 
            this.selectBDSToolStripMenuItem.Name = "selectBDSToolStripMenuItem";
            this.selectBDSToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.selectBDSToolStripMenuItem.Text = "Select BDS";
            this.selectBDSToolStripMenuItem.Visible = false;
            this.selectBDSToolStripMenuItem.Click += new System.EventHandler(this.selectBDSToolStripMenuItem_Click);
            // 
            // selectPermutationToolStripMenuItem
            // 
            this.selectPermutationToolStripMenuItem.Name = "selectPermutationToolStripMenuItem";
            this.selectPermutationToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.selectPermutationToolStripMenuItem.Text = "Select Permutation";
            this.selectPermutationToolStripMenuItem.Click += new System.EventHandler(this.selectPermutationToolStripMenuItem_Click);
            // 
            // deselectPermutationToolStripMenuItem
            // 
            this.deselectPermutationToolStripMenuItem.Name = "deselectPermutationToolStripMenuItem";
            this.deselectPermutationToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.deselectPermutationToolStripMenuItem.Text = "Deselect Permutation";
            this.deselectPermutationToolStripMenuItem.Click += new System.EventHandler(this.deselectPermutationToolStripMenuItem_Click);
            // 
            // btnExportModel
            // 
            this.btnExportModel.Location = new System.Drawing.Point(113, 361);
            this.btnExportModel.Name = "btnExportModel";
            this.btnExportModel.Size = new System.Drawing.Size(99, 23);
            this.btnExportModel.TabIndex = 2;
            this.btnExportModel.Text = "Export Model";
            this.btnExportModel.UseVisualStyleBackColor = true;
            this.btnExportModel.Click += new System.EventHandler(this.btnExportModel_Click);
            // 
            // btnExportBitmaps
            // 
            this.btnExportBitmaps.Location = new System.Drawing.Point(8, 361);
            this.btnExportBitmaps.Name = "btnExportBitmaps";
            this.btnExportBitmaps.Size = new System.Drawing.Size(99, 23);
            this.btnExportBitmaps.TabIndex = 3;
            this.btnExportBitmaps.Text = "Export Bitmaps";
            this.btnExportBitmaps.UseVisualStyleBackColor = true;
            this.btnExportBitmaps.Click += new System.EventHandler(this.btnExportBitmaps_Click);
            // 
            // chkSplit
            // 
            this.chkSplit.AutoSize = true;
            this.chkSplit.Location = new System.Drawing.Point(52, 32);
            this.chkSplit.Name = "chkSplit";
            this.chkSplit.Size = new System.Drawing.Size(117, 17);
            this.chkSplit.TabIndex = 4;
            this.chkSplit.Text = "Split Meshes (EMF)";
            this.chkSplit.UseVisualStyleBackColor = true;
            // 
            // ModelExtractor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkSplit);
            this.Controls.Add(this.btnExportBitmaps);
            this.Controls.Add(this.btnExportModel);
            this.Controls.Add(this.tvRegions);
            this.Controls.Add(this.lblName);
            this.Name = "ModelExtractor";
            this.Size = new System.Drawing.Size(220, 391);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TreeView tvRegions;
        private System.Windows.Forms.Button btnExportModel;
        private System.Windows.Forms.Button btnExportBitmaps;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNoneToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkSplit;
        private System.Windows.Forms.ToolStripMenuItem selectBDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectPermutationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deselectPermutationToolStripMenuItem;
    }
}
