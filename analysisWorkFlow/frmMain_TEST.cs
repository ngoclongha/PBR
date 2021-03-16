using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gProAnalyzer
{
    public partial class frmMain_TEST : Form
    {

        public frmMain_TEST()
        {
            InitializeComponent();
        }

        // Load form 'frmAnalysisNetwork' when click the mainmenu 'menuAnalysisNetwork'
        private void menuAnalysisNetwork_Click(object sender, EventArgs e)
        {
            frmAnalysisNetwork frmLoad = new frmAnalysisNetwork();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }

        // Load form 'frmSimulation' when click the mainmenu 'mnuSimulation'
        private void mnuSimulation_Click(object sender, EventArgs e)
        {
            //frmSimulation frmLoad = new frmSimulation();
            //frmLoad.MdiParent = this;
            //frmLoad.Show();

            frmBasicSimulation bscLoad = new frmBasicSimulation();
            //bscLoad.MdiParent = this;
            bscLoad.Show();

        }

        // Load form 'frmMakeNetwork' when click the mainmenu 'menuMakeRandom'
        private void menuMakeRandom_Click(object sender, EventArgs e)
        {
            frmMakeNetwork frmLoad = new frmMakeNetwork();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }

        // Load form 'frmFromIBM' when click the mainmenu 'menuMakeIBM'
        private void menuMakeIBM_Click(object sender, EventArgs e)
        {
            frmFromIBM frmLoad = new frmFromIBM();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }

        // Load form 'frmFromSAP' when click the mainmenu 'menuMakeSAP'
        private void menuMakeSAP_Click(object sender, EventArgs e)
        {
            frmFromSAP frmLoad = new frmFromSAP();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmSplitModels frmLoad = new frmSplitModels();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }

        private void mnuSimulation_2_Click(object sender, EventArgs e)
        {
            //frmSimulation_2 frmLoad = new frmSimulation_2();
            //frmLoad.MdiParent = this;
            //frmLoad.Show();        
        }

        private void aBCToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void indexingProcessesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gProAnalyzer.GraphVariables.clsGraph graph; // m_Network represent the class "clsAnalysisNetwork"
            gProAnalyzer.Preprocessing.clsLoadGraph loadGraph;
            gProAnalyzer.GraphVariables.clsLoop clsLoop;
            gProAnalyzer.GraphVariables.clsSESE clsSESE;
            gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
            gProAnalyzer.GraphVariables.clsHWLS clsHWLS_Untangle;
            gProAnalyzer.Functionalities.LoopIdentification loopNode;
            gProAnalyzer.Functionalities.IndexingPM indexing;           

            //Initialized All
            graph = new gProAnalyzer.GraphVariables.clsGraph();
            clsLoop = new GraphVariables.clsLoop();
            clsSESE = new GraphVariables.clsSESE();
            loopNode = new gProAnalyzer.Functionalities.LoopIdentification();
            loadGraph = new gProAnalyzer.Preprocessing.clsLoadGraph();
            clsHWLS = new gProAnalyzer.GraphVariables.clsHWLS();
            clsHWLS_Untangle = new gProAnalyzer.GraphVariables.clsHWLS();
            indexing = new gProAnalyzer.Functionalities.IndexingPM();

            //load file
            openFileDialog.Title = "Browse";
            openFileDialog.Filter = "Network Documents (*.net) | *.net";
            openFileDialog.FileName = "";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == "") return;

            string sFilePath = openFileDialog.FileName;
            //lblFileName.Text = openFileDialog.SafeFileName;

            loadGraph.Load_Data(ref graph, graph.orgNet, sFilePath, true);
            //Display information to tabInform
            this.Text = "AnalysisNetwork  --  " + openFileDialog.SafeFileName;

            //gProAnalyzer.Functionalities.IndexingPM.start_Indexing(ref graph, ref clsHWLS, ref clsHWLS_Untangle, ref clsLoop, ref clsSESE);
        }

        private void processQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmProcessQuery frmPQ = new frmProcessQuery();        

            frmPQ.ShowDialog(this);
        }

        private void sESEAnalyzerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAnalysisNetwork_SESE frmSESE = new frmAnalysisNetwork_SESE();
            frmSESE.ShowDialog(this);
        }

        private void simulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBasicSimulation frmBsSim = new frmBasicSimulation();
            frmBsSim.ShowDialog(this);
        }

        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dashboard_Experiment frmDashboard = new Dashboard_Experiment();
            frmDashboard.ShowDialog(this);
        }

        private void ePMLToNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EPML2NET epml2net = new EPML2NET();
            epml2net.ShowDialog(this);
        }

        private void splitRigidsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSplitModels_Rigids frmLoad = new frmSplitModels_Rigids();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }

        private void filterOutSoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmFilterOutSound frmLoad = new frmFilterOutSound();
            //frmLoad.MdiParent = this;
            frmLoad.Show();
        }        




    }
}
