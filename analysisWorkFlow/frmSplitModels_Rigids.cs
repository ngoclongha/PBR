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
    public partial class frmSplitModels_Rigids : Form
    {
        private gProAnalyzer.GraphVariables.clsGraph graph;
        private gProAnalyzer.GraphVariables.clsSESE clsSESE;
        private gProAnalyzer.GraphVariables.clsLoop clsLoop;
        private gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
        private gProAnalyzer.GraphVariables.clsError clsError;
        private gProAnalyzer.Preprocessing.clsLoadGraph loadGraph;
        private gProAnalyzer.Run_Analysis_SESE runSESE;
        private gProAnalyzer.Run_Analysis_Verification runVerification;

        private bool SyntaxError_GW;


        //private clsAnaysisNetwork m_Network;
        public  string[] sFilePaths;
        public string[] sFileNames;
        public frmSplitModels_Rigids()
        {
            InitializeComponent();
            graph = new gProAnalyzer.GraphVariables.clsGraph();
            clsSESE = new gProAnalyzer.GraphVariables.clsSESE();
            clsLoop = new gProAnalyzer.GraphVariables.clsLoop();
            clsHWLS = new gProAnalyzer.GraphVariables.clsHWLS();
            clsError = new gProAnalyzer.GraphVariables.clsError();
            loadGraph = new gProAnalyzer.Preprocessing.clsLoadGraph();
            runSESE = new gProAnalyzer.Run_Analysis_SESE();
            runVerification = new gProAnalyzer.Run_Analysis_Verification();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadFileDialog.Title = "Browse";
            loadFileDialog.Filter = "Network Documents (*.net) | *.net";

            loadFileDialog.FileName = "";
            loadFileDialog.ShowDialog();

            sFilePaths = loadFileDialog.FileNames;
            sFileNames = loadFileDialog.SafeFileNames;          

            if (loadFileDialog.FileName == "") return;

            //After_Load();
        }

        //Processing here
        private void mnuMakeNetwork_Click(object sender, EventArgs e)
        {
            int count_Loop = 0;
            int count_total = 0;
            int count_newGraph = 0;
            for (int run = 0; run < sFileNames.Length; run++)
            {
                //m_Network = new clsAnaysisNetwork();

                //m_Network.Load_Data_ForBreakModel(m_Network.orgNet, sFilePaths[run], true);

                loadGraph.Load_Data_ForBreakModel(ref graph, graph.orgNet, sFilePaths[run], true);
                
                string saveFilePath = txtSaveFolder.Text + @"\" + sFileNames[run];                
                //Check Disconnected model
                int CCs = 0;
                int[] mark = null;
                Ultilities.checkGraph.find_ConnectedComponents(graph, graph.baseNet, ref CCs, ref mark); //We calculate on base net only.!!.

                check_SyntaxError(graph, graph.orgNet);

                gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.orgNet, graph.midNet);
                gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.orgLoop, ref clsLoop.IrreducibleError);
                graph.Network[graph.finalNet] = graph.Network[graph.midNet];
                gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph, graph.midNet, graph.finalNet, ref clsLoop, clsLoop.orgLoop);

                gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.finalNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.finalNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.finalNet, -2);
                gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.finalNet);

                gProAnalyzer.Functionalities.SESEIdentification.find_SESE_Dummy(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);

                gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, true);
                gProAnalyzer.Functionalities.PolygonIdentification.polygonIdentification(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE);
                gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph, graph.finalNet, ref clsHWLS, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE);

                gProAnalyzer.Functionalities.VerificationG.Initialize_Verification(ref graph, ref clsError, ref clsLoop, ref clsSESE, ref clsHWLS); //Verification checking

                runVerification.count_Bonds_Rigids(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS); //label B,R,P //#B, R, P

                int curDepth = clsHWLS.FBLOCK.maxDepth;

                graph.Network[graph.reduceNet] = graph.Network[graph.finalNet];
                gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
                gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop, clsLoop.orgLoop, clsLoop.reduceLoop);

                //count_Loop = 0;
                count_total = 0;

                if (SyntaxError_GW) continue;
                //if (gProAnalyzer.Functionalities.VerificationG.iL_concurrency_Error == true) continue;
                //if (CCs > 1) continue;

                do
                {
                    for (int hw = 0; hw < clsHWLS.FBLOCK.nFBlock; hw++)
                    {
                        if (clsHWLS.FBLOCK.FBlock[hw].depth != curDepth) continue;
                        int curr = clsHWLS.FBLOCK.FBlock[hw].refIndex;
                        if (clsHWLS.FBLOCK.FBlock[hw].type == "R" && clsHWLS.FBLOCK.FBlock[hw].SESE == true) //Split model and store it
                        {                             
                            //count++;
                            count_newGraph++;
                            count_total++;
                            saveFilePath = txtSaveFolder.Text + @"\" + sFileNames[run].Replace(".net", "") + @"_" + count_total + @".net";

                            //if (clsHWLS.FBLOCK.FBlock[hw].SESE == false)
                            //    gProAnalyzer.Ultilities.makeSubNetwork.make_AcyclicSubGraph
                            gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, clsSESE.finalSESE, curr, ref clsSESE, "SESE", -1);


                            int[] nodeList = new int[graph.Network[graph.subNet].nNode*2];
                            int nNodeList = 0;
                            //get node list
                            for (int j = 0; j < graph.Network[graph.subNet].nNode; j++)
                            {
                                    nodeList[nNodeList] = j;
                                    nNodeList++;                                
                            }
                            //get edge list
                            clsAnaysisNetwork.strLink[] LinkList = new clsAnaysisNetwork.strLink[graph.Network[graph.subNet].nLink*2];
                            int nLinkList = 0;
                            for (int j = 0; j < graph.Network[graph.subNet].nLink; j++)
                            {
                                for (int k = 0; k < nNodeList; k++)
                                {
                                    if (nodeList[k] == graph.Network[graph.subNet].Link[j].fromNode)
                                    {
                                        for (int m = 0; m < nNodeList; m++)
                                        {
                                            if (nodeList[m] == graph.Network[graph.subNet].Link[j].toNode)
                                            {
                                                LinkList[nLinkList].fromNode = k;
                                                LinkList[nLinkList].toNode = m;
                                                nLinkList++;
                                            }
                                        }
                                    }
                                }
                            }
                            //Save temporary to file
                            Save_Network_1(graph.subNet, nodeList, nNodeList, LinkList, nLinkList, saveFilePath, false, "");
                            string[] tempLines = File.ReadAllLines(saveFilePath);
                            
                        }
                        if (clsHWLS.FBLOCK.FBlock[hw].SESE == false)
                        {
                            count_Loop++;
                            //count_newGraph++;
                            count_total++;

                            if (clsLoop.Loop[clsLoop.orgLoop].Loop[curr].nEntry == 1)
                            {
                                gProAnalyzer.Ultilities.makeSubNetwork.make_CyclicSubGraph(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, clsLoop.reduceLoop, curr, "NL", ref clsSESE);
                                saveFilePath = txtSaveFolder.Text + @"\" + "NL_" + sFileNames[run].Replace(".net", "") + @"_" + count_total + @".net";
                            }
                            if (clsLoop.Loop[clsLoop.orgLoop].Loop[curr].nEntry > 1)
                            {
                                gProAnalyzer.Ultilities.makeSubNetwork.make_CyclicSubGraph(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, clsLoop.reduceLoop, curr, "IL", ref clsSESE);
                                saveFilePath = txtSaveFolder.Text + @"\" + "IL_" + sFileNames[run].Replace(".net", "") + @"_" + count_total + @".net";
                            }

                            int[] nodeList = new int[graph.Network[graph.subNet].nNode * 2];
                            int nNodeList = 0;
                            //get node list
                            for (int j = 0; j < graph.Network[graph.subNet].nNode; j++)
                            {
                                nodeList[nNodeList] = j;
                                nNodeList++;
                            }
                            //get edge list
                            clsAnaysisNetwork.strLink[] LinkList = new clsAnaysisNetwork.strLink[graph.Network[graph.subNet].nLink * 2];
                            int nLinkList = 0;
                            for (int j = 0; j < graph.Network[graph.subNet].nLink; j++)
                            {
                                for (int k = 0; k < nNodeList; k++)
                                {
                                    if (nodeList[k] == graph.Network[graph.subNet].Link[j].fromNode)
                                    {
                                        for (int m = 0; m < nNodeList; m++)
                                        {
                                            if (nodeList[m] == graph.Network[graph.subNet].Link[j].toNode)
                                            {
                                                LinkList[nLinkList].fromNode = k;
                                                LinkList[nLinkList].toNode = m;
                                                nLinkList++;
                                            }
                                        }
                                    }
                                }
                            }
                            //Save temporary to file
                            Save_Network_1(graph.subNet, nodeList, nNodeList, LinkList, nLinkList, saveFilePath, false, "");
                            string[] tempLines = File.ReadAllLines(saveFilePath);
                        }
                        //reduce Bond - Rigids - NL - IL
                        if (clsHWLS.FBLOCK.FBlock[hw].type != "P" && clsHWLS.FBLOCK.FBlock[hw].SESE == true) //reduce nested rigids (if any)
                            gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, graph.reduceNet, clsSESE, clsSESE.finalSESE, curr);
                        if (clsHWLS.FBLOCK.FBlock[hw].SESE == false && clsLoop.Loop[clsLoop.orgLoop].Loop[curr].nEntry == 1)
                            gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, graph.reduceNet, ref clsLoop, clsLoop.reduceLoop, curr, "", true);
                        if (clsHWLS.FBLOCK.FBlock[hw].SESE == false && clsLoop.Loop[clsLoop.orgLoop].Loop[curr].nEntry > 1)
                            gProAnalyzer.Ultilities.reduceGraph.reduce_IrLoop_Preprocessing(ref graph, graph.reduceNet, clsLoop, clsLoop.reduceLoop, curr);
                    }
                    curDepth--;
                } while (curDepth > 0);
            }
            MessageBox.Show(count_Loop.ToString(), "Loop");
            MessageBox.Show(count_newGraph.ToString(), "Rigids");
        }

        private void Save_Network_1(int currentN, int[] nodeList, int nNodeList, clsAnaysisNetwork.strLink[] LinkList, int nLinkList, string sFilePath, bool isOrg, string appendText)
        {
            StreamWriter sw = new StreamWriter(sFilePath, isOrg);

            if (appendText != "")
            {
                sw.WriteLine(appendText);
            }

            //Node Information
            string imLine = nNodeList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nNodeList; i++)
            {
                imLine = graph.Network[currentN].Node[nodeList[i]].Name + " " + graph.Network[currentN].Node[nodeList[i]].Kind;
                sw.WriteLine(imLine);
            }

            //Link Information
            imLine = nLinkList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nLinkList; i++)
            {
                imLine = LinkList[i].fromNode.ToString() + " " + LinkList[i].toNode.ToString();
                sw.WriteLine(imLine);
            }
            sw.Close();
        }

        private void Save_Network_2(int currentN, int[] nodeList, int nNodeList, clsAnaysisNetwork.strLink[] LinkList, int nLinkList, string sFilePath, bool isOrg, string appendText)
        {
            StreamWriter sw = new StreamWriter(sFilePath, isOrg);

            if (appendText != "")
            {
                sw.WriteLine(appendText);
            }

            //Node Information
            string imLine = nNodeList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nNodeList; i++)
            {
                if (nodeList[i] != -1 && nodeList[i] != -2 && nodeList[i] != -3 && nodeList[i] != -4)
                {
                    imLine = nodeList[i] + " " + graph.Network[currentN].Node[nodeList[i]].Kind;
                    sw.WriteLine(imLine);
                }
            }
            //Add START END SS EE
            // SS -> -1; START -> -2; EE -> -3; END -> -4
            for (int i = 0; i < nNodeList; i++)
            {
                if (nodeList[i] == -2)
                {
                    imLine = "S" + " " + "START";
                    sw.WriteLine(imLine);
                }
                if (nodeList[i] == -4)
                {
                    imLine = "E" + " " + "END";
                    sw.WriteLine(imLine);
                }
                if (nodeList[i] == -1)
                {
                    imLine = "SS" + " " + "OR";
                    sw.WriteLine(imLine);
                }
                if (nodeList[i] == -3)
                {
                    imLine = "EE" + " " + "OR";
                    sw.WriteLine(imLine);
                }
            }

            //Link Information
            imLine = nLinkList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nLinkList; i++)
            {
                imLine = LinkList[i].fromNode.ToString() + " " + LinkList[i].toNode.ToString();
                sw.WriteLine(imLine);
            }
            sw.Close();
        }

        private void btnSetFolder_Click(object sender, EventArgs e)
        {
            folderBrowserMake.SelectedPath = txtLoadFolder.Text;

            folderBrowserMake.ShowDialog();

            txtSaveFolder.Text = folderBrowserMake.SelectedPath;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //int count = 0;
            //int count_newGraph = 0;
            //StreamWriter sw1 = new StreamWriter("c:\\DisconnectedCount.txt", true);
            //for (int run = 0; run < sFileNames.Length; run++)
            {
                //m_Network = new clsAnaysisNetwork();

                //m_Network.Load_Data_ForBreakModel(m_Network.orgNet, sFilePaths[run], true);
                
                //string saveFilePath = txtSaveFolder.Text + @"\" + sFileNames[run];                
                //Check Disconnected model
                //int CCs = 0;
                //int[] mark = null;
                //m_Network.find_ConnectedComponents(m_Network.baseNet, ref CCs, ref mark); //We calculate on base net only.!!.

                
                //sw1.WriteLine(sFileNames[run] + ";" + CCs);

                //count++;
                
            }
            //sw1.Close();
           // MessageBox.Show(count.ToString(), "Number of total model");
            
        
        }

        private void check_SyntaxError(GraphVariables.clsGraph graph, int orgNet) //Check ACTIVITIES/EVENTS as GATEWAYS - More critical
        {
            SyntaxError_GW = false;
            for (int i = 0; i < graph.Network[orgNet].nNode; i++)
            {
                if (graph.Network[orgNet].Node[i].nPre > 1 || graph.Network[orgNet].Node[i].nPost > 1)
                {
                    if (graph.Network[orgNet].Node[i].Kind == "XOR" || graph.Network[orgNet].Node[i].Kind == "OR" || graph.Network[orgNet].Node[i].Kind == "AND") continue;

                    SyntaxError_GW = true;
                    break;
                }
            }
        }
    }
}
