using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace gProAnalyzer
{
    public partial class EPML2NET : Form
    {
        private gProAnalyzer.Functionalities.NodeSplittingType1 SplitType1;
        private gProAnalyzer.Functionalities.NodeSplittingType2 SplitType2;
        private gProAnalyzer.Functionalities.NodeSplittingType3 SplitType3;
        private gProAnalyzer.Functionalities.LoopIdentification findLoop;
        private gProAnalyzer.Functionalities.DominanceIdentification fndDomRel;
        private gProAnalyzer.Functionalities.SESEIdentification sese;
        private gProAnalyzer.Ultilities.makeInstanceFlow makInst;
        private gProAnalyzer.Ultilities.makeSubNetwork makSubNet;
        private gProAnalyzer.Ultilities.AnalyseBehavior_InstF anlyzBh_InstF;
        private gProAnalyzer.Ultilities.reduceGraph reduceG;
        private gProAnalyzer.Ultilities.extendGraph extendG;
        private gProAnalyzer.Ultilities.makeNestingForest makNestingForest;
        private gProAnalyzer.Functionalities.UntanglingIL makUntangling;

        private static int maxXmlNode = 200;
        private static int maxXmlLink = 300;

        private string[] sFilePaths;
        private string[] sFileNames;

        public struct strNode
        {
            public string Name; //실 이름

            public string Kind; //Node 종류  S:start E: end T:Task AND:And  XOR:OR

        }

        public struct strLink
        {
            public int fromNode; //시작 Node
            public int toNode; //끝 Node
        }

        public struct strNetwork
        {
            public int nNode;
            public strNode[] Node;

            public int nLink;
            public strLink[] Link;
        }
        private strNetwork Network;

        private int xNode, xLink = 0;
        private string[,] xmlNode, xmlLink;

        public EPML2NET()
        {
            InitializeComponent();
        }

        public void Initialize_All()
        {
            SplitType1 = new gProAnalyzer.Functionalities.NodeSplittingType1();
            SplitType2 = new gProAnalyzer.Functionalities.NodeSplittingType2();
            SplitType3 = new gProAnalyzer.Functionalities.NodeSplittingType3();

            findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
            fndDomRel = new gProAnalyzer.Functionalities.DominanceIdentification();
            sese = new gProAnalyzer.Functionalities.SESEIdentification();
            makSubNet = new gProAnalyzer.Ultilities.makeSubNetwork();

            makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
            anlyzBh_InstF = new gProAnalyzer.Ultilities.AnalyseBehavior_InstF();
            reduceG = new gProAnalyzer.Ultilities.reduceGraph();
            extendG = new gProAnalyzer.Ultilities.extendGraph();
            makNestingForest = new gProAnalyzer.Ultilities.makeNestingForest();
            makUntangling = new gProAnalyzer.Functionalities.UntanglingIL();            
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadFileDialog.Title = "Browse";
            loadFileDialog.Filter = "EPML Documents (*.epml) | *.epml";

            loadFileDialog.FileName = "";
            loadFileDialog.Multiselect = true;
            loadFileDialog.ShowDialog();

            if (loadFileDialog.FileName == "") return;

            sFilePaths = loadFileDialog.FileNames;
            sFileNames = loadFileDialog.SafeFileNames;

            txtLoadFolder.Text = sFilePaths[0].Remove(loadFileDialog.FileName.Length - loadFileDialog.SafeFileName.Length);
            //txtFileN.Text = sFileNames;
        }

        private void btnSetFolder_Click(object sender, EventArgs e)
        {
            folderBrowserMake.SelectedPath = txtLoadFolder.Text;

            folderBrowserMake.ShowDialog();

            txtSaveFolder.Text = folderBrowserMake.SelectedPath;
        }

        //Create networks (*.net) with a XML file containing many networks
        private void mnuMakeNetwork_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < sFilePaths.Length; index++)
            {
                string currFilePath = sFilePaths[index];
                //if (txtFileN.Text == "") return;

                XmlTextReader xr = new XmlTextReader(currFilePath);

                int intMade = 0;
                string sFileName = ""; 

                int imNode = 0;
                int imLink = 0;
                string sFilePath;

                //scan each element of XML file
                while (xr.Read())
                {
                    if (xr.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        //새 파일
                        if (xr.Name == "epc")
                        {
                            if (sFileName != "")
                            {
                                sFilePath = txtSaveFolder.Text + @"\" + sFileName + @".net";

                                //Made Network
                                imNode = xNode;
                                imLink = xLink;
                                Make_aNetwork(false);
                                Save_Network(sFilePath, false, "");
                                //Original Network
                                xNode = imNode;
                                xLink = imLink;
                                Make_aNetwork(true);
                                Save_Network(sFilePath, true, "ADD --- Original Network ---");

                                intMade++;
                            }                            

                            while (xr.MoveToNextAttribute())
                            {
                                if (xr.Name == "name")
                                {
                                    sFileName = xr.Value;
                                }
                            }
                            xNode = 0; xLink = 0;
                            xmlNode = new string[maxXmlNode, 3]; //ID, Kind
                            xmlLink = new string[maxXmlLink, 3]; //ID, From, To
                        }
                        //노드 //nodes
                        else if (xr.Name == "event" || xr.Name == "function" || xr.Name == "xor" || xr.Name == "and" || xr.Name == "or")
                        {
                            if (xr.Name == "event") xmlNode[xNode, 1] = "EVENT";
                            else if (xr.Name == "function") xmlNode[xNode, 1] = "TASK";
                            else if (xr.Name == "or") xmlNode[xNode, 1] = "XOR"; // XOR 수정
                            else if (xr.Name == "xor") xmlNode[xNode, 1] = "XOR";
                            else if (xr.Name == "and") xmlNode[xNode, 1] = "AND";

                            while (xr.MoveToNextAttribute())
                            {
                                if (xr.Name == "id") xmlNode[xNode, 0] = xr.Value;
                                
                            }
                            xr.MoveToElement();

                            if (xr.Name == "event" || xr.Name == "function")
                            {
                                XmlReader inner = xr.ReadSubtree();
                                inner.ReadToDescendant("name");                                
                                xmlNode[xNode, 1] = xmlNode[xNode, 1] + " " + inner.ReadString();
                            }

                            xNode++;
                        }
                        //링크 //link
                        else if (xr.Name == "arc")
                        {
                            while (xr.MoveToNextAttribute())
                            {
                                if (xr.Name == "id") xmlLink[xLink, 0] = xr.Value;
                            }
                        }
                        else if (xr.Name == "flow")
                        {
                            if (xmlLink[xLink, 0] != "")
                            {
                                while (xr.MoveToNextAttribute())
                                {
                                    if (xr.Name == "source") xmlLink[xLink, 1] = xr.Value;
                                    else if (xr.Name == "target") xmlLink[xLink, 2] = xr.Value;
                                }

                                bool isSame = false;
                                for (int i = 0; i < xLink; i++)
                                {
                                    if (xmlLink[xLink, 1] == xmlLink[i, 1] && xmlLink[xLink, 2] == xmlLink[i, 2])
                                    {
                                        isSame = true;
                                        break;
                                    }
                                }
                                if (!isSame) xLink++;

                            }
                        }

                    }
                }

                sFileName = sFileNames[index].Substring(0, sFileNames[index].LastIndexOf("."));
                //Store the last case
                sFilePath = txtSaveFolder.Text + @"\" + sFileName + @".net";

                //Made Network
                //int imNode = xNode;
                //int imLink = xLink;
                Make_aNetwork(false);
                Save_Network(sFilePath, false, "");

                //Original Network
                xNode = imNode;
                xLink = imLink;
                Make_aNetwork(true);
                Save_Network(sFilePath, true, "ADD --- Original Network ---");

                intMade++;
                //===========

                //MessageBox.Show(intMade.ToString() + " Network files were made");
            }
        }

        //the same a part of IBM Load_XML()
        private void Make_aNetwork(bool isOrg)
        {
            int nStart = 0, nEnd = 0;
            string[,] xmlNode2 = new string[maxXmlNode, 3];
            Array.Copy(xmlNode, xmlNode2, xmlNode.Length);
            
            for (int i = 0; i < xNode; i++)
            {
                int cntPre = 0;
                int cntPost = 0;
                for (int j = 0; j < xLink; j++)
                {
                    if (xmlLink[j, 1] == xmlNode[i, 0]) cntPost++;
                    if (xmlLink[j, 2] == xmlNode[i, 0]) cntPre++;
                }

                if (cntPre == 0)
                {
                    xmlNode[i, 2] = "S";
                    nStart++;
                }
                if (cntPost == 0)
                {
                    xmlNode2[i, 2] = "E";
                    nEnd++;
                }
            }


            ////////////////////// convert in to SS and EE model for single entry single exit process model
            if (!isOrg)
            {
                if (nStart > 1)
                {

                    xmlNode[xNode, 0] = "S";
                    xmlNode[xNode, 1] = "START";
                    xNode++;

                    xmlNode[xNode, 0] = "SS"; //dummy strat
                    xmlNode[xNode, 1] = "XOR";
                    //xmlNode[xNode, 1] = "OR";
                    xNode++;

                    xmlLink[xLink, 0] = "SL0";
                    xmlLink[xLink, 1] = "S";
                    xmlLink[xLink, 2] = "SS";
                    xLink++;

                    for (int i = 0; i < xNode - 2; i++)
                    {
                        if (xmlNode[i, 2] == "S")
                        {
                            //xmlNode[i, 1] = "SD";

                            xmlLink[xLink, 0] = "SL1";
                            xmlLink[xLink, 1] = "SS";
                            xmlLink[xLink, 2] = xmlNode[i, 0];
                            xLink++;
                        }
                    }
                }
                else //nStart = 1
                {
                    xmlNode[xNode, 0] = "S";
                    xmlNode[xNode, 1] = "START";
                    xNode++;

                    for (int i = 0; i < xNode - 1; i++)
                    {
                        if (xmlNode[i, 2] == "S")
                        {
                            xmlLink[xLink, 0] = "SL1";
                            xmlLink[xLink, 1] = "S";
                            xmlLink[xLink, 2] = xmlNode[i, 0];
                            xLink++;
                        }
                    }
                }

                if (nEnd > 1)
                {
                    xmlNode[xNode, 0] = "E";
                    xmlNode[xNode, 1] = "END";
                    xmlNode2[xNode, 0] = "E";
                    xmlNode2[xNode, 1] = "END";
                    xNode++;

                    xmlNode[xNode, 0] = "EE"; // dummy end
                    xmlNode[xNode, 1] = "XOR";
                    //xmlNode[xNode, 1] = "OR";
                    xmlNode2[xNode, 0] = "EE"; // dummy end
                    xmlNode[xNode, 1] = "XOR";
                    //xmlNode2[xNode, 1] = "OR";
                    xNode++;

                    xmlLink[xLink, 0] = "EL0";
                    xmlLink[xLink, 1] = "EE";
                    xmlLink[xLink, 2] = "E";
                    xLink++;

                    for (int i = 0; i < xNode - 2; i++)
                    {
                        if (xmlNode2[i, 2] == "E")
                        {
                            //xmlNode[i, 1] = "ED";

                            xmlLink[xLink, 0] = "EL1";
                            xmlLink[xLink, 1] = xmlNode2[i, 0];
                            xmlLink[xLink, 2] = "EE";
                            xLink++;
                        }
                    }
                }
                else
                {
                    xmlNode[xNode, 0] = "E";
                    xmlNode[xNode, 1] = "END";
                    xmlNode2[xNode, 0] = "E";
                    xmlNode2[xNode, 1] = "END";
                    xNode++;

                    for (int i = 0; i < xNode - 1; i++)
                    {
                        if (xmlNode2[i, 2] == "E")
                        {
                            xmlLink[xLink, 0] = "EL1";
                            xmlLink[xLink, 1] = xmlNode2[i, 0];
                            xmlLink[xLink, 2] = "E";
                            xLink++;
                        }
                    }
                }

            } //////////////////////////

            Network = new strNetwork();

            //노드 수 // number of nodes
            Network.nNode = xNode;
            Network.Node = new strNode[xNode];


            //노드 정보 //node info
            for (int i = 0; i < xNode; i++)
            {

                Network.Node[i].Kind = xmlNode[i, 1];

                if (xmlNode[i, 0] == "S" || xmlNode[i, 0] == "E" || xmlNode[i, 0] == "SS" || xmlNode[i, 0] == "EE")
                {
                    Network.Node[i].Name = xmlNode[i, 0];
                }
                else
                {
                    Network.Node[i].Name = xmlNode[i, 0];
                }

            }

            // 링크 수
            Network.nLink = xLink;
            Network.Link = new strLink[xLink];

            for (int i = 0; i < xLink; i++)
            {
                int fromN = -1, toN = -1;

                for (int j = 0; j < xNode; j++)
                {
                    if (xmlLink[i, 1] == xmlNode[j, 0]) fromN = j;
                    if (xmlLink[i, 2] == xmlNode[j, 0]) toN = j;

                }

                if (fromN >= 0 && toN >= 0)
                {

                    Network.Link[i].fromNode = fromN;
                    Network.Link[i].toNode = toN;
                }
            }

            //추가
            //for (int i = 0; i < Network.nNode; i++)
            //{
            //    int cntPre = 0;
            //    int cntPost = 0;
            //    for (int j = 0; j < Network.nLink; j++)
            //    {
            //        if (Network.Link[j].fromNode == i) cntPost++;
            //        if (Network.Link[j].toNode == i) cntPre++;
            //    }

            //    if (Network.Node[i].Kind == "SD")
            //    {
            //        if (cntPost > 1) Network.Node[i].Kind = "OR";
            //        else Network.Node[i].Kind = "EVENT";
            //    }
            //    else if (Network.Node[i].Kind == "ED")
            //    {
            //        if (cntPre > 1) Network.Node[i].Kind = "OR";
            //        else Network.Node[i].Kind = "EVENT";
            //    }
            //}


        }

        private void Save_Network(string sFilePath, bool isOrg, string appendText)
        {
            StreamWriter sw = new StreamWriter(sFilePath, isOrg);

            //ADD something here

            //=================

            if (appendText != "")
            {
                sw.WriteLine(appendText);
            }

            //Node Information
            string imLine = Network.nNode.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < Network.nNode; i++)
            {
                imLine = Network.Node[i].Name + " " + Network.Node[i].Kind;
                sw.WriteLine(imLine);
            }

            //Link Information
            imLine = Network.nLink.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < Network.nLink; i++)
            {
                imLine = Network.Link[i].fromNode.ToString() + " " + Network.Link[i].toNode.ToString();

                sw.WriteLine(imLine);
            }

            sw.Close();
        }

        private void loadFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void pre_processingG(ref GraphVariables.clsGraph graph, ref GraphVariables.clsLoop clsLoop, ref GraphVariables.clsSESE clsSESE)
        {
            Initialize_All();

            gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.orgNet, graph.midNet);            

            graph.Network[graph.finalNet] = graph.Network[graph.midNet];

            gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.finalNet);
            gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.finalNet);
            gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.finalNet, -2);
            gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.finalNet);

            gProAnalyzer.Functionalities.SESEIdentification.find_SESE_WithLoop(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);
            gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, true);
        }

        /*
        private void check_by_Rule2(int currentN, int workLoop, int workSESE, string strLoop)
        {
            string strOrgLoop = strLoop;
            int curDepth = FBLOCK.maxDepth;
            do {
                //for (int i = 0; i < Loop[workLoop].nLoop; i++)
                for (int j = 0; j < FBLOCK.nFBlock; j++) {
                    //if (Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (FBLOCK.FBlock[j].depth != curDepth) continue;

                    //if (strOrgLoop == "") strLoop = j.ToString();
                    //else strLoop = strOrgLoop + "-" + j.ToString();

                    int i = FBLOCK.FBlock[j].refIndex;

                    if (strOrgLoop == "") strLoop = i.ToString();
                    else strLoop = strOrgLoop + "-" + i.ToString();

                    if (FBLOCK.FBlock[j].SESE) //If SESE => verify SESE
                    {
                        if (i == 9) { }
                        if (Bond_Check(currentN, workSESE, i)) //bond model
                        {
                            //verify some easy Bond (SS, EE) Example => 1Ex_dwdy.net (just for SESE which have Entry or Exit are SS or EE
                            if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Name == "SS") {
                                Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Network[currentN].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;

                            }
                            if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Name == "EE") {
                                Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind;
                                Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind;
                            }
                            //check rule for the rest SESE
                            //informList[13]++;
                            if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind == Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind) { }
                            else if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind == "OR") { }
                            else if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind == "XOR") {
                                Error[nError].currentKind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Error[nError].messageNum = 27;
                                Error[nError].Node = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].parentNum.ToString();
                                Error[nError].SESE = i.ToString();
                                add_Error();
                            }
                            else if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind == "AND") {
                                Error[nError].currentKind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Error[nError].messageNum = 28;
                                Error[nError].Node = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].parentNum.ToString();
                                Error[nError].SESE = i.ToString();
                                add_Error();
                            }
                        }
                        else //rigid model
                        {
                            //informList[14]++; //count rigids
                            if (all_same_kind(currentN, workSESE, i)) {
                                if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Name == "SS") {
                                    Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                    Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                    Network[currentN].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                    Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                }
                                if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Name == "EE") {
                                    Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind;
                                    Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind;
                                }
                            } //no errors for rigid
                            else if (all_OR_join(currentN, workSESE, i)) { }
                            else {
                                //Count OR-split in this Rigid - just simple first
                                count_OR_Split_Rigids(finalNet, workSESE, i);
                                // redSeNet에서 해당 SESE네트워크 추출 
                                make_subNetwork(currentN, subNet, workSESE, i, "SESE", -1);
                                //return;
                                //make_InstanceFlow(subNet, -2, "SESE", strLoop);
                            }
                        }
                        reduce_seseNetwork(currentN, workSESE, i);
                    }
                    
                }
                curDepth--;
            } while (curDepth > 0);
        }
         */
    }
}
