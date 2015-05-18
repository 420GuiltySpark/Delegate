namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mValue
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
            this.txtValue = new System.Windows.Forms.TextBox();
            this.lblDesc = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dumpRawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpZoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(33, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "value";
            // 
            // txtValue
            // 
            this.txtValue.BackColor = System.Drawing.SystemColors.Window;
            this.txtValue.Location = new System.Drawing.Point(182, 5);
            this.txtValue.Name = "txtValue";
            this.txtValue.ReadOnly = true;
            this.txtValue.Size = new System.Drawing.Size(72, 20);
            this.txtValue.TabIndex = 5;
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.Location = new System.Drawing.Point(260, 8);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(58, 13);
            this.lblDesc.TabIndex = 7;
            this.lblDesc.Text = "description";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dumpRawToolStripMenuItem,
            this.dumpZoneToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 70);
            // 
            // dumpRawToolStripMenuItem
            // 
            this.dumpRawToolStripMenuItem.Name = "dumpRawToolStripMenuItem";
            this.dumpRawToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.dumpRawToolStripMenuItem.Text = "Dump Raw";
            this.dumpRawToolStripMenuItem.Click += new System.EventHandler(this.dumpRawToolStripMenuItem_Click);
            // 
            // dumpZoneToolStripMenuItem
            // 
            this.dumpZoneToolStripMenuItem.Name = "dumpZoneToolStripMenuItem";
            this.dumpZoneToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.dumpZoneToolStripMenuItem.Text = "Dump Zone";
            this.dumpZoneToolStripMenuItem.Click += new System.EventHandler(this.dumpZoneToolStripMenuItem_Click);
            // 
            // mValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.lblDesc);
            this.Controls.Add(this.lblName);
            this.Name = "mValue";
            this.Size = new System.Drawing.Size(582, 30);
            this.DoubleClick += new System.EventHandler(this.mValue_DoubleClick);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dumpRawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpZoneToolStripMenuItem;
    }
}
