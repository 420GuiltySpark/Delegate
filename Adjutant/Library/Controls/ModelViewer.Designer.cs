namespace Adjutant.Library.Controls
{
    partial class ModelViewer
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tvRegions = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnExportModel = new System.Windows.Forms.Button();
            this.btnSelNone = new System.Windows.Forms.Button();
            this.btnSelAll = new System.Windows.Forms.Button();
            this.btnBDS = new System.Windows.Forms.Button();
            this.ehRenderer = new System.Windows.Forms.Integration.ElementHost();
            this.renderer1 = new Adjutant.Library.Controls.Renderer();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectPermutationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectPermutationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1MinSize = 170;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ehRenderer);
            this.splitContainer1.Size = new System.Drawing.Size(750, 500);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tvRegions);
            this.splitContainer2.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnExportModel);
            this.splitContainer2.Panel2.Controls.Add(this.btnSelNone);
            this.splitContainer2.Panel2.Controls.Add(this.btnSelAll);
            this.splitContainer2.Panel2.Controls.Add(this.btnBDS);
            this.splitContainer2.Panel2MinSize = 120;
            this.splitContainer2.Size = new System.Drawing.Size(200, 500);
            this.splitContainer2.SplitterDistance = 369;
            this.splitContainer2.TabIndex = 0;
            // 
            // tvRegions
            // 
            this.tvRegions.CheckBoxes = true;
            this.tvRegions.ContextMenuStrip = this.contextMenuStrip1;
            this.tvRegions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvRegions.Location = new System.Drawing.Point(0, 0);
            this.tvRegions.Name = "tvRegions";
            this.tvRegions.Size = new System.Drawing.Size(200, 348);
            this.tvRegions.TabIndex = 0;
            this.tvRegions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvRegions_AfterCheck);
            this.tvRegions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvRegions_AfterSelect);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.txtSearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 348);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 21);
            this.panel1.TabIndex = 4;
            // 
            // txtSearch
            // 
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtSearch.Location = new System.Drawing.Point(0, -1);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(198, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // btnExportModel
            // 
            this.btnExportModel.Location = new System.Drawing.Point(45, 95);
            this.btnExportModel.Name = "btnExportModel";
            this.btnExportModel.Size = new System.Drawing.Size(110, 23);
            this.btnExportModel.TabIndex = 3;
            this.btnExportModel.Text = "Export Model";
            this.btnExportModel.UseVisualStyleBackColor = true;
            this.btnExportModel.Click += new System.EventHandler(this.btnExportModel_Click);
            // 
            // btnSelNone
            // 
            this.btnSelNone.Location = new System.Drawing.Point(45, 66);
            this.btnSelNone.Name = "btnSelNone";
            this.btnSelNone.Size = new System.Drawing.Size(110, 23);
            this.btnSelNone.TabIndex = 2;
            this.btnSelNone.Text = "Select None";
            this.btnSelNone.UseVisualStyleBackColor = true;
            this.btnSelNone.Click += new System.EventHandler(this.btnSelNone_Click);
            // 
            // btnSelAll
            // 
            this.btnSelAll.Location = new System.Drawing.Point(45, 37);
            this.btnSelAll.Name = "btnSelAll";
            this.btnSelAll.Size = new System.Drawing.Size(110, 23);
            this.btnSelAll.TabIndex = 1;
            this.btnSelAll.Text = "Select All";
            this.btnSelAll.UseVisualStyleBackColor = true;
            this.btnSelAll.Click += new System.EventHandler(this.btnSelAll_Click);
            // 
            // btnBDS
            // 
            this.btnBDS.Location = new System.Drawing.Point(45, 8);
            this.btnBDS.Name = "btnBDS";
            this.btnBDS.Size = new System.Drawing.Size(110, 23);
            this.btnBDS.TabIndex = 0;
            this.btnBDS.Text = "Select B/D/S";
            this.btnBDS.UseVisualStyleBackColor = true;
            this.btnBDS.Click += new System.EventHandler(this.btnBDS_Click);
            // 
            // ehRenderer
            // 
            this.ehRenderer.BackColor = System.Drawing.Color.CornflowerBlue;
            this.ehRenderer.Cursor = System.Windows.Forms.Cursors.Cross;
            this.ehRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehRenderer.Location = new System.Drawing.Point(0, 0);
            this.ehRenderer.Name = "ehRenderer";
            this.ehRenderer.Size = new System.Drawing.Size(546, 500);
            this.ehRenderer.TabIndex = 0;
            this.ehRenderer.Text = "elementHost1";
            this.ehRenderer.Child = this.renderer1;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectPermutationToolStripMenuItem,
            this.deselectPermutationToolStripMenuItem,
            this.selectResultsToolStripMenuItem,
            this.deselectResultsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(188, 92);
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
            // selectResultsToolStripMenuItem
            // 
            this.selectResultsToolStripMenuItem.Name = "selectResultsToolStripMenuItem";
            this.selectResultsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.selectResultsToolStripMenuItem.Text = "Select Results";
            this.selectResultsToolStripMenuItem.Visible = false;
            this.selectResultsToolStripMenuItem.Click += new System.EventHandler(this.selectResultsToolStripMenuItem_Click);
            // 
            // deselectResultsToolStripMenuItem
            // 
            this.deselectResultsToolStripMenuItem.Name = "deselectResultsToolStripMenuItem";
            this.deselectResultsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.deselectResultsToolStripMenuItem.Text = "Deselect Results";
            this.deselectResultsToolStripMenuItem.Visible = false;
            this.deselectResultsToolStripMenuItem.Click += new System.EventHandler(this.deselectResultsToolStripMenuItem_Click);
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ModelViewer";
            this.Size = new System.Drawing.Size(750, 500);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView tvRegions;
        private System.Windows.Forms.Button btnExportModel;
        private System.Windows.Forms.Button btnSelNone;
        private System.Windows.Forms.Button btnSelAll;
        private System.Windows.Forms.Button btnBDS;
        private System.Windows.Forms.Integration.ElementHost ehRenderer;
        private Renderer renderer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectPermutationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deselectPermutationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deselectResultsToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtSearch;
    }
}
