namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mTagRef
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
            this.txtClass = new System.Windows.Forms.TextBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gotoTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(33, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "value";
            // 
            // txtClass
            // 
            this.txtClass.BackColor = System.Drawing.SystemColors.Window;
            this.txtClass.Location = new System.Drawing.Point(182, 5);
            this.txtClass.Name = "txtClass";
            this.txtClass.ReadOnly = true;
            this.txtClass.Size = new System.Drawing.Size(35, 20);
            this.txtClass.TabIndex = 1;
            this.txtClass.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtPath
            // 
            this.txtPath.BackColor = System.Drawing.SystemColors.Window;
            this.txtPath.Location = new System.Drawing.Point(223, 5);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(301, 20);
            this.txtPath.TabIndex = 2;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gotoTagToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(124, 26);
            // 
            // gotoTagToolStripMenuItem
            // 
            this.gotoTagToolStripMenuItem.Name = "gotoTagToolStripMenuItem";
            this.gotoTagToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.gotoTagToolStripMenuItem.Text = "Load Tag";
            this.gotoTagToolStripMenuItem.Click += new System.EventHandler(this.loadTagToolStripMenuItem_Click);
            // 
            // mTagRef
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.txtClass);
            this.Controls.Add(this.lblName);
            this.Name = "mTagRef";
            this.Size = new System.Drawing.Size(582, 30);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtClass;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gotoTagToolStripMenuItem;
    }
}
