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

namespace gProAnalyzer
{
    partial class frmAnalysisNetwork : Form
    {
        //start using class "clsAnalysisNetwork"
        private gProAnalyzer.GraphVariables.clsGraph m_Network; // m_Network represent the class "clsAnalysisNetwork"
        private gProAnalyzer.Preprocessing.clsLoadGraph loadGraph;
        private gProAnalyzer.GraphVariables.clsLoop clsLoop;
        private gProAnalyzer.GraphVariables.clsSESE clsSESE;
        private gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
        private gProAnalyzer.GraphVariables.clsError clsError;

        private gProAnalyzer.Functionalities.LoopIdentification loopNode;

        private gProAnalyzer.Testing test;

        private string[] errorMessage;

        //Using Go Library from Northwoods
        private GoBasicNode[] node;
        private GoLink[] link;
        //Main entrance code

        //Error List and InitializeComponent
        public frmAnalysisNetwork()
        {
            errorMessage = new string[40];
            errorMessage[0] = "Rule 1 : The entry node of a Natural loop should be XOR gateway";
            errorMessage[1] = "Rule 1 : The exit node of a Natural loop should be XOR gateway";
            errorMessage[2] = "Rule 1 : The backward split node of a Natural loop should be XOR gateway";
            errorMessage[3] = "Rule 2.1 : The XOR join node in forward flow of a loop should have just one instant join link"; //bFor = true; instanceFlow error finding
            errorMessage[4] = "Rule 2.1 : The XOR join node in backward flow of a loop should have just one instant join link"; //bFor = false;====
            errorMessage[5] = "Rule 2.1 : The AND join node in forward flow of a loop should have same instant join link"; //bFor = true;
            errorMessage[6] = "Rule 2.1 : The AND join node in backward flow of a loop should have same instant join link"; //bFor = false;=====
            errorMessage[7] = "Rule 2.2 : The exit (or back-split) node in forward flow of a loop should not be contained in a parallel structure";
            errorMessage[8] = "Rule 2.2 : The exit node in backward flow of a loop should not be contained in a parallel structure";
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
            errorMessage[24] = "Rule 6.1 : DFlow and PdFlow should be conflict-free";
            errorMessage[25] = "Rule 6.2 : iDFlow and PdFlow should be conflict-free";
            errorMessage[26] = "Rule 6.3 : The subgraph (HFlow) which contains exit nodes should be conflict-free";
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

            InitializeComponent();
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
            loadGraph.Load_Data(ref m_Network, m_Network.orgNet, sFilePath, true);

            //Pre-Split Node.
            //m_Network.Pre_Split(m_Network.orgNet);

            //Display information to tabInform
            Display_Inform(m_Network.orgNet, true);
            Display_Network(m_Network.orgNet, -1);

            Draw_Network(m_Network.baseNet, -1, -1, goViewB);
            Draw_Network(m_Network.orgNet, -1, -1, goViewO); //switch orgNet and midNet
            tabGoView.SelectedIndex = 1;

            mnuAnalysisNetwork.Visible = true;
            //mnuAnalysisStep.Visible = true;

            this.Text = "AnalysisNetwork  --  " + loadFileDialog.SafeFileName;
            //======test=====
           // Draw_Network(m_Network.orgNet, -1, m_Network.finalSESE, goViewSE); // SESE
            //Display_Error(m_Network.orgNet);
            //test==========
        }

        //Clear all form view
        private void Initialize_All()
        {
            if (m_Network != null) m_Network = null;
            
            m_Network = new gProAnalyzer.GraphVariables.clsGraph();
            clsLoop = new GraphVariables.clsLoop();
            clsSESE = new GraphVariables.clsSESE();
            loopNode = new gProAnalyzer.Functionalities.LoopIdentification();
            loadGraph = new gProAnalyzer.Preprocessing.clsLoadGraph();
            clsHWLS = new gProAnalyzer.GraphVariables.clsHWLS();


            test = new gProAnalyzer.Testing();

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
            goViewL.Document.Clear();
            //Tab SESE network
            goViewSE.Document.Clear();
            //Tab Entire Network
            goViewE.Document.Clear();

            //Tab Loop reduction network
            goViewS.Document.Clear();
            //Tab Final Acyclic network
            goViewA.Document.Clear();
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

            lst_Error1.Items.Clear();
            lst_Error2.Items.Clear();
            lst_Error3.Items.Clear();
        }

