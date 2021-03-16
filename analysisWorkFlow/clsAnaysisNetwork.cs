using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

namespace gProAnalyzer
{
    class clsAnaysisNetwork
    {
        //private static int maxXmlNode = 200;
        //private static int maxXmlLink = 300;
        public struct strNode
        {
            public string Kind; //Node 종류  S:start E: end T:Task AND:And  XOR:OR

            public string Name; // Node명

            public int orgNum; //원 노드번호 - Split시 사용 -- SubNetwork경우 Parent Network 번호
            public int parentNum; //Original Network 노드번호 - Error 점검시 사용

            public string Type_I; // "" or j(join)  or s (split) c (Con_AND)
            public string Type_II; // "" or fj(forward join)  or bj(backward join) or fs(forward split) or bs(backward split)

            public string Special; //special Node??   E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS

            public int nPre; //선 Node수 //Number of node which were Predecessor of current Node
            public int[] Pre; //선 Node
            public int nPost; //후 Node수 //Number of node which were Successor of current Node
            public int[] Post; //후 Node

            public int nDom; // dominator 수
            public int[] Dom; //dominator 집합 // d[n] a set of node dom this node
            public int nDomRev; // 역dominator 수 // post dominator
            public int[] DomRev; //역dominator 집합 // set of node d[n]

            public int nDomEI; // dominator 수
            public int[] DomEI; //dominator 집합
            public int nDomRevEI; // 역dominator 수
            public int[] DomRevEI; //역dominator 집합

            public int nDomInverse;
            public int[] DomInverse;
            public int nDomRevInverse;
            public int[] DomRevInverse;

            public int[] DF;
            public int nDF;
            public int[] PdF;
            public int nPdF;

            public int depth; //1부터 //depth of this node
            public bool done; //loop축소시 사용하지 않는 노드 //Maybe used for mark this node is use for reduce network ("true" for the node which is reduce, "false" in reversed)

            public int[,] conEntry; //New code
            public int nConEntry; //New code

            public int DepthDom; //New code
            public int DepthPdom; //New code

            public bool SOS_Corrected; //mark the SOS is corrected or NOT // SOS_Corrected = true => corrected, SOS_Corrected = false => not corrected.

            public string nodeLabel;
        }

        public struct strLink
        {
            public int fromNode; //시작 Node
            public int toNode; //끝 Node

            public bool bBackJ; //BackJoin Edge면 true
            public bool bBackS; //BackSplit Edge면 true

            public bool bInstance; //현재 인스턴스에 포함되면 true  .................
        }
        
        public struct strNetwork
        {
            public int header; //starting Node 번호

            public int nNode;
            public strNode[] Node;

            public int nLink;
            public strLink[] Link;
        }
        public strNetwork[] Network; //[0]-original  [1]- Split I 후 [2]- Split II 후 final [3]-Loop 축소  [4]-임시작업  [5]-Sub
        //=============================================================

        //노드 검색 결과 // Node Result
        public int nSearchNode;
        public int[] searchNode;

        //Irreducible Check용
        public int nSearchNode_P, nSearchNode_F, nSearchNode_B;
        public int[] searchNode_P, searchNode_F, searchNode_B;

        //=================================================================
        // 모든 Loop 찾기위한 구조체 //Find Node Structure for all LOOP
        private struct strBlock
        {
            public bool LoopHeader;
            public bool Irreducible;

            public int iloop_header; // -1이면 null
            public bool tranversed;
            public int DFSP_pos;

            public int nBackEdge;
            public int[] fromNodeBack;

            public int nReentry;
            public int[] fromNodeReentry;
        }
        private strBlock[] Block;
        //=================================================================

        //Nesting Forest for SESEs and LOOPs
        public struct strFBlock
        {
            public int[] child;
            public int nChild;
            public int depth;
            public int[] Entry;
            public int[] Exit;
            public int nEntry;
            public int nExit;
            public int[] Node;
            public int nNode;
            public int parentBlock;
            public bool SESE;
            public int refIndex;
        }
        public struct STRBLOCK
        {
            public strFBlock[] FBlock;
            public int nFBlock;
            public int maxDepth;
        }
        public STRBLOCK FBLOCK;
        //=========================


        // Loop 정보
        public struct strLoopInform
        {
            public bool Irreducible; //True 면 irreducible Loop, False면 Natural loop
            public int depth; //loop 계층 - 1부터

            public int parentLoop; //부모 loop - 없으면(최상위계층) -1;
            public int nChild; // Child Loop 수
            public int[] child; // Child Loop

            public int header; //header

            public int nBackEdge; // BackEdge수
            public int[] linkBack; // BackEdge

            public int nBackSplit; //BackSplit수
            public int[] BackSplit; //BackSplit node

            public int nEntry; //Entry수
            public int[] Entry; //Entry node

            public int nExit; //Exit수
            public int[] Exit; //Exit node

            public int nNode; //포함 Node수
            public int[] Node; //Node

            public int nConcurrency; //Concurrency수
            public int[] Concurrency; //Concurrency 구분번호 0:없음 1,2,3...

            public int nConEntry; //New code
            public int[,] conEntry; //New code

            public int[] etnCandEn; //external candidate entries; //new code
            public int nEtnCandEn;
            public int[] etnCandEx;
            public int nEtnCandEx;
        }

        public struct strLoop
        {
            public int nLoop; //loop 갯수
            public strLoopInform[] Loop;

            public int maxDepth; //loop 최대 계층
        }
        public strLoop[] Loop; // [0]-original  [1]- 임시작업
        //======================================================================

        //SESE정보
        public struct strSESEInform
        {
            public int depth; //loop 계층 -> 1부터

            public int parentSESE; //부모 loop - 없으면(최상위계층) -1;
            public int nChild; // Child Loop 수
            public int[] child; // Child Loop

            public int Entry; //Entry node
            public int Exit; //Exit node

            public int nNode; //포함 Node수
            public int[] Node; //Node
        }
        public struct strSESE
        {
            public int nSESE; //SESE 갯수
            public strSESEInform[] SESE;

            public int maxDepth; //loop 최대 계층
        }
        public strSESE[] SESE; // [0]-original  [1]- 임시작업
        //======================================================================

        //Concurrent Check
        private int nReachNode;
        private int[] reachNode;

        //find parents Loop
        private int npLoopS;
        private int[] pLoopS;


        //Instant Flow
        private int[] InstantNode;
        private int nInstantNode;


        private int[] SearchXOR;
        private int nSearchXOR, nCurrentXOR;

        //Error List =====================================================================
        //private string[] errorMessage;
        public struct strError
        {
            public string Node; //error Node 번호
            public string Loop; //error Loop 번호
            public string SESE; //new => SESE error
            public string currentKind; //현 Node 종류
            public int messageNum; //error message
        }
        public strError[] Error;
        public int nError;
        
        //=================================================================================

        //          Base    Original    
        public int baseNet, orgNet, midNet, finalNet, reduceNet, nickNet, subNet, dummyNet, conNet, seseNet, redSeNet, acyclicNet, tempNet, DFlow_PdFlow, iDFlow_PdFlow, theRestFlow, reduceTempNet, reduceTempNet2;
        public int orgLoop, subLoop, reduceLoop, tempLoop, newTempLoop;
        public int orgSESE, reduceSESE, finalSESE, tempSESE;

        public int Start, End;

        //private bool bDisplay;
        private bool IrreducibleError, ConcurrencyError, SyntexError, checkReRun;
        public double[] run_Time;

        //More inform
        public double[] informList = new double[30];

        //NEW GLOBAL VARIABLES
        public double tempTime = 0;
        public int[][] adjList = null;
        public bool[,] makeLinkDomTree;
        public bool[,] makeLinkPdomTree;
        public bool[,] makeLink_eDomTree;
        public bool[,] makeLink_ePdomTree;
        public int[,] checkEdges;
        public int maxDepth_DomTree;
        public int maxDepth_PdomTree;

        public string fileName_check;
        public int[,] AjM_Network;
        public int[,] adj_matrix_checkEdges;

        public strLink[] LinkSESE;
        public int lemma2_C, prop4_C, lemma3_C, lemma4_C, lemma5_C;

        public int cardinality = 0;
        public int cardinality_SOS = 0;
        public int cardinality_SOS_fixed = 0;

        public int[] START_EVENT;
        public int nSTART_EVENT;
        public int current_nStartEvent;

        //Poly Simulation
        public bool NeglectModel;
        public int numIncreasedNode_Before_Prune = 0;
        public int numIncreasedNode_After_Prune = 0;
        public int numTotalNode_After_Prune = 0;

        //=================================== End of declaring main variables =============
        public string file_check = "";
        //Initiate
        public clsAnaysisNetwork()
        {
            baseNet = 0; //원 Network
            orgNet = 1; //Original Network
            midNet = 2; // Network
            finalNet = 3; //최종 전체 Network


            reduceNet = 4; // Loop축소후 Network
            nickNet = 5;
            subNet = 6; // 부분  Network
            dummyNet = 7;
            conNet = 8; // Concurency Check시 사용

            seseNet = 9; //SESE Network
            redSeNet = 10; //  SESE 축소후 Network
            acyclicNet = 11; //마지막 acyclic Network

            tempNet = 12; //New code
            DFlow_PdFlow = 13;
            iDFlow_PdFlow = 14;
            theRestFlow = 15;
            reduceTempNet = 16;
            reduceTempNet2 = 17;
            

            orgLoop = 0;
            subLoop = 1;
            reduceLoop = 2;
            tempLoop = 3;
            newTempLoop = 4;

            orgSESE = 0;
            reduceSESE = 1;
            finalSESE = 2;
            tempSESE =3;

            Network = new strNetwork[18];
            Loop = new strLoop[5];
            SESE = new strSESE[4];


            IrreducibleError = false;
            ConcurrencyError = false;
            SyntexError = false;
            checkReRun = false;
        }

        //
        public void Load_Data_ForBreakModel(int currentN, string sFilePath, bool readOrigin)
        {
            string rText;
            string[] words;

            fileName_check = sFilePath;

            StreamReader sr = new StreamReader(sFilePath);
            file_check = sFilePath;
            //노드 수
            rText = sr.ReadLine();
            Network[currentN].nNode = Convert.ToInt32(rText);
            Network[currentN].Node = new strNode[Network[currentN].nNode]; //create enough size of the graph


            //노드 정보 //read file, line by line and add NODE to "Network"
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' '); //words is a array of string word[]
                //MessageBox.Show(words[0].ToString());
                Network[currentN].Node[i].orgNum = i;
                Network[currentN].Node[i].parentNum = i;
                Network[currentN].Node[i].Type_I = "";
                Network[currentN].Node[i].Type_II = "";

                Network[currentN].Node[i].Kind = words[1];
                if (words.Length > 2)
                    for (int w = 2; w < words.Length; w++)
                        Network[currentN].Node[i].Kind = Network[currentN].Node[i].Kind + " " + words[w]; //add more label

                if (Network[currentN].Node[i].Kind == "START")
                {
                    Network[currentN].header = i;
                    Network[currentN].Node[i].Name = words[0];// +"(S)";
                }
                else if (Network[currentN].Node[i].Kind == "END")
                {
                    Network[currentN].Node[i].Name = words[0];// +"(E)";
                }
                else
                {
                    Network[currentN].Node[i].Name = words[0];// i.ToString();
                }
                //if (Network[currentN].Node[i].Name == "SS")
                {
                    //Network[currentN].Node[i].Kind = "XOR";
                }
            }

            // 링크 수 Add link
            rText = sr.ReadLine();
            Network[currentN].nLink = Convert.ToInt32(rText);
            Network[currentN].Link = new strLink[Network[currentN].nLink];

            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' ');

                Network[currentN].Link[i].fromNode = Convert.ToInt32(words[0]);
                Network[currentN].Link[i].toNode = Convert.ToInt32(words[1]);
            }

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                find_NodeInform(currentN, i);
            }


            // Original NETWORK 읽기 //Read Original Network, if it did not have. Move to next step
            if (readOrigin && !sr.EndOfStream)
            {
                rText = sr.ReadLine();
                if (rText.Substring(0, 3) == "ADD")
                {

                    //노드 수
                    rText = sr.ReadLine();
                    Network[baseNet].nNode = Convert.ToInt32(rText);
                    Network[baseNet].Node = new strNode[Network[baseNet].nNode];


                    //노드 정보
                    for (int i = 0; i < Network[baseNet].nNode; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');

                        Network[baseNet].Node[i].orgNum = i;
                        Network[baseNet].Node[i].parentNum = i;
                        Network[baseNet].Node[i].Type_I = "";
                        Network[baseNet].Node[i].Type_II = "";

                        Network[baseNet].Node[i].Kind = words[1];
                        if (words.Length > 2)
                            for (int w = 2; w < words.Length; w++)
                                Network[baseNet].Node[i].Kind = Network[baseNet].Node[i].Kind + " " + words[w]; //add more label

                        if (Network[baseNet].Node[i].Kind == "START")
                        {
                            Network[baseNet].header = i;
                            Network[baseNet].Node[i].Name = words[0];// +"(S)";
                        }
                        else if (Network[baseNet].Node[i].Kind == "END")
                        {
                            Network[baseNet].Node[i].Name = words[0];// +"(E)";
                        }
                        else
                        {
                            Network[baseNet].Node[i].Name = words[0];// i.ToString();
                        }


                    }

                    // 링크 수
                    rText = sr.ReadLine();
                    Network[baseNet].nLink = Convert.ToInt32(rText);
                    Network[baseNet].Link = new strLink[Network[baseNet].nLink];

                    for (int i = 0; i < Network[baseNet].nLink; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');

                        Network[baseNet].Link[i].fromNode = Convert.ToInt32(words[0]);
                        Network[baseNet].Link[i].toNode = Convert.ToInt32(words[1]);
                    }

                    for (int i = 0; i < Network[baseNet].nNode; i++)
                    {
                        find_NodeInform(baseNet, i);
                    }

                }
            }
            sr.Close();
        }
        //
        public void Load_Data(int currentN, string sFilePath, bool readOrigin)
        {
            string rText;
            string[] words;

            fileName_check = sFilePath;

            StreamReader sr = new StreamReader(sFilePath);
            file_check = sFilePath;
            //노드 수
            rText = sr.ReadLine();
            Network[currentN].nNode = Convert.ToInt32(rText);
            Network[currentN].Node = new strNode[Network[currentN].nNode]; //create enough size of the graph


            //노드 정보 //read file, line by line and add NODE to "Network"
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' '); //words is a array of string word[]
                //MessageBox.Show(words[0].ToString());
                Network[currentN].Node[i].orgNum = i;
                Network[currentN].Node[i].parentNum = i;
                Network[currentN].Node[i].Type_I = "";
                Network[currentN].Node[i].Type_II = "";

                Network[currentN].Node[i].Kind = words[1];

                //store node label
                if (words.Length > 2)
                {
                    Network[currentN].Node[i].nodeLabel = "";
                    for (int length = 2; length < words.Length; length++)
                    {
                        Network[currentN].Node[i].nodeLabel = Network[currentN].Node[i].nodeLabel + words[length];
                        if (length < words.Length - 1)
                            Network[currentN].Node[i].nodeLabel = Network[currentN].Node[i].nodeLabel + " ";
                    }
                }

                if (Network[currentN].Node[i].Kind == "START")
                {
                    Network[currentN].header = i;
                    Network[currentN].Node[i].Name = words[0];// +"(S)";
                }
                else if (Network[currentN].Node[i].Kind == "END")
                {
                    Network[currentN].Node[i].Name = words[0];// +"(E)";
                }
                else
                {
                    Network[currentN].Node[i].Name = words[0];// i.ToString();
                }
                //if (Network[currentN].Node[i].Name == "SS")
                {
                    //Network[currentN].Node[i].Kind = "XOR";
                }
            }

            // 링크 수 Add link
            rText = sr.ReadLine();
            Network[currentN].nLink = Convert.ToInt32(rText);
            Network[currentN].Link = new strLink[Network[currentN].nLink];

            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' ');

                Network[currentN].Link[i].fromNode = Convert.ToInt32(words[0]);
                Network[currentN].Link[i].toNode = Convert.ToInt32(words[1]);
            }

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                find_NodeInform(currentN, i);
            }

            // Original NETWORK 읽기 //Read Original Network, if it did not have. Move to next step

            if (readOrigin && !sr.EndOfStream)
            {
                rText = sr.ReadLine();
                if (rText.Substring(0, 3) == "ADD")
                {

                    //노드 수
                    rText = sr.ReadLine();
                    Network[baseNet].nNode = Convert.ToInt32(rText);
                    Network[baseNet].Node = new strNode[Network[baseNet].nNode];


                    //노드 정보
                    for (int i = 0; i < Network[baseNet].nNode; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');

                        Network[baseNet].Node[i].orgNum = i;
                        Network[baseNet].Node[i].parentNum = i;
                        Network[baseNet].Node[i].Type_I = "";
                        Network[baseNet].Node[i].Type_II = "";


                        Network[baseNet].Node[i].Kind = words[1];

                        if (Network[baseNet].Node[i].Kind == "START")
                        {
                            Network[baseNet].header = i;
                            Network[baseNet].Node[i].Name = words[0];// +"(S)";
                        }
                        else if (Network[baseNet].Node[i].Kind == "END")
                        {
                            Network[baseNet].Node[i].Name = words[0];// +"(E)";
                        }
                        else
                        {
                            Network[baseNet].Node[i].Name = words[0];// i.ToString();
                        }


                    }

                    // 링크 수
                    rText = sr.ReadLine();
                    Network[baseNet].nLink = Convert.ToInt32(rText);
                    Network[baseNet].Link = new strLink[Network[baseNet].nLink];

                    for (int i = 0; i < Network[baseNet].nLink; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');

                        Network[baseNet].Link[i].fromNode = Convert.ToInt32(words[0]);
                        Network[baseNet].Link[i].toNode = Convert.ToInt32(words[1]);
                    }

                    for (int i = 0; i < Network[baseNet].nNode; i++)
                    {
                        find_NodeInform(baseNet, i);
                    }

                }
            }
            sr.Close();
        }

        //Split type ?
        public void Pre_Split(int currentN)
        {
            int nodeSS = -1;

            //Find the index of node SS in network.
            for (int i = 0; i < Network[currentN].nNode; i++) 
            {
                if (Network[currentN].Node[i].Name == "SS" && Network[currentN].Node[i].nPost > 1)
                {
                    nodeSS = i;
                    break;
                }
            }

            

            #region NOT NEED?
            //If have not node SS => Not do anything in this stage.

            //Change here (not original)
            //if (nodeSS == -1) return;

            // find SESE with our own algorithm
            //find_SESE(currentN, orgSESE, nodeSS);

            //Start_Split(currentN, orgSESE); //Start_Split( what network?, orgSESE) //SPLIT_TYPE_III
            //Run_Split_Type1();            
            #endregion
        }

        public int Find_Node_byName(int currentN, string nodeName)
        {
            int nodeIndex = -1;

            //Find the index of node SS in network.
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].Name == nodeName)
                {
                    nodeIndex = i;
                    break;
                }
            }
            return nodeIndex;
        }
        public int count_ONE(string strCom)
        {
            int count = 0;
            for (int i = 0; i < strCom.Length; i++)
                if (strCom[i] == '1') count++;
            return (count);
        }
        //Extent nNode and nLink
        private void FULL_extent_Network(int currentN, int add_nNode, int add_nLink)
        {
            //AddNum is the number of node are added
            //A full Graph (extended)
            strNetwork saveNetwork = Network[currentN];

            Network[currentN] = new strNetwork();

            Network[currentN].header = saveNetwork.header;

            Network[currentN].nNode = saveNetwork.nNode + add_nNode;
            Network[currentN].Node = new strNode[Network[currentN].nNode];
            for (int i = 0; i < saveNetwork.nNode; i++)
            {
                //Network[currentN].Node[i] = saveNetwork.Node[i];

                Network[currentN].Node[i].Kind = saveNetwork.Node[i].Kind;
                Network[currentN].Node[i].Name = saveNetwork.Node[i].Name;
                Network[currentN].Node[i].orgNum = saveNetwork.Node[i].orgNum;
                Network[currentN].Node[i].parentNum = saveNetwork.Node[i].parentNum;
                Network[currentN].Node[i].Type_I = saveNetwork.Node[i].Type_I;
                Network[currentN].Node[i].Type_II = saveNetwork.Node[i].Type_II;
                Network[currentN].Node[i].Special = saveNetwork.Node[i].Special;
                Network[currentN].Node[i].nodeLabel = saveNetwork.Node[i].nodeLabel;
                Network[currentN].Node[i].SOS_Corrected = saveNetwork.Node[i].SOS_Corrected;

                Network[currentN].Node[i].depth = saveNetwork.Node[i].depth;
                Network[currentN].Node[i].done = saveNetwork.Node[i].done;

                Network[currentN].Node[i].nPre = saveNetwork.Node[i].nPre;
                Network[currentN].Node[i].Pre = new int[Network[currentN].Node[i].nPre];
                for (int k = 0; k < Network[currentN].Node[i].nPre; k++)
                    Network[currentN].Node[i].Pre[k] = saveNetwork.Node[i].Pre[k];

                Network[currentN].Node[i].nPost = saveNetwork.Node[i].nPost;
                Network[currentN].Node[i].Post = new int[Network[currentN].Node[i].nPost];
                for (int k = 0; k < Network[currentN].Node[i].nPost; k++)
                    Network[currentN].Node[i].Post[k] = saveNetwork.Node[i].Post[k];

                Network[currentN].Node[i].nDom = saveNetwork.Node[i].nDom;
                Network[currentN].Node[i].Dom = new int[Network[currentN].Node[i].nDom];
                for (int k = 0; k < Network[currentN].Node[i].nDom; k++)
                    Network[currentN].Node[i].Dom[k] = saveNetwork.Node[i].Dom[k];

                Network[currentN].Node[i].nDomRev = saveNetwork.Node[i].nDomRev;
                Network[currentN].Node[i].DomRev = new int[Network[currentN].Node[i].nDomRev];
                for (int k = 0; k < Network[currentN].Node[i].nDomRev; k++)
                    Network[currentN].Node[i].DomRev[k] = saveNetwork.Node[i].DomRev[k];

                Network[currentN].Node[i].nDomEI = saveNetwork.Node[i].nDomEI;
                Network[currentN].Node[i].DomEI = new int[Network[currentN].Node[i].nDomEI];
                for (int k = 0; k < Network[currentN].Node[i].nDomEI; k++)
                    Network[currentN].Node[i].DomEI[k] = saveNetwork.Node[i].DomEI[k];

                Network[currentN].Node[i].nDomRevEI = saveNetwork.Node[i].nDomRevEI;
                Network[currentN].Node[i].DomRevEI = new int[Network[currentN].Node[i].nDomRevEI];
                for (int k = 0; k < Network[currentN].Node[i].nDomRevEI; k++)
                    Network[currentN].Node[i].DomRevEI[k] = saveNetwork.Node[i].DomRevEI[k];
            }

            Network[currentN].nLink = saveNetwork.nLink + add_nLink;
            Network[currentN].Link = new strLink[Network[currentN].nLink];
            for (int i = 0; i < saveNetwork.nLink; i++)
            {
                //Network[currentN].Link[i] = saveNetwork.Link[i];

                Network[currentN].Link[i].fromNode = saveNetwork.Link[i].fromNode;
                Network[currentN].Link[i].toNode = saveNetwork.Link[i].toNode;
                Network[currentN].Link[i].bBackJ = saveNetwork.Link[i].bBackJ;
                Network[currentN].Link[i].bBackS = saveNetwork.Link[i].bBackS;
                Network[currentN].Link[i].bInstance = saveNetwork.Link[i].bInstance;
            }

        }
        public bool OR_Module(int currentN, int nodeSS) //input (node_SOS); (Events_List) 
        {
            //**   SS & ANDs & XORs & EVENTs  **//

            //int nodeSS = Find_Node_byName(currentN, "SS");
            if (nodeSS == -1) return (false);
            int nEvent = Network[currentN].Node[nodeSS].nPost;
            int nGateway = (int)(Math.Pow(2, nEvent) - 1);
            int add_nLink = nGateway*nEvent; //? (Max cases)
            int curr_nLink = Network[currentN].nLink;

            //Extend currentN by nGateway
            FULL_extent_Network(currentN, nGateway, add_nLink);
            int currNodeIndex = Network[currentN].nNode - nGateway; //start index for adding new Gateways
            int[] binary_referrence_index = new int[nEvent];

            //START_EVENT = new int[nEvent];
            for (int k = 0; k < nEvent; k++)
            {
                int sEvent = Network[currentN].Node[nodeSS].Post[k];
                //Create a new XOR preceding current START EVENT
                Network[currentN].Node[currNodeIndex + k] = new strNode();
                Network[currentN].Node[currNodeIndex + k].Kind = "XOR";
                Network[currentN].Node[currNodeIndex + k].Name = "XOR" + k;
                Network[currentN].Node[currNodeIndex + k].nodeLabel = "";                

                //Connect EVENT from the newly added XOR 
                Network[currentN].Link[curr_nLink].fromNode = currNodeIndex + k;
                Network[currentN].Link[curr_nLink].toNode = sEvent;
                curr_nLink++;

                binary_referrence_index[k] = currNodeIndex + k;
                //START_EVENT[k] = sEvent;
            }

            currNodeIndex = currNodeIndex + nEvent; //current index for node tracking
            for (int k = 1; k <= nGateway; k++)
            {
                //create BINARY Combination
                string strCombination = Convert.ToString(k, 2).PadLeft(nEvent, '0'); //đệm vào bên trái strCombination nEvent số "0"
                //count the number of "1" in strCombination
                if (count_ONE(strCombination) <= 1) continue;

                //create node AND and connect to the XOR (created before..)
                
                Network[currentN].Node[currNodeIndex] = new strNode();
                Network[currentN].Node[currNodeIndex].Kind = "AND";
                Network[currentN].Node[currNodeIndex].Name = "AND" + k;
                Network[currentN].Node[currNodeIndex].nodeLabel = "";

                for (int i = 0; i < nEvent; i++) //searching through the strCombination and find the "1" which indicate the node XOR to connect
                {
                    //figure out which XOR to to connect
                    if (strCombination.Substring(i, 1) == "0") continue; //use binary string 000101010.. if curren index of string is 0 => no instance case => continue
                    //connection for AND & XOR(1)
                    Network[currentN].Link[curr_nLink].fromNode = currNodeIndex;
                    Network[currentN].Link[curr_nLink].toNode = binary_referrence_index[i];
                    curr_nLink++;

                    //Done 2nd step
                }
                currNodeIndex++;
            }            

            //re-connect SS to Module (XORs) //by modify existing link
            Network[currentN].Node[nodeSS].Kind = "XOR";
            currNodeIndex = Network[currentN].nNode - nGateway;
            for (int cLink = 0; cLink < curr_nLink; cLink++)
            {
                if (Network[currentN].Link[cLink].fromNode == nodeSS)
                {
                    Network[currentN].Link[cLink].toNode = currNodeIndex;
                    currNodeIndex++;
                }
            }

            //add new link from SS to ANDs
            for (int i = Network[currentN].nNode - nGateway + nEvent; i < Network[currentN].nNode; i++)
            {
                Network[currentN].Link[curr_nLink].fromNode = nodeSS;
                Network[currentN].Link[curr_nLink].toNode = i;
                curr_nLink++;
            }
            Network[currentN].nLink = curr_nLink;

            //find additional information
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                find_NodeInform(currentN, i);
            }

            return (true);
        }

        #region InstanceFlow for Prunning approach (Poly)
        private void make_InstanceFlow_Modified(int currentN, int parallelNet, int loop, string errType, string strLoop, int workSESE, int currRigid, int sNode) //Applied POLYVYANYY Approach
        {
            SearchXOR = new int[Network[currentN].nNode]; // 0-탐색
            nSearchXOR = 0;
            nCurrentXOR = 0;
            int nInst = 0;
            do
            {
                nCurrentXOR = 0;
                for (int j = 0; j < Network[currentN].nLink; j++) //fill all this subnetwork is un-visit
                {
                    Network[currentN].Link[j].bInstance = false; //bInstance is used for mark the node which have token (instance flow)
                }
                InstantNode = new int[Network[currentN].nNode];
                nInstantNode = 0;
                //int sNode = Network[currentN].header;
                InstantNode[nInstantNode] = sNode;
                nInstantNode++;
                if (find_InstanceNode(currentN, sNode)) //find_InstanceNode() => output will be stored in InstantNode[] (Global variable!) - all the node which is activated in currentN.
                {
                    string[] readable = new string[nInstantNode];
                    convert_Readable(currentN, InstantNode, ref readable);
                    nInst++;

                    bool errorFlag_Global = false;
                    int currNodeRigid = -1;

                    check_InstanceFlow_Modified(currentN, loop, errType, strLoop, ref errorFlag_Global, ref currNodeRigid);  //resulting in 

                    if (errorFlag_Global == true) //error detected in this instanceFlow
                    {
                        if (Node_In_SESE(workSESE, currNodeRigid, currRigid) == true)
                        {
                            //trace back to enSESE of currRigid and remove node (ANDx) or (SS-XOR)
                            int nodeRemove = -1;
                            int tempXOR = -1;
                            for (int k = 0; k < nInstantNode; k++)
                            {
                                if (Network[currentN].Node[InstantNode[k]].Name.Length < 4) continue;
                                if (Network[currentN].Node[InstantNode[k]].Name.Substring(0, 3) == "AND") { nodeRemove = InstantNode[k]; }
                                if (Network[currentN].Node[InstantNode[k]].Name.Substring(0, 3) == "XOR") { tempXOR = InstantNode[k]; }
                                if (nodeRemove != -1 || tempXOR != -1) break;
                            }                        
                            if (nodeRemove == -1) nodeRemove = tempXOR;

                            //marking remove "nodeRemove" (Target is DFS(adjList)) ==>> Try REMOVE EDGE only (Remove SS-AND or SS-XOR)
                            int rm_fromNode = sNode; //SESE[workSESE].SESE[currRigid].Entry;
                            int rm_toNode = nodeRemove;

                            //Merge Link list in currentN
                            for (int k = 0; k < Network[currentN].nLink; k++)
                            {
                                if (Network[currentN].Link[k].fromNode == rm_fromNode && Network[currentN].Link[k].toNode == rm_toNode)
                                {
                                    for (int m = k; m < Network[currentN].nLink - 1; m++)
                                    {
                                        Network[currentN].Link[m] = Network[currentN].Link[m + 1];
                                    }
                                    Network[currentN].nLink--;
                                    break;
                                }
                            }

                            //make another for parallelNet
                            for (int k = 0; k < Network[parallelNet].nLink; k++)
                            {
                                if (Network[parallelNet].Link[k].fromNode == rm_fromNode && Network[parallelNet].Link[k].toNode == rm_toNode)
                                {
                                    for (int m = k; m < Network[parallelNet].nLink - 1; m++)
                                    {
                                        Network[parallelNet].Link[m] = Network[parallelNet].Link[m + 1];
                                    }
                                    Network[parallelNet].nLink--;
                                    break;
                                }
                            }
                        }
                    }
                }
            } while (nSearchXOR > 0);

            cardinality = nInst; //Newcode
        }
        private void check_InstanceFlow_Modified(int currentN, int loop, string errType, string strLoop, ref bool errorFlag_Global, ref int currNodeRigid) //no Error[] recorded
        {

            for (int i = 0; i < nInstantNode; i++)
            {
                //XOR Join에 여러 Instant In이면 error
                if (Network[currentN].Node[InstantNode[i]].Kind == "XOR" && Network[currentN].Node[InstantNode[i]].nPre > 1)
                {
                    int numIn = 0;
                    for (int j = 0; j < Network[currentN].Node[InstantNode[i]].nPre; j++)
                    {
                        bool bLink = false;
                        for (int k = 0; k < Network[currentN].nLink; k++)
                        {
                            if (Network[currentN].Link[k].fromNode == Network[currentN].Node[InstantNode[i]].Pre[j] && Network[currentN].Link[k].toNode == InstantNode[i])
                            {
                                if (Network[currentN].Link[k].bInstance) bLink = true;
                                break;
                            }
                        }
                        if (!bLink) continue;

                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (Network[currentN].Node[InstantNode[i]].Pre[j] == InstantNode[k]) numIn++;
                        }
                    }

                    if (numIn > 1) //error
                    {
                        //Error[nError].Loop = strLoop;
                        //Error[nError].Node = Network[currentN].Node[InstantNode[i]].parentNum.ToString();
                        //Error[nError].currentKind = Network[currentN].Node[InstantNode[i]].Kind;
                        errorFlag_Global = true; //just for POLYVYANYY Mimic
                        currNodeRigid = Network[currentN].Node[InstantNode[i]].parentNum; //just for POLYVYANYY Mimic
                        return;

                        if (loop == -1)
                        {
                            //Error[nError].messageNum = 10;
                        }
                        else
                        {
                            if (errType == "SESE")
                            {
                                //Error[nError].Loop = "";
                                //Error[nError].SESE = strLoop;
                                //Error[nError].messageNum = 27; //SESE lack of synchronization

                                
                            }
                            //if (errType == "eFwd") Error[nError].messageNum = 3; //rule 2.1
                            //if (errType == "eBwd") Error[nError].messageNum = 4; //rule 2.1
                            //if (errType == "DFlow_PdFlow") Error[nError].messageNum = 24; //rule 6.1
                            //if (errType == "iDFlow_PdFlow") Error[nError].messageNum = 25; //rule 6.2
                            //if (errType == "HFlow") Error[nError].messageNum = 26; //rule 6.3

                        }

                        //nError++;
                        //add_Error();
                    }
                }

                //AND Join에  Instant In이 부족하면 error
                if (Network[currentN].Node[InstantNode[i]].Kind == "AND" && Network[currentN].Node[InstantNode[i]].nPre > 1)
                {
                    int numIn = 0;
                    for (int j = 0; j < Network[currentN].Node[InstantNode[i]].nPre; j++)
                    {
                        bool bLink = false;
                        for (int k = 0; k < Network[currentN].nLink; k++)
                        {
                            if (Network[currentN].Link[k].fromNode == Network[currentN].Node[InstantNode[i]].Pre[j] && Network[currentN].Link[k].toNode == InstantNode[i])
                            {
                                if (Network[currentN].Link[k].bInstance) bLink = true;
                                break;
                            }
                        }
                        if (!bLink) continue;

                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (Network[currentN].Node[InstantNode[i]].Pre[j] == InstantNode[k]) numIn++;
                        }
                    }

                    if (numIn < Network[currentN].Node[InstantNode[i]].nPre) //error
                    {
                        //Error[nError].Loop = strLoop;
                        //Error[nError].Node = Network[currentN].Node[InstantNode[i]].parentNum.ToString();
                        //Error[nError].currentKind = Network[currentN].Node[InstantNode[i]].Kind;
                        errorFlag_Global = true; //just for POLYVYANYY Mimic
                        currNodeRigid = Network[currentN].Node[InstantNode[i]].parentNum; //just for POLYVYANYY Mimic
                        return;

                        if (loop == -1)
                        {
                            //Error[nError].messageNum = 11;
                        }
                        else
                        {
                            if (errType == "SESE")
                            {
                                //Error[nError].Loop = "";
                                //Error[nError].SESE = strLoop;
                                //Error[nError].messageNum = 28; //SESE deadlock 

                                
                            }
                            //if (errType == "eFwd") Error[nError].messageNum = 5;
                            //if (errType == "eBwd") Error[nError].messageNum = 6;
                            //same for XOR errors
                            //if (errType == "DFlow_PdFlow") Error[nError].messageNum = 24; //rule 6.1
                            //if (errType == "iDFlow_PdFlow") Error[nError].messageNum = 25; //rule 6.2
                            //if (errType == "HFlow") Error[nError].messageNum = 26; //rule 6.3
                            //if (errType == "SESE") Error[nError].messageNum = 28; 
                        }
                        //nError++;
                        //add_Error();
                    }
                }
            }
        }
        #endregion
     
        public void Mimic_Poly_Approach(int currentN, int parallelNet, int workSESE) //parallelNet will be the output network
        {
            for (int i = 0; i < Network[parallelNet].nNode; i++)
            {
                if (Network[parallelNet].Node[i].SOS_Corrected == false && Network[parallelNet].Node[i].Name == "SS")
                {
                    if (OR_Module(parallelNet, i)) { }
                }
            }
            //COPY NETWORK to parallelNet to currentN
            Network[currentN] = Network[parallelNet];
            extent_Network(currentN, 0);

            //for each rigid (in bottom up order of SESE tree)
            int maxSESE_Depth = SESE[workSESE].maxDepth;
            do
            {
                for (int i = 0; i < SESE[workSESE].nSESE; i++)
                {
                    if (SESE[workSESE].SESE[i].depth != maxSESE_Depth) continue;

                    if (Bond_Check(currentN, workSESE, i) == true)
                    {
                        //reduce the bond
                        reduce_seseNetwork(currentN, workSESE, i);
                        continue;
                    }

                    if (check_StartEvent_InsideRigid(workSESE, i) == false) continue; //can use alternative ways.

                    bool[] mark = new bool[Network[currentN].nNode];               

                    make_InstanceFlow_Modified(currentN, parallelNet, -1, "SESE", "", workSESE, i, SESE[workSESE].SESE[i].Entry); //remove node ANDx and SS-XOR also (BY CHANGING THE LINKs)

                    for (int k = 0; k < Network[currentN].nNode; k++)
                    {
                        find_NodeInform(currentN, k); //UPDATE THE Network based on LINKs
                    }

                    //if (all branch in rigid is removed ~ NodeSet[] = null)
                    if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].nPost == 0)
                    {
                        //if (no parent SESE) -> neglect that model
                        if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Name == "SS" && Network[currentN].Node[Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Pre[0]].Kind == "START")
                        {
                            //MessageBox.Show("Neglect this model. No more branches were found", "Error Detected");
                            NeglectModel = true;
                            continue;
                        }
                        //if (there are outer SESE) -> Remove all XORx and related ANDx
                        if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Name == "SS" && Network[currentN].Node[Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Pre[0]].Kind != "START")
                        {
                            removeOuter_SESE_Node(currentN, workSESE, i);
                            removeOuter_SESE_Node(parallelNet, workSESE, i);
                        }
                    }
                    //else { Do something?? }                    
                }
                maxSESE_Depth--;
            } while (maxSESE_Depth > 0);
        }

        public void removeOuter_SESE_Node(int currentN, int workSESE, int i)
        {
            //Remove all XORx and related ANDx ====>> HOW ABOUT THIS PARENT IS BOND??
            int XORx = Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Pre[0];
            if (Network[currentN].Node[XORx].Name == "SS")
            {
                for (int j = 0; j < Network[currentN].nLink; j++)
                {
                    if (Network[currentN].Link[j].toNode == SESE[workSESE].SESE[i].Entry)
                    {
                        //remove links from SS-ANDx(s) and (SS-XOXx)
                        for (int m = j; m < Network[currentN].nLink - 1; m++)
                        {
                            Network[currentN].Link[m] = Network[currentN].Link[m + 1];
                        }
                        Network[currentN].nLink--;
                    }
                }
                //Bond Problem solved (SHOULD PRUNE THE SS_sx also)
            }
            else
            {
                //find all ANDx related with XOXx
                int[] listANDx = new int[Network[currentN].nNode]; //?? Do we need this??
                int nListANDx = 0;
                for (int j = 0; j < Network[currentN].nLink; j++)
                {
                    if (Network[currentN].Link[j].toNode == XORx)
                    {
                        listANDx[nListANDx] = Network[currentN].Link[j].fromNode;
                        nListANDx++;
                        //remove links from SS-ANDx(s) and (SS-XOXx)

                        for (int m = j; m < Network[currentN].nLink - 1; m++)
                        {
                            Network[currentN].Link[m] = Network[currentN].Link[m + 1];
                        }
                        Network[currentN].nLink--;
                    }
                }
                for (int k = 0; k < Network[currentN].nNode; k++)
                {
                    find_NodeInform(currentN, k); //UPDATE THE Network based on LINKs
                }
            }
        }

        public void findStartEvent(int currentN)
        {
            int nodeSS = Find_Node_byName(currentN, "SS");
            if (nodeSS == -1) return;
            int nEvent = Network[currentN].Node[nodeSS].nPost;

            START_EVENT = new int[nEvent];
            nSTART_EVENT = 0;
            for (int k = 0; k < nEvent; k++)
            {
                int sEvent = Network[currentN].Node[nodeSS].Post[k];
                START_EVENT[k] = sEvent;
                nSTART_EVENT++;
            }
        }

        public void Count_PolySimulation(int finalNet, int nickNet,int nodeSS)
        {
            //if (SyntexError) return;
            if (nodeSS == -1) return;
            //Count POLY SIMULATION
            numIncreasedNode_Before_Prune = Network[finalNet].nNode - Network[nickNet].nNode; //nickNet ~ Original Network
            numIncreasedNode_After_Prune = 0;
            adjList_Create(finalNet, ref adjList);
            //DFS_Rigids()
            int CCs = 0;
            int[,] A = convert_AJ_Directed(finalNet); //use directed graph
            int[] mark = new int[Network[finalNet].nNode]; //autofill = false;
            CCs++;
            ConnectedComponent(finalNet, nodeSS, ref mark, ref A, CCs);
            numTotalNode_After_Prune = 0;
            
            for (int i = 0; i < Network[finalNet].nNode; i++)
            {
                if (mark[i] == CCs)
                {
                    numTotalNode_After_Prune++; //numTotalNode_After_Prune = 
                    if (Network[finalNet].Node[i].Name.Length > 3)
                    {
                        if (Network[finalNet].Node[i].Name.Substring(0, 3) == "AND" || Network[finalNet].Node[i].Name.Substring(0, 3) == "XOR")
                            numIncreasedNode_After_Prune++;
                    }
                }
            }
            numTotalNode_After_Prune--; //Not count START AND END (but we start at SS)

            //Count the START EVENT Leftover
            current_nStartEvent = 0;
            for (int i = 0; i < Network[finalNet].nNode; i++)
            {
                if (mark[i] == CCs)
                {
                    for (int j = 0; j < nSTART_EVENT; j++)
                        if (i == START_EVENT[j])
                            current_nStartEvent++;
                }
            }

        }
        public int find_SE_Rigid(int currentN, int workSESE, int START_node)
        {
            for (int i = 0; i < SESE[workSESE].nSESE; i++ )
            {
                for (int j = 0; j < SESE[workSESE].SESE[i].nNode; j++)
                {
                    if (SESE[workSESE].SESE[i].Entry == Network[currentN].Node[START_node].Post[0])
                        return i;
                }
            }
            return -1;
        }
        public int Run_Analysis_OR_Replace(int NofRun)
        {
            run_Time = new double[5];
            DateTime stTime = new DateTime();
            int errorNum = 0;

            check_SyntexError();
            if (SyntexError) errorNum += 3;

            DateTime totalTime = new DateTime();
            totalTime = DateTime.Now;
            int maxRun = 1;

            findStartEvent(orgNet);
            int nodeSS = Find_Node_byName(orgNet, "SS");

            for (int k = 0; k < NofRun; k++)
            {
                HiPerfTimer pt = new HiPerfTimer();
                pt.Start();
                for (int countTime = 0; countTime < maxRun; countTime++)
                {
                    Run_Split_Type1(); //Split Type 1

                    Run_FindLoop(midNet, orgLoop); //Find the loop, indentify loop-nesting forest of G
                    Network[finalNet] = Network[midNet];
                    if (IrreducibleError) return errorNum + 1;

                    Run_Split_Type2(); //Split Type 2

                    //추가 SESE identification=====
                    Start = find_nodeName(finalNet, "START");
                    End = find_nodeName(finalNet, "END");

                    //preProcessing
                    preProcessingSESE(finalNet, finalSESE, -2); //find Dom - Pdom
                    TestingExe(finalNet); //find DomEI - PdomEI
                }
                pt.Stop();
                run_Time[0] += pt.Duration;//Count to here
                HiPerfTimer pt1 = new HiPerfTimer();
                pt1.Start();

                find_SESE_new(finalNet, finalSESE, -2);

                //run_Time[1] += (DateTime.Now - stTime).TotalMilliseconds;
                pt1.Stop();
                run_Time[1] += pt1.Duration;

                stTime = DateTime.Now;
                HiPerfTimer pt2 = new HiPerfTimer();
                pt2.Start();
                for (int countTime = 0; countTime < maxRun; countTime++)
                {                    
                    Run_FindLoop(finalNet, orgLoop); //Cái này chỉ cố định cho 1 network ==> original => Run_FindLoop() - no input parameters
                    //Create Nesting Forest of SESEs and LOOPs
                    make_NestingForest(finalNet, finalSESE, orgLoop); //because run_findLoop store loops in orgLoop;
                    Start_Split(finalNet, finalSESE, true);// Type III Split
                }
                pt2.Stop();
                run_Time[2] += pt2.Duration;
                
                Run_CheckLoop();

                checkReRun = true; //avoid run multiple time unnecessary function
            }

            for (int i = 0; i < 5; i++)
            {
                //run_Time[i] = run_Time[i] / (double)NofRun; //Run Analysis twice ?? for measure the performance
                run_Time[i] = run_Time[i] / 100;
                //run_Time[i] = run_Time[i] / 1000;
            }

            //if the PdFlow is rigid => Replace it by OR module ==============

            //Mimic Poly approach
            Network[nickNet] = Network[finalNet];
            extent_Network(nickNet, 0);
            NeglectModel = false;
            
            //Make this RIGID as an seperate NETWORK. (From SS to its CIPd) ===> USE tempSESE
            
            int SE_Rigid = find_SE_Rigid(finalNet, finalSESE, Start);
             /* 
            make_subNetwork(nickNet, finalNet, finalSESE, SE_Rigid, "SESE", -1);

            Start = find_nodeName(finalNet, "START");
            End = find_nodeName(finalNet, "END");
            Network[finalNet].header = Start;

            preProcessingSESE(finalNet, finalSESE, -2);
            TestingExe(finalNet);
            find_SESE_new(finalNet, finalSESE, -2);
            */
            
            //cut down the connect from CIPd
            //int tempPost = Network[finalNet].Node[SESE[finalSESE].SESE[SE_Rigid].Exit].nPost;
            //Network[finalNet].Node[SESE[finalSESE].SESE[SE_Rigid].Exit].nPost = 0;

            Mimic_Poly_Approach(midNet, finalNet, finalSESE);

            //Network[finalNet].Node[SESE[finalSESE].SESE[SE_Rigid].Exit].nPost = tempPost;
            //make_InstanceFlow(finalNet, -1, "SESE", "");
            //=================================================================
            

            //Record more inform
            count_Bonds_Rigids(finalNet, finalSESE, reduceLoop); //count # of bonds and rigids
            informRecording(finalNet); //inform list [1] .. [9]
            multiStart_CIPD_Count(orgNet, finalNet, finalSESE); //# of node in PdFlow(SS) - inform list [10]; number of entries informList[11]; number of exit informList[12]; % of PdFlow(SOS(se)) informList[22];
            //# of bonds: informList[13]; # of rigids: informList[14]; 
            countALL_SS_OR(finalNet);//# of SS_OR informList[15];

            //# of node Bond and Rigid.          
            count_Node_Bond_Rigid(finalNet, finalSESE, reduceLoop); //# of Node_Bond informList[16]; # of Node_Rigid informList[17]; #of Node SOS not corrected informList[23]
            //count OR split in rigid => store in informList[18] # of OR Split in rigis for SS; informList[19] store edges from OR split for SS; informList[20], informList[21] for non-SS

            cardinality_SOS_fixed = count_cardinality_SS(finalNet); //count cardinality after SESE configuration
            //cardinality_SOS = count_cardinality_SS(orgNet); //count cardinality of original Network (SOS)

            Count_PolySimulation(finalNet, nickNet, nodeSS);
            
            
            return errorNum;
        }

        public int Run_Analysis_Temp(int NofRun)
        {
            run_Time = new double[5];

            DateTime stTime = new DateTime();

            int errorNum = 0;

            check_SyntexError();
            if (SyntexError) errorNum += 3;

            DateTime totalTime = new DateTime();
            totalTime = DateTime.Now;
            int maxRun = 1;

            findStartEvent(orgNet);

            Run_Split_Type1();
            //Run_FindLoop(midNet, orgLoop);
            Network[finalNet] = Network[midNet];
            //Run_Split_Type2();

            //Network[finalNet] = Network[orgNet];
            
            //find Dom, Pdom...
            find_Dom(finalNet); //Dom
            find_DomRev(finalNet); //Postdom
            return 0;

        }

        //return the number of errors
        public int Run_Analysis(int NofRun)
        {
            run_Time = new double[5];

            DateTime stTime = new DateTime();

            int errorNum = 0;

            check_SyntexError();
            if (SyntexError) errorNum += 3;

            DateTime totalTime = new DateTime();
            totalTime = DateTime.Now;
            int maxRun = 1;

            findStartEvent(orgNet);
            
            
            for (int k = 0; k < NofRun; k++)
            {         
                //Type-1 split================
                //stTime = DateTime.Now;
                HiPerfTimer pt = new HiPerfTimer();
                pt.Start();
                for (int countTime = 0; countTime < maxRun; countTime++)
                {
                    Run_Split_Type1(); //Split Type 1

                    Run_FindLoop(midNet, orgLoop); //Find the loop, indentify loop-nesting forest of G
                    Network[finalNet] = Network[midNet];
                    //run_Time[1] += (DateTime.Now - stTime).TotalSeconds;
                    if (IrreducibleError) return errorNum + 1;

                    Run_Split_Type2(); //Split Type 2
                    #region Hide
                    //if (Loop[orgLoop].nLoop == 0) run_Time[2] += 0;
                    //else run_Time[2] += (DateTime.Now - stTime).TotalSeconds;

                    //return 0;
                    //Loop identification==========
                    //stTime = DateTime.Now;
                    //Run_FindLoop(finalNet, orgLoop); //Find the loop, indentify loop-nesting forest of G
                    //run_Time[1] += (DateTime.Now - stTime).TotalSeconds;
                    //if (IrreducibleError) return errorNum + 1;

                    //stTime = DateTime.Now;
                    #endregion
                    //추가 SESE identification=====
                    Start = find_nodeName(finalNet, "START");
                    End = find_nodeName(finalNet, "END");

                    //preProcessing
                    preProcessingSESE(finalNet, finalSESE, -2);
                    TestingExe(finalNet);                    
                }
                pt.Stop();
                run_Time[0] += pt.Duration;//Count to here

                //stTime = DateTime.Now;
                HiPerfTimer pt1 = new HiPerfTimer();
                pt1.Start();
                for (int countTime = 0; countTime < maxRun; countTime++)
                {
                    //find_SESE_Dummy(finalNet, finalSESE, -2); //-2 mean use for the big model only <= just trick           

                    find_SESE_WithLoop(finalNet, finalSESE, -2);
                }

                //run_Time[1] += (DateTime.Now - stTime).TotalMilliseconds;
                pt1.Stop();
                run_Time[1] += pt1.Duration;

                stTime = DateTime.Now;
                HiPerfTimer pt2 = new HiPerfTimer();
                pt2.Start();
                for (int countTime = 0; countTime < maxRun; countTime++)
                {
                    Start_Split(finalNet, finalSESE, true);// Type III Split

                    Run_FindLoop(finalNet, orgLoop); //Cái này chỉ cố định cho 1 network ==> original => Run_FindLoop() - no input parameters

                    //polygonIdentification(finalNet, finalSESE);

                    //Create Nesting Forest of SESEs and LOOPs
                    make_NestingForest(finalNet, finalSESE, orgLoop); //because run_findLoop store loops in orgLoop;

                    //Pologon Identification => Search each SESE region
                }
                pt2.Stop();
                run_Time[2] += pt2.Duration;
                
                //update loop header
                //upgrade_LoopHeader(finalNet, orgLoop);

                //== Verification state===========
                //stTime = DateTime.Now;
                Run_CheckLoop();
                //if (Loop[orgLoop].nLoop == 0) run_Time[3] += 0;
                //else run_Time[3] += (DateTime.Now - stTime).TotalSeconds;
                //if (ConcurrencyError) return errorNum + 2; //삭제???????
                //===========
                //stTime = DateTime.Now;
                //Run_CheckNetwork();//??
                //run_Time[4] += (DateTime.Now - stTime).TotalSeconds;

                checkReRun = true; //avoid run multiple time unnecessary function
            }
            
            //MessageBox.Show((DateTime.Now - totalTime).TotalSeconds.ToString(),"Long");
            for (int i = 0; i < 5; i++)
            {
                //run_Time[i] = run_Time[i] / (double)NofRun; //Run Analysis twice ?? for measure the performance
                run_Time[i] = run_Time[i] / 100;
                //run_Time[i] = run_Time[i] / 1000;
            }

            //informSameLabelEvent(finalNet); //can be removed
            
            //Record more inform
            count_Bonds_Rigids(finalNet, finalSESE, reduceLoop); //count # of bonds and rigids
            informRecording(finalNet); //inform list [1] .. [9]
            //multiStart_CIPD_Count(orgNet, finalNet, finalSESE); //# of node in PdFlow(SS) - inform list [10]; number of entries informList[11]; number of exit informList[12]; % of PdFlow(SOS(se)) informList[22];
            //# of bonds: informList[13]; # of rigids: informList[14]; 
            //countALL_SS_OR(finalNet);//# of SS_OR informList[15];
            
            //# of node Bond and Rigid.          
            //count_Node_Bond_Rigid(finalNet, finalSESE, reduceLoop); //# of Node_Bond informList[16]; # of Node_Rigid informList[17]; #of Node SOS not corrected informList[23]
            //count OR split in rigid => store in informList[18] # of OR Split in rigis for SS; informList[19] store edges from OR split for SS; informList[20], informList[21] for non-SS

            //findStartEvent(orgNet);
            //cardinality_SOS_fixed = count_cardinality_SS(finalNet); //count cardinality after SESE configuration
            //cardinality_SOS = count_cardinality_SS(orgNet); //count cardinality of original Network (SOS)

            //MessageBox.Show("Bond: " + informList[13].ToString() + " ; Rigid: " + informList[14], "Report");
            
            return errorNum;
        }

        public int Run_Analysis_WholeEnum(int NofRun)
        {
            run_Time = new double[5];

            DateTime stTime = new DateTime();

            int errorNum = 0;

            check_SyntexError();
            if (SyntexError) errorNum += 3;

            DateTime totalTime = new DateTime();
            totalTime = DateTime.Now;
            int maxRun = 1;

            findStartEvent(orgNet);


            for (int k = 0; k < NofRun; k++) {
                //Type-1 split================
                //stTime = DateTime.Now;
                HiPerfTimer pt = new HiPerfTimer();
                pt.Start();
                for (int countTime = 0; countTime < maxRun; countTime++) {
                    //Run_Split_Type1(); //Split Type 1

                    //Run_FindLoop(midNet, orgLoop); //Find the loop, indentify loop-nesting forest of G
                    Network[finalNet] = Network[orgNet];
                    //run_Time[1] += (DateTime.Now - stTime).TotalSeconds;
                    if (IrreducibleError) return errorNum + 1;

                    //Run_Split_Type2(); //Split Type 2

                    //추가 SESE identification=====
                    Start = find_nodeName(finalNet, "START");
                    End = find_nodeName(finalNet, "END");

                    //preProcessing
                    preProcessingSESE(finalNet, finalSESE, -2);
                    TestingExe(finalNet);
                }
                pt.Stop();
                run_Time[0] += pt.Duration;//Count to here

                //stTime = DateTime.Now;
                HiPerfTimer pt1 = new HiPerfTimer();
                pt1.Start();
                for (int countTime = 0; countTime < maxRun; countTime++) {
                    find_SESE_Dummy(finalNet, finalSESE, -2); //-2 mean use for the big model only <= just trick           

                    //find_SESE_WithLoop(finalNet, finalSESE, -2);
                }

                //run_Time[1] += (DateTime.Now - stTime).TotalMilliseconds;
                pt1.Stop();
                run_Time[1] += pt1.Duration;

                stTime = DateTime.Now;
                HiPerfTimer pt2 = new HiPerfTimer();
                pt2.Start();
                for (int countTime = 0; countTime < maxRun; countTime++) {
                    Start_Split(finalNet, finalSESE, true);// Type III Split

                    Run_FindLoop(finalNet, orgLoop); //Cái này chỉ cố định cho 1 network ==> original => Run_FindLoop() - no input parameters

                    //polygonIdentification(finalNet, finalSESE);

                    //Create Nesting Forest of SESEs and LOOPs
                    make_NestingForest(finalNet, finalSESE, orgLoop); //because run_findLoop store loops in orgLoop;

                    //Pologon Identification => Search each SESE region
                }
                pt2.Stop();
                run_Time[2] += pt2.Duration;

                //stTime = DateTime.Now;
                Run_CheckLoop();
                
                checkReRun = true; //avoid run multiple time unnecessary function
            }

            for (int i = 0; i < 5; i++) {
                //run_Time[i] = run_Time[i] / (double)NofRun; //Run Analysis twice ?? for measure the performance
                run_Time[i] = run_Time[i] / 100;
                //run_Time[i] = run_Time[i] / 1000;
            }

            //informSameLabelEvent(finalNet); //can be removed

            //Record more inform
            count_Bonds_Rigids(finalNet, finalSESE, reduceLoop); //count # of bonds and rigids
            informRecording(finalNet); //inform list [1] .. [9]
 
            return errorNum;
        }
        //======================================

        public void TestingExe(int currentN)
        {
            #region for testing execution time
            find_DomEI(currentN, -2); //SS split node만..... //eDom^-1(NodeD)
            find_DomRevEI(currentN); // join node만..... //ePdom^-1(NodeR)

            int nNode = Network[currentN].nNode;

            makeLink_eDomTree = new bool[nNode, nNode];
            makeLink_ePdomTree = new bool[nNode, nNode];
            
            //copy array
            for (int i = 0; i < Network[currentN].nNode; i++)
                for (int j = 0; j < Network[currentN].nNode; j++ )
                {
                    makeLink_eDomTree[i, j] = makeLinkDomTree[i, j];
                    makeLink_ePdomTree[i, j] = makeLinkPdomTree[i, j];
                }

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                //extended eDomTree
                for (int j = 0; j < Network[currentN].Node[i].nDF; j++)
                {
                    makeLink_eDomTree[i, Network[currentN].Node[i].DF[j]] = true;
                }

                //extended ePdomTree
                for (int j = 0; j < Network[currentN].Node[i].nPdF; j++)
                {
                    makeLink_ePdomTree[Network[currentN].Node[i].PdF[j], i] = true;
                }
            }
            #endregion
        }


        //More Information
        #region Record More Information for simulation
        #region Count Cardinality procedures
        public int count_cardinality_SS(int currentN)
        {
            //1st step: Transform finalNet in to concurrency Event Net (1 single EE)
            Network[tempNet] = Network[currentN];
            extent_Network(tempNet, 1);

            strNode newOR = new strNode();
            newOR.Kind = "OR";
            newOR.Name = "NewEE";

            int[] StartEvent = START_EVENT;
            int n_StartEvent = nSTART_EVENT;
            //extend_concurrency_Event(tempNet, ref StartEvent, ref n_StartEvent);
            int lastNode = Network[tempNet].nNode - 1;
            Network[tempNet].Node[lastNode] = newOR;

            bool checkContinute = false; //Check have SS or not
            for (int i = 0; i < Network[tempNet].nNode; i++)
            {
                if (Network[tempNet].Node[i].Kind == "START")
                    if (Network[tempNet].Node[Network[tempNet].Node[i].Post[0]].Name == "SS")
                        checkContinute = true;
            }

            if (checkContinute)
            {
                for (int i = 0; i < n_StartEvent; i++)
                {
                    for (int j = 0; j < Network[tempNet].nLink; j++)
                    {
                        if (Network[tempNet].Link[j].fromNode == StartEvent[i])
                        {
                            Network[tempNet].Link[j].toNode = lastNode;
                        }
                    }
                }

                for (int i = 0; i < Network[tempNet].nLink; i++)
                {
                    if (Network[tempNet].Node[Network[tempNet].Link[i].toNode].Kind == "END")
                    {
                        Network[tempNet].Link[i].fromNode = lastNode;
                    }
                }

                //update node information
                for (int i = 0; i < Network[tempNet].nNode; i++)
                {
                    find_NodeInform(tempNet, i);
                }
                Network[tempNet].header = Network[currentN].header;
                //=========================

                cardinality_SOS = Convert.ToInt32(Math.Pow(2, n_StartEvent) - 1);

                //2nd step: Run instance flow and count the number of  instance => # of cardinality
                make_InstanceFlow_simplified(tempNet, -1, "", "");
            }
            else
            {
                cardinality = 1;
                cardinality_SOS = 1;
            }
            return cardinality;
        }
        public void extend_concurrency_Event(int currentN, ref int[] StartEvent, ref int n_StartEvent)
        {
            strNetwork saveNetwork = Network[currentN]; 

            //find link have sNode = SS
            //Network[currentN].nLink = saveNetwork.nLink + addNum;
            //Network[currentN].Link = new strLink[Network[currentN].nLink];            
            for (int i = 0; i < saveNetwork.nLink; i++)
            {
                if (saveNetwork.Node[saveNetwork.Link[i].fromNode].Name == "SS")
                {
                    if (saveNetwork.Node[saveNetwork.Link[i].toNode].Name != "SS")
                    {
                        StartEvent[n_StartEvent] = saveNetwork.Link[i].toNode;
                        n_StartEvent++;
                    }
                }
            }

            if (n_StartEvent == 0)
            {
                for (int i = 0; i < saveNetwork.nLink; i++)
                {
                    if (saveNetwork.Node[saveNetwork.Link[i].fromNode].Name == "S") //Start node
                    {
                        if (saveNetwork.Node[saveNetwork.Link[i].toNode].Name != "SS")
                        {
                            StartEvent[n_StartEvent] = saveNetwork.Link[i].toNode;
                            n_StartEvent++;
                        }
                    }
                }
            }

        }
        #endregion

        public void informSameLabelEvent(int currentN)
        {
            string informLabel = "";
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                for (int j = i + 1; j < Network[currentN].nNode; j++)
                {
                    if ((Network[currentN].Node[i].nodeLabel == Network[currentN].Node[j].nodeLabel) && (Network[currentN].Node[i].Kind == "EVENT"))
                        informLabel = informLabel + Network[currentN].Node[i].Name + " " + Network[currentN].Node[j].Name + ";" + " ";
                }
            }
            MessageBox.Show(informLabel, "Caution");
        } //can be removed => Count events have same label
        public void count_Bonds_Rigids(int finalNet, int workSESE, int workLoop)
        {
            int currentN = tempNet; //just assign a temporary variable.
            Network[currentN] = Network[finalNet];
            extent_Network(currentN, 0);
            int curDepth = FBLOCK.maxDepth;
            do
            {
                for (int j = 0; j < FBLOCK.nFBlock; j++)
                {
                    if (FBLOCK.FBlock[j].depth != curDepth) continue;

                    int i = FBLOCK.FBlock[j].refIndex;

                    if (FBLOCK.FBlock[j].SESE)
                    {
                        if (Bond_Check(currentN, workSESE, i)) //bond model
                        {
                            informList[13]++;
                        }
                        else //rigid model
                        {
                            informList[14]++;
                        }
                        reduce_seseNetwork(currentN, workSESE, i);
                    }
                    else
                    {
                        if (Loop[workLoop].Loop[i].Irreducible) // Irreducible Loop면
                        {

                            reduce_Network(currentN, workLoop, i, "", true);
                        }
                        else // Natural Loop면
                        {
                            //Natural Loop have single exit also an SESE; solution => Check L<h> whether the children is single entry single exit or not. 
                            if (Loop[workLoop].Loop[i].nEntry == 1 && Loop[workLoop].Loop[i].nExit == 1)
                            {
                                bool checkBond_Loop = true;
                                int count_gateway = 0;
                                for (int k = 0; k < Loop[workLoop].Loop[i].nNode; k++)
                                {
                                    int node = Loop[workLoop].Loop[i].Node[k];
                                    if (Network[currentN].Node[node].nPre > 1 || Network[currentN].Node[node].nPost > 1)
                                    {
                                        count_gateway++;
                                        if (count_gateway > 2) checkBond_Loop = false;
                                    }
                                }
                                if (checkBond_Loop)
                                {
                                    for (int k = 0; k < FBLOCK.FBlock[j].nChild; k++)
                                    {
                                        int childLoop = FBLOCK.FBlock[j].child[k]; //maybe it is SESE
                                        if (FBLOCK.FBlock[childLoop].nEntry > 1 || FBLOCK.FBlock[childLoop].nExit > 1)
                                        {
                                            checkBond_Loop = false;
                                            break;
                                        }
                                    }
                                }
                                if (checkBond_Loop) informList[13]++;
                                else informList[14]++;
                            }
                            
                            reduce_Network(currentN, workLoop, i, "", true);
                        }
                    }
                }
                curDepth--;
            } while (curDepth > 0);
        }

        public void count_OR_Split_Rigids(int currentN, int workSESE, int currentSESE)
        { 
            if (checkReRun) return;
            for (int i = 0; i < SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = SESE[workSESE].SESE[currentSESE].Node[i];
                if (Network[currentN].Node[node].nPost > 0 && Network[currentN].Node[node].nPre > 0)
                {
                    if (Network[currentN].Node[node].Kind == "OR" && Network[currentN].Node[node].nPost > 1)
                    {
                        if (Network[currentN].Node[node].Name == "SS") // for SS only
                        {
                            informList[18]++; //# of OR split rigids for SS
                            informList[19] = informList[19] + Network[currentN].Node[node].nPost; //# of Edges from OR split for SS
                        }
                        else if (Network[currentN].Node[node].Name != "SS")// for non_SS only
                        {
                            informList[20]++; //# of OR split rigids for non-SS
                            informList[21] = informList[19] + Network[currentN].Node[node].nPost; //# of Edges from OR split for SS
                        }
                    }
                }
            }
        }
        public void count_Node_Bond_Rigid(int finalNet, int currentSESE, int workLoop)
        {
            int currentN = tempNet;
            Network[currentN] = Network[finalNet];
            extent_Network(currentN, 0);

            informList[16] = 0; //# of bond
            informList[17] = 0; //# of rigid
            int curDepth = FBLOCK.maxDepth;
            do
            {
                for (int j = 0; j < FBLOCK.nFBlock; j++)
                {
                    if (FBLOCK.FBlock[j].depth != curDepth) continue;

                    int i = FBLOCK.FBlock[j].refIndex;

                    if (FBLOCK.FBlock[j].SESE)
                    {
                        if (Bond_Check(currentN, currentSESE, i)) //bond models
                        {
                            for (int k = 0; k < SESE[currentSESE].SESE[i].nNode; k++)
                            {
                                if (Network[currentN].Node[SESE[currentSESE].SESE[i].Node[k]].nPost != 0 & Network[currentN].Node[SESE[currentSESE].SESE[i].Node[k]].nPre != 0)
                                {
                                    informList[16]++;
                                }
                            }
                            informList[16] = informList[16] - SESE[currentSESE].SESE[i].nChild;
                        }
                        else //rigid models
                        {
                            if (Network[currentN].Node[SESE[currentSESE].SESE[i].Entry].Kind == "OR" && Network[currentN].Node[SESE[currentSESE].SESE[i].Entry].Name == "SS") //just get the number of node in rigids which have the entry is OR split
                            {
                                informList[23]++;
                                for (int k = 0; k < SESE[currentSESE].SESE[i].nNode; k++)
                                {
                                    if (Network[currentN].Node[SESE[currentSESE].SESE[i].Node[k]].nPost != 0 & Network[currentN].Node[SESE[currentSESE].SESE[i].Node[k]].nPre != 0)
                                    {
                                        informList[17]++;
                                    }
                                }
                                informList[17] = informList[17] - SESE[currentSESE].SESE[i].nChild;
                            }
                        }
                        reduce_seseNetwork(currentN, currentSESE, i);
                    }
                    else
                    {
                        reduce_Network(currentN, workLoop, i, "", true);
                    }
                }                
                curDepth--;
            } while (curDepth > 0);
        }
        public void find_DomReI(int currentN, int k, ref int[] find_Node, ref int cntFind) //just find 1 node
        {
            cntFind = 0;
            find_Node = new int[Network[currentN].nNode];


            // Inverse - 나 자신과 나를 포함하는 노드

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                bool isCon = false;
                for (int j = 0; j < Network[currentN].Node[i].nDomRev; j++)
                {
                    if (k == Network[currentN].Node[i].DomRev[j])
                    {
                        isCon = true;
                        break;
                    }

                }

                if (isCon)
                {
                    find_Node[cntFind] = i;
                    cntFind++;
                }

            }
        }
        public void multiStart_CIPD_Count(int currentN, int finalNetwork, int currentSESE) //get % of CIPd; # of entries; # of exits;
        {
            int[] entries = new int[Network[currentN].nNode];
            int nEntries = 0;
            //find node START 
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].nPre == 0)
                {
                    entries[nEntries] = i;
                    nEntries++;
                }
            }//=> node SS = Start.Pre[0];
            if (Network[finalNetwork].Node[Network[currentN].Node[entries[0]].Post[0]].Name == "SS") //Model have multiple start event
            {
                for (int i = 0; i < Network[currentN].nLink; i++)
                {
                    if (Network[currentN].Link[i].fromNode == Network[currentN].Node[entries[0]].Post[0])
                    {
                        entries[nEntries] = Network[currentN].Link[i].toNode;
                        nEntries++;
                    }
                }
            }
            else //Model have not multiple start event
            {
                int tempNodeEn = Network[currentN].Node[entries[0]].Post[0];
                if (Network[currentN].Node[tempNodeEn].Kind == "EVENT")
                {
                    entries[nEntries] = Network[currentN].Node[entries[0]].Post[0];
                    nEntries++;
                }
            }
            if (nEntries > 1) //Prevent the case model have not Start with EVENT (Gateway or Function Instead)
            {
                #region find CIPD
                //find 
                bool checkIn = false;
                int CIPd = 0;
                int minNode = Network[finalNetwork].nNode;
                for (int i = 0; i < Network[finalNetwork].nNode; i++)
                {
                    for (int j = 1; j < nEntries; j++)
                    {
                        checkIn = false;
                        for (int k = 0; k < Network[finalNetwork].Node[i].nDomRevEI; k++)
                        {
                            if (entries[j] == Network[finalNetwork].Node[i].DomRevEI[k])
                            {
                                checkIn = true;
                                break;
                            }
                        }
                        if (checkIn == false) break;
                        else if (j == nEntries - 1)
                        {
                            if (minNode > Network[finalNetwork].Node[i].nDomRevEI - 1)
                            {
                                minNode = Network[finalNetwork].Node[i].nDomRevEI - 1;
                                CIPd = i;
                            }
                        }
                    }
                }
                informList[10] = Convert.ToDouble(Network[finalNetwork].Node[CIPd].nDomRevEI - 1); // Can not find by this way (1Pe_lzyg.net) => Must run the DFS from sos to CIPd
                informList[11] = nEntries - 1;

                //find PdFlow(SOS, SOS_se)
                int[] SOS = new int[Network[finalNetwork].nNode]; // SOS ~ Starting OR Splits
                int nSOS = 0;
                //int CIPd_restSOS = -1;
                for (int i = 0; i < Network[finalNetwork].nNode; i++) //finalNetwork ~ local variable // Find the set of SOS and SOS_se
                {
                    if (Network[finalNetwork].Node[i].Name == "SS" & Network[finalNetwork].Node[i].Kind == "OR" && Network[finalNetwork].Node[i].SOS_Corrected == false)
                    {
                        SOS[nSOS] = i;
                        nSOS++;
                    }
                }
                if (nSOS > 0)
                {
                    int Min_SOS = Network[finalNetwork].nNode;

                    int[] calDomRev = null;
                    int[] calDomRevI = null;
                    for (int k = 0; k < nSOS; k++)
                    {
                        calDomRev = find_Intersection(Network[finalNetwork].nNode, calDomRev, Network[finalNetwork].Node[SOS[k]].DomRev);
                    }
                    if (nSOS == 1) //just a trick for the single SS_OR split, because calDomRev[0] include itself inside, we need to move to another index
                    {
                        find_DomReI(finalNetwork, calDomRev[1], ref calDomRevI, ref Min_SOS);
                    }
                    else
                    {
                        find_DomReI(finalNetwork, calDomRev[0], ref calDomRevI, ref Min_SOS);
                    }


                    if (calDomRev.Length > 0)
                    {
                        bool StartNode_check = false;
                        for (int i = 0; i < Min_SOS; i++)
                        {
                            if (calDomRevI[i] == entries[0])
                            {
                                StartNode_check = true;
                            }
                        }
                        if (StartNode_check == true)
                        {
                            informList[22] = Convert.ToDouble(Min_SOS - 1); /// (Convert.ToDouble(Network[finalNetwork].nNode - 2)); ;//not count START and END
                        }
                        else
                        {
                            informList[22] = Convert.ToDouble(Min_SOS); /// (Convert.ToDouble(Network[finalNetwork].nNode - 2)); ;//not count START and END
                        }
                    }
                    else
                    {
                        informList[22] = 0;
                    }
                }
                #endregion
            }

            //find node exits;
            int[] exits = new int[Network[currentN].nNode];
            int nExits = 0;
            //find virtual node exit 
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].nPost == 0)
                {
                    exits[nExits] = i;
                    nExits++;
                }
            }//=> node EE = Start.Pre[0];
            if (Network[finalNetwork].Node[Network[currentN].Node[exits[0]].Pre[0]].Name == "EE")
            {
                for (int i = 0; i < Network[currentN].nLink; i++)
                {
                    if (Network[currentN].Link[i].toNode == Network[currentN].Node[exits[0]].Pre[0])
                    {
                        exits[nExits] = Network[currentN].Link[i].fromNode;
                        nExits++;
                    }
                }
            }
            else
            {
                int tempNodeExit = Network[currentN].Node[exits[0]].Pre[0];
                if (Network[currentN].Node[tempNodeExit].Kind == "EVENT")
                {
                    entries[nExits] = Network[currentN].Node[exits[0]].Pre[0];
                    nExits++;
                }
            }
            if (nExits > 1) //In case there are no Events as exits (Gateways or Function Instead)
            {
                informList[12] = nExits - 1;
            }
        }
        
        public void countALL_SS_OR(int currentN)
        {
            informList[15] = 0;
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].Name == "SS" && Network[currentN].Node[i].Kind == "OR")
                    informList[15]++;
            }
        }
        public void informRecording(int currentN)
        {
            //informList = new double[20]; //plus 5 to avoid error introducing by model less than 5 nodes
            //Record Type 1 split (number of split node, join node, split node)
            int countOrgI = 0;
            int countJoinI = 0;
            int countSplitI = 0;
            int[] type_I_bool = new int[Network[currentN].nNode];
            for (int i = 0; i< Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].Type_I == "" || Network[currentN].Node[i].Type_I == null) continue;
                {
                    if (i == Network[currentN].Node[i].orgNum) countOrgI++;
                    if (Network[currentN].Node[i].Type_I == "_j") countJoinI++;
                    if (Network[currentN].Node[i].Type_I == "_s") countSplitI++;
                }
            }
            informList[0] = countOrgI;
            informList[1] = countJoinI;
            informList[2] = countSplitI;

            //Record Type 2 split, Type 3 split
            int countOrgII = 0; //it should be called total of type II split node
            int countEn = 0;
            int countEx = 0;
            int countBs = 0;
            //int countOrgIII = 0;
            int countEnIII = 0;
            int countExIII = 0;
            int[] type_II_bool = new int[Network[currentN].nNode];
            string[] type_III_en = new string[Network[currentN].nNode];
            //int type_III_en_Index = 0;
            string[] type_III_ex = new string[Network[currentN].nNode];
            //int type_III_ex_Index = 0;
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].Type_II == "" || Network[currentN].Node[i].Type_II == null) continue;
                {
                    //type_II_bool[Network[currentN].Node[i].orgNum] = 1;

                    //Type-III
                    string type = Network[currentN].Node[i].Type_II;

                    if (type.Length > 2)
                    {
                        type = type.Substring(type.Length - 3, 3);
                        if (type == "_se" || type == "_sx")
                        {
                            //check_count(type_III_en, ref countEnIII)
                            //countOrgIII++; //just sum
                            string name = Network[currentN].Node[i].Name + type;
                            if (type == "_se") countEnIII++;//check_count_TypeIII(ref type_III_en, name, ref countEnIII); //count all split node
                            else countExIII++;//check_count_TypeIII(ref type_III_ex, name , ref countExIII); //count all split node
                        }
                        else type = Network[currentN].Node[i].Type_II;
                    }
                    if (i != Network[currentN].Node[i].orgNum)
                    {
                        //Type II - count entry, exit, backward
                        if (type.Length > 7)
                        {
                            type = type.Substring(type.Length - 8, 8);
                            if (type == "_en_extn")
                            {
                                countEn++;
                                countOrgII++;
                                continue;
                            }
                            else if (type == "_ex_extn")
                            {
                                countEx++;
                                countOrgII++;
                                continue;
                            }
                            else if (type == "_bs_extn")
                            {
                                countBs++;
                                countOrgII++;
                                continue;
                            }
                        }
                        if (type.Length > 2 || type.Length > 3)
                        {
                            if (type.Length > 3)
                            {
                                type = type.Substring(type.Length - 4, 4);
                                if (type == "_xfs" || type == "_xbs")
                                {
                                    countEx++;
                                    countOrgII++;
                                    continue;
                                }
                            }
                            else
                            {
                                type = type.Substring(type.Length - 3, 3);
                                if (type == "_fj" || type == "_bj")
                                {
                                    countEn++;
                                    countOrgII++;
                                    continue;
                                }
                                if (type == "_fs" || type == "_bs")
                                {
                                    countBs++;
                                    countOrgII++;
                                    continue;
                                }
                            }
                        }

                    }
                }
            }
            informList[3] = countOrgII;
            informList[4] = countEn;
            informList[5] = countEx;
            informList[6] = countBs;
            informList[7] = countEnIII + countExIII;//count the Split node only;
            informList[8] = countEnIII;
            informList[9] = countExIII;
        }

        public void check_count_TypeIII(ref string[] count_Array, string node_name, ref int index)
        {
            bool add = false;
            for (int i = 1; i <= index; i++)
            {
                if (count_Array[i] == node_name) add = true;
            }
            if ((add == false) || (index == 0))
            {
                index++;
                count_Array[index] = node_name;
            }
        }
        #endregion

        #region Make Nesting Forest of SESE and LOOP
        public void make_NestingForest(int currentN, int currentSESE, int currentLoop)
        {
            int bIndex = 0;
            //FBLOCK.FBlock = new strFBlock[SESE[currentSESE].nSESE + Loop[currentLoop].nLoop];
            if (currentSESE >= 0 && currentLoop >= 0)
            {
                FBLOCK.FBlock = new strFBlock[SESE[currentSESE].nSESE + Loop[currentLoop].nLoop];
            }
            else
            {
                if (currentSESE >=0)
                {
                    FBLOCK.FBlock = new strFBlock[SESE[currentSESE].nSESE];
                }
                else
                {
                    FBLOCK.FBlock = new strFBlock[Loop[currentLoop].nLoop];
                }
            }

            if (currentSESE >= 0 && SESE[currentSESE].nSESE > 0) //Copy SESE
            {
                //copy SESE and LOOP to FBlock;
                for (int i = 0; i < SESE[currentSESE].nSESE; i++) //copy SESE to FBlock
                {
                    //FBlock[bIndex].child = SESE[currentSESE].SESE[i].child;
                    //FBlock[bIndex].depth = SESE[currentSESE].SESE[i].depth;
                    FBLOCK.FBlock[bIndex].Entry = new int[1];
                    FBLOCK.FBlock[bIndex].Entry[0] = SESE[currentSESE].SESE[i].Entry;
                    FBLOCK.FBlock[bIndex].Exit = new int[1];
                    FBLOCK.FBlock[bIndex].Exit[0] = SESE[currentSESE].SESE[i].Exit;
                    FBLOCK.FBlock[bIndex].nEntry = 1;
                    FBLOCK.FBlock[bIndex].nExit = 1;
                    FBLOCK.FBlock[bIndex].Node = SESE[currentSESE].SESE[i].Node;
                    FBLOCK.FBlock[bIndex].nNode = SESE[currentSESE].SESE[i].nNode;
                    //FBlock[bIndex].parentBlock ??
                    FBLOCK.FBlock[bIndex].SESE = true;
                    FBLOCK.FBlock[bIndex].refIndex = i;

                    bIndex++;
                }
            }

            if (currentLoop >= 0 && Loop[currentLoop].nLoop > 0) //Copy Loop
            {
                copy_Loop(currentLoop, newTempLoop);
                //modify tempLoop => expand the node
                for (int i = 0; i < Loop[newTempLoop].nLoop; i++)
                {
                    if (Loop[newTempLoop].Loop[i].depth != 1) continue;
                    int n = 0;
                    int[] temp = GetFullNode(i, newTempLoop, currentN, ref n); //get full node of Loop (newTempLoop)
                }

                for (int i = 0; i < Loop[newTempLoop].nLoop; i++) //copy Loop to FBlock
                {
                    //FBlock[bIndex].child = ??
                    //FBlock[bIndex].depth = ??
                    FBLOCK.FBlock[bIndex].Entry = Loop[newTempLoop].Loop[i].Entry;
                    FBLOCK.FBlock[bIndex].Exit = Loop[newTempLoop].Loop[i].Exit;
                    FBLOCK.FBlock[bIndex].nEntry = Loop[newTempLoop].Loop[i].nEntry;
                    FBLOCK.FBlock[bIndex].nExit = Loop[newTempLoop].Loop[i].nExit;
                    FBLOCK.FBlock[bIndex].Node = Loop[newTempLoop].Loop[i].Node;
                    FBLOCK.FBlock[bIndex].nNode = Loop[newTempLoop].Loop[i].nNode;
                    //FBlock[bIndex].parentBlock = ??
                    FBLOCK.FBlock[bIndex].SESE = false;
                    FBLOCK.FBlock[bIndex].refIndex = i;
                    bIndex++;
                }
            }

            FBLOCK.nFBlock = bIndex;

            //remove same block
            check_Block_Same();
            //Find parent block
            for (int i = 0; i < FBLOCK.nFBlock; i++)
            {
                int j = find_nearestParentBlock(i);
                FBLOCK.FBlock[i].parentBlock = j;
            }
            //find Children
            for (int i = 0; i < FBLOCK.nFBlock; i++)
            {
                int[] child_of_i = new int[FBLOCK.nFBlock];
                int nChild = 0;
                for (int j = 0; j < FBLOCK.nFBlock; j++)
                {
                    if (FBLOCK.FBlock[j].parentBlock == i)
                    {
                        child_of_i[nChild] = j;
                        nChild++;
                    }
                }
                FBLOCK.FBlock[i].child = child_of_i;
                FBLOCK.FBlock[i].nChild = nChild;
            }
            //find Depth       
            FBLOCK.maxDepth = 0;
            //int maxDepth = 0;
            int curDepth = 0;
            for (int i = 0; i < FBLOCK.nFBlock; i++)
            {
                curDepth = 1;
                //FBLOCK.maxDepth = 1;
                if (FBLOCK.FBlock[i].parentBlock != -1) continue;
                FBLOCK.FBlock[i].depth = curDepth; 
                find_BlockDepth(i, ref curDepth);
            }
            if (FBLOCK.nFBlock > 0 && FBLOCK.maxDepth == 0) FBLOCK.maxDepth = 1;
        }

        public void find_BlockDepth(int i, ref int curDepth)
        {
            if (FBLOCK.FBlock[i].nChild > 0)
            {
                
                for (int j = 0; j < FBLOCK.FBlock[i].nChild; j++)
                {
                    curDepth++;
                    if (FBLOCK.maxDepth < curDepth) FBLOCK.maxDepth = curDepth;
                    FBLOCK.FBlock[FBLOCK.FBlock[i].child[j]].depth = curDepth;
                    find_BlockDepth(FBLOCK.FBlock[i].child[j], ref curDepth);
                    curDepth--;
                }
            }
        }

        public void check_Block_Same()
        {
            bool check = false;
            do
            {
                check = false;
                for (int i = 0; i < FBLOCK.nFBlock - 1; i++)
                {
                    for (int j = i + 1; j < FBLOCK.nFBlock; j++)
                    {
                        if ((FBLOCK.FBlock[i].nNode == FBLOCK.FBlock[j].nNode) && (check_sameBlock(FBLOCK.FBlock[i].Node, FBLOCK.FBlock[i].nNode, FBLOCK.FBlock[j].Node, FBLOCK.FBlock[j].nNode)))
                        {
                            //remove FBLOCK.FBlock[j];
                            if (FBLOCK.FBlock[j].SESE == true)
                            {
                                for (int k = j + 1; k < FBLOCK.nFBlock; k++)
                                {
                                    FBLOCK.FBlock[k - 1] = FBLOCK.FBlock[k];

                                }
                            }
                            else
                            {
                                for (int k = i + 1; k < FBLOCK.nFBlock; k++)
                                {
                                    FBLOCK.FBlock[k - 1] = FBLOCK.FBlock[k];

                                }
                            }
                            FBLOCK.nFBlock--;
                            check = true;
                            break;
                        }
                    }
                    if (check) break;
                }
            } while (check);
        }

        public int find_nearestParentBlock(int i)
        {
            int[] parentCandidates = new int[FBLOCK.nFBlock];
            int nParentCandidate = 0;
            for (int j = 0; j < FBLOCK.nFBlock; j++ )
            {
                if ((FBLOCK.FBlock[i].nNode < FBLOCK.FBlock[j].nNode)&&(check_sameBlock(FBLOCK.FBlock[i].Node, FBLOCK.FBlock[i].nNode, FBLOCK.FBlock[j].Node, FBLOCK.FBlock[j].nNode)))
                {
                    //bool check = false;
                    parentCandidates[nParentCandidate] = j;
                    nParentCandidate++;
                }
            }
            if (nParentCandidate > 0)
            {
                int min = FBLOCK.FBlock[parentCandidates[0]].nNode;
                int minIndex = parentCandidates[0];
                for (int j = 0; j < nParentCandidate; j++)
                {
                    if (min > FBLOCK.FBlock[parentCandidates[j]].nNode)
                    {
                        min = FBLOCK.FBlock[parentCandidates[j]].nNode;
                        minIndex = parentCandidates[j];
                    }
                }
                return minIndex;
            }
            else
            {
                return -1;
            }
        }
        public bool check_sameBlock(int[] A, int n, int[] B, int m) //n < m
        {
            for (int i = 0; i < n; i++ )
            {
                bool tempcheck = false;
                for (int j = 0; j < m; j++)
                {
                    if (A[i] == B[j])
                    {
                        tempcheck = true;
                    }
                }
                if (!tempcheck) return false;
            }
            return true;
        }

        public int[] GetFullNode(int i, int currentLoop, int currentN, ref int n)
        {
            int depth = Loop[currentLoop].maxDepth;       
            int loop = -1;
            //bool check = false;
            int[] tempArr = new int[Network[currentN].nNode];
            int nTempArr = 0;
            int[] tmpLoop = new int[Network[currentN].nNode];
            //1 header
            // for each node of this loop => if node is header => find again; else => add to arrNode
            for (int k = 0; k < Loop[currentLoop].Loop[i].nNode; k++)
            {
                tmpLoop = new int[Network[currentN].nNode];
                n = 0;
                if (check_header(currentLoop, i, k, ref loop))
                {
                    tmpLoop = GetFullNode(loop, currentLoop, currentN, ref n);
                    addArr(tmpLoop, ref tempArr, n, ref nTempArr);
                }
                else
                {
                    tempArr[nTempArr] = Loop[currentLoop].Loop[i].Node[k];
                    nTempArr++;
                }
            }
            tempArr[nTempArr] = Loop[currentLoop].Loop[i].header;
            nTempArr++;
            Loop[currentLoop].Loop[i].nNode = nTempArr;
            Loop[currentLoop].Loop[i].Node = tempArr;
            n = nTempArr;
            return tempArr;
        }
        public void addArr(int[] tmpLoop, ref int[] tempArr, int n, ref int nTempArr)
        {
            for (int i = 0; i < n; i++)
            {
                tempArr[nTempArr] = tmpLoop[i];
                nTempArr++;
            }
        }
        public bool check_header(int currentLoop, int i, int k, ref int loop)
        {
            for (int j = 0; j < Loop[currentLoop].Loop[i].nChild; j++ )
            {
                if (Loop[currentLoop].Loop[i].Node[k] == Loop[currentLoop].Loop[Loop[currentLoop].Loop[i].child[j]].header)
                {
                    loop = Loop[currentLoop].Loop[i].child[j];
                    return true;
                }
            }
            return false;
        }
        public bool check_in_Loop(int header, int i, int currentLoop)
        {
            for (int k = 0; k < Loop[currentLoop].Loop[i].nNode; k++)
            {
                if (header == Loop[currentLoop].Loop[i].Node[k])
                    return true;
            }
            return false;
        }

        #endregion

        #region 네트워크 작업

        //Check whether a node (not Gateway) have more than 1 sucessor or predecessor
        private void check_SyntexError()
        {
            SyntexError = false;
            for (int i = 0; i < Network[orgNet].nNode; i++)
            {
                if (Network[orgNet].Node[i].nPre > 1 || Network[orgNet].Node[i].nPost > 1)
                {
                    if (Network[orgNet].Node[i].Kind == "XOR" || Network[orgNet].Node[i].Kind == "OR" || Network[orgNet].Node[i].Kind == "AND") continue;

                    SyntexError = true;
                    break;
                }
            }

            for (int i = 0; i < Network[orgNet].nNode; i++)
            {
                if (Network[orgNet].Node[i].nPre == 1 && Network[orgNet].Node[i].nPost == 1) 
                {
                    if (Network[orgNet].Node[i].Kind == "XOR" || Network[orgNet].Node[i].Kind == "OR" || Network[orgNet].Node[i].Kind == "AND")
                    {
                        SyntexError = true;
                        break;                 
                    }
                }
            }
            /*if (SyntexError != true)
            {
                int CCs = 0;
                find_ConnectedComponents(baseNet, ref CCs);
                if (CCs > 1)
                {
                    SyntexError = true;
                }
            }*/
        }
        #region additional check syntax errors
        public void find_ConnectedComponents(int baseNet, ref int CCs, ref int[] mark)
        {
            CCs = 0;
            int[,] A = convert_AJ(baseNet);
            mark = new int[Network[baseNet].nNode]; //autofill = false;
            for (int i = 0; i < Network[baseNet].nNode; i++)
            {
                if (mark[i] == 0)
                {
                    CCs++;
                    ConnectedComponent(baseNet, i, ref mark, ref A, CCs);                  
                }
            }
        }
        public void ConnectedComponent(int baseNet, int header, ref int[] mark, ref int[,] A, int CCs)
        {
            if (mark[header] == 0)
            {
                mark[header] = CCs;
                for (int i = 0; i < Network[baseNet].nNode; i++)
                {
                    if (A[header, i] == 1)
                    {
                        ConnectedComponent(baseNet, i, ref mark, ref A, CCs);
                    }
                }
            }
        }
        public int[,] convert_AJ(int baseNet) //convert to Adjacency Matrix (non directed)
        {
            int n = Network[baseNet].nNode;
            int[,] AJ = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    AJ[i, j] = 0;
            }
            for (int i = 0; i < Network[baseNet].nLink; i++)
            {
                int frN = Network[baseNet].Link[i].fromNode;
                int tN = Network[baseNet].Link[i].toNode;
                AJ[frN, tN] = 1;
                AJ[tN, frN] = 1;

            }
            return AJ;
        }

        public int[,] convert_AJ_Directed(int baseNet) //convert to Adjacency Matrix (non directed)
        {
            int n = Network[baseNet].nNode;
            int[,] AJ = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    AJ[i, j] = 0;
            }
            for (int i = 0; i < Network[baseNet].nLink; i++)
            {
                int frN = Network[baseNet].Link[i].fromNode;
                int tN = Network[baseNet].Link[i].toNode;
                AJ[frN, tN] = 1;
            }
            return AJ;
        }
        #endregion End 

        //Find all the rest of information of a Node. Such as => Predecessors, Sucessors.. based on link
        private void find_NodeInform(int currentN, int node)
        {
            int cntPre = 0;
            int[] find_Pre = new int[Network[currentN].nNode];
            int cntPost = 0;
            int[] find_Post = new int[Network[currentN].nNode];

            Network[currentN].Node[node].nPre = 0;
            Network[currentN].Node[node].nPost = 0;
            Network[currentN].Node[node].Pre = null;
            Network[currentN].Node[node].Post = null;

            for (int j = 0; j < Network[currentN].nLink; j++)
            {
                if (Network[currentN].Link[j].fromNode == node && Network[currentN].Link[j].toNode != node)
                {
                    find_Post[cntPost] = Network[currentN].Link[j].toNode;
                    cntPost++;
                }
                if (Network[currentN].Link[j].toNode == node && Network[currentN].Link[j].fromNode != node)
                {
                    find_Pre[cntPre] = Network[currentN].Link[j].fromNode;
                    cntPre++;
                }
            }

            if (cntPre > 0)
            {
                Network[currentN].Node[node].nPre = cntPre;
                Network[currentN].Node[node].Pre = new int[cntPre];
                for (int k = 0; k < cntPre; k++) Network[currentN].Node[node].Pre[k] = find_Pre[k];
            }

            if (cntPost > 0)
            {
                Network[currentN].Node[node].nPost = cntPost;
                Network[currentN].Node[node].Post = new int[cntPost];
                for (int k = 0; k < cntPost; k++) Network[currentN].Node[node].Post[k] = find_Post[k];
            }

            if (cntPre == 0) Network[currentN].header = node;
        }

        //Add more Space for Node and Link (null in value)
        private void extent_Network(int currentN, int addNum)
        {
            //AddNum is the number of node are added
            //A full Graph (extended)
            strNetwork saveNetwork = Network[currentN];

            Network[currentN] = new strNetwork();

            Network[currentN].header = saveNetwork.header;

            Network[currentN].nNode = saveNetwork.nNode + addNum;
            Network[currentN].Node = new strNode[Network[currentN].nNode];
            for (int i = 0; i < saveNetwork.nNode; i++)
            {
                //Network[currentN].Node[i] = saveNetwork.Node[i];

                Network[currentN].Node[i].Kind = saveNetwork.Node[i].Kind;
                Network[currentN].Node[i].Name = saveNetwork.Node[i].Name;
                Network[currentN].Node[i].orgNum = saveNetwork.Node[i].orgNum;
                Network[currentN].Node[i].parentNum = saveNetwork.Node[i].parentNum;
                Network[currentN].Node[i].Type_I = saveNetwork.Node[i].Type_I;
                Network[currentN].Node[i].Type_II = saveNetwork.Node[i].Type_II;
                Network[currentN].Node[i].Special = saveNetwork.Node[i].Special;
                Network[currentN].Node[i].nodeLabel = saveNetwork.Node[i].nodeLabel;
                Network[currentN].Node[i].SOS_Corrected = saveNetwork.Node[i].SOS_Corrected;

                Network[currentN].Node[i].depth = saveNetwork.Node[i].depth;
                Network[currentN].Node[i].done = saveNetwork.Node[i].done;

                Network[currentN].Node[i].nPre = saveNetwork.Node[i].nPre;
                Network[currentN].Node[i].Pre = new int[Network[currentN].Node[i].nPre];
                for (int k = 0; k < Network[currentN].Node[i].nPre; k++)
                    Network[currentN].Node[i].Pre[k] = saveNetwork.Node[i].Pre[k];

                Network[currentN].Node[i].nPost = saveNetwork.Node[i].nPost;
                Network[currentN].Node[i].Post = new int[Network[currentN].Node[i].nPost];
                for (int k = 0; k < Network[currentN].Node[i].nPost; k++)
                    Network[currentN].Node[i].Post[k] = saveNetwork.Node[i].Post[k];

                Network[currentN].Node[i].nDom = saveNetwork.Node[i].nDom;
                Network[currentN].Node[i].Dom = new int[Network[currentN].Node[i].nDom];
                for (int k = 0; k < Network[currentN].Node[i].nDom; k++)
                    Network[currentN].Node[i].Dom[k] = saveNetwork.Node[i].Dom[k];

                Network[currentN].Node[i].nDomRev = saveNetwork.Node[i].nDomRev;
                Network[currentN].Node[i].DomRev = new int[Network[currentN].Node[i].nDomRev];
                for (int k = 0; k < Network[currentN].Node[i].nDomRev; k++)
                    Network[currentN].Node[i].DomRev[k] = saveNetwork.Node[i].DomRev[k];

                Network[currentN].Node[i].nDomEI = saveNetwork.Node[i].nDomEI;
                Network[currentN].Node[i].DomEI = new int[Network[currentN].Node[i].nDomEI];
                for (int k = 0; k < Network[currentN].Node[i].nDomEI; k++)
                    Network[currentN].Node[i].DomEI[k] = saveNetwork.Node[i].DomEI[k];

                Network[currentN].Node[i].nDomRevEI = saveNetwork.Node[i].nDomRevEI;
                Network[currentN].Node[i].DomRevEI = new int[Network[currentN].Node[i].nDomRevEI];
                for (int k = 0; k < Network[currentN].Node[i].nDomRevEI; k++)
                    Network[currentN].Node[i].DomRevEI[k] = saveNetwork.Node[i].DomRevEI[k];
            }

            Network[currentN].nLink = saveNetwork.nLink + addNum;
            Network[currentN].Link = new strLink[Network[currentN].nLink];
            for (int i = 0; i < saveNetwork.nLink; i++)
            {
                //Network[currentN].Link[i] = saveNetwork.Link[i];

                Network[currentN].Link[i].fromNode = saveNetwork.Link[i].fromNode;
                Network[currentN].Link[i].toNode = saveNetwork.Link[i].toNode;
                Network[currentN].Link[i].bBackJ = saveNetwork.Link[i].bBackJ;
                Network[currentN].Link[i].bBackS = saveNetwork.Link[i].bBackS;
                Network[currentN].Link[i].bInstance = saveNetwork.Link[i].bInstance;
            }

        }

        private void copy_FBlock(STRBLOCK fromBlock, ref STRBLOCK toBlock)
        {
            toBlock.maxDepth = fromBlock.maxDepth;
            toBlock.nFBlock = fromBlock.nFBlock;

            toBlock.FBlock = new strFBlock[toBlock.nFBlock];
            for (int i = 0; i < toBlock.nFBlock; i++)
            {
                toBlock.FBlock[i].nChild = fromBlock.FBlock[i].nChild;
                toBlock.FBlock[i].child = new int[toBlock.FBlock[i].nChild];
                for (int k = 0; k < toBlock.FBlock[i].nChild; k++)
                {
                    toBlock.FBlock[i].child[k] = fromBlock.FBlock[i].child[k];
                }

                toBlock.FBlock[i].depth = fromBlock.FBlock[i].depth;
                
                toBlock.FBlock[i].nEntry = fromBlock.FBlock[i].nEntry;
                toBlock.FBlock[i].Entry = new int[toBlock.FBlock[i].nEntry];
                for (int k = 0; k < toBlock.FBlock[i].nEntry; k++)
                {
                    toBlock.FBlock[i].Entry[k] = fromBlock.FBlock[i].Entry[k];
                }

                toBlock.FBlock[i].nExit = fromBlock.FBlock[i].nExit;
                toBlock.FBlock[i].Exit = new int[toBlock.FBlock[i].nExit];
                for (int k = 0; k < toBlock.FBlock[i].nExit; k++)
                {
                    toBlock.FBlock[i].Exit[k] = fromBlock.FBlock[i].Exit[k];
                }

                toBlock.FBlock[i].nNode = fromBlock.FBlock[i].nNode;
                toBlock.FBlock[i].Node = new int[toBlock.FBlock[i].nNode];
                for (int k = 0; k < toBlock.FBlock[i].nNode; k++)
                {
                    toBlock.FBlock[i].Node[k] = fromBlock.FBlock[i].Node[k];
                }

                toBlock.FBlock[i].parentBlock = fromBlock.FBlock[i].parentBlock;
                toBlock.FBlock[i].refIndex = fromBlock.FBlock[i].refIndex;
                toBlock.FBlock[i].SESE = fromBlock.FBlock[i].SESE;
            }
        }

        private void copy_Loop(int fromLoop, int toLoop)
        {
            Loop[toLoop] = new strLoop();

            Loop[toLoop].maxDepth = Loop[fromLoop].maxDepth;

            Loop[toLoop].nLoop = Loop[fromLoop].nLoop;
            Loop[toLoop].Loop = new strLoopInform[Loop[toLoop].nLoop];
            for (int i = 0; i < Loop[toLoop].nLoop; i++)
            {
                Loop[toLoop].Loop[i].Irreducible = Loop[fromLoop].Loop[i].Irreducible;
                Loop[toLoop].Loop[i].depth = Loop[fromLoop].Loop[i].depth;

                Loop[toLoop].Loop[i].parentLoop = Loop[fromLoop].Loop[i].parentLoop;
                Loop[toLoop].Loop[i].nChild = Loop[fromLoop].Loop[i].nChild;
                Loop[toLoop].Loop[i].child = new int[Loop[toLoop].Loop[i].nChild];
                for (int k = 0; k < Loop[toLoop].Loop[i].nChild; k++)
                    Loop[toLoop].Loop[i].child[k] = Loop[fromLoop].Loop[i].child[k];

                Loop[toLoop].Loop[i].header = Loop[fromLoop].Loop[i].header;

                Loop[toLoop].Loop[i].nBackEdge = Loop[fromLoop].Loop[i].nBackEdge;
                Loop[toLoop].Loop[i].linkBack = new int[Loop[toLoop].Loop[i].nBackEdge];
                for (int k = 0; k < Loop[toLoop].Loop[i].nBackEdge; k++)
                    Loop[toLoop].Loop[i].linkBack[k] = Loop[fromLoop].Loop[i].linkBack[k];

                Loop[toLoop].Loop[i].nBackSplit = Loop[fromLoop].Loop[i].nBackSplit;
                Loop[toLoop].Loop[i].BackSplit = new int[Loop[toLoop].Loop[i].nBackSplit];
                for (int k = 0; k < Loop[toLoop].Loop[i].nBackSplit; k++)
                    Loop[toLoop].Loop[i].BackSplit[k] = Loop[fromLoop].Loop[i].BackSplit[k];

                Loop[toLoop].Loop[i].nEntry = Loop[fromLoop].Loop[i].nEntry;
                Loop[toLoop].Loop[i].Entry = new int[Loop[toLoop].Loop[i].nEntry];
                for (int k = 0; k < Loop[toLoop].Loop[i].nEntry; k++)
                    Loop[toLoop].Loop[i].Entry[k] = Loop[fromLoop].Loop[i].Entry[k];

                Loop[toLoop].Loop[i].nExit = Loop[fromLoop].Loop[i].nExit;
                Loop[toLoop].Loop[i].Exit = new int[Loop[toLoop].Loop[i].nExit];
                for (int k = 0; k < Loop[toLoop].Loop[i].nExit; k++)
                    Loop[toLoop].Loop[i].Exit[k] = Loop[fromLoop].Loop[i].Exit[k];

                Loop[toLoop].Loop[i].nNode = Loop[fromLoop].Loop[i].nNode;
                Loop[toLoop].Loop[i].Node = new int[Loop[toLoop].Loop[i].nNode];
                for (int k = 0; k < Loop[toLoop].Loop[i].nNode; k++)
                    Loop[toLoop].Loop[i].Node[k] = Loop[fromLoop].Loop[i].Node[k];

                Loop[toLoop].Loop[i].nConcurrency = Loop[fromLoop].Loop[i].nConcurrency;
                if (Loop[fromLoop].Loop[i].Concurrency != null)
                {
                    Loop[toLoop].Loop[i].Concurrency = new int[Loop[toLoop].Loop[i].nEntry];
                    for (int k = 0; k < Loop[toLoop].Loop[i].nEntry; k++)
                        Loop[toLoop].Loop[i].Concurrency[k] = Loop[fromLoop].Loop[i].Concurrency[k];
                }
            }

        }

        private void copy_SESE(int fromSESE, int toSESE)
        {
            SESE[toSESE] = new strSESE();

            SESE[toSESE].maxDepth = SESE[fromSESE].maxDepth;

            SESE[toSESE].nSESE = SESE[fromSESE].nSESE;
            SESE[toSESE].SESE = new strSESEInform[SESE[toSESE].nSESE];
            for (int i = 0; i < SESE[toSESE].nSESE; i++)
            {
                SESE[toSESE].SESE[i].depth = SESE[fromSESE].SESE[i].depth;

                SESE[toSESE].SESE[i].parentSESE = SESE[fromSESE].SESE[i].parentSESE;
                SESE[toSESE].SESE[i].nChild = SESE[fromSESE].SESE[i].nChild;
                SESE[toSESE].SESE[i].child = new int[SESE[toSESE].SESE[i].nChild];
                for (int k = 0; k < SESE[toSESE].SESE[i].nChild; k++)
                    SESE[toSESE].SESE[i].child[k] = SESE[fromSESE].SESE[i].child[k];

                SESE[toSESE].SESE[i].Entry = SESE[fromSESE].SESE[i].Entry;
                SESE[toSESE].SESE[i].Exit = SESE[fromSESE].SESE[i].Exit;

                SESE[toSESE].SESE[i].nNode = SESE[fromSESE].SESE[i].nNode;
                SESE[toSESE].SESE[i].Node = new int[SESE[toSESE].SESE[i].nNode];
                for (int k = 0; k < SESE[toSESE].SESE[i].nNode; k++)
                    SESE[toSESE].SESE[i].Node[k] = SESE[fromSESE].SESE[i].Node[k];

            }

        }

        private void resize_Network(int currentN, int nNode, int nLink)
        {
            Network[currentN].nNode = nNode;
            strNode[] newNode = new strNode[nNode];
            for (int i = 0; i < nNode; i++)
            {
                newNode[i] = Network[currentN].Node[i];
            }

            Network[currentN].nLink = nLink;
            strLink[] newLink = new strLink[nLink];
            for (int i = 0; i < nLink; i++)
            {
                newLink[i] = Network[currentN].Link[i];
            }

            Network[currentN].Node = newNode;
            Network[currentN].Link = newLink;

        }

        //find Dominance Tree =========================================================================================================================
        private void find_Dom(int currentN)
        {
            //초기 Dom
            for (int i = 0; i < Network[currentN].nNode; i++) //Visit all node in graph (index range from 0 to nNode)
            {
                if (Network[currentN].Node[i].nPre == 0) //시작점 //Starting Point
                {
                    Network[currentN].Node[i].nDom = 1;
                    Network[currentN].Node[i].Dom = new int[1];
                    Network[currentN].Node[i].Dom[0] = i; //If it is the first node => there is only its node dominate itself
                }
                else
                {
                    Network[currentN].Node[i].nDom = 0;
                    Network[currentN].Node[i].Dom = null; //initiating all Dom[] array by null and 0 for further evaluation
                }
            }

            bool change;
            do
            {
                change = false;
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    // i from 0 to Total number of Node
                    int[] calDom = null;
                    for (int k = 0; k < Network[currentN].Node[i].nPre; k++)
                    {
                        //k from 0 to Total number of "nPre"
                        //find the intersection of (calDom[] and B[]) => return calDom[]
                        calDom = find_Intersection(Network[currentN].nNode, calDom, Network[currentN].Node[Network[currentN].Node[i].Pre[k]].Dom); // calDOm = caDom (intersection) Dom(j)
                    }

                    if (calDom != null)
                    {
                        int[] newDom = new int[calDom.Length + 1];

                        for (int k = 0; k < calDom.Length; k++) newDom[k] = calDom[k]; //교집합
                        newDom[calDom.Length] = i; //자기자신 // Node i dominated its self
                        //Check same and transfer the result
                        if (!check_Same(newDom, Network[currentN].Node[i].Dom))
                        {
                            Network[currentN].Node[i].Dom = newDom;

                            change = true;
                        }

                        Network[currentN].Node[i].nDom = Network[currentN].Node[i].Dom.Length;
                    }
                }

            } while (change);
            //
        }
        //Post Dominance
        private void find_DomRev(int currentN)
        {
            //초기 DomRev
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].nPost == 0) //끝점
                {
                    Network[currentN].Node[i].nDomRev = 1;
                    Network[currentN].Node[i].DomRev = new int[1];
                    Network[currentN].Node[i].DomRev[0] = i;
                }
                else
                {
                    Network[currentN].Node[i].nDomRev = 0;
                    Network[currentN].Node[i].DomRev = null;
                }
            }



            bool change;
            do
            {
                change = false;
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    int[] calDom = null;
                    for (int k = 0; k < Network[currentN].Node[i].nPost; k++)
                    {
                        calDom = find_Intersection(Network[currentN].nNode, calDom, Network[currentN].Node[Network[currentN].Node[i].Post[k]].DomRev);
                    }

                    if (calDom != null)
                    {
                        int[] newDom = new int[calDom.Length + 1];

                        newDom[0] = i; //자기자신
                        for (int k = 0; k < calDom.Length; k++) newDom[k + 1] = calDom[k]; //교집합


                        if (!check_Same(newDom, Network[currentN].Node[i].DomRev))
                        {
                            Network[currentN].Node[i].DomRev = newDom;

                            change = true;
                        }

                        Network[currentN].Node[i].nDomRev = Network[currentN].Node[i].DomRev.Length;
                    }
                }

            } while (change);

        }
        // Extended Inverse Dominance eDom^-1
        private void find_DomEI(int currentN, int nSS) // nSS = -1 이면 split node만.....아니면 nSS만
        {
            for (int k = 0; k < Network[currentN].nNode; k++)
            {
                //if (nSS != -1 && nSS != k) continue; ????

                int cntFind = 0;
                int[] find_Node = new int[Network[currentN].nNode]; //store Dom^-1 array find_Node[] was created, which length was nNode

                int[] calDom = null; //store the eDom^-1


                // Find Inverse Dominanators of a split Node K in here. (Dom^-1 (k))
                for (int i = 0; i < Network[currentN].nNode; i++)
                {

                    bool isCon = false;
                    for (int j = 0; j < Network[currentN].Node[i].nDom; j++)
                    {
                        if (k == Network[currentN].Node[i].Dom[j]) //if node K existing in a set of node in Dom(i), then i is dominated by K
                        {
                            isCon = true;
                            break; // node inside Dom(i) have only 1 node = K, so break is unquestionable.
                        }

                    }

                    if (isCon)
                    {
                        find_Node[cntFind] = i; //find_Node used for saving all node which Node K dominate its.
                        cntFind++;
                    }

                }

                // extension => 
                int numCandidate = 0;
                int candidateNode = 0;
                int[] df = new int[Network[currentN].nNode];
                int ndf = 0;
                // Search for all of entire set of Node in flow graph
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    //Filter a node which have at least 2 Predecessors
                    if (Network[currentN].Node[i].nPre < 2) continue;  // Join 아니면
                    //Find dominance Frontier (DF)
                    bool isCon = false;
                    //search this node (have at least 2 predecessors), if this node (i) existing in Inverse Dominance Find_node => this is Dominance Frontier (DF)
                    for (int find = 0; find < cntFind; find++)
                    {
                        //If node i belong to Inverse Dominance (k) - eDom^-1 => mark and skip this step
                        if (i == find_Node[find])
                        {
                            isCon = true;
                            break;
                        }
                    }

                    if (isCon) continue; //we do again the for loop, skip all command below.
                    // We already a Node i which have at least 2 predecessors. (Toi day=> loc ra nhung nut i co 2 cha tro len va nut i ko nam trong Inverse Dom(k)
                    int numFrom = 0;
                    for (int pre = 0; pre < Network[currentN].Node[i].nPre; pre++) //Luot qua tat ca cac cha cua nut I
                    {
                        for (int find = 0; find < cntFind; find++)
                        {
                            if (Network[currentN].Node[i].Pre[pre] == find_Node[find]) numFrom++; //??
                        }
                    }

                    if (numFrom > 1)
                    {
                        candidateNode = i;
                        numCandidate++;
                        //Need add this node to eDom^-1 => (find_Node)
                        find_Node[cntFind] = candidateNode; //Done explanation //ADD LAm` gi vay chu'???????
                        cntFind++;
                        df[ndf] = candidateNode;
                        ndf++;

                    }

                }

                calDom = new int[cntFind];
                for (int j = 0; j < cntFind; j++) calDom[j] = find_Node[j];

                Network[currentN].Node[k].DF = df;
                Network[currentN].Node[k].nDF = ndf;

                Network[currentN].Node[k].nDomEI = cntFind;
                Network[currentN].Node[k].DomEI = calDom;
            }
        }

        //Extended Inverse Post Dominance ePdom^-1
        private void find_DomRevEI(int currentN) // join node만.....
        {
            for (int k = 0; k < Network[currentN].nNode; k++)
            {
                int cntFind = 0;
                int[] find_Node = new int[Network[currentN].nNode];

                int[] calDom = null;

                // Inverse - 나 자신과 나를 포함하는 노드

                for (int i = 0; i < Network[currentN].nNode; i++)
                {

                    bool isCon = false;
                    for (int j = 0; j < Network[currentN].Node[i].nDomRev; j++)
                    {
                        if (k == Network[currentN].Node[i].DomRev[j])
                        {
                            isCon = true;
                            break;
                        }

                    }

                    if (isCon)
                    {
                        find_Node[cntFind] = i;
                        cntFind++;
                    }

                }

                // extension
                int numCandidate = 0;
                int candidateNode = 0;
                int[] pdf = new int[Network[currentN].nNode];
                int npdf = 0;
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    if (Network[currentN].Node[i].nPost < 2) continue;  // Split 아니면

                    bool isCon = false;
                    for (int find = 0; find < cntFind; find++)
                    {
                        if (i == find_Node[find])
                        {
                            isCon = true;
                            break;
                        }
                    }

                    if (isCon) continue;

                    int numFrom = 0;
                    for (int post = 0; post < Network[currentN].Node[i].nPost; post++)
                    {
                        for (int find = 0; find < cntFind; find++)
                        {
                            if (Network[currentN].Node[i].Post[post] == find_Node[find]) numFrom++;
                        }
                    }

                    if (numFrom > 1)
                    {
                        candidateNode = i;
                        numCandidate++;
                        //Need add this node to eDom^-1 => (find_Node)
                        find_Node[cntFind] = candidateNode; //Done explanation
                        cntFind++;

                        pdf[npdf] = candidateNode;
                        npdf++;
                    }

                }

                calDom = new int[cntFind];
                for (int j = 0; j < cntFind; j++) calDom[j] = find_Node[j];

                Network[currentN].Node[k].PdF = pdf;
                Network[currentN].Node[k].nPdF = npdf;

                Network[currentN].Node[k].nDomRevEI = cntFind;
                Network[currentN].Node[k].DomRevEI = calDom;

            }
        }

        private void find_DomInverse(int currentN)
        {
            for (int k = 0; k < Network[currentN].nNode; k++)
            {
                //if (nSS != -1 && nSS != k) continue; ????

                int cntFind = 0;
                int[] find_Node = new int[Network[currentN].nNode]; //store Dom^-1 array find_Node[] was created, which length was nNode

                int[] calDom = null; //store the eDom^-1
                //if (Network[currentN].Node[k].nPost > 1)  // Only split nodes are considered as a candidate entry of an SESE and header
                //if ((Network[currentN].Node[k].nPost > 1 && isLoopExit(k, orgLoop)) || isHeader(k, orgLoop) == true)
                {

                    // Find Inverse Dominanators of a split Node K in here. (Dom^-1 (k))
                    for (int i = 0; i < Network[currentN].nNode; i++)
                    {

                        bool isCon = false;
                        for (int j = 0; j < Network[currentN].Node[i].nDom; j++)
                        {
                            if (k == Network[currentN].Node[i].Dom[j]) //if node K existing in a set of node in Dom(i), then i is dominated by K
                            {
                                isCon = true;
                                break; // node inside Dom(i) have only 1 node = K, so break is unquestionable.
                            }

                        }

                        if (isCon)
                        {
                            find_Node[cntFind] = i; //find_Node used for saving all node which Node K dominate its.
                            cntFind++;
                        }

                    }
                    calDom = new int[cntFind];
                    for (int j = 0; j < cntFind; j++) calDom[j] = find_Node[j];

                }

                Network[currentN].Node[k].nDomInverse = cntFind;
                Network[currentN].Node[k].DomInverse = calDom;

            }
        }
        private void find_DomRevInverse(int currentN)
        {
            for (int k = 0; k < Network[currentN].nNode; k++)
            {
                int cntFind = 0;
                int[] find_Node = new int[Network[currentN].nNode];

                int[] calDom = null;
                //if (Network[currentN].Node[k].nPre > 1)  // join node만.....
                //if ((Network[currentN].Node[k].nPre > 1 && isHeader(k,orgLoop) == false) || isLoopExit(k, orgLoop))
                {

                    // Inverse - 나 자신과 나를 포함하는 노드

                    for (int i = 0; i < Network[currentN].nNode; i++)
                    {

                        bool isCon = false;
                        for (int j = 0; j < Network[currentN].Node[i].nDomRev; j++)
                        {
                            if (k == Network[currentN].Node[i].DomRev[j])
                            {
                                isCon = true;
                                break;
                            }

                        }

                        if (isCon)
                        {
                            find_Node[cntFind] = i;
                            cntFind++;
                        }

                    }
                    calDom = new int[cntFind];
                    for (int j = 0; j < cntFind; j++) calDom[j] = find_Node[j];

                }

                Network[currentN].Node[k].nDomRevInverse = cntFind;
                Network[currentN].Node[k].DomRevInverse = calDom;

            }
        }
        //========= End of finding Dom, PostDom (DomRev), eDom(), ePdom()
        //It return a set of node after intersection A[], B[]
        private int[] find_Intersection(int totNum, int[] A, int[] B)
        {
            int cntFind = 0;
            int[] find_Node = new int[totNum]; //totNum = Total number of node

            int[] retSet;

            if (A == null && B == null)
            {
                retSet = null;
            }
            else if (A == null)
            {
                retSet = B; //Must be null??? //Because all of set of node Predecessor of node i are not null => so we can use this commands
            }
            else if (B == null)
            {
                retSet = A; //Must be null???
            }
            else
            {
                for (int i = 0; i < A.Length; i++)
                {
                    for (int j = 0; j < B.Length; j++)
                    {
                        if (A[i] == B[j])
                        {
                            find_Node[cntFind] = A[i];
                            cntFind++;
                            break;
                        }
                    }
                }
                //if (cntFind == 0) retSet = null;
                //else
                //{
                    retSet = new int[cntFind];
                    for (int k = 0; k < cntFind; k++) retSet[k] = find_Node[k];
                //}

            }
            return retSet;
        }

        private bool check_Same(int[] A, int[] B)
        {
            bool same = false;

            if (A == null && B == null)
            {
                same = true;
            }
            else if (A == null)
            {
                same = false;
            }
            else if (B == null)
            {
                same = false;
            }
            else
            {
                if (A.Length != B.Length)
                {
                    same = false;
                }
                else
                {
                    same = true;
                    for (int i = 0; i < A.Length; i++)
                    {
                        bool isIn = false;
                        for (int j = 0; j < B.Length; j++)
                        {
                            if (A[i] == B[j])
                            {
                                isIn = true;
                                break;
                            }
                        }

                        if (!isIn)
                        {
                            same = false;
                            break;
                        }
                    }
                }


            }

            return same;
        }

        private void find_Reach(int currentN, int workLoop, int loop, int sNode, int toNode, string Type) //check can reach from "sNode" to "toNode" //Type = "CC" or ...
        {
            if (sNode == toNode) return; //Start node = end path Node

            for (int i = 0; i < Network[currentN].Node[sNode].nPost; i++) //Visit all potential path from sNode (Start)
            {
                /////////////////////////////////
                bool bEnd = false;

                if (Type == "CC") //sNode for this case is Header (searchNode[0])
                {
                    for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //Check nut lien ke tu sNode (header) toi entries dc hay ko (Duong di truc tiep)
                    {
                        if (Network[currentN].Node[sNode].Post[i] == Loop[workLoop].Loop[loop].Entry[k])
                        {
                            bEnd = true;
                            break;
                        }
                    }
                }

                if (bEnd) continue;
                ////////////////////////////////////////

                int fromNode = Network[currentN].Node[sNode].Post[i]; //begining with a single path

                nReachNode = 0;
                reachNode = new int[Network[currentN].nNode]; //initiate reachNode by a array with scale of nNode (it will store the node, in "sequentially" - just guess)
                reachNode[nReachNode] = fromNode; //Begining with the sucessor of start (sNode)
                nReachNode++;

                if (can_Reach(currentN, workLoop, loop, fromNode, toNode, Type))
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == fromNode)
                        {
                            bSame = true;
                            break;
                        }
                    }

                    if (!bSame)
                    {
                        searchNode[nSearchNode] = fromNode;
                        nSearchNode++;
                    }

                    find_Reach(currentN, workLoop, loop, fromNode, toNode, Type);
                }

            }
        }

        //New one for the case there are loops in the DFlow ============================================
        private void find_Reach_2(int currentN, int workLoop, int loop, int sNode, int toNode, string Type) //it OK -> just change the Can_Reach_2() function
        {
            if (sNode == toNode) return; //Start node = end path Node

            for (int i = 0; i < Network[currentN].Node[sNode].nPost; i++) //Visit all potential path from sNode (Start)
            {
                /////////////////////////////////
                bool bEnd = false;

                if (Type == "CC") //sNode for this case is Header (searchNode[0])
                {
                    for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //Check nut lien ke tu sNode (header) toi entries dc hay ko (Duong di truc tiep)
                    {
                        if (Network[currentN].Node[sNode].Post[i] == Loop[workLoop].Loop[loop].Entry[k])
                        {
                            bEnd = true;
                            break;
                        }
                    }
                }

                if (bEnd) continue;
                ////////////////////////////////////////

                int fromNode = Network[currentN].Node[sNode].Post[i]; //begining with a single path

                nReachNode = 0;
                reachNode = new int[Network[currentN].nNode]; //initiate reachNode by a array with scale of nNode (it will store the node, in "sequentially" - just guess)
                reachNode[nReachNode] = fromNode; //Begining with the sucessor of start (sNode)
                nReachNode++;
                bool[] mark_reach = new bool[Network[finalNet].nNode];

                if (can_Reach_2(currentN, workLoop, loop, fromNode, toNode, Type, ref mark_reach))
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == fromNode)
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (!bSame)
                    {
                        searchNode[nSearchNode] = fromNode;
                        nSearchNode++;
                    }
                    find_Reach_2(currentN, workLoop, loop, fromNode, toNode, Type);
                }

            }
        }
        private bool can_Reach_2(int currentN, int workLoop, int loop, int fromNode, int toNode, string Type, ref bool[] mark_reach)
        {
            bool bReach = false;
            if (Network[currentN].Node[fromNode].nPost == 0) return bReach;
            mark_reach[fromNode] = true;

            for (int i = 0; i < Network[currentN].Node[fromNode].nPost; i++)
            {
                bool bSame = false;
                for (int j = 0; j < nReachNode; j++)
                {
                    if (Network[currentN].Node[fromNode].Post[i] == reachNode[j])
                    {
                        bSame = true;
                        break;
                    }
                }
                if (bSame) continue;

                reachNode[nReachNode] = Network[currentN].Node[fromNode].Post[i];
                nReachNode++;

                if (Network[currentN].Node[fromNode].Post[i] == toNode)
                {
                    bReach = true;
                    //break;
                }
                else if (Type == "CC")
                {
                    if (!Node_In_Loop(workLoop, Network[currentN].Node[fromNode].Post[i], loop))  //???????????
                    {
                        if (mark_reach[Network[currentN].Node[fromNode].Post[i]]) continue;
                        if (can_Reach_2(currentN, workLoop, loop, Network[currentN].Node[fromNode].Post[i], toNode, Type, ref mark_reach))
                        {
                            bReach = true;
                            //break;
                        }
                    }
                }
                else //we don't need it =>
                {
                    if (can_Reach_2(currentN, workLoop, loop, Network[currentN].Node[fromNode].Post[i], toNode, Type, ref mark_reach))
                    {
                        bReach = true;
                        //break;
                    }
                }
                nReachNode--; ;
            }
            mark_reach[fromNode] = false;

            return bReach;
        }
        //================================================================================================================================

        private bool can_Reach(int currentN, int workLoop, int loop, int fromNode, int toNode, string Type)
        {
            bool bReach = false;
            if (Network[currentN].Node[fromNode].nPost == 0) return bReach;

            for (int i = 0; i < Network[currentN].Node[fromNode].nPost; i++)
            {
                bool bSame = false;
                for (int j = 0; j < nReachNode; j++)
                {
                    if (Network[currentN].Node[fromNode].Post[i] == reachNode[j])
                    {
                        bSame = true;
                        break;
                    }
                }
                if (bSame) continue;

                reachNode[nReachNode] = Network[currentN].Node[fromNode].Post[i];
                nReachNode++;

                if (Network[currentN].Node[fromNode].Post[i] == toNode)
                {
                    bReach = true;
                    break;
                }
                else if (Type == "CC")
                {
                    if (!Node_In_Loop(workLoop, Network[currentN].Node[fromNode].Post[i], loop))  //???????????
                    {
                        if (can_Reach(currentN, workLoop, loop, Network[currentN].Node[fromNode].Post[i], toNode, Type))
                        {
                            bReach = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (can_Reach(currentN, workLoop, loop, Network[currentN].Node[fromNode].Post[i], toNode, Type))
                    {
                        bReach = true;
                        break;
                    }
                }
            }

            return bReach;
        }

        #endregion

        

        #region SESE 

        public void preProcessingSESE(int currentN, int currentSESE, int nodeSS)
        {
            #region find Dom, Pdom...
            find_Dom(currentN); //Dom
            find_DomRev(currentN); //Postdom
            

            //find_DomInverse(currentN); //bonus 
            //find_DomRevInverse(currentN); //bonus
            #endregion

            #region preprocessing data for SESE identify
            /////////////////////////////////////////////////////////////////
            SESE[currentSESE] = new strSESE();
            SESE[currentSESE].nSESE = 0;

            //Convert to adjacency list
            adjList_Create(currentN, ref adjList);

            transfer_AdjacencyMatrix(currentN, ref AjM_Network, ref Network[currentN].nNode);
            //adj_matrix_checkEdges = new int[Network[currentN].nNode, Network[currentN].nNode];
            LinkSESE = new strLink[Network[currentN].nLink];

            //==Make DomTree for identifying the candidate exit //Copy from original code (MAKE DOOM/POSTDOM TREE)
            int nNode = Network[currentN].nNode;
            makeLinkDomTree = new bool[nNode, nNode];
            int nLink = 0;
            for (int i = 0; i < nNode; i++)
            {
                for (int k = 1; k < Network[currentN].Node[i].nDom; k++)
                {
                    makeLinkDomTree[Network[currentN].Node[i].Dom[k - 1], Network[currentN].Node[i].Dom[k]] = true;
                    nLink++;
                }
            }
            // Make Postdom Tree
            makeLinkPdomTree = new bool[nNode, nNode];
            int nLink2 = 0;
            for (int i = 0; i < nNode; i++)
            {
                for (int k = 1; k < Network[currentN].Node[i].nDomRev; k++)
                {
                    makeLinkPdomTree[Network[currentN].Node[i].DomRev[k - 1], Network[currentN].Node[i].DomRev[k]] = true;
                    nLink2++;
                }
            }

            if (nodeSS != -2)
            {
                TestingExe(currentN);
            }
            /*if (nodeSS != -2)
            {
                // Make eDom Tree
                makeLink_eDomTree = new bool[nNode, nNode];
                int nLink3 = 0;
                for (int i = 0; i < nNode; i++)
                {
                    for (int k = 1; k < Network[currentN].Node[i].nDomEI; k++)
                    {
                        makeLink_eDomTree[Network[currentN].Node[i].DomEI[k - 1], Network[currentN].Node[i].DomEI[k]] = true;
                        nLink3++;
                    }
                }

                // Make ePdom Tree
                makeLink_ePdomTree = new bool[nNode, nNode];
                int nLink4 = 0;
                for (int i = 0; i < nNode; i++)
                {
                    for (int k = 1; k < Network[currentN].Node[i].nDomRevEI; k++)
                    {
                        makeLink_ePdomTree[Network[currentN].Node[i].DomRevEI[k - 1], Network[currentN].Node[i].DomRevEI[k]] = true;
                        nLink4++;
                    }
                }
            }*/

            #endregion

            #region reduce Loop in model (for easier to identify which node is in or out the loop) => store in [reduceTempNet]

            Network[reduceTempNet] = Network[finalNet];
            extent_Network(reduceTempNet, 0);

            //copy_Loop(orgLoop, reduceLoop);
            int curDepth = Loop[orgLoop].maxDepth;
            do
            {
                for (int i = 0; i < Loop[orgLoop].nLoop; i++)
                {
                    if (Loop[orgLoop].Loop[i].depth != curDepth) continue;
                    //reduceLoop => store in reduceNetwork
                    reduce_Network(reduceTempNet, orgLoop, i, "", false);
                }
                curDepth--;
            } while (curDepth > 0);
            #endregion

            maxDepth_DomTree = 0;
            maxDepth_PdomTree = 0;
            //BFS and get the depth of EntrySESE or ExitSESE
            if (nodeSS == -2)
            {
                maxDepth_DomTree = BFS(currentN, makeLinkDomTree, true);
                maxDepth_PdomTree = BFS(currentN, makeLinkPdomTree, false);
            }

            
            
            //==============================

        }

        //Check whether a node is header or not
        private bool isHeader(int curNode, int workLoop) //should use orgLoop
        {
            int numLoop = Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++) 
            {
                if (Loop[workLoop].Loop[i].nEntry == 1)
                {
                    if (Loop[workLoop].Loop[i].header == curNode)
                        return true;
                }
            }
            return false;
        }
        private bool isLoopEntries(int workLoop, int curNode)
        {
            int numLoop = Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                for (int j = 0; j < Loop[workLoop].Loop[i].nEntry; j++ )
                {
                    if (Loop[workLoop].Loop[i].Entry[j] == curNode)
                        return true;
                }
            }
            return false;
        }
        private bool isLoopExits(int workLoop, int curNode)
        {
            int numLoop = Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                for (int j = 0; j < Loop[workLoop].Loop[i].nExit; j++)
                {
                    if (Loop[workLoop].Loop[i].Exit[j] == curNode)
                        return true;
                }
            }
            return false;
        }


        private bool isLoopSingleExit(int curNode, int workLoop) //it mean this curNode must be single loop exit
        {
            int numLoop = Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                if (Loop[workLoop].Loop[i].nExit == 1)
                {
                    if (Loop[workLoop].Loop[i].Exit[0] == curNode)
                        return true;
                }
            }
            return false;
        }

        private bool isLoopExit(int curNode, int workLoop) //it mean this curNode must be single loop exit
        {
            int numLoop = Loop[workLoop].nLoop;
            int numExit;
            for (int i = 0; i < numLoop; i++)
            {
                numExit = Loop[workLoop].Loop[i].nExit;
                for (int j = 0; j < numExit; j++)
                {
                    if (Loop[workLoop].Loop[i].Exit[j] == curNode)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //find CandEnt and CandExit only inside loop (except loop construct)
        public void get_arrange_CandEnt_CandExt(int currentN, int reduceTempNet, int workLoop, int currentLoop, ref int[] entrySESE, ref int nEntrySESE, ref int[] exitSESE, ref int nExitSESE, int maxDepth_DomTree, int maxDepth_PdomTree)
        {
            //get all node inside this layer (loop nesting forest) ==> for Cyclic model //ONLY SPLIT AND JOIN?
            if (currentLoop >= 0) {
                for (int i = 0; i < Loop[workLoop].Loop[currentLoop].nNode; i++)
                {
                    int node = Loop[workLoop].Loop[currentLoop].Node[i];
                    {
                        //if (!isInLoopConstruct(workLoop, currentLoop, node))                    
                        if (Network[currentN].Node[node].nPost > 1)
                        {
                            entrySESE[nEntrySESE] = node;
                            nEntrySESE++;
                        }
                        if (Network[currentN].Node[node].nPre > 1)
                        {
                            exitSESE[nExitSESE] = node;
                            nExitSESE++;
                        }
                    }
                }
                exitSESE[nExitSESE] = Loop[workLoop].Loop[currentLoop].header;
                nExitSESE++;
                //more added to deal with type-2 SESE
                for (int i = 0; i < Loop[workLoop].Loop[currentLoop].nEntry; i++)
                {
                    int node = Loop[workLoop].Loop[currentLoop].Entry[i];
                    entrySESE[nEntrySESE] = node;
                    nEntrySESE++;
                }
                for (int i = 0; i < Loop[workLoop].Loop[currentLoop].nExit; i++)
                {
                    int node = Loop[workLoop].Loop[currentLoop].Exit[i];
                    exitSESE[nExitSESE] = node;
                    nExitSESE++;
                }
            }

            if (currentLoop == -1) //for Acyclic models ONLY (because we use find_SESE 3 times?)
            {
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    //if (isLoopHeader(workLoop, i))
                    if (Network[currentN].Node[i].nPost > 1)
                    {
                        entrySESE[nEntrySESE] = i;
                        nEntrySESE++;
                    }
                    if (Network[currentN].Node[i].nPre > 1)
                    {
                        exitSESE[nExitSESE] = i;
                        nExitSESE++;
                    }
                }
            }

            //============= New test -- Use all gateway for candidate entrySESE and candidate exitSESE
            if (currentLoop == -5) //for Testing Acyclic and Cyclic model purpose
            {
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    if (Network[currentN].Node[i].Kind == "START" || Network[currentN].Node[i].Kind == "END") continue;
                    if (Network[currentN].Node[i].nPre == 1 && Network[currentN].Node[i].nPost == 1) continue;

                    entrySESE[nEntrySESE] = i;
                    nEntrySESE++;

                    exitSESE[nExitSESE] = i;
                    nExitSESE++;
                }
            }
            //===========================================================================================

            if (currentLoop == -3) // mean nodes outside all loops (of a cyclic model)
            {
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    if (Network[reduceTempNet].Node[i].nPost == 0 && Network[reduceTempNet].Node[i].nPre == 0 || isLoopHeader(workLoop, i)) continue;
                    if (Network[currentN].Node[i].nPost > 1)
                    {
                        entrySESE[nEntrySESE] = i;
                        nEntrySESE++;
                    }
                    if (Network[currentN].Node[i].nPre > 1)
                    {
                        exitSESE[nExitSESE] = i;
                        nExitSESE++;
                    }
                }
            }

            //add more node from the lower hyerarchy of loop forest
            if (currentLoop >= 0 || currentLoop == -3)
            {
                add_more_CandEn_CandEx(workLoop, currentLoop, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE);
            }

            //re-ordering CandEnt and CandExit
            reOrdering_candidate_EnEx(currentN, ref entrySESE, ref exitSESE, nEntrySESE, nExitSESE, true, maxDepth_DomTree, maxDepth_PdomTree);
            reOrdering_candidate_EnEx(currentN, ref entrySESE, ref exitSESE, nEntrySESE, nExitSESE, false, maxDepth_DomTree, maxDepth_PdomTree);
        }

        public void add_more_CandEn_CandEx(int workLoop, int loop, ref int[] entrySESE, ref int nEntrySESE, ref int[] exitSESE, ref int nExitSESE)
        {
            //add more candidate entries and candidate exits to current entrySESE and exitSESE
            if (loop >= 0)
            {
                for (int i = 0; i < Loop[workLoop].Loop[loop].nChild; i++)
                {
                    int child = Loop[workLoop].Loop[loop].child[i];
                    if (Loop[workLoop].Loop[child].etnCandEn != null && check_Exist_in_aSet(Loop[workLoop].Loop[child].etnCandEn[0], entrySESE, nEntrySESE) == false)
                    {
                        //add etnCandEn[0] to entrySESE, increase index
                        entrySESE[nEntrySESE] = Loop[workLoop].Loop[child].etnCandEn[0];
                        nEntrySESE++;
                    }
                    if (Loop[workLoop].Loop[child].etnCandEx != null && check_Exist_in_aSet(Loop[workLoop].Loop[child].etnCandEx[0], exitSESE, nExitSESE) == false)
                    {
                        //add etnCandEx[0] t exitSESE, increase index
                        exitSESE[nExitSESE] = Loop[workLoop].Loop[child].etnCandEx[0];
                        nExitSESE++;
                    }
                }
            }
            if (loop == -3) //no more loop outside
            {
                for (int i = 0; i < Loop[workLoop].nLoop; i++)
                {
                    if (Loop[workLoop].Loop[i].parentLoop == -1)
                    {
                        int child = i;

                        if (Loop[workLoop].Loop[child].etnCandEn != null && check_Exist_in_aSet(Loop[workLoop].Loop[child].etnCandEn[0], entrySESE, nEntrySESE) == false)
                        {
                            //add etnCandEn[0] to entrySESE, increase index
                            entrySESE[nEntrySESE] = Loop[workLoop].Loop[child].etnCandEn[0];
                            nEntrySESE++;
                        }
                        if (Loop[workLoop].Loop[child].etnCandEx != null && check_Exist_in_aSet(Loop[workLoop].Loop[child].etnCandEx[0], exitSESE, nExitSESE) == false)
                        {
                            //add etnCandEx[0] t exitSESE, increase index
                            exitSESE[nExitSESE] = Loop[workLoop].Loop[child].etnCandEx[0];
                            nExitSESE++;
                        }
                    }
                }
            }
        }
        public bool check_Exist_in_aSet(int node, int[] set, int nSet)
        {
            for (int i = 0; i < nSet; i++ )
            {
                if (node == set[i])
                {
                    return true;
                }
            }
            return false;
        }
        public void reduce_SESE(int currentSESE, int sese, int workLoop, int loop, ref bool[] reduceSESE)
        {
            if (currentSESE == -1)
            {
                for (int i = 0; i < Loop[workLoop].Loop[loop].nNode; i++)
                {
                    int node = Loop[workLoop].Loop[loop].Node[i];
                    reduceSESE[node] = true;
                }
                reduceSESE[Loop[workLoop].Loop[loop].header] = true;
            }
            else
            {
                for (int i = 0; i < SESE[currentSESE].SESE[sese].nNode; i++)
                {
                    int node = SESE[currentSESE].SESE[sese].Node[i];
                    reduceSESE[node] = true;
                }
            }
        }

        public void find_SESE_new(int currentN, int currentSESE, int nodeSS)
        {            
            //find SESE
            //build loop-nesting forest
            //build 
            //from bottom-up layer 
            {
                //=> identify SESE which node inside the loop (except loop construct ~ split and join only)
                        //=>intersection dom(-1) and pdom(-1)
                //=> identify SESE which node outside the loop 
                //if NL have multiple exit => header = entrySESE; the exitSESE only in the next layer of NL (join) (CIPd of NL exits) or IL having single exit in this layer (2 option only) ===>> with 2 option, we must choice which one identify first to avoid the case: NL have CIPd is the CID of IL in the same layer!!!
                    //if IL have single exit => IL exit = exitSESE; the entrySESE only in the next layer of IL (split) or or header of NL having multiple exit in this layer
            }

            bool[] reduceSESE = new bool[Network[currentN].nNode];
            transfer_AdjacencyMatrix(currentN, ref AjM_Network, ref Network[currentN].nNode);
            checkEdges = new int[Network[currentN].nNode, Network[currentN].nNode]; //mark which edges belong to different SESE

            int[] entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
            int nEntrySESE = 0;
            int[] exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
            int nExitSESE = 0;
            bool NL = false;
            int curDepth;
            //initiate SESE
            SESE[currentSESE] = new strSESE();

            if (nodeSS == -2)
            {
                //Reset counting variable to 0
                lemma2_C = 0;
                prop4_C = 0;
                lemma3_C = 0;
                lemma4_C = 0;
                lemma5_C = 0;
            }

            if (Loop[orgLoop].nLoop != 0 && nodeSS == -2)
            {               
                curDepth = Loop[orgLoop].maxDepth;
                do
                {
                    for (int i = 0; i < Loop[orgLoop].nLoop; i++)
                    {
                        if (Loop[orgLoop].Loop[i].depth != curDepth) continue;
                        {
                            //OK we will work here======
                            bool checkIL = false;
                            if (Loop[orgLoop].Loop[i].nEntry == 1) NL = true;
                            if (Loop[orgLoop].Loop[i].nEntry > 1) NL = false;
                            entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                            nEntrySESE = 0;
                            exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                            nExitSESE = 0;

                            //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                            get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, i, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
                            if (nEntrySESE > 0 && nExitSESE > 0)
                            {
                                identifySESE_new(currentN, currentSESE, orgLoop, i, NL, true, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE);
                            }

                            //If Loop = NL => if nExits > 1 => find CIPd(exits) ~ CIPd must inside the next layer or single exit of ILs of this layer ONLY. => break()
                            if (NL)
                            {
                                if (Loop[orgLoop].Loop[i].nExit > 1)
                                {
                                    int CIPd_exits = -1;
                                    #region find CIPd(exits)
                                    int[] calDom = null;
                                    for (int k = 0; k < Loop[orgLoop].Loop[i].nExit; k++)
                                    {
                                        calDom = find_Intersection(Network[currentN].nNode, calDom, Network[currentN].Node[Loop[orgLoop].Loop[i].Exit[k]].DomRev);
                                    }
                                    if (calDom.Length > 0)
                                    {
                                        bool check_CIPd = false;
                                        for (int cal = 0; cal < calDom.Length; cal++) //find until have the suitable CIPd
                                        {
                                            if (check_CIPd == false) CIPd_exits = calDom[cal];

                                            //CIPd_exits = header;

                                            int parent_i = Loop[orgLoop].Loop[i].parentLoop;
                                            //check CIPd(exits) in parent or not                                            
                                            if (parent_i != -1)
                                            {
                                                for (int k = 0; k < Loop[orgLoop].Loop[parent_i].nNode; k++)
                                                {
                                                    if (Loop[orgLoop].Loop[parent_i].Node[k] == CIPd_exits) { check_CIPd = true; break; }

                                                }
                                                if (check_CIPd == false)
                                                {
                                                    for (int il = 0; il < Loop[orgLoop].Loop[parent_i].nChild; il++)
                                                    {
                                                        if (il != i)
                                                        {
                                                            if (Loop[orgLoop].Loop[il].nEntry > 1 && Loop[orgLoop].Loop[il].nExit == 1)
                                                                if (CIPd_exits == Loop[orgLoop].Loop[il].Exit[0]) { check_CIPd = true; break; }
                                                        }
                                                    }
                                                    if (check_CIPd) break;
                                                }
                                            }
                                            else //there are no loop outside
                                            {
                                                for (int k = 0; k < Network[currentN].nNode; k++)
                                                {
                                                    if (Network[reduceTempNet].Node[k].nPost > 0 && Network[reduceTempNet].Node[k].nPre > 0)
                                                        if (CIPd_exits == k && !isLoopEntries(orgLoop, k)) { check_CIPd = true; break; }
                                                }
                                                if (check_CIPd == false)
                                                {
                                                    for (int j = 0; j < Loop[orgLoop].nLoop; j++)
                                                    {
                                                        if (Loop[orgLoop].Loop[j].depth == 1 && i != j)
                                                        {
                                                            if (Loop[orgLoop].Loop[j].nEntry > 1 && Loop[orgLoop].Loop[j].nExit == 1)
                                                                if (CIPd_exits == Loop[orgLoop].Loop[j].Exit[0]) { check_CIPd = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (check_CIPd)
                                        {
                                            /*
                                            entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                                            nEntrySESE = 0;
                                            exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                                            nExitSESE = 0;

                                            entrySESE[nEntrySESE] = Loop[orgLoop].Loop[i].header;
                                            nEntrySESE++;
                                            exitSESE[nExitSESE] = CIPd_exits;
                                            nExitSESE++;
                                             * */

                                            //store 2 nodes - 1 from the loop 1 from outside the loop => loop.etnCandEn and loop.etnCandEx
                                            Loop[orgLoop].Loop[i].etnCandEn = new int[1];
                                            Loop[orgLoop].Loop[i].nEtnCandEn = 0;
                                            Loop[orgLoop].Loop[i].etnCandEn[Loop[orgLoop].Loop[i].nEtnCandEn] = Loop[orgLoop].Loop[i].header;
                                            Loop[orgLoop].Loop[i].nEtnCandEn++;

                                            Loop[orgLoop].Loop[i].etnCandEx = new int[1];
                                            Loop[orgLoop].Loop[i].nEtnCandEx = 0;
                                            Loop[orgLoop].Loop[i].etnCandEx[Loop[orgLoop].Loop[i].nEtnCandEx] = CIPd_exits;
                                            Loop[orgLoop].Loop[i].nEtnCandEx++;

                                            /*
                                            if (nEntrySESE > 0 && nExitSESE > 0)
                                            {
                                                identifySESE_new(currentN, currentSESE, orgLoop, i, NL, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, true);
                                            }
                                            */
                                        }
                                    }
                                    #endregion
                                }
                                else lemma3_C++;

                            }
                            //If loop = IL => if nExits = 1 => find CID(entries) ~ CID must inside the next layer or header of NL of this layer only => break()
                            else
                            {
                                if (Loop[orgLoop].Loop[i].nExit == 1)
                                {
                                    int CID_entries = -1;
                                    #region find CID(entries)
                                    int[] calDom = null;
                                    for (int k = 0; k < Loop[orgLoop].Loop[i].nEntry; k++)
                                    {
                                        calDom = find_Intersection(Network[currentN].nNode, calDom, Network[currentN].Node[Loop[orgLoop].Loop[i].Entry[k]].Dom);
                                    }
                                    if (calDom.Length > 0)
                                    {                                        
                                        bool check_CID = false;
                                        for (int cal = calDom.Length - 1; cal > 0; cal--)
                                        {
                                            if (check_CID == false) CID_entries = calDom[cal];
                                            else break;

                                            int parent_i = Loop[orgLoop].Loop[i].parentLoop;
                                            //check CID(entries) in parent or not
                                            
                                            if (parent_i != -1) //if it have parent loop
                                            {
                                                for (int k = 0; k < Loop[orgLoop].Loop[parent_i].nNode; k++)
                                                {
                                                    if (Loop[orgLoop].Loop[parent_i].Node[k] == CID_entries) { check_CID = true; break; }
                                                }
                                                if (check_CID == false)
                                                {
                                                    for (int nl = 0; nl < Loop[orgLoop].Loop[parent_i].nChild; nl++)
                                                    {
                                                        if (nl != i)
                                                        {
                                                            if (Loop[orgLoop].Loop[nl].nEntry == 1 && Loop[orgLoop].Loop[nl].nExit > 1)
                                                                if (CID_entries == Loop[orgLoop].Loop[nl].header) { check_CID = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                            else //if there are no loop outside
                                            {
                                                for (int k = 0; k < Network[currentN].nNode; k++)
                                                {
                                                    if (Network[reduceTempNet].Node[k].nPost > 0 && Network[reduceTempNet].Node[k].nPre > 0)
                                                        if (CID_entries == k && !isLoopExits(orgLoop, k)) { check_CID = true; break; }
                                                }
                                                if (check_CID == false)
                                                {
                                                    for (int j = 0; j < Loop[orgLoop].nLoop; j++)
                                                    {
                                                        if (Loop[orgLoop].Loop[j].depth == 1 && i != j)
                                                        {
                                                            if (Loop[orgLoop].Loop[j].nEntry == 1 && Loop[orgLoop].Loop[j].nExit > 1)
                                                                if (CID_entries == Loop[orgLoop].Loop[j].header) { check_CID = true; break; }
                                                        }
                                                    }
                                                }

                                            }

                                            
                                            if (check_CID)
                                            {
                                                /*
                                                entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                                                nEntrySESE = 0;
                                                exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                                                nExitSESE = 0;

                                                entrySESE[nEntrySESE] = CID_entries;
                                                nEntrySESE++;
                                                exitSESE[nExitSESE] = Loop[orgLoop].Loop[i].Exit[0];
                                                nExitSESE++;
                                                */ 

                                                //store 2 nodes - 1 from the loop 1 from outside the loop => loop.etnCandEn and loop.etnCandEx
                                                Loop[orgLoop].Loop[i].etnCandEn = new int[1];
                                                Loop[orgLoop].Loop[i].nEtnCandEn = 0;
                                                Loop[orgLoop].Loop[i].etnCandEn[Loop[orgLoop].Loop[i].nEtnCandEn] = CID_entries;
                                                Loop[orgLoop].Loop[i].nEtnCandEn++;

                                                Loop[orgLoop].Loop[i].etnCandEx = new int[1];
                                                Loop[orgLoop].Loop[i].nEtnCandEx = 0;
                                                Loop[orgLoop].Loop[i].etnCandEx[Loop[orgLoop].Loop[i].nEtnCandEx] = Loop[orgLoop].Loop[i].Exit[0];
                                                Loop[orgLoop].Loop[i].nEtnCandEx++;
                                              
                                                /*
                                                if (nEntrySESE > 0 && nExitSESE > 0)
                                                {
                                                    identifySESE_new(currentN, currentSESE, orgLoop, i, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, true);
                                                }
                                                */ 
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    checkIL = true;
                                }
                                
                            }
                            if (!checkIL)
                            {
                                int[] calSESE = new int[Loop[orgLoop].Loop[i].nNode + 1];
                                for (int m = 0; m < Loop[orgLoop].Loop[i].nNode; m++) calSESE[m] = Loop[orgLoop].Loop[i].Node[m];
                                calSESE[Loop[orgLoop].Loop[i].nNode] = Loop[orgLoop].Loop[i].header;
                                // markingEdges_calSESE(currentN, calSESE, nodeD, nodeR); (Partialy remove?, need to be restored?????
                            }
                        }
                    }
                    curDepth--;
                } while (curDepth > 0);

                //for the rest of the node outside the loops (in cyclic model)
                entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                nEntrySESE = 0;
                exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                nExitSESE = 0;

                //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, -3, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree); //use reduceTempNet for identify which nodes is outside the loops
                if (nEntrySESE > 0 && nExitSESE > 0)
                {
                    identifySESE_new(currentN, currentSESE, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                }
            }
            else
            {
                //acyclic model
                entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                nEntrySESE = 0;
                exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                nExitSESE = 0;

                //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, -1, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
                if (nEntrySESE > 0 && nExitSESE > 0)
                {
                    identifySESE_new(currentN, currentSESE, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                }
            }
            //make hierarchy
            #region Make hierarchy
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[iSE].nNode >= SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = find_Intersection(SESE[currentSESE].SESE[jSE].nNode, SESE[currentSESE].SESE[iSE].Node, SESE[currentSESE].SESE[jSE].Node);

                    if (check_Same(calSESE, SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (SESE[currentSESE].SESE[jSE].nNode > SESE[currentSESE].SESE[SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            int max_Depth = 0;
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                SESE[currentSESE].SESE[iSE].depth = depth;
                SESE[currentSESE].SESE[iSE].nChild = cntFind;
                SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            SESE[currentSESE].maxDepth = max_Depth;
            //================================
            //modify SESE (SESE[currentSESE].SESE[i].parentSESE);
            //modify_SESE_Hierarchy(currentSESE);
            //================================
            for (int i = 0; i < SESE[currentSESE].nSESE; i++) SESE[currentSESE].SESE[i].parentSESE = -1;

            // Make hierarchy
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[iSE].nNode >= SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = find_Intersection(SESE[currentSESE].SESE[jSE].nNode, SESE[currentSESE].SESE[iSE].Node, SESE[currentSESE].SESE[jSE].Node);

                    if (check_Same(calSESE, SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (SESE[currentSESE].SESE[jSE].nNode > SESE[currentSESE].SESE[SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            max_Depth = 0;
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                if (iSE == 17)
                {
                    //int stop;
                }
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }

                SESE[currentSESE].SESE[iSE].depth = depth;
                SESE[currentSESE].SESE[iSE].nChild = cntFind;
                SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }

            SESE[currentSESE].maxDepth = max_Depth;
            #endregion

            
        }

        public void find_SESE_Dummy(int currentN, int currentSESE, int nodeSS)
        {
            bool[] reduceSESE = new bool[Network[currentN].nNode];
            transfer_AdjacencyMatrix(currentN, ref AjM_Network, ref Network[currentN].nNode);
            checkEdges = new int[Network[currentN].nNode, Network[currentN].nNode]; //mark which edges belong to different SESE

            int[] entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
            int nEntrySESE = 0;
            int[] exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
            int nExitSESE = 0;
            bool NL = false;
            int curDepth;
            //initiate SESE
            SESE[currentSESE] = new strSESE();

            if (nodeSS == -2)
            {
                //Reset counting variable to 0
                lemma2_C = 0;
                prop4_C = 0;
                lemma3_C = 0;
                lemma4_C = 0;
                lemma5_C = 0;
            }


            //acyclic model
            entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
            nEntrySESE = 0;
            exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
            nExitSESE = 0;

            //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
            get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, -5, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
            if (nEntrySESE > 0 && nExitSESE > 0)
            {
                identifySESE_NewApproach(currentN, currentSESE, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
            }

            //make hierarchy
            make_SESE_hierarchy(currentN, currentSESE);           
        }

        public void find_SESE_WithLoop(int currentN, int currentSESE, int nodeSS)
        {
            //find SESE
            //build loop-nesting forest
            //build 
            //from bottom-up layer 
            {
                //=> identify SESE which node inside the loop (except loop construct ~ split and join only)
                //=>intersection dom(-1) and pdom(-1)
                //=> identify SESE which node outside the loop 
                //if NL have multiple exit => header = entrySESE; the exitSESE only in the next layer of NL (join) (CIPd of NL exits) or IL having single exit in this layer (2 option only) ===>> with 2 option, we must choice which one identify first to avoid the case: NL have CIPd is the CID of IL in the same layer!!!
                //if IL have single exit => IL exit = exitSESE; the entrySESE only in the next layer of IL (split) or or header of NL having multiple exit in this layer
            }

            bool[] reduceSESE = new bool[Network[currentN].nNode];
            transfer_AdjacencyMatrix(currentN, ref AjM_Network, ref Network[currentN].nNode);
            checkEdges = new int[Network[currentN].nNode, Network[currentN].nNode]; //mark which edges belong to different SESE

            int[] entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
            int nEntrySESE = 0;
            int[] exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
            int nExitSESE = 0;
            bool NL = false;
            int curDepth;
            //initiate SESE
            SESE[currentSESE] = new strSESE();

            if (nodeSS == -2)
            {
                //Reset counting variable to 0
                lemma2_C = 0;
                prop4_C = 0;
                lemma3_C = 0;
                lemma4_C = 0;
                lemma5_C = 0;
            }

            if (Loop[orgLoop].nLoop != 0 && nodeSS == -2)
            {
                curDepth = Loop[orgLoop].maxDepth;
                do
                {
                    for (int i = 0; i < Loop[orgLoop].nLoop; i++)
                    {
                        if (Loop[orgLoop].Loop[i].depth != curDepth) continue;
                        {
                            //OK we will work here======
                            bool checkIL = false;
                            if (Loop[orgLoop].Loop[i].nEntry == 1) NL = true;
                            if (Loop[orgLoop].Loop[i].nEntry > 1) NL = false;
                            entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                            nEntrySESE = 0;
                            exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                            nExitSESE = 0;

                            //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                            get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, i, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
                            if (nEntrySESE > 0 && nExitSESE > 0)
                            {
                                identifySESE_NewApproach(currentN, currentSESE, orgLoop, i, NL, true, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE);
                            }

                            //If Loop = NL => if nExits > 1 => find CIPd(exits) ~ CIPd must inside the next layer or single exit of ILs of this layer ONLY. => break()
                            if (NL)
                            {
                                if (Loop[orgLoop].Loop[i].nExit > 1)
                                {
                                    int CIPd_exits = -1;
                                    #region find CIPd(exits)
                                    int[] calDom = null;
                                    for (int k = 0; k < Loop[orgLoop].Loop[i].nExit; k++)
                                    {
                                        calDom = find_Intersection(Network[currentN].nNode, calDom, Network[currentN].Node[Loop[orgLoop].Loop[i].Exit[k]].DomRev);
                                    }
                                    if (calDom.Length > 0)
                                    {
                                        bool check_CIPd = false;
                                        for (int cal = 0; cal < calDom.Length; cal++) //find until have the suitable CIPd
                                        {
                                            if (check_CIPd == false) CIPd_exits = calDom[cal];

                                            //CIPd_exits = header;

                                            int parent_i = Loop[orgLoop].Loop[i].parentLoop;
                                            //check CIPd(exits) in parent or not                                            
                                            if (parent_i != -1)
                                            {
                                                for (int k = 0; k < Loop[orgLoop].Loop[parent_i].nNode; k++)
                                                {
                                                    if (Loop[orgLoop].Loop[parent_i].Node[k] == CIPd_exits) { check_CIPd = true; break; }

                                                }
                                                if (check_CIPd == false)
                                                {
                                                    for (int il = 0; il < Loop[orgLoop].Loop[parent_i].nChild; il++)
                                                    {
                                                        if (il != i)
                                                        {
                                                            if (Loop[orgLoop].Loop[il].nEntry > 1 && Loop[orgLoop].Loop[il].nExit == 1)
                                                                if (CIPd_exits == Loop[orgLoop].Loop[il].Exit[0]) { check_CIPd = true; break; }
                                                        }
                                                    }
                                                    if (check_CIPd) break;
                                                }
                                            }
                                            else //there are no loop outside
                                            {
                                                for (int k = 0; k < Network[currentN].nNode; k++)
                                                {
                                                    if (Network[reduceTempNet].Node[k].nPost > 0 && Network[reduceTempNet].Node[k].nPre > 0)
                                                        if (CIPd_exits == k && !isLoopEntries(orgLoop, k)) { check_CIPd = true; break; }
                                                }
                                                if (check_CIPd == false)
                                                {
                                                    for (int j = 0; j < Loop[orgLoop].nLoop; j++)
                                                    {
                                                        if (Loop[orgLoop].Loop[j].depth == 1 && i != j)
                                                        {
                                                            if (Loop[orgLoop].Loop[j].nEntry > 1 && Loop[orgLoop].Loop[j].nExit == 1)
                                                                if (CIPd_exits == Loop[orgLoop].Loop[j].Exit[0]) { check_CIPd = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (check_CIPd)
                                        {
                                            /*
                                            entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                                            nEntrySESE = 0;
                                            exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                                            nExitSESE = 0;

                                            entrySESE[nEntrySESE] = Loop[orgLoop].Loop[i].header;
                                            nEntrySESE++;
                                            exitSESE[nExitSESE] = CIPd_exits;
                                            nExitSESE++;
                                             * */

                                            //store 2 nodes - 1 from the loop 1 from outside the loop => loop.etnCandEn and loop.etnCandEx
                                            Loop[orgLoop].Loop[i].etnCandEn = new int[1];
                                            Loop[orgLoop].Loop[i].nEtnCandEn = 0;
                                            Loop[orgLoop].Loop[i].etnCandEn[Loop[orgLoop].Loop[i].nEtnCandEn] = Loop[orgLoop].Loop[i].header;
                                            Loop[orgLoop].Loop[i].nEtnCandEn++;

                                            Loop[orgLoop].Loop[i].etnCandEx = new int[1];
                                            Loop[orgLoop].Loop[i].nEtnCandEx = 0;
                                            Loop[orgLoop].Loop[i].etnCandEx[Loop[orgLoop].Loop[i].nEtnCandEx] = CIPd_exits;
                                            Loop[orgLoop].Loop[i].nEtnCandEx++;

                                            /*
                                            if (nEntrySESE > 0 && nExitSESE > 0)
                                            {
                                                identifySESE_new(currentN, currentSESE, orgLoop, i, NL, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, true);
                                            }
                                            */
                                        }
                                    }
                                    #endregion
                                }
                                else lemma3_C++;

                            }
                            //If loop = IL => if nExits = 1 => find CID(entries) ~ CID must inside the next layer or header of NL of this layer only => break()
                            else
                            {
                                if (Loop[orgLoop].Loop[i].nExit == 1)
                                {
                                    int CID_entries = -1;
                                    #region find CID(entries)
                                    int[] calDom = null;
                                    for (int k = 0; k < Loop[orgLoop].Loop[i].nEntry; k++)
                                    {
                                        calDom = find_Intersection(Network[currentN].nNode, calDom, Network[currentN].Node[Loop[orgLoop].Loop[i].Entry[k]].Dom);
                                    }
                                    if (calDom.Length > 0)
                                    {
                                        bool check_CID = false;
                                        for (int cal = calDom.Length - 1; cal > 0; cal--)
                                        {
                                            if (check_CID == false) CID_entries = calDom[cal];
                                            else break;

                                            int parent_i = Loop[orgLoop].Loop[i].parentLoop;
                                            //check CID(entries) in parent or not

                                            if (parent_i != -1) //if it have parent loop
                                            {
                                                for (int k = 0; k < Loop[orgLoop].Loop[parent_i].nNode; k++)
                                                {
                                                    if (Loop[orgLoop].Loop[parent_i].Node[k] == CID_entries) { check_CID = true; break; }
                                                }
                                                if (check_CID == false)
                                                {
                                                    for (int nl = 0; nl < Loop[orgLoop].Loop[parent_i].nChild; nl++)
                                                    {
                                                        if (nl != i)
                                                        {
                                                            if (Loop[orgLoop].Loop[nl].nEntry == 1 && Loop[orgLoop].Loop[nl].nExit > 1)
                                                                if (CID_entries == Loop[orgLoop].Loop[nl].header) { check_CID = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                            else //if there are no loop outside
                                            {
                                                for (int k = 0; k < Network[currentN].nNode; k++)
                                                {
                                                    if (Network[reduceTempNet].Node[k].nPost > 0 && Network[reduceTempNet].Node[k].nPre > 0)
                                                        if (CID_entries == k && !isLoopExits(orgLoop, k)) { check_CID = true; break; }
                                                }
                                                if (check_CID == false)
                                                {
                                                    for (int j = 0; j < Loop[orgLoop].nLoop; j++)
                                                    {
                                                        if (Loop[orgLoop].Loop[j].depth == 1 && i != j)
                                                        {
                                                            if (Loop[orgLoop].Loop[j].nEntry == 1 && Loop[orgLoop].Loop[j].nExit > 1)
                                                                if (CID_entries == Loop[orgLoop].Loop[j].header) { check_CID = true; break; }
                                                        }
                                                    }
                                                }

                                            }


                                            if (check_CID)
                                            {                                                
                                                //store 2 nodes - 1 from the loop 1 from outside the loop => loop.etnCandEn and loop.etnCandEx
                                                Loop[orgLoop].Loop[i].etnCandEn = new int[1];
                                                Loop[orgLoop].Loop[i].nEtnCandEn = 0;
                                                Loop[orgLoop].Loop[i].etnCandEn[Loop[orgLoop].Loop[i].nEtnCandEn] = CID_entries;
                                                Loop[orgLoop].Loop[i].nEtnCandEn++;

                                                Loop[orgLoop].Loop[i].etnCandEx = new int[1];
                                                Loop[orgLoop].Loop[i].nEtnCandEx = 0;
                                                Loop[orgLoop].Loop[i].etnCandEx[Loop[orgLoop].Loop[i].nEtnCandEx] = Loop[orgLoop].Loop[i].Exit[0];
                                                Loop[orgLoop].Loop[i].nEtnCandEx++;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    checkIL = true;
                                }

                            }
                            if (!checkIL)
                            {
                                int[] calSESE = new int[Loop[orgLoop].Loop[i].nNode + 1];
                                for (int m = 0; m < Loop[orgLoop].Loop[i].nNode; m++) calSESE[m] = Loop[orgLoop].Loop[i].Node[m];
                                calSESE[Loop[orgLoop].Loop[i].nNode] = Loop[orgLoop].Loop[i].header;
                                // markingEdges_calSESE(currentN, calSESE, nodeD, nodeR); (Partialy remove?, need to be restored?????
                            }
                        }
                    }
                    curDepth--;
                } while (curDepth > 0);

                //for the rest of the node outside the loops (in cyclic model)
                entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                nEntrySESE = 0;
                exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                nExitSESE = 0;

                //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, -3, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree); //use reduceTempNet for identify which nodes is outside the loops
                if (nEntrySESE > 0 && nExitSESE > 0)
                {
                    identifySESE_NewApproach(currentN, currentSESE, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                }
            }
            else
            {
                //acyclic model
                entrySESE = new int[Network[currentN].nNode]; //store current candidate entries of SESE
                nEntrySESE = 0;
                exitSESE = new int[Network[currentN].nNode]; //store current candidate exits of SESE
                nExitSESE = 0;

                //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                get_arrange_CandEnt_CandExt(currentN, reduceTempNet, orgLoop, -1, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
                if (nEntrySESE > 0 && nExitSESE > 0)
                {
                    identifySESE_NewApproach(currentN, currentSESE, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                }
            }
            //make hierarchy
            #region Make hierarchy
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[iSE].nNode >= SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = find_Intersection(SESE[currentSESE].SESE[jSE].nNode, SESE[currentSESE].SESE[iSE].Node, SESE[currentSESE].SESE[jSE].Node);

                    if (check_Same(calSESE, SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (SESE[currentSESE].SESE[jSE].nNode > SESE[currentSESE].SESE[SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            int max_Depth = 0;
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                SESE[currentSESE].SESE[iSE].depth = depth;
                SESE[currentSESE].SESE[iSE].nChild = cntFind;
                SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            SESE[currentSESE].maxDepth = max_Depth;
            //================================
            //modify SESE (SESE[currentSESE].SESE[i].parentSESE);
            //modify_SESE_Hierarchy(currentSESE);
            //================================
            for (int i = 0; i < SESE[currentSESE].nSESE; i++) SESE[currentSESE].SESE[i].parentSESE = -1;

            // Make hierarchy
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[iSE].nNode >= SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = find_Intersection(SESE[currentSESE].SESE[jSE].nNode, SESE[currentSESE].SESE[iSE].Node, SESE[currentSESE].SESE[jSE].Node);

                    if (check_Same(calSESE, SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (SESE[currentSESE].SESE[jSE].nNode > SESE[currentSESE].SESE[SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            max_Depth = 0;
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                if (iSE == 17)
                {
                    //int stop;
                }
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }

                SESE[currentSESE].SESE[iSE].depth = depth;
                SESE[currentSESE].SESE[iSE].nChild = cntFind;
                SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }

            SESE[currentSESE].maxDepth = max_Depth;
            #endregion


        }

        public void make_SESE_hierarchy(int currentN, int currentSESE)
        {
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[iSE].nNode >= SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = find_Intersection(SESE[currentSESE].SESE[jSE].nNode, SESE[currentSESE].SESE[iSE].Node, SESE[currentSESE].SESE[jSE].Node);

                    if (check_Same(calSESE, SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (SESE[currentSESE].SESE[jSE].nNode > SESE[currentSESE].SESE[SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            int max_Depth = 0;
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                SESE[currentSESE].SESE[iSE].depth = depth;
                SESE[currentSESE].SESE[iSE].nChild = cntFind;
                SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            SESE[currentSESE].maxDepth = max_Depth;
            //================================
            //modify SESE (SESE[currentSESE].SESE[i].parentSESE);
            //modify_SESE_Hierarchy(currentSESE);
            //================================
            for (int i = 0; i < SESE[currentSESE].nSESE; i++) SESE[currentSESE].SESE[i].parentSESE = -1;

            // Make hierarchy
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[iSE].nNode >= SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = find_Intersection(SESE[currentSESE].SESE[jSE].nNode, SESE[currentSESE].SESE[iSE].Node, SESE[currentSESE].SESE[jSE].Node);

                    if (check_Same(calSESE, SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (SESE[currentSESE].SESE[jSE].nNode > SESE[currentSESE].SESE[SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            max_Depth = 0;
            for (int iSE = 0; iSE < SESE[currentSESE].nSESE; iSE++)
            {
                if (iSE == 17)
                {
                    //int stop;
                }
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < SESE[currentSESE].nSESE; jSE++)
                {
                    if (SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }

                SESE[currentSESE].SESE[iSE].depth = depth;
                SESE[currentSESE].SESE[iSE].nChild = cntFind;
                SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }

            SESE[currentSESE].maxDepth = max_Depth;
        }

        public void polygonIdentification(int currentN, int currentSESE)
        {
            //output => polygon which contain parentSESE information

            //convert model to adjList[][]
            adjList_Directed_Create(currentN, ref adjList);
            bool[] Mark = new bool[Network[currentN].nNode];
            Mark = Array.ConvertAll<bool, bool>(Mark, b => b = false); //MARK FOR DFS()
            int curDepth = SESE[currentSESE].maxDepth;
            int[] getPolygons = new int[Network[currentN].nNode*2];
            int nPolygons = 0;
            int firstNode;
            copy_SESE(currentSESE, tempSESE);
            do
            {
                for (int i = 0; i < SESE[currentSESE].nSESE; i++)
                {
                    if (SESE[currentSESE].SESE[i].depth == curDepth)
                    {
                        int curEntry = SESE[currentSESE].SESE[i].Entry;
                        int curExit = SESE[currentSESE].SESE[i].Exit;
                        if (i == 3)
                        { }
                        //reduce all nested SESE in adjList
                        for (int k = 0; k < SESE[currentSESE].SESE[i].nChild; k++)
                        {
                            int child = SESE[currentSESE].SESE[i].child[k];
                            int en = SESE[currentSESE].SESE[child].Entry;
                            int ex = SESE[currentSESE].SESE[child].Exit;
                            adjList[en] = new int[2];
                            adjList[en][0] = 1;
                            adjList[en][1] = ex;
                            for (int z = 0; z < SESE[currentSESE].SESE[child].nNode; z++)
                            {
                                int temp_node = SESE[currentSESE].SESE[child].Node[z];
                                Mark[temp_node] = true;
                                if ((temp_node == SESE[currentSESE].SESE[child].Entry) || (temp_node == SESE[currentSESE].SESE[child].Exit))
                                    Mark[temp_node] = false;
                            }
                            int[] temp = new int[Network[currentN].Node[ex].nPost];
                            int nTemp = 0;
                            for (int w = 0; w < Network[currentN].Node[ex].nPost; w++)
                            {
                                if (Mark[Network[currentN].Node[ex].Post[w]] == false)
                                {
                                    temp[nTemp] = Network[currentN].Node[ex].Post[w];
                                    nTemp++;
                                }
                            }
                            adjList[ex][0] = nTemp;
                            for (int w = 0; w < nTemp; w++)
                                adjList[ex][w + 1] = temp[w]; 

                        }
                        //DFS_Polygon()
                        for (int j = 0; j < Network[currentN].Node[curEntry].nPost; j++)
                        {
                            if (i == 3)
                            { }
                            getPolygons = new int[Network[currentN].nNode*2];
                            nPolygons = 0;
                            firstNode = Network[currentN].Node[curEntry].Post[j];
                            int after_node = get_post_exitSESE(currentN, currentSESE, i, curExit);
                            DFS_Polygon(adjList, ref Mark, firstNode, curEntry, after_node, ref getPolygons, ref nPolygons); //=> 0011110000111001110001110 sample
                            if (nPolygons > 1)
                            {
                                //process polygon
                                get_calSESE_Polygon(getPolygons, nPolygons, currentN, tempSESE, curEntry, curExit, curDepth, i);
                            }
                        }
                        Mark = Array.ConvertAll<bool, bool>(Mark, b => b = false);
                    }
                }
                curDepth--;
            }
            while (curDepth > 0);

            //reduce all SESE first;
            //reduce all nested SESE in adjList
            for (int k = 0; k < SESE[currentSESE].nSESE; k++)
            {
                if (SESE[currentSESE].SESE[k].depth == 1)
                {
                    //int child = SESE[currentSESE].SESE[i].child[k];
                    int en = SESE[currentSESE].SESE[k].Entry;
                    int ex = SESE[currentSESE].SESE[k].Exit;
                    
                    adjList[en] = new int[2];
                    adjList[en][0] = 1;
                    adjList[en][1] = ex;                    
                    for (int z = 0; z < SESE[currentSESE].SESE[k].nNode; z++)
                    {
                        Mark[SESE[currentSESE].SESE[k].Node[z]] = true;
                        if ((SESE[currentSESE].SESE[k].Node[z] == SESE[currentSESE].SESE[k].Entry) || (SESE[currentSESE].SESE[k].Node[z] == SESE[currentSESE].SESE[k].Exit))
                            Mark[SESE[currentSESE].SESE[k].Node[z]] = false;
                    }
                    int[] temp = new int[Network[currentN].Node[ex].nPost];
                    int nTemp = 0;
                    for (int w = 0; w < Network[currentN].Node[ex].nPost; w++)
                    {
                        if (Mark[Network[currentN].Node[ex].Post[w]] == false)
                        {
                            temp[nTemp] = Network[currentN].Node[ex].Post[w];
                            nTemp++;
                        }
                    }
                    adjList[ex][0] = nTemp;
                    for (int w = 0; w < nTemp; w++)
                        adjList[ex][w + 1] = temp[w]; 
                }
            }
            //find polygon outside from START to END
            int START_plg = -1;
            int END_plg = -1;
            for (int i = 0; i < Network[currentN].nNode; i++ )
            {
                if (Network[currentN].Node[i].Kind == "START") START_plg = i;
                if (Network[currentN].Node[i].Kind == "END") END_plg = i;
            }
            getPolygons = new int[Network[currentN].nNode*2];
            nPolygons = 0;
            firstNode = Network[currentN].Node[START_plg].Post[0];
            DFS_Polygon(adjList, ref Mark, firstNode, START_plg, END_plg, ref getPolygons, ref nPolygons); //=> 0011110000111001110001110 sample
            if (nPolygons > 1)
            {
                get_calSESE_Polygon(getPolygons, nPolygons, currentN, tempSESE, START_plg, END_plg, curDepth, -1);
            }

            //update the hierarchy of SESE (copy SESE from tempSESE to currentSESE)
            copy_SESE(tempSESE, currentSESE);
            make_SESE_hierarchy(currentN, currentSESE);
        }
        public int get_post_exitSESE(int currentN, int currentSESE, int sese, int node)
        {
            if (Network[currentN].Node[node].nPost > 1)
            {
                for (int i = 0; i < Network[currentN].Node[node].nPost; i++)
                {
                    if (node_insideSESE(SESE[currentSESE].SESE[sese].Node, Network[currentN].Node[node].Post[i]) == false) return Network[currentN].Node[node].Post[i];
                }
                //node_insideSESE(int[] calSESE, int node)
            }
            else
                if (Network[currentN].Node[node].nPost == 1)
                    return Network[currentN].Node[node].Post[0];
            return -1;
        }
        public void get_calSESE_Polygon(int[] getPolygons, int nPolygons, int currentN, int currentSESE, int nodeD, int nodeR, int depth, int parentSESE)
        {
            bool flag = true;
            int[] tempList = new int[nPolygons];
            int[] tempLabel = new int[nPolygons];
            int count_temp = 0;
            for (int i = 0; i < nPolygons; i++)
            {
                if (flag)
                {
                    tempList = new int[nPolygons];
                    tempLabel = new int[nPolygons];
                    count_temp = 0;
                }
                int node_type = type_Node(currentN, currentSESE, getPolygons[i], nodeD, nodeR);
                if (node_type > 0)
                {
                    tempList[count_temp] = getPolygons[i];
                    tempLabel[count_temp] = node_type;
                    count_temp++;
                    if (i < (nPolygons - 1))
                    {
                        if (type_Node(currentN, currentSESE, getPolygons[i + 1], nodeD, nodeR) > 0) flag = false;
                        else flag = true;
                    }
                    else flag = true;
                }
                else flag = true; //cut 01110001 string and extract a polygon

                if (flag == true && count_temp > 1) 
                {
                    int[] calSESE = new int[Network[currentN].nNode];
                    int curIndex = 0;
                    //extend calSESE
                    int[][] listSESE = new int[SESE[currentSESE].nSESE][];
                    int nListSESE = 0;
                    bool isOK = false;
                    for (int k = 0 ; k < count_temp; k++)
                    {
                        if (tempLabel[k] == 2 && (k + 1) < count_temp)
                            if (tempLabel[k + 1] == 3)
                            {
                                listSESE[nListSESE] = new int[2];
                                listSESE[nListSESE][0] = tempList[k];
                                listSESE[nListSESE][1] = tempList[k + 1];
                                nListSESE++;
                            }
                        if (tempLabel[k] == 1)
                        {
                            calSESE[curIndex] = tempList[k];
                            curIndex++;
                            isOK = true;
                        }
                    }
                    for (int k = 0; k < nListSESE; k++)
                    {
                        int en, ex;
                        en = listSESE[k][0];
                        ex = listSESE[k][1];
                        int seseIndex = getSESE_index(currentSESE, en, ex);
                        if (seseIndex > -1)
                        {
                            SESE[currentSESE].SESE[seseIndex].Node.CopyTo(calSESE, curIndex);
                            curIndex = curIndex + SESE[currentSESE].SESE[seseIndex].nNode;
                        }
                    }
                    int[] old_calSESE = new int[curIndex];
                    for (int k = 0; k < curIndex; k++) old_calSESE[k] = calSESE[k];
                    calSESE = new int[curIndex];
                    for (int k = 0; k < curIndex; k++) calSESE[k] = old_calSESE[k];
                    if (nListSESE > 1 || isOK)
                        add_SESE(currentSESE, tempList[0], tempList[count_temp - 1], calSESE, (depth + 1), parentSESE);
                }
            }
        }
        public int getSESE_index(int currentSESE, int entry, int exit)
        {
            for (int i = 0 ; i < SESE[currentSESE].nSESE; i++)
            {
                if (SESE[currentSESE].SESE[i].Entry == entry && SESE[currentSESE].SESE[i].Exit == exit)
                    return i;
            }
            return -1;
        }
        public int type_Node(int currentN, int currentSESE, int node, int nodeD, int nodeR) // return 1 for TASK/EVENT, return 2 for ENTRY SESE, return 3 for EXIT SESE
        {
            if (node != -1)
            {
                if (node == nodeD || node == nodeR) return 0;
                if (Network[currentN].Node[node].nPre > 1 || Network[currentN].Node[node].nPost > 1)
                {
                    for (int i = 0; i < SESE[currentSESE].nSESE; i++)
                    {
                        if (node == SESE[currentSESE].SESE[i].Entry) return 2;
                        if (node == SESE[currentSESE].SESE[i].Exit) return 3;
                    }
                }
                else
                    if (Network[currentN].Node[node].Kind == "TASK" || Network[currentN].Node[node].Kind == "EVENT")
                        return 1;
            }
            return 0;
        }

        public void add_SESE(int currentSESE, int nodeD, int nodeR, int[] calSESE, int depth, int parentSESE)
        {

            //final checking rigid and add more SESE (1 or more rigids inside)            
            strSESEInform[] oldSESE = new strSESEInform[SESE[currentSESE].nSESE];
            for (int k = 0; k < SESE[currentSESE].nSESE; k++) oldSESE[k] = SESE[currentSESE].SESE[k];

            SESE[currentSESE].SESE = new strSESEInform[SESE[currentSESE].nSESE + 1];
            for (int k = 0; k < SESE[currentSESE].nSESE; k++) SESE[currentSESE].SESE[k] = oldSESE[k];

            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].depth = depth;
            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].parentSESE = parentSESE;
            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Entry = nodeD;
            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Exit = nodeR;
            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].nNode = calSESE.Length;
            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Node = calSESE;

            SESE[currentSESE].nSESE++;
        }
        public void DFS_Polygon(int[][] adjList, ref bool[] Mark, int firstNode, int Entry, int Exit, ref int[] getRigids, ref int nRigids)
        {
            Stack stack = new Stack();
            Mark[firstNode] = true;
            stack.Push(firstNode);
            //getRigids[nRigids] = firstNode;
            //nRigids++;
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());
                //do nothing
                getRigids[nRigids] = curNode;
                nRigids++;
                
                //do something!!
                if (curNode != -1)
                {
                    string abc = Network[3].Node[curNode].Name;
                    for (int i = 1; i <= adjList[curNode][0]; i++)
                    {
                        int v = adjList[curNode][i];

                        if (!Mark[v] && v != Entry && v != Exit)
                        {
                            stack.Push(v);
                            Mark[v] = true;

                            //if v is not gateway => marking 1
                            //if v is SESE_entry => marking 2, SESE_exit => marking 3                        
                        }
                        else //put break branch point
                        {
                            stack.Push(-1);
                        }
                    }
                }
            }
        }

        public bool isInLoopConstruct(int workLoop, int curLoop, int node) //check a node is entry or exit or backward-splitf
        {
            for (int i = 0; i < Loop[workLoop].Loop[curLoop].nEntry; i++)
            {
                if (Loop[workLoop].Loop[curLoop].Entry[i] == node) return true;
            }
            for (int i = 0; i < Loop[workLoop].Loop[curLoop].nExit; i++)
            {
                if (Loop[workLoop].Loop[curLoop].Exit[i] == node) return true;
            }
            for (int i = 0; i < Loop[workLoop].Loop[curLoop].nBackSplit; i++)
            {
                if (Loop[workLoop].Loop[curLoop].BackSplit[i] == node) return true;
            }
            return false;
        }
        public bool isLoopHeader(int workloop, int node) //check "node" is header (reduced (if any) or not
        {
            for (int i = 0; i < Loop[workloop].nLoop; i++)
            {
                if (Loop[workloop].Loop[i].header == node) return true;
            }
            return false;
        }
        public bool checkNL_header_multi_exit(int workloop, int parent, int node)
        {
            for (int i = 0; i < Loop[workloop].nLoop; i++)
            {
                if (Loop[workloop].Loop[i].header == node)
                {
                    if (Loop[workloop].Loop[i].nExit > 1) return true;
                }
            }
            return false;
        }
        //upgrade version of SESE identification
        private void identifySESE_new(int currentN, int currentSESE, int workLoop, int curLoop, bool NL, bool sameSide, bool[,] makeLinkDomTree, 
            bool[,] makeLinkPdomTree, int[][] adjList, int maxDepth_DomTree, int maxDepth_PdomTree, int[] entrySESE, int nEntrySESE, int[] exitSESE, int nExitSESE, ref int[,] checkEdges, bool isLoop, ref bool[] reduceSESE) //isLoop mean we will ignore checking CandEn and CandExt in eDom Tree and EPdomTree
        {
            
            int countEdge = 0;
            //use the set => get the SESE
            //bool[] notVisitExit = new bool[nNode];
            //Identifying SESE
            for (int en = 0; en < nEntrySESE; en++)
            {
                for (int ex = 0; ex < nExitSESE; ex++)
                {
                    int nodeD = entrySESE[en];
                    int nodeR = exitSESE[ex];
                    if (nodeD == nodeR) continue;
                    if (nodeD == 15 && nodeR == 19)
                    {

                    }


                    bool isEntry = true, isExit = true, isOut = false;
                    //SESE = eDom^-1(NodeD) Intersect ePdom^-1(NodeR)
                    int[] calSESE = find_Intersection(Network[currentN].nNode, Network[currentN].Node[nodeD].DomEI, Network[currentN].Node[nodeR].DomRevEI);


                    if (!en_ex_inSESE(calSESE, nodeD, nodeR)) continue; //candidate entry and exit should exist in calSESE ==>> also mean => 
                                                                        //entrySESE weakly dominates exitSESE and exitSESE weakly postdominates entrySESE

                    if (!isOut)
                    {
                        //check_En_Ex_inSESE(nodeD, nodeR, calSESE_temp); <<== remove node in Loop (
                        int nNode = Network[currentN].nNode;
                        int[] SourceOfError = new int[nNode];
                        int[] DecendantOfError = new int[nNode];
                        bool[] Mark = new bool[nNode];
                        Mark = Array.ConvertAll<bool, bool>(Mark, b => b = true); //MARK FOR DFS()
                        int nS = 0;
                        int arrEr = 0;


                        for (int k = 0; k < calSESE.Length; k++)
                        {
                            Mark[calSESE[k]] = false; //mark for DFS

                            if (calSESE[k] == nodeD || calSESE[k] == nodeR) continue;
                            //check the rest of candiate SESE (whether its have predecessor or succesors inside the candiate SESE)                                
                            //int sourceNode = -1;
                            int nodeSESE = calSESE[k];
                            for (int j = 0; j < Network[currentN].Node[calSESE[k]].nPost; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, Network[currentN].Node[calSESE[k]].Post[j]))
                                {
                                    SourceOfError[nS] = Network[currentN].Node[calSESE[k]].Post[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                            for (int j = 0; j < Network[currentN].Node[calSESE[k]].nPre; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, Network[currentN].Node[calSESE[k]].Pre[j]))
                                {
                                    SourceOfError[nS] = Network[currentN].Node[calSESE[k]].Pre[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                        }


                        //Find all relate node
                        if (nS > 0)
                        {
                            //Foreach SOURCEOFERR[i]
                            for (int i = 0; i < nS; i++)
                            {
                                DFS(adjList, ref Mark, i, SourceOfError, DecendantOfError, nodeD, nodeR); //=> COULD be LINEAR O(n) => Mark[] = true for node was visited
                            }

                            //Remove unnecessary Node in SESE to get the SESE
                            int nRemove = 0;
                            for (int i = 0; i < calSESE.Length; i++)
                            {
                                if (Mark[calSESE[i]] == true) //mark the nodes should be removed in calSESE[] // asign = -1; //=> LINEAR
                                {
                                    calSESE[i] = -1;
                                    nRemove++;
                                }
                            }

                            //Re-create a new calSESE for next step; => LINEAR
                            int[] tempSESE = new int[calSESE.Length - nRemove];
                            int nTemp = 0;
                            for (int i = 0; i < calSESE.Length; i++)
                            {
                                if (calSESE[i] != -1)
                                {
                                    tempSESE[nTemp] = calSESE[i];
                                    nTemp++;
                                }
                            }
                            calSESE = tempSESE;
                        }

                        #region Check Entry and Exit which have at least 2 SUC and 2 PRE
                        //check Entries
                        for (int k = 0; k < calSESE.Length; k++)
                        {
                            //Check nodeD - Entry whether belong inside calSESE
                            if (calSESE[k] == nodeD)
                            {
                                isEntry = true;
                                int inPost = 0;
                                for (int i = 0; i < Network[currentN].Node[nodeD].nPost; i++) //check successor of SESE Entry
                                {

                                    for (int j = 0; j < calSESE.Length; j++)
                                    {
                                        if (calSESE[j] == Network[currentN].Node[nodeD].Post[i])
                                        {
                                            inPost++;
                                            //break;
                                        }
                                    }
                                }
                                /*if (inPost < 2)
                                {
                                    isOut = true;
                                    break;
                                }*/

                                //Check entry have at least 1 incoming edge from OUTSIDE calSESE
                                int inPre = 0;
                                for (int i = 0; i < Network[currentN].Node[nodeD].nPre; i++) //check predecessor of SESE Exit
                                {
                                    for (int j = 0; j < calSESE.Length; j++)
                                    {
                                        if (calSESE[j] == Network[currentN].Node[nodeD].Pre[i])
                                        {
                                            inPre++;
                                            //break;
                                        }
                                    }
                                }
                                //Final check (if it have 2 succ or at least 1 succ and 1 pre from inside SESE => out = false
                                if ((inPost + inPre) < 2) isOut = true;
                            }
                            else if (calSESE[k] == nodeR)
                            {
                                isExit = true;

                                if (!isOut)
                                {
                                    int inPre = 0;
                                    for (int i = 0; i < Network[currentN].Node[nodeR].nPre; i++) //check predecessor of SESE Exit
                                    {
                                        //if (Network[currentN].Node[i].Kind != "XOR" && Network[currentN].Node[i].Kind != "OR" && Network[currentN].Node[i].Kind != "AND") continue;
                                        for (int j = 0; j < calSESE.Length; j++)
                                        {
                                            if (calSESE[j] == Network[currentN].Node[nodeR].Pre[i])
                                            {
                                                inPre++;
                                                //break;
                                            }
                                        }

                                    }
                                    int inPost = 0;
                                    for (int i = 0; i < Network[currentN].Node[nodeR].nPost; i++) //check successor of SESE Entry
                                    {

                                        for (int j = 0; j < calSESE.Length; j++)
                                        {
                                            if (calSESE[j] == Network[currentN].Node[nodeR].Post[i])
                                            {
                                                inPost++;
                                                //break;
                                            }
                                        }
                                    }
                                    if ((inPost + inPre) < 2) isOut = true;
                                }
                            }
                        }
                        #endregion
                    }


                    if (isEntry && isExit && !isOut)  // Add
                    {
                        //count lemma  4 5
                        bool notCountLemma2 = false;
                        if (isLoopHeader(orgLoop, nodeD)) { lemma4_C++; notCountLemma2 = true; }
                        if (isLoopSingleExit(nodeR, orgLoop)) { lemma5_C++; notCountLemma2 = true; }

                        //final checking rigid and add more SESE (1 or more rigids inside)
                        identify_more_rigids_BTC(currentN, currentSESE, calSESE, nodeD, nodeR, adjList, ref reduceSESE, makeLink_eDomTree, makeLink_ePdomTree);
                        strSESEInform[] oldSESE = new strSESEInform[SESE[currentSESE].nSESE];
                        for (int k = 0; k < SESE[currentSESE].nSESE; k++) oldSESE[k] = SESE[currentSESE].SESE[k];

                        if (!(NL == true && Loop[workLoop].Loop[curLoop].nExit == 1 && Loop[workLoop].Loop[curLoop].Entry[0] == nodeD && Loop[workLoop].Loop[curLoop].Exit[0] == nodeR))
                        {
                            SESE[currentSESE].SESE = new strSESEInform[SESE[currentSESE].nSESE + 1];
                            for (int k = 0; k < SESE[currentSESE].nSESE; k++) SESE[currentSESE].SESE[k] = oldSESE[k];

                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].depth = -1;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].parentSESE = -1;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Entry = nodeD;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Exit = nodeR;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].nNode = calSESE.Length;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Node = calSESE;

                            SESE[currentSESE].nSESE++;

                            if (notCountLemma2 == false) lemma2_C++;

                            markingEdges_calSESE(currentN, calSESE, nodeD, nodeR);
                            //reduce_SESE(currentSESE, SESE[currentSESE].nSESE -1, -1, -1, ref reduceSESE);
                        }
                    }

                }
            }
        }

        private void identifySESE_NewApproach(int currentN, int currentSESE, int workLoop, int curLoop, bool NL, bool sameSide, bool[,] makeLinkDomTree,
            bool[,] makeLinkPdomTree, int[][] adjList, int maxDepth_DomTree, int maxDepth_PdomTree, int[] entrySESE, int nEntrySESE, int[] exitSESE, int nExitSESE, ref int[,] checkEdges, bool isLoop, ref bool[] reduceSESE) //isLoop mean we will ignore checking CandEn and CandExt in eDom Tree and EPdomTree
        {

            int countEdge = 0;
            //use the set => get the SESE
            //bool[] notVisitExit = new bool[nNode];
            //Identifying SESE
            for (int en = 0; en < nEntrySESE; en++)
            {
                for (int ex = 0; ex < nExitSESE; ex++)
                {
                    int nodeD = entrySESE[en];
                    int nodeR = exitSESE[ex];
                    if (nodeD == nodeR) continue;
                    if (nodeD == 30 && nodeR == 2)
                    {

                    }


                    bool isEntry = true, isExit = true, isOut = false;
                    //SESE = eDom^-1(NodeD) Intersect ePdom^-1(NodeR)
                    int[] calSESE = find_Intersection(Network[currentN].nNode, Network[currentN].Node[nodeD].DomEI, Network[currentN].Node[nodeR].DomRevEI);


                    if (!en_ex_inSESE(calSESE, nodeD, nodeR)) continue; //candidate entry and exit should exist in calSESE ==>> also mean => 
                    //entrySESE weakly dominates exitSESE and exitSESE weakly postdominates entrySESE

                    if (!isOut)
                    {
                        //check_En_Ex_inSESE(nodeD, nodeR, calSESE_temp); <<== remove node in Loop (
                        int nNode = Network[currentN].nNode;
                        int[] SourceOfError = new int[nNode];
                        int[] DecendantOfError = new int[nNode];
                        bool[] Mark = new bool[nNode];
                        Mark = Array.ConvertAll<bool, bool>(Mark, b => b = true); //MARK FOR DFS()
                        int nS = 0;
                        int arrEr = 0;


                        for (int k = 0; k < calSESE.Length; k++)
                        {
                            Mark[calSESE[k]] = false; //mark for DFS

                            if (calSESE[k] == nodeD || calSESE[k] == nodeR) continue;
                            //check the rest of candiate SESE (whether its have predecessor or succesors inside the candiate SESE)                                
                            //int sourceNode = -1;
                            int nodeSESE = calSESE[k];
                            for (int j = 0; j < Network[currentN].Node[calSESE[k]].nPost; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, Network[currentN].Node[calSESE[k]].Post[j]))
                                {
                                    SourceOfError[nS] = Network[currentN].Node[calSESE[k]].Post[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                            for (int j = 0; j < Network[currentN].Node[calSESE[k]].nPre; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, Network[currentN].Node[calSESE[k]].Pre[j]))
                                {
                                    SourceOfError[nS] = Network[currentN].Node[calSESE[k]].Pre[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                        }


                        //Find all relate node
                        if (nS > 0)
                        {
                            //Foreach SOURCEOFERR[i]
                            for (int i = 0; i < nS; i++)
                            {
                                DFS(adjList, ref Mark, i, SourceOfError, DecendantOfError, nodeD, nodeR); //=> COULD be LINEAR O(n) => Mark[] = true for node was visited
                            }

                            //Remove unnecessary Node in SESE to get the SESE
                            int nRemove = 0;
                            for (int i = 0; i < calSESE.Length; i++)
                            {
                                if (Mark[calSESE[i]] == true) //mark the nodes should be removed in calSESE[] // asign = -1; //=> LINEAR
                                {
                                    calSESE[i] = -1;
                                    nRemove++;
                                }
                            }

                            //Re-create a new calSESE for next step; => LINEAR
                            int[] tempSESE = new int[calSESE.Length - nRemove];
                            int nTemp = 0;
                            for (int i = 0; i < calSESE.Length; i++)
                            {
                                if (calSESE[i] != -1)
                                {
                                    tempSESE[nTemp] = calSESE[i];
                                    nTemp++;
                                }
                            }
                            calSESE = tempSESE;
                        }

                        #region Check Entry and Exit which have at least 2 SUC and 2 PRE
                        //check Entries
                        for (int k = 0; k < calSESE.Length; k++)
                        {
                            //Check nodeD - Entry whether belong inside calSESE
                            if (calSESE[k] == nodeD)
                            {
                                isEntry = true;
                                int inPost = 0;
                                for (int i = 0; i < Network[currentN].Node[nodeD].nPost; i++) //check successor of SESE Entry
                                {

                                    for (int j = 0; j < calSESE.Length; j++)
                                    {
                                        if (calSESE[j] == Network[currentN].Node[nodeD].Post[i])
                                        {
                                            inPost++;
                                            //break;
                                        }
                                    }
                                }
                                /*if (inPost < 2)
                                {
                                    isOut = true;
                                    break;
                                }*/

                                //Check entry have at least 1 incoming edge from OUTSIDE calSESE
                                int inPre = 0;
                                for (int i = 0; i < Network[currentN].Node[nodeD].nPre; i++) //check predecessor of SESE Exit
                                {
                                    for (int j = 0; j < calSESE.Length; j++)
                                    {
                                        if (calSESE[j] == Network[currentN].Node[nodeD].Pre[i])
                                        {
                                            inPre++;
                                            //break;
                                        }
                                    }
                                }
                                //Final check (if it have 2 succ or at least 1 succ and 1 pre from inside SESE => out = false
                                if ((inPost + inPre) < 2) isOut = true;
                            }
                            else if (calSESE[k] == nodeR)
                            {
                                isExit = true;

                                if (!isOut)
                                {
                                    int inPre = 0;
                                    for (int i = 0; i < Network[currentN].Node[nodeR].nPre; i++) //check predecessor of SESE Exit
                                    {
                                        //if (Network[currentN].Node[i].Kind != "XOR" && Network[currentN].Node[i].Kind != "OR" && Network[currentN].Node[i].Kind != "AND") continue;
                                        for (int j = 0; j < calSESE.Length; j++)
                                        {
                                            if (calSESE[j] == Network[currentN].Node[nodeR].Pre[i])
                                            {
                                                inPre++;
                                                //break;
                                            }
                                        }

                                    }
                                    int inPost = 0;
                                    for (int i = 0; i < Network[currentN].Node[nodeR].nPost; i++) //check successor of SESE Entry
                                    {

                                        for (int j = 0; j < calSESE.Length; j++)
                                        {
                                            if (calSESE[j] == Network[currentN].Node[nodeR].Post[i])
                                            {
                                                inPost++;
                                                //break;
                                            }
                                        }
                                    }
                                    if ((inPost + inPre) < 2) isOut = true;
                                }
                            }
                        }
                        #endregion
                    }

                    if (check_allEdgesInside(currentN, nodeD, nodeR, calSESE, checkEdges, ref countEdge)) continue; //CHECK DUPLICATED SESE (PREPOSITION 4) // DEFINITION B (new)

                    if (isEntry && isExit && !isOut)  // Add
                    {
                        //count lemma  4 5
                        bool notCountLemma2 = false;
                        if (isLoopHeader(orgLoop, nodeD)) { lemma4_C++; notCountLemma2 = true; }
                        if (isLoopSingleExit(nodeR, orgLoop)) { lemma5_C++; notCountLemma2 = true; }

                        //final checking rigid and add more SESE (1 or more rigids inside)
                        identify_more_rigids_BTC(currentN, currentSESE, calSESE, nodeD, nodeR, adjList, ref reduceSESE, makeLink_eDomTree, makeLink_ePdomTree);
                        strSESEInform[] oldSESE = new strSESEInform[SESE[currentSESE].nSESE];
                        for (int k = 0; k < SESE[currentSESE].nSESE; k++) oldSESE[k] = SESE[currentSESE].SESE[k];

                        if (!(NL == true && Loop[workLoop].Loop[curLoop].nExit == 1 && Loop[workLoop].Loop[curLoop].Entry[0] == nodeD && Loop[workLoop].Loop[curLoop].Exit[0] == nodeR))
                        {
                            SESE[currentSESE].SESE = new strSESEInform[SESE[currentSESE].nSESE + 1];
                            for (int k = 0; k < SESE[currentSESE].nSESE; k++) SESE[currentSESE].SESE[k] = oldSESE[k];

                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].depth = -1;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].parentSESE = -1;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Entry = nodeD;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Exit = nodeR;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].nNode = calSESE.Length;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Node = calSESE;

                            SESE[currentSESE].nSESE++;

                            if (notCountLemma2 == false) lemma2_C++;

                            markingEdges_calSESE(currentN, calSESE, nodeD, nodeR); //fix on 2018/1/27 => 
                            //reduce_SESE(currentSESE, SESE[currentSESE].nSESE -1, -1, -1, ref reduceSESE);
                        }
                    }
                }
            }
        }

        public void markingEdges_calSESE(int currentN, int[] calSESE, int nodeD, int nodeR)
        {
            //get edges from calSESE
            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                if (node_insideSESE(calSESE, Network[currentN].Link[i].fromNode) && node_insideSESE(calSESE, Network[currentN].Link[i].toNode))
                {
                    AjM_Network[Network[currentN].Link[i].fromNode, Network[currentN].Link[i].toNode] = 0;
                }
            }
            AjM_Network[nodeD, nodeR] = 1; //newly added for fixing the refinement glitch
        }

        public void count_Edges(int currentN, int[] calSESE, int nCalSESE, ref int remainedEdges, int nodeD, int nodeR)
        {
            calSESE[nCalSESE] = nodeD;
            nCalSESE++;
            calSESE[nCalSESE] = nodeR;
            nCalSESE++;
            //get edges from calSESE
            //we should careful about the case natural loop single exit have only 1 edge in forward flow but have multiple backedges (Loop and SESE 1.net)
            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                int frNode = Network[currentN].Link[i].fromNode;
                int toNode = Network[currentN].Link[i].toNode;
                if (node_insideSET(calSESE, nCalSESE, frNode) && node_insideSET(calSESE, nCalSESE, toNode))
                {
                    if ((AjM_Network[Network[currentN].Link[i].fromNode, Network[currentN].Link[i].toNode] != 0) && !(frNode ==nodeR && toNode ==nodeD))
                    {
                        remainedEdges++;
                    }
                }
            }
            nCalSESE = nCalSESE - 2; //roll-back to original
        }
        public bool node_insideSET(int[] calSESE, int nCalSESE, int node)
        {
            for (int i = 0; i < nCalSESE; i++)
            {
                if (calSESE[i] == node)
                    return true;
            }
            return false;
        }


        //identify more rigids as Tri-connected component approach (do we need it?)
        public void identify_more_rigids_BTC(int currentN, int currentSESE, int[] calSESE, int nodeD, int nodeR, int[][] adjList, ref bool[] reduceSESE, bool[,] makeLink_eDomTree, bool[,] makeLink_ePdomTree)
        {
            int[] aloneNode = new int[Network[currentN].nNode];
            int nAloneNode = 0;
            if (nodeD == 36 && nodeR == 38)
            {

            }

            //find all sucessor of current NodeD (candidate entry) and add to aloneNode[]
            for (int i = 0; i < Network[currentN].Node[nodeD].nPost; i++)
            {
                int nodePost = Network[currentN].Node[nodeD].Post[i];
                if (node_insideSESE(calSESE, nodePost) && nodePost != nodeR)
                {
                    aloneNode[nAloneNode] = nodePost;
                    nAloneNode++;
                }
            }

            int nbond = 0;
            bool[] mark = new bool[Network[currentN].nNode];
            int[] bondsArr = new int[calSESE.Length];
            int nbondsArr = 0;
            int nrigid = 0;
            for (int i = 0; i < nAloneNode; i++)
            {
                if (mark[aloneNode[i]]== false)
                {
                    int[] getRigids = new int[calSESE.Length];
                    int nRigids = 0;
                    getRigids[nRigids] = aloneNode[i]; //add first node to rigids
                    nRigids++;
                    int remainedEdges = 0;
                    bool stop = false;
                    
                    int pathEdges_calibrate = 0;
                    bool is_newSESE = false;
                    bool is_bond = false;             
                    //int[,] adjMatrix = new int[Network[currentN].nNode, Network[currentN].nNode];

                    DFS_Rigids(adjList, ref mark, aloneNode[i], nodeD, nodeR, ref getRigids, ref nRigids);                  

                    if (nRigids < (calSESE.Length - 2)) //if it is not the existing SESE (calsese)
                    {
                        count_Edges(currentN, getRigids, nRigids, ref remainedEdges, nodeD, nodeR); //count all edges in the [getRigids] region.
                        bool[] mark_2 = new bool[Network[currentN].nNode];
                        first_path(currentN, ref pathEdges_calibrate, aloneNode[i], nodeR, ref stop, ref mark_2);
                        if (AjM_Network[nodeD, aloneNode[i]] != 0) pathEdges_calibrate++;
                        if (remainedEdges > pathEdges_calibrate) is_newSESE = true;
                        if (remainedEdges == pathEdges_calibrate) is_bond = true;

                        if (is_newSESE)
                        {
                            getRigids[nRigids] = nodeD;
                            nRigids++;
                            getRigids[nRigids] = nodeR;
                            nRigids++;
                            int[] tempRigids = new int[nRigids];
                            for (int k = 0; k < nRigids; k++) tempRigids[k] = getRigids[k];

                            strSESEInform[] oldSESE = new strSESEInform[SESE[currentSESE].nSESE];
                            for (int k = 0; k < SESE[currentSESE].nSESE; k++) oldSESE[k] = SESE[currentSESE].SESE[k];

                            SESE[currentSESE].SESE = new strSESEInform[SESE[currentSESE].nSESE + 1];
                            for (int k = 0; k < SESE[currentSESE].nSESE; k++) SESE[currentSESE].SESE[k] = oldSESE[k];
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].depth = -1;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].parentSESE = -1;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Entry = nodeD;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Exit = nodeR;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].nNode = nRigids;
                            SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Node = tempRigids;
                            SESE[currentSESE].nSESE++;

                            prop4_C++;
                            nrigid++;
                            //reduce_SESE(currentSESE, SESE[currentSESE].nSESE - 1, -1, -1, ref reduceSESE);
                        }
                        if (!is_newSESE)
                        {
                            nbond++;
                            //store element in getRigids array!!!
                            for (int k = 0; k < nRigids; k++)
                            {
                                bondsArr[nbondsArr]= getRigids[k];
                                nbondsArr++;
                            }
                        }
                    }
                    else
                    {
                        if (nRigids == (calSESE.Length - 2)) break;
                    }
                }
            }
            //we may not need it ===============
            /*if (nbond > 1 && nrigid > 0)
            {
                //make the SESE
                bondsArr[nbondsArr] = nodeD;
                nbondsArr++;
                bondsArr[nbondsArr] = nodeR;
                nbondsArr++;
                int[] tempBonds = new int[nbondsArr];
                for (int k = 0; k < nbondsArr; k++) tempBonds[k] = bondsArr[k];

                strSESEInform[] oldSESE = new strSESEInform[SESE[currentSESE].nSESE];
                for (int k = 0; k < SESE[currentSESE].nSESE; k++) oldSESE[k] = SESE[currentSESE].SESE[k];

                SESE[currentSESE].SESE = new strSESEInform[SESE[currentSESE].nSESE + 1];
                for (int k = 0; k < SESE[currentSESE].nSESE; k++) SESE[currentSESE].SESE[k] = oldSESE[k];
                SESE[currentSESE].SESE[SESE[currentSESE].nSESE].depth = -1;
                SESE[currentSESE].SESE[SESE[currentSESE].nSESE].parentSESE = -1;
                SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Entry = nodeD;
                SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Exit = nodeR;
                SESE[currentSESE].SESE[SESE[currentSESE].nSESE].nNode = nbondsArr;
                SESE[currentSESE].SESE[SESE[currentSESE].nSESE].Node = tempBonds;
                SESE[currentSESE].nSESE++;

                prop4_C++;
            }   
            */
        }



        public void first_path(int currentN, ref int pathEdges_calibrate, int startNode, int nodeR, ref bool stop, ref bool[] mark_2)
        {
            mark_2[startNode] = true;
            for (int i = 0; i < Network[currentN].Node[startNode].nPost; i++)
            {
                if (stop) return;
                if (mark_2[Network[currentN].Node[startNode].Post[i]] == true) continue;
                if (Network[currentN].Node[startNode].Post[i] != nodeR)
                {
                    
                    if (AjM_Network[startNode, Network[currentN].Node[startNode].Post[i]] != 0) pathEdges_calibrate++;
                    first_path(currentN, ref pathEdges_calibrate, Network[currentN].Node[startNode].Post[i], nodeR, ref stop,ref mark_2);
                }
                else
                {
                    stop = true;
                    pathEdges_calibrate++;
                }
            }
        }
        public void DFS_Rigids(int[][] adjList, ref bool[] Mark, int firstNode, int Entry, int Exit, ref int[] getRigids, ref int nRigids)
        {
            Stack stack = new Stack();
            Mark[firstNode] = true;
            stack.Push(firstNode);
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());
                //do nothing
                for (int i = 1; i <= adjList[curNode][0]; i++)
                {
                    int v = adjList[curNode][i];

                    if (!Mark[v] && v != Entry && v != Exit)
                    {
                        stack.Push(v);
                        Mark[v] = true;
                        getRigids[nRigids] = v;
                        nRigids++;
                    }                    
                }
            }
        }
        private void reduce_SESE_Modified(int currentN, int workSESE, int kSESE) //for SESE identification only
        {
            // SESE내 노드만 구성

            for (int j = 0; j < SESE[workSESE].SESE[kSESE].nNode; j++)
            {
                if (SESE[workSESE].SESE[kSESE].Node[j] == SESE[workSESE].SESE[kSESE].Entry) continue;

                Network[currentN].Node[SESE[workSESE].SESE[kSESE].Node[j]].done = true; // entry제외한 sese내 노드 축소
            }

            int sNode = SESE[workSESE].SESE[kSESE].Entry; //Loop[workLoop].Loop[loop].header를 대표 Node 로
            int eNode = SESE[workSESE].SESE[kSESE].Exit; //Loop[workLoop].Loop[loop].header를 대표 Node 로
            //Network[currentN].Node[loopNode].Kind = "XOR"; // 대표 Node는 XOR노드로........



            //대표 Node 정보 변경

            Network[currentN].Node[sNode].Name = "S[" + kSESE.ToString() + "]";
            Network[currentN].Node[sNode].Type_I = "";
            Network[currentN].Node[sNode].Type_II = "";

            for (int k = 0; k < Network[currentN].nLink; k++)
            {
                //대표노드로 부터 SESE내로 나가는 링크 제거

                if (Network[currentN].Link[k].fromNode == sNode)
                {
                    bool inSESE = false;
                    for (int m = 0; m < SESE[workSESE].SESE[kSESE].nNode; m++)
                    {
                        if (Network[currentN].Link[k].toNode == SESE[workSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        Network[currentN].Link[k].fromNode = Network[currentN].Link[k].toNode;
                    }
                }
            }

            for (int k = 0; k < Network[currentN].nLink; k++)
            {
                if (Network[currentN].Link[k].toNode == eNode) continue;

                //대표노드로 부터 SESE내로 나가는 링크 제거
                if (Network[currentN].Link[k].fromNode == eNode)
                {
                    Network[currentN].Link[k].fromNode = sNode;
                }
            }

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].done)
                {
                    Network[currentN].Node[i].nPre = 0;
                    Network[currentN].Node[i].nPost = 0;
                    Network[currentN].Node[i].Pre = null;
                    Network[currentN].Node[i].Post = null;
                }
                else
                {
                    find_NodeInform(currentN, i);
                }
            }

        }
       
        public bool en_ex_inSESE(int[] calSESE, int nodeD, int nodeR)
        {
            bool check_1 = false;
            for (int i = 0; i < calSESE.Length; i++ )
            {
                if (calSESE[i] == nodeD)
                {
                    check_1 = true;
                    break;
                }
            }
            if (check_1)
            {
                for (int i = 0; i < calSESE.Length; i++)
                {
                    if (calSESE[i] == nodeR)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool check_allEdgesInside(int currentN, int nodeD, int nodeR, int[] calSESE, int[,] checkEdges, ref int countEdge)
        {
            countEdge++;
            //first => assign all edges
            bool checkOut = true;
            bool flag = true;
            int curEdge = 0;

            for (int i = 0; i < Network[currentN].Node[nodeD].nPost; i++)
            {
                if (node_insideSESE(calSESE, Network[currentN].Node[nodeD].Post[i]))
                {
                    if (flag) curEdge = checkEdges[nodeD, Network[currentN].Node[nodeD].Post[i]];

                    //if the entry have at least 1 edge to the calSESE which is not consider before => good SESE
                    if (checkEdges[nodeD, Network[currentN].Node[nodeD].Post[i]] != curEdge || checkEdges[nodeD, Network[currentN].Node[nodeD].Post[i]] == 0)
                    {
                        checkOut = false;
                        break;
                    }
                    flag = false;
                }
            }
            for (int i = 0; i < Network[currentN].Node[nodeD].nPre; i++)
            {
                if (node_insideSESE(calSESE, Network[currentN].Node[nodeD].Pre[i]))
                {
                    if (flag) curEdge = checkEdges[nodeD, Network[currentN].Node[nodeD].Pre[i]];

                    //if the entry have at least 1 edge to the calSESE which is not consider before => good SESE
                    if (checkEdges[nodeD, Network[currentN].Node[nodeD].Pre[i]] != curEdge || checkEdges[nodeD, Network[currentN].Node[nodeD].Pre[i]] == 0)
                    {
                        checkOut = false;
                        break;
                    }
                    flag = false;
                }
            }
            //===================================================================
            if (!checkOut)
            {
                for (int i = 0; i < Network[currentN].Node[nodeD].nPost; i++)
                {
                    if (node_insideSESE(calSESE, Network[currentN].Node[nodeD].Post[i]))
                    {
                        checkEdges[nodeD, Network[currentN].Node[nodeD].Post[i]] = countEdge;
                    }
                }
                for (int i = 0; i < Network[currentN].Node[nodeD].nPre; i++)
                {
                    if (node_insideSESE(calSESE, Network[currentN].Node[nodeD].Pre[i]))
                    {
                        checkEdges[nodeD, Network[currentN].Node[nodeD].Pre[i]] = countEdge;
                    }
                }
            }


            if (checkOut) countEdge--;
            return checkOut;
        }
        public bool node_insideSESE(int[] calSESE, int node)
        {
            for (int i = 0; i < calSESE.Length; i++)
            {
                if (calSESE[i] == node)
                    return true;
            }
            return false;
        }
        public void reOrdering_candidate_EnEx(int currentN, ref int[] EntrySESE, ref int[] ExitSESE, int nEntrySESE, int nExitSESE, bool isEntrySet, int maxDepth_DomTree, int maxDepth_PdomTree)
        {
            
            //Re-arrange EntrySESE and ExitSESE basing on the depth of DomTree or PdomTree
            Queue<int> Q = new Queue<int>();
            if (isEntrySet)
            {
                int curDepth = maxDepth_DomTree;
                do
                {
                    for (int i = 0; i < nEntrySESE; i++)
                    {
                        if (Network[currentN].Node[EntrySESE[i]].DepthDom != curDepth) continue;
                        Q.Enqueue(EntrySESE[i]);
                    }
                    curDepth--;
                } while (curDepth > 0);
                EntrySESE = new int[Q.Count];
                Q.CopyTo(EntrySESE, 0);
            }
            else
            {
                int curDepth = maxDepth_PdomTree;
                do
                {
                    for (int i = 0; i < nExitSESE; i++)
                    {
                        if (Network[currentN].Node[ExitSESE[i]].DepthPdom != curDepth) continue;
                        Q.Enqueue(ExitSESE[i]);
                    }
                        curDepth--;
                } while (curDepth > 0);
                ExitSESE = new int[Q.Count];
                Q.CopyTo(ExitSESE, 0);
            }
        }
        public int BFS(int currentN, bool[,] Tree, bool isEntrySet)
        {
            
            Queue<int> Q = new Queue<int>();
            if (isEntrySet)
            {
                Q.Enqueue(Network[currentN].Node[Start].Post[0]);
                Network[currentN].Node[Start].DepthDom = 0;
                Network[currentN].Node[Q.Peek()].DepthDom = 1;
            }
            else
            {
                Q.Enqueue(Network[currentN].Node[End].Pre[0]);
                Network[currentN].Node[End].DepthPdom = 0;
                Network[currentN].Node[Q.Peek()].DepthPdom = 1;
            }
            int maxDepth = 0;
            do
            {
                int u = Q.Dequeue();
                for (int v = 0; v < Network[currentN].nNode; v++)
                {
                    if (isEntrySet && Tree[u, v] == true)
                    {
                        Network[currentN].Node[v].DepthDom = Network[currentN].Node[u].DepthDom + 1;
                        if (Network[currentN].Node[v].DepthDom > maxDepth) maxDepth = Network[currentN].Node[v].DepthDom;
                        Q.Enqueue(v);
                    }
                    else if (!isEntrySet && Tree[v, u] == true)
                    {
                        Network[currentN].Node[v].DepthPdom = Network[currentN].Node[u].DepthPdom + 1;
                        if (Network[currentN].Node[v].DepthPdom > maxDepth) maxDepth = Network[currentN].Node[v].DepthPdom;
                        Q.Enqueue(v);
                    }
                }
            } while (Q.Count != 0);
            return maxDepth;
        }
        public int find_nodeName(int currentN, string name)
        {
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].Kind == name)
                    return i;
            }
            return -1;
        }
        public bool check_path_JoinSplit(int currentN, bool[,] Tree, int join, int split)
        {
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if ((Network[currentN].Node[i].nPost == 1 && Network[currentN].Node[i].nPre == 1)|| (i == split))
                {
                    if (Tree[join, i] == true && i != split)
                    {
                        return check_path_JoinSplit(currentN, Tree, i, split);
                    }
                    if (Tree[join, i] == true && i == split)
                        return true;
                }
            }
            return false;
        }

        public bool isIn_DomEI(int currentN, int nodeD, int nodeR, bool isDomEI)
        {
            if (isDomEI)
            {
                for (int i = 0; i < Network[currentN].Node[nodeD].nDomEI; i++)
                {
                    if (nodeR == Network[currentN].Node[nodeD].DomEI[i])
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Network[currentN].Node[nodeR].nDomRevEI; i++)
                {
                    if (nodeD == Network[currentN].Node[nodeR].DomRevEI[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //create adjacency List
        public void adjList_Create(int currentN, ref int[][] adjList) //must use the last index of each List ( <= N)
        {
            int nNode = Network[currentN].nNode;
            int nLink = Network[currentN].nLink;
            adjList = new int[nNode][];
            //initiate each List
            for (int i = 0; i < nNode; i++)
            {
                adjList[i] = new int[nNode];
                adjList[i][0] = 0;
            }
            int fromNode, toNode;
            for (int i = 0; i < nLink; i++)
            {
                fromNode = Network[currentN].Link[i].fromNode;
                toNode = Network[currentN].Link[i].toNode;
                adjList[fromNode][0]++;
                adjList[fromNode][adjList[fromNode][0]] = toNode;

                adjList[toNode][0]++;
                adjList[toNode][adjList[toNode][0]] = fromNode;
            }
        }
        public void adjList_Directed_Create(int currentN, ref int[][] adjList) //must use the last index of each List ( <= N)
        {
            int nNode = Network[currentN].nNode;
            int nLink = Network[currentN].nLink;
            adjList = new int[nNode][];
            //initiate each List
            for (int i = 0; i < nNode; i++)
            {
                adjList[i] = new int[nNode];
                adjList[i][0] = 0;
            }
            int fromNode, toNode;
            for (int i = 0; i < nLink; i++)
            {
                fromNode = Network[currentN].Link[i].fromNode;
                toNode = Network[currentN].Link[i].toNode;
                adjList[fromNode][0]++;
                adjList[fromNode][adjList[fromNode][0]] = toNode;

                //adjList[toNode][0]++;
                //adjList[toNode][adjList[toNode][0]] = fromNode;
            }

        }

        public void DFS(int[][] adjList, ref bool[] Mark, int index, int[] SourceOfError, int[] DecendantOfError, int Entry, int Exit)
        {
            Stack stack = new Stack();
            Mark[DecendantOfError[index]] = true;
            stack.Push(DecendantOfError[index]);
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());               
                //do nothing
                for (int i = 1; i <= adjList[curNode][0]; i++)
                {
                    int v = adjList[curNode][i];
                    if (!Mark[v] && v != Entry && v!= Exit && v!= SourceOfError[index])
                    {
                        stack.Push(v);
                        Mark[v] = true;
                    }
                }
            }
        }

        //Check a node in SESE region have Intermediate Dom or Pdom is inside this region or NOT
        public bool check_in_Tree(bool[,] makeLink, int nNode, int[] calSESE, int k, bool DomTree, ref int sourceNode) //DomTree = true / False
        {
            if (DomTree)
            {
                int IDom_k = -1;
                for (int i = 0; i < nNode; i++)
                {
                    if (makeLink[i, calSESE[k]] == true)
                    {
                        IDom_k = i;
                        sourceNode = IDom_k;
                        break;
                    }
                }
                for (int i = 0; i < calSESE.Length; i++)
                {
                    if (calSESE[i] == IDom_k)
                    {
                        return true; //This intermediate Dominator is inside the SESE region
                    }
                }
            }
            else
            {
                int IPdom_k = -1;
                for (int i = 0; i < nNode; i++)
                {
                    if (makeLink[calSESE[k], i] == true) //we need reverse the edges
                    {
                        IPdom_k = i;
                        sourceNode = IPdom_k;
                        break;
                    }
                }
                for (int i = 0; i < calSESE.Length; i++)
                {
                    if (calSESE[i] == IPdom_k)
                    {
                        return true; //This intermediate Dominator is inside the SESE region
                    }
                }
            }
            return false; //This intermediate Dominator is NOT inside the SESE region
        }
        //check nodeD, nodeR belong 1 loop or not
        public bool node_in_SameLoop(int workLoop, int nodeD, int nodeR)
        {
            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                if (Node_In_Loop(workLoop, nodeD, i) && Node_In_Loop(workLoop, nodeR, i))
                {
                    return true;
                }
            }
            return false;
        }


        public int[] remove_node_in_Array(int[] Arr, bool[] checkArr)
        {
            int nArr = Arr.Length;
            int[] tempArr;
            int[] temp_1 = Arr;
            int ntemp_1 = 0;
            for (int i = 0; i < nArr; i++)
            {
                if (checkArr[i] == false)
                {
                    temp_1[ntemp_1] = Arr[i];
                    ntemp_1++;
                }
            }

            tempArr = new int[ntemp_1];
            for (int i = 0; i < ntemp_1; i++)
            {
                tempArr[i] = temp_1[i];
            }
            return tempArr;
        }

        private void modify_SESE_Hierarchy(int currentSESE)
        {
            int maxDepth = SESE[currentSESE].maxDepth;
            int nSESE = SESE[currentSESE].nSESE;
            int[] child = new int[nSESE];
            int numOfChild = 0;
            int numOfRemove = 0;
            int curDepth = 1;
            while (curDepth <= maxDepth)
            {
                for (int i = 0; i < nSESE; i++)
                {
                    if (SESE[currentSESE].SESE[i].depth != curDepth) continue;
                    //if SESE have 2 child and check sum ok (output: true false, numOfChild/  input: currentSESE, i)
                    if (checkDuplicatedSESE(currentSESE, i,ref numOfChild,ref child, nSESE))
                    {
                        for (int j = 0; j < numOfChild; j++)
                        {
                            SESE[currentSESE].SESE[child[j]].parentSESE = SESE[currentSESE].SESE[i].parentSESE;

                        }
                        //remove SESE[i](nSESE, numOfRemove)                        
                        removeSESE(currentSESE, i, ref numOfRemove, ref nSESE);

                        //Change Depth of all children
                        bool[] visited = new bool[nSESE];
                        for (int b = 0; b < nSESE; b++) visited[b] = false;
                            for (int j = 0; j < numOfChild; j++)
                            {
                                try_Change_SESEDepth(currentSESE, child[j], nSESE, ref visited);
                            }
                    }
                    numOfChild = 0;
                }
                curDepth++;
            }
            //update nSESE - numOfRemove
            SESE[currentSESE].nSESE = SESE[currentSESE].nSESE - numOfRemove;
        }
        private void try_Change_SESEDepth(int currentSESE, int i, int nSESE, ref bool[] visited)
        {
            for (int j = 0; j < nSESE; j++ )
            {
                if (SESE[currentSESE].SESE[j].parentSESE == i && visited[j] == false)
                {
                    SESE[currentSESE].SESE[j].depth--;
                    visited[j] = true;
                    try_Change_SESEDepth(currentSESE, j, nSESE, ref visited);
                }
            }
        }

        private bool checkDuplicatedSESE(int currentSESE, int i, ref int numOfChild, ref int[] child, int nSESE)
        {
            int count = 0;
            int countTotal = 0;

            for (int m = 0; m < SESE[currentSESE].SESE[i].nNode; m++)
            {
                 int node = SESE[currentSESE].SESE[i].Node[m];
                 if (Network[finalNet].Node[node].nPre > 1 || Network[finalNet].Node[node].nPost > 1)
                 {
                     countTotal++;
                 }
            }

            for (int k = 0; k < nSESE; k++)
            {
                if (SESE[currentSESE].SESE[k].parentSESE == i)
                {
                    for (int m = 0; m < SESE[currentSESE].SESE[k].nNode; m++)
                    {
                        int node = SESE[currentSESE].SESE[k].Node[m];
                        if (Network[finalNet].Node[node].nPre > 1 || Network[finalNet].Node[node].nPost > 1)
                        {
                            count++;
                        }
                    }
                    
                }
            }
            if (count == countTotal)
            {
                return true;
            }
            else
                return false;
        }
        private void removeSESE(int currentSESE, int i, ref int numOfRemove, ref int nSESE)
        {
            for (int k = i; k < nSESE; k++)
            {
                if (k < nSESE - 1)
                {
                    SESE[currentSESE].SESE[k] = SESE[currentSESE].SESE[k + 1];
                }
            }
            nSESE--;
            numOfRemove++;
        }


        //Check all sucessors of sNode are in this SESE
        private bool check_allIn(int currentN, int currentSESE, int kNode, int kSESE, bool isPost)
        {
            int count;
            int[] cNode;
            //Check whether all sucessors or predecessors are in this SESE by this flag variable. (isPost)
            if (isPost)
            {
                count = Network[currentN].Node[kNode].nPost;
                cNode = Network[currentN].Node[kNode].Post; //cNode[] store all the sucessors of node "kNode"
            }
            else
            {
                count = Network[currentN].Node[kNode].nPre;
                cNode = Network[currentN].Node[kNode].Pre; //cNode[] store all the predecessor of node "kNode"
            }

            bool allIn = true;
            for (int k = 0; k < count; k++)
            {
                bool inSESE = false;
                for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
                {
                    if (cNode[k] == SESE[currentSESE].SESE[kSESE].Node[m])
                    {
                        inSESE = true;
                        break;
                    }
                }

                if (!inSESE)
                {
                    allIn = false;
                    break;
                }
            }
            return allIn;
        }

        private void Type_III_Split_Entry(int currentN, int currentSESE, int kSESE, int addNum, int sNode, int cNode, string sName)
        {
            extent_Network(currentN, addNum);

            int addNode = Network[currentN].nNode - addNum;
            // Node - 추가
            //Network[currentN].Node[addNode].Kind = Network[currentN].Node[cNode].Kind;
            Network[currentN].Node[addNode].Kind = Network[currentN].Node[sNode].Kind; //new change => to keep the node after split same it father.
            Network[currentN].Node[addNode].orgNum = sNode;
            Network[currentN].Node[addNode].parentNum = addNode;
            Network[currentN].Node[addNode].Special = "";
            if (sName == "SSD")
            {
                Network[currentN].Node[addNode].Name = "SSD";// addNode.ToString();
                Network[currentN].Node[addNode].Type_I = "";
            }
            else
            {
                Network[currentN].Node[addNode].Name = Network[currentN].Node[sNode].Name;// sNode.ToString();
                Network[currentN].Node[addNode].Type_I = Network[currentN].Node[sNode].Type_I;
                Network[currentN].Node[addNode].Type_II = Network[currentN].Node[sNode].Type_II + "_se";
            }



            int addLink = Network[currentN].nLink - addNum;
            //Link 추가
            Network[currentN].Link[addLink].fromNode = sNode;
            Network[currentN].Link[addLink].toNode = addNode;

            //기존 Link 정보 변경
            //Noi link trong SESE
            for (int k = 0; k < Network[currentN].nLink - addNum; k++)
            {
                if (Network[currentN].Link[k].fromNode == sNode)
                {
                    bool inSESE = false;
                    for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        //Neu moi Link K trong SESE => inSESE = true => tạo kết nối với addNode 
                        if (Network[currentN].Link[k].toNode == SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        Network[currentN].Link[k].fromNode = addNode;
                        find_NodeInform(currentN, Network[currentN].Link[k].toNode);
                    }
                }
                if (Network[currentN].Link[k].toNode == sNode)
                {
                    bool inSESE = false;
                    for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        if (Network[currentN].Link[k].fromNode == SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        Network[currentN].Link[k].toNode = addNode;
                        find_NodeInform(currentN, Network[currentN].Link[k].fromNode);
                    }
                }
            }

            find_NodeInform(currentN, addNode); //find the new predecessors and sucessor of this node
            find_NodeInform(currentN, sNode); //find the new predecessors and sucessor of this node => after split type III into 2 node (sNode and addNode)

            // SESE정보변경
            // 자신정보 변경
            //Đổi entry bằng addNode cho SESE
            SESE[currentSESE].SESE[kSESE].Entry = addNode;
            for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
            {
                if (SESE[currentSESE].SESE[kSESE].Node[m] == sNode)
                {
                    SESE[currentSESE].SESE[kSESE].Node[m] = addNode;
                    break;
                }
            }

            //parent정보 변경
            if (SESE[currentSESE].SESE[kSESE].parentSESE != -1)
            {
                Add_Parent_SESE(currentSESE, SESE[currentSESE].SESE[kSESE].parentSESE, addNum, addNode);
            }

            //==== ADD CODE HERE ==========
            //We need to check whether this SESE (currentSESE and kSESE) is a simple strucuture or not.
            //if it is a simple structure we will find the way to change the SESE Entry gateway.
            //A simple structure is a structure which have 2 gateways, Entry and Exit (of SESE)
            //So First, we will check SESE is simple structure, Second, we will change the gateway type
            
            //verifySimpleStructure(currentN, currentSESE, kSESE, SESE[currentSESE].SESE[kSESE].Entry, SESE[currentSESE].SESE[kSESE].Exit, true);    

            //==== END OF NEW CODE ========

        }

        //Flag == True (for node SS)
        //Flag == False (for node EE)
        private void verifySimpleStructure(int currentN, int currentSESE,int kSESE, int entrySESE, int exitSESE, bool flag)
        {

            int index;
            string currName = null;
            if (flag == true)
            {
                index = Network[currentN].Node[entrySESE].Name.Length;
                if (index > 1)
                    currName = Network[currentN].Node[entrySESE].Name.Substring(0, 2);
                if (isSimpleStructure(currentN, currentSESE, kSESE) == true && currName == "SS")
                {

                    Network[currentN].Node[entrySESE].Kind = Network[currentN].Node[exitSESE].Kind;
                }
            }
            else
            {
                index = Network[currentN].Node[exitSESE].Name.Length;
                if (index > 1)
                    currName = Network[currentN].Node[exitSESE].Name.Substring(0, 2);
                if (isSimpleStructure(currentN, currentSESE, kSESE) == true && currName == "EE")
                {

                    Network[currentN].Node[exitSESE].Kind = Network[currentN].Node[entrySESE].Kind;
                    
                }
            }
        }

        private bool isSimpleStructure(int currentN, int currentSESE, int kSESE)
        {
            int nNodeSESE = SESE[currentSESE].SESE[kSESE].nNode;
            int countNode = 0;
            int currNode;
            for (int i = 0; i < nNodeSESE; i++)
            {
                currNode = SESE[currentSESE].SESE[kSESE].Node[i];

                if (Network[currentN].Node[currNode].Kind == "OR" || Network[currentN].Node[currNode].Kind == "XOR" || Network[currentN].Node[currNode].Kind == "AND")
                    countNode = countNode + 1;
            }
            if (countNode > 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Add_Parent_SESE(int currentSESE, int pSESE, int addNum, int addNode)
        {
            int[] imSESE = SESE[currentSESE].SESE[pSESE].Node;

            SESE[currentSESE].SESE[pSESE].nNode += addNum;
            SESE[currentSESE].SESE[pSESE].Node = new int[SESE[currentSESE].SESE[pSESE].nNode];
            for (int m = 0; m < imSESE.Length; m++) SESE[currentSESE].SESE[pSESE].Node[m] = imSESE[m];
            SESE[currentSESE].SESE[pSESE].Node[SESE[currentSESE].SESE[pSESE].nNode - addNum] = addNode;

            if (SESE[currentSESE].SESE[pSESE].parentSESE != -1)
            {
                Add_Parent_SESE(currentSESE, SESE[currentSESE].SESE[pSESE].parentSESE, addNum, addNode);
            }
        }


        private void Type_III_Split_Exit(int currentN, int currentSESE, int kSESE, int addNum, int eNode, int cNode)
        {
            extent_Network(currentN, addNum);

            int addNode = Network[currentN].nNode - addNum;
            if (addNode == 84)
            {

            }
            // Node - 추가
            Network[currentN].Node[addNode].Kind = Network[currentN].Node[cNode].Kind;
            Network[currentN].Node[addNode].orgNum = eNode;
            Network[currentN].Node[addNode].parentNum = addNode;
            Network[currentN].Node[addNode].Special = "";

            Network[currentN].Node[addNode].Name = Network[currentN].Node[eNode].Name;// eNode.ToString();
            Network[currentN].Node[addNode].Type_I = Network[currentN].Node[eNode].Type_I;
            Network[currentN].Node[addNode].Type_II = Network[currentN].Node[eNode].Type_II + "_sx";


            int addLink = Network[currentN].nLink - addNum;
            //Link 추가
            Network[currentN].Link[addLink].fromNode = addNode;
            Network[currentN].Link[addLink].toNode = eNode;

            //기존 Link 정보 변경
            for (int k = 0; k < Network[currentN].nLink - addNum; k++)
            {
                if (Network[currentN].Link[k].toNode == eNode) //For normal SESE (Acyclic)
                {
                    bool inSESE = false;
                    for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        if (Network[currentN].Link[k].fromNode == SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        Network[currentN].Link[k].toNode = addNode;
                        find_NodeInform(currentN, Network[currentN].Link[k].fromNode);
                    }
                }
                if (Network[currentN].Link[k].fromNode == eNode) //For special SESE (Weird NL)
                {
                    bool inSESE = false;
                    for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        if (Network[currentN].Link[k].toNode == SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        Network[currentN].Link[k].fromNode = addNode;
                        find_NodeInform(currentN, Network[currentN].Link[k].toNode);
                    }
                }
            }

            find_NodeInform(currentN, addNode);
            find_NodeInform(currentN, eNode);

            // SESE정보변경
            // 자신정보 변경
            SESE[currentSESE].SESE[kSESE].Exit = addNode;
            for (int m = 0; m < SESE[currentSESE].SESE[kSESE].nNode; m++)
            {
                if (SESE[currentSESE].SESE[kSESE].Node[m] == eNode)
                {
                    SESE[currentSESE].SESE[kSESE].Node[m] = addNode;
                    break;
                }
            }

            //parent정보 변경
            if (SESE[currentSESE].SESE[kSESE].parentSESE != -1)
            {
                Add_Parent_SESE(currentSESE, SESE[currentSESE].SESE[kSESE].parentSESE, addNum, addNode);
            }

            //Do the same entry of SESE => We check simple structure of this current SESE and change the gateway type
            //verifySimpleStructure(currentN, currentSESE, kSESE, SESE[currentSESE].SESE[kSESE].Entry, SESE[currentSESE].SESE[kSESE].Exit, false);

        }

        public bool check_StartEvent_InsideRigid(int currentSESE, int sese)
        {
            //after BondCheck;
            for (int i = 0; i < SESE[currentSESE].SESE[sese].nNode; i++)
            {
                for (int j = 0; j < nSTART_EVENT; j++)
                {
                    if (SESE[currentSESE].SESE[sese].Node[i] == START_EVENT[j])
                        return true;
                }
            }
            return false;
        }
        private void Start_Split(int currentN, int currentSESE, bool isRigidSplit) //isRigidSplit is used for flag to determine whether to split entry of rigid containing start events or not!!
        {
            bool mStop = false;
            int sNode;
            int eNode;
            bool checkEnRigid = true; //check split or not

            int curDepth = SESE[currentSESE].maxDepth;
            do
            {
                for (int j = 0; j < SESE[currentSESE].nSESE; j++) //Visit all the SESE flow
                {
                    if (SESE[currentSESE].SESE[j].depth != curDepth) continue;  //Consider the depthest SESE

                    //check current SESE is rigid or not?
                    if (isRigidSplit == false)
                    {
                        if (!Bond_Check(currentN, currentSESE, j))
                        {
                            if (check_StartEvent_InsideRigid(currentSESE, j))
                            {
                                checkEnRigid = false;
                            }
                        }
                    }

                    sNode = SESE[currentSESE].SESE[j].Entry;
                    eNode = SESE[currentSESE].SESE[j].Exit;

                    //check if all sucessor of sNode in currentSESE
                    if (check_SESE_Entry(currentN, currentSESE, sNode, j)) // 단순히 Kind만 변경
                    {
                        //Network[currentN].Node[sNode].Kind = Network[currentN].Node[eNode].Kind;
                        //mStop = true;
                        //Verify simple structure for node SS (also the entry of SESE)
                        //verifySimpleStructure(currentN, currentSESE, j, SESE[currentSESE].SESE[j].Entry, SESE[currentSESE].SESE[j].Exit, true); 

                    }
                    else //노드, 링크 추가
                    {
                        if (checkEnRigid) //if rigid have start event, dont split entry
                        {
                            //Type_III_Split_Entry(currentN, currentSESE, j, 1, sNode, eNode, "SSD");
                            Type_III_Split_Entry(currentN, currentSESE, j, 1, sNode, eNode, "");
                        }
                        
                    }
                    //New============================================================================
                    if (check_SESE_Entry(currentN, currentSESE, eNode, j)) // 단순히 Kind만 변경
                    {
                        //Network[currentN].Node[eNode].Kind = Network[currentN].Node[sNode].Kind;
                        //mStop = true;
                        //verifySimpleStructure(currentN, currentSESE, j, SESE[currentSESE].SESE[j].Entry, SESE[currentSESE].SESE[j].Exit, false); 
                    }
                    else //노드, 링크 추가
                    {
                        Type_III_Split_Exit(currentN, currentSESE, j, 1, eNode, eNode); //New Change

                    }
                    //New============================================================================


                }

                curDepth--;
            } while (curDepth > 0 && !mStop);

            find_Dom(currentN); //Dom
            find_DomRev(currentN); //Postdom

            find_DomEI(currentN, -1); //SS split node만..... //eDom^-1(NodeD)
            find_DomRevEI(currentN); // join node만..... //ePdom^-1(NodeR)

        }

        public bool check_SESE_Entry(int currentN, int currentSESE, int curNode, int curSESE) //check outter edges of an entrySESE or exitSESE (if more than 1 => should split)
        {
            int count_outterEdges = 0;
            for (int i = 0; i < Network[currentN].Node[curNode].nPost; i++)
            {               
                int node = Network[currentN].Node[curNode].Post[i];
                if (!node_insideSESE(SESE[currentSESE].SESE[curSESE].Node, node))
                    count_outterEdges++;
            }
            for (int i = 0; i < Network[currentN].Node[curNode].nPre; i++)
            {
                int node = Network[currentN].Node[curNode].Pre  [i];
                if (!node_insideSESE(SESE[currentSESE].SESE[curSESE].Node, node))
                    count_outterEdges++;
            }
            if (count_outterEdges == 1) return true;
            else
                return false;
        }

        #endregion


        #region sub 네트워크 만들기

        private void make_subNetwork(int fromN, int toN, int workLoop, int loop, string Type, int entryH) // Type = "SESE"일때는  workLoop, loop 가 SESE 지칭 
        {

            // 포함 Node 찾기
            find_includeNode(fromN, workLoop, loop, Type, entryH); //===> we will have "searchNode" here and "nsearchNode" 

            //Loop[workLoop].Loop내 포함된 모든 노드 찾아서
            if (Type == "CI" || Type == "II")
            {
                make_subNode(fromN, toN, true, "START", true, "END");

                make_subLink(fromN, toN, workLoop, loop, Type, true, Loop[workLoop].Loop[loop].Entry, true, Loop[workLoop].Loop[loop].Exit, entryH);
            }
            else if (Type == "CC") //Chi la them link gia, node gia thoi! => Moi thu deu dc lam o find_includeNode()
            {
                make_subNode(fromN, toN, false, "", true, "XOR"); //Make subnetwork (Network[6]) => add node

                make_subLink(fromN, toN, workLoop, loop, Type, false, null, true, Loop[workLoop].Loop[loop].Entry, entryH); //=: make subnetwork => add link
            }
            else if (Type == "ICF" || Type == "ICB" || Type == "ICC") //For irreducible loop (concurrent entry sets)
            {
                make_subNode(fromN, toN, false, "", false, "");

                make_subLink(fromN, toN, workLoop, loop, Type, false, null, false, null, entryH);
            }
            else if (Type == "IR")
            {
                make_subNode(fromN, toN, true, "START", true, "END");

                make_subLink(fromN, toN, workLoop, loop, Type, true, Loop[workLoop].Loop[loop].Entry, true, Loop[workLoop].Loop[loop].Exit, entryH);
            }

            //============= make Forward Flow network ================================
            else if (Type == "FF") 
            {
                int num = 0;
                int[] imNode = new int[Network[fromN].nNode];

                // --- real terminal 찾기
                for (int i = 0; i < nSearchNode; i++)
                {
                    int fromNode = searchNode[i];
                    if (Network[fromN].Node[fromNode].Special != "T") continue;

                    bool bIn = false;
                    for (int k = 0; k < Network[fromN].Node[fromNode].nPost; k++)
                    {
                        int toNode = Network[fromN].Node[fromNode].Post[k];

                        bool isBack = false;
                        for (int j = 0; j < Network[fromN].nLink; j++)
                        {
                            if (Network[fromN].Link[j].fromNode == fromNode && Network[fromN].Link[j].toNode == toNode)
                            {
                                if (Network[fromN].Link[j].bBackS) isBack = true;

                                break;
                            }
                        }

                        if (isBack) continue;

                        for (int j = 0; j < nSearchNode; j++)
                        {
                            if (toNode == searchNode[j])
                            {
                                bIn = true;
                                break;
                            }
                        }
                        if (bIn) break;
                    }

                    if (!bIn) // real terminal
                    {
                        imNode[num] = fromNode;
                        num++;
                    }
                }
                if (num <= 1)
                {
                    make_subNode(fromN, toN, false, "", false, "");
                    make_subLink(fromN, toN, workLoop, loop, Type, false, null, false, null, entryH);

                }
                else
                {
                    int[] dNode = new int[num];
                    for (int k = 0; k < num; k++) dNode[k] = imNode[k]; //filter all the appropriated link in this searchNode[] (fromNode-toNode) and storing in dNode[]

                    make_subNode(fromN, toN, false, "", true, "XOR"); //sDummy = true => Add node VS; eDummy = true => Add node VE
                    make_subLink(fromN, toN, workLoop, loop, Type, false, null, true, dNode, entryH);
                }
            }
            //============================= End FF ============================
            else if (Type == "BF")
            {
                int num = 0;
                int[] imNode = new int[Network[fromN].nNode];

                for (int j = 0; j < Loop[workLoop].Loop[loop].nNode; j++)
                {

                    if (Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].Special == "T" || Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].Special == "B")
                    {
                        for (int k = 0; k < Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].nPost; k++)
                        {
                            int backNode = Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].Post[k];
                            bool bIn = false;
                            for (int kk = 0; kk < nSearchNode; kk++)
                            {
                                if (searchNode[kk] == backNode)
                                {
                                    bIn = true;
                                    break;
                                }
                            }

                            if (bIn)
                            {
                                imNode[num] = Loop[workLoop].Loop[loop].Node[j];
                                num++;
                            }
                        }
                    }
                }

                for (int k = 0; k < num; k++)
                {
                    searchNode[nSearchNode] = imNode[k];
                    nSearchNode++;
                }

                if (num <= 1)
                {
                    make_subNode(fromN, toN, false, "", false, "");
                    make_subLink(fromN, toN, workLoop, loop, Type, false, null, false, null, entryH);

                }
                else
                {
                    int[] dNode = new int[num];
                    for (int k = 0; k < num; k++) dNode[k] = imNode[k];

                    make_subNode(fromN, toN, true, "XOR", false, "");
                    make_subLink(fromN, toN, workLoop, loop, Type, true, dNode, false, null, entryH);
                }


            }
            else if (Type == "AC")
            {
                make_subNode(fromN, toN, false, "", false, "");
                make_subLink(fromN, toN, workLoop, loop, Type, false, null, false, null, entryH);
            }
            else if (Type == "SESE")
            {
                make_subNode(fromN, toN, true, "START", true, "END");
                int[] sNode = new int[1];
                sNode[0] = SESE[workLoop].SESE[loop].Entry;
                int[] eNode = new int[1];
                eNode[0] = SESE[workLoop].SESE[loop].Exit;
                make_subLink(fromN, toN, workLoop, loop, Type, true, sNode, true, eNode, entryH);
            }


            // Main purpose of this function is creating the Network[toN] (a subNetwork) for verification (instance flow) - just guess
            for (int i = 0; i < Network[toN].nNode; i++)
            {
                find_NodeInform(toN, i);
            }

        }

        private void find_includeNode(int fromN, int workLoop, int loop, string Type, int entryH) //SearchNode[] will be cleared when use this function.
        {

            nSearchNode = 0;
            searchNode = new int[Network[fromN].nNode];

            if (Type == "CI" || Type == "II") //Loop[workLoop].Loop[loop]내 포함된 모든 노드 찾아서
            {
                searchNode[nSearchNode] = Loop[workLoop].Loop[loop].header;
                nSearchNode++;
                find_LoopNode(workLoop, loop);
            }
            else if (Type == "CC") //concurrent entries
            {
                int[] calDom = null;
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    calDom = find_Intersection(Network[fromN].nNode, calDom, Network[fromN].Node[Loop[workLoop].Loop[loop].Entry[k]].Dom);
                }

                if (calDom.Length > 0)
                {
                    int header = calDom[calDom.Length - 1];
                    searchNode[nSearchNode] = header;
                    nSearchNode++;

                    for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                    {
                        find_Reach(fromN, workLoop, loop, header, Loop[workLoop].Loop[loop].Entry[k], Type); //find reach from HEADER to ENTRY[k]

                        searchNode[nSearchNode] = Loop[workLoop].Loop[loop].Entry[k];
                        nSearchNode++;
                    }
                }
            }
            else if (Type == "ICF")
            {
                // 일단 Concurrency Entry
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    if (Loop[tempLoop].Loop[loop].Concurrency[k] != entryH) continue;

                    searchNode[nSearchNode] = Loop[tempLoop].Loop[loop].Entry[k];
                    nSearchNode++;
                }

                for (int k = 0; k < nSearchNode_P; k++)
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == searchNode_P[k])
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (bSame) continue;

                    searchNode[nSearchNode] = searchNode_P[k];
                    nSearchNode++;
                }
                for (int k = 0; k < nSearchNode_F; k++)
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == searchNode_F[k])
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (bSame) continue;

                    searchNode[nSearchNode] = searchNode_F[k];
                    nSearchNode++;
                }
            }
            else if (Type == "ICB") //This is PdFlow ================
            {
                // 일단 Concurrency Entry
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    if (Loop[tempLoop].Loop[loop].Concurrency[k] != entryH) continue;

                    searchNode[nSearchNode] = Loop[tempLoop].Loop[loop].Entry[k];
                    nSearchNode++;
                }

                for (int k = 0; k < nSearchNode_B; k++)
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == searchNode_B[k])
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (bSame) continue;

                    searchNode[nSearchNode] = searchNode_B[k];
                    nSearchNode++;
                }
                for (int k = 0; k < nSearchNode_F; k++)
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == searchNode_F[k])
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (bSame) continue;

                    searchNode[nSearchNode] = searchNode_F[k];
                    nSearchNode++;
                }
            }
            else if (Type == "ICC")
            {
                // 일단 Concurrency Entry
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //Duyet qua cac entries cua current Loop
                {
                    if (Loop[tempLoop].Loop[loop].Concurrency[k] != entryH) continue;

                    searchNode[nSearchNode] = Loop[tempLoop].Loop[loop].Entry[k];
                    nSearchNode++;
                }

                for (int k = 0; k < nSearchNode_F; k++)
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == searchNode_F[k])
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (bSame) continue;

                    searchNode[nSearchNode] = searchNode_F[k];
                    nSearchNode++;
                }
            }
            else if (Type == "FF") // ForwardFlow - Natural Loop
            {
                int rootDepth = Network[fromN].Node[Loop[workLoop].Loop[loop].header].depth;

                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........
                //searchNode[] is used for storing the node of Forward Flow or Backward Flow (it will be erase when we find the node include "find_nodeInclude")
                searchNode[nSearchNode] = Loop[workLoop].Loop[loop].header;
                nSearchNode++;
                for (int j = 0; j < Loop[workLoop].Loop[loop].nNode; j++) //visit all node from the current loop "loop"
                {
                    if (Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].depth == rootDepth && Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].nPost > 0 
                        && Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].nPre > 0)
                    {
                        searchNode[nSearchNode] = Loop[workLoop].Loop[loop].Node[j];
                        nSearchNode++;

                    }
                }

            }
            else if (Type == "BF") // BorwardFlow - Natural Loop
            {
                int rootDepth = Network[fromN].Node[Loop[workLoop].Loop[loop].header].depth;

                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........
                for (int j = 0; j < Loop[workLoop].Loop[loop].nNode; j++)
                {
                    if (Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].depth > rootDepth && Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].nPost > 0 && Network[fromN].Node[Loop[workLoop].Loop[loop].Node[j]].nPre > 0)
                    {
                        searchNode[nSearchNode] = Loop[workLoop].Loop[loop].Node[j];
                        nSearchNode++;
                    }
                }

                searchNode[nSearchNode] = Loop[workLoop].Loop[loop].header;
                nSearchNode++;
            }
            else if (Type == "IR") // TotalFlow - Irreducible Loop
            {
                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........
                searchNode[nSearchNode] = Loop[workLoop].Loop[loop].header;
                nSearchNode++;
                for (int j = 0; j < Loop[workLoop].Loop[loop].nNode; j++)
                {
                    searchNode[nSearchNode] = Loop[workLoop].Loop[loop].Node[j];
                    nSearchNode++;

                }

            }
            else if (Type == "AC") // acyclic Flow 
            {
                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........

                for (int j = 0; j < Network[fromN].nNode; j++)
                {
                    if (Network[fromN].Node[j].done) continue;

                    searchNode[nSearchNode] = j;
                    nSearchNode++;

                }

            }
            else if (Type == "SESE") // acyclic Flow 
            {
                for (int j = 0; j < SESE[workLoop].SESE[loop].nNode; j++)
                {
                    if (Network[fromN].Node[SESE[workLoop].SESE[loop].Node[j]].done) continue;

                    searchNode[nSearchNode] = SESE[workLoop].SESE[loop].Node[j];
                    nSearchNode++;

                }

            }

        }

        public void make_VirtualSTART_END(int currentN)
        {

        }

        //make_subNode(fromN, toN, false, "", true, "XOR");
        private void make_subNode(int fromN, int toN, bool sDummy, string sType, bool eDummy, string eType) //fromN ~ From Netwrok; toN ~ To Network
        {
            //Sub Network 구성
            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;

            Network[toN] = new strNetwork();
            Network[toN].nNode = nSearchNode + addNum;
            Network[toN].Node = new strNode[Network[toN].nNode];
            for (int i = 0; i < nSearchNode; i++)
            {
                Network[toN].Node[i] = Network[fromN].Node[searchNode[i]];
                Network[toN].Node[i].orgNum = searchNode[i];
            }
            // Add details of new node (node VS) in BFlow (JUST for Natural Loop)
            if (sDummy)
            {
                Network[toN].Node[Network[toN].nNode - addNum].Kind = sType;
                Network[toN].Node[Network[toN].nNode - addNum].Name = "DM";
                Network[toN].Node[Network[toN].nNode - addNum].parentNum = -1;
                Network[toN].Node[Network[toN].nNode - addNum].orgNum = -1;// "D";
                Network[toN].Node[Network[toN].nNode - addNum].Special = "";

                addNum--;
            }
            //Add type of the new node (node VE) in FFlow (JUST for Natural Loop)
            if (eDummy)
            {
                Network[toN].Node[Network[toN].nNode - 1].Kind = eType;
                Network[toN].Node[Network[toN].nNode - 1].Name = "DM";
                Network[toN].Node[Network[toN].nNode - addNum].parentNum = -1;
                Network[toN].Node[Network[toN].nNode - 1].orgNum = -1;// "D";
                Network[toN].Node[Network[toN].nNode - 1].Special = "";
            }

            //Network[toN].header = Network[toN].nNode - 2;
        }

        private void make_subLink(int fromN, int toN, int workLoop, int loop, string Type, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int entryH)
        {
            strLink[] imLink = new strLink[Network[fromN].nLink + 2];
            int imNum = 0;
            for (int i = 0; i < Network[fromN].nLink; i++)
            {
                if (!check_addLink(fromN, workLoop, loop, i, Type)) continue;

                int nFrom = -1;
                int nTo = -1;

                for (int k = 0; k < nSearchNode; k++)
                {
                    if (Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (Network[fromN].Link[i].toNode == searchNode[k])
                    {
                        //if (Type == "FF" && !eDummy && Network[fromN].Node[searchNode[k]].Special == "E") { }
                        //else if (Type == "BF" && !sDummy && (Network[fromN].Node[searchNode[k]].Special == "T" || Network[fromN].Node[searchNode[k]].Special == "B")) { }
                        //else
                        //{
                        //    nTo = k;
                        //}
                        if (Type == "FF" && Network[fromN].Node[searchNode[k]].Special == "E") { }
                        else if (Type == "BF" && (Network[fromN].Node[searchNode[k]].Special == "T" || Network[fromN].Node[searchNode[k]].Special == "B")) { }
                        else
                        {
                            nTo = k;
                        }
                    }
                }


                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    imLink[imNum] = Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
                else if (Type == "CC" && nFrom >= 0)
                {
                    if (Network[fromN].Node[searchNode[nFrom]].Kind == "XOR" && Network[fromN].Node[searchNode[nFrom]].nPost > 1)
                    {
                        imLink[imNum] = Network[fromN].Link[i];
                        imLink[imNum].fromNode = nFrom;
                        imLink[imNum].toNode = Network[toN].nNode - 1; ;
                        imNum++;
                    }

                }


            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;

            if (sDummy)
            {

                for (int i = 0; i < sNode.Length; i++)
                {
                    if (Type == "II") // inspect_Irreducible
                    {
                        if (sNode[i] != entryH) continue;
                    }

                    imLink[imNum].fromNode = Network[toN].nNode - addNum;

                    for (int k = 0; k < nSearchNode; k++)
                    {
                        if (sNode[i] == searchNode[k])
                        {
                            imLink[imNum].toNode = k;
                            break;
                        }
                    }

                    imNum++;
                }

                addNum--;
            }

            if (eDummy)
            {
                for (int i = 0; i < eNode.Length; i++)
                {
                    imLink[imNum].toNode = Network[toN].nNode - addNum;

                    for (int k = 0; k < nSearchNode; k++)
                    {
                        if (eNode[i] == searchNode[k])
                        {
                            imLink[imNum].fromNode = k;
                            break;
                        }
                    }

                    imNum++;
                }
            }


            Network[toN].nLink = imNum;
            Network[toN].Link = new strLink[Network[toN].nLink];
            for (int i = 0; i < Network[toN].nLink; i++)
            {
                Network[toN].Link[i] = imLink[i];
            }

        }

        private bool check_addLink(int fromN, int workLoop, int loop, int link, string Type)
        {
            bool bAdd = true;

            if (Type == "CI") // check_Irreducible
            {
                for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                {
                    if (Loop[workLoop].Loop[loop].Entry[j] == Network[fromN].Link[link].toNode) // 제거
                    {
                        bAdd = false;
                        break;
                    }
                }
            }
            else if (Type == "CC") // concurrency
            {
                for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                {
                    if (Loop[workLoop].Loop[loop].Entry[j] == Network[fromN].Link[link].fromNode) // 제거
                    {
                        bAdd = false;
                        break;
                    }
                }
            }


            return bAdd;

        }

        #endregion


        #region Instance Flow 만들기

        private bool find_InstanceNode(int currentN, int sNode)// bool conAND // OR를 AND로 간주 - Concurrency Check 시
        {
            bool retBool = true;

            int tNode;
            if (Network[currentN].Node[sNode].Kind == "OR" && Network[currentN].Node[sNode].nPost > 1)
            {
                int num = 0;
                if (nCurrentXOR < nSearchXOR - 1)
                {
                    num = SearchXOR[nCurrentXOR];
                }
                else if (nCurrentXOR == nSearchXOR - 1)
                {
                    //num += 1;
                    SearchXOR[nCurrentXOR] += 1;
                    num = SearchXOR[nCurrentXOR];

                    if (SearchXOR[nCurrentXOR] >= Math.Pow(2, Network[currentN].Node[sNode].nPost) - 1) //(2^n -1) case 
                    {
                        num = -1;
                        nSearchXOR--;
                        retBool = false;
                    }
                }
                else
                {
                    num = 0;
                    SearchXOR[nSearchXOR] = 0;
                    nSearchXOR++;
                }

                if (num >= 0)
                {
                    nCurrentXOR++;

                    string strCombination = Convert.ToString(num + 1, 2).PadLeft(Network[currentN].Node[sNode].nPost, '0'); //đệm vào bên trái nPost số "0"

                    for (int i = 0; i < Network[currentN].Node[sNode].nPost; i++)
                    {
                        if (strCombination.Substring(i, 1) == "0") continue; //use binary string 000101010.. if curren index of string is 0 => no instance case => continue

                        tNode = Network[currentN].Node[sNode].Post[i];

                        bool bSame = false;
                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (InstantNode[k] == tNode)
                            {
                                bSame = true;
                                break;
                            }
                        }

                        for (int j = 0; j < Network[currentN].nLink; j++)
                        {
                            if (Network[currentN].Link[j].fromNode == sNode && Network[currentN].Link[j].toNode == tNode)
                            {
                                Network[currentN].Link[j].bInstance = true;
                                break;
                            }
                        }

                        if (!bSame)
                        {
                            InstantNode[nInstantNode] = tNode;
                            nInstantNode++;

                            retBool = find_InstanceNode(currentN, tNode);
                            if (!retBool) return retBool;
                        }
                    }
                }

            }
            else if (Network[currentN].Node[sNode].Kind == "XOR" && Network[currentN].Node[sNode].nPost > 1)
            {
                int num = 0;
                if (nCurrentXOR < nSearchXOR - 1)
                {
                    num = SearchXOR[nCurrentXOR];
                }
                else if (nCurrentXOR == nSearchXOR - 1)
                {
                    //num += 1;
                    SearchXOR[nCurrentXOR] += 1;
                    num = SearchXOR[nCurrentXOR];

                    if (SearchXOR[nCurrentXOR] >= Network[currentN].Node[sNode].nPost)
                    {
                        num = -1;
                        nSearchXOR--;
                        retBool = false;
                    }
                }
                else
                {
                    num = 0;
                    SearchXOR[nSearchXOR] = 0;
                    nSearchXOR++;
                }

                if (num >= 0)
                {
                    nCurrentXOR++;


                    tNode = Network[currentN].Node[sNode].Post[num];

                    bool bSame = false;
                    for (int k = 0; k < nInstantNode; k++)
                    {
                        if (InstantNode[k] == tNode)
                        {
                            bSame = true;
                            break;
                        }
                    }

                    for (int j = 0; j < Network[currentN].nLink; j++)
                    {
                        if (Network[currentN].Link[j].fromNode == sNode && Network[currentN].Link[j].toNode == tNode)
                        {
                            Network[currentN].Link[j].bInstance = true;
                            break;
                        }
                    }

                    if (!bSame)
                    {
                        InstantNode[nInstantNode] = tNode;
                        nInstantNode++;

                        retBool = find_InstanceNode(currentN, tNode);
                        if (!retBool) return retBool;
                    }
                }

            }
            else if (Network[currentN].Node[sNode].nPost > 0)
            {
                for (int i = 0; i < Network[currentN].Node[sNode].nPost; i++)
                {
                    tNode = Network[currentN].Node[sNode].Post[i];

                    bool bSame = false;
                    for (int k = 0; k < nInstantNode; k++)
                    {
                        if (InstantNode[k] == tNode)
                        {
                            bSame = true;
                            break;
                        }
                    }

                    for (int j = 0; j < Network[currentN].nLink; j++)
                    {
                        if (Network[currentN].Link[j].fromNode == sNode && Network[currentN].Link[j].toNode == tNode)
                        {
                            Network[currentN].Link[j].bInstance = true;
                            break;
                        }
                    }

                    if (!bSame)
                    {
                        InstantNode[nInstantNode] = tNode;
                        nInstantNode++;

                        retBool = find_InstanceNode(currentN, tNode);
                        if (!retBool) return retBool;
                    }
                }
            }

            return retBool;

        }

        #endregion


        #region Type I splitting

        //Make A Gateways in to Single Exit, Single Entry (1 into 2 nodes)
        //Using midNet to store the results
        private void Run_Split_Type1()
        {
            // Network(0)로 작업하여 Network(1) 생성

            int nNode = Network[orgNet].nNode;
            int nLink = Network[orgNet].nLink;

            //This is Global Variables
            nSearchNode = 0;
            searchNode = new int[nNode];
            //nPre ~ Predecessor; nPost ~ Sucessor
            for (int i = 0; i < nNode; i++)
            {
                if (Network[orgNet].Node[i].nPre >= 2 && Network[orgNet].Node[i].nPost >= 2)
                {
                    searchNode[nSearchNode] = i;
                    nSearchNode++;
                }
            }

            // 새네트워크 생성 (복제)
            Network[midNet] = Network[orgNet];
            //midNet => save the extent_Network. Create more node for split
            //just extent the number (nSearchNode) of Node and Link with null value
            extent_Network(midNet, nSearchNode);

            if (nSearchNode > 0)
            {
                Type_I_Split(midNet, nNode, nLink);
            }
        }

        private void Type_I_Split(int currentN, int nNode, int nLink)
        {
            for (int i = 0; i < nSearchNode; i++)
            {
                //For example searchNode{5, 6, 7, 10} (4 nodes) => ( [0] = 5, [1] = 6) => sNode = 4 + i => 4, 5, 6, 7
                int jNode = searchNode[i];
                int sNode = nNode + i; //node i ~ join Node => node s ~ split Node = n + i

                //Split Node - 추가
                Network[currentN].Node[sNode].Kind = Network[currentN].Node[jNode].Kind;
                Network[currentN].Node[sNode].Name = Network[currentN].Node[jNode].Name;// jNode.ToString();
                Network[currentN].Node[sNode].orgNum = jNode;
                Network[currentN].Node[sNode].parentNum = sNode;
                Network[currentN].Node[sNode].Type_I = "_s";
                Network[currentN].Node[sNode].Special = "";
                //Join Node - 변경
                Network[currentN].Node[jNode].Type_I = "_j";

                //New Link 추가
                Network[currentN].Link[nLink + i].fromNode = jNode;
                Network[currentN].Link[nLink + i].toNode = sNode;

                //기존 Link 정보 변경
                for (int j = 0; j < nLink; j++)
                {
                    if (Network[currentN].Link[j].fromNode == jNode)
                    {
                        Network[currentN].Link[j].fromNode = sNode;
                        //Find all the rest of information of a Node. Such as => Predecessors, Sucessor
                        find_NodeInform(currentN, Network[currentN].Link[j].toNode);
                    }
                }
                //Find all the rest of information of a Node. Such as => Predecessors, Sucessor
                find_NodeInform(currentN, jNode);
                find_NodeInform(currentN, sNode);
            }

        }

        #endregion


        #region Loop 구하기

        private void Run_FindLoop(int currentN, int workLoop)
        {
            // Network(1)로 작업하여 Loop(0) 생성
            //int currentN =1;
            //int workLoop = 0;

            //search_Loop(midNet, orgLoop); //originally, he use midNet
            search_Loop(currentN, orgLoop);


            if (check_Irreducible(currentN, orgLoop))
            {
                //MessageBox.Show("Irreducible Error : This network can not be handled");
                IrreducibleError = true;
                return;
            }

            //Irreducible loop merge
            merge_Irreducible(orgLoop);

            //Irreducible loop내  헤드공유하는 Natural Loop 찾기
            inspect_Irreducible(currentN, orgLoop);

            //find Special Node (Loop Entry: E, Loop Exit: X, Backward Split: B, BS and Exit: T)
            find_SpecialNode(currentN, orgLoop); //update all loops

             

        }

        public void upgrade_LoopHeader(int currentN, int orgLoop)
        {
            for (int i = 0; i < Loop[orgLoop].nLoop; i++)
            {
                if (Loop[orgLoop].Loop[i].Irreducible != true) continue;

                //set concurrencyEntry = 1;
                Loop[orgLoop].Loop[i].Concurrency = new int[Loop[orgLoop].Loop[i].nEntry];
                Loop[orgLoop].Loop[i].nConcurrency = 1;
                for (int e = 0; e < Loop[orgLoop].Loop[i].nEntry; e ++) Loop[orgLoop].Loop[i].Concurrency[e] = 1;

                //Make Flow from CID to entries
                int CID = -1;
                //find current DFLOW; => DONE ==================================================== => Storing in subNet
                //find all path from CID to current entry Set
                bool bFor = true;
                find_Node_DFlow(currentN, orgLoop, i, "CC", 1, 1, ref CID, ref bFor); // because conNet store full network;
                check_SearchNode_Same(ref searchNode, ref nSearchNode);
                //Add Node
                Network[subNet] = new strNetwork();
                Network[subNet].nNode = nSearchNode;
                Network[subNet].Node = new strNode[Network[subNet].nNode];
                for (int k = 0; k < nSearchNode; k++)
                {
                    Network[subNet].Node[k] = Network[currentN].Node[searchNode[k]];
                    Network[subNet].Node[k].orgNum = searchNode[k];
                }
                //Add Link
                make_subLink_2(currentN, subNet, orgLoop, i, "CC", 1, CID, -1);
                //find node inform
                for (int k = 0; k < Network[subNet].nNode; k++)
                {
                    find_NodeInform(subNet, k);
                }
                find_Dom(subNet);
                find_DomEI(subNet, -1);
                find_DomRev(subNet);
                find_DomRevEI(subNet);
                Network[DFlow_PdFlow] = Network[subNet];
                extent_Network(DFlow_PdFlow, 0);
                for (int k = 0; k < Network[DFlow_PdFlow].nNode; k++)
                {
                    if (Network[DFlow_PdFlow].Node[k].Name == CID.ToString()) Network[DFlow_PdFlow].header = k;
                }

                //int a = 0;
                bool[] mark = new bool[Network[currentN].nNode];
                //find header
                int header = find_header(DFlow_PdFlow, orgLoop, i); //head = -1 or NOT

                //swap new header and node member
                if (header != -1) //if find out new header (replace it)
                {
                    for (int k = 0; k < Loop[orgLoop].Loop[i].nNode; k++)
                    {
                        if (header == Loop[orgLoop].Loop[i].Node[k])
                        {
                            Loop[orgLoop].Loop[i].Node[k] = Loop[orgLoop].Loop[i].header;
                            Loop[orgLoop].Loop[i].header = header;
                        }
                    }
                }
                //return original state of conccurency and nConcurrency
                Loop[orgLoop].Loop[i].Concurrency = null;
                Loop[orgLoop].Loop[i].nConcurrency = 0;
            }
        }

        public int find_header(int currentN, int workLoop, int loop)
        {
            for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++)
            {
                //if (Network[currentN].Node[i].Special != "E") continue;
                bool[] mark = new bool[Network[currentN].nNode];
                int countEntries = 1;
                visit_all_entries(i, currentN, workLoop, loop, ref mark, ref countEntries);
                if (countEntries == Loop[workLoop].Loop[loop].nEntry - 1) return Loop[workLoop].Loop[loop].Entry[i];
            }
            return -1;
        }
        public void visit_all_entries(int startNode, int currentN, int workLoop, int loop, ref bool[] mark, ref int countEntries)
        {
            mark[startNode] = true;
            for (int i = 0; i < Network[currentN].Node[startNode].nPost; i++)
            {
                int post = Network[currentN].Node[startNode].Post[i];
                if (Network[currentN].Node[post].Special == "E") countEntries++;
                if (mark[Network[currentN].Node[startNode].Post[i]] == true) visit_all_entries(Network[currentN].Node[startNode].Post[i], currentN, workLoop, loop, ref mark, ref countEntries);
            }
            mark[startNode] = false;
        }

        private void search_Loop(int currentN, int workLoop)
        {
            make_Loop(currentN);

            Loop[workLoop] = new strLoop();
            // Loop생성
            int cnt = 0;
            for (int i = 0; i < Block.Length; i++)
            {
                if (Block[i].LoopHeader) cnt++;
            }

            Loop[workLoop].Loop = new strLoopInform[cnt];
            Loop[workLoop].nLoop = 0;
            Loop[workLoop].maxDepth = 0;

            find_Loop(currentN, workLoop, -1, 1);


            //Depth Search 
            int depth = 1;
            int count = Network[currentN].nNode;
            do
            {
                find_DepthNode(currentN, depth);
                count = count_UnDepth(currentN);
                depth++;

            } while (count > 0);


            // Loop별 Entry, Exit, Back SplitNode 검색
            find_LoopInform(currentN, workLoop);
        }

        private void make_Loop(int currentN)
        {
            //초기화
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                Network[currentN].Node[i].depth = 0;
                Network[currentN].Node[i].Special = "";
                Network[currentN].Node[i].done = false;
            }
            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                Network[currentN].Link[i].bBackJ = false;
                Network[currentN].Link[i].bBackS = false;
            }

            //모든 Loop찾아서
            int tempN = Network[currentN].nNode;
            Block = new strBlock[tempN];
            for (int i = 0; i < tempN; i++)
            {
                Block[i].LoopHeader = false;
                Block[i].Irreducible = false;
                Block[i].tranversed = false;
                Block[i].DFSP_pos = 0;
                Block[i].iloop_header = -1;

                Block[i].nBackEdge = 0;
                Block[i].fromNodeBack = new int[Network[currentN].Node[i].nPre];//.nPost];
                Block[i].nReentry = 0;
                Block[i].fromNodeReentry = new int[Network[currentN].Node[i].nPre];//.nPost];
            }

            trav_loops_DFS(currentN, Network[currentN].header, 1);


        }
        //Duyet sau bat dau tu header.
        private int trav_loops_DFS(int currentN, int b0, int DFSP_pos)
        {
            Block[b0].tranversed = true;     //mark node as trnversed
            Block[b0].DFSP_pos = DFSP_pos;   //mark node's position in DFSP

            for (int i = 0; i < Network[currentN].Node[b0].nPost; i++)
            {
                int b = Network[currentN].Node[b0].Post[i];

                if (!Block[b].tranversed)
                {
                    //CASE A - New
                    int nh = trav_loops_DFS(currentN, b, DFSP_pos + 1);
                    tag_lhead(b0, nh);
                }
                else
                {
                    if (Block[b].DFSP_pos > 0) //b in DFSP(b0)
                    {
                        //CASE B
                        Block[b].LoopHeader = true; // mark as a loop header
                        Block[b].fromNodeBack[Block[b].nBackEdge] = b0; // mark (b0,b) as backedge
                        Block[b].nBackEdge++;

                        tag_lhead(b0, b);
                    }
                    else if (Block[b].iloop_header == -1)
                    {
                        //CASE C  -  Do nothing
                    }
                    else
                    {
                        int h = Block[b].iloop_header;

                        if (Block[h].DFSP_pos > 0) //h in DFSP(b0)
                        {
                            //CASE D
                            tag_lhead(b0, h);
                        }
                        else //h not in DFSP(b0)
                        {
                            //CASE E - reentry
                            Block[b].fromNodeReentry[Block[b].nReentry] = b0; // mark (b0,b) as re entry
                            Block[b].nReentry++;

                            Block[h].Irreducible = true; // mark the loop of h as Irreducible

                            while (Block[h].iloop_header != -1)
                            {
                                h = Block[h].iloop_header;
                                if (Block[h].DFSP_pos > 0) //h in DFSP(b0)
                                {
                                    tag_lhead(b0, h);
                                    break;
                                }
                                Block[h].Irreducible = true; // mark the loop of h as Irreducible
                            }

                        }

                    }

                }
            }

            Block[b0].DFSP_pos = 0;  // clear b0's DFSP position

            return Block[b0].iloop_header;
        }

        private void tag_lhead(int b, int h)
        {
            if (b == h || h == -1) return;

            int cur1 = b, cur2 = h;

            while (Block[cur1].iloop_header != -1)
            {
                int ih = Block[cur1].iloop_header;

                if (ih == cur2) return;

                if (Block[ih].DFSP_pos < Block[cur2].DFSP_pos)
                {
                    Block[cur1].iloop_header = cur2;
                    cur1 = cur2;
                    cur2 = ih;
                }
                else
                {
                    cur1 = ih;
                }
            }
            Block[cur1].iloop_header = cur2;

        }

        private void find_Loop(int currentN, int workLoop, int header, int depth)
        {

            for (int i = 0; i < Block.Length; i++)
            {
                if (!Block[i].LoopHeader) continue;
                if (Block[i].iloop_header != header) continue;

                Loop[workLoop].Loop[Loop[workLoop].nLoop].header = i;
                Loop[workLoop].Loop[Loop[workLoop].nLoop].Irreducible = Block[i].Irreducible;
                //if (Loop[workLoop].Loop[Loop[workLoop].nLoop].Irreducible) Network[currentN].Node[i].Special = "hL";
                //else Network[currentN].Node[i].Special = "hN";
                Loop[workLoop].Loop[Loop[workLoop].nLoop].depth = depth;

                Loop[workLoop].Loop[Loop[workLoop].nLoop].parentLoop = -1;
                for (int k = 0; k < Loop[workLoop].nLoop; k++)
                {
                    if (Loop[workLoop].Loop[k].depth != depth - 1) continue;
                    if (Loop[workLoop].Loop[k].header == Block[i].iloop_header)
                    {
                        Loop[workLoop].Loop[Loop[workLoop].nLoop].parentLoop = k;
                        break;
                    }
                }

                Loop[workLoop].Loop[Loop[workLoop].nLoop].nBackEdge = Block[i].nBackEdge;
                Loop[workLoop].Loop[Loop[workLoop].nLoop].linkBack = new int[Loop[workLoop].Loop[Loop[workLoop].nLoop].nBackEdge];
                for (int j = 0; j < Block[i].nBackEdge; j++)
                {
                    for (int k = 0; k < Network[currentN].nLink; k++)
                    {
                        if (Network[currentN].Link[k].fromNode == Block[i].fromNodeBack[j] && Network[currentN].Link[k].toNode == i)
                        {
                            if (!Loop[workLoop].Loop[Loop[workLoop].nLoop].Irreducible) Network[currentN].Link[k].bBackJ = true; //Natural Loop[workLoop].Loop만 BackEdge;
                            Loop[workLoop].Loop[Loop[workLoop].nLoop].linkBack[j] = k;
                            break;
                        }
                    }
                }

                Loop[workLoop].Loop[Loop[workLoop].nLoop].nNode = 0;
                for (int j = 0; j < Block.Length; j++)
                {
                    if (Block[j].iloop_header != Loop[workLoop].Loop[Loop[workLoop].nLoop].header) continue;
                    Loop[workLoop].Loop[Loop[workLoop].nLoop].nNode++;
                }

                Loop[workLoop].Loop[Loop[workLoop].nLoop].Node = new int[Loop[workLoop].Loop[Loop[workLoop].nLoop].nNode];
                Loop[workLoop].Loop[Loop[workLoop].nLoop].nNode = 0;
                for (int j = 0; j < Block.Length; j++)
                {
                    if (Block[j].iloop_header != Loop[workLoop].Loop[Loop[workLoop].nLoop].header) continue;

                    Loop[workLoop].Loop[Loop[workLoop].nLoop].Node[Loop[workLoop].Loop[Loop[workLoop].nLoop].nNode] = j;
                    Loop[workLoop].Loop[Loop[workLoop].nLoop].nNode++;
                }

                Loop[workLoop].nLoop++;


                if (depth > Loop[workLoop].maxDepth) Loop[workLoop].maxDepth = depth;
                find_Loop(currentN, workLoop, i, depth + 1);
            }


        }

        private void find_DepthNode(int currentN, int d)
        {
            int nEnd = 0;
            int[] EndNode = new int[Network[currentN].nNode];

            if (d == 1) //종료노드 찾기
            {
                for (int i = 0; i < Network[currentN].nNode; i++)
                {
                    if (Network[currentN].Node[i].nPost == 0)
                    {
                        EndNode[nEnd] = i;
                        Network[currentN].Node[i].depth = d;
                        nEnd++;
                    }
                }
            }
            else
            {
                for (int j = 0; j < Network[currentN].nLink; j++)
                {
                    if (!Network[currentN].Link[j].bBackJ) continue; //BackEdge 아니면

                    int fromNode = Network[currentN].Link[j].fromNode;
                    int toNode = Network[currentN].Link[j].toNode;

                    if (Network[currentN].Node[toNode].depth == d - 1 && Network[currentN].Node[fromNode].depth == 0)
                    {
                        EndNode[nEnd] = fromNode;
                        Network[currentN].Node[fromNode].depth = d;
                        nEnd++;
                    }

                }
            }


            do
            {
                int nFrom = 0;
                int[] FromNode = new int[Network[currentN].nNode];
                for (int i = 0; i < nEnd; i++)
                {
                    for (int k = 0; k < Network[currentN].Node[EndNode[i]].nPre; k++)
                    {
                        int node = Network[currentN].Node[EndNode[i]].Pre[k];

                        if (Network[currentN].Node[node].depth != 0) continue; //이미 depth가 결정됬으면

                        bool isBack = false;

                        for (int j = 0; j < Network[currentN].nLink; j++)
                        {
                            if (!Network[currentN].Link[j].bBackJ) continue; //BackEdge 아니면

                            if (Network[currentN].Link[j].fromNode == node && Network[currentN].Link[j].toNode == EndNode[i])
                            {
                                isBack = true;
                                break;
                            }

                        }

                        if (isBack) continue; //backEdge면

                        Network[currentN].Node[node].depth = d;
                        FromNode[nFrom] = node;
                        nFrom++;
                    }
                }

                nEnd = nFrom;
                EndNode = FromNode;

            } while (nEnd > 0);

        }

        private int count_UnDepth(int currentN)
        {
            int count = 0;

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].depth == 0) count++;

            }

            return count;
        }

        private void find_LoopInform(int currentN, int workLoop)
        {
            int cntFind = 0;
            int[] find_Node = new int[Loop[workLoop].nLoop];


            // Child Loop[workLoop].Loop

            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                cntFind = 0;
                for (int j = 0; j < Loop[workLoop].nLoop; j++)
                {
                    if (Loop[workLoop].Loop[j].parentLoop == i)
                    {
                        find_Node[cntFind] = j;
                        cntFind++;
                    }

                }

                Loop[workLoop].Loop[i].nChild = cntFind;
                Loop[workLoop].Loop[i].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) Loop[workLoop].Loop[i].child[k] = find_Node[k];

            }

            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                //Loop[workLoop].Loop내 포함된 모든 노드 찾아서
                nSearchNode = 0;
                searchNode = new int[Network[currentN].nNode];

                searchNode[nSearchNode] = Loop[workLoop].Loop[i].header;
                nSearchNode++;
                find_LoopNode(workLoop, i);

                // Entry

                cntFind = 0;
                find_Node = new int[Network[currentN].nNode];

                for (int r = 0; r < nSearchNode; r++) // Loop[workLoop].Loop내 모든 노드에 대해
                {
                    bool isEntry = true;

                    for (int j = 0; j < Network[currentN].Node[searchNode[r]].nPre; j++) // 노드의 모든 pre 노드에 대해
                    {
                        int nodeP = Network[currentN].Node[searchNode[r]].Pre[j];

                        isEntry = true;
                        for (int k = 0; k < nSearchNode; k++)
                        {
                            if (nodeP == searchNode[k])
                            {
                                isEntry = false;
                                break;
                            }
                        }

                        if (isEntry) break;

                    }

                    if (isEntry)
                    {
                        find_Node[cntFind] = searchNode[r];
                        cntFind++;
                    }

                }

                Loop[workLoop].Loop[i].nEntry = cntFind;
                Loop[workLoop].Loop[i].Entry = new int[cntFind];
                for (int k = 0; k < cntFind; k++) Loop[workLoop].Loop[i].Entry[k] = find_Node[k];

                //Exit

                cntFind = 0;
                find_Node = new int[Network[currentN].nNode];

                for (int r = 0; r < nSearchNode; r++) // Loop[workLoop].Loop내 모든 노드에 대해
                {
                    bool isExit = true;

                    for (int j = 0; j < Network[currentN].Node[searchNode[r]].nPost; j++) // 노드의 모든 post 노드에 대해
                    {
                        int nodeP = Network[currentN].Node[searchNode[r]].Post[j];

                        isExit = true;
                        for (int k = 0; k < nSearchNode; k++)
                        {
                            if (nodeP == searchNode[k])
                            {
                                isExit = false;
                                break;
                            }
                        }

                        if (isExit) break;

                    }

                    if (isExit)
                    {
                        find_Node[cntFind] = searchNode[r];
                        cntFind++;
                    }

                }

                Loop[workLoop].Loop[i].nExit = cntFind;
                Loop[workLoop].Loop[i].Exit = new int[cntFind];
                for (int k = 0; k < cntFind; k++) Loop[workLoop].Loop[i].Exit[k] = find_Node[k];


                //BackSplit
                cntFind = 0;
                find_Node = new int[Network[currentN].nNode];

                int depth = Network[currentN].Node[Loop[workLoop].Loop[i].header].depth;
                for (int r = 0; r < nSearchNode; r++) // Loop[workLoop].Loop내 모든 노드에 대해
                {
                    int node = searchNode[r];
                    if (Network[currentN].Node[node].depth != depth) continue;

                    bool isBack = false;
                    for (int k = 0; k < Network[currentN].Node[node].nPost; k++)
                    {
                        int nodeP = Network[currentN].Node[node].Post[k];

                        if (Network[currentN].Node[nodeP].depth > depth)
                        {
                            for (int j = 0; j < Network[currentN].nLink; j++)
                            {
                                if (Network[currentN].Link[j].fromNode == node && Network[currentN].Link[j].toNode == nodeP)
                                {
                                    Network[currentN].Link[j].bBackS = true;
                                    isBack = true;
                                    break;
                                }
                            }


                        }
                        else if (Network[currentN].Node[nodeP].depth == depth) //바로 BackEdge 면
                        {
                            for (int j = 0; j < Network[currentN].nLink; j++)
                            {
                                if (!Network[currentN].Link[j].bBackJ) continue;

                                if (Network[currentN].Link[j].fromNode == node && Network[currentN].Link[j].toNode == nodeP)
                                {
                                    Network[currentN].Link[j].bBackS = true;
                                    isBack = true;
                                    break;
                                }

                            }

                        }
                    }
                    if (isBack)
                    {
                        //if (Loop[workLoop].Loop[i].Irreducible) Network[currentN].Node[node].Special = "bL";
                        //else Network[currentN].Node[node].Special = "bN";
                        find_Node[cntFind] = node;
                        cntFind++;
                    }
                }

                Loop[workLoop].Loop[i].nBackSplit = cntFind;
                Loop[workLoop].Loop[i].BackSplit = new int[cntFind];
                for (int k = 0; k < cntFind; k++) Loop[workLoop].Loop[i].BackSplit[k] = find_Node[k];
            }

        }

        public void find_LoopNode(int workLoop, int kLoop)
        {

            for (int i = 0; i < Loop[workLoop].Loop[kLoop].nNode; i++)
            {
                searchNode[nSearchNode] = Loop[workLoop].Loop[kLoop].Node[i];
                nSearchNode++;
            }

            for (int k = 0; k < Loop[workLoop].Loop[kLoop].nChild; k++)
            {
                find_LoopNode(workLoop, Loop[workLoop].Loop[kLoop].child[k]);
            }

        }

        private void find_SpecialNode(int currentN, int workLoop)
        {
            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                //Etry
                for (int j = 0; j < Loop[workLoop].Loop[i].nEntry; j++)
                {
                    int k = Loop[workLoop].Loop[i].Entry[j];
                    Network[currentN].Node[k].Special = "E";
                }

                //Exit
                for (int j = 0; j < Loop[workLoop].Loop[i].nExit; j++)
                {
                    int k = Loop[workLoop].Loop[i].Exit[j];
                    Network[currentN].Node[k].Special = "X";
                }
            }

            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                if (!Network[currentN].Link[i].bBackS) continue;

                int from = Network[currentN].Link[i].fromNode;

                if (Network[currentN].Node[from].Special == "X")
                {
                    Network[currentN].Node[from].Special = "T";
                }
                else
                {
                    if (Network[currentN].Node[from].Special != "T") //New line here (We add new condition for the check Special Node for BS, in order to avoid the reassetment of node "T"
                    {
                        Network[currentN].Node[from].Special = "B";
                    }
                }
            }

        }

        #endregion

        #region Network(Loop) Check

        private void merge_Irreducible(int workLoop)
        {
            int curDepth = Loop[workLoop].maxDepth;
            do
            {
                for (int i = 0; i < Loop[workLoop].nLoop; i++)
                {
                    if (Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (!Loop[workLoop].Loop[i].Irreducible) continue;

                    int parent = Loop[workLoop].Loop[i].parentLoop;
                    if (parent < 0) continue;
                    if (!Loop[workLoop].Loop[parent].Irreducible) continue;

                    merge_Loop(workLoop, parent, i);
                }
                curDepth--;
            } while (curDepth > 0);

            Loop[workLoop].maxDepth = 0;
            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                if (Loop[workLoop].Loop[i].depth > Loop[workLoop].maxDepth) Loop[workLoop].maxDepth = Loop[workLoop].Loop[i].depth;
            }
        }

        private void merge_Loop(int workLoop, int parent, int child)
        {
            //loop포함 노드
            int nNewNode = Loop[workLoop].Loop[parent].nNode + Loop[workLoop].Loop[child].nNode;
            int[] newNode = new int[nNewNode];
            nNewNode = 0;
            for (int i = 0; i < Loop[workLoop].Loop[parent].nNode; i++)
            {
                newNode[nNewNode] = Loop[workLoop].Loop[parent].Node[i];
                nNewNode++;
            }
            for (int i = 0; i < Loop[workLoop].Loop[child].nNode; i++)
            {
                newNode[nNewNode] = Loop[workLoop].Loop[child].Node[i];
                nNewNode++;
            }

            Loop[workLoop].Loop[parent].nNode = nNewNode;
            Loop[workLoop].Loop[parent].Node = newNode;

            // loop child loop
            int nNewChild = Loop[workLoop].Loop[parent].nChild - 1;
            int[] newChild = new int[nNewChild];
            nNewChild = 0;
            for (int i = 0; i < Loop[workLoop].Loop[parent].nChild; i++)
            {
                if (Loop[workLoop].Loop[parent].child[i] == child) continue;
                newChild[nNewChild] = Loop[workLoop].Loop[parent].child[i];
                nNewChild++;
            }

            Loop[workLoop].Loop[parent].nChild = nNewChild;
            Loop[workLoop].Loop[parent].child = newChild;

            //child loop 제거
            strLoopInform[] newLoop = new strLoopInform[Loop[workLoop].nLoop - 1];
            int nNewLoop = 0;
            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                if (i == child) continue;
                newLoop[nNewLoop] = Loop[workLoop].Loop[i];
                nNewLoop++;
            }

            Loop[workLoop].nLoop = nNewLoop;
            Loop[workLoop].Loop = newLoop;

            //child reNumbering
            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                for (int k = 0; k < Loop[workLoop].Loop[i].nChild; k++)
                {
                    if (Loop[workLoop].Loop[i].child[k] >= child) Loop[workLoop].Loop[i].child[k] -= 1;
                }


            }

            //parent reNumbering
            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                if (Loop[workLoop].Loop[i].parentLoop >= child) Loop[workLoop].Loop[i].parentLoop -= 1;
            }

        }

        private void inspect_Irreducible(int currentN, int workLoop)
        {
            int curDepth = 1;
            do
            {
                for (int i = 0; i < Loop[workLoop].nLoop; i++)
                {
                    if (Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (!Loop[workLoop].Loop[i].Irreducible) continue;

                    for (int k = 0; k < Loop[workLoop].Loop[i].nEntry; k++)
                    {
                        if (Loop[workLoop].Loop[i].Entry[k] == Loop[workLoop].Loop[i].header) continue;

                        make_subNetwork(currentN, subNet, workLoop, i, "II", Loop[workLoop].Loop[i].Entry[k]);    //5는 SubNetwork

                        add_subNatural(subNet, currentN, subLoop, workLoop, i);

                        break; //header가 다른 한 경우만 고려하면 됨....
                    }

                }
                curDepth++;
            } while (curDepth <= Loop[workLoop].maxDepth);

        }

        private void add_subNatural(int currentN, int saveN, int workLoop, int saveLoop, int loop)
        {

            //Loop[workLoop].Loop찾기
            search_Loop(currentN, workLoop);


            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                if (Loop[workLoop].Loop[i].parentLoop == -1) continue;
                if (Loop[workLoop].Loop[i].Irreducible) continue;

                int orgHeader = Network[currentN].Node[Loop[workLoop].Loop[i].header].parentNum;
                if (orgHeader != Loop[saveLoop].Loop[loop].header) continue; //원 loop의 헤더를 공유하는 것만 추가

                // Loop[workLoop].Loop 추가 ----------------------
                strLoopInform addLoop = new strLoopInform();
                int orgNode, orgNode2;

                addLoop = Loop[workLoop].Loop[i]; //일단 copy

                addLoop.depth = Loop[saveLoop].Loop[loop].depth + 1;
                addLoop.parentLoop = loop;
                for (int k = 0; k < Loop[workLoop].Loop[i].nChild; k++)
                {
                    orgNode = Network[currentN].Node[Loop[workLoop].Loop[Loop[workLoop].Loop[i].child[k]].header].parentNum;
                    int findLoop = -1;
                    for (int k2 = 0; k2 < Loop[saveLoop].nLoop; k2++)
                    {
                        if (orgNode == Loop[saveLoop].Loop[k2].header)
                        {
                            findLoop = k2;
                            break;
                        }
                    }
                    addLoop.child[k] = findLoop;
                    Loop[saveLoop].Loop[findLoop].parentLoop = Loop[saveLoop].nLoop;
                }

                orgNode = Network[currentN].Node[Loop[workLoop].Loop[i].header].parentNum;
                addLoop.header = orgNode;

                for (int k = 0; k < Loop[workLoop].Loop[i].nBackEdge; k++)
                {
                    orgNode = Network[currentN].Link[Loop[workLoop].Loop[i].linkBack[k]].fromNode;
                    orgNode = Network[currentN].Node[orgNode].parentNum;
                    orgNode2 = Network[currentN].Link[Loop[workLoop].Loop[i].linkBack[k]].toNode;
                    orgNode2 = Network[currentN].Node[orgNode2].parentNum;

                    int orgLink = -1;
                    for (int k2 = 0; k2 < Network[saveN].nLink; k2++)
                    {
                        if (orgNode == Network[saveN].Link[k2].fromNode && orgNode2 == Network[saveN].Link[k2].toNode)
                        {
                            orgLink = k2;
                            break;
                        }
                    }
                    addLoop.linkBack[k] = orgLink;
                }

                for (int k = 0; k < Loop[workLoop].Loop[i].nEntry; k++)
                {
                    orgNode = Network[currentN].Node[Loop[workLoop].Loop[i].Entry[k]].parentNum;
                    addLoop.Entry[k] = orgNode;
                }

                for (int k = 0; k < Loop[workLoop].Loop[i].nExit; k++)
                {
                    orgNode = Network[currentN].Node[Loop[workLoop].Loop[i].Exit[k]].parentNum;
                    addLoop.Exit[k] = orgNode;
                }

                for (int k = 0; k < Loop[workLoop].Loop[i].nNode; k++)
                {
                    orgNode = Network[currentN].Node[Loop[workLoop].Loop[i].Node[k]].parentNum;
                    addLoop.Node[k] = orgNode;
                }

                int nNewLoop = Loop[saveLoop].nLoop + 1;
                strLoop newLoop = new strLoop();
                newLoop.Loop = new strLoopInform[nNewLoop];

                for (int k = 0; k < Loop[saveLoop].nLoop; k++)
                {
                    newLoop.Loop[k] = Loop[saveLoop].Loop[k];
                    if (k == loop) //추가Loop[workLoop].Loop의 parents면
                    {
                        //----Child  추가 및 제거
                        int numAdd = 0;
                        int[] add = new int[Loop[saveLoop].Loop[k].nChild + 1];
                        for (int k2 = 0; k2 < Loop[saveLoop].Loop[k].nChild; k2++)
                        {
                            bool bDel = false;
                            for (int k3 = 0; k3 < addLoop.nChild; k3++)
                            {
                                if (Loop[saveLoop].Loop[k].child[k2] == addLoop.child[k3])
                                {
                                    bDel = true;
                                    break;
                                }
                            }
                            if (!bDel)
                            {
                                add[numAdd] = Loop[saveLoop].Loop[k].child[k2];
                                numAdd++;
                            }
                        }
                        add[numAdd] = Loop[saveLoop].nLoop;
                        numAdd++;

                        newLoop.Loop[k].nChild = numAdd;
                        newLoop.Loop[k].child = add;

                        //-----포함 Node 제거
                        numAdd = 0;
                        add = new int[Loop[saveLoop].Loop[k].nNode];
                        for (int k2 = 0; k2 < Loop[saveLoop].Loop[k].nNode; k2++)
                        {
                            bool bDel = false;
                            for (int k3 = 0; k3 < addLoop.nNode; k3++)
                            {
                                if (Loop[saveLoop].Loop[k].Node[k2] == addLoop.Node[k3])
                                {
                                    bDel = true;
                                    break;
                                }
                            }
                            if (!bDel)
                            {
                                add[numAdd] = Loop[saveLoop].Loop[k].Node[k2];
                                numAdd++;
                            }
                        }

                        newLoop.Loop[k].nNode = numAdd;
                        newLoop.Loop[k].Node = add;
                    }
                }
                newLoop.Loop[Loop[saveLoop].nLoop] = addLoop;

                Loop[saveLoop].Loop = newLoop.Loop;
                Loop[saveLoop].nLoop = nNewLoop;

            }

            Loop[saveLoop].maxDepth = 0;
            for (int i = 0; i < Loop[saveLoop].nLoop; i++)
            {
                if (Loop[saveLoop].Loop[i].depth > Loop[saveLoop].maxDepth) Loop[saveLoop].maxDepth = Loop[saveLoop].Loop[i].depth;
            }
        }

        private bool check_Irreducible(int currentN, int workLoop)
        {
            bool bError = false;
            int curDepth = 1;
            do
            {
                for (int i = 0; i < Loop[workLoop].nLoop; i++)
                {
                    if (Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (!Loop[workLoop].Loop[i].Irreducible) continue;

                    int nIrr = 0;
                    for (int k = 0; k < Loop[workLoop].Loop[i].nChild; k++)
                    {
                        if (Loop[workLoop].Loop[Loop[workLoop].Loop[i].child[k]].Irreducible) nIrr++;
                    }
                    if (nIrr == 0) continue;

                    make_subNetwork(currentN, subNet, workLoop, i, "CI", -1);    //5는 SubNetwork

                    if (check_subIrreducible(subNet, subLoop))
                    {
                        bError = true;
                        break;
                    }
                }
                curDepth++;
            } while (curDepth <= Loop[workLoop].maxDepth && !bError);


            return bError;

        }

        private bool check_subIrreducible(int currentN, int workLoop)
        {

            //Loop[workLoop].Loop찾기
            search_Loop(currentN, workLoop);

            bool bError = false;


            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                if (Loop[workLoop].Loop[i].depth > 1) continue;
                if (Loop[workLoop].Loop[i].Irreducible)
                {
                    bError = true;
                    break;
                }
            }

            return bError;
        }


        #endregion

        #region Type II  splitting

        //using finalNet to store the results
        private void Run_Split_Type2()
        {
            // Network(2) 생성후 작업

            int nNode = Network[midNet].nNode;
            int nLink = Network[midNet].nLink;

            // 새네트워크 생성 (복제)
            Network[finalNet] = Network[midNet];

            //Loop없으면 건너뛰기
            if (Loop[orgLoop].nLoop == 0)
            {
                extent_Network(finalNet, 0);
                return;
            }
            //Extent_network with the new number of node. (because we assign the fixed number of node of model when we read the model.
            //For example: nNode = 10; nLink = 14 => after extent => nNode = 10 + 10 (nNode); nLink = 14 + 10 (nNode)
            extent_Network(finalNet, nNode);

            //Extension Type II split in Entry of Loop => (2 new node and 2 new link maybe create)
            int nSplit = Type_II_Split_Entry(finalNet, orgLoop, nNode, nLink);
            nNode += nSplit;
            nLink += nSplit;
            
            //we need identify the loop after split entry (extension)
            //resize_Network(finalNet, nNode, nLink);
            Run_FindLoop(finalNet, orgLoop); //===> Because the extension of type_2 create nodes inside the loops => we need add it into a Loop[] structure.
            //extent_Network(finalNet, nNode);


            nSplit = 0;
            nSplit = Type_II_Split_Exit(finalNet, orgLoop, nNode, nLink);
            nNode += nSplit;
            nLink += nSplit;

            Run_FindLoop(finalNet, orgLoop);
            //nSplit = 0;
            //nSplit = Type_II_Split_Back(finalNet, nNode, nLink);
            //nNode += nSplit;
            //nLink += nSplit;

            resize_Network(finalNet, nNode, nLink);

            After_Type_II_Split(finalNet, orgLoop);

        }

        private void After_Type_II_Split(int currentN, int workLoop)
        {
            //Loop 재탐색
            search_Loop(currentN, workLoop);
            merge_Irreducible(workLoop);
            find_SpecialNode(currentN, workLoop);

            //


        }

        //This function consider all of the loop of model (workLoop) and split entry of these loop
        private int Type_II_Split_Entry(int currentN, int workLoop, int nNode, int nLink)
        {
            int nSplit = 0;

            for (int i = 0; i < Loop[workLoop].nLoop; i++) //visit each loop in model
            {
                for (int k = 0; k < Loop[workLoop].Loop[i].nEntry; k++) //vist each entry of current loop.
                {
                    int fjNode;
                    int bjNode;

                    int node = Loop[workLoop].Loop[i].Entry[k]; //get current entry and then split this node - if necessary

                    int[] preF = new int[nNode]; //store all node outside the loop
                    int[] preB = new int[nNode]; //store all node from inside the current loop (i)
                    int cntF = 0, cntB = 0;

                    for (int j = 0; j < nLink; j++)
                    {
                        if (Network[currentN].Link[j].toNode == node) //if find a node which is go to current entry (node)
                        {

                            if (Node_In_Loop(workLoop, Network[currentN].Link[j].fromNode, i)) //If the predecessor of current entry belong to this loop(i).
                            {
                                preB[cntB] = Network[currentN].Link[j].fromNode; //store all node from inside the current loop (i) go to this entry (k) (node)
                                cntB++; //increase index - number of node inside the loop
                            }
                            else
                            {
                                preF[cntF] = Network[currentN].Link[j].fromNode; //store all node which is predecessor of this entry (node)
                                cntF++;
                            }
                        }
                    }

                    if (cntF > 1) // For join이 2 미만이면 //it mean if all node go to entry outside the loop is just 1 or 0 => don't need to split.
                    {
                        fjNode = nNode;
                        bjNode = node;

                        //For Join Node - 추가 //Tao nut moi
                        Network[currentN].Node[fjNode].Kind = Network[currentN].Node[node].Kind;
                        Network[currentN].Node[fjNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                        Network[currentN].Node[fjNode].orgNum = node;
                        Network[currentN].Node[fjNode].parentNum = fjNode;
                        Network[currentN].Node[fjNode].Type_I = Network[currentN].Node[node].Type_I;
                        Network[currentN].Node[fjNode].Type_II = Network[currentN].Node[node].Type_II + "_fj";

                        //We need focus on this node => split this node
                        //Back Join Node - 변경 //Thay doi info cua nut current entry
                        Network[currentN].Node[bjNode].Type_II = Network[currentN].Node[node].Type_II + "_bj"; //just add "_bj" for current entry
                        Network[currentN].Node[fjNode].Special = ""; //What is that mean

                        //New Link 추가
                        Network[currentN].Link[nLink].fromNode = fjNode;
                        Network[currentN].Link[nLink].toNode = bjNode;


                        nSplit++;
                        nNode++; //nNode < nNode in extend_network
                        nLink++;



                        //기존 Link 정보 변경
                        //This code just consider the node outside the loop come to this entry. (adjust the link coming from outside the loop)
                        for (int j = 0; j < nLink; j++)
                        {
                            for (int j2 = 0; j2 < cntF; j2++)
                            {
                                //ajust the link from outside the loop to this entry node.
                                if (Network[currentN].Link[j].fromNode == preF[j2] && Network[currentN].Link[j].toNode == node)
                                {
                                    Network[currentN].Link[j].toNode = fjNode;
                                    find_NodeInform(currentN, Network[currentN].Link[j].fromNode);
                                }
                            }
                        }

                        find_NodeInform(currentN, fjNode);
                        find_NodeInform(currentN, bjNode);
                        if (Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(workLoop, node, fjNode, Loop[workLoop].Loop[i].parentLoop);
                    }
                    //========== EXTENTION ========================================================================(1 bai hoc nho doi)
                    #region hide
                    //if there are at least 2 incoming edges from inside the loop to this entry => extend type II 

                    /*if (isInsideLoop(currentN, workLoop, i, node) == true)
                    {
                        //==New code===
                        fjNode = nNode; //New Node
                        bjNode = node; //Current Entry
                        //create new node
                        Network[currentN].Node[fjNode].Kind = Network[currentN].Node[node].Kind;
                        Network[currentN].Node[fjNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                        Network[currentN].Node[fjNode].orgNum = node;
                        Network[currentN].Node[fjNode].parentNum = fjNode;
                        Network[currentN].Node[fjNode].Type_I = Network[currentN].Node[node].Type_I;
                        Network[currentN].Node[fjNode].Type_II = Network[currentN].Node[node].Type_II + "_2";
                        //create new link
                        Network[currentN].Link[nLink].fromNode = fjNode;
                        Network[currentN].Link[nLink].toNode = bjNode;

                        nSplit++;
                        nNode++; //nNode < nNode in extend_network
                        nLink++;

                        //Consider all incoming edges from inside the current loop to this current entry === (new move from above) crazy <===
                        for (int j = 0; j < nLink; j++)
                        {
                            for (int j2 = 0; j2 < cntB; j2++) //consider the node inside the loop which have outgoing edges to loop entry
                            {
                                //ajust the link from outside the loop to this entry node. preB[] set node inside loop ; preF[] set of node outside the loop
                                if (Network[currentN].Link[j].fromNode == preB[j2] && Network[currentN].Link[j].toNode == node)
                                {
                                    Network[currentN].Link[j].toNode = fjNode; //fjNode is the new node
                                    find_NodeInform(currentN, Network[currentN].Link[j].fromNode);
                                }
                            }
                        }
                        find_NodeInform(currentN, fjNode); //inform for new node (after split type 2 extension)
                        find_NodeInform(currentN, bjNode); //inform for this loop's entry
                        if (Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(workLoop, node, fjNode, Loop[workLoop].Loop[i].parentLoop);
                    }
                    */
                    #endregion
                    //========== END  ==============================================================================
                    //Type_II_Extension_Entry(currentN, workLoop, i, nNode, nLink, node, ref nSplit, preB, cntB);

                    
                }
            }

            return nSplit;
        }

        //Extension Type II=================
        private void Type_II_Extension_Entry(int currentN, int workLoop, int currentWorkLoop, int nNode, int nLink, int currentNode, ref int nSplit, int[] nodesInsideLoop, int nNodeInsideLoop)
        {
            int i = currentWorkLoop;
            int node = currentNode;
            int[] preB = nodesInsideLoop; //all node from inside the loop
            int cntB = nNodeInsideLoop;

            //========== EXTENTION ========================================================================(1 bai hoc nho doi)
            //if there are at least 2 incoming edges from inside the loop to this entry => extend type II 

            if (isInsideLoop(currentN, workLoop, i, node, "en") == true)
            {
                //==New code===
                int fjNode = nNode; //New Node
                int bjNode = node; //Current Entry
                //create new node
                Network[currentN].Node[fjNode].Kind = Network[currentN].Node[node].Kind;
                Network[currentN].Node[fjNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                Network[currentN].Node[fjNode].orgNum = node;
                Network[currentN].Node[fjNode].parentNum = fjNode;
                Network[currentN].Node[fjNode].Type_I = Network[currentN].Node[node].Type_I;
                Network[currentN].Node[fjNode].Type_II = Network[currentN].Node[node].Type_II + "_en_extn";
                //create new link
                Network[currentN].Link[nLink].fromNode = fjNode;
                Network[currentN].Link[nLink].toNode = bjNode;

                nSplit++;
                nNode++; //nNode < nNode in extend_network
                nLink++;

                //Consider all incoming edges from inside the current loop to this current entry === (new move from above) crazy <===
                for (int j = 0; j < nLink; j++)
                {
                    for (int j2 = 0; j2 < cntB; j2++) //consider the node inside the loop which have outgoing edges to loop entry
                    {
                        //ajust the link from outside the loop to this entry node. preB[] set node inside loop ; preF[] set of node outside the loop
                        if (Network[currentN].Link[j].fromNode == preB[j2] && Network[currentN].Link[j].toNode == node)
                        {
                            Network[currentN].Link[j].toNode = fjNode; //fjNode is the new node
                            find_NodeInform(currentN, Network[currentN].Link[j].fromNode);
                        }
                    }
                }
                find_NodeInform(currentN, fjNode); //inform for new node (after split type 2 extension)
                find_NodeInform(currentN, bjNode); //inform for this loop's entry
                if (Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(workLoop, node, fjNode, Loop[workLoop].Loop[i].parentLoop);
            }


            //========== END  ==============================================================================
        }

        private bool isInsideLoop(int currentN, int workLoop, int currentLoop, int currEntry, string flag)
        {
            if (flag == "en")
            {
                int count = 0;
                int nodeIndex = currEntry;
                if (Network[currentN].Node[nodeIndex].nPre >= 3)
                {
                    count = 0;
                    int nPre_temp = Network[currentN].Node[nodeIndex].nPre;
                    int nLoop_temp = Loop[workLoop].Loop[currentLoop].nNode;
                    for (int i = 0; i < nPre_temp; i++)
                    {
                        for (int j = 0; j < nLoop_temp; j++)
                        {
                            if (Network[currentN].Node[nodeIndex].Pre[i] == Loop[workLoop].Loop[currentLoop].Node[j])
                            {
                                count = count + 1;
                            }
                        }
                    }
                }
                if (count >= 2)
                    return true;
                else
                    return false;
            }
            
            if (flag == "ex")
            {
                int count = 0;
                int nodeIndex = currEntry;
                if (Network[currentN].Node[nodeIndex].nPost >= 3) //count the number of successors
                {
                    count = 0;
                    int nPost_temp = Network[currentN].Node[nodeIndex].nPost;
                    int nLoop_temp = Loop[workLoop].Loop[currentLoop].nNode;
                    for (int i = 0; i < nPost_temp; i++)
                    {
                        for (int j = 0; j < nLoop_temp; j++)
                        {
                            if (Network[currentN].Node[nodeIndex].Post[i] == Loop[workLoop].Loop[currentLoop].Node[j])
                            {
                                count = count + 1;
                            }
                        }
                    }
                }
                if (count >= 2)
                    return true;
                else
                    return false;

            }
            /*if (flag == "bs")
            {

            }*/

            return false;

        }
        private int Type_II_Split_Back(int currentN, int nNode, int nLink)
        {
            int nSplit = 0;

            //Split - Back Split Node 이고 For split이 2 이상이면

            for (int i = 0; i < nNode; i++)
            {
                if (Network[currentN].Node[i].Special != "B" && Network[currentN].Node[i].Special != "T") continue; // Back 아니면
                //find the backward split node
                int[] postF = new int[nNode];
                int[] postB = new int[nNode];
                int cntF = 0, cntB = 0;
                for (int j = 0; j < nLink; j++)
                {
                    if (Network[currentN].Link[j].fromNode == i)
                    {
                        if (Network[currentN].Link[j].bBackS) //just consider the split node
                        {
                            postB[cntB] = Network[currentN].Link[j].toNode;
                            cntB++;
                        }
                        else
                        {
                            postF[cntF] = Network[currentN].Link[j].toNode;
                            cntF++;
                        }
                    }
                }
                if (cntF > 1) //if (cntF < 2) continue; // For split이 2 미만이면
                {

                    int fsNode = nNode;
                    int bsNode = i;

                    //For Spilt Node - 추가
                    Network[currentN].Node[fsNode].Kind = Network[currentN].Node[i].Kind;
                    Network[currentN].Node[fsNode].Name = Network[currentN].Node[i].Name; // i.ToString();
                    Network[currentN].Node[fsNode].orgNum = i;
                    Network[currentN].Node[fsNode].parentNum = fsNode;
                    Network[currentN].Node[fsNode].Type_I = Network[currentN].Node[i].Type_I;
                    Network[currentN].Node[fsNode].Type_II = Network[currentN].Node[i].Type_II + "_fs";
                    Network[currentN].Node[fsNode].Special = "";

                    //Back Join Node - 변경
                    Network[currentN].Node[bsNode].Type_II = Network[currentN].Node[i].Type_II + "_bs";

                    //New Link 추가
                    Network[currentN].Link[nLink].fromNode = bsNode;
                    Network[currentN].Link[nLink].toNode = fsNode;


                    nSplit++;
                    nNode++;
                    nLink++;

                    //기존 Link 정보 변경

                    for (int j = 0; j < nLink; j++)
                    {
                        for (int j2 = 0; j2 < cntF; j2++)
                        {
                            if (Network[currentN].Link[j].fromNode == i && Network[currentN].Link[j].toNode == postF[j2])
                            {
                                Network[currentN].Link[j].fromNode = fsNode;
                                find_NodeInform(currentN, Network[currentN].Link[j].toNode);
                            }
                        }
                    }

                    find_NodeInform(currentN, fsNode);
                    find_NodeInform(currentN, bsNode);
                }
                //Extension Type II Backward split
                //Type_II_Extension_Back(currentN, nNode, nLink, i, ref nSplit, postB, cntB); //PostB[] and cntB (Store all BS of node i)


            }

            return nSplit;
        }

        private void Type_II_Extension_Back(int currentN, int nNode, int nLink, int currentNode, ref int nSplit, int[] nodesInsideLoop, int nNodeInsideLoop)
        {
            //int i = currentWorkLoop;
            int node = currentNode;
            int[] preB = nodesInsideLoop; //all node from inside the loop
            int cntB = nNodeInsideLoop;

            //========== EXTENTION ========================================================================(1 bai hoc nho doi)
            //if there are at least 2 incoming edges from inside the loop to this entry => extend type II 

            //if (isInsideLoop(currentN, workLoop, i, node, "ex") == true) //problem here //solved
            if (cntB > 1)
            {
                //==New code===
                int fjNode = nNode; //New Node
                int bjNode = node; //Current Entry
                //create new node
                Network[currentN].Node[fjNode].Kind = Network[currentN].Node[node].Kind;
                Network[currentN].Node[fjNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                Network[currentN].Node[fjNode].orgNum = node;
                Network[currentN].Node[fjNode].parentNum = fjNode;
                Network[currentN].Node[fjNode].Type_I = Network[currentN].Node[node].Type_I;
                Network[currentN].Node[fjNode].Type_II = Network[currentN].Node[node].Type_II + "_bs_extn";
                //create new link
                Network[currentN].Link[nLink].fromNode = bjNode; //from exit
                Network[currentN].Link[nLink].toNode = fjNode; // to newNode (spitted from exit Node)

                nSplit++;
                nNode++; //nNode < nNode in extend_network
                nLink++;

                //Consider all outgoing edges from exit to the nodes which are inside the current loop === (new move from above) crazy <===
                for (int j = 0; j < nLink; j++)
                {
                    for (int j2 = 0; j2 < cntB; j2++) //consider the node inside the loop which have outgoing edges to loop entry
                    {
                        //ajust the link from outside the loop to this entry node. preB[] set node inside loop ; preF[] set of node outside the loop
                        if (Network[currentN].Link[j].toNode == preB[j2] && Network[currentN].Link[j].fromNode == node) //find the links from Exit to the Nodes inside the loop
                        {
                            Network[currentN].Link[j].fromNode = fjNode; //fjNode is the new node
                            find_NodeInform(currentN, Network[currentN].Link[j].toNode);
                        }
                    }
                }
                find_NodeInform(currentN, fjNode); //inform for new node (after split type 2 extension)
                find_NodeInform(currentN, bjNode); //inform for this loop's entry
                //if (Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(workLoop, node, fjNode, Loop[workLoop].Loop[i].parentLoop);
            }
            


            //========== END  ==============================================================================
        }

        private int Type_II_Split_Exit(int currentN, int workLoop, int nNode, int nLink)
        {
            int nSplit = 0;

            for (int i = 0; i < Loop[workLoop].nLoop; i++)
            {
                for (int k = 0; k < Loop[workLoop].Loop[i].nExit; k++)
                {

                    int node = Loop[workLoop].Loop[i].Exit[k];

                    int[] postF = new int[nNode];
                    int[] postB = new int[nNode];
                    int cntF = 0, cntB = 0;

                    for (int j = 0; j < nLink; j++)
                    {
                        if (Network[currentN].Link[j].fromNode == node)
                        {

                            if (Node_In_Loop(workLoop, Network[currentN].Link[j].toNode, i))
                            {
                                postB[cntB] = Network[currentN].Link[j].toNode;
                                cntB++;
                            }
                            else
                            {
                                postF[cntF] = Network[currentN].Link[j].toNode;
                                cntF++;
                            }
                        }
                    }

                    if (cntF > 1) // For join이 2 미만이면
                    {
                        int fsNode = nNode;
                        int bsNode = node;

                        //For Spilt Node - 추가
                        Network[currentN].Node[fsNode].Kind = Network[currentN].Node[node].Kind;
                        Network[currentN].Node[fsNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                        Network[currentN].Node[fsNode].orgNum = node;
                        Network[currentN].Node[fsNode].parentNum = fsNode;
                        Network[currentN].Node[fsNode].Type_I = Network[currentN].Node[node].Type_I;
                        Network[currentN].Node[fsNode].Type_II = Network[currentN].Node[node].Type_II + "_xfs";
                        Network[currentN].Node[fsNode].Special = "";

                        //Back Spilt Node - 변경
                        Network[currentN].Node[bsNode].Type_II = Network[currentN].Node[node].Type_II + "_xbs";

                        //New Link 추가
                        Network[currentN].Link[nLink].fromNode = bsNode;
                        Network[currentN].Link[nLink].toNode = fsNode;


                        nSplit++;
                        nNode++;
                        nLink++;

                        //기존 Link 정보 변경

                        for (int j = 0; j < nLink; j++)
                        {
                            for (int j2 = 0; j2 < cntF; j2++)
                            {
                                if (Network[currentN].Link[j].fromNode == node && Network[currentN].Link[j].toNode == postF[j2])
                                {
                                    Network[currentN].Link[j].fromNode = fsNode;
                                    find_NodeInform(currentN, Network[currentN].Link[j].toNode);
                                }
                            }
                        }

                        find_NodeInform(currentN, fsNode);
                        find_NodeInform(currentN, bsNode);

                        if (Loop[workLoop].Loop[i].parentLoop >= 0) Loop_Inform(workLoop, node, fsNode, Loop[workLoop].Loop[i].parentLoop);
                    }
                    //Extention Type 2 Exit
                    //Type_II_Extension_Exit(currentN, workLoop, i, nNode, nLink, node, ref nSplit, postB, cntB);
                }
            }

            return nSplit;
        }

        private void Type_II_Extension_Exit(int currentN, int workLoop, int currentWorkLoop, int nNode, int nLink, int currentNode, ref int nSplit, int[] nodesInsideLoop, int nNodeInsideLoop)
        {
            int i = currentWorkLoop;
            int node = currentNode;
            int[] preB = nodesInsideLoop; //all node from inside the loop
            int cntB = nNodeInsideLoop;

            //========== EXTENTION ========================================================================(1 bai hoc nho doi)
            //if there are at least 2 incoming edges from inside the loop to this entry => extend type II 

            if (isInsideLoop(currentN, workLoop, i, node, "ex") == true) //problem here
            {
                //==New code===
                int fjNode = nNode; //New Node
                int bjNode = node; //Current Entry
                //create new node
                Network[currentN].Node[fjNode].Kind = Network[currentN].Node[node].Kind;
                Network[currentN].Node[fjNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                Network[currentN].Node[fjNode].orgNum = node;
                Network[currentN].Node[fjNode].parentNum = fjNode;
                Network[currentN].Node[fjNode].Type_I = Network[currentN].Node[node].Type_I;
                Network[currentN].Node[fjNode].Type_II = Network[currentN].Node[node].Type_II + "_ex_extn";
                //create new link
                Network[currentN].Link[nLink].fromNode = bjNode; //from exit
                Network[currentN].Link[nLink].toNode = fjNode; // to newNode (spitted from exit Node)

                nSplit++;
                nNode++; //nNode < nNode in extend_network
                nLink++;

                //Consider all outgoing edges from exit to the nodes which are inside the current loop === (new move from above) crazy <===
                for (int j = 0; j < nLink; j++)
                {
                    for (int j2 = 0; j2 < cntB; j2++) //consider the node inside the loop which have outgoing edges to loop entry
                    {
                        //ajust the link from outside the loop to this entry node. preB[] set node inside loop ; preF[] set of node outside the loop
                        if (Network[currentN].Link[j].toNode == preB[j2] && Network[currentN].Link[j].fromNode == node) //find the links from Exit to the Nodes inside the loop
                        {
                            Network[currentN].Link[j].fromNode = fjNode; //fjNode is the new node
                            find_NodeInform(currentN, Network[currentN].Link[j].toNode);
                        }
                    }
                }
                find_NodeInform(currentN, fjNode); //inform for new node (after split type 2 extension)
                find_NodeInform(currentN, bjNode); //inform for this loop's entry
                if (Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(workLoop, node, fjNode, Loop[workLoop].Loop[i].parentLoop);
            }


            //========== END  ==============================================================================
        }
        
        private bool Node_In_SESE(int workSESE, int node, int currSESE)
        {
            bool inside = false;
            for (int i = 0; i < SESE[workSESE].SESE[currSESE].nNode; i++ )
            {
                if (SESE[workSESE].SESE[currSESE].Node[i] == node)
                {
                    inside = true;
                    break;
                }
            }
            if (inside)
            {
                for (int i = 0; i < SESE[workSESE].SESE[currSESE].nChild; i++)
                {
                    if (currSESE != i)
                    {
                        for (int j = 0; j < SESE[workSESE].SESE[SESE[workSESE].SESE[currSESE].child[i]].nNode; j++)
                        {
                            if (SESE[workSESE].SESE[SESE[workSESE].SESE[currSESE].child[i]].Node[j] == node)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            else return false;
            return true;
        }
        private bool Node_In_Loop(int workLoop, int node, int loop) //node in loop => return true; not in loop => return false
        {
            bool inLoop = false;

            if (node == Loop[workLoop].Loop[loop].header)
            {
                inLoop = true;
            }
            else
            {
                for (int i = 0; i < Loop[workLoop].Loop[loop].nNode; i++)
                {
                    if (node == Loop[workLoop].Loop[loop].Node[i])
                    {
                        inLoop = true;
                        break;
                    }
                }
            }
            if (!inLoop)
            {
                for (int i = 0; i < Loop[workLoop].Loop[loop].nChild; i++)
                {
                    inLoop = Node_In_Loop(workLoop, node, Loop[workLoop].Loop[loop].child[i]);
                    if (inLoop) break;
                }
            }

            return inLoop;
        }

        private void Loop_Inform(int workLoop, int rNode, int node, int loop)
        {
            //포함 노드 수정
            int nNew = Loop[workLoop].Loop[loop].nNode + 1;
            int[] New = new int[nNew];

            for (int i = 0; i < Loop[workLoop].Loop[loop].nNode; i++)
            {
                New[i] = Loop[workLoop].Loop[loop].Node[i];
            }
            New[nNew - 1] = node;

            Loop[workLoop].Loop[loop].nNode = nNew;
            Loop[workLoop].Loop[loop].Node = New;

            //entry 수정
            for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++)
            {
                if (Loop[workLoop].Loop[loop].Entry[i] == rNode) Loop[workLoop].Loop[loop].Entry[i] = node;

            }


            //exit 수정
            for (int i = 0; i < Loop[workLoop].Loop[loop].nExit; i++)
            {
                if (Loop[workLoop].Loop[loop].Exit[i] == rNode) Loop[workLoop].Loop[loop].Exit[i] = node;

            }

        }



        #endregion

        #region Verify Simple structure of SS and EE (Verify BONDs) Not Necessary anymore

        public void VerifyBonds(int currentN, int currentSESE)
        {
            int curDepth = SESE[currentSESE].maxDepth;
            do
            {
                for (int i = 0; i < SESE[currentSESE].nSESE; i++)
                {
                    if (SESE[currentSESE].SESE[i].depth != curDepth) continue;

                    if (Bond_Check(currentN, currentSESE, i)) //bond models (can check for reduced model)
                    {
                        int En = SESE[currentSESE].SESE[i].Entry;
                        int Ex = SESE[currentSESE].SESE[i].Exit;
                        if (Network[currentN].Node[En].Name == "SS" && Network[currentN].Node[En].Kind != Network[currentN].Node[Ex].Kind)
                        {
                            Network[currentN].Node[En].Kind = Network[currentN].Node[Ex].Kind;
                            
                        }
                    }
                    else //rigid models
                    {
                        int En = SESE[currentSESE].SESE[i].Entry;
                        if (Network[currentN].Node[En].Name == "SS")
                            Network[currentN].Node[En].SOS_Corrected = true;
                    }
                    reduce_seseNetwork(currentN, currentSESE, i);
                }

                curDepth--;
            } while (curDepth > 0);
        }

        #endregion //Not necessary anymore

        #region Check Loop

        private void Run_CheckLoop()
        {
            if (IrreducibleError || ConcurrencyError) return; //concurrencyError == true => Return
            // Network(3) 생성후 작업

            Network[reduceNet] = Network[finalNet];
            extent_Network(reduceNet, 0);

            copy_Loop(orgLoop, reduceLoop);

            nError = 0;
            Error = new strError[Network[reduceNet].nNode * 3];
            initiate_Error(Network[reduceNet].nNode * 3);

            check_by_Rule2(reduceNet, reduceLoop, finalSESE, "");
        }

        public void initiate_Error(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Error[i].currentKind = "";
                Error[i].Loop = "";
                Error[i].Node = "";
                Error[i].SESE = "";
            }
        }

        private void check_by_Rule1(int currentN, int workLoop, int loop, string strLoop)
        {
            if (Loop[workLoop].Loop[loop].Irreducible) return;

            for (int i = 0; i < Network[currentN].nNode; i++)
            {

                //if (!Node_In_Loop(workLoop, i, loop)) continue;
                //check node in loop;
                bool check = false;
                for (int k = 0; k < Loop[workLoop].Loop[loop].nNode; k++)
                {
                    if (Loop[workLoop].Loop[loop].header == i)
                    {
                        check = true;
                        break;
                    }
                    if (Loop[workLoop].Loop[loop].Node[k] == i)
                    {
                        check = true;
                        break;
                    }
                }
                if (check == false) continue;

                if (Network[currentN].Node[i].Special == "" || Network[currentN].Node[i].Special == null) continue;

                if (Network[currentN].Node[i].Kind == "XOR") continue; // error 아님 (//free)


                Error[nError].Loop = strLoop;
                Error[nError].Node = Network[currentN].Node[i].parentNum.ToString();
                //Error[nError].Node = i.ToString();
                Error[nError].currentKind = Network[currentN].Node[i].Kind;


                if (Network[currentN].Node[i].Special == "E") Error[nError].messageNum = 0;
                else if (Network[currentN].Node[i].Special == "X" || Network[currentN].Node[i].Special == "T") Error[nError].messageNum = 1;
                else Error[nError].messageNum = 2;


                //nError++;
                add_Error();

            }
        }

        private void check_by_Rule5_New(int currentN, int workLoop, int loop, string strLoop)
        {
            if (!Loop[workLoop].Loop[loop].Irreducible) return;
            if (Loop[workLoop].Loop[loop].nConcurrency == 0)
            {
                for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++) //rule 5.1
                {
                    if (Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].Kind != "XOR")
                    {
                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].Kind;

                        Error[nError].messageNum = 16;
                        add_Error();
                    }
                }
                for (int i = 0; i < Loop[workLoop].Loop[loop].nExit; i++) //rule 5.1
                {
                    if (Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].Kind != "XOR")
                    {
                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].Kind;

                        Error[nError].messageNum = 17;
                        add_Error();
                    }
                }
            }
            else //rule 5.2...
            {
                for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++)
                {
                    if (Loop[workLoop].Loop[loop].Concurrency[i] == 0)
                    {
                        if ((Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].Kind != "XOR") && (Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].nPre > 1))
                        {
                            Error[nError].Loop = strLoop;
                            Error[nError].Node = Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].parentNum.ToString();
                            Error[nError].currentKind = Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].Kind;

                            Error[nError].messageNum = 18;
                            add_Error();
                        }
                    }
                    else
                    {
                        if ((Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].Kind == "AND") && (Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].nPre > 1))
                        {
                            Error[nError].Loop = strLoop;
                            Error[nError].Node = Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].parentNum.ToString();
                            Error[nError].currentKind = Network[currentN].Node[Loop[workLoop].Loop[loop].Entry[i]].Kind;

                            Error[nError].messageNum = 19;
                            add_Error();
                        }
                    }
                }
                if ((Network[currentN].Node[Loop[workLoop].Loop[loop].header].Kind != "XOR") && (Network[currentN].Node[Loop[workLoop].Loop[loop].header].nPre > 1))
                {
                    Error[nError].Loop = strLoop;
                    Error[nError].Node = Network[currentN].Node[Loop[workLoop].Loop[loop].header].parentNum.ToString();
                    Error[nError].currentKind = Network[currentN].Node[Loop[workLoop].Loop[loop].header].Kind;

                    Error[nError].messageNum = 20;
                    add_Error();
                }
                for (int i = 0; i < Loop[workLoop].Loop[loop].nExit; i++)
                {
                    if ((Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].Kind != "XOR") && (Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].nPost > 1))
                    {
                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].Kind;

                        Error[nError].messageNum = 21;
                        add_Error();
                    }

                }
            }

        }

        private void check_by_Rule5(int currentN, int workLoop, int loop, string strLoop)
        {
            if (!Loop[workLoop].Loop[loop].Irreducible) return;

            for (int i = 0; i < Network[currentN].nNode; i++)
            {

                if (!Node_In_Loop(workLoop, i, loop)) continue;

                if (Network[currentN].Node[i].Special == "" || Network[currentN].Node[i].Special == null) continue;

                if (Network[currentN].Node[i].Kind == "XOR") continue; // error 아님

                if (Loop[workLoop].Loop[loop].Irreducible && Network[currentN].Node[i].Special == "B") continue;

                //Cuncurrency면
                bool bCon = false;
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    if (Loop[workLoop].Loop[loop].Entry[k] == i)
                    {
                        if (Loop[workLoop].Loop[loop].Concurrency[k] != 0)
                        {
                            bCon = true;
                            break;
                        }
                    }
                }
                if (bCon) continue;


                Error[nError].Loop = strLoop;
                Error[nError].Node = Network[currentN].Node[i].parentNum.ToString();
                Error[nError].currentKind = Network[currentN].Node[i].Kind;


                if (Network[currentN].Node[i].Special == "E") Error[nError].messageNum = 12;
                else Error[nError].messageNum = 13;//Exit 


                //nError++;
                add_Error();

            }
        }

        private void check_by_Rule7(int currentN, int workLoop, int loop, string strLoop)
        {

            for (int i = 0; i < Network[currentN].nNode; i++)
            {

                bool bEntry = false;
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    if (Network[currentN].Node[i].orgNum == Loop[workLoop].Loop[loop].Entry[k])
                    {
                        bEntry = true;
                        break;
                    }
                }
                if (!bEntry) continue;

                if (Network[currentN].Node[i].Kind == "XOR") continue; // error 아님

                bool bError = true;
                if (Network[currentN].Node[i].nPre > 0 && Network[currentN].Node[i].Kind == "OR")
                {
                    bError = false;
                }
                if (!bError) continue;


                Error[nError].Loop = strLoop;
                Error[nError].Node = Network[currentN].Node[i].parentNum.ToString();
                Error[nError].currentKind = Network[currentN].Node[i].Kind;


                Error[nError].messageNum = 15;//Exit 

                //nError++;
                add_Error();

            }
        }

        private void check_by_Rule2(int currentN, int workLoop, int workSESE, string strLoop)
        {
            string strOrgLoop = strLoop;
            //int curDepth = Loop[workLoop].maxDepth;
            int curDepth = FBLOCK.maxDepth;
            //informList[13] = 0; //# of bonds
            //informList[14] = 0; //# of rigids
            //It mean: the data structure we use was insprised from Loop-nesting forest
            do
            {
                //for (int i = 0; i < Loop[workLoop].nLoop; i++)
                for (int j = 0; j < FBLOCK.nFBlock; j++)
                {
                    //if (Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (FBLOCK.FBlock[j].depth != curDepth) continue;

                    //if (strOrgLoop == "") strLoop = j.ToString();
                    //else strLoop = strOrgLoop + "-" + j.ToString();

                    int i = FBLOCK.FBlock[j].refIndex;

                    if (strOrgLoop == "") strLoop = i.ToString();
                    else strLoop = strOrgLoop + "-" + i.ToString();

                    if (FBLOCK.FBlock[j].SESE) //If SESE => verify SESE
                    {
                        if (i == 9)
                        { }
                        if (Bond_Check(currentN, workSESE, i)) //bond model
                        {
                            //verify some easy Bond (SS, EE) Example => 1Ex_dwdy.net (just for SESE which have Entry or Exit are SS or EE
                            if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Name == "SS")
                            {
                                Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Network[currentN].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;

                            }
                            if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Name == "EE")
                            {
                                Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind;
                                Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind;
                            }
                            //check rule for the rest SESE
                            //informList[13]++;
                            if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind == Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind) { }
                            else if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind == "OR") { }
                            else if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind == "XOR")
                            {
                                Error[nError].currentKind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                Error[nError].messageNum = 27;
                                Error[nError].Node = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].parentNum.ToString();
                                Error[nError].SESE = i.ToString();
                                add_Error();
                            }
                            else if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind == "AND")
                            {
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
                            if (all_same_kind(currentN, workSESE, i)) 
                            {
                                if (Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Name == "SS")
                                {
                                    Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                    Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind;
                                    Network[currentN].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                    Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                }
                                if (Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Name == "EE")
                                {
                                    Network[currentN].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[currentN].Node[SESE[workSESE].SESE[i].Entry].Kind;
                                    Network[finalNet].Node[SESE[workSESE].SESE[i].Exit].Kind = Network[finalNet].Node[SESE[workSESE].SESE[i].Entry].Kind;
                                }
                            } //no errors for rigid
                            else if (all_OR_join(currentN, workSESE, i)) {}
                            else
                            {
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
                    else //if Loop => verify Loop
                    {
                        if (Loop[workLoop].Loop[i].Irreducible) // Irreducible Loop면
                        {
                            ///Concurrency Check - 새로이 작업
                            check_Concurrency(currentN, workLoop, i); //Find a CEs set - set of nodes which are concurrency (OF EACH LOOP) => we will have iDFLOW()
                            //check_by_Rule5_New(currentN, workLoop, i, strLoop); // we move it to below => after check Concurrency

                            //== NEW =========
                            bool check_IrrError = false;
                            for (int k = 0; k < Loop[workLoop].Loop[i].nConcurrency; k++)
                            {
                                inspect_Concurrency(currentN, workLoop, i, k + 1); //GET VALUE TO ConcurrencyError 
                                //ConcurrencyError = false;
                                //if (!ConcurrencyError)
                                {
                                    if (!clone_InstanceLoop(currentN, tempLoop, i, k + 1, strLoop, j)) //find DFlow; PdFlow; iDFlow, and the rest of subgraph => run Instance flow to find error in acyclic graphs type\
                                    { //clone == true => can be decomposed; == false => can not be decomposed; violation rule 7
                                        check_IrrError = true;
                                        break;
                                    }
                                }
                            }

                            //== End NEW =========
                            if (check_IrrError)
                            {
                                reduce_Network(currentN, workLoop, i, strLoop, true);
                                continue; // break big FOR
                            }
                            else
                            {
                                check_by_Rule5_New(currentN, workLoop, i, strLoop);
                            }

                            #region remove old concurrency check code
                            /*
                            ///Concurrency인 경우 우선 Check                                                
                            //========================================================================== OLD ====================
                            for (int k = 0; k < Loop[workLoop].Loop[i].nConcurrency; k++)
                            {
                                inspect_Concurrency(currentN, workLoop, i, k + 1); //GET VALUE TO ConcurrencyError 
                                if (ConcurrencyError) return;
                                //=================================================

                                make_subNetwork(currentN, subNet, tempLoop, i, "ICC", k + 1);    //5는 SubNetwork //From CE set to cIPd node (cIPd Flow)
                                check_by_Rule7(subNet, tempLoop, i, strLoop);

                                //Forward 검사

                                make_subNetwork(currentN, seseNet, tempLoop, i, "ICF", k + 1); // == DFlow + PdFlow ==

                                Check_SESENetwork(seseNet, orgSESE, i, true, strLoop);

                                make_subNetwork(redSeNet, subNet, workLoop, -1, "AC", -1);    //5는 SubNetwork
                                make_InstanceFlow(subNet, i, true, strLoop); //Error will be indicated here============

                                //Backward 검사

                                make_subNetwork(currentN, seseNet, tempLoop, i, "ICB", k + 1); // == iDFlow + PdFlow ===

                                Check_SESENetwork(seseNet, orgSESE, i, true, strLoop);

                                make_subNetwork(redSeNet, subNet, workLoop, -1, "AC", -1);    //5는 SubNetwork
                                make_InstanceFlow(subNet, i, true, strLoop); //Error will be indicated here===========


                                /////////////////////////
                                // 축소후 검사
                                find_includeNode(currentN, tempLoop, i, "ICB", k + 1);
                                make_ConcurrencyIrreducible(dummyNet, nickNet, workLoop, i, k + 1);

                                After_Type_II_Split(nickNet, subLoop);

                                //임시저장 //temporary save
                                strNetwork saveNetwok = Network[currentN];
                                strLoop saveLoop = Loop[workLoop];
                                strNetwork IrrNetwok = Network[dummyNet];

                                Network[currentN] = Network[nickNet];
                                extent_Network(currentN, 0);

                                //Loop[workLoop] = Loop[subLoop];
                                copy_Loop(subLoop, workLoop);

                                check_by_Rule2(currentN, workLoop, strLoop);

                                //환원 // restoration => phục hồi lại.
                                Network[currentN] = saveNetwok;
                                Loop[workLoop] = saveLoop;
                                Network[dummyNet] = IrrNetwok;
                            }
                            */
                            #endregion

                            make_subNetwork(currentN, dummyNet, workLoop, i, "IR", -1);

                            for (int k = 0; k < Loop[workLoop].Loop[i].nEntry; k++)
                            {
                                if (Loop[workLoop].Loop[i].Concurrency == null) continue;
                                if (Loop[workLoop].Loop[i].Concurrency[k] != 0) continue;

                                make_IndependentIrreducible(dummyNet, nickNet, Loop[workLoop].Loop[i].Entry[k]);
                                //return;

                                After_Type_II_Split(nickNet, subLoop);
                                //find_SESE(currentN, orgSESE, -1);
                                STRBLOCK tempBlock = new STRBLOCK();
                                

                                //임시저장 //temporary save!
                                strNetwork saveNetwok = Network[currentN];
                                strLoop saveLoop = Loop[workLoop];
                                strNetwork IrrNetwok = Network[dummyNet];
                                string tempS = strLoop;
                                strLoop = strLoop + " -NL- " + "("+ Network[currentN].Node[Loop[workLoop].Loop[i].Entry[k]].Name + Network[currentN].Node[Loop[workLoop].Loop[i].Entry[k]].Type_I + Network[currentN].Node[Loop[workLoop].Loop[i].Entry[k]].Type_II + ")";
                                Network[currentN] = Network[nickNet];
                                extent_Network(currentN, 0);

                                //return;
                                //Loop[workLoop] = Loop[subLoop];
                                copy_Loop(subLoop, workLoop);
                                copy_FBlock(FBLOCK, ref tempBlock);
                                make_NestingForest(currentN, -1, workLoop);
                                //copy_FBlock(subBlock, workBlock);

                                //check_by_Rule2(currentN, workLoop, strLoop);
                                //return;
                                //naturalLoop_Verify(currentN, workLoop, 0, strLoop); //because it will have just only 1 loop in here => i = 0;
                                //Run_FindLoop(currentN, newTempLoop);
                                check_by_Rule2(currentN, workLoop, workSESE, strLoop);

                                //환원
                                Network[currentN] = saveNetwok;
                                Loop[workLoop] = saveLoop;
                                Network[dummyNet] = IrrNetwok;
                                copy_FBlock(tempBlock, ref FBLOCK);
                                strLoop = tempS;
                            }
                            reduce_Network(currentN, workLoop, i, strLoop, true);
                        }
                        else // Natural Loop면
                        {
                            //naturalLoop_Verify(currentN, workLoop, i, strLoop);
                        }
                    }
                }

                curDepth--;
            } while (curDepth > 0);

        }
        public void naturalLoop_Verify(int currentN, int workLoop, int i, string strLoop)
        {
            check_by_Rule1(currentN, workLoop, i, strLoop);

            //Forward Check
            //1) Loop내 FF Network 만들어
            make_subNetwork(currentN, seseNet, workLoop, i, "FF", -1);   //5는 SubNetwork
            //check_ParallelFlow(seseNet, i, true, strLoop); // (return errors - concurrent structures)
            check_ParallelStructure(seseNet, "efwd", strLoop);

            make_InstanceFlow(seseNet, i, "eFwd", strLoop); //New code
            
            //WHY WE NEED IT ?? WHY WE MUST RUN INSTANT FLOW FOR THE BIG MODEL
            //Check_SESENetwork(seseNet, orgSESE, i, true, strLoop); //find SESE, Split III, Reduce SESE =>> All will be stored in Network[redSeNet]
            /*Network[redSeNet] = Network[currentN];
            extent_Network(redSeNet, 0);

            make_subNetwork(redSeNet, subNet, workLoop, -1, "AC", -1);    //5는 SubNetwork // make acylic network (reduce all loop or SESE)
            make_InstanceFlow(subNet, i, true, strLoop); //check instanceFlow of "subNet = 6" => acyclic network*/

            //Backward Check ====================
            make_subNetwork(currentN, seseNet, workLoop, i, "BF", -1);   //5는 SubNetwork
            //check_ParallelFlow(seseNet, i, false, strLoop);
            check_ParallelStructure(seseNet, "ebwd", strLoop);

            make_InstanceFlow(seseNet, i, "eBwd", strLoop); //new code

            //Check_SESENetwork(seseNet, orgSESE, i, false, strLoop);
            /*Network[redSeNet] = Network[currentN];
            extent_Network(redSeNet, 0);

            make_subNetwork(redSeNet, subNet, workLoop, -1, "AC", -1);    //5는 SubNetwork
            make_InstanceFlow(subNet, i, false, strLoop);*/

            reduce_Network(currentN, workLoop, i, strLoop, true);
        }
        public void check_ParallelStructure(int currentN, string flowType, string strLoop)
        {
            preProcessingSESE(currentN, tempSESE, -1);
            find_SESE_new(currentN, tempSESE, -1);
            for (int i = 0; i < SESE[tempSESE].nSESE; i++)
            {
                if (Bond_Check(currentN, tempSESE, i)) //if bond => check and inform errors
                {
                    for (int j = 0; j < SESE[tempSESE].SESE[i].nNode; j++)
                    {
                        int node = SESE[tempSESE].SESE[i].Node[j];
                        if ((Network[currentN].Node[node].Special == "X") || (Network[currentN].Node[node].Special == "B"))
                        {
                            int entry = SESE[tempSESE].SESE[i].Entry;
                            if (Network[currentN].Node[entry].Kind == "OR" || Network[currentN].Node[entry].Kind == "AND") //error // Check whether the AND Split to this node exist or not => if exist (return 1) => error - conflic here
                            {                                           //because before Backward split and Exit node do not allow AND Split => 1 token left 1 token in the loop => error
                                Error[nError].Loop = strLoop;
                                Error[nError].Node = Network[currentN].Node[node].parentNum.ToString();
                                Error[nError].currentKind = Network[currentN].Node[node].Kind;

                                if (flowType == "efwd") Error[nError].messageNum = 7;
                                if (flowType == "ebwd") Error[nError].messageNum = 8;
                                if (flowType == "HFlow") Error[nError].messageNum = 31;

                                //nError++;
                                add_Error();

                            }
                        }
                    }
                }
                else //if rigid => just warning errors
                {
                    for (int j = 0; j < SESE[tempSESE].SESE[i].nNode; j++)
                    {
                        int node = SESE[tempSESE].SESE[i].Node[j];
                        if ((Network[currentN].Node[node].Special == "X") || (Network[currentN].Node[node].Special == "B"))
                        {
                            //int entry = SESE[tempSESE].SESE[i].Entry;
                            //if (Network[currentN].Node[entry].Kind == "OR" || Network[currentN].Node[entry].Kind == "AND") //error // Check whether the AND Split to this node exist or not => if exist (return 1) => error - conflic here
                            {                                           //because before Backward split and Exit node do not allow AND Split => 1 token left 1 token in the loop => error
                                Error[nError].Loop = strLoop;
                                Error[nError].Node = Network[currentN].Node[node].parentNum.ToString();
                                Error[nError].currentKind = Network[currentN].Node[node].Kind;

                                if (flowType == "efwd") Error[nError].messageNum = 29;
                                if (flowType == "ebwd") Error[nError].messageNum = 30;
                                if (flowType == "HFlow") Error[nError].messageNum = 31;

                                //nError++;
                                add_Error();

                            }

                        }
                    }

                }
            }
        }
        public int getFblockIndex(int currentSESE)
        {
            for (int i = 0; i < FBLOCK.nFBlock; i++)
            {
                if (currentSESE == FBLOCK.FBlock[i].refIndex)
                    return i;
            }
            return -1;
        }
        public bool Bond_Check(int currentN, int workSESE, int currentSESE) //should upgrade bond check ===
        {
            int count_gateway = 0;
            //check SESE nested with Loops (2 cases) (if SESE have at least 1 directly child is loop => rigid.
            int f_index = getFblockIndex(currentSESE);
            for (int i = 0; i < FBLOCK.FBlock[f_index].nChild; i++ )
            {
                int child = FBLOCK.FBlock[f_index].child[i];
                if (FBLOCK.FBlock[child].SESE == false && !(FBLOCK.FBlock[child].nEntry == 1 && FBLOCK.FBlock[child].nExit == 1)) return false;
            }

            for (int i = 0; i < SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = SESE[workSESE].SESE[currentSESE].Node[i];
                if (Node_In_SESE(workSESE, node, currentSESE))
                {
                    if (Network[currentN].Node[node].nPre > 1 || Network[currentN].Node[node].nPost > 1)
                    {
                        count_gateway++;
                        if (count_gateway > 2) return false;
                    }
                }
            }
            return true;
        }
        public bool all_same_kind(int currentN, int workSESE, int currentSESE)
        {
            string Gateway_kind = "";
            for (int i = 0; i < SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = SESE[workSESE].SESE[currentSESE].Node[i];
                if (i == 0) Gateway_kind = Network[currentN].Node[node].Kind;
                if (Network[currentN].Node[node].nPre > 1 || Network[currentN].Node[node].nPost > 1)
                {
                    if (Network[currentN].Node[node].Kind != Gateway_kind) return false;
                }
            }
            return true;
        }
        public bool all_OR_join(int currentN, int workSESE, int currentSESE)
        {
            for (int i = 0; i < SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = SESE[workSESE].SESE[currentSESE].Node[i];
                if (Network[currentN].Node[node].nPre > 1) //consider Join-node
                {
                    if (Network[currentN].Node[node].Kind != "OR") return false;
                }
            }
            return true;
        }


        //create a Sub-network base on current instance entries trigged.
        public bool clone_InstanceLoop(int currentN, int workLoop, int current_Loop, int current_ConEnSet, string strLoop, int fblockIndex) //tam thoi void da
        {
            int CID = -1;
            int CIPd = -1;
            int iCID = -1;
            //find current DFLOW; => DONE ==================================================== => Storing in subNet
            //find all path from CID to current entry Set
            bool bFor = true;
            find_Node_DFlow(currentN, workLoop, current_Loop, "CC", 1, current_ConEnSet, ref CID, ref bFor); // because conNet store full network;
            //=========================================DONE DFLOW ==============================
            
            //find PDFLOW ======
            find_Node_PdFlow(currentN, workLoop, current_Loop, "CC", 1, current_ConEnSet, ref CIPd);
            check_SearchNode_Same(ref searchNode, ref nSearchNode);
            if (!check_CIPd(workLoop, CIPd, fblockIndex))
            {
                Error[nError].Loop = strLoop;
                Error[nError].Node = "-1";
                Error[nError].currentKind = "";
                Error[nError].messageNum = 32;
                add_Error();
                return false; //Check CIPd out of the loop; rule 7.1
            }
            //Add Node
            Network[subNet] = new strNetwork();
            Network[subNet].nNode = nSearchNode;
            Network[subNet].Node = new strNode[Network[subNet].nNode];
            for (int i = 0; i < nSearchNode; i++)
            {
                Network[subNet].Node[i] = Network[currentN].Node[searchNode[i]];
                Network[subNet].Node[i].orgNum = searchNode[i];
            }
            //Add Link
            make_subLink_2(currentN, subNet, workLoop, current_Loop, "CC", current_ConEnSet, CID, CIPd);
            //find node inform
            for (int i = 0; i < Network[subNet].nNode; i++)
            {
                find_NodeInform(subNet, i);
            }
            find_Dom(subNet);
            find_DomEI(subNet, -1);
            find_DomRev(subNet);
            find_DomRevEI(subNet);

            Network[DFlow_PdFlow] = Network[subNet];
            extent_Network(DFlow_PdFlow, 0);
            for (int i = 0; i < Network[DFlow_PdFlow].nNode; i++)
            {
                if (Network[DFlow_PdFlow].Node[i].Name == CID.ToString()) Network[DFlow_PdFlow].header = i;
            }
            //Instance Flow of DFlow + PdFlow => check error and store in ERROR[]
            
            //=========

            find_Node_iDFlow(workLoop, current_Loop, currentN, subNet, current_ConEnSet, ref iCID, CIPd, "iCID_CIPd"); //all done in there (find node + Link) => store in toN           
            if (!check_CIPd(workLoop, iCID, fblockIndex))
            {
                Error[nError].Loop = strLoop;
                Error[nError].Node = "-1";
                Error[nError].currentKind = "";
                Error[nError].messageNum = 33;
                add_Error();
                return false; //check iCID in the loop; rule 7.2
            }
            Network[iDFlow_PdFlow] = Network[subNet];
            extent_Network(iDFlow_PdFlow, 0);
            for (int i = 0; i < Network[iDFlow_PdFlow].nNode; i++)
            {
                if (Network[iDFlow_PdFlow].Node[i].Name == iCID.ToString()) Network[iDFlow_PdFlow].header = i;
            }
            //instance Flow iDFlow_PdFlow
            
            
            //=============================
            find_Node_iDFlow(workLoop, current_Loop, currentN, subNet, current_ConEnSet, ref iCID, CIPd, "CIPd_iCID"); //all done in there (find node + Link) => store in toN
            if (!check_CIPd(workLoop, iCID, fblockIndex))//maybe not necessary
            {
                Error[nError].Loop = strLoop;
                Error[nError].Node = "-1";
                Error[nError].currentKind = "";
                Error[nError].messageNum = 33;
                add_Error();
                return false; //check iCID in the loop; rule 7.2
            }
            Network[theRestFlow] = Network[subNet];
            extent_Network(theRestFlow, 0);
            for (int i = 0; i < Network[theRestFlow].nNode; i++) //find the header (CIPd)
            {
                if (Network[theRestFlow].Node[i].Name == CIPd.ToString()) Network[theRestFlow].header = i;
            }
            //intance flow PdFlow_iDFlow======================

            //until here we have 3 subgraph
            nSearchNode = Network[iDFlow_PdFlow].nNode + Network[theRestFlow].nNode;
            searchNode = new int[nSearchNode]; //not count the iCID and CIPd
            int index_searchNode = 0;
            for (int i = 0 ; i < Network[iDFlow_PdFlow].nNode; i++)
            {
                searchNode[index_searchNode] = Network[iDFlow_PdFlow].Node[i].orgNum;
                index_searchNode++;
            }
            for (int i = 0 ; i < Network[theRestFlow].nNode; i++)
            {
                searchNode[index_searchNode] = Network[theRestFlow].Node[i].orgNum;
                index_searchNode++;
            }
            // => we have searchNode again
            check_SearchNode_Same(ref searchNode, ref nSearchNode);

            //Put virtual START and END => for new network => for breaking into small part and doing check_Rule2() again
            make_subNode_2(currentN, subNet, true, "START", true, "END");

            //make_subLink(conNet, subNet, workLoop, current_Loop, "II", true, Loop[workLoop].Loop[current_Loop].Entry, true, Loop[workLoop].Loop[current_Loop].Exit, current_ConEnSet);
            make_subLink_2(currentN, subNet, workLoop, current_Loop, "IR", current_ConEnSet, CIPd, iCID);
            for (int i = 0; i < Network[subNet].nNode; i++)
            {
                find_NodeInform(subNet, i);
            }
            //START and END not good
            find_Dom(subNet);
            find_DomEI(subNet, -1);
            find_DomRev(subNet);
            find_DomRevEI(subNet);
            //Until here => we have a a subgraph which have only this irreducible loop     

            //check rule 7 - PdFlow() and iDFlow() should not contain exit
            check_rule7_exit(DFlow_PdFlow, true, strLoop);
            check_rule7_exit(iDFlow_PdFlow, false, strLoop);

            //check rule 6.4
            check_ParallelStructure(theRestFlow, "HFlow", strLoop);

            make_InstanceFlow(DFlow_PdFlow, current_Loop, "DFlow_PdFlow", strLoop);
            make_InstanceFlow(iDFlow_PdFlow, current_Loop, "iDFlow_PdFlow", strLoop);
            make_InstanceFlow(theRestFlow, current_Loop, "HFlow", strLoop);

            return true;
        }
        public void check_rule7_exit(int currentN, bool bFor, string strLoop) //bFor == true => DFlow_PdFlow; bFor == false => iDFlow_PdFlow
        {
            for (int i = 0; i < Network[currentN].nNode; i++)
                if (Network[currentN].Node[i].Special == "X") //conflict rule 7.2
                {
                    Error[nError].Loop = strLoop;
                    Error[nError].Node = Network[currentN].Node[i].Name;
                    Error[nError].currentKind = Network[currentN].Node[i].Kind;
                    if (bFor)
                        Error[nError].messageNum = 34;
                    else
                        Error[nError].messageNum = 35;
                    add_Error();
                }
        }
        public bool check_CIPd(int workLoop, int CIPd, int fblockIndex)
        {
            for (int i = 0; i < FBLOCK.FBlock[fblockIndex].nNode; i++ )
            {
                if (CIPd == FBLOCK.FBlock[fblockIndex].Node[i])
                    return true;
            }
                return false;
        }

        public void check_SearchNode_Same(ref int[] searchNode,ref int nSearchNode)
        {
            bool check = false;
            do
            {
                check = false;
                for (int i = 0; i < nSearchNode -1 ; i++)
                {
                    for (int j = i + 1; j < nSearchNode; j++)
                    {
                        if (searchNode[i] == searchNode[j])
                        {
                            //remove searchnode[j];
                            for (int k = j + 1; k < nSearchNode; k++)
                            {
                                searchNode[k - 1] = searchNode[k];
                            
                            }
                            nSearchNode--;
                            check = true;
                            break;
                        }
                    }
                    if (check) break;
                }
            } while (check);
        }

        public void find_Node_iDFlow(int workLoop, int current_Loop, int fromN, int toN, int current_ConEnSet, ref int iCID, int CIPd, string flow)
        {
            //make_LoopNetwork(workLoop, current_Loop, fromN, toN, current_ConEnSet);
            reverse_LoopNetwork(fromN, toN, workLoop, current_Loop, current_ConEnSet); // reverse "fromN" and store in "toN" network ====

            int[] calDomRev = null;
            for (int k = 0; k < Loop[workLoop].Loop[current_Loop].nEntry; k++)
            {
                if (Loop[workLoop].Loop[current_Loop].Concurrency[k] == current_ConEnSet)
                {
                    calDomRev = find_Intersection(Network[toN].nNode, calDomRev, Network[toN].Node[Loop[workLoop].Loop[current_Loop].Entry[k]].DomRev);
                }
            }

            //Now we have iCID node => CalDomRev[0];
            Network[tempNet] = Network[toN];
            extent_Network(tempNet, 0);
            searchNode = new int[Network[fromN].nNode]; //just test
            nSearchNode = 0; //just test

            if (calDomRev.Length > 0)
            {
                int header = calDomRev[0];
                iCID = header;
                searchNode[nSearchNode] = header;
                nSearchNode++;
                int[,] A = null;
                bool[] mark = new bool[Network[fromN].nNode];
                int[] X = new int[Network[fromN].nNode];
                int nNode = 0;
                prepare_find_Path(fromN, workLoop, current_Loop, ref mark, false);

                transfer_AdjacencyMatrix(fromN, ref A, ref nNode);
                if (flow == "iCID_CIPd")
                {
                    find_NodeFlow(iCID, CIPd, A, nNode, ref mark, workLoop, current_Loop); //=> Store in searchNode;
                }
                else if (flow == "CIPd_iCID")
                {
                    find_NodeFlow(CIPd, iCID, A, nNode, ref mark, workLoop, current_Loop);
                }

                //transfer to searchNode[] for input of re-create Network[subNet]
                searchNode = new int[Loop[workLoop].Loop[current_Loop].nNode + 1];
                nSearchNode = 0;
                for (int i = 0; i < Network[fromN].nNode; i++)
                    if (mark[i] == true)
                    {
                        searchNode[nSearchNode] = i;
                        nSearchNode++;
                    }
            }
            

            //Add Node
            Network[toN] = new strNetwork();
            Network[toN].nNode = nSearchNode;
            Network[toN].Node = new strNode[Network[toN].nNode];
            for (int i = 0; i < nSearchNode; i++)
            {
                Network[toN].Node[i] = Network[fromN].Node[searchNode[i]]; //get the data from fromN (original network, not reversed network)
                Network[toN].Node[i].orgNum = searchNode[i];
            }

            //Add Link
            if (flow == "iCID_CIPd") //
                make_subLink_2(fromN, toN, workLoop, current_Loop, "CC", current_ConEnSet, iCID, CIPd); //sublink "fromN" to "toN" (here is network[8] to network[6]
            if (flow == "CIPd_iCID")
                make_subLink_2(fromN, toN, workLoop, current_Loop, "CC", current_ConEnSet, CIPd, iCID);
            //===========
            for (int i = 0; i < Network[toN].nNode; i++)
            {
                find_NodeInform(toN, i);
            }
            find_Dom(toN);
            find_DomEI(toN, -1);
            find_DomRev(toN);
            find_DomRevEI(toN);
        }

        public void transfer_AdjacencyMatrix(int currentN, ref int[,] A, ref int nNode)        {
            nNode = Network[currentN].nNode;
            //initiate adjacency matrix
            A = new int[nNode, nNode]; 
            for (int i = 0; i < nNode; i++)
                for (int j = 0; j < nNode; j++)
                    A[i,j] = -1;

            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                A[Network[currentN].Link[i].fromNode, Network[currentN].Link[i].toNode] = 1;
            }
        }

        public void find_NodeFlow(int fromNode, int toNode, int[,] A, int nNode, ref bool[] Mark, int workLoop, int current_Loop)
        {
            Mark[fromNode] = true;
            if (fromNode != toNode)
            {
                for (int v = 0; v < nNode; v++)
                {
                    if (Node_In_Loop(workLoop, v, current_Loop) && A[fromNode, v] != -1 && Mark[v] == false) find_NodeFlow(v, toNode, A, nNode, ref Mark, workLoop, current_Loop); //we need to prevent searching outside the loop
                    
                }
            }
        }

        /*public int[] transferLoop(int fromN, int toN, int workLoop, int current_Loop, ref int[] transferConEn, int current_ConEnSet)
        {
            int[] temp = new int[Loop[workLoop].Loop[current_Loop].nEntry];
            int ntemp = 0;
            for (int k = 0; k < Loop[workLoop].Loop[current_Loop].nEntry; k++)
            {
                for (int j = 0; j < Network[toN].nNode; j++)
                {
                    if (Network[fromN].Node[Loop[workLoop].Loop[current_Loop].Entry[k]].Name == Network[toN].Node[j].parentNum.ToString())
                    {
                        temp[ntemp] = j;
                        
                        if (Loop[workLoop].Loop[current_Loop].conEntry[current_ConEnSet, k] > 0)
                        {
                            transferConEn[ntemp] = 1;
                        }
                        ntemp++;
                        break;
                    }
                }

            }
            return temp;
        }*/

        /*public void make_LoopNetwork(int workLoop, int current_Loop, int fromN, int toN, int current_ConEnSet) //tempNet
        {

            Network[toN].nNode = Loop[workLoop].Loop[current_Loop].nNode + 1;
            Network[toN].Node = new strNode[Network[toN].nNode];
            nSearchNode = 0;
            searchNode = new int[Network[fromN].nNode];

            Network[toN].Node[0] = Network[fromN].Node[Loop[workLoop].Loop[current_Loop].header]; //add loop header in to "toN network"
            searchNode[nSearchNode] = Loop[workLoop].Loop[current_Loop].header;
            nSearchNode++;
            for (int i = 0; i < Loop[workLoop].Loop[current_Loop].nNode; i++)
            {
                int node = Loop[workLoop].Loop[current_Loop].Node[i];
                Network[toN].Node[i+1] = Network[fromN].Node[node];
                searchNode[nSearchNode] = node;
                nSearchNode++;
            }
            //add 1 more node (header of loop) into toN and searchNode[]

            make_subLink_2(fromN, toN, workLoop, current_Loop, "CC");
            for (int i = 0; i < Network[toN].nNode; i++)
            {
                find_NodeInform(toN, i);
            }
            find_Dom(toN);
        }*/
        public void reverse_LoopNetwork(int fromN, int toN, int workLoop, int current_Loop, int current_ConEnSet)
        {
            Network[toN] = Network[fromN];
            extent_Network(toN, 0);
            //Network[tempNet] = Network[fromN];
            for (int linkIndex = 0; linkIndex < Network[toN].nLink; linkIndex++ )
            {
                if (Node_In_Loop(workLoop, Network[toN].Link[linkIndex].fromNode, current_Loop) && Node_In_Loop(workLoop, Network[toN].Link[linkIndex].toNode, current_Loop) )
                {
                    int temp = 0;
                    temp = Network[toN].Link[linkIndex].fromNode;
                    Network[toN].Link[linkIndex].fromNode = Network[toN].Link[linkIndex].toNode;
                    Network[toN].Link[linkIndex].toNode = temp;
                }
            }

            for (int i = 0; i < Network[toN].nNode; i++)
            {
                find_NodeInform(toN, i);
            }
            find_Dom(toN);
            find_DomEI(toN, -1);
            find_DomRev(toN);
            find_DomRevEI(toN);
            //Network[fromN] = Network[toN];
        }

        public void find_Node_PdFlow(int fromN, int workLoop, int loop, string Type, int entryH, int current_ConEnSet, ref int CIPd)
        {
            //nSearchNode = 0;
            //searchNode = new int[Network[fromN].nNode];

            int[] calDomRev = null;
            for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
            {
                if (Loop[workLoop].Loop[loop].Concurrency[k] == current_ConEnSet)
                {
                    calDomRev = find_Intersection(Network[fromN].nNode, calDomRev, Network[fromN].Node[Loop[workLoop].Loop[loop].Entry[k]].DomRev);
                }
            }

            if (calDomRev.Length > 0)
            {
                int header = calDomRev[0];
                CIPd = header;
                searchNode[nSearchNode] = header;
                nSearchNode++;

                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                {
                    if (Loop[workLoop].Loop[loop].Concurrency[k] == current_ConEnSet)
                    {
                        //find_Reach_2(fromN, workLoop, loop, header, Loop[workLoop].Loop[loop].Entry[k], "CC"); //find reach from HEADER to ENTRY[k]

                        //searchNode[nSearchNode] = Loop[workLoop].Loop[loop].Entry[k];
                        //nSearchNode++;

                        //mark[i] fill = false; x[i]; searchNode; nSearchNode;
                        bool[] mark = new bool[Network[fromN].nNode];
                        int[] X = new int[Network[fromN].nNode];
                        //searchNode = new int[Loop[workLoop].Loop[loop].nNode];
                        //nSearchNode = 0;
                        prepare_find_Path(fromN, workLoop, loop, ref mark, false);
                        if (header != Loop[workLoop].Loop[loop].Entry[k])
                        {
                            find_Path(fromN, workLoop, loop, header, Loop[workLoop].Loop[loop].Entry[k], ref mark, ref X, header, ref searchNode, ref nSearchNode);
                        }
                        //store value
                    }
                }
            }
        } //co van de roi! => co cai dek j dau

        public void prepare_find_Path(int fromN, int workLoop, int loop, ref bool[] mark, bool type)
        {
            for (int i = 0; i < Network[fromN].nNode; i++)
            {
                mark[i] = type;
            }
        }
        public void find_Path(int fromN, int workLoop, int loop, int fromNode, int toNode, ref bool[] mark, ref int[] X, int header, ref int[] searchNode, ref int nSearchNode)
        {
            for (int j = 0; j < Network[fromN].Node[fromNode].nPre; j++)
            {
                int preNode = Network[fromN].Node[fromNode].Pre[j];
                if (mark[preNode] == false && Node_In_Loop(workLoop, preNode, loop))
                {
                    X[fromNode] = preNode;
                    mark[preNode] = true;
                    if (preNode == toNode) 
                    { 
                        getResult(ref X, header, toNode, ref searchNode, ref nSearchNode);
                        mark[preNode] = false;
                    }
                    else
                    {
                        mark[preNode] = true;
                        find_Path(fromN, workLoop, loop, preNode, toNode, ref mark, ref X, header, ref searchNode, ref nSearchNode);
                        mark[preNode] = false;
                    }
                }
            }
        }
        public void getResult(ref int[] X, int header, int toNode, ref int[] searchNode, ref int nSearchNode)
        {
            int track = header;
            /*if (nSearchNode == 0)
            {
                searchNode[nSearchNode] = header;
                nSearchNode++;
            }*/
            bool bFor = true;
            do
            {

                for (int i = 0; i < nSearchNode; i++)
                {
                    if (searchNode[i] == X[track]) //sai day => check all set
                    {
                        bFor = false;
                        break;
                    }
                }
                if (bFor)
                {
                    searchNode[nSearchNode] = X[track];
                    nSearchNode++;
                }
                track = X[track];
            } while (track != toNode);
        }

        private void make_subNode_2(int fromN, int toN, bool sDummy, string sType, bool eDummy, string eType) //fromN ~ From Netwrok; toN ~ To Network
        {
            //Sub Network 구성
            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum = addNum + 2; // for VS VE

            Network[toN] = new strNetwork();
            Network[toN].nNode = nSearchNode + addNum;
            Network[toN].Node = new strNode[Network[toN].nNode];
            for (int i = 0; i < nSearchNode; i++)
            {
                Network[toN].Node[i] = Network[fromN].Node[searchNode[i]];
                Network[toN].Node[i].orgNum = searchNode[i];
            }

            // Add details of new node START
            if (sDummy)
            {
                Network[toN].Node[Network[toN].nNode - addNum].Kind = sType;
                Network[toN].Node[Network[toN].nNode - addNum].Name = "DM";
                Network[toN].Node[Network[toN].nNode - addNum].parentNum = -1;
                Network[toN].Node[Network[toN].nNode - addNum].orgNum = -1;// "D";
                Network[toN].Node[Network[toN].nNode - addNum].Special = "";

                addNum--;
            }
            //Add type of the new node END
            if (eDummy)
            {
                Network[toN].Node[Network[toN].nNode - addNum].Kind = eType;
                Network[toN].Node[Network[toN].nNode - addNum].Name = "DM";
                Network[toN].Node[Network[toN].nNode - addNum].parentNum = -1;
                Network[toN].Node[Network[toN].nNode - addNum].orgNum = -1;// "D";
                Network[toN].Node[Network[toN].nNode - addNum].Special = "";

                addNum--;
            }
            //VS
            Network[toN].Node[Network[toN].nNode - addNum].Kind = "OR";
            Network[toN].Node[Network[toN].nNode - addNum].Name = "VS";
            Network[toN].Node[Network[toN].nNode - addNum].parentNum = -1;
            Network[toN].Node[Network[toN].nNode - addNum].orgNum = -1;// "D";
            Network[toN].Node[Network[toN].nNode - addNum].Special = "";
            addNum--;
            //VE
            Network[toN].Node[Network[toN].nNode - addNum].Kind = "OR";
            Network[toN].Node[Network[toN].nNode - addNum].Name = "VE";
            Network[toN].Node[Network[toN].nNode - addNum].parentNum = -1;
            Network[toN].Node[Network[toN].nNode - addNum].orgNum = -1;// "D";
            Network[toN].Node[Network[toN].nNode - addNum].Special = "";
            addNum--;

            //Network[toN].header = Network[toN].nNode - 2;
        }
        private void make_subLink_2(int fromN, int toN, int workLoop, int loop, string Type, int entryH, int fromNode, int toNode)
        {
            strLink[] imLink = new strLink[Network[fromN].nLink + 2];
            int imNum = 0;
            for (int i = 0; i < Network[fromN].nLink; i++)
            {
                //if (!check_addLink(fromN, workLoop, loop, i, Type)) continue;

                int nFrom = -1;
                int nTo = -1;

                for (int k = 0; k < nSearchNode; k++)
                {
                    if (Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (Network[fromN].Link[i].toNode == searchNode[k]) nTo = k;
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    if (toNode != -1)
                    {
                        if (searchNode[nFrom] == toNode && searchNode[nTo] == fromNode) continue; //ex: CIPd -> iCID => there are 1 case which iCID -> CIPd have 1 edge
                    }
                    imLink[imNum] = Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
            }
            if (Type == "IR")
            {
                int addNum = 4;
                //link for VS to a CE set
                for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++)
                {
                    if (Loop[workLoop].Loop[loop].Concurrency[i] != entryH) continue;
                    imLink[imNum].fromNode = Network[toN].nNode - 2; //Network[subNet].Node[nNode -2] => VS
                    imLink[imNum].toNode = get_Index(toN, Loop[workLoop].Loop[loop].Entry[i]);
                    imNum++;
                }

                addNum--;
                //link for Exits to VE
                for (int i = 0; i < Loop[workLoop].Loop[loop].nExit; i++)
                {
                    imLink[imNum].toNode = Network[toN].nNode - 1;
                    imLink[imNum].fromNode = get_Index(toN,Loop[workLoop].Loop[loop].Exit[i]);
                    imNum++;
                }
                //link for START to VS
                imLink[imNum].fromNode = Network[toN].nNode - 4; //Network[subNet].Node[nNode -2] => VS
                imLink[imNum].toNode = Network[toN].nNode - 2;
                imNum++;
                //link for VE to END
                imLink[imNum].fromNode = Network[toN].nNode - 1; //Network[subNet].Node[nNode -2] => VS
                imLink[imNum].toNode = Network[toN].nNode - 3;
                imNum++;

            }

            Network[toN].nLink = imNum;
            Network[toN].Link = new strLink[Network[toN].nLink];
            for (int i = 0; i < Network[toN].nLink; i++)
            {
                Network[toN].Link[i] = imLink[i];
            }
        }

        public int get_Index(int currentN, int old_index)
        {
            for (int i = 0; i < Network[currentN].nNode; i++ )
            {
                if (old_index == Network[currentN].Node[i].orgNum)
                    return i;
            }
            return -1;
        }
        private void find_Node_DFlow(int fromN, int workLoop, int loop, string Type, int entryH, int current_ConEnSet, ref int CID, ref bool bFor) //SearchNode[] will be cleared when use this function.
        {
            nSearchNode = 0;
            searchNode = new int[Network[fromN].nNode];
            
            int[] calDom = null;
            for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
            {
                if (Loop[workLoop].Loop[loop].Concurrency[k] == current_ConEnSet)
                {
                    calDom = find_Intersection(Network[fromN].nNode, calDom, Network[fromN].Node[Loop[workLoop].Loop[loop].Entry[k]].Dom);
                }
            }

            if (calDom.Length > 0)
            {
                int header = calDom[calDom.Length - 1];
                CID = header;
                searchNode[nSearchNode] = header;
                nSearchNode++;

                /*for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                {
                    if (Loop[workLoop].Loop[loop].Concurrency[k] == current_ConEnSet)
                    {
                        find_Reach_2(fromN, workLoop, loop, header, Loop[workLoop].Loop[loop].Entry[k], "CC"); //find reach from HEADER to ENTRY[k]

                        searchNode[nSearchNode] = Loop[workLoop].Loop[loop].Entry[k];
                        nSearchNode++;
                        //find_NodeFlow(iCID, CIPd, A, nNode, ref mark, workLoop, current_Loop);
                    }
                }*/
                //============================

                //int[,] A = null;
                bool[] mark = new bool[Network[fromN].nNode];
                //int[] X = new int[Network[fromN].nNode];
                int nNode = 0;
                prepare_find_Path(fromN, workLoop, loop, ref mark, false);

                //transfer_AdjacencyMatrix(fromN, ref A, ref nNode);
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                {
                    if (Loop[workLoop].Loop[loop].Concurrency[k] == current_ConEnSet)
                    {
                        mark = new bool[Network[fromN].nNode];
                        find_NodeFlow_CID_Entries(fromN, header, Loop[workLoop].Loop[loop].Entry[k], nNode, ref mark, workLoop, loop); //=> Store in searchNode;
                        
                        //transfer to searchNode[] for input of re-create Network[subNet
                        
                        check_SearchNode_Same(ref searchNode, ref nSearchNode); //because it will duplicate from CID to each entries
                    }
                }

            }
        }

        public void get_searchNode_value(int fromN, ref int[] searchNode, ref int nSearchNode, bool[] mark)
        {
            for (int i = 0; i < Network[fromN].nNode; i++)
            {
                if (mark[i] == true)
                {
                    searchNode[nSearchNode] = i;
                    nSearchNode++;
                }
            }
        }

        public void find_NodeFlow_CID_Entries(int fromN, int fromNode, int toNode, int nNode, ref bool[] Mark, int workLoop, int current_Loop)
        {
            Mark[fromNode] = true;
            if (fromNode != toNode)
            {
                /*for (int v = 0; v < nNode; v++)
                {
                    if (A[fromNode, v] != -1 && Mark[v] == false) 
                        find_NodeFlow_CID_Entries(fromN, v, toNode, A, nNode, ref Mark, workLoop, current_Loop); //we need to prevent searching outside the loop
                }*/
                for (int v = 0; v < Network[fromN].Node[fromNode].nPost; v++)
                {
                    if ((!Node_In_Loop(workLoop, Network[fromN].Node[fromNode].Post[v], current_Loop) || Network[fromN].Node[fromNode].Post[v] == toNode)  
                                    && Mark[Network[fromN].Node[fromNode].Post[v]] == false)
                        find_NodeFlow_CID_Entries(fromN, Network[fromN].Node[fromNode].Post[v], toNode, nNode, ref Mark, workLoop, current_Loop);
                    
                }
            }
            else
            {
                //get value to searchNode
                get_searchNode_value(fromN, ref searchNode, ref nSearchNode, Mark);
                check_SearchNode_Same(ref searchNode, ref nSearchNode); //because it will duplicate from CID to each entries
            }
            Mark[fromNode] = false;
        }
        public bool is_Entry(int workLoop, int loop, int node)
        {
            for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++ )
            {
                if (node == Loop[workLoop].Loop[loop].Entry[i])
                {
                    return true;
                }
            }
            return false;
        }

        #region Find_ConcurrencyEntrySet
        public void find_ConcurrencyEntrySet(int subNet, int tempLoop, int current_loop) //New function
        {
            //prepare_Find_CC();
            //try(j)
            //Network[subNet].Node[0].conEntry = try 
            Loop[tempLoop].Loop[current_loop].conEntry = try_CE(subNet, tempLoop, current_loop, 0);

            //transfer from Node[0] (CID of all Entries) to Loop[current_loop].conEntry
            Loop[tempLoop].Loop[current_loop].conEntry = Network[subNet].Node[0].conEntry; //move the concurrent entries which is stored in CID to the Loop
            Loop[tempLoop].Loop[current_loop].nConEntry = Network[subNet].Node[0].nConEntry;

            
        }

        //fill the initiate value of each entries. 
        public void prepare_Find_CC(int currentN, int workLoop, int current_Loop)
        {
            //Network[].Node[].conEntry[,] work with the index of the loop entries Loop[].Entry[] (workloop ~ 3 (tempLoop))
            for (int i = 0; i < Network[currentN].nNode;i++)
            {
                for (int j = 0; j < Loop[workLoop].Loop[current_Loop].nEntry; j++)
                {
                    if (Network[currentN].Node[i].Name == Loop[workLoop].Loop[current_Loop].Entry[j].ToString())
                    {
                        int index = Loop[workLoop].Loop[current_Loop].nEntry;
                        Network[currentN].Node[i].conEntry = new int[1, index];
                        Network[currentN].Node[i].conEntry[0, j] = 1;
                        Network[currentN].Node[i].nConEntry = 1;
                    }
                }
            }
        } //prepare stage for find_ConcurrencyEntrySet
        public int[,] try_CE(int subNet, int tempLoop, int current_Loop, int node_j) //New function
        {
            //anchor of Recursive algorithm
            if (isEntry(subNet, tempLoop, current_Loop, node_j))
            {
                return Network[subNet].Node[node_j].conEntry;
            }
            else if (Network[subNet].Node[node_j].Kind == "AND")
            {
                for (int i = 0; i < Network[subNet].Node[node_j].nPost; i++)
                {
                    int child = Network[subNet].Node[node_j].Post[i];
                    Network[subNet].Node[child].conEntry = try_CE(subNet, tempLoop, current_Loop, child);
                }
                addConEn_AND(subNet, tempLoop, current_Loop, node_j, Network[subNet].Node[node_j].Post, Network[subNet].Node[node_j].nPost);
            }
            else if (Network[subNet].Node[node_j].Kind == "OR")
            {
                //============== FIRST Find all XOR case of this OR split's child ===============
                for (int i = 0; i < Network[subNet].Node[node_j].nPost; i++)
                {
                    int child = Network[subNet].Node[node_j].Post[i];
                    Network[subNet].Node[child].conEntry = try_CE(subNet, tempLoop, current_Loop, child);
                }
                //addConEn_XOR(subNet, tempLoop, current_Loop, node_j, Network[subNet].Node[node_j].Post, Network[subNet].Node[node_j].nPost);
                //================================================================================

                //============== SECOND Combine all the instance case of this OR split with the result of XOR=====
                int totalEntry = Loop[tempLoop].Loop[current_Loop].nEntry;
                int totalInstance = (int)Math.Pow(2, totalEntry + 1) - 1; //not totalEntry => just current Post of OR split //
                                                                      //we must to decide how much the maximum TemConEntry_OR case will have => to prevent further error => we increase (1 entries)
                int[,] TempConEntry_OR = new int[totalInstance, totalEntry]; //Store total instance cases of this OR split
                int nTemConEntry_OR = 0; //number of instance case

                int nPost = Network[subNet].Node[node_j].nPost;
                int[,] instance_OR_Case = new int[(int)Math.Pow(2, nPost) - 1, nPost];

                TempConEntry_OR = Generate_OR_Cases(Network[subNet].Node[node_j].nPost, Network[subNet].Node[node_j].Post, ref nTemConEntry_OR);
                int[,] transferConEn = new int[(int)Math.Pow(2, totalEntry) - 1, totalEntry];
                int nTransferConEn = 0;

                for (int i = 0; i < nTemConEntry_OR; i++) //duyet qua tat ca cac instance cua OR split hien tai
                {
                    int[] Post_node_j = new int[totalEntry];
                    int nPost_node_j = 0;
                    Post_node_j = get_a_Set(i, TempConEntry_OR, subNet, node_j, ref nPost_node_j, nPost); //Lay 1 dãy instance ra => dong thoi tra lai so luong instance
                    
                    if (nPost_node_j == 1) //neu so luong = 1 => XOR add
                    {
                        addConEn_XOR(subNet, tempLoop, current_Loop, node_j, Post_node_j, nPost_node_j);
                        temporary_Save(ref transferConEn, ref nTransferConEn, Network[subNet].Node[node_j].nConEntry, Network[subNet].Node[node_j].conEntry, totalEntry);
                        remove_Same_ConEntry(subNet, ref transferConEn, ref nTransferConEn, totalEntry);
                    }
                    else
                    {
                        addConEn_AND(subNet, tempLoop, current_Loop, node_j, Post_node_j, nPost_node_j);
                        temporary_Save(ref transferConEn, ref nTransferConEn, Network[subNet].Node[node_j].nConEntry, Network[subNet].Node[node_j].conEntry, totalEntry);
                        remove_Same_ConEntry(subNet, ref transferConEn, ref nTransferConEn, totalEntry);
                    }
                }
                //==================================================================================================
                Network[subNet].Node[node_j].conEntry = new int[nTransferConEn, totalEntry];
                //transfer ==
                for (int i = 0; i < nTransferConEn; i++)
                {
                    for (int j = 0; j < totalEntry; j++)
                    {
                        Network[subNet].Node[node_j].conEntry[i,j] = transferConEn[i,j];
                    }
                }
                Network[subNet].Node[node_j].nConEntry = nTransferConEn;
                //============
            }
            else
            {
                for (int i = 0; i < Network[subNet].Node[node_j].nPost; i++)
                {
                    int child = Network[subNet].Node[node_j].Post[i];
                    Network[subNet].Node[child].conEntry = try_CE(subNet, tempLoop, current_Loop, child);
                }
                addConEn_XOR(subNet, tempLoop, current_Loop, node_j, Network[subNet].Node[node_j].Post, Network[subNet].Node[node_j].nPost); //the other gateways which are differed from OR , AND will be acted like XOR gateway
            }
            return Network[subNet].Node[node_j].conEntry; 
        } //VERY IMPORTANT FUNCTION => IT FIND ALL POSSIBLE CASE OF IRLOOP ENTRIES TRIGGED

        //cummulative the set of entries of OR split => return to "transferConEn[,]"
        public void temporary_Save(ref int[,] transferConEn, ref int nTransferConEn, int nConEntry, int[,] conEntry, int totalEntry)
        {
            for (int i = 0; i < nConEntry; i++)
            {
                for (int j = 0; j < totalEntry; j++)
                {
                    transferConEn[nTransferConEn, j] = conEntry[i, j];
                }
                nTransferConEn++;
            }
        }

        //from CC matrix TemConENtry[,] => we get an array which have a single set of concurrent Entry => which is indicaded the index of node_j.Post[];
        public int[] get_a_Set(int i, int[,] TempConEntry, int subNet, int node_j, ref int nPost_node_j, int nPost)
        {
            int[] temp = new int[nPost];
            for (int k = 0; k < nPost; k++)
            {
                temp[k] = -1;
            }
            int count = 0;
            for (int j = 0; j < nPost; j++)
            {
                if (TempConEntry[i,j] == 1)
                {
                    temp[count] = Network[subNet].Node[node_j].Post[j]; //might be true right!
                    //temp[count] = j; //right!
                    count++;
                }
            }
            nPost_node_j = count;
            return temp;
        }
        public int[,] Generate_OR_Cases(int nPost, int[] Post, ref int totalIns)
        {
            int totalInsCase = (int)Math.Pow(2, nPost) - 1;
            int[,] tempInstance = new int[totalInsCase, nPost];
                        
            int insCurCase = 0;

            for (int token = 1; token <= nPost; token++ )
            {
                //int sIndex = insCurCase;
                if (token == 1)
                {
                    for (int i = 0; i < nPost; i++)
                    {
                        tempInstance[insCurCase, i] = 1;
                        insCurCase++;
                    }
                }
                else
                {
                    int[] x = new int[nPost + 1];
                    x[0] = 0;
                    combination(1, nPost, token, ref x, ref insCurCase, ref tempInstance);
                }
            }
            totalIns = totalInsCase;
            return tempInstance;
        }

        public void combination(int i, int n, int k, ref int[] x, ref int currInstance, ref int[,] tempInstance)
        {
            for (int j = x[i - 1] + 1; j <= n - k + i; j++)
            {
                x[i] = j;
                if (i == k)
                {
                    store_X(x, k, ref currInstance, ref tempInstance, n); //store 1 instance to tempInstance => increase currInstance (1 unit)
                }
                else
                {
                    combination(i + 1, n, k, ref x, ref currInstance, ref tempInstance);
                }
            }
        } //Generate OR possible case of an OR Split

        //add a new instance from OR to "tempInstance[,] array
        public void store_X(int[] x, int k, ref int currInstance, ref int[,] tempInstance, int nPost)
        {
            for (int i = 1; i <= k; i++)
            {

                tempInstance[currInstance, x[i] - 1] = 1;

            }
            currInstance++;
        } //using for combination()

        public void addConEn_AND(int subNet, int tempLoop, int current_Loop, int node_j, int[] Post_node_j, int nPost_node_j)
        {
            //declare variable;
            int nChild = nPost_node_j;
            int current_LoopEntry = Loop[tempLoop].Loop[current_Loop].nEntry;
            int total_InsCase = 1;
            for (int i = 0; i < nChild; i++)
            {
                int child = Network[subNet].Node[node_j].Post[i];
                total_InsCase = total_InsCase * Network[subNet].Node[child].nConEntry; //we have to multiply all number of entries
            }

            Network[subNet].Node[node_j].conEntry = new int[total_InsCase, current_LoopEntry];
            int[,] DummyConEntry = new int[total_InsCase, current_LoopEntry];
            int currentInstance_XOR = 0; //nDummyConEntry;
            
            for (int eachChild = 0; eachChild < nChild; eachChild++) //visit all child of current AND
            {
                int child = Post_node_j[eachChild]; //we will use this variable for further calculation
                //int[,] tempConEntry = new int[total_InsCase, current_LoopEntry]; 
                if (eachChild == 0)
                {   //fill the inititate state of conEntry of "node_j" (fill the first conEntry[,] from the first child which is visited
                    for (int subIntance = 0; subIntance < Network[subNet].Node[child].nConEntry; subIntance++) //visit all instance of current child of XOR
                    {
                        for (int i = 0; i < current_LoopEntry; i++)
                        {
                            DummyConEntry[currentInstance_XOR, i] = Network[subNet].Node[child].conEntry[subIntance, i];
                        }
                        currentInstance_XOR++; //problem here!! => maybe right!??
                    }
                }
                else //multiply 2 matrix to get the instance result.
                {
                    multiply_EachChild(subNet, ref DummyConEntry, node_j, child, current_LoopEntry, ref currentInstance_XOR);
                }
            }
            remove_Same_ConEntry(subNet, ref DummyConEntry, ref currentInstance_XOR, current_LoopEntry);

            Network[subNet].Node[node_j].nConEntry = currentInstance_XOR; //get the number of CE
            Network[subNet].Node[node_j].conEntry = DummyConEntry;
        } //deadling with AND Split

        //==search the DummyConEntry => If have same rows => remove until have just 1
        public void remove_Same_ConEntry(int subNet, ref int[,] DummyConEntry, ref int currentInstance, int current_LoopEntry)
        {
            bool[] mark = new bool[currentInstance];
            int nMark = 0;
            for (int i = 0; i < currentInstance; i++ )
            {
                mark[i] = false;
            }
            for (int i = 0; i < currentInstance - 1; i++)
            {
                for (int j = i + 1; j < currentInstance; j++)
                {
                    if (check_Same_ConEntry(DummyConEntry, i, j, current_LoopEntry))
                    {
                        mark[j] = true;
                        nMark++;
                    }
                }
            }
            remove_Same(ref DummyConEntry, mark, current_LoopEntry, ref currentInstance, nMark);
        } //because my method could introduce some duplicate entries SETs

        public bool check_Same_ConEntry(int[,] DummyConEntry, int i, int j, int current_LoopEntry)
        {
            bool isSame = true;
            for (int k = 0; k < current_LoopEntry; k++)
            {
                if (DummyConEntry[i, k] != DummyConEntry[j, k])
                {
                    if (DummyConEntry[i, k] > 0 && DummyConEntry[j, k] > 0) continue;
                    isSame = false;
                    break;
                }
            }
            return isSame;
        } //it works fine! => USING FOR remove_Same_Conentry()

        public void remove_Same(ref int[,] DummyConEntry, bool[] mark, int current_LoopEntry, ref int currentInstance, int nMark)
        {
            int[,] TempDummyConEn = new int[currentInstance, current_LoopEntry];
            TempDummyConEn = DummyConEntry;
            if (nMark > 0)
            {
                DummyConEntry = new int[(int)Math.Pow(2, current_LoopEntry + 1) -1, current_LoopEntry]; //the index must be equal to the total case can happen of all entries
                int count = 0;
                for (int i = 0; i < currentInstance; i++)
                {
                    if (mark[i] == false)
                    {
                        for (int j = 0; j < current_LoopEntry; j++)
                        {
                            DummyConEntry[count, j] = TempDummyConEn[i, j];
                        }
                        count++;
                    }
                }
                currentInstance = count;
            }
        } //USING for remove_Same_ConENtry()
        public void multiply_EachChild(int subNet, ref int[,] DummyConEntry, int node_j, int child, int current_LoopEntry, ref int currentInstance_XOR)
        {
            int currInstance = 0;
            int[,] tempDummyConEn = new int [currentInstance_XOR, current_LoopEntry];
            tempDummyConEn = DummyConEntry;
            tempDummyConEn = Network[subNet].Node[node_j].conEntry;
            for (int i = 0; i < currentInstance_XOR; i++ )
            {
                for (int j = 0; j < Network[subNet].Node[child].nConEntry; j++)
                {
                    for (int k = 0; k < current_LoopEntry; k++)
                    {
                        tempDummyConEn[currInstance, k] = DummyConEntry[i, k] + Network[subNet].Node[child].conEntry[j, k];
                        if (DummyConEntry[currInstance, k] > 1) //prevent duplicate entries 
                            DummyConEntry[currInstance, k] = 1;
                    }
                    currInstance++;
                }
            }
            DummyConEntry = tempDummyConEn;
            currentInstance_XOR = currInstance;
            //return tempConEntry;
        } //USING FOR AddConEN_AND()

        public void addConEn_XOR(int subNet, int tempLoop, int current_Loop, int node_j, int[] Post_node_j, int nPost_node_j)
        {
            //declare variable;
            int nChild = nPost_node_j;
            int current_LoopEntry = Loop[tempLoop].Loop[current_Loop].nEntry;
            int total_InsCase = 0;
            for (int i = 0; i < nChild; i++)
            {
                int child = Network[subNet].Node[node_j].Post[i];
                total_InsCase = total_InsCase + Network[subNet].Node[child].nConEntry;
            }

            //main transfer;
            Network[subNet].Node[node_j].conEntry = new int[total_InsCase, current_LoopEntry]; //because the instance case of XOR equal to the number of total instance case of children.
            int[,] DummyConEntry = new int[total_InsCase, current_LoopEntry];

            int currentInstance_XOR = 0;
            for (int eachChild = 0; eachChild < nChild; eachChild++) //visit all child of current XOR
            {
                int child = Post_node_j[eachChild];
                for (int subIntance = 0; subIntance < Network[subNet].Node[child].nConEntry; subIntance++) //visit all instance of current child of XOR
                {
                    for (int i = 0; i < current_LoopEntry; i++)
                    {
                        DummyConEntry[currentInstance_XOR, i] = Network[subNet].Node[child].conEntry[subIntance, i];
                    }
                    currentInstance_XOR++; //increase the index of current XOR instance (increase the number of instance of this XOR)
                }
            }

            remove_Same_ConEntry(subNet, ref DummyConEntry, ref currentInstance_XOR, current_LoopEntry); //remove same concurrent Entry Set

            Network[subNet].Node[node_j].nConEntry = currentInstance_XOR; //get the number of CE
            Network[subNet].Node[node_j].conEntry = DummyConEntry;
        }

        public bool isEntry(int subNet, int tempLoop, int current_Loop, int node_j)
        {
            for (int k = 0; k < Loop[tempLoop].Loop[current_Loop].nEntry; k++)
            {
                if (Network[subNet].Node[node_j].Name == Loop[tempLoop].Loop[current_Loop].Entry[k].ToString())
                {
                    return true;
                }
            }
            return false;
        }

        #endregion End of find_ConEntry_SET procedures.

        private void check_by_Rule6(int currentN, int workLoop, int loop, string strLoop)
        {

            for (int i = 0; i < Network[currentN].nLink; i++)
            {
                int nFrom = -1;
                int nTo = -1;

                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    //if (Loop[workLoop].Loop[loop].Concurrency[k] <= 0) continue;

                    if (Network[currentN].Link[i].fromNode == Loop[workLoop].Loop[loop].Entry[k]) nFrom = k;// Loop[workLoop].Loop[loop].Entry[k];
                    if (Network[currentN].Link[i].toNode == Loop[workLoop].Loop[loop].Entry[k]) nTo = k;// Loop[workLoop].Loop[loop].Entry[k];
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    //Error면
                    if (Loop[workLoop].Loop[loop].Concurrency[nFrom] >= 1 || Loop[workLoop].Loop[loop].Concurrency[nTo] >= 1)
                    {

                        nFrom = Loop[workLoop].Loop[loop].Entry[nFrom];
                        nTo = Loop[workLoop].Loop[loop].Entry[nTo];

                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[nTo].parentNum.ToString();
                        //Error[nError].Node = Network[currentN].Node[nFrom].parentNum.ToString() + "->" + Network[currentN].Node[nTo].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[nFrom].parentNum.ToString() + "(" + Network[currentN].Node[nFrom].Kind + ") -> " + Network[currentN].Node[nTo].parentNum.ToString() + "(" + Network[currentN].Node[nTo].Kind + ")";

                        Error[nError].messageNum = 14;
                        //nError++;
                        add_Error();
                    }
                }
            }
        }

        private void make_ConcurrencyIrreducible(int fromN, int toN, int workLoop, int loop, int conK)
        {
            int nNode = 0;
            strNode[] imNode = new strNode[Network[fromN].nNode];

            int nLink = 0;
            strLink[] imLink = new strLink[Network[fromN].nLink];

            //기존 Nodeㅑ
            for (int i = 0; i < Network[fromN].nNode; i++)
            {
                bool bSame = false;

                for (int k = 0; k < nSearchNode; k++)
                {
                    if (searchNode[k] == Network[fromN].Node[i].orgNum)
                    {
                        bSame = true;
                        break;
                    }
                }
                if (bSame) continue;


                // bSame == false
                imNode[nNode] = Network[fromN].Node[i];
                nNode++;
            }

            //Node 추가 (Merge_Join, Merge_Split)

            imNode[nNode].Kind = "XOR";
            imNode[nNode].Name = "MG";
            imNode[nNode].Type_I = "_j";
            imNode[nNode].parentNum = -1;// imNode[newCon[0]].parentNum;//- 1;
            imNode[nNode].orgNum = -1;// imNode[newCon[0]].parentNum; // -1;
            imNode[nNode].Special = "";
            nNode++;

            imNode[nNode].Kind = "XOR";
            imNode[nNode].Name = "MG";
            imNode[nNode].Type_I = "_s";
            imNode[nNode].parentNum = -1;// imNode[newCon[0]].parentNum;//- 1;
            imNode[nNode].orgNum = -1;// imNode[newCon[0]].parentNum; // -1;
            imNode[nNode].Special = "";
            nNode++;

            // Link 작업
            for (int i = 0; i < Network[fromN].nLink; i++)
            {
                //Entry link 모두 제거
                if (Network[fromN].Link[i].fromNode == Network[fromN].header) continue;

                int fromOrg = Network[fromN].Node[Network[fromN].Link[i].fromNode].orgNum;
                int toOrg = Network[fromN].Node[Network[fromN].Link[i].toNode].orgNum;

                bool fSearch = false, tSearch = false;

                for (int k = 0; k < nSearchNode; k++)
                {
                    if (searchNode[k] == fromOrg) fSearch = true;
                    if (searchNode[k] == toOrg) tSearch = true;
                }


                if (fSearch && tSearch) continue;



                if (fSearch)
                {
                    int nTo = -1;

                    for (int k = 0; k < nNode - 2; k++)
                    {
                        if (imNode[k].orgNum == -1 && imNode[k].Kind != "END") continue;

                        if (imNode[k].orgNum == toOrg)
                        {
                            nTo = k;
                            break;
                        }
                    }

                    if (nTo >= 0)
                    {
                        imLink[nLink] = Network[fromN].Link[i];
                        imLink[nLink].fromNode = nNode - 1;
                        imLink[nLink].toNode = nTo;
                        nLink++;
                    }
                }
                else if (tSearch)
                {
                    int nFrom = -1;

                    for (int k = 0; k < nNode - 2; k++)
                    {
                        if (imNode[k].orgNum == fromOrg)
                        {
                            nFrom = k;
                            break;
                        }
                    }
                    if (nFrom >= 0)
                    {
                        imLink[nLink] = Network[fromN].Link[i];
                        imLink[nLink].fromNode = nFrom;
                        imLink[nLink].toNode = nNode - 2;
                        nLink++;
                    }
                }
                else
                {
                    int nFrom = -1;
                    int nTo = -1;

                    for (int k = 0; k < nNode - 2; k++)
                    {
                        if (imNode[k].orgNum == -1 && imNode[k].Kind != "END") continue;

                        if (imNode[k].orgNum == fromOrg) nFrom = k;

                        if (imNode[k].orgNum == toOrg) nTo = k;
                    }

                    if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                    {
                        imLink[nLink] = Network[fromN].Link[i];
                        imLink[nLink].fromNode = nFrom;
                        imLink[nLink].toNode = nTo;
                        nLink++;
                    }

                }
            }

            int nHead = -1;
            for (int k = 0; k < nNode - 2; k++)
            {
                if (imNode[k].orgNum == -1 && imNode[k].Kind == "START")
                {
                    nHead = k;
                    break;
                }
            }

            if (nHead >= 0)
            {
                imLink[nLink].fromNode = nHead;
                imLink[nLink].toNode = nNode - 2;
                nLink++;
            }
            imLink[nLink].fromNode = nNode - 2;
            imLink[nLink].toNode = nNode - 1;
            nLink++;


            //SUb network
            Network[toN] = new strNetwork();
            Network[toN].nNode = nNode;
            Network[toN].Node = new strNode[Network[toN].nNode];
            for (int i = 0; i < nNode; i++)
            {
                Network[toN].Node[i] = imNode[i];
            }
            Network[toN].nLink = nLink;
            Network[toN].Link = new strLink[Network[toN].nLink];
            for (int i = 0; i < nLink; i++)
            {
                Network[toN].Link[i] = imLink[i];
                Network[toN].Link[i].bBackJ = false;
                Network[toN].Link[i].bBackS = false;
            }

            for (int i = 0; i < Network[toN].nNode; i++)
            {
                find_NodeInform(toN, i);
            }
        }

        private void make_IndependentIrreducible(int fromN, int toN, int nEntry)
        {
            // 새네트워크 생성 (복제)
            Network[toN] = Network[fromN];
            extent_Network(toN, 0);

            for (int i = 0; i < Network[toN].nLink; i++)
            {
                if (Network[toN].Link[i].fromNode == Network[toN].header)
                {
                    if (Network[toN].Node[Network[toN].Link[i].toNode].orgNum == nEntry) continue;

                    Network[toN].Link[i].toNode = Network[toN].Link[i].fromNode;
                }
            }

            for (int i = 0; i < Network[toN].nNode; i++)
            {
                find_NodeInform(toN, i);
            }
        }

       
        //simplified version for Count cardinality
        private void make_InstanceFlow_simplified(int currentN, int loop, string errType, string strLoop) 
        {

            SearchXOR = new int[Network[currentN].nNode]; // 0-탐색
            nSearchXOR = 0;
            nCurrentXOR = 0;

            int nInst = 0;

            do
            {
                nCurrentXOR = 0;

                for (int j = 0; j < Network[currentN].nLink; j++) //fill all this subnetwork is un-visit
                {
                    Network[currentN].Link[j].bInstance = false; //bInstance is used for mark the node which have token (instance flow)
                }

                InstantNode = new int[Network[currentN].nNode];
                nInstantNode = 0;

                int sNode = Network[currentN].header;

                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                if (find_InstanceNode(currentN, sNode)) //find_InstanceNode() => output will be stored in InstantNode[] (Global variable!) - all the node which is activated in currentN.
                {
                    string[] readable = new string[nInstantNode];
                    convert_Readable(currentN, InstantNode, ref readable);
                    //Make_InstantNetwork(currentN, dummyNet); // 삭제 ??????????????
                    nInst++;
                    

                    //check_InstanceFlow(currentN, loop, errType, strLoop);  //bFor = true; => Forward// bFor = false; => Backward error reporting. (Errors[])
                }

            } while (nSearchXOR > 0);
            cardinality = nInst; //Newcode

        }
        private void make_InstanceFlow(int currentN, int loop, string errType, string strLoop) //loop = -1 => final-Acyclic ; loop = -2 => SESE (new)
        {
            SearchXOR = new int[Network[currentN].nNode]; // 0-탐색
            nSearchXOR = 0;
            nCurrentXOR = 0;

            int nInst = 0;

            //DateTime stTime2 = new DateTime();
            //stTime2 = DateTime.Now;
            //double Run_Times = 0;
            do
            {
                nCurrentXOR = 0;

                for (int j = 0; j < Network[currentN].nLink; j++) //fill all this subnetwork is un-visit
                {
                    Network[currentN].Link[j].bInstance = false; //bInstance is used for mark the node which have token (instance flow)
                }

                InstantNode = new int[Network[currentN].nNode];
                nInstantNode = 0;

                int sNode = Network[currentN].header;

                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                //DateTime stTime = new DateTime();
                //stTime = DateTime.Now;
                //Instant Flow 찾으면
                if (find_InstanceNode(currentN, sNode)) //find_InstanceNode() => output will be stored in InstantNode[] (Global variable!) - all the node which is activated in currentN.
                {
                    string[] readable = new string[nInstantNode];
                    convert_Readable(currentN, InstantNode, ref readable);
                    //Make_InstantNetwork(currentN, dummyNet); // 삭제 ??????????????
                    nInst++;
                    //if (nInst > 2097152)
                    //{
                    //Run_Times += (DateTime.Now - stTime).TotalSeconds;
                    //MessageBox.Show(nInst.ToString(), "Long");
                    //}

                    //Moi remove code o day==========
                    //if (nInst == 2) break;

                    check_InstanceFlow(currentN, loop, errType, strLoop);  //bFor = true; => Forward// bFor = false; => Backward error reporting. (Errors[])
                }
            } while (nSearchXOR > 0);
            //double Run_Total = 0;
            //Run_Total += (DateTime.Now - stTime2).TotalSeconds;
            //MessageBox.Show(nInst.ToString(), "Long");
            cardinality = nInst; //Newcode
        }

        public void convert_Readable(int currentN, int[] instantNode, ref string[] readable)
        {
            //readable = new int[nInstantNode];
            for (int i = 0; i < nInstantNode; i++)
            {
                readable[i] = Network[currentN].Node[instantNode[i]].Name;
            }
        }

        private void Make_InstantNetwork(int orginN, int currentN)
        {
            Network[currentN] = Network[orginN];
            extent_Network(currentN, 0);

            for (int k = 0; k < Network[currentN].nLink; k++)
            {
                if (!Network[currentN].Link[k].bInstance)
                {
                    Network[currentN].Link[k].toNode = Network[currentN].Link[k].fromNode;
                }
            }
        }

        private void check_InstanceFlow(int currentN, int loop, string errType, string strLoop)
        {

            for (int i = 0; i < nInstantNode; i++)
            {
                //XOR Join에 여러 Instant In이면 error
                if (Network[currentN].Node[InstantNode[i]].Kind == "XOR" && Network[currentN].Node[InstantNode[i]].nPre > 1)
                {
                    int numIn = 0;
                    for (int j = 0; j < Network[currentN].Node[InstantNode[i]].nPre; j++)
                    {
                        bool bLink = false;
                        for (int k = 0; k < Network[currentN].nLink; k++)
                        {
                            if (Network[currentN].Link[k].fromNode == Network[currentN].Node[InstantNode[i]].Pre[j] && Network[currentN].Link[k].toNode == InstantNode[i])
                            {
                                if (Network[currentN].Link[k].bInstance) bLink = true;
                                break;
                            }
                        }
                        if (!bLink) continue;

                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (Network[currentN].Node[InstantNode[i]].Pre[j] == InstantNode[k]) numIn++;
                        }
                    }

                    if (numIn > 1) //error
                    {
                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[InstantNode[i]].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[InstantNode[i]].Kind;

                        if (loop == -1)
                        {
                            Error[nError].messageNum = 10;
                        }
                        else
                        {
                            if (errType == "SESE")
                            {
                                Error[nError].Loop = "";
                                Error[nError].SESE = strLoop;
                                Error[nError].messageNum = 27; //SESE lack of synchronization

                            }
                            if (errType == "eFwd") Error[nError].messageNum = 3; //rule 2.1
                            if (errType == "eBwd") Error[nError].messageNum = 4; //rule 2.1
                            if (errType == "DFlow_PdFlow") Error[nError].messageNum = 24; //rule 6.1
                            if (errType == "iDFlow_PdFlow") Error[nError].messageNum = 25; //rule 6.2
                            if (errType == "HFlow") Error[nError].messageNum = 26; //rule 6.3
                                                    
                        }

                        //nError++;
                        add_Error();
                    }
                }

                //AND Join에  Instant In이 부족하면 error
                if (Network[currentN].Node[InstantNode[i]].Kind == "AND" && Network[currentN].Node[InstantNode[i]].nPre > 1)
                {
                    int numIn = 0;
                    for (int j = 0; j < Network[currentN].Node[InstantNode[i]].nPre; j++)
                    {
                        bool bLink = false;
                        for (int k = 0; k < Network[currentN].nLink; k++)
                        {
                            if (Network[currentN].Link[k].fromNode == Network[currentN].Node[InstantNode[i]].Pre[j] && Network[currentN].Link[k].toNode == InstantNode[i])
                            {
                                if (Network[currentN].Link[k].bInstance) bLink = true;
                                break;
                            }
                        }
                        if (!bLink) continue;

                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (Network[currentN].Node[InstantNode[i]].Pre[j] == InstantNode[k]) numIn++;
                        }
                    }

                    if (numIn < Network[currentN].Node[InstantNode[i]].nPre) //error
                    {
                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[InstantNode[i]].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[InstantNode[i]].Kind;

                        if (loop == -1)
                        {
                            Error[nError].messageNum = 11;
                        }
                        else
                        {
                            if (errType == "SESE")
                            {
                                Error[nError].Loop = "";
                                Error[nError].SESE = strLoop;
                                Error[nError].messageNum = 28; //SESE deadlock 

                            }
                            if (errType == "eFwd") Error[nError].messageNum = 5;
                            if (errType == "eBwd") Error[nError].messageNum = 6;
                            //same for XOR errors
                            if (errType == "DFlow_PdFlow") Error[nError].messageNum = 24; //rule 6.1
                            if (errType == "iDFlow_PdFlow") Error[nError].messageNum = 25; //rule 6.2
                            if (errType == "HFlow") Error[nError].messageNum = 26; //rule 6.3
                            //if (errType == "SESE") Error[nError].messageNum = 28; 
                        }
                        //nError++;
                        add_Error();
                    }
                }
            }
        }
        
        //check whether the AND sl
        private void check_ParallelFlow(int currentN, int loop, bool bFor, string strLoop) 
        {

            // AND Spit 후 AND join 이전에 Exit (not Terminal) or BackSplit 있으면 error
            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if ((Network[currentN].Node[i].Special == "X") || (bFor && Network[currentN].Node[i].Special == "B"))
                {
                    if (find_ANDsplit(currentN, i) == 1) //error // Check whether the AND Split to this node exist or not => if exist (return 1) => error - conflic here
                    {                                           //because before Backward split and Exit node do not allow AND Split => 1 token left 1 token in the loop => error
                        Error[nError].Loop = strLoop;
                        Error[nError].Node = Network[currentN].Node[i].parentNum.ToString();
                        Error[nError].currentKind = Network[currentN].Node[i].Kind;

                        if (bFor) Error[nError].messageNum = 7;
                        else Error[nError].messageNum = 8;

                        //nError++;
                        add_Error();

                    }

                }

            }

        }

        //Return 0 if it is AND_split, 1 if not
        //May be it find out that whether have any concurrent structures from "node"
        private int find_ANDsplit(int currentN, int node)
        {
            //int OorC = 0; //OorC - -1; close  OorC  1: open   0: unknown

            for (int i = 0; i < Network[currentN].Node[node].nPre; i++)
            {
                //OorC = 0;
                int preNode = Network[currentN].Node[node].Pre[i];
                if (Network[currentN].Node[preNode].Kind == "AND")
                {
                    if (Network[currentN].Node[preNode].nPost > 1)
                    {
                        //OorC = 1;   //AND split
                        return 1;
                    }
                    //else
                    //{
                    //    OorC = -1;   //AND join
                    //}
                    //else if (Network[currentN].Node[preNode].nPre > 1) OorC = -1;   //AND join
                    //else OorC = 0;
                }
                else
                {
                    if (find_ANDsplit(currentN, preNode) == 1) return 1;
                }

                //if (OorC == 1)
                //{
                //    break;
                //}
                //else if (OorC == 0)
                //{
                //    if (find_ANDsplit(currentN, preNode) == 1) return 1;
                //    //OorC = find_ANDsplit(currentN, preNode);
                //    //if (OorC == 1) break;
                //}
            }


            return 0;
        }

        private void reduce_Network(int currentN, int workLoop, int loop, string strLoop, bool checkError) //Reduce all Loops
        {
            //Multi entry - Multi Exit이면 분리????????????????????????
            bool doSplit = false;
            //if (Loop[workLoop].Loop[loop].nEntry > 1 && Loop[workLoop].Loop[loop].nExit > 1) doSplit = true; //why split here??

            // Loop내 노드만 구성
            int[] imNode = new int[Network[currentN].nNode];
            int num = 0;

            imNode[num] = Loop[workLoop].Loop[loop].header;
            num++;
            for (int j = 0; j < Loop[workLoop].Loop[loop].nNode; j++)
            {
                Network[currentN].Node[Loop[workLoop].Loop[loop].Node[j]].done = true; // header제외한 loop내 노드 축소
                imNode[num] = Loop[workLoop].Loop[loop].Node[j];
                num++;
            }

            int loopNode = Loop[workLoop].Loop[loop].header; //Loop[workLoop].Loop[loop].header를 대표 Node 로
            Network[currentN].Node[loopNode].Kind = "XOR"; // 대표 Node는 XOR노드로........

            Network[currentN].Node[loopNode].Name = "L[" + loop.ToString() + "]";
            Network[currentN].Node[loopNode].Type_I = "";
            Network[currentN].Node[loopNode].Type_II = "";

            int exitNode = loopNode;
            if (doSplit)
            {
                exitNode = Loop[workLoop].Loop[loop].Exit[0];
                Network[currentN].Node[exitNode].done = false;

                Network[currentN].Node[loopNode].Type_I = "_j";

                Network[currentN].Node[exitNode].Kind = "XOR"; // 대표 Node는 XOR노드로........
                Network[currentN].Node[exitNode].Name = "L[" + loop.ToString() + "]";
                Network[currentN].Node[exitNode].Type_I = "_s";
                Network[currentN].Node[exitNode].Type_II = "";
            }

            //irreducible loop이면
            //헤더아닌 entry노드로 들어오는 링크를 헤더로 변경*******************************
            if (Loop[workLoop].Loop[loop].Irreducible)
            {
                int fromNode = -1;
                for (int k = 0; k < Network[currentN].nLink; k++)
                {
                    if (Network[currentN].Link[k].toNode == loopNode)
                    {
                        fromNode = Network[currentN].Link[k].fromNode;
                        break;
                    }
                }

                for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++)
                {
                    if (Loop[workLoop].Loop[loop].Entry[i] == loopNode) continue;

                    for (int k = 0; k < Network[currentN].nLink; k++)
                    {
                        if (Network[currentN].Link[k].toNode == Loop[workLoop].Loop[loop].Entry[i])
                        {
                            if (Network[currentN].Link[k].fromNode == fromNode)
                            {
                                Network[currentN].Link[k].toNode = fromNode;
                            }
                            else
                            {
                                Network[currentN].Link[k].toNode = loopNode;
                            }

                        }
                    }

                }

            }
            //**************************************************************************
            //대표 Node 정보 변경
            int pLoop = Loop[workLoop].Loop[loop].parentLoop;

            bool makeSplitLink = false;

            for (int k = 0; k < Network[currentN].nLink; k++)
            {
                if (Network[currentN].Link[k].fromNode == loopNode) //대표노드로 부터 나가는 링크 제거
                {
                    if (doSplit && !makeSplitLink) // Split 연결
                    {
                        Network[currentN].Link[k].toNode = exitNode;
                        makeSplitLink = true;
                    }
                    else
                    {
                        Network[currentN].Link[k].fromNode = Network[currentN].Link[k].toNode;
                    }
                }

                if (Network[currentN].Link[k].toNode == loopNode) //대표노드로 들어오는 내부 링크 제거
                {
                    bool bInLoop = false;
                    for (int j = 0; j < num; j++)
                    {
                        if (Network[currentN].Link[k].fromNode == imNode[j])
                        {
                            bInLoop = true;
                            break;
                        }
                    }

                    if (bInLoop)
                    {
                        Network[currentN].Link[k].toNode = Network[currentN].Link[k].fromNode;
                    }
                }
            }


            for (int i = 0; i < Loop[workLoop].Loop[loop].nExit; i++)
            {
                if (pLoop >= 0)
                {
                    for (int j = 0; j < Loop[workLoop].Loop[pLoop].nExit; j++)
                    {
                        if (Loop[workLoop].Loop[loop].Exit[i] == Loop[workLoop].Loop[pLoop].Exit[j]) //Parent와 Exit 공유하면
                        {
                            Loop[workLoop].Loop[pLoop].Exit[j] = exitNode; //대표노드를 Parent의 Exit으로 변경
                            Network[currentN].Node[exitNode].Special = Network[currentN].Node[Loop[workLoop].Loop[loop].Exit[i]].Special;
                            break;
                        }
                    }
                }

                for (int k = 0; k < Network[currentN].nLink; k++) //exit노드로 부터 loop밖으로 나가는 링크를 대표노드로 부터 나가도록 변경
                {
                    if (Network[currentN].Link[k].fromNode == Loop[workLoop].Loop[loop].Exit[i])
                    {
                        bool bInLoop = false;
                        for (int j = 0; j < num; j++)
                        {
                            if (Network[currentN].Link[k].toNode == imNode[j])
                            {
                                bInLoop = true;
                                break;
                            }
                        }

                        if (!bInLoop)
                        {
                            Network[currentN].Link[k].fromNode = exitNode;
                        }

                    }
                }

            }


            //동일한 노드로의 Exit이면 Error
            if (checkError)
            {
                for (int k = 0; k < Network[currentN].nLink; k++)
                {
                    if (Network[currentN].Link[k].fromNode != exitNode) continue;

                    for (int k2 = k + 1; k2 < Network[currentN].nLink; k2++)
                    {
                        if (Network[currentN].Link[k2].fromNode != exitNode) continue;

                        if (Network[currentN].Link[k].toNode == Network[currentN].Link[k2].toNode)
                        {
                            Network[currentN].Link[k2].fromNode = Network[currentN].Link[k2].toNode;

                            if (Network[currentN].Node[Network[currentN].Link[k].toNode].Kind == "END") continue;

                            if (Network[currentN].Node[Network[currentN].Link[k].toNode].Kind == "XOR") continue;

                            //Error
                            Error[nError].Loop = strLoop;
                            Error[nError].Node = Network[currentN].Node[Network[currentN].Link[k].toNode].parentNum.ToString();

                            Error[nError].currentKind = Network[currentN].Node[Network[currentN].Link[k].toNode].Kind;
                            Error[nError].messageNum = 9;

                            //nError++;
                            add_Error();

                        }
                    }
                }
            }


            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].done)
                {
                    Network[currentN].Node[i].nPre = 0;
                    Network[currentN].Node[i].nPost = 0;
                    Network[currentN].Node[i].Pre = null;
                    Network[currentN].Node[i].Post = null;
                }
                else
                {
                    find_NodeInform(currentN, i);
                }
            }



        }

        #endregion

        private void add_Error()
        {
            bool bSame = false;
            for (int i = 0; i < nError; i++)
            {
                if (Error[i].Node == Error[nError].Node && Error[i].Loop == Error[nError].Loop && Error[i].currentKind == Error[nError].currentKind && Error[i].messageNum == Error[nError].messageNum)
                {
                    bSame = true;
                    break;
                }
            }
            if (!bSame) nError++;
        }

        private void Run_CheckNetwork()
        {
            if (IrreducibleError || ConcurrencyError) return;

            make_subNetwork(reduceNet, seseNet, reduceLoop, -1, "AC", -1);    //5는 SubNetwork

            #region Unnecessary Code
            // Check SESE
            //Check_SESENetwork(seseNet, orgSESE, -1, true, ""); //I think no need to check SESE again??
            
            // Check remain ==> Should be careful
            //make_subNetwork(redSeNet, subNet, reduceLoop, -1, "AC", -1);    //5는 SubNetwork 
            //make_InstanceFlow(subNet, -1, true, ""); //I think we also no need it

            //new===========================================================
            //find_SESE(seseNet, orgSESE, -1);
            #endregion

            Network[redSeNet] = Network[seseNet];
            extent_Network(redSeNet, 0);

            #region Unnecessary Code
            /*copy_SESE(orgSESE, reduceSESE);
            int curDepth = SESE[reduceSESE].maxDepth;
            do
            {
                for (int j = 0; j < SESE[reduceSESE].nSESE; j++)
                {
                    if (SESE[reduceSESE].SESE[j].depth != curDepth) continue;

                    // redSeNet에서 해당 SESE네트워크 추출
                    make_subNetwork(redSeNet, subNet, reduceSESE, j, "SESE", -1);

                    //make_InstanceFlow(subNet, loop, bFor, strLoop);

                    //reduce....
                    reduce_seseNetwork(redSeNet, reduceSESE, j);
                }

                curDepth--;
            } while (curDepth > 0);*/
            #endregion

            make_subNetwork(redSeNet, subNet, reduceLoop, -1, "AC", -1);
            make_InstanceFlow(finalNet, -1, "", "");
            //==========================================================================

            //마지막 Acyclic Network 저장
            Network[acyclicNet] = Network[subNet];
            extent_Network(acyclicNet, 0);
        }


        //Find SESE Network => Type 3 Split => reduce to 1 node.
        //Need to careful consider this case => it find type III split again ?
        private void Check_SESENetwork(int currentN, int currentSESE, int loop, bool bFor, string strLoop)
        {
            //SESE 찾기
            preProcessingSESE(currentN, currentSESE, -1);
            find_SESE_new(currentN, currentSESE, -1);

            // Type III split

            int curDepth = SESE[currentSESE].maxDepth;
            do
            {
                for (int j = 0; j < SESE[currentSESE].nSESE; j++)
                {
                    if (SESE[currentSESE].SESE[j].depth != curDepth) continue;

                    int sNode = SESE[currentSESE].SESE[j].Entry;
                    int eNode = SESE[currentSESE].SESE[j].Exit;

                    // start Node 추가
                    if (!check_allIn(currentN, currentSESE, sNode, j, true))
                    {
                        Type_III_Split_Entry(currentN, currentSESE, j, 1, sNode, sNode, "");
                    }

                    // end Node 추가
                    if (!check_allIn(currentN, currentSESE, eNode, j, false))
                    {
                        Type_III_Split_Exit(currentN, currentSESE, j, 1, eNode, eNode);
                    }
                }

                curDepth--;
            } while (curDepth > 0);

            //for (int j = 0; j < SESE[currentSESE].nSESE; j++)
            //{
            //    int sNode = SESE[currentSESE].SESE[j].Entry;
            //    int eNode = SESE[currentSESE].SESE[j].Exit;

            //    // start Node 추가
            //    if (!check_allIn(currentN, currentSESE, sNode, j, true))
            //    {
            //        Type_III_Split_Entry(currentN, currentSESE, j, 1, sNode, sNode, "");
            //    }

            //    // end Node 추가
            //    if (!check_allIn(currentN, currentSESE, eNode, j, false))
            //    {
            //        Type_III_Split_Exit(currentN, currentSESE, j, 1, eNode, eNode);
            //    }
            //}


            // check & merge - redSeNet과 reduceSESE로 작업
            Network[redSeNet] = Network[currentN];
            extent_Network(redSeNet, 0);

            copy_SESE(currentSESE, reduceSESE);

            curDepth = SESE[reduceSESE].maxDepth;
            do
            {
                for (int j = 0; j < SESE[reduceSESE].nSESE; j++)
                {
                    if (SESE[reduceSESE].SESE[j].depth != curDepth) continue;

                    // redSeNet에서 해당 SESE네트워크 추출
                    make_subNetwork(redSeNet, subNet, reduceSESE, j, "SESE", -1);

                    make_InstanceFlow(subNet, loop, "", strLoop);

                    //reduce....
                    reduce_seseNetwork(redSeNet, reduceSESE, j);
                }

                curDepth--;
            } while (curDepth > 0);



        }


        private void reduce_seseNetwork(int currentN, int workSESE, int kSESE)
        {
            // SESE내 노드만 구성

            for (int j = 0; j < SESE[workSESE].SESE[kSESE].nNode; j++)
            {
                if (SESE[workSESE].SESE[kSESE].Node[j] == SESE[workSESE].SESE[kSESE].Entry) continue;

                Network[currentN].Node[SESE[workSESE].SESE[kSESE].Node[j]].done = true; // entry제외한 sese내 노드 축소
            }

            int sNode = SESE[workSESE].SESE[kSESE].Entry; //Loop[workLoop].Loop[loop].header를 대표 Node 로
            int eNode = SESE[workSESE].SESE[kSESE].Exit; //Loop[workLoop].Loop[loop].header를 대표 Node 로
            //Network[currentN].Node[loopNode].Kind = "XOR"; // 대표 Node는 XOR노드로........



            //대표 Node 정보 변경

            Network[currentN].Node[sNode].Name = "S[" + kSESE.ToString() + "]";
            Network[currentN].Node[sNode].Type_I = "";
            Network[currentN].Node[sNode].Type_II = "";

            for (int k = 0; k < Network[currentN].nLink; k++)
            {
                //대표노드로 부터 SESE내로 나가는 링크 제거

                if (Network[currentN].Link[k].fromNode == sNode)
                {
                    bool inSESE = false;
                    for (int m = 0; m < SESE[workSESE].SESE[kSESE].nNode; m++)
                    {
                        if (Network[currentN].Link[k].toNode == SESE[workSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        Network[currentN].Link[k].fromNode = Network[currentN].Link[k].toNode;
                    }
                }
            }

            for (int k = 0; k < Network[currentN].nLink; k++)
            {
                if (Network[currentN].Link[k].toNode == eNode) continue;

                //대표노드로 부터 SESE내로 나가는 링크 제거
                if (Network[currentN].Link[k].fromNode == eNode)
                {
                    Network[currentN].Link[k].fromNode = sNode;
                }
            }

            //부모 Exit정보 변경 (삭제되는 Exit 공유하는 경우)
            for (int j = 0; j < SESE[workSESE].nSESE; j++)
            {
                if (j == kSESE) continue;

                if (SESE[workSESE].SESE[j].Exit == eNode)
                {
                    SESE[workSESE].SESE[j].Exit = sNode;
                }

            }

            for (int i = 0; i < Network[currentN].nNode; i++)
            {
                if (Network[currentN].Node[i].done)
                {
                    Network[currentN].Node[i].nPre = 0;
                    Network[currentN].Node[i].nPost = 0;
                    Network[currentN].Node[i].Pre = null;
                    Network[currentN].Node[i].Post = null;
                }
                else
                {
                    find_NodeInform(currentN, i);
                }
            }

        }




        /*public void Run_Test()
        {
            Run_Split_Type1();
            Run_FindLoop();
            Run_Split_Type2();
            Run_CheckLoop();
            //Run_CheckNetwork();

        }*/



        /////////////////////////////////////////////////---------------
        private void prepare_Concurrency(int currentN, int workLoop, int loop)
        {
            npLoopS = 0; //number of parent
            pLoopS = new int[Loop[workLoop].nLoop]; //pLoopS => parent(s) of this loop
            find_ParentLoop(workLoop, loop); // return value of pLoops

            int curDepth = Loop[workLoop].maxDepth;
            do
            {
                for (int j = 0; j < Loop[workLoop].nLoop; j++)
                {
                    if (Loop[workLoop].Loop[j].depth != curDepth) continue;
                    if (Network[currentN].Node[Loop[workLoop].Loop[j].header].Name.Substring(0, 1) == "L") continue;
                    if (j == loop) continue; 
                    bool bParent = false;
                    for (int k = 0; k < npLoopS; k++)
                    {
                        if (j == pLoopS[k])
                        {
                            bParent = true;
                            break;
                        }
                    }
                    if (bParent) continue;

                    reduce_Network(currentN, workLoop, j, "", false);
                }

                curDepth--;
            } while (curDepth > 0);


            find_Dom(currentN);
            find_DomRev(currentN);
        }

        //The final result for this procedure is fill the information of concurrency to workLoop (loop[2]) (loop.concurrency[], loop.nConcurrency)
        private void check_Concurrency(int currentN, int workLoop, int loop)
        {

            //make_ConcurrencyFlow

            Network[conNet] = Network[currentN]; //CIDFlow begining is stored in conNet (Network[8])
            extent_Network(conNet, 0);

            copy_Loop(workLoop, tempLoop);

            prepare_Concurrency(conNet, tempLoop, loop); //reduce all loop except "loop" and its parents.

            make_subNetwork(conNet, subNet, tempLoop, loop, "CC", -1);    //5는 SubNetwork //"CC" ~ concurrent entries => after that we will have iDFlow()

            make_ConcurrencyInstance(subNet, tempLoop, loop); //tempLoop ~ Loop[3]

            //Prepare the entry conEntry[,] array => Simply set the value by [1 0 0] [0 1 0] or ...
            prepare_Find_CC(subNet, tempLoop, loop); //new

            //Everything related with CE will be solved in here =====================================
            //find_ConcurrencyEntrySet(subNet, tempLoop, loop); //=====================================
            //=======================================================================================

            //copy concurrency inform => IT WAS WRONG!!!!
            Loop[workLoop].Loop[loop].nConcurrency = Loop[tempLoop].Loop[loop].nConcurrency; //tranfer all the concurrency value from tempLoop [3] to workloop [2]
            if (Loop[tempLoop].Loop[loop].Concurrency != null)
            {
                Loop[workLoop].Loop[loop].Concurrency = new int[Loop[workLoop].Loop[loop].nEntry];
                for (int k = 0; k < Loop[workLoop].Loop[loop].nEntry; k++)
                    Loop[workLoop].Loop[loop].Concurrency[k] = Loop[tempLoop].Loop[loop].Concurrency[k];
            }

        }

        private void find_ParentLoop(int workLoop, int loop)
        {
            if (Loop[workLoop].Loop[loop].parentLoop == -1) return;

            pLoopS[npLoopS] = Loop[workLoop].Loop[loop].parentLoop;
            npLoopS++;

            find_ParentLoop(workLoop, Loop[workLoop].Loop[loop].parentLoop);

        }

        private void make_ConcurrencyInstance(int currentN, int workLoop, int loop) //Must check here !!!!!!!!!!!!
        {
            int conNum = 0;
            int[][] conEntry = new int[Loop[workLoop].Loop[loop].nEntry][];
            for (int i = 0; i < Loop[workLoop].Loop[loop].nEntry; i++)
            {
                conEntry[i] = new int[Loop[workLoop].Loop[loop].nEntry];
            }

            //set the initiated value for SearchXOR, nSearch, nCurrentXOR
            SearchXOR = new int[Network[currentN].nNode]; // 0-탐색 //navigation??
            nSearchXOR = 0;
            nCurrentXOR = 0;

            do
            {
                nCurrentXOR = 0;

                for (int j = 0; j < Network[currentN].nLink; j++)
                {
                    Network[currentN].Link[j].bInstance = false;
                }

                InstantNode = new int[Network[currentN].nNode];
                nInstantNode = 0;

                int sNode = Network[currentN].header;

                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                //Instant Flow 찾으면

                if (find_InstanceNode(currentN, sNode)) //SearchXOR[] will be use in here //instanceNode() => will find the path of each instance flow (i.g. 1->4->5->8)
                {
                    //find
                    int[] imEntry = new int[Loop[workLoop].Loop[loop].nEntry];
                    bool isError = true;
                    for (int i = 0; i < nInstantNode; i++)
                    {
                        for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                        {
                            if (Network[currentN].Node[InstantNode[i]].orgNum == Loop[workLoop].Loop[loop].Entry[j])
                            {
                                imEntry[j] = 1; //import entry cua node thu j (ko phai node j) = 1 sau nay no co the = 2 3 4 ...
                                isError = false;
                                break;
                            }
                        }
                    }

                    if (!isError)
                    {
                        //Smae Check
                        int sameCon = 0; //sameConcurrent??
                        bool[] kCon = new bool[conNum];

                        for (int k = 0; k < conNum; k++)
                        {
                            for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                            {
                                if (imEntry[j] == 1 && conEntry[k][j] == 1)
                                {
                                    sameCon++;
                                    kCon[k] = true;
                                    break;
                                }
                            }
                        }

                        if (sameCon == 0)
                        {
                            conEntry[conNum] = imEntry;
                            conNum++;
                        }
                        else
                        {

                            int conNum_T = conNum;
                            int[][] conEntry_T = new int[conNum][];
                            for (int k = 0; k < conNum_T; k++)
                            {
                                conEntry_T[k] = new int[Loop[workLoop].Loop[loop].nEntry];
                                for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                                {
                                    conEntry_T[k][j] = conEntry[k][j];
                                }
                            }


                            conNum = 0;
                            for (int k = 0; k < conNum_T; k++)
                            {
                                if (kCon[k]) continue;
                                for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                                {
                                    conEntry[conNum][j] = conEntry[k][j];
                                }

                                conNum++;
                            }
                            for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                            {
                                conEntry[conNum][j] = 0;
                                if (imEntry[j] == 1)
                                {
                                    conEntry[conNum][j] = 1;
                                }
                                for (int k = 0; k < conNum_T; k++)
                                {
                                    if (!kCon[k]) continue;
                                    if (conEntry_T[k][j] == 1)
                                    {
                                        conEntry[conNum][j] = 1;
                                    }
                                }
                            }
                            conNum++;
                        }
                    }
                } // if error

            } while (nSearchXOR > 0);

            Loop[workLoop].Loop[loop].Concurrency = new int[Loop[workLoop].Loop[loop].nEntry]; //here => what I need to know //number of Concurrency = number of Loop Entry

            int numType = 1;
            for (int k = 0; k < conNum; k++)
            {
                int cntTrue = 0;
                for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                {
                    if (conEntry[k][j] == 1) cntTrue++;
                }

                if (cntTrue > 1)
                {
                    for (int j = 0; j < Loop[workLoop].Loop[loop].nEntry; j++)
                    {
                        if (conEntry[k][j] == 1)
                        {
                            Loop[workLoop].Loop[loop].Concurrency[j] = numType;
                        }
                    }
                    numType++;
                }

            }
            Loop[workLoop].Loop[loop].nConcurrency = numType - 1;

        }

        private void inspect_Concurrency(int currentN, int workLoop, int loop, int conK)
        {
            // 1. 우선 역 Edge방향 PD찾기

            //Network 만들기 //Make Network (conNet)
            Network[conNet] = Network[currentN];
            extent_Network(conNet, 0);

            copy_Loop(workLoop, tempLoop);

            //Loop내 Edge방향 변경 //Reverse the direction of all Edges in this loop
            for (int k = 0; k < Network[conNet].nLink; k++)
            {
                if (Node_In_Loop(tempLoop, Network[conNet].Link[k].fromNode, loop) && Node_In_Loop(tempLoop, Network[conNet].Link[k].toNode, loop))
                {
                    int tempNode = Network[conNet].Link[k].fromNode;
                    Network[conNet].Link[k].fromNode = Network[conNet].Link[k].toNode;
                    Network[conNet].Link[k].toNode = tempNode;
                }
            }

            for (int i = 0; i < Network[conNet].nNode; i++)
            {
                find_NodeInform(conNet, i);
            }

            prepare_Concurrency(conNet, tempLoop, loop);

            //해당 노드 검색 //Retrieve the Node
            nSearchNode = 0;
            searchNode = new int[Network[conNet].nNode];

            int[] calDom = null;
            for (int k = 0; k < Loop[tempLoop].Loop[loop].nEntry; k++) //Find the CIPd of all entries
            {
                if (Loop[tempLoop].Loop[loop].Concurrency[k] != conK) continue;

                calDom = find_Intersection(Network[conNet].nNode, calDom, Network[conNet].Node[Loop[tempLoop].Loop[loop].Entry[k]].DomRev);
            }

            //Concurrency Error Check!!!!!!!!!!!
            if (calDom.Length == 0) //new code //Can not find CIPD
            {
                ConcurrencyError = true;//=======
                return;
            }//=======
            if (calDom == null) //can not find CIPD
            {
                ConcurrencyError = true;
                return;
            }
            else //CIPD outside the loop => Concurrency Errors
                if (!Node_In_Loop(tempLoop, calDom[0], loop))
                {
                    ConcurrencyError = true;
                    return;
                }
            //if the CIPd existing inside the loop
            if (calDom.Length > 0)
            {
                int tail = calDom[0];
                searchNode[nSearchNode] = tail; //first node is CIPd
                nSearchNode++;

                for (int k = 0; k < Loop[tempLoop].Loop[loop].nEntry; k++)
                {
                    if (Loop[tempLoop].Loop[loop].Concurrency[k] != conK) continue;

                    find_Reach(conNet, tempLoop, loop, Loop[tempLoop].Loop[loop].Entry[k], tail, "CB");

                }
            }

            //결과 복제
            nSearchNode_B = nSearchNode;
            searchNode_B = new int[nSearchNode_B];
            for (int k = 0; k < nSearchNode_B; k++)
            {
                searchNode_B[k] = searchNode[k];
            }



            // 1. 정 Edge방향 D-PD찾기

            //Network 만들기
            Network[conNet] = Network[currentN];
            extent_Network(conNet, 0);

            copy_Loop(workLoop, tempLoop);

            prepare_Concurrency(conNet, tempLoop, loop);

            //해당 노드 검색
            nSearchNode = 0;
            searchNode = new int[Network[conNet].nNode];

            calDom = null;
            for (int k = 0; k < Loop[tempLoop].Loop[loop].nEntry; k++)
            {
                if (Loop[tempLoop].Loop[loop].Concurrency[k] != conK) continue;

                calDom = find_Intersection(Network[conNet].nNode, calDom, Network[conNet].Node[Loop[tempLoop].Loop[loop].Entry[k]].DomRev);
            }

            //Concurrency Error Check!!!!!!!!!!!
            if (calDom == null)
            {
                ConcurrencyError = true;
                return;
            }
            else if (!Node_In_Loop(tempLoop, calDom[0], loop))
            {
                ConcurrencyError = true;
                return;
            }

            if (calDom.Length > 0)
            {
                int tail = calDom[0];
                searchNode[nSearchNode] = tail;
                nSearchNode++;

                for (int k = 0; k < Loop[tempLoop].Loop[loop].nEntry; k++)
                {
                    if (Loop[tempLoop].Loop[loop].Concurrency[k] != conK) continue;

                    find_Reach(conNet, tempLoop, loop, Loop[tempLoop].Loop[loop].Entry[k], tail, "CF");

                }
            }

            //결과 복제
            nSearchNode_F = nSearchNode;
            searchNode_F = new int[nSearchNode_F];
            for (int k = 0; k < nSearchNode_F; k++)
            {
                searchNode_F[k] = searchNode[k];
            }


            //DDDDDDDDDDDDDDDDDDDDDDDDDDD
            nSearchNode = 0;
            searchNode = new int[Network[conNet].nNode];

            calDom = null;
            for (int k = 0; k < Loop[tempLoop].Loop[loop].nEntry; k++)
            {
                if (Loop[tempLoop].Loop[loop].Concurrency[k] != conK) continue;

                calDom = find_Intersection(Network[conNet].nNode, calDom, Network[conNet].Node[Loop[tempLoop].Loop[loop].Entry[k]].Dom);
            }

            if (calDom.Length > 0)
            {
                int header = calDom[calDom.Length - 1];
                searchNode[nSearchNode] = header;
                nSearchNode++;

                for (int k = 0; k < Loop[tempLoop].Loop[loop].nEntry; k++)
                {
                    if (Loop[tempLoop].Loop[loop].Concurrency[k] != conK) continue;

                    find_Reach(conNet, tempLoop, loop, header, Loop[tempLoop].Loop[loop].Entry[k], "CC");
                }
            }

            //결과 복제
            nSearchNode_P = nSearchNode;
            searchNode_P = new int[nSearchNode_P];
            for (int k = 0; k < nSearchNode_P; k++)
            {
                searchNode_P[k] = searchNode[k];
            }


        }

    }
}
