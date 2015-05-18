namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mBitmask
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
            this.clbOptions = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(33, 13);
            this.lblName.TabIndex = 5;
            this.lblName.Text = "value";
            // 
            // clbOptions
            // 
            this.clbOptions.FormattingEnabled = true;
            this.clbOptions.Location = new System.Drawing.Point(182, 5);
            this.clbOptions.Name = "clbOptions";
            this.clbOptions.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.clbOptions.Size = new System.Drawing.Size(171, 19);
            this.clbOptions.TabIndex = 6;
            // 
            // mBitmask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.clbOptions);
            this.Controls.Add(this.lblName);
            this.Name = "mBitmask";
            this.Size = new System.Drawing.Size(582, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.CheckedListBox clbOptions;
    }
}