        //Run Analysis
        private void mnuAnalysisNetwork_Click(object sender, EventArgs e)
        {
            //RUN ANALYSIS => Return the error
            int retVal = test.RunTest(ref m_Network, ref clsHWLS, ref clsLoop, ref clsSESE, 1); //retVal ~ ErrorNum
            

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
            Draw_Network(m_Network.finalNet, clsLoop.orgLoop, -1, goViewL); // LOOP
            tabGoView.SelectedIndex = 2;

            Display_Inform(m_Network.finalNet, false);
            Display_Network(m_Network.finalNet, clsLoop.orgLoop);
            //Display_Error(m_Network.finalNet);

            if (retVal == 2)
            {
                MessageBox.Show("Concurrency Error : This network can not be handled");
                //return;
            }
            else if (retVal == 5)
            {
                MessageBox.Show("Syntex & Concurrency Error : This network can not be handled");
                return;
            }

            //Draw "Loop reduction network" Tab diagram
            Draw_Network(m_Network.seseNet, -1, clsSESE.orgSESE, goViewS); //SESE
            //Display_SESE_Error(m_Network.seseNet);

            //Draw Final Acyclic diagram
            Draw_Network(m_Network.acyclicNet, -1, -1, goViewA); // Final Acyclic

            if (retVal == 3)
            {
                MessageBox.Show("SyntexError : This network should be modified");
            }

            //추가 Draw SESE Tab
            Draw_Network(m_Network.finalNet, -1, clsSESE.finalSESE, goViewSE); // SESE
            //Display_Error(m_Network.finalNet);
            #region
            //Compare the differences/ the differences from this parameter will decide what kind of diagram will be drawn
            //Draw_Network(m_Network.finalNet, m_Network.orgLoop,       -1           , goViewL);
            //Draw_Network(m_Network.finalNet,      -1,           m_Network.finalSESE, goViewSE);
            //Draw_Network(m_Network.finalNet, m_Network.orgLoop, m_Network.finalSESE, goViewE);
            #endregion

            Draw_Network(m_Network.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewE); // Entire
            //Display_Error(m_Network.finalNet);

            Draw_DomTree(m_Network.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewD, false); // DOM
            Draw_DomTree(m_Network.finalNet, clsLoop.orgLoop, clsSESE.finalSESE, goViewRD, true); // RDOM

            //Draw_Forest(m_Network.finalNet, m_Network.orgLoop, m_Network.finalSESE, goViewForest, false); //Nesting Forest of Loop and SESE            
        }


        public void Display_Inform(int currentN, bool bOrg)
        {
            int nGW = 0;
            for (int i = 0; i < m_Network.Network[currentN].nNode; i++)
            {
                if (m_Network.Network[currentN].Node[i].Kind == "XOR" || m_Network.Network[currentN].Node[i].Kind == "AND") nGW++;
            }
            double imEF = Convert.ToDouble(m_Network.Network[currentN].nLink) / Convert.ToDouble(m_Network.Network[currentN].nNode);
            double imRG = Convert.ToDouble(nGW) / Convert.ToDouble(m_Network.Network[currentN].nNode);

            if (bOrg)
            {
                lblorgN.Text = m_Network.Network[currentN].nNode.ToString("#,#");
                lblorgE.Text = m_Network.Network[currentN].nLink.ToString("#,#");
                lblorgEF.Text = imEF.ToString("#0.00");
                lblorgRG.Text = imRG.ToString("#0.00");
            }
            else
            {
                lblsptN.Text = m_Network.Network[currentN].nNode.ToString("#,#");
                lblsptE.Text = m_Network.Network[currentN].nLink.ToString("#,#");
                lblsptEF.Text = imEF.ToString("#0.00");
                lblsptRG.Text = imRG.ToString("#0.00");
            }

        }

