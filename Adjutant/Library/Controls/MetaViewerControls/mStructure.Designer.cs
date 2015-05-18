namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mStructure
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblName = new System.Windows.Forms.Label();
            this.cmbChunks = new System.Windows.Forms.ComboBox();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlContainer
            // 
            this.pnlContainer.BackColor = System.Drawing.SystemColors.Control;
            this.pnlContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlContainer.Location = new System.Drawing.Point(8, 32);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(590, 31);
            this.pnlContainer.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.DarkGray;
            this.pnlHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlHeader.Controls.Add(this.lblName);
            this.pnlHeader.Controls.Add(this.cmbChunks);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(595, 63);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(20, 5);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(167, 20);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "STRUCTURE";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbChunks
            // 
            this.cmbChunks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChunks.FormattingEnabled = true;
            this.cmbChunks.Location = new System.Drawing.Point(195, 5);
            this.cmbChunks.Name = "cmbChunks";
            this.cmbChunks.Size = new System.Drawing.Size(145, 21);
            this.cmbChunks.TabIndex = 1;
            this.cmbChunks.SelectedIndexChanged += new System.EventHandler(this.cmbChunks_SelectedIndexChanged);
            // 
            // mStructure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlContainer);
            this.Controls.Add(this.pnlHeader);
            this.Name = "mStructure";
            this.Size = new System.Drawing.Size(595, 63);
            this.EnabledChanged += new System.EventHandler(this.mStructure_EnabledChanged);
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.ComboBox cmbChunks;
        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.Label lblName;
    }
}
