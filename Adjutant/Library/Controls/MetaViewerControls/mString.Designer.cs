namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mString
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
            this.lblName = new System.Windows.Forms.Label();
            this.txtValue = new System.Windows.Forms.TextBox();
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
            // txtValue
            // 
            this.txtValue.BackColor = System.Drawing.SystemColors.Window;
            this.txtValue.Location = new System.Drawing.Point(182, 5);
            this.txtValue.Name = "txtValue";
            this.txtValue.ReadOnly = true;
            this.txtValue.Size = new System.Drawing.Size(145, 20);
            this.txtValue.TabIndex = 1;
            // 
            // mString
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.lblName);
            this.Name = "mString";
            this.Size = new System.Drawing.Size(582, 30);
            this.DoubleClick += new System.EventHandler(this.mString_DoubleClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtValue;
    }
}
