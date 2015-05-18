namespace Adjutant.Library.Controls
{
    partial class StringsViewer
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
            this.lstStrings = new System.Windows.Forms.ListView();
            this.clmIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmbLang = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstStrings
            // 
            this.lstStrings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmIndex,
            this.clmText});
            this.lstStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstStrings.FullRowSelect = true;
            this.lstStrings.GridLines = true;
            this.lstStrings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstStrings.HideSelection = false;
            this.lstStrings.Location = new System.Drawing.Point(0, 0);
            this.lstStrings.MultiSelect = false;
            this.lstStrings.Name = "lstStrings";
            this.lstStrings.Size = new System.Drawing.Size(925, 519);
            this.lstStrings.TabIndex = 2;
            this.lstStrings.UseCompatibleStateImageBehavior = false;
            this.lstStrings.View = System.Windows.Forms.View.Details;
            this.lstStrings.ClientSizeChanged += new System.EventHandler(this.lstStrings_ClientSizeChanged);
            // 
            // clmIndex
            // 
            this.clmIndex.Text = "Index";
            this.clmIndex.Width = 46;
            // 
            // clmText
            // 
            this.clmText.Text = "Text";
            this.clmText.Width = 526;
            // 
            // cmbLang
            // 
            this.cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLang.FormattingEnabled = true;
            this.cmbLang.Items.AddRange(new object[] {
            "English",
            "Japanese",
            "German",
            "French",
            "Spanish",
            "LatinAmericanSpanish",
            "Italian",
            "Korean",
            "Chinese",
            "Unknown0",
            "Portuguese",
            "Unknown1"});
            this.cmbLang.Location = new System.Drawing.Point(6, 23);
            this.cmbLang.Name = "cmbLang";
            this.cmbLang.Size = new System.Drawing.Size(180, 21);
            this.cmbLang.TabIndex = 1;
            this.cmbLang.SelectedIndexChanged += new System.EventHandler(this.cmbLang_SelectedIndexChanged);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(224, 23);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.cmbLang);
            this.panel1.Controls.Add(this.txtSearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 519);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(925, 50);
            this.panel1.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(221, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Search:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Language:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(446, 21);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save All";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(103, 26);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // StringsViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstStrings);
            this.Controls.Add(this.panel1);
            this.Name = "StringsViewer";
            this.Size = new System.Drawing.Size(925, 569);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstStrings;
        private System.Windows.Forms.ColumnHeader clmIndex;
        private System.Windows.Forms.ColumnHeader clmText;
        private System.Windows.Forms.ComboBox cmbLang;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
    }
}
