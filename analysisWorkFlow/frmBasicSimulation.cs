using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer
{
    public partial class frmBasicSimulation : Form
    {
        private string[] sFilePaths;
        private string[] sFileNames;
        private clsAnaysisNetwork m_Network;
        private gProAnalyzer.Preprocessing.clsLoadGraph loadG;
        private gProAnalyzer.GraphVariables.clsGraph graph;
        private gProAnalyzer.GraphVariables.clsLoop clsLoop;
        private gProAnalyzer.GraphVariables.clsSESE clsSESE;
        private gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
        private gProAnalyzer.GraphVariables.clsError clsError;

        private gProAnalyzer.Testing test;
        private gProAnalyzer.Run_Analysis_Verification runVerification;

        public frmBasicSimulation()
        {
            loadG = new Preprocessing.clsLoadGraph();
            graph = new GraphVariables.clsGraph();
            clsLoop = new GraphVariables.clsLoop();
            clsSESE = new GraphVariables.clsSESE();
            clsHWLS = new gProAnalyzer.GraphVariables.clsHWLS();
            clsError = new gProAnalyzer.GraphVariables.clsError();

            test = new gProAnalyzer.Testing();
            runVerification = new gProAnalyzer.Run_Analysis_Verification();

            InitializeComponent();
        }

        //fill data
        private void btnLoad_Click(object sender, EventArgs e)
        {
            openFileDialogSml.Title = "Browse";
            openFileDialogSml.Filter = "Network Documents (*.net) | *.net";

            openFileDialogSml.FileName = "";
            openFileDialogSml.Multiselect = true;

            openFileDialogSml.ShowDialog();

            if (openFileDialogSml.FileName == "") return;


            After_Load();
        }
        private void After_Load()
        {
            sFilePaths = openFileDialogSml.FileNames;
            sFileNames = openFileDialogSml.SafeFileNames;

            //txtFolder.Text = loadFileDialog.FileName.Remove(loadFileDialog.FileName.Length - loadFileDialog.SafeFileName.Length);
            //txtFileN.Text = sFileNames.Length.ToString();            
        }        

        private void buttonRun_Click(object sender, EventArgs e)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\WriteLines_Verification_01.txt", true))
            {
                for (int run = 0; run < sFileNames.Length; run++)
                {
                    //m_Network = new clsAnaysisNetwork();
                    //m_Network.Load_Data(m_Network.orgNet, sFilePaths[run], true);
                    //int retVal = m_Network.Run_Analysis(1);
                    //file.WriteLine(sFileNames[run].ToString() + ";" + m_Network.informList[13] + ";" + m_Network.informList[14]);

                    loadG.Load_Data(ref graph, graph.orgNet, sFilePaths[run], true);

                    int rel = runVerification.run_VerificationG(ref graph, ref clsHWLS, ref clsLoop, ref clsSESE, ref clsError, 1);
                    string SyntaxErr = "";
                    if (rel > 0)
                    {
                        string errorM = "";
                        if (rel == 1) errorM = "Irreducible Error";
                        else if (rel == 2) errorM = "Concurrency Error";
                        else if (rel == 3) errorM = "Syntex Error";
                        else if (rel == 4) errorM = "Syntex & Irreducible Error";
                        else if (rel == 5) errorM = "Syntex & Concurrency Error";
                        else if (rel == 6) errorM = runVerification.CCs.ToString("#,#0");

                        SyntaxErr = errorM;
                    }

                    //file.WriteLine(sFileNames[run].ToString() + ";" + clsSESE.SESE[clsSESE.finalSESE].nSESE + ";" + test.duration); //correct, but not count Type 3 SESE (NL unique exit)
                    //file.WriteLine(sFileNames[run].ToString() + ";" + runVerification.finalTime + ";" + runVerification.rig[0] + ";" + runVerification.rig[1] + ";" + runVerification.rig[2] + ";" + runVerification.rig[3] + ";"
                    //    + runVerification.rig[4] + ";" + runVerification.rig[5] + ";");

                    string newData_Loop = "";
                    newData_Loop = ";" + runVerification.AND_Loop[1] + ";" + runVerification.AND_Loop[3] + ";" + runVerification.AND_Loop[5] + ";" + runVerification.AND_Loop[7]
                        + ";" + runVerification.OR_Loop[1] + ";" + runVerification.OR_Loop[3] + ";" + runVerification.OR_Loop[5] + ";" + runVerification.OR_Loop[7]
                    + ";" + runVerification.XOR_Loop[1] + ";" + runVerification.XOR_Loop[3] + ";" + runVerification.XOR_Loop[5] + ";" + runVerification.XOR_Loop[7];

                    string newData_Loop_Type = ";";
                    for (int i = 1; i < 29; i++) //1-28
                    {
                        newData_Loop_Type = newData_Loop_Type + runVerification.ext_informlist_2[i] + ";";
                    }

                    string newCrossTable_EnEx_Bond = ";";
                    for (int i = 1; i < 13; i++)
                    {
                        newCrossTable_EnEx_Bond = newCrossTable_EnEx_Bond + runVerification.En_Ex_Type[i] + ";";
                    }

                    /*
                    file.WriteLine(sFileNames[run].ToString() + ";" + SyntaxErr + ";" + graph.Network[graph.finalNet].nNode + ";" + runVerification.run_Time[0] + ";"
                        + runVerification.XOR_join_total[0] + ";" + runVerification.XOR_join_total[1] + ";" + runVerification.XOR_join_total[2] + ";"
                        + runVerification.XOR_join_err[0] + ";" + runVerification.XOR_join_err[1] + ";" + runVerification.XOR_join_err[2] + ";"
                        + runVerification.AND_join_total[0] + ";" + runVerification.AND_join_total[1] + ";" + runVerification.AND_join_total[2] + ";"
                        + runVerification.AND_join_err[0] + ";" + runVerification.AND_join_err[1] + ";" + runVerification.AND_join_err[2] + ";"
                        + runVerification.OR_join_total[0] + ";" + runVerification.OR_join_total[1] + ";" + runVerification.OR_join_total[2] + ";"
                        + runVerification.XOR_entry[0] + ";" + runVerification.XOR_entry[1] + ";" + runVerification.XOR_entry[2] + ";" + runVerification.XOR_entry[3] + ";"
                        + runVerification.AND_entry[0] + ";" + runVerification.AND_entry[1] + ";" + runVerification.AND_entry[2] + ";" + runVerification.AND_entry[3] + ";"
                        + runVerification.OR_entry[0] + ";" + runVerification.OR_entry[1] + ";" + runVerification.OR_entry[2] + ";" + runVerification.OR_entry[3] + ";"
                        + runVerification.informList[23] + ";" + runVerification.informList[24] + ";" + runVerification.bond_rigid_PdF[0] + ";" + runVerification.bond_rigid_PdF[1] + ";"
                        + "---" + newData_Loop + ";" + "---" + newData_Loop_Type);
                    */

                    file.WriteLine(sFileNames[run].ToString() + ";" + SyntaxErr + ";" + graph.Network[graph.finalNet].nNode + ";" + runVerification.run_Time[0] + ";"
                        + runVerification.XOR_join_total[0] + ";" + runVerification.XOR_join_total[1] + ";" + runVerification.XOR_join_total[2] + ";"
                        + runVerification.XOR_join_err[0] + ";" + runVerification.XOR_join_err[1] + ";" + runVerification.XOR_join_err[2] + ";"
                        + runVerification.AND_join_total[0] + ";" + runVerification.AND_join_total[1] + ";" + runVerification.AND_join_total[2] + ";"
                        + runVerification.AND_join_err[0] + ";" + runVerification.AND_join_err[1] + ";" + runVerification.AND_join_err[2] + ";"
                        + runVerification.OR_join_total[0] + ";" + runVerification.OR_join_total[1] + ";" + runVerification.OR_join_total[2] + ";"
                        + runVerification.OR_join_total[3] + ";" + runVerification.OR_join_total[4] + ";"
                        + runVerification.XOR_entry[0] + ";" + runVerification.XOR_entry[1] + ";" + runVerification.XOR_entry[2] + ";" + runVerification.XOR_entry[3] + ";"
                        + runVerification.AND_entry[0] + ";" + runVerification.AND_entry[1] + ";" + runVerification.AND_entry[2] + ";" + runVerification.AND_entry[3] + ";"
                        + runVerification.OR_entry[0] + ";" + runVerification.OR_entry[1] + ";" + runVerification.OR_entry[2] + ";" + runVerification.OR_entry[3] + ";"
                        + runVerification.informList[23] + ";" + runVerification.informList[24] + ";" + "bond_rigid_PdF_0" + ";" + "bond_rigid_PdF_1" + ";"
                        + "---" + newData_Loop + ";" + "---" + newData_Loop_Type + ";" + "__New_En_Ex__" + newCrossTable_EnEx_Bond);



                    //file.WriteLine(sFileNames[run].ToString() + ";" + graph.Network[graph.finalNet].nNode + ";" + runVerification.run_Time[0] + ";"
                    //    + runVerification.XOR_err_efwd_ebwd[0] + ";" + runVerification.XOR_err_efwd_ebwd[1] + ";"
                    //    + runVerification.AND_err_efwd_ebwd[0] + ";" + runVerification.AND_err_efwd_ebwd[1] + ";");

                }
            }
            MessageBox.Show("Finish", "Finish");
        }
    }
}
