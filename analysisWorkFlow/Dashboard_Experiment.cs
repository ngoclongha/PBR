using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace gProAnalyzer
{
    public partial class Dashboard_Experiment : Form
    {
        public static DataTable dt = new DataTable();
        public static DataRow row1;
        public static string[] sFilePaths;
        public static string[] sFileNames;

        private gProAnalyzer.Preprocessing.clsLoadGraph loadG;
        private gProAnalyzer.GraphVariables.clsGraph graph;
        private gProAnalyzer.GraphVariables.clsLoop clsLoop;
        private gProAnalyzer.GraphVariables.clsSESE clsSESE;
        private gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
        private gProAnalyzer.GraphVariables.clsError clsError;
        private gProAnalyzer.Run_Analysis_Verification runVerification;

        public Dashboard_Experiment()
        {
            InitializeComponent();
            loadG = new Preprocessing.clsLoadGraph();
            graph = new GraphVariables.clsGraph();
            clsLoop = new GraphVariables.clsLoop();
            clsSESE = new GraphVariables.clsSESE();
            clsHWLS = new gProAnalyzer.GraphVariables.clsHWLS();
            clsError = new gProAnalyzer.GraphVariables.clsError();            
            runVerification = new gProAnalyzer.Run_Analysis_Verification();

            progressBar1.Visible = false;
            btnRun.Visible = false;
            exportExcel.Visible = false;
        }

        public void Initialized_All()
        {
            progressBar1.Visible = true;
            btnRun.Visible = true;
            exportExcel.Visible = true;
            //gridView.ColumnCount = 80;
            //gridView.Columns[0].Name = 
            //Add needed column for experimental results
            dt = new DataTable();
            dt.Columns.Add("ID", typeof(string));                       //[0]
            dt.Columns.Add("File Name", typeof(string));                //[1]
            dt.Columns.Add("Syntax Error", typeof(string));             //[2]
            dt.Columns.Add("Total Org", typeof(string));                //[3]
            dt.Columns.Add("Starts", typeof(string));                   //[4]
            dt.Columns.Add("Ends", typeof(string));                     //[5]
            dt.Columns.Add("Events", typeof(string));                   //[6]
            dt.Columns.Add("Task", typeof(string));                     //[7]
            dt.Columns.Add("OR", typeof(string));                       //[8]
            dt.Columns.Add("OR Join", typeof(string));                  //[9]
            dt.Columns.Add("OR Split", typeof(string));                 //[10]
            dt.Columns.Add("XOR", typeof(string));                      //[11]
            dt.Columns.Add("XOR Join", typeof(string));                 //[12]
            dt.Columns.Add("XOR Split", typeof(string));                //[13]
            dt.Columns.Add("AND", typeof(string));                      //[14]
            dt.Columns.Add("AND Join", typeof(string));                 //[15]
            dt.Columns.Add("AND Split", typeof(string));                //[16]
            dt.Columns.Add("Edges", typeof(string));                    //[17]
            dt.Columns.Add("Edge Factor", typeof(string));              //[18]
            dt.Columns.Add("Percentage Gateways", typeof(string));      //[19]
            
            dt.Columns.Add("Total Loops", typeof(string));              //[20]
            dt.Columns.Add("NL", typeof(string));                       //[21]
            dt.Columns.Add("IL", typeof(string));                       //[22]
            dt.Columns.Add("Max Depth Loops", typeof(string));          //[23]

            dt.Columns.Add("Total SESEs", typeof(string));              //[24]
            dt.Columns.Add("Polygon", typeof(string));                  //[25]
            dt.Columns.Add("Bonds", typeof(string));                    //[26]
            dt.Columns.Add("Rigid", typeof(string));                    //[27]
            dt.Columns.Add("Nodes Bond", typeof(string));               //[28]
            dt.Columns.Add("Nodes Rigid", typeof(string));              //[29]
            dt.Columns.Add("Max Depth SESE", typeof(string));           //[30]

            dt.Columns.Add("Type-1 Nodes", typeof(string));             //[31]
            dt.Columns.Add("Type-2 Nodes", typeof(string));             //[32]
            dt.Columns.Add("Type-2 Entries", typeof(string));           //[33]
            dt.Columns.Add("Type-2 Exits", typeof(string));             //[34]
            dt.Columns.Add("Type-2 BS", typeof(string));                //[35]

            dt.Columns.Add("Type-3 Nodes", typeof(string));             //[36]
            dt.Columns.Add("Type-3 Entries", typeof(string));           //[37]
            dt.Columns.Add("Type-3 Exits", typeof(string));             //[38]

            //Processed Data =================================================
            dt.Columns.Add("Total Proc", typeof(string));               //[39]
            dt.Columns.Add("Event Proc", typeof(string));               //[40]
            dt.Columns.Add("Task Proc", typeof(string));                //[41]
            dt.Columns.Add("OR Proc", typeof(string));                  //[42]
            dt.Columns.Add("SS_OR", typeof(string));                    //[43]
            dt.Columns.Add("OR Not Correct", typeof(string));           //[44]       
            dt.Columns.Add("EE_OR", typeof(string));                    //[45]
            dt.Columns.Add("OR Join Proc", typeof(string));             //[46]
            dt.Columns.Add("OR Split Proc", typeof(string));            //[47]

            dt.Columns.Add("OR Split for SS", typeof(string));                      //[48]
            dt.Columns.Add("OR Split for non-SS", typeof(string));                  //[49]
            dt.Columns.Add("Edges from OR Split for SS", typeof(string));           //[50]
            dt.Columns.Add("Edges from OR Split for non-SS", typeof(string));       //[51]
            dt.Columns.Add("#Edges in PdFlow(SS) --- [old] Percent of node in PdFlow(SS)", typeof(string));        //[52]
            dt.Columns.Add("#Edges out of PdFlow(SS) ---- [old] Percent of node in PdFlow(SOS(se))", typeof(string));   //[53]

            dt.Columns.Add("XOR Proc", typeof(string));                 //[54]
            dt.Columns.Add("SS_XOR", typeof(string));                   //[55]
            dt.Columns.Add("EE_XOR", typeof(string));                   //[56]
            dt.Columns.Add("XOR Join Proc", typeof(string));           //[57]
            dt.Columns.Add("XOR Split Proc", typeof(string));            //[58]
            dt.Columns.Add("AND Proc", typeof(string));                 //[59]
            dt.Columns.Add("SS_And", typeof(string));                   //[60]
            dt.Columns.Add("EE_AND", typeof(string));                   //[61]
            dt.Columns.Add("AND Join Proc", typeof(string));            //[62]
            dt.Columns.Add("AND Split Proc", typeof(string));           //[63]

            dt.Columns.Add("Edges Proc", typeof(string));               //[63]
            dt.Columns.Add("BJ & BS", typeof(string));                  //[65]
            dt.Columns.Add("BJ", typeof(string));                       //[66]
            dt.Columns.Add("BS", typeof(string));                       //[67]
            dt.Columns.Add("Edges Factor Proc", typeof(string));        //[68]
            dt.Columns.Add("Percentage Gateways Proc", typeof(string)); //[69]

            dt.Columns.Add("Error Loop Construct", typeof(string));         //[70]
            dt.Columns.Add("Error Acyclic-Decomposition", typeof(string));  //[71]
            dt.Columns.Add("Error Acyclic-reduced", typeof(string));        //[72]

            dt.Columns.Add("VR 1", typeof(string));                     //[73]
            dt.Columns.Add("VR 2", typeof(string));                     //[74]    
            dt.Columns.Add("VR 3", typeof(string));                     //[75]
            dt.Columns.Add("VR 4", typeof(string));                     //[76]
            dt.Columns.Add("VR 5", typeof(string));                     //[77]
            dt.Columns.Add("VR 6", typeof(string));                     //[78]
            dt.Columns.Add("VR 7", typeof(string));                     //[79]
            dt.Columns.Add("VR 8", typeof(string));                     //[80]

            dt.Columns.Add("Time Total", typeof(string));               //[81]
            dt.Columns.Add("SESE Identification", typeof(string));      //[82] //OLD "Time Type-1"
            dt.Columns.Add("Verification All", typeof(string));         //[83] //"Time Find Loops"
            dt.Columns.Add("Time IL", typeof(string));                  //[84]  //Time check IRREDUCIBLE LOOP
            dt.Columns.Add("Time Check BOND", typeof(string));          //[85] //OLD "Time Check Loop"
            dt.Columns.Add("Time Check RIGID", typeof(string));       //[86] //OLD "Time Check Acyclic"           

            dt.Columns.Add("Total Instances", typeof(string));          //[87]

            dt.Columns.Add("Number of OR-split in computed Rigid", typeof(string));          //[88]
            dt.Columns.Add("Number of outgoing edges of those OR-split", typeof(string));          //[89]
            dt.Columns.Add("# Split GateWay", typeof(string));
            dt.Columns.Add("# Join Gateway", typeof(string)); 


            //========New data====================================================

            dt.Columns.Add("# edges in PdFlow", typeof(string));
            dt.Columns.Add("# edges out of PdFlow", typeof(string));

            dt.Columns.Add("# actual errors LoS", typeof(string));
            dt.Columns.Add("# actual errors DL", typeof(string));
            dt.Columns.Add("# potential errors LoS", typeof(string));
            dt.Columns.Add("# potential errors DL", typeof(string));
            dt.Columns.Add("# dominated errors LoS", typeof(string));
            dt.Columns.Add("# dominated errors DL", typeof(string));
            dt.Columns.Add("# dominated Potential errors LoS", typeof(string));
            dt.Columns.Add("# dominated Potential errors DL", typeof(string));

            dt.Columns.Add("# PdFlow - actual errors LoS", typeof(string));
            dt.Columns.Add("# PdFlow - actual errors DL", typeof(string));
            dt.Columns.Add("# PdFlow - potential errors LoS", typeof(string));
            dt.Columns.Add("# PdFlow - potential errors DL", typeof(string));
            dt.Columns.Add("# PdFlow - dominated errors LoS", typeof(string));
            dt.Columns.Add("# PdFlow - dominated errors DL", typeof(string));
            dt.Columns.Add("# PdFlow - dominated Potential errors LoS", typeof(string));
            dt.Columns.Add("# PdFlow - dominated Potential errors DL", typeof(string));

            dt.Columns.Add("# NL exits", typeof(string));
            dt.Columns.Add("# NL backward split", typeof(string));
            dt.Columns.Add("# NL exits & backward split", typeof(string));
            dt.Columns.Add("# edges in Fwd", typeof(string));
            dt.Columns.Add("# edges in Bwd", typeof(string));
            dt.Columns.Add("# dummy nodes", typeof(string));

            dt.Columns.Add("# Type 1 SESE", typeof(string));
            dt.Columns.Add("# Type 2 SESE", typeof(string));
            dt.Columns.Add("# Type 3 SESE", typeof(string));
            dt.Columns.Add("# Type 4 SESE", typeof(string));
            dt.Columns.Add("# Type 4 Bond SESE", typeof(string));
            dt.Columns.Add("# Type 4 Rigid SESE", typeof(string));
            dt.Columns.Add("# Type 4 Bond NL Inside", typeof(string));
            dt.Columns.Add("# Type 4 Bond IL Inside", typeof(string));
            dt.Columns.Add("# Type 4 Rigid NL Inside", typeof(string));
            dt.Columns.Add("# Type 4 Rigid IL Inside", typeof(string));

            dt.Columns.Add("Label Duplication", typeof(string));

        }

        public void btnLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Browse";
            openFileDialog1.Filter = "Network Documents (*.net) | *.net";

            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "") return;

            sFilePaths = openFileDialog1.FileNames;
            sFileNames = openFileDialog1.SafeFileNames;
            NumOfRun.Text = "1";

            btnRun.Visible = true;
        }       

        public void btnRun_Click(object sender, EventArgs e)
        {
            Initialized_All();
            int progress = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = sFileNames.Length;

            for (int run = 0; run < sFileNames.Length; run++)
            {                
                row1 = dt.NewRow(); //new row for a single process model
                row1[0] = "abc";

                loadG.Load_Data(ref graph, graph.orgNet, sFilePaths[run], true);

                int NofRun = Convert.ToInt32(NumOfRun.Text);
                int retVal = 0;

                try { 
                    retVal = runVerification.run_VerificationG(ref graph, ref clsHWLS, ref clsLoop, ref clsSESE, ref clsError, NofRun);
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error at: " + sFilePaths[run] + " " + er.Message, "Code Errors");
                }


                Display_Basic(run);

                if (retVal > 0)
                {
                    string errorM = "";
                    if (retVal == 1) errorM = "Irreducible Error";
                    else if (retVal == 2) errorM = "Concurrency Error";
                    else if (retVal == 3) errorM = "Syntex Error";
                    else if (retVal == 4) errorM = "Syntex & Irreducible Error";
                    else if (retVal == 5) errorM = "Syntex & Concurrency Error";
                    else if (retVal == 6) errorM = runVerification.CCs.ToString("#,#0");

                    row1[2] = errorM;
                }

                if (retVal == 0 || retVal == 3 || retVal == 6) Display_Result(run, false);
                else if (retVal == 2 || retVal == 5) Display_Result(run, false);

                dt.Rows.Add(row1);
                progress++;
                progressBar1.Value = progress;
            }

            //mnuSaveResult.Visible = true;

            //Final to bind to GridView
           
            gridView.DataSource = dt;
            
        }

        public void Display_Basic(int run)
        {
            int startCol = 0;
            row1[startCol++] = (run + 1).ToString();
            row1[startCol++] = sFileNames[run];

            startCol++; // for error message

            int[] nGW = Network_Inform(graph.baseNet, true); //true => count OR from original
            row1[startCol++] = graph.Network[graph.baseNet].nNode.ToString("#,#0"); //nNode            
            //row1[startCol++] = runVerification.informList[11].ToString("#,#0"); //original STARTs event
            row1[startCol++] = nGW[23].ToString("#,#0"); //just experiment for pure START EVENT
            row1[startCol++] = nGW[22].ToString("#,#0"); //original ENDs event (Re-count)

            for (int i = 0; i <= 10; i++)
                row1[startCol++] = nGW[i].ToString("#,#0");

            row1[startCol++] = graph.Network[graph.baseNet].nLink.ToString("#,#0"); //nLink            
            //for (int i = 11; i <= 13; i++)
                //row1[startCol++] = nGW[i].ToString("#,#0");

            double tempDbl = Convert.ToDouble(graph.Network[graph.baseNet].nLink) / Convert.ToDouble(graph.Network[graph.baseNet].nNode);
            row1[startCol++] = tempDbl.ToString("#0.00");
            tempDbl = Convert.ToDouble(nGW[2] + nGW[5] + nGW[8]) / Convert.ToDouble(graph.Network[graph.baseNet].nNode);
            row1[startCol++] = tempDbl.ToString("#0.00");
        }

        public void Display_Result(int run, bool bError)
        {
            int startCol = 20;
            int[] nGW = Network_Inform(graph.finalNet, false);
            int[] VR = VR_Inform(graph.finalNet);
            int nIL = Loop_Inform(clsLoop.orgLoop);

            row1[startCol++] = clsLoop.Loop[clsLoop.orgLoop].nLoop.ToString("#,#0");
            row1[startCol++] = (clsLoop.Loop[clsLoop.orgLoop].nLoop - nIL).ToString("#,#0");
            row1[startCol++] = nIL.ToString("#,#0");
            row1[startCol++] = clsLoop.Loop[clsLoop.orgLoop].maxDepth.ToString("#,#0");

            row1[startCol++] = (runVerification.informList[12] + runVerification.informList[13] + runVerification.informList[14]).ToString(); //SESE
            row1[startCol++] = runVerification.informList[12].ToString("#,#0"); //polygon
            row1[startCol++] = runVerification.informList[13].ToString("#,#0"); //bond
            row1[startCol++] = runVerification.informList[14].ToString("#,#0"); //rigid
            row1[startCol++] = runVerification.informList[16].ToString("#,#0"); //node in bond
            row1[startCol++] = runVerification.informList[17].ToString("#,#0"); //node in rigid
            row1[startCol++] = clsSESE.SESE[clsSESE.finalSESE].maxDepth.ToString("#,#0");

            //Split node *Type-1 *Type-2 *Type-3
            row1[startCol++] = runVerification.informList[0].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[3].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[4].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[5].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[6].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[7].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[8].ToString("#0.0000000000");
            row1[startCol++] = runVerification.informList[9].ToString("#0.0000000000");

            row1[startCol++] = (graph.Network[graph.finalNet].nNode - 2).ToString("#,#0"); //Total node (except START - END)
            row1[startCol++] = nGW[0].ToString("#,#0"); //Event
            row1[startCol++] = nGW[1].ToString("#,#0"); //Task
            row1[startCol++] = (nGW[2] + nGW[3] + nGW[4]).ToString("#,#0"); //# of OR
            row1[startCol++] = nGW[3].ToString("#,#0"); //SS_OR
            row1[startCol++] = runVerification.informList[23].ToString("#,#0"); //SS_OR not Corrected
            row1[startCol++] = nGW[4].ToString("#,#0"); //EE_OR
            row1[startCol++] = nGW[5].ToString("#,#0"); //OR-join exclude SS, EE
            row1[startCol++] = nGW[6].ToString("#,#0"); //OR-Split exclude SS, EE

            //node SS EE inform
            row1[startCol++] = runVerification.informList[18].ToString("#0.0000000000"); //# of OR split in rigids from OR split SS; SS_se
            row1[startCol++] = runVerification.informList[20].ToString("#0.0000000000"); //# of edges in rigids from OR split SS; SS_se
            row1[startCol++] = runVerification.informList[19].ToString("#0.0000000000"); //# of OR split in rigids from OR split non-SS
            row1[startCol++] = runVerification.informList[21].ToString("#0.0000000000"); //# of edges in rigids from OR split non-SS
            row1[startCol++] = runVerification.informList[10].ToString("#0.0000000000"); //# of nodes from Starts to CIPd //re-purpose this index for # of edges in PdFlow(SS)
            row1[startCol++] = runVerification.informList[22].ToString("#0.0000000000"); //# of node in PdFlow(SOS(se)) //re-purpose this index
            for (int k = 7; k <= 16; k++) {
                if (k == 7) {                 
                    nGW[k] = nGW[k] + nGW[8] + nGW[9];
                }
                if (k == 12) {                 
                    nGW[k] = nGW[k] + nGW[13] + nGW[14];
                }
                row1[startCol++] = nGW[k].ToString("#,#0");
            }

            //Edges
            row1[startCol++] = graph.Network[graph.finalNet].nLink.ToString("#,#0");
            for (int k = 18; k <= 20; k++) {             
                row1[startCol++] = nGW[k].ToString("#,#0");
            }
            //edge factor & % of gateways
            double tempDbl = Convert.ToDouble(graph.Network[graph.finalNet].nLink) / Convert.ToDouble(graph.Network[graph.finalNet].nNode);
            row1[startCol++] = tempDbl.ToString("#0.00");
            tempDbl = Convert.ToDouble(graph.Network[graph.finalNet].nNode - (nGW[0] + nGW[1] + 2)) / Convert.ToDouble(graph.Network[graph.finalNet].nNode);
            row1[startCol++] = tempDbl.ToString("#0.00"); //percentage of gateways

            if (!bError)
            {
                //Error -  loop constructs, acyclic-decomposed flows (of loops),  final acyclic-reduced model
                int[] nE = Error_Inform();
                row1[startCol++] = nE[0].ToString("#,#0");
                row1[startCol++] = nE[1].ToString("#,#0");
                row1[startCol++] = nE[2].ToString("#,#0");

                //Rule Violation VR#1 -> VR#8
                row1[startCol++] = VR[1].ToString();
                row1[startCol++] = VR[2].ToString();
                row1[startCol++] = VR[3].ToString();
                row1[startCol++] = VR[4].ToString();
                row1[startCol++] = VR[5].ToString();
                row1[startCol++] = VR[6].ToString();
                row1[startCol++] = VR[7].ToString();
                row1[startCol++] = VR[8].ToString();

                //Time                
                row1[startCol++] = runVerification.run_Time[0].ToString("#0.0000000000");
                row1[startCol++] = runVerification.run_Time[1].ToString("#0.0000000000");                
                row1[startCol++] = runVerification.run_Time[2].ToString("#0.0000000000");
                //break here
                row1[startCol++] = runVerification.run_Time[5].ToString("#0.0000000000"); //IL time 
                row1[startCol++] = runVerification.run_Time[3].ToString("#0.0000000000"); //bond time
                row1[startCol++] = runVerification.run_Time[4].ToString("#0.0000000000"); //rigid time
            }

            row1[startCol++] = runVerification.total_inst.ToString("#0.0000000000");

            //details of rigids instance on OR-split
            row1[startCol++] = runVerification.rig_01[0].ToString("#0.0000000000");
            row1[startCol++] = runVerification.rig_01[1].ToString("#0.0000000000");
            row1[startCol++] = runVerification.GW_Count_S_J[0].ToString("#0.0000000000");
            row1[startCol++] = runVerification.GW_Count_S_J[1].ToString("#0.0000000000");

            //=========New Data=========================================================================

            //additional info for Type of error
            row1[startCol++] = runVerification.informList[10].ToString("#,#0"); //#edges in pdFlow
            row1[startCol++] = runVerification.informList[22].ToString("#,#0"); //out
            //total errors acyclic
            row1[startCol++] = runVerification.Real_err[0].ToString("#,#0"); //real LoS
            row1[startCol++] = runVerification.Real_err[1].ToString("#,#0"); //real DL
            row1[startCol++] = runVerification.Potential_err[0].ToString("#,#0"); //Potential LoS
            row1[startCol++] = runVerification.Potential_err[1].ToString("#,#0"); //Potential DL
            row1[startCol++] = runVerification.Dominated_err[0].ToString("#,#0"); //Dom LoS
            row1[startCol++] = runVerification.Dominated_err[1].ToString("#,#0"); //Dom DL
            row1[startCol++] = runVerification.Dominated_err_Potential[0].ToString("#,#0"); //Dom_Potential LoS
            row1[startCol++] = runVerification.Dominated_err_Potential[1].ToString("#,#0"); //Dom_Potential DL

            //PdFlow error
            row1[startCol++] = runVerification.Real_err[2].ToString("#,#0"); //real LoS
            row1[startCol++] = runVerification.Real_err[3].ToString("#,#0"); //real DL
            row1[startCol++] = runVerification.Potential_err[2].ToString("#,#0"); //Potential LoS
            row1[startCol++] = runVerification.Potential_err[3].ToString("#,#0"); //Potential DL
            row1[startCol++] = runVerification.Dominated_err[2].ToString("#,#0"); //Dom LoS
            row1[startCol++] = runVerification.Dominated_err[3].ToString("#,#0"); //Dom DL
            row1[startCol++] = runVerification.Dominated_err_Potential[2].ToString("#,#0"); //Dom_Potential LoS
            row1[startCol++] = runVerification.Dominated_err_Potential[3].ToString("#,#0"); //Dom_Potential DL

            //NL infor - More
            row1[startCol++] = runVerification.ext_informlist[0].ToString("#,#0"); //#exit
            row1[startCol++] = runVerification.ext_informlist[1].ToString("#,#0"); //#bs
            row1[startCol++] = runVerification.ext_informlist[2].ToString("#,#0"); //#exit&bs
            row1[startCol++] = runVerification.ext_informlist[4].ToString("#,#0"); //#edges in Fwd
            row1[startCol++] = runVerification.ext_informlist[5].ToString("#,#0"); //#edges in Bwd
            row1[startCol++] = runVerification.ext_informlist[3].ToString("#,#0"); //#dummy

            //Type 1-4 bonds and rigids June_2020
            row1[startCol++] = runVerification.type_1_4[0].ToString("#,#0"); //#Type 1
            row1[startCol++] = runVerification.type_1_4[1].ToString("#,#0"); //#Type 2
            row1[startCol++] = runVerification.type_1_4[2].ToString("#,#0"); //#Type 3
            row1[startCol++] = runVerification.type_1_4[3].ToString("#,#0"); //#Type 4
            row1[startCol++] = runVerification.type_1_4[4].ToString("#,#0"); //#Type 4 Bonds
            row1[startCol++] = runVerification.type_1_4[5].ToString("#,#0"); //#Type 4 Rigids
            row1[startCol++] = runVerification.type_1_4[6].ToString("#,#0"); //#Type 4 Bonds_NL
            row1[startCol++] = runVerification.type_1_4[7].ToString("#,#0"); //#Type 4 Bonds_IL
            row1[startCol++] = runVerification.type_1_4[8].ToString("#,#0"); //#Type 4 Rigids_NL
            row1[startCol++] = runVerification.type_1_4[9].ToString("#,#0"); //#Type 4 Rigids_IL

            row1[startCol++] = runVerification.dup_label.ToString(); //Check label duplication

        }

        public int[] Network_Inform(int currentN, bool check)
        {
            int[] nGW = new int[24];

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (check == true) // only for base model (We count all syntax error case for START/ END events)
                {
                    if (graph.Network[currentN].Node[i].nPre == 0) nGW[23]++; //Start event
                    if (graph.Network[currentN].Node[i].nPost == 0) nGW[22]++; //End event
                }
                if (graph.Network[currentN].Node[i].Kind == "EVENT")
                {
                    nGW[0]++;
                    //if (graph.Network[currentN].Node[i].nPre == 0) nGW[23]++; //Start event
                    //if (graph.Network[currentN].Node[i].nPost == 0) nGW[22]++; //End event
                }
                else if (graph.Network[currentN].Node[i].Kind == "TASK")
                {
                    nGW[1]++;
                }
                else if (graph.Network[currentN].Node[i].Kind == "OR")
                {
                    if (!check) //check = false (Display_Results) Find OR Exclude SSS EE
                    {
                        if (graph.Network[currentN].Node[i].Name != "SS" && graph.Network[currentN].Node[i].Name != "EE") //exclude SS and EE and _se _sx
                        {
                            nGW[2]++; //OR
                            if (graph.Network[currentN].Node[i].nPre > 1) nGW[5]++;
                            if (graph.Network[currentN].Node[i].nPost > 1) nGW[6]++;
                        }
                        if (graph.Network[currentN].Node[i].Name == "SS")
                        {
                            nGW[3]++;
                            //if (graph.Network[currentN].Node[i].SOS_NotCorrected == true) nGW[21]++;
                        }
                        if (graph.Network[currentN].Node[i].Name == "EE")
                        {
                            nGW[4]++;
                        }
                    }
                    else if (graph.Network[currentN].Node[i].Name != "SS" && graph.Network[currentN].Node[i].Name != "EE") //not count SS or EE
                    {
                        nGW[2]++; //OR
                        if (graph.Network[currentN].Node[i].nPre > 1) nGW[3]++;
                        if (graph.Network[currentN].Node[i].nPost > 1) nGW[4]++;
                    }
                }
                else if (graph.Network[currentN].Node[i].Kind == "XOR")
                {
                    if (!check)
                    {
                        if (graph.Network[currentN].Node[i].Name != "SS" && graph.Network[currentN].Node[i].Name != "EE") //exclude SS and EE and _se _sx
                        {
                            nGW[7]++;//XOR exclude SS or EE..
                            if (graph.Network[currentN].Node[i].nPre > 1) nGW[10]++;//XOR join exclude SS or EE..
                            if (graph.Network[currentN].Node[i].nPost > 1) nGW[11]++;//XOR split exclude SS or EE..
                        }
                        if (graph.Network[currentN].Node[i].Name == "SS")
                        {
                            nGW[8]++;//SS_XOR
                        }
                        if (graph.Network[currentN].Node[i].Name == "EE")
                        {
                            nGW[9]++;//EE_XOR
                        }
                    }
                    else if (graph.Network[currentN].Node[i].Name != "SS" && graph.Network[currentN].Node[i].Name != "EE")
                    {
                        nGW[5]++; //OR
                        if (graph.Network[currentN].Node[i].nPre > 1) nGW[6]++;
                        if (graph.Network[currentN].Node[i].nPost > 1) nGW[7]++;
                    }
                }
                else if (graph.Network[currentN].Node[i].Kind == "AND")
                {
                    if (!check)
                    {
                        if (graph.Network[currentN].Node[i].Name != "SS" && graph.Network[currentN].Node[i].Name != "EE") //exclude SS and EE and _se _sx
                        {
                            nGW[12]++;//AND exclude SS or EE..
                            if (graph.Network[currentN].Node[i].nPre > 1) nGW[15]++;//AND join exclude SS or EE..
                            if (graph.Network[currentN].Node[i].nPost > 1) nGW[16]++;//AND split exclude SS or EE..
                        }
                        if (graph.Network[currentN].Node[i].Name == "SS")
                        {
                            nGW[13]++;
                        }
                        if (graph.Network[currentN].Node[i].Name == "EE")
                        {
                            nGW[14]++;
                        }
                    }
                    else if (graph.Network[currentN].Node[i].Name != "SS" && graph.Network[currentN].Node[i].Name != "EE")
                    {
                        nGW[8]++; //OR
                        if (graph.Network[currentN].Node[i].nPre > 1) nGW[9]++;
                        if (graph.Network[currentN].Node[i].nPost > 1) nGW[10]++;
                    }

                }
            }
            for (int i = 0; i < graph.Network[currentN].nLink; i++) //link
            {
                if (!check)
                {
                    if (graph.Network[currentN].Link[i].bBackJ && graph.Network[currentN].Link[i].bBackS) nGW[18]++;//BJ & BS
                    else if (graph.Network[currentN].Link[i].bBackJ) nGW[19]++;//BJ
                    else if (graph.Network[currentN].Link[i].bBackS) nGW[20]++;//BS
                }
            }

            return nGW;
        }

        public int[] VR_Inform(int currentN)
        {
            int[] vr = new int[9];
            for (int i = 0; i < clsError.nError; i++)
            {
                if ((clsError.Error[i].messageNum <= 2) && (clsError.Error[i].Loop.IndexOf("NL") == -1)) vr[1]++;
                if (clsError.Error[i].messageNum > 2 && clsError.Error[i].messageNum <= 8 && clsError.Error[i].Loop.IndexOf("NL") == -1) vr[2]++; //maybe duplicate
                if ((clsError.Error[i].messageNum == 9) && (clsError.Error[i].Loop.IndexOf("NL") == -1)) vr[3]++;
                if (((clsError.Error[i].messageNum >= 10 && clsError.Error[i].messageNum <= 11) || ((clsError.Error[i].messageNum >= 27 && clsError.Error[i].messageNum <= 28))) 
                    && (clsError.Error[i].Loop.IndexOf("NL") == -1)) vr[4]++;
                if (((clsError.Error[i].messageNum >= 12 && clsError.Error[i].messageNum <= 13) || clsError.Error[i].messageNum >= 16 && clsError.Error[i].messageNum <= 21) //IL Loop gateways
                    && (clsError.Error[i].Loop.IndexOf("NL") == -1)) 
                    vr[5]++;
                if (clsError.Error[i].messageNum >= 22 && clsError.Error[i].messageNum <= 26 && (clsError.Error[i].Loop.IndexOf("NL") == -1)) 
                    vr[6]++;
                if (clsError.Error[i].messageNum >= 32 && clsError.Error[i].messageNum <= 35 && (clsError.Error[i].Loop.IndexOf("NL") == -1)) 
                    vr[7]++;
                if (clsError.Error[i].Loop.IndexOf("NL") != -1) 
                    vr[8]++;
            }
            return vr;
        }

        private int Loop_Inform(int workLoop)
        {
            int nIL = 0;

            for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[i].Irreducible) nIL++;
            }
            return nIL;
        }

        private int[] Error_Inform()
        {
            int[] nE = new int[3];

            for (int i = 0; i < clsError.nError; i++) //[0] Loop construct; [1] Decomposed; [2] Final-Acyclic
            {
                if ((clsError.Error[i].messageNum >= 0 && clsError.Error[i].messageNum <= 2) || (clsError.Error[i].messageNum >= 16 && clsError.Error[i].messageNum <= 21)) // 0,1,2, 16-21
                    nE[0]++; //Loop construc
                else if (clsError.Error[i].messageNum >= 27 && clsError.Error[i].messageNum <= 28) nE[2]++; //final Acyclic
                else nE[1]++; //Decomposed
            }

            return nE;
        }

        private void exportExcel_Click(object sender, EventArgs e)
        {
            if(gridView.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook workbook =  ExcelApp.Application.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel.Worksheet worksheet = null;

                for (int i = 1; i < gridView.Columns.Count + 1; i++)
                    ExcelApp.Cells[1, i] = gridView.Columns[i - 1].HeaderText;

                for (int i = 0; i < gridView.Rows.Count - 1; i++)
                    for (int j = 0; j < gridView.Columns.Count; j++)
                        ExcelApp.Cells[i + 2, j + 1] = gridView.Rows[i].Cells[j].Value.ToString();

                ExcelApp.Columns.AutoFit();
                

                var saveFileDialoge = new SaveFileDialog();
                saveFileDialoge.FileName = "Results_Simulation";
                saveFileDialoge.DefaultExt = ".xlsx";
                if (saveFileDialoge.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveFileDialoge.FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                ExcelApp.Visible = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Initialized_All();
            int progress = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = sFileNames.Length;

            for (int run = 0; run < sFileNames.Length; run++)
            {
                row1 = dt.NewRow(); //new row for a single process model
                row1[0] = "abc";

                loadG.Load_Data(ref graph, graph.orgNet, sFilePaths[run], true);

                int NofRun = Convert.ToInt32(NumOfRun.Text);
                int retVal = 0;

                try
                {
                    retVal = runVerification.run_TestG(ref graph, ref clsHWLS, ref clsLoop, ref clsSESE, ref clsError, NofRun);
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error at: " + sFilePaths[run] + " " + er.Message, "Code Errors");
                }

                Display_Basic(run); //stop at startcol = 19
                int startCol = 20;
                row1[startCol++] = runVerification.dup_label.ToString(); 

                dt.Rows.Add(row1);
                progress++;
                progressBar1.Value = progress;
            }


            gridView.DataSource = dt;
        }
    }
}
