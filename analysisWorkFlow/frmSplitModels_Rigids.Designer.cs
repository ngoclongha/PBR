namespace gProAnalyzer
{
    partial class frmSplitModels_Rigids
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
            this.menuMake = new System.Windows.Forms.MenuStrip();
            this.mnuMakeNetwork = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserMake = new System.Windows.Forms.FolderBrowserDialog();
            this.loadFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtLoadFolder = new System.Windows.Forms.TextBox();
            this.groupSave = new System.Windows.Forms.GroupBox();
            this.btnSetFolder = new System.Windows.Forms.Button();
            this.txtSaveFolder = new System.Windows.Forms.TextBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMake.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupSave.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuMake
            // 
            this.menuMake.AllowMerge = false;
            this.menuMake.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMakeNetwork,
            this.toolStripMenuItem1});
            this.menuMake.Location = new System.Drawing.Point(0, 0);
            this.menuMake.Name = "menuMake";
            this.menuMake.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuMake.Size = new System.Drawing.Size(390, 24);
            this.menuMake.TabIndex = 65;
            this.menuMake.Text = "menuStrip1";
            // 
            // mnuMakeNetwork
            // 
            this.mnuMakeNetwork.Name = "mnuMakeNetwork";
            this.mnuMakeNetwork.Size = new System.Drawing.Size(109, 20);
            this.mnuMakeNetwork.Text = "Convert Network";
            this.mnuMakeNetwork.Click += new System.EventHandler(this.mnuMakeNetwork_Click);
            // 
            // loadFileDialog
            // 
            this.loadFileDialog.FileName = "loadFileDialog";
            this.loadFileDialog.Multiselect = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnLoad);
            this.groupBox1.Controls.Add(this.txtLoadFolder);
            this.groupBox1.Location = new System.Drawing.Point(10, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(368, 77);
            this.groupBox1.TabIndex = 67;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "< Load NET Files >";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(17, 22);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(84, 34);
            this.btnLoad.TabIndex = 53;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // txtLoadFolder
            // 
            this.txtLoadFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLoadFolder.BackColor = System.Drawing.Color.White;
            this.txtLoadFolder.Location = new System.Drawing.Point(106, 28);
            this.txtLoadFolder.Name = "txtLoadFolder";
            this.txtLoadFolder.ReadOnly = true;
            this.txtLoadFolder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtLoadFolder.Size = new System.Drawing.Size(239, 20);
            this.txtLoadFolder.TabIndex = 54;
            // 
            // groupSave
            // 
            this.groupSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupSave.Controls.Add(this.btnSetFolder);
            this.groupSave.Controls.Add(this.txtSaveFolder);
            this.groupSave.Location = new System.Drawing.Point(12, 120);
            this.groupSave.Name = "groupSave";
            this.groupSave.Size = new System.Drawing.Size(366, 75);
            this.groupSave.TabIndex = 66;
            this.groupSave.TabStop = false;
            this.groupSave.Text = "< Saving Condition >";
            // 
            // btnSetFolder
            // 
            this.btnSetFolder.Location = new System.Drawing.Point(17, 22);
            this.btnSetFolder.Name = "btnSetFolder";
            this.btnSetFolder.Size = new System.Drawing.Size(84, 34);
            this.btnSetFolder.TabIndex = 53;
            this.btnSetFolder.Text = "Save Folder";
            this.btnSetFolder.UseVisualStyleBackColor = true;
            this.btnSetFolder.Click += new System.EventHandler(this.btnSetFolder_Click);
            // 
            // txtSaveFolder
            // 
            this.txtSaveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaveFolder.BackColor = System.Drawing.Color.White;
            this.txtSaveFolder.Location = new System.Drawing.Point(106, 28);
            this.txtSaveFolder.Name = "txtSaveFolder";
            this.txtSaveFolder.ReadOnly = true;
            this.txtSaveFolder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtSaveFolder.Size = new System.Drawing.Size(237, 20);
            this.txtSaveFolder.TabIndex = 54;
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(127, 20);
            this.toolStripMenuItem1.Text = "Count Disconnected";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // frmSplitModels
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 243);
            this.Controls.Add(this.menuMake);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupSave);
            this.Name = "frmSplitModels";
            this.Text = "frmSplitModels";
            this.menuMake.ResumeLayout(false);
            this.menuMake.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupSave.ResumeLayout(false);
            this.groupSave.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuMake;
        private System.Windows.Forms.ToolStripMenuItem mnuMakeNetwork;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserMake;
        private System.Windows.Forms.OpenFileDialog loadFileDialog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.TextBox txtLoadFolder;
        private System.Windows.Forms.GroupBox groupSave;
        private System.Windows.Forms.Button btnSetFolder;
        private System.Windows.Forms.TextBox txtSaveFolder;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    }
}