        public void Display_Network(int currentN, int workLoop)
        {

            string imStr = "";

            if (currentN >= 0)
            {
                lst_Node.Items.Clear();
                for (int i = 0; i < m_Network.Network[currentN].nNode; i++)
                {
                    lst_Node.Items.Add(i.ToString());

                    string tempName = m_Network.Network[currentN].Node[i].Name + m_Network.Network[currentN].Node[i].Type_I + m_Network.Network[currentN].Node[i].Type_II;
                    lst_Node.Items[i].SubItems.Add(tempName);//m_Network.Network[currentN].Node[i].orgNum.ToString());

                    lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].Kind);
                    lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].Special);
                    lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].depth.ToString());

                    //lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].Type_I);
                    //lst_Node.Items[i].SubItems.Add(m_Network.Network[currentN].Node[i].Type_II);

                }

                lst_Link.Items.Clear();
                for (int i = 0; i < m_Network.Network[currentN].nLink; i++)
                {

                    lst_Link.Items.Add(i.ToString());

                    int tempNode = m_Network.Network[currentN].Link[i].fromNode;
                    string tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;
                    lst_Link.Items[i].SubItems.Add(tempName);

                    tempNode = m_Network.Network[currentN].Link[i].toNode;
                    tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;
                    lst_Link.Items[i].SubItems.Add(tempName);

                    imStr = "";
                    if (m_Network.Network[currentN].Link[i].bBackJ) imStr += "BJ";
                    if (m_Network.Network[currentN].Link[i].bBackS) imStr += "BS";
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
                    string tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;
                    lst_Loop.Items[i].SubItems.Add(tempName);//m_Network.Loop[workLoop].Loop[i].header.ToString());

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++)
                    {
                        tempNode = clsLoop.Loop[workLoop].Loop[i].Entry[k];
                        tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;

                        imStr += tempName;// m_Network.Loop[workLoop].Loop[i].Entry[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nEntry - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nExit; k++)
                    {
                        tempNode = clsLoop.Loop[workLoop].Loop[i].Exit[k];
                        tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;

                        imStr += tempName; // m_Network.Loop[workLoop].Loop[i].Exit[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nExit - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                    imStr = "";
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nNode; k++)
                    {
                        tempNode = clsLoop.Loop[workLoop].Loop[i].Node[k];
                        tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;

                        imStr += tempName;// m_Network.Loop[workLoop].Loop[i].Node[k].ToString();
                        if (k < clsLoop.Loop[workLoop].Loop[i].nNode - 1) imStr += " ";
                    }
                    lst_Loop.Items[i].SubItems.Add(imStr);

                }
            }
        }

        #region Not use now        
        private void Display_Error(int currentN)
        {

            // Rule 1, 3, 5, (6)
            lst_Error1.Items.Clear();
            int id = 0;
            for (int i = 0; i < clsError.nError; i++)
            {
                // 0, 1, 2, 9,// 16 17 18 19 20 21
                if ((clsError.Error[i].messageNum >= 0 && clsError.Error[i].messageNum <= 2) || (clsError.Error[i].messageNum >= 16 &&
                    clsError.Error[i].messageNum <= 21) || clsError.Error[i].messageNum == 9)
                {

                    lst_Error1.Items.Add((id + 1).ToString());
                    lst_Error1.Items[id].Tag = i.ToString();
                    lst_Error1.Items[id].SubItems.Add(clsError.Error[i].Node);

                    int tempNode = Convert.ToInt32(clsError.Error[i].Node);
                    string tempName;
                    if (tempNode < 0) tempName = tempNode.ToString();
                    else tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;
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

            // Rule 2, 6
            lst_Error2.Items.Clear();
            id = 0;
            for (int i = 0; i < clsError.nError; i++)
            {
                // 3, 4, 5, 6, (7), (8) // 29 30 -parallel structure // 22 23 24 25 26
                //if ((m_Network.Error[i].messageNum >= 0 && m_Network.Error[i].messageNum <= 2) || (m_Network.Error[i].messageNum >= 16 && m_Network.Error[i].messageNum <= 21)) continue;
                //if (m_Network.Error[i].messageNum >= 27 && m_Network.Error[i].messageNum <= 28) continue;
                if ((clsError.Error[i].messageNum >= 3 && clsError.Error[i].messageNum <= 6) || (clsError.Error[i].messageNum >= 22 &&
                    clsError.Error[i].messageNum <= 26) || (clsError.Error[i].messageNum >= 29 && clsError.Error[i].messageNum <= 30) || (clsError.Error[i].messageNum >= 32 && clsError.Error[i].messageNum <= 35))
                {
                    lst_Error2.Items.Add((id + 1).ToString());
                    lst_Error2.Items[id].Tag = i.ToString();
                    lst_Error2.Items[id].SubItems.Add(clsError.Error[i].Node);

                    int tempNode = Convert.ToInt32(clsError.Error[i].Node);
                    string tempName;
                    if (tempNode < 0) tempName = tempNode.ToString();
                    else tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;
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

            // Rule 4 8
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
                    else tempName = m_Network.Network[currentN].Node[tempNode].Name + m_Network.Network[currentN].Node[tempNode].Type_I + m_Network.Network[currentN].Node[tempNode].Type_II;
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
                for (int k = 0; k < m_Network.Network[currentN].nNode; k++)
                {
                    if (Convert.ToInt32(clsError.Error[i].Node) == m_Network.Network[currentN].Node[k].parentNum)
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
        #endregion

        public void Draw_Network(int currentN, int workLoop, int workSESE, GoView goViewer)
        {

            goViewer.Document.Clear();

            GoDocument doc = goViewer.Document;

            int nNode = m_Network.Network[currentN].nNode;
            int nLink = m_Network.Network[currentN].nLink;


            node = new GoBasicNode[nNode];

            //Node 그리기 draw Node
            for (int i = 0; i < nNode; i++)
            {
                //if (m_Network.Network[currentN].Node[i].nPre <= 0 && m_Network.Network[currentN].Node[i].nPost <= 0) continue;

                node[i] = new GoBasicNode();

                node[i].LabelSpot = GoObject.TopCenter;//.Middle;
                node[i].Text = m_Network.Network[currentN].Node[i].Name + m_Network.Network[currentN].Node[i].Type_I + m_Network.Network[currentN].Node[i].Type_II;

                //모양
                if (m_Network.Network[currentN].Node[i].Kind == "OR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Ellipse;
                    node[i].Shape.BrushColor = Color.LightGray;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Times;
                    node[i].Shape.BrushColor = Color.LightGray;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "AND")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Plus;
                    node[i].Shape.BrushColor = Color.LightGray;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "TASK")
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.BrushColor = Color.LightGreen;
                    node[i].Shape.Size = new SizeF((float)40, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "EVENT")
                {
                    node[i].Shape = new GoHexagon();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.BrushColor = Color.HotPink;

                }
                else if (m_Network.Network[currentN].Node[i].Kind == "START")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleRight;

                    //node[i].LabelSpot = GoObject.MiddleCenter;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "END")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleLeft;
                }

                if (m_Network.Network[currentN].Node[i].Kind != "TASK")
                {
                    //크기
                    node[i].Shape.Size = new SizeF((float)30, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);
                }

                //색깔
                if (m_Network.Network[currentN].Node[i].Kind == "START") node[i].Shape.BrushColor = Color.YellowGreen;
                else if (m_Network.Network[currentN].Node[i].Kind == "END") node[i].Shape.BrushColor = Color.LightPink;

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
                if (m_Network.Network[currentN].Link[j].fromNode == m_Network.Network[currentN].Link[j].toNode) continue;

                IGoLink ilink = goViewer.CreateLink(node[m_Network.Network[currentN].Link[j].fromNode].Port, node[m_Network.Network[currentN].Link[j].toNode].Port);

                link[j] = ilink.GoObject as GoLink;

                if (m_Network.Network[currentN].Link[j].bBackJ && m_Network.Network[currentN].Link[j].bBackS)
                {
                    link[j].PenColor = Color.Purple;
                    link[j].ToolTipText = "BackSplit & BackJoin";
                }
                else if (m_Network.Network[currentN].Link[j].bBackJ)
                {
                    link[j].PenColor = Color.DarkGoldenrod;
                    link[j].ToolTipText = "BackJoin";
                }
                else if (m_Network.Network[currentN].Link[j].bBackS)
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



            //Loop 그리기
            //    if (workLoop >= 0)
            //    {
            //        /////////////////////////
            //        for (int i = 0; i < nNode; i++)
            //        {
            //            //E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
            //            if (m_Network.Network[currentN].Node[i].Special == "E")
            //            {
            //                node[i].Shape.BrushColor = Color.Yellow;//.Khaki;
            //                node[i].ToolTipText = "Loop Entry Node";// \r\n";
            //            }
            //            else if (m_Network.Network[currentN].Node[i].Special == "B")
            //            {
            //                node[i].Shape.BrushColor = Color.SandyBrown;//.LightCyan;
            //                node[i].ToolTipText = "BackSplit Node";
            //            }
            //            else if (m_Network.Network[currentN].Node[i].Special == "X")
            //            {
            //                node[i].Shape.BrushColor = Color.Brown;//.DarkOrange;//.LightBlue;
            //                node[i].ToolTipText = "Loop Exit Node";
            //            }
            //            else if (m_Network.Network[currentN].Node[i].Special == "T")
            //            {
            //                node[i].Shape.BrushColor = Color.IndianRed;//.Cyan;
            //                node[i].ToolTipText = "Loop Exit & BackSplit Node";
            //            }

            //        }

            //        /////////////////////////

            //        int curDepth = 1;
            //        int nLoop = m_Network.Loop[workLoop].nLoop;

            //        do
            //        {
            //            for (int i = 0; i < nLoop; i++)
            //            {
            //                if (m_Network.Loop[workLoop].Loop[i].depth != curDepth) continue;

            //                GoCollection loopCol = new GoCollection();
            //                GoObject common = null;

            //                m_Network.nSearchNode = 0;
            //                m_Network.searchNode = new int[m_Network.Network[currentN].nNode];

            //                m_Network.searchNode[m_Network.nSearchNode] = m_Network.Loop[workLoop].Loop[i].header;
            //                m_Network.nSearchNode++;
            //                m_Network.find_LoopNode(workLoop, i);


            //                loopCol.Add(node[m_Network.searchNode[0]]);
            //                common = node[m_Network.searchNode[0]].Parent;

            //                for (int k = 1; k < m_Network.nSearchNode; k++)
            //                {
            //                    loopCol.Add(node[m_Network.searchNode[k]]);
            //                    common = GoObject.FindCommonParent(common, node[m_Network.searchNode[k]]);
            //                }

            //                GoSubGraph sg = new GoSubGraph();
            //                sg.LabelSpot = GoObject.TopLeft;
            //                sg.Text = "Loop[" + i.ToString() + "]";

            //                if (m_Network.Loop[workLoop].Loop[i].Irreducible) sg.ToolTipText = "Irreducible Loop";
            //                else sg.ToolTipText = "Natural Loop";

            //                sg.BackgroundColor = Color.YellowGreen;
            //                //sg.BorderPenColor = Color.Blue;
            //                //sg.CollapsedObject.Width = 20;

            //                if (common is GoSubGraph)
            //                {
            //                    ((GoSubGraph)common).Add(sg);
            //                }
            //                else
            //                {  // otherwise just add as a top-level object
            //                    goViewer.Document.Add(sg);
            //                }

            //                sg.AddCollection(loopCol, true);
            //            }

            //            curDepth++;
            //        } while (curDepth <= m_Network.Loop[workLoop].maxDepth);
            //    }

            ////SESE 그리기
            //if (workSESE >= 0)
            //{

            //    int curDepth = 1;
            //    int nSESE = m_Network.SESE[workSESE].nSESE;
            //    int curNum = 0;
            //    do
            //    {
            //        for (int i = 0; i < nSESE; i++)
            //        {
            //            if (m_Network.SESE[workSESE].SESE[i].depth != curDepth) continue;

            //            ///////////////////////
            //            node[m_Network.SESE[workSESE].SESE[i].Entry].Shape.BrushColor = Color.Cyan;
            //            node[m_Network.SESE[workSESE].SESE[i].Entry].ToolTipText = "SESE Entry Node";// \r\n";

            //            node[m_Network.SESE[workSESE].SESE[i].Exit].Shape.BrushColor = Color.SteelBlue;
            //            node[m_Network.SESE[workSESE].SESE[i].Exit].ToolTipText = "SESE Exit Node";// \r\n";
            //            ///////////////////////

            //            GoCollection loopCol = new GoCollection();
            //            GoObject common = null;


            //            loopCol.Add(node[m_Network.SESE[workSESE].SESE[i].Node[0]]);
            //            common = node[m_Network.SESE[workSESE].SESE[i].Node[0]].Parent;

            //            for (int k = 1; k < m_Network.SESE[workSESE].SESE[i].nNode; k++)
            //            {
            //                loopCol.Add(node[m_Network.SESE[workSESE].SESE[i].Node[k]]);
            //                common = GoObject.FindCommonParent(common, node[m_Network.SESE[workSESE].SESE[i].Node[k]]);
            //            }

            //            GoSubGraph sg = new GoSubGraph();
            //            sg.LabelSpot = GoObject.TopLeft;
            //            sg.Text = "SESE[" + curNum.ToString() + "]";
            //            curNum++;

            //            sg.BackgroundColor = Color.SkyBlue;
            //            //sg.BorderPenColor = Color.Blue;
            //            //sg.CollapsedObject.Width = 20;

            //            if (common is GoSubGraph)
            //            {
            //                ((GoSubGraph)common).Add(sg);
            //            }
            //            else
            //            {  // otherwise just add as a top-level object
            //                goViewer.Document.Add(sg);
            //            }

            //            sg.AddCollection(loopCol, true);
            //        }


            //        curDepth++;
            //    } while (curDepth <= m_Network.SESE[workSESE].maxDepth);


            //}




        }

        private void Draw_DomTree(int currentN, int workLoop, int workSESE, GoView goViewer, bool bRev)
        {

            goViewer.Document.Clear();

            GoDocument doc = goViewer.Document;

            int nNode = m_Network.Network[currentN].nNode;

            node = new GoBasicNode[nNode];

            //Node 그리기
            for (int i = 0; i < nNode; i++)
            {
                //if (m_Network.Network[currentN].Node[i].nPre <= 0 && m_Network.Network[currentN].Node[i].nPost <= 0) continue;

                node[i] = new GoBasicNode();

                node[i].LabelSpot = GoObject.MiddleRight;//.TopCenter;//.Middle;
                node[i].Text = m_Network.Network[currentN].Node[i].Name + m_Network.Network[currentN].Node[i].Type_I + m_Network.Network[currentN].Node[i].Type_II;

                //모양
                if (m_Network.Network[currentN].Node[i].Kind == "OR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Ellipse;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Times;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "AND")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Plus;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "TASK")
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.None;
                    node[i].Shape.Size = new SizeF((float)40, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);

                }
                else if (m_Network.Network[currentN].Node[i].Kind == "EVENT")
                {
                    node[i].Shape = new GoHexagon();
                    node[i].Port.Style = GoPortStyle.None;

                }
                else if (m_Network.Network[currentN].Node[i].Kind == "START")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleRight;

                    //node[i].LabelSpot = GoObject.MiddleCenter;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "END")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleLeft;
                }

                //final draw===============
                if (m_Network.Network[currentN].Node[i].Kind != "TASK")
                {
                    //크기
                    node[i].Shape.Size = new SizeF((float)30, (float)30);
                    node[i].Port.Size = new SizeF((float)10, (float)10);
                }

                //색깔
                if (m_Network.Network[currentN].Node[i].Kind == "START") node[i].Shape.BrushColor = Color.YellowGreen;
                else if (m_Network.Network[currentN].Node[i].Kind == "END") node[i].Shape.BrushColor = Color.LightPink;

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
                    for (int k = 1; k < m_Network.Network[currentN].Node[i].nDom; k++)
                    {
                        makeLink[m_Network.Network[currentN].Node[i].Dom[k - 1], m_Network.Network[currentN].Node[i].Dom[k]] = true;
                        nLink++;
                    }
                }
            }
            else
            {

                for (int i = 0; i < nNode; i++)
                {
                    for (int k = 1; k < m_Network.Network[currentN].Node[i].nDomRev; k++)
                    {
                        makeLink[m_Network.Network[currentN].Node[i].DomRev[k - 1], m_Network.Network[currentN].Node[i].DomRev[k]] = true;
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
            int nNode = m_Network.Network[currentN].nNode;
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
                    if (m_Network.Network[currentN].Node[i].Special == "E")
                    {
                        node[i].Shape.BrushColor = Color.Yellow;//.Khaki;
                        node[i].ToolTipText = "Loop Entry Node";// \r\n";
                    }
                    else if (m_Network.Network[currentN].Node[i].Special == "B")
                    {
                        node[i].Shape.BrushColor = Color.SandyBrown;//.LightCyan;
                        node[i].ToolTipText = "BackSplit Node";
                    }
                    else if (m_Network.Network[currentN].Node[i].Special == "X")
                    {
                        node[i].Shape.BrushColor = Color.Brown;//.DarkOrange;//.LightBlue;
                        node[i].ToolTipText = "Loop Exit Node";
                    }
                    else if (m_Network.Network[currentN].Node[i].Special == "T")
                    {
                        node[i].Shape.BrushColor = Color.IndianRed;//.Cyan;
                        node[i].ToolTipText = "Loop Exit & BackSplit Node";
                    }
                }
                for (int i = 0; i < nLoop; i++)
                {
                    //포함 Node수
                    gProAnalyzer.Functionalities.LoopIdentification.nSearchNode = 0;
                    gProAnalyzer.Functionalities.LoopIdentification.searchNode = new int[m_Network.Network[currentN].nNode];

                    gProAnalyzer.Functionalities.LoopIdentification.searchNode[gProAnalyzer.Functionalities.LoopIdentification.nSearchNode] = clsLoop.Loop[workLoop].Loop[i].header;
                    gProAnalyzer.Functionalities.LoopIdentification.nSearchNode++;
                    gProAnalyzer.Functionalities.LoopIdentification.find_LoopNode(ref clsLoop, workLoop, i);

                    setDraw[nSet, 0] = 0;
                    setDraw[nSet, 1] = i;
                    setDraw[nSet, 2] = gProAnalyzer.Functionalities.LoopIdentification.nSearchNode;
                    nSet++;
                }

            }

            if (workSESE >= 0)
            {
                for (int i = 0; i < nSESE; i++)
                {
                    //Node 색
                    node[clsSESE.SESE[workSESE].SESE[i].Entry].Shape.BrushColor = Color.Cyan;
                    node[clsSESE.SESE[workSESE].SESE[i].Entry].ToolTipText = "SESE Entry Node";

                    node[clsSESE.SESE[workSESE].SESE[i].Exit].Shape.BrushColor = Color.SteelBlue;
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

                    gProAnalyzer.Functionalities.LoopIdentification.nSearchNode = 0;
                    gProAnalyzer.Functionalities.LoopIdentification.searchNode = new int[m_Network.Network[currentN].nNode];

                    gProAnalyzer.Functionalities.LoopIdentification.searchNode[gProAnalyzer.Functionalities.LoopIdentification.nSearchNode] = clsLoop.Loop[workLoop].Loop[setDraw[s, 1]].header;
                    gProAnalyzer.Functionalities.LoopIdentification.nSearchNode++;
                    gProAnalyzer.Functionalities.LoopIdentification.find_LoopNode(ref clsLoop, workLoop, setDraw[s, 1]);


                    loopCol.Add(node[gProAnalyzer.Functionalities.LoopIdentification.searchNode[0]]);
                    common = node[gProAnalyzer.Functionalities.LoopIdentification.searchNode[0]].Parent;

                    for (int k = 1; k < gProAnalyzer.Functionalities.LoopIdentification.nSearchNode; k++)
                    {
                        loopCol.Add(node[gProAnalyzer.Functionalities.LoopIdentification.searchNode[k]]);
                        common = GoObject.FindCommonParent(common, node[gProAnalyzer.Functionalities.LoopIdentification.searchNode[k]]);
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

                    GoSubGraph sg = new GoSubGraph();
                    sg.LabelSpot = GoObject.TopLeft;
                    sg.Text = "SESE[" + setDraw[s, 1].ToString() + "]";
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

        #region Not need now                
        //Draw SESE & Loop nesting Forest
        //create a reduced network ;
         /*
        private void Draw_Forest(int currentN, int workLoop, int workSESE, GoView goViewer, bool bRev)
        {
            goViewer.Document.Clear();
            GoDocument doc = goViewer.Document;

            int nNode = m_Network.Network[currentN].nNode;
            node = new GoBasicNode[nNode + m_Network.FBLOCK.nFBlock];
            bool[] markNode = new bool[nNode + m_Network.FBLOCK.nFBlock];

            //Plot the node (just care about the header S[], L[]) from "reducedNet"
            GoLink protolink = goViewer.NewGoLink; //plot the link also
            protolink.ToArrow = true;
            protolink.AvoidsNodes = true;
            protolink.Style = GoStrokeStyle.RoundedLine;

            bool[,] makeLink = new bool[nNode + m_Network.FBLOCK.nFBlock, nNode + m_Network.FBLOCK.nFBlock];
            int nLink = 0;

            //First - plot all node to the "doc" => ready for creating edges
            for (int i = 0; i < nNode; i++)
            {
                node[i] = new GoBasicNode();
                node[i].LabelSpot = GoObject.MiddleCenter;//.TopCenter;//.Middle;

                if (m_Network.Network[currentN].Node[i].Kind == "AND" && m_Network.Network[currentN].Node[i].Kind == "OR" || m_Network.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Shape.BrushColor = Color.LightGray;
                    node[i].AutoResizes = false;
                    node[i].Shape.Size = new SizeF((float)40, (float)40);
                }

                node[i].Text = m_Network.Network[currentN].Node[i].Name + m_Network.Network[currentN].Node[i].Type_I + m_Network.Network[currentN].Node[i].Type_II;
                node[i].Label.FontSize = 12;
                doc.Add(node[i]);
            }

            //Second - connect all block
            int curDepth = m_Network.FBLOCK.maxDepth;
            do
            {
                for (int i = 0; i < m_Network.FBLOCK.nFBlock; i++)
                {
                    if (m_Network.FBLOCK.FBlock[i].depth != curDepth) continue;

                    int ref_Index = m_Network.FBLOCK.FBlock[i].Entry[0];
                    node[nNode + i] = new GoBasicNode();
                    node[nNode + i].LabelSpot = GoObject.MiddleRight;//.TopCenter;//.Middle;
                    string nameNode = "";
                    for (int k = 0; k < m_Network.FBLOCK.FBlock[i].nEntry; k++)
                    {
                        int nodeRef = m_Network.FBLOCK.FBlock[i].Entry[k];
                        nameNode = nameNode + m_Network.Network[currentN].Node[nodeRef].Name + m_Network.Network[currentN].Node[nodeRef].Type_I + m_Network.Network[currentN].Node[nodeRef].Type_II;
                        if (k < m_Network.FBLOCK.FBlock[i].nEntry - 1)
                            nameNode = nameNode + ", ";
                    }
                    node[nNode + i].LabelSpot = GoObject.MiddleCenter;
                    node[nNode + i].Text = nameNode;

                    //node[nNode + i].AutoResizes = false;
                    //node[nNode + i].Size = new SizeF((float)50, (float)50);
                    
                    //node[nNode + i].Port.Style = GoPortStyle.TriangleMiddleTop;
                    node[nNode + i].Shape.Size = new SizeF((float)60, (float)60);

                    if (m_Network.FBLOCK.FBlock[i].SESE == true)
                    {
                        node[nNode + i].Shape = new GoRoundedRectangle();
                        node[nNode + i].Shape.BrushColor = Color.LightGreen;
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
                    if (m_Network.FBLOCK.FBlock[i].parentBlock != -1)
                    {
                        from_block = m_Network.FBLOCK.FBlock[i].parentBlock;
                        makeLink[nNode + from_block, nNode + i] = true;
                        nLink++;
                    }
                }
                curDepth--;
            } while (curDepth > 0);

            //third - connect all nodes in loops
            curDepth = m_Network.FBLOCK.maxDepth;
            do
            {
                for (int i = 0; i < m_Network.FBLOCK.nFBlock; i++)
                {
                    if (m_Network.FBLOCK.FBlock[i].depth != curDepth) continue;

                    //if (m_Network.FBLOCK.FBlock[i].SESE == false)
                    {
                        int toNode;
                        for (int k = 0; k < m_Network.FBLOCK.FBlock[i].nNode; k++)
                        {
                            toNode = m_Network.FBLOCK.FBlock[i].Node[k];
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
            link = new GoLink[nNode + m_Network.FBLOCK.nFBlock];
            nLink = 0;
            for (int i = 0; i < nNode + m_Network.FBLOCK.nFBlock; i++)
            {
                for (int j = 0; j < nNode + m_Network.FBLOCK.nFBlock; j++)
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
            //GoLayoutTreeNetwork Tree = new GoLayoutTreeNetwork();
            //Tree.Layout.Document = goViewer.Document;
            //Tree.Layout.Alignment = GoLayoutTreeAlignment.Start;
            //Tree.Layout.PerformLayout();

            //Draw_Loop_SESE(currentN, workLoop, workSESE, goViewer);
        }

        private void Draw_DomTree___(int currentN, int workLoop, int workSESE, GoView goViewer, bool bRev)
        {

            goViewer.Document.Clear();

            GoDocument doc = goViewer.Document;

            int nNode = m_Network.Network[currentN].nNode;

            node = new GoBasicNode[nNode];

            //Node 그리기
            for (int i = 0; i < nNode; i++)
            {
                //if (m_Network.Network[currentN].Node[i].nPre <= 0 && m_Network.Network[currentN].Node[i].nPost <= 0) continue;

                node[i] = new GoBasicNode();

                node[i].LabelSpot = GoObject.MiddleRight;//.TopCenter;//.Middle;
                node[i].Text = m_Network.Network[currentN].Node[i].Name + m_Network.Network[currentN].Node[i].Type_I + m_Network.Network[currentN].Node[i].Type_II;

                //모양
                if (m_Network.Network[currentN].Node[i].Kind == "OR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Ellipse;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "XOR")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Times;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "AND")
                {
                    node[i].Shape = new GoDiamond();
                    node[i].Port.Style = GoPortStyle.Plus;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "TASK")
                {
                    node[i].Shape = new GoRectangle();
                    node[i].Port.Style = GoPortStyle.None;

                }
                else if (m_Network.Network[currentN].Node[i].Kind == "EVENT")
                {
                    node[i].Shape = new GoHexagon();
                    node[i].Port.Style = GoPortStyle.None;

                }
                else if (m_Network.Network[currentN].Node[i].Kind == "START")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleRight;

                    //node[i].LabelSpot = GoObject.MiddleCenter;
                }
                else if (m_Network.Network[currentN].Node[i].Kind == "END")// Start, End
                {
                    node[i].Shape = new GoRoundedRectangle();
                    node[i].Port.Style = GoPortStyle.TriangleMiddleLeft;
                }

                //크기
                node[i].Shape.Size = new SizeF((float)25, (float)25);
                node[i].Port.Size = new SizeF((float)10, (float)10);

                //색깔
                if (m_Network.Network[currentN].Node[i].Kind == "START") node[i].Shape.BrushColor = Color.YellowGreen;
                else if (m_Network.Network[currentN].Node[i].Kind == "END") node[i].Shape.BrushColor = Color.LightPink;

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
                    for (int k = 1; k < m_Network.Network[currentN].Node[i].nDom; k++)
                    {
                        makeLink[m_Network.Network[currentN].Node[i].Dom[k - 1], m_Network.Network[currentN].Node[i].Dom[k]] = true;
                        nLink++;
                    }
                }
            }
            else
            {

                for (int i = 0; i < nNode; i++)
                {
                    for (int k = 1; k < m_Network.Network[currentN].Node[i].nDomRev; k++)
                    {
                        makeLink[m_Network.Network[currentN].Node[i].DomRev[k - 1], m_Network.Network[currentN].Node[i].DomRev[k]] = true;
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
        */
        #endregion

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
            else if (tabGoView.SelectedIndex == 2)
            {
                goOverviewer.Observed = goViewL;
            }
            else if (tabGoView.SelectedIndex == 3)
            {
                goOverviewer.Observed = goViewSE;
            }
            else if (tabGoView.SelectedIndex == 4)
            {
                goOverviewer.Observed = goViewE;
            }
            else if (tabGoView.SelectedIndex == 5)
            {
                goOverviewer.Observed = goViewD;
            }
            else if (tabGoView.SelectedIndex == 6)
            {
                goOverviewer.Observed = goViewRD;
            }
            else if (tabGoView.SelectedIndex == 7)
            {
                goOverviewer.Observed = goViewS;
            }
            else if (tabGoView.SelectedIndex == 8)
            {
                goOverviewer.Observed = goViewA;
            }
            else if (tabGoView.SelectedIndex == 9)
            {
                goOverviewer.Observed = goViewForest;
            }

        }


        // For just test !!!!!!!!!!!!!
        private void mnuTest_Click(object sender, EventArgs e)
        {
            //m_Network.Run_Test();

            Draw_Network(m_Network.midNet, clsLoop.orgLoop, -1, goViewL);
            //////
            //Draw_Network(m_Network.dummyNet, -1, -1, goViewS);
            ////Draw_Network(m_Network.seseNet, -1, -1, goViewA);
            //Draw_Network(m_Network.nickNet, -1, -1, goViewA);
            ////////////////
            Draw_Network(m_Network.conNet, -1, -1, goViewS);
            Draw_Network(m_Network.subNet, -1, -1, goViewA);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Loop Entries: Yellow \nLoop Exits: Brown \nBackward Split: Light Orange \nSESE Entries: Blue \nSESE Exits: Dark Blue", "Node Color Detail");
        }

        //========================= JUST FOR VISUALIZATION ===============================
        public void displayProcessModel(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop_, int workLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE_, int workSESE)
        {           
            Initialize_All(); //m_network initialized

            m_Network = graph;
            clsLoop = clsLoop_;
            clsSESE = clsSESE_;

            
            //Display_Inform(m_Network.orgNet, true);
            //Display_Network(m_Network.orgNet, -1);

            Draw_Network(currentN, -1, -1, goViewB);
            //Draw_Network(m_Network.orgNet, -1, -1, goViewO); //switch orgNet and midNet
            tabGoView.SelectedIndex = 1;

            mnuAnalysisNetwork.Visible = true;
            //mnuAnalysisStep.Visible = true;

            if (workLoop != -1) {

                Draw_Network(currentN, workLoop, -1, goViewL); // LOOP
                tabGoView.SelectedIndex = 2;

                Display_Inform(currentN, false);
                Display_Network(currentN, workLoop);
            }
            //Display_Error(m_Network.finalNet);

            //Draw "Loop reduction network" Tab diagram
            //Draw_Network(m_Network.seseNet, -1, workSESE, goViewS); //SESE

            //Draw Final Acyclic diagram
            //Draw_Network(m_Network.acyclicNet, -1, -1, goViewA); // Final Acyclic

            if (workSESE != -1) {

                //추가 Draw SESE Tab
                Draw_Network(currentN, -1, workSESE, goViewSE); // SESE

                Draw_Network(currentN, workLoop, workSESE, goViewE); // Entire
                Display_Error(m_Network.finalNet);

                Draw_DomTree(currentN, workLoop, workSESE, goViewD, false); // DOM
                Draw_DomTree(currentN, workLoop, workSESE, goViewRD, true); // RDOM
            }

        }
    }
}
