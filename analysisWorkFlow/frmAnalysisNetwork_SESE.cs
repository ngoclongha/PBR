using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Northwoods.Go;
using Northwoods.Go.Layout;
using System.Threading;

namespace gProAnalyzer
{
    public partial class frmAnalysisNetwork_SESE : Form
    {
        //start using class "clsAnalysisNetwork"
        private gProAnalyzer.GraphVariables.clsGraph graph;
        private gProAnalyzer.GraphVariables.clsSESE clsSESE;
        private gProAnalyzer.GraphVariables.clsLoop clsLoop;
        private gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
        private gProAnalyzer.GraphVariables.clsError clsError;
        private gProAnalyzer.Preprocessing.clsLoadGraph loadGraph;
        private gProAnalyzer.Run_Analysis_SESE runSESE;
        private gProAnalyzer.Run_Analysis_Verification runVerification;

        private int nSearchNode = 0;
        private int[] searchNode;


        private string[] errorMessage;

        //Using Go Library from Northwoods
        private GoBasicNode[] node;
        private GoLink[] link;
        //Main entrance code

        //Error List and InitializeComponent
        public frmAnalysisNetwork_SESE()
        {
            errorMessage = new string[40];
            errorMessage[0] = "Rule 1 : The entry node of a Natural loop should be XOR gateway";
            errorMessage[1] = "Rule 1 : The exit node of a Natural loop should be XOR gateway";
            errorMessage[2] = "Rule 1 : The backward split node of a Natural loop should be XOR gateway";
            errorMessage[3] = "Rule 2.1 : The XOR join node in forward flow of a loop should have just one instant join link"; //bFor = true; instanceFlow error finding
            errorMessage[4] = "Rule 2.1 : The XOR join node in backward flow of a loop should have just one instant join link"; //bFor = false;====
            errorMessage[5] = "Rule 2.1 : The AND join node in forward flow of a loop should have same instant join link"; //bFor = true;
            errorMessage[6] = "Rule 2.1 : The AND join node in backward flow of a loop should have same instant join link"; //bFor = false;=====
            errorMessage[7] = "Rule 2.xx : The exit (or back-split) node in forward flow of a loop should not be contained in a parallel structure";
            errorMessage[8] = "Rule 2.2 : The exit node in backward flow of a loop should not be contained in a parallel structure"; //NO NEED IT ==============
            errorMessage[9] = "Rule 3 : In case multiple exits of a natural Loop lead to the same loop-external successor, that successor should be XOR-join";
            errorMessage[10] = "Rule 4 : The XOR join node in the final acyclic reduced model should have just one instant join link"; //bFor = true;
            errorMessage[11] = "Rule 4 : The AND join node in the final acyclic reduced model should have same instant join link"; //bFor = false;=====
            //errorMessage[12] = "Rule 5 : All exclusive entry nodes of an Irreducible loop should be XOR gateway";
            //errorMessage[13] = "Rule 5 : The exit node of an Irreducible loop should be XOR gateway";
            //errorMessage[14] = "Rule 6 : There should be no structural conflicts in each final instantiated natural loops of an irreducible loop";
            //errorMessage[15] = "Rule 7 : All concurrent entry nodes of an Irreducible loop should not be AND-joins, and each CE should have starting entries which are XOR-joins";
            //============= New message =========
            errorMessage[16] = "Rule 5.1.1 : All of its entries should be XOR gateways for irreducible loop having only exclusive entries";
            errorMessage[17] = "Rule 5.1.2 : All of its exits should be XOR gateways for irreducible loop having only exclusive entries";
            errorMessage[18] = "Rule 5.2.1 : All exclusive entries should be XOR-joins for irreducible loop having concurrent entries";
            errorMessage[19] = "Rule 5.2.2 : All concurrent entries should not be AND-joins for irreducible loop having concurrent entries";
            errorMessage[20] = "Rule 5.2.3 : Each CE should have starting entries which are XOR-joins for irreducible loop having concurrent entries";
            errorMessage[21] = "Rule 5.2.4 : All of its exits should be XOR-splits for irreducible loop having concurrent entries";

            errorMessage[22] = "Rule 6 : CIPd should be in the loop and PdFlow should not contain any exit";
            errorMessage[23] = "Rule 6 : iCID should be in the loop and iDFlow should not contain any exits";
            errorMessage[24] = "Rule 6.1 : LoS (DFlow_PdFlow) DFlow and PdFlow should be conflict-free";
            errorMessage[25] = "Rule 6.2 : LoS (eFwd_IL) iDFlow and PdFlow should be conflict-free";
            errorMessage[26] = "Rule 6.3 : LoS (eBwd_PdFlow) The subgraph (HFlow) which contains exit nodes should be conflict-free";
            //SESE construct //rule 4
            errorMessage[27] = "Rule 9.1: Lack of Synchronization in the SESE";
            errorMessage[28] = "Rule 9.2: Deadlock in the SESE";

            errorMessage[29] = "Rule 2.2: Warning - The exit (or Back-split) node in efwd() maybe contain in prallel structure";
            errorMessage[30] = "Rule 2.2: Warning - The exit (or Back-split) node in ebwd() maybe contain in prallel structure";
            errorMessage[31] = "Rule 6.4: Warning - The exit (or Back-split) node in HFlow() maybe contain in prallel structure";

            errorMessage[32] = "Rule 7.1: CIPd(entries) of an irreducible loop should be in the loop";
            errorMessage[33] = "Rule 7.1: iCID(entries) of an irreducible loop should be in the loop";
            errorMessage[34] = "Rule 7.2: The PdFlow(entries) of an irreducible loop should not contain exits";
            errorMessage[35] = "Rule 7.3: The iDFlow(entries) of an irreducible loop should not contain exits";

            errorMessage[36] = "Rule 8: All natural loops instantiated from an irreducible loop which an exclusive entry should have not structural conflicts";

            //extend acyclic flow eFwd & eBwd
            errorMessage[37] = "Rule 6.1 : DL (DFlow_PdFlow) DFlow and PdFlow should be conflict-free";
            errorMessage[38] = "Rule 6.2 : DL (eFwd_IL) iDFlow and PdFlow should be conflict-free";
            errorMessage[39] = "Rule 6.3 : DL (eBwd_PdFlow) The subgraph (HFlow) which contains exit nodes should be conflict-free";

            InitializeComponent();

            //Initialized All            
            graph = new gProAnalyzer.GraphVariables.clsGraph();
            clsSESE = new gProAnalyzer.GraphVariables.clsSESE();
            clsLoop = new gProAnalyzer.GraphVariables.clsLoop();
            clsHWLS = new gProAnalyzer.GraphVariables.clsHWLS();
            clsError = new gProAnalyzer.GraphVariables.clsError();
            loadGraph = new gProAnalyzer.Preprocessing.clsLoadGraph();
            runSESE = new gProAnalyzer.Run_Analysis_SESE();
            runVerification = new gProAnalyzer.Run_Analysis_Verification();
        }



