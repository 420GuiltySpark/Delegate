namespace Adjutant.Library.Controls.MetaViewerControls
{
    partial class mMultiValue
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
            this.btnColour = new System.Windows.Forms.Button();
            this.lblD = new System.Windows.Forms.Label();
            this.lblC = new System.Windows.Forms.Label();
            this.lblB = new System.Windows.Forms.Label();
            this.lblA = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtD = new System.Windows.Forms.TextBox();
            this.txtC = new System.Windows.Forms.TextBox();
            this.txtB = new System.Windows.Forms.TextBox();
            this.txtA = new System.Windows.Forms.TextBox();
            this.lblDesc = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnColour
            // 
            this.btnColour.Enabled = false;
            this.btnColour.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColour.Location = new System.Drawing.Point(530, 5);
            this.btnColour.Name = "btnColour";
            this.btnColour.Size = new System.Drawing.Size(24, 20);
            this.btnColour.TabIndex = 9;
            this.btnColour.UseVisualStyleBackColor = true;
            // 
            // lblD
            // 
            this.lblD.Location = new System.Drawing.Point(434, 5);
            this.lblD.Name = "lblD";
            this.lblD.Size = new System.Drawing.Size(18, 18);
            this.lblD.TabIndex = 8;
            this.lblD.Text = "X";
            this.lblD.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblC
            // 
            this.lblC.Location = new System.Drawing.Point(344, 5);
            this.lblC.Name = "lblC";
            this.lblC.Size = new System.Drawing.Size(18, 18);
            this.lblC.TabIndex = 7;
            this.lblC.Text = "X";
            this.lblC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblB
            // 
            this.lblB.Location = new System.Drawing.Point(254, 5);
            this.lblB.Name = "lblB";
            this.lblB.Size = new System.Drawing.Size(18, 18);
            this.lblB.TabIndex = 6;
            this.lblB.Text = "X";
            this.lblB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblA
            // 
            this.lblA.Location = new System.Drawing.Point(163, 5);
            this.lblA.Name = "lblA";
            this.lblA.Size = new System.Drawing.Size(18, 18);
            this.lblA.TabIndex = 5;
            this.lblA.Text = "X";
            this.lblA.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(33, 13);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "value";
            // 
            // txtD
            // 
            this.txtD.BackColor = System.Drawing.SystemColors.Window;
            this.txtD.Location = new System.Drawing.Point(452, 5);
            this.txtD.Name = "txtD";
            this.txtD.ReadOnly = true;
            this.txtD.Size = new System.Drawing.Size(72, 20);
            this.txtD.TabIndex = 3;
            // 
            // txtC
            // 
            this.txtC.BackColor = System.Drawing.SystemColors.Window;
            this.txtC.Location = new System.Drawing.Point(362, 5);
            this.txtC.Name = "txtC";
            this.txtC.ReadOnly = true;
            this.txtC.Size = new System.Drawing.Size(72, 20);
            this.txtC.TabIndex = 2;
            // 
            // txtB
            // 
            this.txtB.BackColor = System.Drawing.SystemColors.Window;
            this.txtB.Location = new System.Drawing.Point(272, 5);
            this.txtB.Name = "txtB";
            this.txtB.ReadOnly = true;
            this.txtB.Size = new System.Drawing.Size(72, 20);
            this.txtB.TabIndex = 1;
            // 
            // txtA
            // 
            this.txtA.BackColor = System.Drawing.SystemColors.Window;
            this.txtA.Location = new System.Drawing.Point(182, 5);
            this.txtA.Name = "txtA";
            this.txtA.ReadOnly = true;
            this.txtA.Size = new System.Drawing.Size(72, 20);
            this.txtA.TabIndex = 0;
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.Location = new System.Drawing.Point(350, 8);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(58, 13);
            this.lblDesc.TabIndex = 10;
            this.lblDesc.Text = "description";
            this.lblDesc.Visible = false;
            // 
            // mMultiValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnColour);
            this.Controls.Add(this.lblD);
            this.Controls.Add(this.lblC);
            this.Controls.Add(this.lblB);
            this.Controls.Add(this.lblA);
            this.Controls.Add(this.txtD);
            this.Controls.Add(this.txtC);
            this.Controls.Add(this.txtB);
            this.Controls.Add(this.txtA);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblDesc);
            this.Name = "mMultiValue";
            this.Size = new System.Drawing.Size(582, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtA;
        private System.Windows.Forms.TextBox txtB;
        private System.Windows.Forms.TextBox txtC;
        private System.Windows.Forms.TextBox txtD;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblA;
        private System.Windows.Forms.Label lblB;
        private System.Windows.Forms.Label lblC;
        private System.Windows.Forms.Label lblD;
        private System.Windows.Forms.Button btnColour;
        private System.Windows.Forms.Label lblDesc;
    }
}
