namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mEnum
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
            this.cmbOptions = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(33, 13);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "value";
            // 
            // cmbOptions
            // 
            this.cmbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOptions.FormattingEnabled = true;
            this.cmbOptions.Location = new System.Drawing.Point(182, 5);
            this.cmbOptions.Name = "cmbOptions";
            this.cmbOptions.Size = new System.Drawing.Size(145, 21);
            this.cmbOptions.TabIndex = 4;
            this.cmbOptions.SelectedIndexChanged += new System.EventHandler(this.cmbOptions_SelectedIndexChanged);
            // 
            // mEnum
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbOptions);
            this.Controls.Add(this.lblName);
            this.Name = "mEnum";
            this.Size = new System.Drawing.Size(582, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ComboBox cmbOptions;
    }
}
