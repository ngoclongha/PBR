namespace gProAnalyzer
{
    partial class frmMakeNetwork
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
            this.textXOR_Rate = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtToBF = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtFormBF = new System.Windows.Forms.TextBox();
            this.txtToFF = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtFormFF = new System.Windows.Forms.TextBox();
            this.txtToSS = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtFormSS = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtSF_Rate = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtToNF_Rate = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtFormNF = new System.Windows.Forms.TextBox();
            this.txtN = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.menuMake = new System.Windows.Forms.MenuStrip();
            this.mnuMakeNetwork = new System.Windows.Forms.ToolStripMenuItem();
            this.groupCondition = new System.Windows.Forms.GroupBox();
            this.folderBrowserMake = new System.Windows.Forms.FolderBrowserDialog();
            this.btnSetFolder = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.txtFileA = new System.Windows.Forms.TextBox();
            this.txtFileB = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.txtFileN = new System.Windows.Forms.TextBox();
            this.groupSave = new System.Windows.Forms.GroupBox();
            this.textOR_Rate = new System.Windows.Forms.TextBox();
            this.textAND_Rate = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.menuMake.SuspendLayout();
            this.groupCondition.SuspendLayout();
            this.groupSave.SuspendLayout();
            this.SuspendLayout();
            // 
            // textXOR_Rate
            // 
            this.textXOR_Rate.Location = new System.Drawing.Point(263, 228);
            this.textXOR_Rate.Name = "textXOR_Rate";
            this.textXOR_Rate.Size = new System.Drawing.Size(41, 21);
            this.textXOR_Rate.TabIndex = 50;
            this.textXOR_Rate.Text = "0.5";
            this.textXOR_Rate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textXOR_Rate.Leave += new System.EventHandler(this.textXOR_Rate_Leave);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(21, 232);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(105, 12);
            this.label13.TabIndex = 49;
            this.label13.Text = "Rate of Gateways";
            // 
            // txtToBF
            // 
            this.txtToBF.Location = new System.Drawing.Point(384, 191);
            this.txtToBF.Name = "txtToBF";
            this.txtToBF.Size = new System.Drawing.Size(41, 21);
            this.txtToBF.TabIndex = 47;
            this.txtToBF.Text = "2";
            this.txtToBF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(367, 194);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(11, 12);
            this.label12.TabIndex = 46;
            this.label12.Text = "-";
            // 
            // txtFormBF
            // 
            this.txtFormBF.Location = new System.Drawing.Point(320, 191);
            this.txtFormBF.Name = "txtFormBF";
            this.txtFormBF.Size = new System.Drawing.Size(41, 21);
            this.txtFormBF.TabIndex = 45;
            this.txtFormBF.Text = "0";
            this.txtFormBF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtToFF
            // 
            this.txtToFF.Location = new System.Drawing.Point(374, 164);
            this.txtToFF.Name = "txtToFF";
            this.txtToFF.Size = new System.Drawing.Size(41, 21);
            this.txtToFF.TabIndex = 44;
            this.txtToFF.Text = "3";
            this.txtToFF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(357, 167);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(11, 12);
            this.label11.TabIndex = 43;
            this.label11.Text = "-";
            // 
            // txtFormFF
            // 
            this.txtFormFF.Location = new System.Drawing.Point(310, 164);
            this.txtFormFF.Name = "txtFormFF";
            this.txtFormFF.Size = new System.Drawing.Size(41, 21);
            this.txtFormFF.TabIndex = 42;
            this.txtFormFF.Text = "1";
            this.txtFormFF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtToSS
            // 
            this.txtToSS.Location = new System.Drawing.Point(284, 129);
            this.txtToSS.Name = "txtToSS";
            this.txtToSS.Size = new System.Drawing.Size(41, 21);
            this.txtToSS.TabIndex = 41;
            this.txtToSS.Text = "4";
            this.txtToSS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(267, 132);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(11, 12);
            this.label10.TabIndex = 40;
            this.label10.Text = "-";
            // 
            // txtFormSS
            // 
            this.txtFormSS.Location = new System.Drawing.Point(220, 129);
            this.txtFormSS.Name = "txtFormSS";
            this.txtFormSS.Size = new System.Drawing.Size(41, 21);
            this.txtFormSS.TabIndex = 39;
            this.txtFormSS.Text = "2";
            this.txtFormSS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(300, 87);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 12);
            this.label9.TabIndex = 38;
            this.label9.Text = "* NF";
            // 
            // txtSF_Rate
            // 
            this.txtSF_Rate.Location = new System.Drawing.Point(258, 84);
            this.txtSF_Rate.Name = "txtSF_Rate";
            this.txtSF_Rate.Size = new System.Drawing.Size(41, 21);
            this.txtSF_Rate.TabIndex = 37;
            this.txtSF_Rate.Text = "0.4";
            this.txtSF_Rate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(364, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 12);
            this.label7.TabIndex = 36;
            this.label7.Text = "* N";
            // 
            // txtToNF_Rate
            // 
            this.txtToNF_Rate.Location = new System.Drawing.Point(322, 53);
            this.txtToNF_Rate.Name = "txtToNF_Rate";
            this.txtToNF_Rate.Size = new System.Drawing.Size(41, 21);
            this.txtToNF_Rate.TabIndex = 35;
            this.txtToNF_Rate.Text = "0.1";
            this.txtToNF_Rate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(305, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(11, 12);
            this.label8.TabIndex = 34;
            this.label8.Text = "-";
            // 
            // txtFormNF
            // 
            this.txtFormNF.Location = new System.Drawing.Point(258, 53);
            this.txtFormNF.Name = "txtFormNF";
            this.txtFormNF.Size = new System.Drawing.Size(41, 21);
            this.txtFormNF.TabIndex = 33;
            this.txtFormNF.Text = "2";
            this.txtFormNF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtN
            // 
            this.txtN.Location = new System.Drawing.Point(171, 24);
            this.txtN.Name = "txtN";
            this.txtN.Size = new System.Drawing.Size(41, 21);
            this.txtN.TabIndex = 32;
            this.txtN.Text = "50";
            this.txtN.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 196);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(293, 12);
            this.label6.TabIndex = 31;
            this.label6.Text = "Number of Nodes for Backward Flow in a Structure";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 167);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(283, 12);
            this.label5.TabIndex = 30;
            this.label5.Text = "Number of Nodes for Forward Flow in a Structure";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(171, 12);
            this.label4.TabIndex = 29;
            this.label4.Text = "Number of Split in a Structure";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(215, 12);
            this.label3.TabIndex = 28;
            this.label3.Text = "Rate of the Structure in a Frame Flow";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(231, 12);
            this.label2.TabIndex = 27;
            this.label2.Text = "Number of Nodes in a Frame Flow (NF)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 12);
            this.label1.TabIndex = 26;
            this.label1.Text = "Number of Nodes (N)";
            // 
            // menuMake
            // 
            this.menuMake.AllowMerge = false;
            this.menuMake.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMakeNetwork});
            this.menuMake.Location = new System.Drawing.Point(0, 0);
            this.menuMake.Name = "menuMake";
            this.menuMake.Size = new System.Drawing.Size(519, 24);
            this.menuMake.TabIndex = 51;
            this.menuMake.Text = "menuStrip1";
            // 
            // mnuMakeNetwork
            // 
            this.mnuMakeNetwork.Name = "mnuMakeNetwork";
            this.mnuMakeNetwork.Size = new System.Drawing.Size(97, 20);
            this.mnuMakeNetwork.Text = "Make Network";
            this.mnuMakeNetwork.Click += new System.EventHandler(this.mnuMakeNetwork_Click);
            // 
            // groupCondition
            // 
            this.groupCondition.Controls.Add(this.label18);
            this.groupCondition.Controls.Add(this.label17);
            this.groupCondition.Controls.Add(this.label16);
            this.groupCondition.Controls.Add(this.textAND_Rate);
            this.groupCondition.Controls.Add(this.textOR_Rate);
            this.groupCondition.Controls.Add(this.label1);
            this.groupCondition.Controls.Add(this.textXOR_Rate);
            this.groupCondition.Controls.Add(this.label2);
            this.groupCondition.Controls.Add(this.label13);
            this.groupCondition.Controls.Add(this.label3);
            this.groupCondition.Controls.Add(this.label4);
            this.groupCondition.Controls.Add(this.txtToBF);
            this.groupCondition.Controls.Add(this.label5);
            this.groupCondition.Controls.Add(this.label12);
            this.groupCondition.Controls.Add(this.label6);
            this.groupCondition.Controls.Add(this.txtFormBF);
            this.groupCondition.Controls.Add(this.txtN);
            this.groupCondition.Controls.Add(this.txtToFF);
            this.groupCondition.Controls.Add(this.txtFormNF);
            this.groupCondition.Controls.Add(this.label11);
            this.groupCondition.Controls.Add(this.label8);
            this.groupCondition.Controls.Add(this.txtFormFF);
            this.groupCondition.Controls.Add(this.txtToNF_Rate);
            this.groupCondition.Controls.Add(this.txtToSS);
            this.groupCondition.Controls.Add(this.label7);
            this.groupCondition.Controls.Add(this.label10);
            this.groupCondition.Controls.Add(this.txtSF_Rate);
            this.groupCondition.Controls.Add(this.txtFormSS);
            this.groupCondition.Controls.Add(this.label9);
            this.groupCondition.Location = new System.Drawing.Point(38, 40);
            this.groupCondition.Name = "groupCondition";
            this.groupCondition.Size = new System.Drawing.Size(451, 259);
            this.groupCondition.TabIndex = 52;
            this.groupCondition.TabStop = false;
            this.groupCondition.Text = "< Making Condition >";
            // 
            // btnSetFolder
            // 
            this.btnSetFolder.Location = new System.Drawing.Point(18, 68);
            this.btnSetFolder.Name = "btnSetFolder";
            this.btnSetFolder.Size = new System.Drawing.Size(98, 31);
            this.btnSetFolder.TabIndex = 53;
            this.btnSetFolder.Text = "Folder";
            this.btnSetFolder.UseVisualStyleBackColor = true;
            this.btnSetFolder.Click += new System.EventHandler(this.btnSetFolder_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.BackColor = System.Drawing.Color.White;
            this.txtFolder.Location = new System.Drawing.Point(122, 74);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.ReadOnly = true;
            this.txtFolder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtFolder.Size = new System.Drawing.Size(314, 21);
            this.txtFolder.TabIndex = 54;
            // 
            // txtFileA
            // 
            this.txtFileA.Location = new System.Drawing.Point(331, 32);
            this.txtFileA.Name = "txtFileA";
            this.txtFileA.Size = new System.Drawing.Size(60, 21);
            this.txtFileA.TabIndex = 55;
            this.txtFileA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFileB
            // 
            this.txtFileB.Location = new System.Drawing.Point(393, 32);
            this.txtFileB.Name = "txtFileB";
            this.txtFileB.Size = new System.Drawing.Size(25, 21);
            this.txtFileB.TabIndex = 56;
            this.txtFileB.Text = "1";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(21, 35);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(95, 12);
            this.label14.TabIndex = 57;
            this.label14.Text = "Number of Files";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(223, 35);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(102, 12);
            this.label15.TabIndex = 58;
            this.label15.Text = "File Name (Start)";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtFileN
            // 
            this.txtFileN.Location = new System.Drawing.Point(122, 32);
            this.txtFileN.Name = "txtFileN";
            this.txtFileN.Size = new System.Drawing.Size(54, 21);
            this.txtFileN.TabIndex = 59;
            this.txtFileN.Text = "1";
            // 
            // groupSave
            // 
            this.groupSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupSave.Controls.Add(this.btnSetFolder);
            this.groupSave.Controls.Add(this.txtFileN);
            this.groupSave.Controls.Add(this.txtFolder);
            this.groupSave.Controls.Add(this.label15);
            this.groupSave.Controls.Add(this.txtFileA);
            this.groupSave.Controls.Add(this.label14);
            this.groupSave.Controls.Add(this.txtFileB);
            this.groupSave.Location = new System.Drawing.Point(38, 322);
            this.groupSave.Name = "groupSave";
            this.groupSave.Size = new System.Drawing.Size(450, 111);
            this.groupSave.TabIndex = 60;
            this.groupSave.TabStop = false;
            this.groupSave.Text = "< Saving Condition >";
            // 
            // textOR_Rate
            // 
            this.textOR_Rate.Location = new System.Drawing.Point(178, 228);
            this.textOR_Rate.Name = "textOR_Rate";
            this.textOR_Rate.Size = new System.Drawing.Size(41, 21);
            this.textOR_Rate.TabIndex = 51;
            this.textOR_Rate.Text = "0.1";
            this.textOR_Rate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textOR_Rate.Leave += new System.EventHandler(this.textOR_Rate_Leave);
            // 
            // textAND_Rate
            // 
            this.textAND_Rate.Enabled = false;
            this.textAND_Rate.Location = new System.Drawing.Point(348, 228);
            this.textAND_Rate.Name = "textAND_Rate";
            this.textAND_Rate.Size = new System.Drawing.Size(41, 21);
            this.textAND_Rate.TabIndex = 61;
            this.textAND_Rate.Text = "0.4";
            this.textAND_Rate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(155, 232);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(22, 12);
            this.label16.TabIndex = 62;
            this.label16.Text = "OR";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(233, 232);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(30, 12);
            this.label17.TabIndex = 63;
            this.label17.Text = "XOR";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(319, 232);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(30, 12);
            this.label18.TabIndex = 64;
            this.label18.Text = "AND";
            // 
            // frmMakeNetwork
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 464);
            this.Controls.Add(this.groupSave);
            this.Controls.Add(this.groupCondition);
            this.Controls.Add(this.menuMake);
            this.MainMenuStrip = this.menuMake;
            this.Name = "frmMakeNetwork";
            this.Text = "Make Random Network";
            this.menuMake.ResumeLayout(false);
            this.menuMake.PerformLayout();
            this.groupCondition.ResumeLayout(false);
            this.groupCondition.PerformLayout();
            this.groupSave.ResumeLayout(false);
            this.groupSave.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textXOR_Rate;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtToBF;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtFormBF;
        private System.Windows.Forms.TextBox txtToFF;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtFormFF;
        private System.Windows.Forms.TextBox txtToSS;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtFormSS;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtSF_Rate;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtToNF_Rate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtFormNF;
        private System.Windows.Forms.TextBox txtN;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MenuStrip menuMake;
        private System.Windows.Forms.ToolStripMenuItem mnuMakeNetwork;
        private System.Windows.Forms.GroupBox groupCondition;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserMake;
        private System.Windows.Forms.Button btnSetFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.TextBox txtFileA;
        private System.Windows.Forms.TextBox txtFileB;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtFileN;
        private System.Windows.Forms.GroupBox groupSave;
        private System.Windows.Forms.TextBox textAND_Rate;
        private System.Windows.Forms.TextBox textOR_Rate;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
    }
}