        // Load the network for analysis and display the information
        private void mnuLoadNetwork_Click(object sender, EventArgs e)
        {
            loadFileDialog.Title = "Browse";
            loadFileDialog.Filter = "Network Documents (*.net) | *.net";

            loadFileDialog.FileName = "";
            loadFileDialog.ShowDialog();
            if (loadFileDialog.FileName == "") return;


            Initialize_All();

            string sFilePath = loadFileDialog.FileName;
            lblFileName.Text = loadFileDialog.SafeFileName;

            //Call load_Data from class clsAnlysis
            //MessageBox.Show(m_Network.orgNet.ToString());
            loadGraph.Load_Data(ref graph, graph.orgNet, sFilePath, true);

            //Pre-Split Node.
            //graph.Pre_Split(m_Network.orgNet);

            //Display information to tabInform
            Display_Inform(graph.orgNet, true);
            Display_Network(graph.orgNet, -1);

            Draw_Network(graph.baseNet, -1, -1, goViewB);
            Draw_Network(graph.orgNet, -1, -1, goViewO); //switch orgNet and midNet
            tabGoView.SelectedIndex = 1;

            mnuAnalysisNetwork.Visible = true;
            //mnuAnalysisStep.Visible = true;

            this.Text = "gProAnalyzer :: SESE Identification Module -- " + loadFileDialog.SafeFileName;
            //======test=====
           // Draw_Network(m_Network.orgNet, -1, m_Network.finalSESE, goViewSE); // SESE
            //Display_Error(m_Network.orgNet);
            //test==========
        }

        //Clear all form view
        private void Initialize_All()
        {
            //if (graph != null) graph = null;
            //m_Network = new clsAnaysisNetwork();


            lblFileName.Text = "";
            lblorgN.Text = "";
            lblorgE.Text = "";
            lblorgEF.Text = "";
            lblorgRG.Text = "";
            lblsptN.Text = "";
            lblsptE.Text = "";
            lblsptEF.Text = "";
            lblsptRG.Text = "";
            //Tab Base Network
            goViewB.Document.Clear();
            //Tab Base Original Network
            goViewO.Document.Clear();
            //Tab Loop Network
            //goViewL.Document.Clear();
            //Tab SESE network
            goViewSE.Document.Clear();
            //Tab Entire Network
            //iewE.Document.Clear();

            //Tab Loop reduction network
            //goViewS.Document.Clear();

            //Tab Final Acyclic network
            //goViewA.Document.Clear();
            //Tab Dom Tree
            goViewD.Document.Clear();
            //Tab Post Dominance
            goViewRD.Document.Clear();

            //goViewForest.Name = "Nesting Forest";
            Initialize_List();
        }

        //clear all list
        private void Initialize_List()
        {
            lst_Node.Items.Clear();
            lst_Link.Items.Clear();
            lst_Loop.Items.Clear();

            //lst_Error1.Items.Clear();
            //lst_Error2.Items.Clear();
            //lst_Error3.Items.Clear();
        }

        //Run Analysis
        private void mnuAnalysisNetwork_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            //RUN ANALYSIS => Return the error
            int retVal = 0;
            //runSESE.run_SESE(ref graph, ref clsHWLS, ref clsHWLS, ref clsLoop, ref clsSESE); //retVal ~ ErrorNum
            int errNum = runVerification.run_VerificationG(ref graph, ref clsHWLS, ref clsLoop, ref clsSESE, ref clsError, 1);
            //int retVal = m_Network.Run_Analysis_Temp(1);   
            watch.Stop();

            //MessageBox.Show("The System has finished identify the SESE region in: " + watch.ElapsedMilliseconds.ToString() + " milisecond", "Finish");

            //Draw_Network(6, -1, -1, goViewA);
            if (retVal == 1)
            {
                MessageBox.Show("Irreducible Error : This network can not be handled");
                return;
            }
            else if (retVal == 4)
            {
                MessageBox.Show("Syntex & Irreducible Error : This network can not be handled");
                return;
            }

            //clear all list
            Initialize_List();

            //Call an local function "Draw_Network" Draw Loop Network
            //Draw_Network(m_Network.finalNet, m_Network.orgLoop, -1, goViewL); // LOOP
            tabGoView.SelectedIndex = 1;

            progressBar1.Value = 0;
            backgroundWorker1.RunWorkerAsync();
        }


