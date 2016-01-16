namespace Adjutant
{
    partial class SettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBrowsePlugins = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPluginPath = new System.Windows.Forms.TextBox();
            this.btnBrowseData = new System.Windows.Forms.Button();
            this.btnBrowseMap = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMapPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDataPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPermFilter = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtClassFilter = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmbLang = new System.Windows.Forms.ComboBox();
            this.cmbBitm = new System.Windows.Forms.ComboBox();
            this.cmbMode = new System.Windows.Forms.ComboBox();
            this.btnColour = new System.Windows.Forms.Button();
            this.cmbSnd_ = new System.Windows.Forms.ComboBox();
            this.tbMapScale = new System.Windows.Forms.TrackBar();
            this.tbPakScale = new System.Windows.Forms.TrackBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkFlags = new System.Windows.Forms.CheckedListBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblSnd_ = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblPakScale = new System.Windows.Forms.Label();
            this.lblMapScale = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPakScale)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnBrowsePlugins);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtPluginPath);
            this.groupBox2.Controls.Add(this.btnBrowseData);
            this.groupBox2.Controls.Add(this.btnBrowseMap);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtMapPath);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtDataPath);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(250, 142);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Directories";
            // 
            // btnBrowsePlugins
            // 
            this.btnBrowsePlugins.AutoSize = true;
            this.btnBrowsePlugins.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBrowsePlugins.Location = new System.Drawing.Point(218, 108);
            this.btnBrowsePlugins.Name = "btnBrowsePlugins";
            this.btnBrowsePlugins.Size = new System.Drawing.Size(26, 23);
            this.btnBrowsePlugins.TabIndex = 8;
            this.btnBrowsePlugins.Text = "...";
            this.btnBrowsePlugins.UseVisualStyleBackColor = true;
            this.btnBrowsePlugins.Click += new System.EventHandler(this.btnBrowsePlugins_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Plugins Folder:";
            // 
            // txtPluginPath
            // 
            this.txtPluginPath.Location = new System.Drawing.Point(9, 110);
            this.txtPluginPath.Name = "txtPluginPath";
            this.txtPluginPath.Size = new System.Drawing.Size(205, 20);
            this.txtPluginPath.TabIndex = 7;
            this.toolTip1.SetToolTip(this.txtPluginPath, "The folder to load plugins from");
            // 
            // btnBrowseData
            // 
            this.btnBrowseData.AutoSize = true;
            this.btnBrowseData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBrowseData.Location = new System.Drawing.Point(218, 69);
            this.btnBrowseData.Name = "btnBrowseData";
            this.btnBrowseData.Size = new System.Drawing.Size(26, 23);
            this.btnBrowseData.TabIndex = 5;
            this.btnBrowseData.Text = "...";
            this.btnBrowseData.UseVisualStyleBackColor = true;
            this.btnBrowseData.Click += new System.EventHandler(this.btnBrowseData_Click);
            // 
            // btnBrowseMap
            // 
            this.btnBrowseMap.AutoSize = true;
            this.btnBrowseMap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBrowseMap.Location = new System.Drawing.Point(218, 30);
            this.btnBrowseMap.Name = "btnBrowseMap";
            this.btnBrowseMap.Size = new System.Drawing.Size(26, 23);
            this.btnBrowseMap.TabIndex = 2;
            this.btnBrowseMap.Text = "...";
            this.btnBrowseMap.UseVisualStyleBackColor = true;
            this.btnBrowseMap.Click += new System.EventHandler(this.btnBrowseMap_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Map Folder:";
            // 
            // txtMapPath
            // 
            this.txtMapPath.Location = new System.Drawing.Point(9, 32);
            this.txtMapPath.Name = "txtMapPath";
            this.txtMapPath.Size = new System.Drawing.Size(205, 20);
            this.txtMapPath.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txtMapPath, "The default folder to start in when opening maps");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Data Folder:";
            // 
            // txtDataPath
            // 
            this.txtDataPath.Location = new System.Drawing.Point(9, 71);
            this.txtDataPath.Name = "txtDataPath";
            this.txtDataPath.Size = new System.Drawing.Size(205, 20);
            this.txtDataPath.TabIndex = 4;
            this.toolTip1.SetToolTip(this.txtDataPath, "The default save folder for batch extracts");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtPermFilter);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtClassFilter);
            this.groupBox1.Location = new System.Drawing.Point(12, 160);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 105);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Permutation Filter:";
            // 
            // txtPermFilter
            // 
            this.txtPermFilter.Location = new System.Drawing.Point(9, 71);
            this.txtPermFilter.Name = "txtPermFilter";
            this.txtPermFilter.Size = new System.Drawing.Size(235, 20);
            this.txtPermFilter.TabIndex = 3;
            this.toolTip1.SetToolTip(this.txtPermFilter, "Load certain permutations when the model viewer starts");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Class Filter:";
            // 
            // txtClassFilter
            // 
            this.txtClassFilter.Location = new System.Drawing.Point(9, 32);
            this.txtClassFilter.Name = "txtClassFilter";
            this.txtClassFilter.Size = new System.Drawing.Size(235, 20);
            this.txtClassFilter.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txtClassFilter, "Only show certain tag classes in the tag tree");
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
            this.cmbLang.Location = new System.Drawing.Point(10, 72);
            this.cmbLang.Name = "cmbLang";
            this.cmbLang.Size = new System.Drawing.Size(112, 21);
            this.cmbLang.TabIndex = 5;
            this.toolTip1.SetToolTip(this.cmbLang, "The format to use when batch extracting images");
            // 
            // cmbBitm
            // 
            this.cmbBitm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBitm.FormattingEnabled = true;
            this.cmbBitm.Items.AddRange(new object[] {
            "TIF (*.tif)",
            "DDS (*.dds)",
            "Raw (*.bin)",
            "PNG (*.png)"});
            this.cmbBitm.Location = new System.Drawing.Point(9, 32);
            this.cmbBitm.Name = "cmbBitm";
            this.cmbBitm.Size = new System.Drawing.Size(112, 21);
            this.cmbBitm.TabIndex = 1;
            this.toolTip1.SetToolTip(this.cmbBitm, "The format to use when batch extracting images");
            // 
            // cmbMode
            // 
            this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMode.FormattingEnabled = true;
            this.cmbMode.Items.AddRange(new object[] {
            "EMF v3 (*.emf)",
            "OBJ (*.obj)",
            "AMF (*.amf)",
            "JMS (*.jms)"});
            this.cmbMode.Location = new System.Drawing.Point(132, 32);
            this.cmbMode.Name = "cmbMode";
            this.cmbMode.Size = new System.Drawing.Size(112, 21);
            this.cmbMode.TabIndex = 3;
            this.toolTip1.SetToolTip(this.cmbMode, "The format to use when batch extracting models");
            // 
            // btnColour
            // 
            this.btnColour.BackColor = System.Drawing.SystemColors.Control;
            this.btnColour.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnColour.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColour.Location = new System.Drawing.Point(14, 219);
            this.btnColour.Name = "btnColour";
            this.btnColour.Size = new System.Drawing.Size(88, 23);
            this.btnColour.TabIndex = 6;
            this.btnColour.Text = "Viewer Colour";
            this.toolTip1.SetToolTip(this.btnColour, "The background colour of the model viewer");
            this.btnColour.UseVisualStyleBackColor = false;
            this.btnColour.BackColorChanged += new System.EventHandler(this.btnColour_BackColorChanged);
            this.btnColour.Click += new System.EventHandler(this.btnColour_Click);
            // 
            // cmbSnd_
            // 
            this.cmbSnd_.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmbSnd_.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSnd_.FormattingEnabled = true;
            this.cmbSnd_.Items.AddRange(new object[] {
            "WAV (*.wav)",
            "XMA (*.xma)",
            "Raw (*.bin)"});
            this.cmbSnd_.Location = new System.Drawing.Point(132, 72);
            this.cmbSnd_.Name = "cmbSnd_";
            this.cmbSnd_.Size = new System.Drawing.Size(112, 21);
            this.cmbSnd_.TabIndex = 7;
            this.toolTip1.SetToolTip(this.cmbSnd_, "The format to use when batch extracting sounds");
            // 
            // tbMapScale
            // 
            this.tbMapScale.AutoSize = false;
            this.tbMapScale.LargeChange = 25;
            this.tbMapScale.Location = new System.Drawing.Point(14, 46);
            this.tbMapScale.Maximum = 100;
            this.tbMapScale.Minimum = 10;
            this.tbMapScale.Name = "tbMapScale";
            this.tbMapScale.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbMapScale.Size = new System.Drawing.Size(42, 155);
            this.tbMapScale.SmallChange = 5;
            this.tbMapScale.TabIndex = 7;
            this.tbMapScale.TickFrequency = 10;
            this.tbMapScale.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.toolTip1.SetToolTip(this.tbMapScale, "Texture quality when viewing .map BSPs.\r\nLower values will use less memory.");
            this.tbMapScale.Value = 100;
            this.tbMapScale.ValueChanged += new System.EventHandler(this.tbMapScale_ValueChanged);
            // 
            // tbPakScale
            // 
            this.tbPakScale.AutoSize = false;
            this.tbPakScale.LargeChange = 25;
            this.tbPakScale.Location = new System.Drawing.Point(62, 46);
            this.tbPakScale.Maximum = 100;
            this.tbPakScale.Minimum = 10;
            this.tbPakScale.Name = "tbPakScale";
            this.tbPakScale.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbPakScale.Size = new System.Drawing.Size(42, 155);
            this.tbPakScale.SmallChange = 5;
            this.tbPakScale.TabIndex = 8;
            this.tbPakScale.TickFrequency = 10;
            this.tbPakScale.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.toolTip1.SetToolTip(this.tbPakScale, "Texture quality when viewing .s3dpak BSPs.\r\nLower values will use less memory.");
            this.tbPakScale.Value = 100;
            this.tbPakScale.ValueChanged += new System.EventHandler(this.tbPakScale_ValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkFlags);
            this.groupBox3.Location = new System.Drawing.Point(268, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(250, 142);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Options";
            // 
            // chkFlags
            // 
            this.chkFlags.FormattingEnabled = true;
            this.chkFlags.Items.AddRange(new object[] {
            "Automatic update check",
            "Short class names",
            "Sort tag list",
            "Overwrite tags in batch extraction",
            "Don\'t ask for batch extract folder",
            "Load model specular in model viewer",
            "Extract bitmap alpha in batch extract",
            "Split model meshes in batch extract",
            "Use permutation filter",
            "Use class filter",
            "Show invisibles in meta viewer",
            "Force-load models in viewer",
            "Separate output by tag type"});
            this.chkFlags.Location = new System.Drawing.Point(9, 19);
            this.chkFlags.Name = "chkFlags";
            this.chkFlags.Size = new System.Drawing.Size(235, 109);
            this.chkFlags.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblSnd_);
            this.groupBox4.Controls.Add(this.cmbSnd_);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.cmbLang);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.cmbBitm);
            this.groupBox4.Controls.Add(this.cmbMode);
            this.groupBox4.Location = new System.Drawing.Point(268, 160);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(250, 105);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Batch Extraction";
            // 
            // lblSnd_
            // 
            this.lblSnd_.AutoSize = true;
            this.lblSnd_.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblSnd_.Location = new System.Drawing.Point(129, 56);
            this.lblSnd_.Name = "lblSnd_";
            this.lblSnd_.Size = new System.Drawing.Size(76, 13);
            this.lblSnd_.TabIndex = 6;
            this.lblSnd_.Text = "Sound Format:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(93, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Locale Language:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(129, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Model Format:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Bitmap Format:";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(328, 274);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(247, 274);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.lblPakScale);
            this.groupBox5.Controls.Add(this.btnColour);
            this.groupBox5.Controls.Add(this.lblMapScale);
            this.groupBox5.Controls.Add(this.tbPakScale);
            this.groupBox5.Controls.Add(this.tbMapScale);
            this.groupBox5.Location = new System.Drawing.Point(524, 12);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(116, 253);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Model Viewer";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(60, 32);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(42, 18);
            this.label11.TabIndex = 12;
            this.label11.Text = "PAK";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(13, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(42, 18);
            this.label10.TabIndex = 11;
            this.label10.Text = "MAP";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(105, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "BSP Texture Quality:";
            // 
            // lblPakScale
            // 
            this.lblPakScale.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPakScale.Location = new System.Drawing.Point(60, 199);
            this.lblPakScale.Name = "lblPakScale";
            this.lblPakScale.Size = new System.Drawing.Size(44, 14);
            this.lblPakScale.TabIndex = 9;
            this.lblPakScale.Text = "100%";
            this.lblPakScale.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblMapScale
            // 
            this.lblMapScale.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapScale.Location = new System.Drawing.Point(12, 199);
            this.lblMapScale.Name = "lblMapScale";
            this.lblMapScale.Size = new System.Drawing.Size(44, 14);
            this.lblMapScale.TabIndex = 8;
            this.lblMapScale.Text = "100%";
            this.lblMapScale.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 309);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPakScale)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBrowsePlugins;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPluginPath;
        private System.Windows.Forms.Button btnBrowseData;
        private System.Windows.Forms.Button btnBrowseMap;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMapPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDataPath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPermFilter;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtClassFilter;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckedListBox chkFlags;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbLang;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbBitm;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnColour;
        private System.Windows.Forms.Label lblSnd_;
        private System.Windows.Forms.ComboBox cmbSnd_;
        private System.Windows.Forms.TrackBar tbMapScale;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblPakScale;
        private System.Windows.Forms.Label lblMapScale;
        private System.Windows.Forms.TrackBar tbPakScale;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
    }
}