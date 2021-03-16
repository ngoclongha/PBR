namespace gProAnalyzer
{
    partial class frmMain_TEST
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain_TEST));
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.menuMakeNetwork = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMakeRandom = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMakeIBM = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMakeSAP = new System.Windows.Forms.ToolStripMenuItem();
            this.ePMLToNetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitRigidsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAnalysisNetwork = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSimulation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSimulation_2 = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton4 = new System.Windows.Forms.ToolStripDropDownButton();
            this.sESEAnalyzerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.indexingProcessesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.processQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.filterOutSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel3.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuMakeNetwork,
            this.menuAnalysisNetwork,
            this.mnuSimulation,
            this.mnuSimulation_2});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuMain.Size = new System.Drawing.Size(1058, 24);
            this.menuMain.TabIndex = 1;
            this.menuMain.Text = "menuStrip1";
            // 
            // menuMakeNetwork
            // 
            this.menuMakeNetwork.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuMakeRandom,
            this.menuMakeIBM,
            this.menuMakeSAP,
            this.ePMLToNetToolStripMenuItem,
            this.splitRigidsToolStripMenuItem,
            this.filterOutSoundToolStripMenuItem});
            this.menuMakeNetwork.Name = "menuMakeNetwork";
            this.menuMakeNetwork.Size = new System.Drawing.Size(96, 20);
            this.menuMakeNetwork.Text = "Make Network";
            // 
            // menuMakeRandom
            // 
            this.menuMakeRandom.Name = "menuMakeRandom";
            this.menuMakeRandom.Size = new System.Drawing.Size(201, 22);
            this.menuMakeRandom.Text = "Random";
            this.menuMakeRandom.Click += new System.EventHandler(this.menuMakeRandom_Click);
            // 
            // menuMakeIBM
            // 
            this.menuMakeIBM.Name = "menuMakeIBM";
            this.menuMakeIBM.Size = new System.Drawing.Size(201, 22);
            this.menuMakeIBM.Text = "From IBM";
            this.menuMakeIBM.Click += new System.EventHandler(this.menuMakeIBM_Click);
            // 
            // menuMakeSAP
            // 
            this.menuMakeSAP.Name = "menuMakeSAP";
            this.menuMakeSAP.Size = new System.Drawing.Size(201, 22);
            this.menuMakeSAP.Text = "From SAP";
            this.menuMakeSAP.Click += new System.EventHandler(this.menuMakeSAP_Click);
            // 
            // ePMLToNetToolStripMenuItem
            // 
            this.ePMLToNetToolStripMenuItem.Name = "ePMLToNetToolStripMenuItem";
            this.ePMLToNetToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.ePMLToNetToolStripMenuItem.Text = "EPML to Net";
            this.ePMLToNetToolStripMenuItem.Click += new System.EventHandler(this.ePMLToNetToolStripMenuItem_Click);
            // 
            // splitRigidsToolStripMenuItem
            // 
            this.splitRigidsToolStripMenuItem.Name = "splitRigidsToolStripMenuItem";
            this.splitRigidsToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.splitRigidsToolStripMenuItem.Text = "Split Rigids";
            this.splitRigidsToolStripMenuItem.Click += new System.EventHandler(this.splitRigidsToolStripMenuItem_Click);
            // 
            // menuAnalysisNetwork
            // 
            this.menuAnalysisNetwork.Name = "menuAnalysisNetwork";
            this.menuAnalysisNetwork.Size = new System.Drawing.Size(110, 20);
            this.menuAnalysisNetwork.Text = "Analysis Network";
            this.menuAnalysisNetwork.Click += new System.EventHandler(this.menuAnalysisNetwork_Click);
            // 
            // mnuSimulation
            // 
            this.mnuSimulation.Name = "mnuSimulation";
            this.mnuSimulation.Size = new System.Drawing.Size(76, 20);
            this.mnuSimulation.Text = "Simulation";
            this.mnuSimulation.Click += new System.EventHandler(this.mnuSimulation_Click);
            // 
            // mnuSimulation_2
            // 
            this.mnuSimulation_2.Name = "mnuSimulation_2";
            this.mnuSimulation_2.Size = new System.Drawing.Size(110, 20);
            this.mnuSimulation_2.Text = "Simulation - Poly";
            this.mnuSimulation_2.Click += new System.EventHandler(this.mnuSimulation_2_Click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(413, 1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Split Models";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightBlue;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 700);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1058, 24);
            this.panel1.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.BackColor = System.Drawing.Color.LightBlue;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1058, 27);
            this.panel2.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 51);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel2.Controls.Add(this.panel4);
            this.splitContainer1.Panel2.Controls.Add(this.panel3);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip1);
            this.splitContainer1.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer1.Size = new System.Drawing.Size(1058, 649);
            this.splitContainer1.SplitterDistance = 210;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 7;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 69);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(841, 576);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(833, 550);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Processes";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.chart1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(833, 550);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Charts";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(189, 29);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(492, 404);
            this.chart1.TabIndex = 1;
            this.chart1.Text = "chart1";
            // 
            // panel4
            // 
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 69);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(841, 576);
            this.panel4.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.textBox1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 25);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(841, 44);
            this.panel3.TabIndex = 3;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(118, 20);
            this.textBox1.TabIndex = 4;
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.toolStrip1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(18, 18);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton4,
            this.toolStripDropDownButton2,
            this.toolStripDropDownButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(841, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(120, 22);
            this.toolStripDropDownButton1.Text = "Make Process";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(83, 22);
            this.toolStripMenuItem5.Text = "1";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(83, 22);
            this.toolStripMenuItem6.Text = "2";
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(83, 22);
            this.toolStripMenuItem7.Text = "3";
            // 
            // toolStripDropDownButton4
            // 
            this.toolStripDropDownButton4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sESEAnalyzerToolStripMenuItem,
            this.simulationToolStripMenuItem,
            this.dashboardToolStripMenuItem});
            this.toolStripDropDownButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton4.Image")));
            this.toolStripDropDownButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton4.Name = "toolStripDropDownButton4";
            this.toolStripDropDownButton4.Size = new System.Drawing.Size(132, 22);
            this.toolStripDropDownButton4.Text = "Analyze Process";
            // 
            // sESEAnalyzerToolStripMenuItem
            // 
            this.sESEAnalyzerToolStripMenuItem.Name = "sESEAnalyzerToolStripMenuItem";
            this.sESEAnalyzerToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.sESEAnalyzerToolStripMenuItem.Text = "SESE Analyzer";
            this.sESEAnalyzerToolStripMenuItem.Click += new System.EventHandler(this.sESEAnalyzerToolStripMenuItem_Click);
            // 
            // simulationToolStripMenuItem
            // 
            this.simulationToolStripMenuItem.Name = "simulationToolStripMenuItem";
            this.simulationToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.simulationToolStripMenuItem.Text = "Simulation";
            this.simulationToolStripMenuItem.Click += new System.EventHandler(this.simulationToolStripMenuItem_Click);
            // 
            // dashboardToolStripMenuItem
            // 
            this.dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            this.dashboardToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.dashboardToolStripMenuItem.Text = "Dashboard";
            this.dashboardToolStripMenuItem.Click += new System.EventHandler(this.dashboardToolStripMenuItem_Click);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(142, 22);
            this.toolStripDropDownButton2.Text = "Empirical Analysis";
            // 
            // toolStripDropDownButton3
            // 
            this.toolStripDropDownButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.indexingProcessesToolStripMenuItem,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.processQueryToolStripMenuItem});
            this.toolStripDropDownButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton3.Image")));
            this.toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            this.toolStripDropDownButton3.Size = new System.Drawing.Size(136, 22);
            this.toolStripDropDownButton3.Text = "Manage Process";
            this.toolStripDropDownButton3.ToolTipText = "Manage Process";
            // 
            // indexingProcessesToolStripMenuItem
            // 
            this.indexingProcessesToolStripMenuItem.Name = "indexingProcessesToolStripMenuItem";
            this.indexingProcessesToolStripMenuItem.Size = new System.Drawing.Size(189, 24);
            this.indexingProcessesToolStripMenuItem.Text = "Indexing Processes";
            this.indexingProcessesToolStripMenuItem.Click += new System.EventHandler(this.indexingProcessesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem2.Image")));
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(189, 24);
            this.toolStripMenuItem2.Text = "Similarity Search";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(189, 24);
            this.toolStripMenuItem3.Text = "Compare Process";
            // 
            // processQueryToolStripMenuItem
            // 
            this.processQueryToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("processQueryToolStripMenuItem.Image")));
            this.processQueryToolStripMenuItem.Name = "processQueryToolStripMenuItem";
            this.processQueryToolStripMenuItem.Size = new System.Drawing.Size(189, 24);
            this.processQueryToolStripMenuItem.Text = "Process Query";
            this.processQueryToolStripMenuItem.Click += new System.EventHandler(this.processQueryToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // filterOutSoundToolStripMenuItem
            // 
            this.filterOutSoundToolStripMenuItem.Name = "filterOutSoundToolStripMenuItem";
            this.filterOutSoundToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.filterOutSoundToolStripMenuItem.Text = "Filter Out Sound Acyclic";
            this.filterOutSoundToolStripMenuItem.Click += new System.EventHandler(this.filterOutSoundToolStripMenuItem_Click);
            // 
            // frmMain_TEST
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 724);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.menuMain);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuMain;
            this.Name = "frmMain_TEST";
            this.Text = "gProAnalyzer 1.1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuMain;
        private System.Windows.Forms.ToolStripMenuItem menuMakeNetwork;
        private System.Windows.Forms.ToolStripMenuItem menuAnalysisNetwork;
        private System.Windows.Forms.ToolStripMenuItem mnuSimulation;
        private System.Windows.Forms.ToolStripMenuItem menuMakeRandom;
        private System.Windows.Forms.ToolStripMenuItem menuMakeIBM;
        private System.Windows.Forms.ToolStripMenuItem menuMakeSAP;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem mnuSimulation_2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem processQueryToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ToolStripMenuItem indexingProcessesToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton4;
        private System.Windows.Forms.ToolStripMenuItem sESEAnalyzerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simulationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dashboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ePMLToNetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitRigidsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterOutSoundToolStripMenuItem;
    }
}

