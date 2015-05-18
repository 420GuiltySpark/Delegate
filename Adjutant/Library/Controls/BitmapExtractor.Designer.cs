namespace Adjutant.Library.Controls
{
    partial class BitmapExtractor
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
            this.chkAlpha = new System.Windows.Forms.CheckBox();
            this.cmbFormat = new System.Windows.Forms.ComboBox();
            this.picImage = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportThisImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lstBitmaps = new System.Windows.Forms.ListView();
            this.clmIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmWidth = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmHeight = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkAlpha
            // 
            this.chkAlpha.AutoSize = true;
            this.chkAlpha.Checked = true;
            this.chkAlpha.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAlpha.Location = new System.Drawing.Point(21, 8);
            this.chkAlpha.Name = "chkAlpha";
            this.chkAlpha.Size = new System.Drawing.Size(83, 17);
            this.chkAlpha.TabIndex = 1;
            this.chkAlpha.Text = "Show Alpha";
            this.chkAlpha.UseVisualStyleBackColor = true;
            this.chkAlpha.CheckedChanged += new System.EventHandler(this.chkAlpha_CheckedChanged);
            // 
            // cmbFormat
            // 
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(213, 6);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(150, 21);
            this.cmbFormat.TabIndex = 0;
            this.cmbFormat.SelectedIndexChanged += new System.EventHandler(this.cmbFormat_SelectedIndexChanged);
            // 
            // picImage
            // 
            this.picImage.BackColor = System.Drawing.Color.Black;
            this.picImage.Location = new System.Drawing.Point(0, 134);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(384, 384);
            this.picImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picImage.TabIndex = 1;
            this.picImage.TabStop = false;
            this.picImage.Click += new System.EventHandler(this.picImage_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportThisImageToolStripMenuItem,
            this.exportAllImagesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(169, 70);
            // 
            // exportThisImageToolStripMenuItem
            // 
            this.exportThisImageToolStripMenuItem.Name = "exportThisImageToolStripMenuItem";
            this.exportThisImageToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.exportThisImageToolStripMenuItem.Text = "Export This Image";
            this.exportThisImageToolStripMenuItem.Click += new System.EventHandler(this.exportThisImageToolStripMenuItem_Click);
            // 
            // exportAllImagesToolStripMenuItem
            // 
            this.exportAllImagesToolStripMenuItem.Name = "exportAllImagesToolStripMenuItem";
            this.exportAllImagesToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.exportAllImagesToolStripMenuItem.Text = "Export All Images";
            this.exportAllImagesToolStripMenuItem.Click += new System.EventHandler(this.exportAllImagesToolStripMenuItem_Click);
            // 
            // lstBitmaps
            // 
            this.lstBitmaps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmIndex,
            this.clmWidth,
            this.clmHeight,
            this.clmType,
            this.clmFormat});
            this.lstBitmaps.ContextMenuStrip = this.contextMenuStrip1;
            this.lstBitmaps.FullRowSelect = true;
            this.lstBitmaps.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstBitmaps.HideSelection = false;
            this.lstBitmaps.Location = new System.Drawing.Point(0, 33);
            this.lstBitmaps.MultiSelect = false;
            this.lstBitmaps.Name = "lstBitmaps";
            this.lstBitmaps.Size = new System.Drawing.Size(384, 97);
            this.lstBitmaps.TabIndex = 0;
            this.lstBitmaps.UseCompatibleStateImageBehavior = false;
            this.lstBitmaps.View = System.Windows.Forms.View.Details;
            this.lstBitmaps.SelectedIndexChanged += new System.EventHandler(this.lstBitmaps_SelectedIndexChanged);
            // 
            // clmIndex
            // 
            this.clmIndex.Text = "Index";
            this.clmIndex.Width = 46;
            // 
            // clmWidth
            // 
            this.clmWidth.Text = "Width";
            this.clmWidth.Width = 66;
            // 
            // clmHeight
            // 
            this.clmHeight.Text = "Height";
            this.clmHeight.Width = 66;
            // 
            // clmType
            // 
            this.clmType.Text = "Type";
            this.clmType.Width = 91;
            // 
            // clmFormat
            // 
            this.clmFormat.Text = "Format";
            this.clmFormat.Width = 111;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.chkAlpha);
            this.panel1.Controls.Add(this.cmbFormat);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(384, 33);
            this.panel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(160, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "View as:";
            // 
            // BitmapExtractor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.picImage);
            this.Controls.Add(this.lstBitmaps);
            this.Name = "BitmapExtractor";
            this.Size = new System.Drawing.Size(384, 518);
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAlpha;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.ListView lstBitmaps;
        private System.Windows.Forms.ColumnHeader clmIndex;
        private System.Windows.Forms.ColumnHeader clmWidth;
        private System.Windows.Forms.ColumnHeader clmHeight;
        private System.Windows.Forms.ColumnHeader clmType;
        private System.Windows.Forms.ColumnHeader clmFormat;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportThisImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllImagesToolStripMenuItem;
        private System.Windows.Forms.Label label1;
    }
}