        //Display
        private void Display_Inform(int currentN, bool bOrg)
        {
            int nGW = 0;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Kind == "XOR" || graph.Network[currentN].Node[i].Kind == "AND") nGW++;
            }
            double imEF = Convert.ToDouble(graph.Network[currentN].nLink) / Convert.ToDouble(graph.Network[currentN].nNode);
            double imRG = Convert.ToDouble(nGW) / Convert.ToDouble(graph.Network[currentN].nNode);

            if (bOrg)
            {
                lblorgN.Text = graph.Network[currentN].nNode.ToString("#,#");
                lblorgE.Text = graph.Network[currentN].nLink.ToString("#,#");
                lblorgEF.Text = imEF.ToString("#0.00");
                lblorgRG.Text = imRG.ToString("#0.00");
            }
            else
            {
                lblsptN.Text = graph.Network[currentN].nNode.ToString("#,#");
                lblsptE.Text = graph.Network[currentN].nLink.ToString("#,#");
                lblsptEF.Text = imEF.ToString("#0.00");
                lblsptRG.Text = imRG.ToString("#0.00");
            }

        }

        private void Display_Network(int currentN, int workLoop)
        {

            string imStr = "";

            if (currentN >= 0)
            {
                lst_Node.Items.Clear();
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    lst_Node.Items.Add(i.ToString());

                    string tempName = graph.Network[currentN].Node[i].Name + graph.Network[currentN].Node[i].Type_I + graph.Network[currentN].Node[i].Type_II;
                    lst_Node.Items[i].SubItems.Add(tempName);//m_Network.Network[currentN].Node[i].orgNum.ToString());

                    lst_Node.Items[i].SubItems.Add(graph.Network[currentN].Node[i].Kind);
                    lst_Node.Items[i].SubItems.Add(graph.Network[currentN].Node[i].Special);
                    lst_Node.Items[i].SubItems.Add(graph.Network[currentN].Node[i].depth.ToString());

                    //lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].Type_I);
                    //lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].Type_II);

                }

                lst_Link.Items.Clear();
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {

                    lst_Link.Items.Add(i.ToString());

                    int tempNode = graph.Network[currentN].Link[i].fromNode;
                    string tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;
                    lst_Link.Items[i].SubItems.Add(tempName);

                    tempNode = graph.Network[currentN].Link[i].toNode;
                    tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;
                    lst_Link.Items[i].SubItems.Add(tempName);

                    imStr = "";
                    if (graph.Network[currentN].Link[i].bBackJ) imStr += "BJ";
                    if (graph.Network[currentN].Link[i].bBackS) imStr += "BS";
                    lst_Link.Items[i].SubItems.Add(imStr);
                }
            }

            if (workLoop >= 0)
            {
                lst_Loop.Items.Clear();
                for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
                {

                    lst_Loop.Items.Add(i.ToString());

                    imStr = "N";
                    if (clsLoop.Loop[workLoop].Loop[i].Irreducible) imStr = "I";
                    lst_Loop.Items[i].SubItems.Add(imStr);
                    lst_Loop.Items[i].SubItems.Add(clsLoop.Loop[workLoop].Loop[i].depth.ToString());
                    lst_Loop.Items[i].SubItems.Add(clsLoop.Loop[workLoop].Loop[i].parentLoop.ToString());

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nChild; k++)
                    {
                        imStr += clsLoop.Loop[workLoop].Loop[i].child[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nChild - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                    int tempNode = clsLoop.Loop[workLoop].Loop[i].header;
                    string tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;
                    lst_Loop.Items[i].SubItems.Add(tempName);//m_Network.Loop[workLoop].Loop[i].header.ToString());

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++)
                    {
                        tempNode = clsLoop.Loop[workLoop].Loop[i].Entry[k];
                        tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;

                        imStr += tempName;// m_Network.Loop[workLoop].Loop[i].Entry[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nEntry - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nExit; k++)
                    {
                        tempNode = clsLoop.Loop[workLoop].Loop[i].Exit[k];
                        tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;

                        imStr += tempName; // m_Network.Loop[workLoop].Loop[i].Exit[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nExit - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nNode; k++)
                    {
                        tempNode = clsLoop.Loop[workLoop].Loop[i].Node[k];
                        tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;

                        imStr += tempName;// m_Network.Loop[workLoop].Loop[i].Node[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nNode - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                }
            }
        }

        private void Display_Error(int currentN) //Display into GridView and Mark the gateways also
        {

            // Rule 1, 3, 5, (6) //LOOP ERRORS
            lst_Error1.Items.Clear();
            int id = 0;
            for (int i = 0; i < clsError.nError; i++)
            {
                // 0, 1, 2, 9,// 16 17 18 19 20 21
                if ((clsError.Error[i].messageNum >= 0 && clsError.Error[i].messageNum <= 2) || (clsError.Error[i].messageNum >= 16 && clsError.Error[i].messageNum <= 21) || clsError.Error[i].messageNum == 9)
                {
                    lst_Error1.Items.Add((id + 1).ToString());
                    lst_Error1.Items[id].Tag = i.ToString();
                    lst_Error1.Items[id].SubItems.Add(clsError.Error[i].Node);

                    int tempNode = Convert.ToInt32(clsError.Error[i].Node);
                    string tempName;
                    if (tempNode < 0) tempName = tempNode.ToString();
                    else tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;
                    lst_Error1.Items[id].SubItems.Add(tempName);

                    //m_Network.Network[currentN].Node[i].Name + m_Network.Network[currentN].Node[i].Type_I + m_Network.Network[currentN].Node[i].Type_II;

                    //string tempS = m_Network.Error[i].Node.Contains("->");
                    int nodeE = Convert.ToInt32(clsError.Error[i].Node);
                    if (nodeE >= 0)
                    {
                        node[nodeE].Shape.PenWidth = 3;
                        node[nodeE].Shape.PenColor = Color.Red;

                        if (node[nodeE].ToolTipText != "") node[nodeE].ToolTipText += "\r\n";
                        node[nodeE].ToolTipText += "Conflicts of loop constructs - " + (id + 1).ToString();
                    }
                    lst_Error1.Items[id].SubItems.Add(clsError.Error[i].Loop);
                    lst_Error1.Items[id].SubItems.Add(clsError.Error[i].currentKind);
                    lst_Error1.Items[id].SubItems.Add(errorMessage[clsError.Error[i].messageNum]);
                    id++;
                }
            }

            // Rule 2, 6 //INSTANCE FLOW ERRORS (OF LOOPS)
            lst_Error2.Items.Clear();
            id = 0;
            for (int i = 0; i < clsError.nError; i++)
            {
                // 3, 4, 5, 6, (7), (8) // 29 30 -parallel structure // 22 23 24 25 26
                //if ((m_Network.Error[i].messageNum >= 0 && m_Network.Error[i].messageNum <= 2) || (m_Network.Error[i].messageNum >= 16 && m_Network.Error[i].messageNum <= 21)) continue;
                //if (m_Network.Error[i].messageNum >= 27 && m_Network.Error[i].messageNum <= 28) continue;
                if ((clsError.Error[i].messageNum >= 3 && clsError.Error[i].messageNum <= 6) || (clsError.Error[i].messageNum >= 22 && clsError.Error[i].messageNum <= 26) ||
                    (clsError.Error[i].messageNum >= 29 && clsError.Error[i].messageNum <= 30) || (clsError.Error[i].messageNum >= 32 && clsError.Error[i].messageNum <= 35) ||
                    (clsError.Error[i].messageNum >= 37 && clsError.Error[i].messageNum <= 39))
                {
                    lst_Error2.Items.Add((id + 1).ToString());
                    lst_Error2.Items[id].Tag = i.ToString();
                    lst_Error2.Items[id].SubItems.Add(clsError.Error[i].Node);

                    int tempNode = Convert.ToInt32(clsError.Error[i].Node);
                    string tempName;
                    if (tempNode < 0) tempName = tempNode.ToString();
                    else tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;
                    lst_Error2.Items[id].SubItems.Add(tempName);

                    int nodeE = Convert.ToInt32(clsError.Error[i].Node);
                    if (nodeE >= 0)
                    {
                        node[nodeE].Shape.PenWidth = 3;
                        node[nodeE].Shape.PenColor = Color.Red;

                        if (node[nodeE].ToolTipText != "") node[nodeE].ToolTipText += "\r\n";
                        node[nodeE].ToolTipText += "Conflicts in acyclic-decomposed flows (of loops) - " + (id + 1).ToString();
                    }

                    lst_Error2.Items[id].SubItems.Add(clsError.Error[i].Loop);
                    lst_Error2.Items[id].SubItems.Add(clsError.Error[i].currentKind);
                    lst_Error2.Items[id].SubItems.Add(errorMessage[clsError.Error[i].messageNum]);
                    id++;
                }
            }

            // Rule 4 8 //FINAL ACYCLIC-REDUCED MODEL
            lst_Error3.Items.Clear();
            id = 0;
            for (int i = 0; i < clsError.nError; i++)
            {
                // 10, 11 // 27 28
                if ((clsError.Error[i].messageNum >= 10 && clsError.Error[i].messageNum <= 11 || (clsError.Error[i].messageNum >= 27 && clsError.Error[i].messageNum <= 28)))
                {
                    lst_Error3.Items.Add((id + 1).ToString());
                    lst_Error3.Items[id].Tag = i.ToString();
                    lst_Error3.Items[id].SubItems.Add(clsError.Error[i].Node);

                    int tempNode = Convert.ToInt32(clsError.Error[i].Node);
                    string tempName;
                    if (tempNode < 0) tempName = tempNode.ToString();
                    else tempName = graph.Network[currentN].Node[tempNode].Name + graph.Network[currentN].Node[tempNode].Type_I + graph.Network[currentN].Node[tempNode].Type_II;
                    lst_Error3.Items[id].SubItems.Add(tempName);

                    int nodeE = Convert.ToInt32(clsError.Error[i].Node);
                    if (nodeE >= 0)
                    {
                        node[nodeE].Shape.PenWidth = 3;
                        node[nodeE].Shape.PenColor = Color.Red;

                        if (node[nodeE].ToolTipText != "") node[nodeE].ToolTipText += "\r\n";
                        node[nodeE].ToolTipText += "Conflicts in final acyclic-reduced model - " + (id + 1).ToString();
                    }

                    //lst_Error3.Items[id].SubItems.Add(m_Network.Error[i].Loop);// there are no loop here, just SESE only
                    lst_Error3.Items[id].SubItems.Add(clsError.Error[i].SESE);
                    lst_Error3.Items[id].SubItems.Add(clsError.Error[i].currentKind);
                    lst_Error3.Items[id].SubItems.Add(errorMessage[clsError.Error[i].messageNum]);
                    id++;
                }
            }
        }

        private void Display_SESE_Error(int currentN)
        {
            // Rule 4
            int id = 0;
            for (int i = 0; i < clsError.nError; i++)
            {
                // 10, 11
                if (clsError.Error[i].messageNum <= 9 || clsError.Error[i].messageNum >= 12) continue;

                int nodeE = -1;
                for (int k = 0; k < graph.Network[currentN].nNode; k++)
                {
                    if (Convert.ToInt32(clsError.Error[i].Node) == graph.Network[currentN].Node[k].parentNum)
                    {
                        nodeE = k;
                        break;
                    }
                }
                if (nodeE >= 0)
                {
                    node[nodeE].Shape.PenWidth = 3;
                    node[nodeE].Shape.PenColor = Color.Red;

                    if (node[nodeE].ToolTipText != "") node[nodeE].ToolTipText += "\r\n";
                    node[nodeE].ToolTipText += "Conflicts in final acyclic-reduced model - " + (id + 1).ToString();
                }
                id++;
            }
        }
      

        //Draw new
        private void Draw_Network(int currentN, int workLoop, int workSESE, GoView goViewer)
        {

            goViewer.Document.Clear();

            GoDocument doc = goViewer.Document;

            int nNode = graph.Network[currentN].nNode;
            int nLink = graph.Network[currentN].nLink;


            node = new GoBasicNode[nNode];

            //Node 그리기 draw Node
            for (int i = 0; i < nNode; i++)
            {
                //if (m_Network.Network[currentN].Node[i].nPre <= 0 && m_Network.Network[currentN].Node[i].nPost <= 0) continue;

                node[i] = new GoBasicNode();

                node[i].LabelSpot = GoObject.TopCenter;//.Middle;
                node[i].Text = graph.Network[currentN].Node[i].Name + graph.Network[currentN].Node[i].Type_I + graph.Network[currentN].Node[i].Type_II;

                //모양
                if (graph.Network[currentN].Node[i].Kind == "OR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Ellipse;
                    //node[i].Shape.BrushColor = Color.LightGray;
                    node[i].Shape.BrushColor = Color.Transparent;
                }
                else if (graph.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Times;
                    node[i].Shape.BrushColor = Color.Transparent;
                }
                else if (graph.Network[currentN].Node[i].Kind == "AND")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Plus;
                    node[i].Shape.BrushColor = Color.Transparent;
                }
                else if (graph.Network[currentN].Node[i].Kind == "TASK")
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.BrushColor = Color.Transparent;
                    node[i].Shape.Size = new SizeF((float)40, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);
                }
                else if (graph.Network[currentN].Node[i].Kind == "EVENT")
                {
                    node[i].Shape = new GoHexagon();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.BrushColor = Color.Transparent;

                }
                else if (graph.Network[currentN].Node[i].Kind == "START")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleRight;

                    //node[i].LabelSpot = GoObject.MiddleCenter;
                }
                else if (graph.Network[currentN].Node[i].Kind == "END")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleLeft;
                }

                if (graph.Network[currentN].Node[i].Kind != "TASK")
                {
                    //크기
                    node[i].Shape.Size = new SizeF((float)30, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);
                }

                //색깔
                if (graph.Network[currentN].Node[i].Kind == "START") node[i].Shape.BrushColor = Color.YellowGreen;
                else if (graph.Network[currentN].Node[i].Kind == "END") node[i].Shape.BrushColor = Color.LightPink;

                node[i].ToolTipText = "";

                doc.Add(node[i]);
            }

            //Link 그리기

            GoLink protolink = goViewer.NewGoLink;
            protolink.ToArrow = true;
            //protolink.Orthogonal = true;
            protolink.AvoidsNodes = true;
            //protolink.PenWidth = 2;
            protolink.Style = GoStrokeStyle.RoundedLine;
            //protolink.PenColor = Color.Red;
            //protolink.BrushColor = Color.Red;


            link = new GoLink[nLink];

            for (int j = 0; j < nLink; j++)
            {
                if (graph.Network[currentN].Link[j].fromNode == graph.Network[currentN].Link[j].toNode) continue;

                IGoLink ilink = goViewer.CreateLink(node[graph.Network[currentN].Link[j].fromNode].Port, node[graph.Network[currentN].Link[j].toNode].Port);

                link[j] = ilink.GoObject as GoLink;

                if (graph.Network[currentN].Link[j].bBackJ && graph.Network[currentN].Link[j].bBackS)
                {
                    link[j].PenColor = Color.Purple;
                    link[j].ToolTipText = "BackSplit & BackJoin";
                }
                else if (graph.Network[currentN].Link[j].bBackJ)
                {
                    link[j].PenColor = Color.DarkGoldenrod;
                    link[j].ToolTipText = "BackJoin";
                }
                else if (graph.Network[currentN].Link[j].bBackS)
                {
                    link[j].PenColor = Color.Blue;
                    link[j].ToolTipText = "BackSplit";
                }
            }

            //link[0].PenColor = Color.Red;


            //정렬
            //GoLayoutForceDirected layout = new GoLayoutForceDirected();
            //layout.Document = this.goViewer.Document;
            //layout.SetsPortSpots = false;

            GoLayoutLayeredDigraph layout = new GoLayoutLayeredDigraph();
            layout.Document = goViewer.Document;
            layout.SetsPortSpots = false;
            //layout.ArrangementOrigin = new PointF(0, 0);
            layout.DirectionOption = GoLayoutDirection.Right;
            layout.LayerSpacing = 3;
            //layout.ColumnSpacing = 30;
            //layout.AggressiveOption = GoLayoutLayeredDigraphAggressive.More;
            //layout.InitializeOption = GoLayoutLayeredDigraphInitIndices.Naive;

            // maybe set other properties too . . .
            layout.PerformLayout();


            Draw_Loop_SESE(currentN, workLoop, workSESE, goViewer);            

        }

        private void Draw_DomTree(int currentN, int workLoop, int workSESE, GoView goViewer, bool bRev)
        {

            goViewer.Document.Clear();

            GoDocument doc = goViewer.Document;

            int nNode = graph.Network[currentN].nNode;

            node = new GoBasicNode[nNode];

            //Node 그리기
            for (int i = 0; i < nNode; i++)
            {
                //if (m_Network.Network[currentN].Node[i].nPre <= 0 && m_Network.Network[currentN].Node[i].nPost <= 0) continue;

                node[i] = new GoBasicNode();

                node[i].LabelSpot = GoObject.MiddleRight;//.TopCenter;//.Middle;
                node[i].Text = graph.Network[currentN].Node[i].Name + graph.Network[currentN].Node[i].Type_I + graph.Network[currentN].Node[i].Type_II;

                //모양
                if (graph.Network[currentN].Node[i].Kind == "OR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Ellipse;
                }
                else if (graph.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Times;
                    //node[i].Port.Style = GoPortStyle.None;
                }
                else if (graph.Network[currentN].Node[i].Kind == "AND")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Plus;
                }
                else if (graph.Network[currentN].Node[i].Kind == "TASK")
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.Size = new SizeF((float)40, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);

                }
                else if (graph.Network[currentN].Node[i].Kind == "EVENT")
                {
                    node[i].Shape = new GoHexagon();
                    node[i].Port.Style = GoPortStyle.None;

                }
                else if (graph.Network[currentN].Node[i].Kind == "START")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleRight;

                    //node[i].LabelSpot = GoObject.MiddleCenter;
                }
                else if (graph.Network[currentN].Node[i].Kind == "END")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleLeft;
                }

                //final draw===============
                if (graph.Network[currentN].Node[i].Kind != "TASK")
                {
                    //크기
                    node[i].Shape.Size = new SizeF((float)30, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);
                }

                //색깔
                if (graph.Network[currentN].Node[i].Kind == "START") node[i].Shape.BrushColor = Color.YellowGreen;
                else if (graph.Network[currentN].Node[i].Kind == "END") node[i].Shape.BrushColor = Color.LightPink;

                //E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
                //if (m_Network.Network[currentN].Node[i].Special == "E")
                //{
                //    node[i].Shape.BrushColor = Color.Khaki;
                //    node[i].ToolTipText = "Entry Node";// \r\n";
                //}
                //else if (m_Network.Network[currentN].Node[i].Special == "B")
                //{
                //    node[i].Shape.BrushColor = Color.LightCyan;
                //    node[i].ToolTipText = "BackSplit Node";
                //}
                //else if (m_Network.Network[currentN].Node[i].Special == "X")
                //{
                //    node[i].Shape.BrushColor = Color.LightBlue;
                //    node[i].ToolTipText = "Exit Node";
                //}
                //else if (m_Network.Network[currentN].Node[i].Special == "T")
                //{
                //    node[i].Shape.BrushColor = Color.Cyan;
                //    node[i].ToolTipText = "BackSplit & Exit Node";
                //}
                //else
                //{
                node[i].ToolTipText = "";
                //}

                doc.Add(node[i]);
            }

            //Link 그리기

            GoLink protolink = goViewer.NewGoLink;
            protolink.ToArrow = true;
            //protolink.Orthogonal = true;
            protolink.AvoidsNodes = true;
            //protolink.PenWidth = 2;
            protolink.Style = GoStrokeStyle.RoundedLine;
            //protolink.PenColor = Color.Red;
            //protolink.BrushColor = Color.Red;

            bool[,] makeLink = new bool[nNode, nNode];
            int nLink = 0;
            if (!bRev)
            {
                for (int i = 0; i < nNode; i++)
                {
                    for (int k = 1; k < graph.Network[currentN].Node[i].nDom; k++)
                    {
                        makeLink[graph.Network[currentN].Node[i].Dom[k - 1], graph.Network[currentN].Node[i].Dom[k]] = true;
                        nLink++;
                    }
                }
            }
            else
            {

                for (int i = 0; i < nNode; i++)
                {
                    for (int k = 1; k < graph.Network[currentN].Node[i].nDomRev; k++)
                    {
                        makeLink[graph.Network[currentN].Node[i].DomRev[k - 1], graph.Network[currentN].Node[i].DomRev[k]] = true;
                        nLink++;
                    }
                }
            }





            link = new GoLink[nLink];

            nLink = 0;
            for (int i = 0; i < nNode; i++)
            {
                for (int j = 0; j < nNode; j++)
                {
                    if (!makeLink[i, j]) continue;

                    IGoLink ilink = goViewer.CreateLink(node[i].Port, node[j].Port);

                    link[j] = ilink.GoObject as GoLink;
                }
            }



            //정렬

            GoLayoutLayeredDigraph layout = new GoLayoutLayeredDigraph();
            layout.Document = goViewer.Document;
            layout.SetsPortSpots = false;
            if (bRev) layout.DirectionOption = GoLayoutDirection.Up;
            else layout.DirectionOption = GoLayoutDirection.Down;
            layout.LayerSpacing = 3;
            layout.PerformLayout();

            Draw_Loop_SESE(currentN, workLoop, workSESE, goViewer);
        }


        private void Draw_Loop_SESE(int currentN, int workLoop, int workSESE, GoView goViewer)
        {
            int nNode = graph.Network[currentN].nNode;
            //////////////////////////////
            int nLoop = 0, nSESE = 0;
            if (workLoop >= 0) nLoop = clsLoop.Loop[workLoop].nLoop;
            if (workSESE >= 0) nSESE = clsSESE.SESE[workSESE].nSESE;

            

            int[,] setDraw = new int[nLoop + nSESE, 3]; // 0: Loop 1: SESE / LoopNum, nNode
            int nSet = 0;
            if (workLoop >= 0)
            {
                //Node 색
                for (int i = 0; i < nNode; i++)
                {
                    //E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
                    if (graph.Network[currentN].Node[i].Special == "E")
                    {
                        node[i].Shape.BrushColor = Color.Yellow;//.Khaki;
                        node[i].ToolTipText = "Loop Entry Node";// \r\n";
                    }
                    else if (graph.Network[currentN].Node[i].Special == "B")
                    {
                        node[i].Shape.BrushColor = Color.SandyBrown;//.LightCyan;
                        node[i].ToolTipText = "BackSplit Node";
                    }
                    else if (graph.Network[currentN].Node[i].Special == "X")
                    {
                        node[i].Shape.BrushColor = Color.Brown;//.DarkOrange;//.LightBlue;
                        node[i].ToolTipText = "Loop Exit Node";
                    }
                    else if (graph.Network[currentN].Node[i].Special == "T")
                    {
                        node[i].Shape.BrushColor = Color.IndianRed;//.Cyan;
                        node[i].ToolTipText = "Loop Exit & BackSplit Node";
                    }
                }
                for (int i = 0; i < nLoop; i++)
                {
                    //포함 Node수
                    nSearchNode = 0;
                    searchNode = new int[graph.Network[currentN].nNode];

                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[i].header;
                    nSearchNode++;
                    find_LoopNode(workLoop, i);

                    setDraw[nSet, 0] = 0;
                    setDraw[nSet, 1] = i;
                    setDraw[nSet, 2] = nSearchNode;
                    nSet++;
                }

            }

            if (workSESE >= 0)
            {
                for (int i = 0; i < nSESE; i++)
                {
                    //Node 색
                    node[clsSESE.SESE[workSESE].SESE[i].Entry].Shape.BrushColor = Color.YellowGreen;
                    node[clsSESE.SESE[workSESE].SESE[i].Entry].ToolTipText = "SESE Entry Node";

                    node[clsSESE.SESE[workSESE].SESE[i].Exit].Shape.BrushColor = Color.Pink;
                    node[clsSESE.SESE[workSESE].SESE[i].Exit].ToolTipText = "SESE Exit Node";

                    setDraw[nSet, 0] = 1;
                    setDraw[nSet, 1] = i;
                    setDraw[nSet, 2] = clsSESE.SESE[workSESE].SESE[i].nNode;
                    nSet++;
                }

            }

            //sorting

            for (int i = 0; i < nSet; i++)
            {
                for (int j = i + 1; j < nSet; j++)
                {
                    if (setDraw[i, 2] < setDraw[j, 2])
                    {
                        int[] temp = new int[3];
                        temp[0] = setDraw[i, 0]; temp[1] = setDraw[i, 1]; temp[2] = setDraw[i, 2];
                        setDraw[i, 0] = setDraw[j, 0]; setDraw[i, 1] = setDraw[j, 1]; setDraw[i, 2] = setDraw[j, 2];
                        setDraw[j, 0] = temp[0]; setDraw[j, 1] = temp[1]; setDraw[j, 2] = temp[2];
                    }
                }

            }


            // 그리기
            //int numSE = 0;
            for (int s = 0; s < nSet; s++)
            {
                //Loop
                if (setDraw[s, 0] == 0)
                {
                    GoCollection loopCol = new GoCollection();
                    GoObject common = null;

                    nSearchNode = 0;
                    searchNode = new int[graph.Network[currentN].nNode];

                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[setDraw[s, 1]].header;
                    nSearchNode++;
                    find_LoopNode(workLoop, setDraw[s, 1]);


                    loopCol.Add(node[searchNode[0]]);
                    common = node[searchNode[0]].Parent;

                    for (int k = 1; k < nSearchNode; k++)
                    {
                        loopCol.Add(node[searchNode[k]]);
                        common = GoObject.FindCommonParent(common, node[searchNode[k]]);
                    }

                    GoSubGraph sg = new GoSubGraph();
                    sg.LabelSpot = GoObject.TopLeft;
                    sg.Text = "Loop[" + setDraw[s, 1].ToString() + "]";

                    if (clsLoop.Loop[workLoop].Loop[setDraw[s, 1]].Irreducible) sg.ToolTipText = "Irreducible Loop";
                    else sg.ToolTipText = "Natural Loop";

                    sg.BackgroundColor = Color.YellowGreen;
                    //sg.BorderPenColor = Color.Blue;
                    //sg.CollapsedObject.Width = 20;

                    if (common is GoSubGraph)
                    {
                        ((GoSubGraph)common).Add(sg);
                    }
                    else
                    {  // otherwise just add as a top-level object
                        goViewer.Document.Add(sg);
                    }

                    sg.AddCollection(loopCol, true);
                }
                else if (setDraw[s, 0] == 1)
                {
                    GoCollection loopCol = new GoCollection();
                    GoObject common = null;


                    loopCol.Add(node[clsSESE.SESE[workSESE].SESE[setDraw[s, 1]].Node[0]]);
                    common = node[clsSESE.SESE[workSESE].SESE[setDraw[s, 1]].Node[0]].Parent;

                    for (int k = 1; k < clsSESE.SESE[workSESE].SESE[setDraw[s, 1]].nNode; k++)
                    {
                        loopCol.Add(node[clsSESE.SESE[workSESE].SESE[setDraw[s, 1]].Node[k]]);
                        common = GoObject.FindCommonParent(common, node[clsSESE.SESE[workSESE].SESE[setDraw[s, 1]].Node[k]]);
                    }

                    int curSESE = setDraw[s, 1];
                    int fblock_indx = getFBLOCK_Index(curSESE);
                    if (fblock_indx == -1) continue;

                    int s_ = clsHWLS.FBLOCK.FBlock[fblock_indx].Entry[0];
                    int t_ = clsHWLS.FBLOCK.FBlock[fblock_indx].Exit[0];
                    string s_text = graph.Network[currentN].Node[s_].Name + graph.Network[currentN].Node[s_].Type_I + graph.Network[currentN].Node[s_].Type_II;
                    string t_text = graph.Network[currentN].Node[t_].Name + graph.Network[currentN].Node[t_].Type_I + graph.Network[currentN].Node[t_].Type_II;

                    GoSubGraph sg = new GoSubGraph();
                    sg.LabelSpot = GoObject.TopLeft;
                    if (clsSESE.SESE[workSESE].SESE[curSESE].type == "B")
                    {
                        //sg.Text = "SESE[" + setDraw[s, 1].ToString() + "]";
                        sg.Text = "B(" + s_text + ", " + t_text +")";
                    }
                    if (clsSESE.SESE[workSESE].SESE[curSESE].type == "R")
                    {
                        sg.Text = "R(" + s_text + ", " + t_text + ")";
                    }
                    if (clsSESE.SESE[workSESE].SESE[curSESE].type == "P")
                    {
                        sg.Text = "S(" + s_text + ", " + t_text + ")";
                    }

                    //sg.Text = "SESE[" + numSE.ToString() + "]";
                    //numSE++;

                    sg.BackgroundColor = Color.SkyBlue;
                    //sg.BorderPenColor = Color.Blue;
                    //sg.CollapsedObject.Width = 20;

                    if (common is GoSubGraph)
                    {
                        ((GoSubGraph)common).Add(sg);
                    }
                    else
                    {  // otherwise just add as a top-level object
                        goViewer.Document.Add(sg);
                    }

                    sg.AddCollection(loopCol, true);

                }

            }

        }

        public void find_LoopNode(int workLoop, int kLoop)
        {

            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[kLoop].nNode; i++)
            {
                searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[kLoop].Node[i];
                nSearchNode++;
            }

            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[kLoop].nChild; k++)
            {
                find_LoopNode(workLoop, clsLoop.Loop[workLoop].Loop[kLoop].child[k]);
            }

        } 

        //Draw SESE & Loop nesting Forest
        //create a reduced network ;
        private void Draw_Forest(int currentN, int workLoop, int workSESE, GoView goViewer, bool bRev)
        {
            goViewer.Document.Clear();
            GoDocument doc = goViewer.Document;

            int nNode = graph.Network[currentN].nNode;
            node = new GoBasicNode[nNode + clsHWLS.FBLOCK.nFBlock];
            bool[] markNode = new bool[nNode + clsHWLS.FBLOCK.nFBlock];

            //Plot the node (just care about the header S[], L[]) from "reducedNet"
            GoLink protolink = goViewer.NewGoLink; //plot the link also
            protolink.ToArrow = true;
            protolink.AvoidsNodes = true;
            protolink.Style = GoStrokeStyle.RoundedLine;

            bool[,] makeLink = new bool[nNode + clsHWLS.FBLOCK.nFBlock, nNode + clsHWLS.FBLOCK.nFBlock];
            int nLink = 0;

            //First - plot all node to the "doc" => ready for creating edges
            for (int i = 0; i < nNode; i++)
            {
                node[i] = new GoBasicNode();
                node[i].LabelSpot = GoObject.MiddleCenter;//.TopCenter;//.Middle;

                if (graph.Network[currentN].Node[i].Kind == "AND" || graph.Network[currentN].Node[i].Kind == "OR" || graph.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Shape.BrushColor = Color.Transparent;
                    node[i].AutoResizes = false;
                    node[i].Shape.Size = new SizeF((float)40, (float)40);
                }
                if (graph.Network[currentN].Node[i].Kind == "TASK" || graph.Network[currentN].Node[i].Kind == "EVENT")
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.BrushColor = Color.Transparent;
                    node[i].Shape.Size = new SizeF((float)80, (float)30);
                    node[i].Port.Size = new SizeF((float)80, (float)10); 
                }

                if (isEntry_Exit(i) == "EnSESE")
                    node[i].Shape.BrushColor = Color.YellowGreen;
                if (isEntry_Exit(i) == "ExSESE")
                    node[i].Shape.BrushColor = Color.Pink;

                node[i].Text = graph.Network[currentN].Node[i].Name + graph.Network[currentN].Node[i].Type_I + graph.Network[currentN].Node[i].Type_II;
                node[i].Label.FontSize = 12;
                doc.Add(node[i]);
            }

            //Second - connect all block
            int curDepth = clsHWLS.FBLOCK.maxDepth;
            do
            {
                for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
                {
                    if (clsHWLS.FBLOCK.FBlock[i].depth != curDepth) continue;

                    int ref_Index = clsHWLS.FBLOCK.FBlock[i].Entry[0];
                    node[nNode + i] = new GoBasicNode();
                    node[nNode + i].LabelSpot = GoObject.MiddleRight;//.TopCenter;//.Middle;
                    string nameNode = "";
                    for (int k = 0; k < clsHWLS.FBLOCK.FBlock[i].nEntry; k++)
                    {
                        int nodeRef = clsHWLS.FBLOCK.FBlock[i].Entry[k];
                        nameNode = nameNode + graph.Network[currentN].Node[nodeRef].Name + graph.Network[currentN].Node[nodeRef].Type_I + graph.Network[currentN].Node[nodeRef].Type_II;                        
                        if (k < clsHWLS.FBLOCK.FBlock[i].nEntry - 1)
                            nameNode = nameNode + ", ";
                    }

                    if (clsHWLS.FBLOCK.FBlock[i].SESE == true)
                    {
                        int nodeRef_en = clsHWLS.FBLOCK.FBlock[i].Entry[0];
                        int nodeRef_ex = clsHWLS.FBLOCK.FBlock[i].Exit[0];
                        string en_text = graph.Network[currentN].Node[nodeRef_en].Name + graph.Network[currentN].Node[nodeRef_en].Type_I + graph.Network[currentN].Node[nodeRef_en].Type_II;
                        string ex_text = graph.Network[currentN].Node[nodeRef_ex].Name + graph.Network[currentN].Node[nodeRef_ex].Type_I + graph.Network[currentN].Node[nodeRef_ex].Type_II; ;
                        string TypeSESE = clsHWLS.FBLOCK.FBlock[i].type;
                        if (clsHWLS.FBLOCK.FBlock[i].type == "P")
                            TypeSESE = "S";
                        nameNode = TypeSESE + "(" + en_text + ", " + ex_text + ")";
                    }

                    node[nNode + i].LabelSpot = GoObject.MiddleCenter;
                    node[nNode + i].Text = nameNode;

                    //node[nNode + i].AutoResizes = false;
                    //node[nNode + i].Size = new SizeF((float)50, (float)50);
                    
                    //node[nNode + i].Port.Style = GoPortStyle.TriangleMiddleTop;
                    node[nNode + i].Shape.Size = new SizeF((float)60, (float)60);

                    if (clsHWLS.FBLOCK.FBlock[i].SESE == true)
                    {
                        node[nNode + i].Shape = new GoRectangle();
                        node[nNode + i].Shape.BrushColor = Color.SkyBlue;
                        if (clsHWLS.FBLOCK.FBlock[i].type == "P")
                            node[nNode + i].Shape.BrushColor = Color.White;
                    }
                    else
                    {
                        node[nNode + i].Label.AutoResizes = false;
                        node[nNode + i].Shape = new GoEllipse();
                        node[nNode + i].Shape.BrushColor = Color.Yellow;
                        node[nNode + i].Shape.Size = new SizeF((float)40, (float)40);
                    }


                    //node[nNode + i].Label.AutoRescales = true;
                    //== Fond size for node
                    node[nNode + i].Label.FontSize = 12;
                    //node[nNode + i].Shape.AutoRescales = true;
                    node[nNode + i].Port.Size = new SizeF((float)20, (float)20); //not necessary
                    //node[nNode + i].Shape.BrushColor = Color.LightGreen;

                    node[nNode + i].ToolTipText = "";

                    doc.Add(node[nNode + i]);
                    
                    //ad link

                    int from_block;
                    if (clsHWLS.FBLOCK.FBlock[i].parentBlock != -1)
                    {
                        from_block = clsHWLS.FBLOCK.FBlock[i].parentBlock;
                        makeLink[nNode + from_block, nNode + i] = true;
                        nLink++;
                    }
                }
                curDepth--;
            } while (curDepth > 0);

            //third - connect all nodes in loops
            curDepth = clsHWLS.FBLOCK.maxDepth;
            do
            {
                for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
                {
                    if (clsHWLS.FBLOCK.FBlock[i].depth != curDepth) continue;

                    //if (m_Network.FBLOCK.FBlock[i].SESE == false)
                    {
                        int toNode;
                        for (int k = 0; k < clsHWLS.FBLOCK.FBlock[i].nNode; k++)
                        {
                            toNode = clsHWLS.FBLOCK.FBlock[i].Node[k];
                            if (markNode[toNode] == false)
                            {                              
                                makeLink[nNode + i, toNode] = true;
                                nLink++;
                                markNode[toNode] = true;
                            }
                        }
                    }
                }
                curDepth--;
            } while (curDepth > 0);

            //draw the results from node[] and makeLink[,]
            link = new GoLink[nNode + clsHWLS.FBLOCK.nFBlock];
            nLink = 0;
            for (int i = 0; i < nNode + clsHWLS.FBLOCK.nFBlock; i++)
            {
                for (int j = 0; j < nNode + clsHWLS.FBLOCK.nFBlock; j++)
                {
                    if (!makeLink[i, j]) continue;

                    IGoLink ilink = goViewer.CreateLink(node[i].Port, node[j].Port);

                    link[j] = ilink.GoObject as GoLink;
                }
            }
            //정렬
            GoLayoutLayeredDigraph layout = new GoLayoutLayeredDigraph();
            layout.Document = goViewer.Document;
            layout.SetsPortSpots = false;
            if (bRev) layout.DirectionOption = GoLayoutDirection.Up;
            else layout.DirectionOption = GoLayoutDirection.Down;
            layout.LayerSpacing = 10;
            layout.PerformLayout();
            /*GoLayoutTreeNetwork Tree = new GoLayoutTreeNetwork();
            Tree.Layout.Document = goViewer.Document;
            Tree.Layout.Alignment = GoLayoutTreeAlignment.Start;
            Tree.Layout.PerformLayout();*/

            //Draw_Loop_SESE(currentN, workLoop, workSESE, goViewer);
        }        

        private void tabGoView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabGoView.SelectedIndex == 0)
            {
                goOverviewer.Observed = goViewB;
            }
            else if (tabGoView.SelectedIndex == 1)
            {
                goOverviewer.Observed = goViewO;
            }
            //else if (tabGoView.SelectedIndex == 2)
            //{
                //goOverviewer.Observed = goViewL;
            //}
            else if (tabGoView.SelectedIndex == 2)
            {
                goOverviewer.Observed = goViewD;
            }
            //else if (tabGoView.SelectedIndex == 4)
            //{
                //goOverviewer.Observed = goViewE;
            //}
            else if (tabGoView.SelectedIndex == 3)
            {
                goOverviewer.Observed = goViewRD;
            }
            else if (tabGoView.SelectedIndex == 4)
            {               
                goOverviewer.Observed = goViewForest;
            }
            //else if (tabGoView.SelectedIndex == 7)
            //{
                //goOverviewer.Observed = goViewS;
            //}
            //else if (tabGoView.SelectedIndex == 8)
            //{
                //goOverviewer.Observed = goViewA;
            //}
            else if (tabGoView.SelectedIndex == 5)
            {               
                goOverviewer.Observed = goViewSE;
            }

        }


        // For just test !!!!!!!!!!!!!
        private void mnuTest_Click(object sender, EventArgs e)
        {
            //m_Network.Run_Test();

            //Draw_Network(m_Network.midNet, m_Network.orgLoop, -1, goViewL);
            //////
            //Draw_Network(m_Network.dummyNet, -1, -1, goViewS);
            ////Draw_Network(m_Network.seseNet, -1, -1, goViewA);
            //Draw_Network(m_Network.nickNet, -1, -1, goViewA);
            ////////////////
            //Draw_Network(m_Network.conNet, -1, -1, goViewS);
            //Draw_Network(m_Network.subNet, -1, -1, goViewA);
        }

        private int getFBLOCK_Index(int curSESE)
        {
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
            {
                if (clsHWLS.FBLOCK.FBlock[i].refIndex == curSESE && clsHWLS.FBLOCK.FBlock[i].SESE == true)
                    return i;
            }
            return -1;
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Display_Inform(graph.finalNet, false);
            backgroundWorker1.ReportProgress(10);

            Display_Network(graph.finalNet, clsLoop.orgLoop);
            //Display_Error(m_Network.finalNet);                        
            backgroundWorker1.ReportProgress(30);
            //추가 Draw SESE Tab
            Draw_Network(graph.finalNet, -1, clsSESE.finalSESE, goViewSE); // SESE
            this.Invoke((MethodInvoker)delegate { Display_Error(graph.finalNet); }); //Display error for SESE tab                       
            backgroundWorker1.ReportProgress(50);

            Draw_Network(graph.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewE); // Entire (+ LOOPS)
            this.Invoke((MethodInvoker)delegate { Display_Error(graph.finalNet); }); //Display error for SESE & Loop Tab

            Draw_DomTree(graph.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewD, false); // DOM
            backgroundWorker1.ReportProgress(70);
            Draw_DomTree(graph.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewRD, true); // RDOM
            backgroundWorker1.ReportProgress(80);
            Draw_Forest(graph.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewForest, false); //Nesting Forest of Loop and SESE       
            backgroundWorker1.ReportProgress(100);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

            if (progressBar1.Value == 100)
                MessageBox.Show("Finish The Visualization", "Finish");
        }

        private string isEntry_Exit(int node)
        {
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
            {
                if (clsHWLS.FBLOCK.FBlock[i].Entry[0] == node)
                {
                    return "EnSESE";
                }
                if (clsHWLS.FBLOCK.FBlock[i].Exit[0] == node)
                {
                    return "ExSESE";
                }
            }
            return "";
        }


    }
}
