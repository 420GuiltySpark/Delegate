namespace Adjutant.Library.Controls
{
    partial class MetaViewer
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
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.chkInvis = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // pnlContainer
            // 
            this.pnlContainer.AutoScroll = true;
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(598, 498);
            this.pnlContainer.TabIndex = 0;
            // 
            // chkInvis
            // 
            this.chkInvis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkInvis.AutoSize = true;
            this.chkInvis.Location = new System.Drawing.Point(481, 478);
            this.chkInvis.Name = "chkInvis";
            this.chkInvis.Size = new System.Drawing.Size(99, 17);
            this.chkInvis.TabIndex = 1;
            this.chkInvis.Text = "Show Invisibles";
            this.chkInvis.UseVisualStyleBackColor = true;
            this.chkInvis.Visible = false;
            this.chkInvis.CheckedChanged += new System.EventHandler(this.chkInvis_CheckedChanged);
            // 
            // MetaViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.chkInvis);
            this.Controls.Add(this.pnlContainer);
            this.Name = "MetaViewer";
            this.Size = new System.Drawing.Size(598, 498);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.CheckBox chkInvis;
    }
}
