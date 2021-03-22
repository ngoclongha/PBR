namespace gProAnalyzer
{
    partial class frmProcessQuery
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
            if (disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProcessQuery));
            this.panel3 = new System.Windows.Forms.Panel();
            this.VersionInfo = new System.Windows.Forms.Label();
            this.Status = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.Results = new System.Windows.Forms.Label();
            this.ExeTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.QueryStatus = new System.Windows.Forms.Label();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.IndexingBtn = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.comboBox_BhR = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.startBtn = new System.Windows.Forms.Button();
            this.b_indexTime = new System.Windows.Forms.Button();
            this.b_queryTime = new System.Windows.Forms.Button();
            this.b_combine = new System.Windows.Forms.Button();
            this.b_Prop2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DodgerBlue;
            this.panel3.Controls.Add(this.VersionInfo);
            this.panel3.Controls.Add(this.Status);
            this.panel3.Location = new System.Drawing.Point(12, 509);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(507, 31);
            this.panel3.TabIndex = 10;
            // 
            // VersionInfo
            // 
            this.VersionInfo.AutoSize = true;
            this.VersionInfo.BackColor = System.Drawing.Color.DodgerBlue;
            this.VersionInfo.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.VersionInfo.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.VersionInfo.Location = new System.Drawing.Point(731, 8);
            this.VersionInfo.Name = "VersionInfo";
            this.VersionInfo.Size = new System.Drawing.Size(68, 12);
            this.VersionInfo.TabIndex = 1;
            this.VersionInfo.Text = "Version 1.0";
            // 
            // Status
            // 
            this.Status.AutoSize = true;
            this.Status.BackColor = System.Drawing.Color.DodgerBlue;
            this.Status.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Status.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Status.Location = new System.Drawing.Point(3, 8);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(41, 12);
            this.Status.TabIndex = 0;
            this.Status.Text = "Ready";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Khaki;
            this.panel4.Controls.Add(this.label7);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Controls.Add(this.Results);
            this.panel4.Controls.Add(this.ExeTime);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.QueryStatus);
            this.panel4.Location = new System.Drawing.Point(12, 476);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(507, 27);
            this.panel4.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Khaki;
            this.label7.Font = new System.Drawing.Font("Gulim", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label7.ForeColor = System.Drawing.Color.DarkKhaki;
            this.label7.Location = new System.Drawing.Point(159, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(16, 16);
            this.label7.TabIndex = 13;
            this.label7.Text = "|";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Khaki;
            this.label6.Font = new System.Drawing.Font("Gulim", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.ForeColor = System.Drawing.Color.DarkKhaki;
            this.label6.Location = new System.Drawing.Point(361, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 16);
            this.label6.TabIndex = 4;
            this.label6.Text = "|";
            // 
            // Results
            // 
            this.Results.AutoSize = true;
            this.Results.BackColor = System.Drawing.Color.Khaki;
            this.Results.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Results.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Results.Location = new System.Drawing.Point(380, 8);
            this.Results.Name = "Results";
            this.Results.Size = new System.Drawing.Size(108, 12);
            this.Results.TabIndex = 3;
            this.Results.Text = "0 Process Models";
            // 
            // ExeTime
            // 
            this.ExeTime.AutoSize = true;
            this.ExeTime.BackColor = System.Drawing.Color.Khaki;
            this.ExeTime.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ExeTime.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ExeTime.Location = new System.Drawing.Point(177, 8);
            this.ExeTime.Name = "ExeTime";
            this.ExeTime.Size = new System.Drawing.Size(150, 12);
            this.ExeTime.TabIndex = 2;
            this.ExeTime.Text = "Execution Time:  00:00:00";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.DodgerBlue;
            this.label4.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label4.Location = new System.Drawing.Point(741, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "Version";
            // 
            // QueryStatus
            // 
            this.QueryStatus.AutoSize = true;
            this.QueryStatus.BackColor = System.Drawing.Color.Khaki;
            this.QueryStatus.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.QueryStatus.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.QueryStatus.Location = new System.Drawing.Point(3, 8);
            this.QueryStatus.Name = "QueryStatus";
            this.QueryStatus.Size = new System.Drawing.Size(25, 12);
            this.QueryStatus.TabIndex = 0;
            this.QueryStatus.Text = "Idle";
            // 
            // tabControl2
            // 
            this.tabControl2.AllowDrop = true;
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Location = new System.Drawing.Point(12, 186);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(508, 283);
            this.tabControl2.TabIndex = 14;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.dataGridView1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(500, 257);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Results";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(494, 251);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            // 
            // Column1
            // 
            this.Column1.FillWeight = 400F;
            this.Column1.HeaderText = "Model name";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(500, 257);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Messages";
            // 
            // IndexingBtn
            // 
            this.IndexingBtn.Location = new System.Drawing.Point(23, 21);
            this.IndexingBtn.Name = "IndexingBtn";
            this.IndexingBtn.Size = new System.Drawing.Size(86, 40);
            this.IndexingBtn.TabIndex = 16;
            this.IndexingBtn.Text = "Load models & Pre-processing";
            this.IndexingBtn.UseVisualStyleBackColor = true;
            this.IndexingBtn.Click += new System.EventHandler(this.IndexingBtn_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Info;
            this.textBox1.Location = new System.Drawing.Point(227, 138);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(29, 20);
            this.textBox1.TabIndex = 17;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Info;
            this.textBox2.Location = new System.Drawing.Point(420, 138);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(29, 20);
            this.textBox2.TabIndex = 18;
            // 
            // comboBox_BhR
            // 
            this.comboBox_BhR.FormattingEnabled = true;
            this.comboBox_BhR.Items.AddRange(new object[] {
            "0-totalConcurrency",
            "1-existConcurrency",
            "2-totalCausal",
            "3-existCausal",
            "4-canConflict",
            "6-canCoocur"});
            this.comboBox_BhR.Location = new System.Drawing.Point(262, 138);
            this.comboBox_BhR.Name = "comboBox_BhR";
            this.comboBox_BhR.Size = new System.Drawing.Size(127, 21);
            this.comboBox_BhR.TabIndex = 1;
            this.comboBox_BhR.Text = "Select BhR";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(198, 141);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "X =";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(395, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Y =";
            // 
            // startBtn
            // 
            this.startBtn.BackgroundImage = global::gProAnalyzer.Properties.Resources.kisspng_youtube_play_button_computer_icons_play_button_5ac411361103f5_4688314415227989020697;
            this.startBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.startBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.startBtn.FlatAppearance.BorderSize = 0;
            this.startBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startBtn.Location = new System.Drawing.Point(144, 131);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(51, 33);
            this.startBtn.TabIndex = 6;
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // b_indexTime
            // 
            this.b_indexTime.Location = new System.Drawing.Point(62, 19);
            this.b_indexTime.Name = "b_indexTime";
            this.b_indexTime.Size = new System.Drawing.Size(87, 39);
            this.b_indexTime.TabIndex = 1;
            this.b_indexTime.Text = "Pre-processing Time";
            this.b_indexTime.UseVisualStyleBackColor = true;
            this.b_indexTime.Click += new System.EventHandler(this.b_indexTime_Click);
            // 
            // b_queryTime
            // 
            this.b_queryTime.Location = new System.Drawing.Point(155, 19);
            this.b_queryTime.Name = "b_queryTime";
            this.b_queryTime.Size = new System.Drawing.Size(75, 39);
            this.b_queryTime.TabIndex = 2;
            this.b_queryTime.Text = "Query Time";
            this.b_queryTime.UseVisualStyleBackColor = true;
            this.b_queryTime.Click += new System.EventHandler(this.b_queryTime_Click);
            // 
            // b_combine
            // 
            this.b_combine.Location = new System.Drawing.Point(236, 19);
            this.b_combine.Name = "b_combine";
            this.b_combine.Size = new System.Drawing.Size(75, 39);
            this.b_combine.TabIndex = 21;
            this.b_combine.Text = "Combine Index Query";
            this.b_combine.UseVisualStyleBackColor = true;
            this.b_combine.Click += new System.EventHandler(this.b_combine_Click);
            // 
            // b_Prop2
            // 
            this.b_Prop2.Location = new System.Drawing.Point(317, 19);
            this.b_Prop2.Name = "b_Prop2";
            this.b_Prop2.Size = new System.Drawing.Size(75, 39);
            this.b_Prop2.TabIndex = 22;
            this.b_Prop2.Text = "Pure Inst";
            this.b_Prop2.UseVisualStyleBackColor = true;
            this.b_Prop2.Click += new System.EventHandler(this.b_Prop2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox1.Controls.Add(this.b_Prop2);
            this.groupBox1.Controls.Add(this.b_combine);
            this.groupBox1.Controls.Add(this.b_indexTime);
            this.groupBox1.Controls.Add(this.b_queryTime);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(501, 74);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Empirical Analysis";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox2.Controls.Add(this.IndexingBtn);
            this.groupBox2.Location = new System.Drawing.Point(12, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 75);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Querying Process Models";
            // 
            // frmProcessQuery
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(531, 546);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_BhR);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmProcessQuery";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process Query";
            this.TransparencyKey = System.Drawing.Color.WhiteSmoke;
            this.Load += new System.EventHandler(this.frmProcessQuery_Load);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label VersionInfo;
        private System.Windows.Forms.Label Status;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label Results;
        private System.Windows.Forms.Label ExeTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label QueryStatus;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button IndexingBtn;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        public System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ComboBox comboBox_BhR;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.Button b_indexTime;
        private System.Windows.Forms.Button b_queryTime;
        private System.Windows.Forms.Button b_combine;
        private System.Windows.Forms.Button b_Prop2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}