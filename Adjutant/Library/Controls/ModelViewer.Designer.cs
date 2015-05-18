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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tvRegions = new System.Windows.Forms.TreeView();
            this.btnExportModel = new System.Windows.Forms.Button();
            this.btnSelNone = new System.Windows.Forms.Button();
            this.btnSelAll = new System.Windows.Forms.Button();
            this.btnBDS = new System.Windows.Forms.Button();
            this.ehRenderer = new System.Windows.Forms.Integration.ElementHost();
            this.renderer1 = new Adjutant.Library.Controls.Renderer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(674, 500);
            this.splitContainer1.SplitterDistance = 170;
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
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnExportModel);
            this.splitContainer2.Panel2.Controls.Add(this.btnSelNone);
            this.splitContainer2.Panel2.Controls.Add(this.btnSelAll);
            this.splitContainer2.Panel2.Controls.Add(this.btnBDS);
            this.splitContainer2.Panel2MinSize = 135;
            this.splitContainer2.Size = new System.Drawing.Size(170, 500);
            this.splitContainer2.SplitterDistance = 361;
            this.splitContainer2.TabIndex = 0;
            // 
            // tvRegions
            // 
            this.tvRegions.CheckBoxes = true;
            this.tvRegions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvRegions.Location = new System.Drawing.Point(0, 0);
            this.tvRegions.Name = "tvRegions";
            this.tvRegions.Size = new System.Drawing.Size(170, 361);
            this.tvRegions.TabIndex = 0;
            this.tvRegions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvRegions_AfterCheck);
            // 
            // btnExportModel
            // 
            this.btnExportModel.Location = new System.Drawing.Point(35, 99);
            this.btnExportModel.Name = "btnExportModel";
            this.btnExportModel.Size = new System.Drawing.Size(100, 23);
            this.btnExportModel.TabIndex = 3;
            this.btnExportModel.Text = "Export Model";
            this.btnExportModel.UseVisualStyleBackColor = true;
            this.btnExportModel.Click += new System.EventHandler(this.btnExportModel_Click);
            // 
            // btnSelNone
            // 
            this.btnSelNone.Location = new System.Drawing.Point(35, 70);
            this.btnSelNone.Name = "btnSelNone";
            this.btnSelNone.Size = new System.Drawing.Size(100, 23);
            this.btnSelNone.TabIndex = 2;
            this.btnSelNone.Text = "Select None";
            this.btnSelNone.UseVisualStyleBackColor = true;
            this.btnSelNone.Click += new System.EventHandler(this.btnSelNone_Click);
            // 
            // btnSelAll
            // 
            this.btnSelAll.Location = new System.Drawing.Point(35, 41);
            this.btnSelAll.Name = "btnSelAll";
            this.btnSelAll.Size = new System.Drawing.Size(100, 23);
            this.btnSelAll.TabIndex = 1;
            this.btnSelAll.Text = "Select All";
            this.btnSelAll.UseVisualStyleBackColor = true;
            this.btnSelAll.Click += new System.EventHandler(this.btnSelAll_Click);
            // 
            // btnBDS
            // 
            this.btnBDS.Location = new System.Drawing.Point(35, 12);
            this.btnBDS.Name = "btnBDS";
            this.btnBDS.Size = new System.Drawing.Size(100, 23);
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
            this.ehRenderer.Size = new System.Drawing.Size(500, 500);
            this.ehRenderer.TabIndex = 0;
            this.ehRenderer.Text = "elementHost1";
            this.ehRenderer.Child = this.renderer1;
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ModelViewer";
            this.Size = new System.Drawing.Size(674, 500);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
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
    }
}
