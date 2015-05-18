namespace Adjutant.Library.Controls
{
    partial class SoundExtractorH4
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
            this.lstPerms = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSaveSelected = new System.Windows.Forms.Button();
            this.lblPerms = new System.Windows.Forms.Label();
            this.btnSaveSingle = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstPerms
            // 
            this.lstPerms.ContextMenuStrip = this.contextMenuStrip1;
            this.lstPerms.FormattingEnabled = true;
            this.lstPerms.Location = new System.Drawing.Point(29, 35);
            this.lstPerms.Name = "lstPerms";
            this.lstPerms.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstPerms.Size = new System.Drawing.Size(180, 108);
            this.lstPerms.TabIndex = 0;
            this.lstPerms.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstPerms_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.selectNoneToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(138, 48);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // selectNoneToolStripMenuItem
            // 
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.selectNoneToolStripMenuItem.Text = "Select None";
            this.selectNoneToolStripMenuItem.Click += new System.EventHandler(this.selectNoneToolStripMenuItem_Click);
            // 
            // btnSaveSelected
            // 
            this.btnSaveSelected.Location = new System.Drawing.Point(29, 149);
            this.btnSaveSelected.Name = "btnSaveSelected";
            this.btnSaveSelected.Size = new System.Drawing.Size(87, 23);
            this.btnSaveSelected.TabIndex = 2;
            this.btnSaveSelected.Text = "Save Selected";
            this.btnSaveSelected.UseVisualStyleBackColor = true;
            this.btnSaveSelected.Click += new System.EventHandler(this.btnSaveSelected_Click);
            // 
            // lblPerms
            // 
            this.lblPerms.AutoSize = true;
            this.lblPerms.Location = new System.Drawing.Point(26, 19);
            this.lblPerms.Name = "lblPerms";
            this.lblPerms.Size = new System.Drawing.Size(71, 13);
            this.lblPerms.TabIndex = 6;
            this.lblPerms.Text = "Permutations:";
            // 
            // btnSaveSingle
            // 
            this.btnSaveSingle.Enabled = false;
            this.btnSaveSingle.Location = new System.Drawing.Point(122, 149);
            this.btnSaveSingle.Name = "btnSaveSingle";
            this.btnSaveSingle.Size = new System.Drawing.Size(87, 23);
            this.btnSaveSingle.TabIndex = 7;
            this.btnSaveSingle.Text = "Save As Single";
            this.btnSaveSingle.UseVisualStyleBackColor = true;
            this.btnSaveSingle.Click += new System.EventHandler(this.btnSaveSingle_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(29, 178);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(87, 23);
            this.btnPlay.TabIndex = 8;
            this.btnPlay.Text = "Play Sel";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(122, 178);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(87, 23);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(122, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "---";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SoundExtractorH4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnSaveSingle);
            this.Controls.Add(this.lblPerms);
            this.Controls.Add(this.btnSaveSelected);
            this.Controls.Add(this.lstPerms);
            this.Name = "SoundExtractorH4";
            this.Size = new System.Drawing.Size(235, 210);
            this.Load += new System.EventHandler(this.SoundExtractorH4_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstPerms;
        private System.Windows.Forms.Button btnSaveSelected;
        private System.Windows.Forms.Label lblPerms;
        private System.Windows.Forms.Button btnSaveSingle;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNoneToolStripMenuItem;
        private System.Windows.Forms.Label label1;
    }
}
