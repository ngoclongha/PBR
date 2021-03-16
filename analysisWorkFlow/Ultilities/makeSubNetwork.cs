using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer.Ultilities
{
    class makeSubNetwork
    {
        //노드 검색 결과 // Node Result
        public static int nSearchNode;
        public static int[] searchNode;
        //Irreducible Check
        public static int nSearchNode_P, nSearchNode_F, nSearchNode_B;
        public static int[] searchNode_P, searchNode_F, searchNode_B;

        public gProAnalyzer.GraphVariables.clsLoop.strLoop Loop;
        public gProAnalyzer.Ultilities.findIntersection interSect;
        public gProAnalyzer.Ultilities.findReachNode fndReachNode;
        public gProAnalyzer.Ultilities.clsFindNodeInfo fndNodeInfo;
        public gProAnalyzer.Ultilities.findIntersection fndIntersec;
        public gProAnalyzer.Ultilities.reduceGraph reduceG;

        public static gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink;
        public static int nImLink = 0;

        public static void Initialize_All()
        {
            //interSect = new gProAnalyzer.Ultilities.findIntersection();
            //fndReachNode = new gProAnalyzer.Ultilities.findReachNode();
            //fndNodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
            //fndIntersec = new gProAnalyzer.Ultilities.findIntersection();
            //reduceG = new gProAnalyzer.Ultilities.reduceGraph();
        }

        //workLoop - loop // can be used for SESE.
        public static void make_subNetwork(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop,
            int loop, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, string Type, int entryH) // Type = "SESE"일때는  workLoop, loop 가 SESE 지칭 
        {
            Initialize_All();

            // 포함 Node 찾기
            find_includeNode(ref graph, fromN, ref clsLoop, workLoop, loop, ref clsSESE, Type, entryH); //===> we will have "searchNode" here and "nsearchNode" 

            //Loop[workLoop].Loop내 포함된 모든 노드 찾아서
            if (Type == "CI" || Type == "II")
            {
                make_subNode(ref graph, fromN, toN, true, "START", true, "END");

                make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, true, clsLoop.Loop[workLoop].Loop[loop].Entry, true, clsLoop.Loop[workLoop].Loop[loop].Exit, entryH);
            }
            else if (Type == "CC") //Chi la them link gia, node gia thoi! => Moi thu deu dc lam o find_includeNode()
            {
                make_subNode(ref graph, fromN, toN, false, "", true, "XOR"); //Make subnetwork (Network[6]) => add node

                make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, false, null, true, clsLoop.Loop[workLoop].Loop[loop].Entry, entryH); //=: make subnetwork => add link
            }
            else if (Type == "ICF" || Type == "ICB" || Type == "ICC") //For irreducible loop (concurrent entry sets)
            {
                make_subNode(ref graph, fromN, toN, false, "", false, "");

                make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, false, null, false, null, entryH);
            }
            else if (Type == "IR")
            {
                make_subNode(ref graph, fromN, toN, true, "START", true, "END");

                make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, true, clsLoop.Loop[workLoop].Loop[loop].Entry, true, clsLoop.Loop[workLoop].Loop[loop].Exit, entryH);
            }

            //============= make Forward Flow network ================================
            else if (Type == "FF")
            {
                int num = 0;
                int[] imNode = new int[graph.Network[fromN].nNode];

                // --- real terminal 찾기
                for (int i = 0; i < nSearchNode; i++)
                {
                    int fromNode = searchNode[i];
                    if (graph.Network[fromN].Node[fromNode].Special == "T" || graph.Network[fromN].Node[fromNode].Special == "X")
                    {
                        imNode[num] = fromNode;
                        num++;
                    }
                }
                if (num <= 1)
                {
                    make_subNode(ref graph, fromN, toN, false, "", false, "");
                    make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, false, null, false, null, entryH);
                }
                else
                {
                    int[] dNode = new int[num];
                    for (int k = 0; k < num; k++) dNode[k] = imNode[k]; //filter all the appropriated link in this searchNode[] (fromNode-toNode) and storing in dNode[]

                    make_subNode(ref graph, fromN, toN, false, "", true, "XOR"); //sDummy = true => Add node VS; eDummy = true => Add node VE
                    make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, false, null, true, dNode, entryH);
                }
            }
            //============================= End FF ============================
            else if (Type == "BF")
            {
                int num = 0;
                int[] imNode = new int[graph.Network[fromN].nNode];
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nNode; j++)
                {
                    if (graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].Special == "T" || graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].Special == "B")
                    {
                        for (int k = 0; k < graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].nPost; k++)
                        {
                            int backNode = graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].Post[k];
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
                                imNode[num] = clsLoop.Loop[workLoop].Loop[loop].Node[j];
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
                    make_subNode(ref graph, fromN, toN, false, "", false, "");
                    make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, false, null, false, null, entryH);
                }
                else
                {
                    int[] dNode = new int[num];
                    for (int k = 0; k < num; k++) dNode[k] = imNode[k];
                    make_subNode(ref graph, fromN, toN, true, "XOR", false, "");
                    make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, true, dNode, false, null, entryH);
                }
            }
            else if (Type == "AC")
            {
                make_subNode(ref graph, fromN, toN, false, "", false, "");
                make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, false, null, false, null, entryH);
            }
            else if (Type == "SESE")
            {
                make_subNode(ref graph, fromN, toN, true, "START", true, "END");
                int[] sNode = new int[1];
                sNode[0] = clsSESE.SESE[workLoop].SESE[loop].Entry;
                int[] eNode = new int[1];
                eNode[0] = clsSESE.SESE[workLoop].SESE[loop].Exit;
                make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, loop, Type, true, sNode, true, eNode, entryH);
            }
            // Main purpose of this function is creating the Network[toN] (a subNetwork) for verification (instance flow) - just guess
            for (int i = 0; i < graph.Network[toN].nNode; i++)
            {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
            }
        }

        public static void find_includeNode(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int loop, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, string Type, int entryH) //SearchNode[] will be be clear when use this function.
        {
            nSearchNode = 0;
            searchNode = new int[graph.Network[fromN].nNode];
            int tempLoop = clsLoop.tempLoop;

            if (Type == "CI" || Type == "II") //Loop[workLoop].Loop[loop]내 포함된 모든 노드 찾아서
            {
                searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].header;
                nSearchNode++;
                find_LoopNode(ref clsLoop, workLoop, loop);
            }
            else if (Type == "CC") //concurrent entries
            {
                int[] calDom = null;
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[fromN].nNode, calDom, graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Entry[k]].Dom);
                }

                if (calDom.Length > 0)
                {
                    int CID = -1;
                    //pick another CID if current CID
                    for (int i = calDom.Length - 1; i > -1; i--)
                        if (gProAnalyzer.Ultilities.reduceGraph.check_CID_In_Loop(clsLoop, workLoop, loop, calDom[i]) == false)
                        {
                            CID = calDom[i];
                            break;
                        }
                    if (CID == -1)
                    {
                        MessageBox.Show("Find_IncludeNode of DFlow error", "Error");
                        return; //error
                    }

                    int header = CID;
                    searchNode[nSearchNode] = header;
                    nSearchNode++;

                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                    {
                        gProAnalyzer.Ultilities.findReachNode.find_Reach(ref graph, fromN, ref clsLoop, workLoop, loop, header,
                            clsLoop.Loop[workLoop].Loop[loop].Entry[k], Type, ref searchNode, ref nSearchNode); //find reach from HEADER to ENTRY[k]

                        searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].Entry[k];
                        nSearchNode++;
                    }
                }
            }
            else if (Type == "ICF")
            {
                // 일단 Concurrency Entry
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    if (clsLoop.Loop[tempLoop].Loop[loop].Concurrency[k] != entryH) continue;

                    searchNode[nSearchNode] = clsLoop.Loop[tempLoop].Loop[loop].Entry[k];
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
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++)
                {
                    if (clsLoop.Loop[tempLoop].Loop[loop].Concurrency[k] != entryH) continue;

                    searchNode[nSearchNode] = clsLoop.Loop[tempLoop].Loop[loop].Entry[k];
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
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++) //Duyet qua cac entries cua current Loop
                {
                    if (clsLoop.Loop[tempLoop].Loop[loop].Concurrency[k] != entryH) continue;

                    searchNode[nSearchNode] = clsLoop.Loop[tempLoop].Loop[loop].Entry[k];
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
                int rootDepth = graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].header].depth;
                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........
                //searchNode[] is used for storing the node of Forward Flow or Backward Flow (it will be erase when we find the node include "find_nodeInclude")
                searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].header;
                nSearchNode++;
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nNode; j++) //visit all node from the current loop "loop"
                {
                    if (graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].depth == rootDepth && graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].nPost > 0
                        && graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].nPre > 0)
                    {
                        searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].Node[j];
                        nSearchNode++;
                    }
                }
            }
            else if (Type == "BF") // BorwardFlow - Natural Loop
            {
                int rootDepth = graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].header].depth;

                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nNode; j++)
                {
                    if (graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].depth > rootDepth &&
                        graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].nPost > 0 && graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].nPre > 0)
                    {
                        searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].Node[j];
                        nSearchNode++;
                    }
                }
                searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].header;
                nSearchNode++;
            }
            else if (Type == "IR") // TotalFlow - Irreducible Loop
            {
                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........
                searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].header;
                nSearchNode++;
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nNode; j++)
                {
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[loop].Node[j];
                    nSearchNode++;
                }

            }
            else if (Type == "AC") // acyclic Flow 
            {
                //해당 Loop정보만 검색 ... reduce된 노드 자동 제거........

                for (int j = 0; j < graph.Network[fromN].nNode; j++)
                {
                    if (graph.Network[fromN].Node[j].done) continue;
                    searchNode[nSearchNode] = j;
                    nSearchNode++;
                }
            }
            else if (Type == "SESE") // acyclic Flow
            {
                for (int j = 0; j < clsSESE.SESE[workLoop].SESE[loop].nNode; j++)
                {
                    if (graph.Network[fromN].Node[clsSESE.SESE[workLoop].SESE[loop].Node[j]].done) continue;
                    searchNode[nSearchNode] = clsSESE.SESE[workLoop].SESE[loop].Node[j];
                    nSearchNode++;
                }
            }
        }

        public static void find_LoopNode(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int kLoop)
        {
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[kLoop].nNode; i++)
            {
                searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[kLoop].Node[i];
                nSearchNode++;
            }
            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[kLoop].nChild; k++)
            {
                find_LoopNode(ref clsLoop, workLoop, clsLoop.Loop[workLoop].Loop[kLoop].child[k]);
            }
        }

        //make_subNode(fromN, toN, false, "", true, "XOR");
        public static void make_subNode(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, bool sDummy, string sType, bool eDummy, string eType) //fromN ~ From Netwrok; toN ~ To Network
        {
            //Sub Network 구성
            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;

            graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
            graph.Network[toN].nNode = nSearchNode + addNum;
            graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
            for (int i = 0; i < nSearchNode; i++)
            {
                graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                graph.Network[toN].Node[i].orgNum = searchNode[i];
            }
            // Add details of new node (node VS) in BFlow (JUST for Natural Loop)
            if (sDummy)
            {
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = sType;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
            }
            //Add type of the new node (node VE) in FFlow (JUST for Natural Loop)
            if (eDummy)
            {
                graph.Network[toN].Node[graph.Network[toN].nNode - 1].Kind = eType;
                graph.Network[toN].Node[graph.Network[toN].nNode - 1].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - 1].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - 1].Special = "";
            }

            graph.Network[toN].header = graph.Network[toN].nNode - 2; //Network HEADER is VS
        }

        public static void make_subLink(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int loop, string Type, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int entryH)
        {
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;
            for (int i = 0; i < graph.Network[fromN].nLink; i++)
            {
                if (!check_addLink(ref graph, fromN, ref clsLoop, workLoop, loop, i, Type)) continue;

                int nFrom = -1;
                int nTo = -1;

                for (int k = 0; k < nSearchNode; k++)
                {
                    if (graph.Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (graph.Network[fromN].Link[i].toNode == searchNode[k])
                    {
                        if (Type == "FF" && graph.Network[fromN].Node[searchNode[k]].Special == "E") { }
                        else if (Type == "BF" && (graph.Network[fromN].Node[searchNode[k]].Special == "T" || graph.Network[fromN].Node[searchNode[k]].Special == "B")) { }
                        else
                        {
                            nTo = k;
                        }
                    }
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    imLink[imNum] = graph.Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
                else if (Type == "CC" && nFrom >= 0)
                {
                    if (graph.Network[fromN].Node[searchNode[nFrom]].Kind == "XOR" && graph.Network[fromN].Node[searchNode[nFrom]].nPost > 1)
                    {
                        imLink[imNum] = graph.Network[fromN].Link[i];
                        imLink[imNum].fromNode = nFrom;
                        imLink[imNum].toNode = graph.Network[toN].nNode - 1; ;
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

                    imLink[imNum].fromNode = graph.Network[toN].nNode - addNum;

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
                    imLink[imNum].toNode = graph.Network[toN].nNode - addNum;

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

            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        public static bool check_addLink(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int loop, int link, string Type)
        {
            bool bAdd = true;

            if (Type == "CI") // check_Irreducible
            {
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++)
                {
                    if (clsLoop.Loop[workLoop].Loop[loop].Entry[j] == graph.Network[fromN].Link[link].toNode) // 제거
                    {
                        bAdd = false;
                        break;
                    }
                }
            }
            else if (Type == "CC") // concurrency
            {
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++)
                {
                    if (clsLoop.Loop[workLoop].Loop[loop].Entry[j] == graph.Network[fromN].Link[link].fromNode) // 제거
                    {
                        bAdd = false;
                        break;
                    }
                }
            }
            return bAdd;
        }

        //=====================
        //Convert Loop into acylic graph by remove "Back-edges" for NL and "Synchronized-edges" for IL
        public static void make_AcyclicSubGraph(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int currLoop, string Type)
        {
            Initialize_All();

            if (Type == "NL")
            {
                if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry > 1) return;
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header;
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = header;
                nSearchNode++;

                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nNode; i++)
                {
                    //elimiate nested nodes (SESE or Loop)
                    if (graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Node[i]].done)
                    {
                        graph.Network[toN].nNode--;
                        continue;
                    }
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Node[i];
                    nSearchNode++;
                }

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = graph.Network[fromN].Node[header].Kind;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_HEADER";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = header;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3;

                //Add LINK (index should be original)
                int[] sNode = new int[1];
                sNode[0] = clsLoop.Loop[workLoop].Loop[currLoop].Entry[0];
                int[] eNode = new int[clsLoop.Loop[workLoop].Loop[currLoop].nExit];
                for (int i = 0; i < eNode.Length; i++)
                    eNode[i] = clsLoop.Loop[workLoop].Loop[currLoop].Exit[i];
                make_subLink_AC(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
            }

            if (Type == "IL")
            {
                if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry == 1) return;
                int CIPd = find_CIPd_IL(ref graph, fromN, ref clsLoop, workLoop, currLoop);
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = CIPd; //add CIPd
                nSearchNode++;
                if (CIPd != clsLoop.Loop[workLoop].Loop[currLoop].header)
                {
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].header; //add header if != CIPd
                    nSearchNode++;
                }

                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nNode; i++)
                {
                    //elimiate nested nodes (SESE or Loop)
                    if (graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Node[i]].done)
                    {
                        graph.Network[toN].nNode--;
                        continue;
                    }
                    if (clsLoop.Loop[workLoop].Loop[currLoop].Node[i] == CIPd) continue;
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Node[i];
                    nSearchNode++;
                }

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];

                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = graph.Network[fromN].Node[CIPd].Kind;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CIPd";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = CIPd;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Add Header
                graph.Network[toN].header = graph.Network[toN].nNode - 3;

                //Add LINK
                int[] sNode = new int[1];
                sNode[0] = CIPd;
                int[] eNode = new int[clsLoop.Loop[workLoop].Loop[currLoop].nExit];
                for (int i = 0; i < eNode.Length; i++)
                    eNode[i] = clsLoop.Loop[workLoop].Loop[currLoop].Exit[i];
                make_subLink_AC(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, CIPd);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
            }
        }

        public static void make_subLink_AC(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int currLoop, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int HEADER)
        {
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;
            int[] backEdges = new int[clsLoop.Loop[workLoop].Loop[currLoop].nNode];
            int nBackEdges = 0;
            for (int i = 0; i < graph.Network[fromN].nLink; i++)
            {
                int nFrom = -1;
                int nTo = -1;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (graph.Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (graph.Network[fromN].Link[i].toNode == searchNode[k]) nTo = k;
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    //recorded BackEdges index in imLink
                    if (searchNode[nTo] == HEADER)
                    {
                        backEdges[nBackEdges] = imNum;
                        nBackEdges++;
                    }
                    imLink[imNum] = graph.Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum++; //for HEADER or CIPd

            if (sDummy)
            {
                for (int i = 0; i < sNode.Length; i++)
                {
                    imLink[imNum].fromNode = graph.Network[toN].nNode - addNum;
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
                    imLink[imNum].toNode = graph.Network[toN].nNode - addNum;
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
                addNum--;
            }
            // connect Virtual_HEADER to END (MIGHT NEED TO REMOVE)
            /*
            if (true) {
                imLink[imNum].toNode = graph.Network[toN].nNode - addNum;
                addNum--;
                imLink[imNum].fromNode = graph.Network[toN].nNode - addNum; //Index of New HEADER
                imNum++;                
            }
             */
            //connect backEdges to Virtual_HEADER (in addNum index)
            if (true)
            {
                for (int i = 0; i < nBackEdges; i++)
                {
                    imLink[backEdges[i]].toNode = graph.Network[toN].nNode - addNum;
                }
            }

            //transfer LINK to new Graph
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        //Make subNetwork for each loop - NL & IL utilize the make_acyclicSubgraph()
        public static void make_CyclicSubGraph(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int currLoop, string Type, ref GraphVariables.clsSESE clsSESE)
        {
            if (Type == "NL")
            {
                if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry > 1) return;
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header;
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = header;
                nSearchNode++;

                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nNode; i++)
                {
                    //elimiate nested nodes (SESE or Loop)
                    if (graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Node[i]].done)
                    {
                        graph.Network[toN].nNode--;
                        continue;
                    }
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Node[i];
                    nSearchNode++;
                }

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "OR"; //this CIPd will synchronize all exit to the END
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CIPd";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Add LINK (index should be original)
                int[] sNode = new int[1];
                sNode[0] = clsLoop.Loop[workLoop].Loop[currLoop].Entry[0];
                int[] eNode = new int[clsLoop.Loop[workLoop].Loop[currLoop].nExit];
                for (int i = 0; i < eNode.Length; i++)
                    eNode[i] = clsLoop.Loop[workLoop].Loop[currLoop].Exit[i];
                make_subLink_Cyclic(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
            }

            if (Type == "IL")
            {
                gProAnalyzer.Ultilities.findConcurrencyEntriesIL.check_Concurrency(ref graph, fromN, graph.conNet, graph.subNet, clsLoop, workLoop, currLoop, ref clsSESE);
                int CID = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID;
                //we can access searchNode[] in fndConcurrencyIL to get DF => then find boundaries nodes of current IL
                int[] DFlow_IL_Flow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.DFlow_IL_Flow; ;
                int nNodeDFlow_IL = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.nNodeDFlow_IL;

                if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry == 1) return;

                //int CIPd = find_CIPd_IL(ref graph, fromN, ref clsLoop, workLoop, currLoop);
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = CID; //add CIPd
                nSearchNode++;

                for (int i = 0; i < nNodeDFlow_IL; i++)
                {
                    //elimiate nested nodes (SESE or Loop)
                    if (graph.Network[fromN].Node[DFlow_IL_Flow[i]].done)
                    {
                        graph.Network[toN].nNode--;
                        continue;
                    }
                    if (DFlow_IL_Flow[i] == CID) continue;
                    searchNode[nSearchNode] = DFlow_IL_Flow[i];
                    nSearchNode++;
                }

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];

                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "OR";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CIPd";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Add Header
                graph.Network[toN].header = graph.Network[toN].nNode - 3;

                //Add LINK
                int[] sNode = new int[1];
                sNode[0] = CID;
                int[] eNode = new int[clsLoop.Loop[workLoop].Loop[currLoop].nExit];
                for (int i = 0; i < eNode.Length; i++)
                    eNode[i] = clsLoop.Loop[workLoop].Loop[currLoop].Exit[i];
                make_subLink_Cyclic(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, CID);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
            }
        }

        public static void make_subLink_Cyclic(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int currLoop, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int HEADER)
        {
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;
            int[] backEdges = new int[clsLoop.Loop[workLoop].Loop[currLoop].nNode];
            int nBackEdges = 0;
            for (int i = 0; i < graph.Network[fromN].nLink; i++)
            {
                int nFrom = -1;
                int nTo = -1;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (graph.Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (graph.Network[fromN].Link[i].toNode == searchNode[k]) nTo = k;
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    //recorded BackEdges index in imLink
                    if (searchNode[nTo] == HEADER)
                    {
                        backEdges[nBackEdges] = imNum;
                        nBackEdges++;
                    }
                    imLink[imNum] = graph.Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum++; //for HEADER or CIPd

            if (sDummy)
            {
                for (int i = 0; i < sNode.Length; i++)
                {
                    imLink[imNum].fromNode = graph.Network[toN].nNode - addNum;
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
            if (eDummy) //connect Virtual_CIPd to END DM
            {
                imLink[imNum].toNode = graph.Network[toN].nNode - addNum;
                addNum--;
                imLink[imNum].fromNode = graph.Network[toN].nNode - addNum; //Index of virtual_CIPd
                imNum++;
            }

            // connect Virtual_CIPd to END (MIGHT NEED TO REMOVE)
            for (int i = 0; i < eNode.Length; i++)
            {
                imLink[imNum].toNode = graph.Network[toN].nNode - addNum;
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
            //addNum--;        

            //transfer LINK to new Graph
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        //This only for ePdFlow(ENTTi) => new concept (exclude the DFlow(ENTTi)
        public static void make_subLink_Cyclic_ePdFlow(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int currLoop, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int HEADER)
        {
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;
            int[] backEdges = new int[clsLoop.Loop[workLoop].Loop[currLoop].nNode];
            int nBackEdges = 0;
            for (int i = 0; i < graph.Network[fromN].nLink; i++)
            {
                int nFrom = -1;
                int nTo = -1;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (graph.Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (graph.Network[fromN].Link[i].toNode == searchNode[k]) nTo = k;
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    //recorded BackEdges index in imLink
                    if (searchNode[nTo] == HEADER)
                    {
                        backEdges[nBackEdges] = imNum;
                        nBackEdges++;
                    }
                    imLink[imNum] = graph.Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum++; //for HEADER or CIPd
            int indx_vCID = graph.Network[toN].nNode - 1;
            int indx_Start = graph.Network[toN].nNode - addNum;
            int indx_End = graph.Network[toN].nNode - addNum + 1;

            if (sDummy) //Connect START to V_CID
            {
                imLink[imNum].toNode = indx_vCID;
                imLink[imNum].fromNode = indx_Start;
                imNum++;
            }
            if (eDummy) //connect all eNode to END DM
            {
                //imLink[imNum].toNode = graph.Network[toN].nNode - addNum;
                //addNum--;
                //imLink[imNum].fromNode = graph.Network[toN].nNode - addNum; //Index of virtual_CIPd
                //imNum++;
                for (int i = 0; i < eNode.Length; i++)
                {
                    imLink[imNum].toNode = indx_End;
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

            // connectVirtual_CID to [sNode] for ePdFlow(ENTTi)
            for (int i = 0; i < sNode.Length; i++)
            {
                imLink[imNum].fromNode = indx_vCID;
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
            //addNum--;        

            //transfer LINK to new Graph
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        public static void make_subLink_Cyclic_eBwd(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int currLoop, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int HEADER, ref gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink_org, ref int imNum_org)
        {
            //We already have imLink for BWD
            //need convert fromN index to toN index
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;

            for (int i = 0; i < imNum_org; i++)
            {
                int fromNode = imLink_org[i].fromNode;
                int toNode = imLink_org[i].toNode;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (fromNode == searchNode[k])
                    {
                        imLink[imNum].fromNode = k; //CIPd
                    }
                    if (toNode == searchNode[k])
                    {
                        imLink[imNum].toNode = k; //CIPd
                    }
                }
                imNum++;
            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum++; //for V_S
            addNum++; //for V_E
            int indx_vE = graph.Network[toN].nNode - 1;
            int indx_vS = graph.Network[toN].nNode - 1 - 1;
            int indx_Start = graph.Network[toN].nNode - addNum;
            int indx_End = graph.Network[toN].nNode - addNum + 1;

            if (sDummy) //connect START_DM to vS
            {
                imLink[imNum].toNode = indx_vS;
                imLink[imNum].fromNode = indx_Start;
                imNum++;
            }
            if (eDummy) //connect vE to END_DM
            {
                imLink[imNum].toNode = indx_End;
                imLink[imNum].fromNode = indx_vE; //Index of virtual_CIPd
                imNum++;
            }

            //vS to all BS
            for (int i = 0; i < sNode.Length; i++)
            {
                imLink[imNum].fromNode = indx_vS;
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

            //all ENTT to vE
            for (int i = 0; i < eNode.Length; i++)
            {
                imLink[imNum].toNode = indx_vE;
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

            //transfer LINK to new Graph
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        //borrow the concept from eBwd_IL
        public static void make_subLink_Cyclic_NewIL(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
           int workLoop, int currLoop, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int HEADER)
        {
            //We already have imLink for BWD
            //need convert fromN index to toN index
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;
            int[] backEdges = new int[clsLoop.Loop[workLoop].Loop[currLoop].nNode];
            int nBackEdges = 0;
            for (int i = 0; i < graph.Network[fromN].nLink; i++)
            {
                int nFrom = -1;
                int nTo = -1;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (graph.Network[fromN].Link[i].fromNode == searchNode[k]) nFrom = k;
                    if (graph.Network[fromN].Link[i].toNode == searchNode[k]) nTo = k;
                }

                if (nFrom >= 0 && nTo >= 0 && nFrom != nTo)
                {
                    //recorded BackEdges index in imLink
                    if (searchNode[nTo] == HEADER)
                    {
                        backEdges[nBackEdges] = imNum;
                        nBackEdges++;
                    }
                    imLink[imNum] = graph.Network[fromN].Link[i];
                    imLink[imNum].fromNode = nFrom;
                    imLink[imNum].toNode = nTo;
                    imNum++;
                }
            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum++; //for V_S
            addNum++; //for V_E
            int indx_vE = graph.Network[toN].nNode - 1;
            int indx_vS = graph.Network[toN].nNode - 1 - 1;
            int indx_Start = graph.Network[toN].nNode - addNum;
            int indx_End = graph.Network[toN].nNode - addNum + 1;

            if (sDummy) //connect START_DM to vS
            {
                imLink[imNum].toNode = indx_vS;
                imLink[imNum].fromNode = indx_Start;
                imNum++;
            }
            if (eDummy) //connect vE to END_DM
            {
                imLink[imNum].toNode = indx_End;
                imLink[imNum].fromNode = indx_vE; //Index of virtual_CIPd
                imNum++;
            }

            //vS to all BS
            for (int i = 0; i < sNode.Length; i++)
            {
                imLink[imNum].fromNode = indx_vS;
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

            //all ENTT to vE
            for (int i = 0; i < eNode.Length; i++)
            {
                imLink[imNum].toNode = indx_vE;
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

            //transfer LINK to new Graph
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        //OLD This is only for Bwd_IL
        public static void make_subLink_Cyclic_2_CID(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int currLoop, bool sDummy, int[] sNode, bool eDummy, int[] eNode, int HEADER, ref gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink_org, ref int imNum_org)
        {
            //We already have imLink for BWD
            //need convert fromN index to toN index
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];
            int imNum = 0;

            for (int i = 0; i < imNum_org; i++)
            {
                int fromNode = imLink_org[i].fromNode;
                int toNode = imLink_org[i].toNode;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (fromNode == searchNode[k])
                    {
                        imLink[imNum].fromNode = k; //CIPd
                    }
                    if (toNode == searchNode[k])
                    {
                        imLink[imNum].toNode = k; //CIPd
                    }
                }
                imNum++;
            }

            int addNum = 0;
            if (sDummy) addNum++;
            if (eDummy) addNum++;
            addNum++; //for HEADER or CIPd

            // connect Virtual_CID to sNode (MIGHT NEED TO REMOVE)
            for (int i = 0; i < sNode.Length; i++)
            {
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (sNode[i] == searchNode[k])
                    {
                        imLink[imNum].toNode = k; //CIPd
                        break;
                    }
                }
                imLink[imNum].fromNode = graph.Network[toN].nNode - 1; //index of V_CID
                imNum++;
            }

            if (sDummy)
            {
                imLink[imNum].toNode = graph.Network[toN].nNode - 1;
                imLink[imNum].fromNode = graph.Network[toN].nNode - 3;
                imNum++;
            }
            if (eDummy) //connect Virtual_CIPd to END DM
            {
                imLink[imNum].toNode = graph.Network[toN].nNode - 2;
                for (int k = 0; k < nSearchNode; k++)
                {
                    if (eNode[0] == searchNode[k])
                    {
                        imLink[imNum].fromNode = k; //CIPd
                        break;
                    }
                }
                imNum++;
            }

            //transfer LINK to new Graph
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        public static void make_IL_Subgraph(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int currLoop,
            string Type, int currentCC_indx, ref GraphVariables.clsSESE clsSESE, int CID, ref int curr_CIPd, ref int[] list_nodes_return, ref int[] list_nodes_return2, ref int[] newENTTi, ref bool is_firstEnter)
        {
            //OLD DFlow_PdFlow
            #region
            if (Type == "DFlow_PdFlow")
            {
                //if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry > 1) return;
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header;
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = header;
                nSearchNode++;
                curr_CIPd = -1;

                int[] sNode = new int[1];
                int[] fwd_node = null; //dummy
                find_subNode_IL_Subgraph(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, clsLoop.reduceLoop, currLoop, "DFlow_PdFlow", currentCC_indx, ref clsSESE, CID, ref curr_CIPd, ref fwd_node, ref fwd_node, ref sNode, ref is_firstEnter); //fill up searchNode

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "XOR"; //this CIPd will synchronize all exit to the END
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CIPd";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Add LINK (index should be original) 
                sNode = new int[1];
                sNode[0] = CID;
                int[] eNode = new int[1];
                eNode[0] = curr_CIPd;
                make_subLink_Cyclic(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
            }
            #endregion

            if (Type == "IL_ENTTi") //make new IL with vS = AND and vE = XOR
            {
                //use newENTTi to find the sNode
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header;
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = header;
                nSearchNode++;
                curr_CIPd = -1;

                //find searchNode;
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nNode; i++)
                {
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Node[i];
                    nSearchNode++;
                }
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; i++)
                {
                    if (!gProAnalyzer.Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, clsLoop.Loop[workLoop].Loop[currLoop].Entry[i]))
                    {
                        searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Entry[i];
                        nSearchNode++;
                    }
                }

                //Working in new graph
                int addNum = 4; //add 4 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual XOR split of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "AND"; //this CID will START all BS & Exits of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_S";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "OR"; //this CID will START all BS & Exits of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_E";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Find sNode
                int[] sNode = newENTTi;
                //Find eNode
                int[] eNode = clsLoop.Loop[workLoop].Loop[currLoop].Exit;

                //make_subLink_Cyclic_eBwd(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header, ref imLink, ref nImLink);
                make_subLink_Cyclic_NewIL(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }

                //assgin new ENTTi for new iteration of IL verification
                newENTTi = eNode;
            }

            if (Type == "ePdFlow")
            {
                //if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry > 1) return;
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header;
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                searchNode[nSearchNode] = header;
                nSearchNode++;
                curr_CIPd = -1;

                int[] sNode = new int[1];
                int[] fwd_node = null; //dummy
                //change graph.reduceNet, graph.subNet to ==> fromN, toN
                find_subNode_IL_Subgraph(ref graph, fromN, toN, ref clsLoop, clsLoop.reduceLoop, currLoop, "ePdFlow", currentCC_indx, ref clsSESE, CID, ref curr_CIPd, ref fwd_node, ref fwd_node, ref sNode, ref is_firstEnter); //fill up searchNode

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "AND"; //this CIPd will synchronize all exit to the END
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CID";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Add LINK (index should be original) 
                int[] temp = new int[clsLoop.Loop[workLoop].Loop[currLoop].nEntry];
                int nTemp = 0;

                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; i++)
                {
                    //if (clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[i] == currentCC_indx)
                    {
                        temp[nTemp] = clsLoop.Loop[workLoop].Loop[currLoop].Entry[i];
                        nTemp++;

                    }
                }
                sNode = new int[nTemp];
                for (int i = 0; i < nTemp; i++) sNode[i] = temp[i];

                int[] eNode = new int[1];
                eNode[0] = curr_CIPd;
                //make_subLink_Cyclic(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);
                make_subLink_Cyclic_ePdFlow(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
                list_nodes_return = new int[nSearchNode];
                for (int i = 0; i < nSearchNode; i++)
                    list_nodes_return[i] = searchNode[i];
            }

            if (Type == "eFwd_IL")
            {
                int header = curr_CIPd; //??
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];

                int[] sNode = new int[1];
                int[] fwd_node = null; //dummy
                find_subNode_IL_Subgraph(ref graph, fromN, toN, ref clsLoop, clsLoop.reduceLoop, currLoop, "eFwd_IL", currentCC_indx, ref clsSESE, CID, ref curr_CIPd, ref fwd_node, ref fwd_node, ref sNode, ref is_firstEnter); //fill up searchNode

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "XOR"; //this CIPd will be the source of all BACKWARD SPLITs
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CIPd";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Add LINK (index should be original) //Find sNode and find eNode
                sNode = new int[1];
                sNode[0] = curr_CIPd;
                int[] temp_eNode = new int[clsLoop.Loop[workLoop].Loop[currLoop].nExit];
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currLoop].nExit; i++)
                    temp_eNode[i] = clsLoop.Loop[workLoop].Loop[currLoop].Exit[i];
                int[] BS_node = find_BackwardSplits_ENTT_IL(ref graph, fromN, searchNode, nSearchNode);
                int[] total = new int[temp_eNode.Length + BS_node.Length];
                int nTotal = 0;
                for (int i = 0; i < temp_eNode.Length; i++)
                {
                    total[nTotal] = temp_eNode[i];
                    nTotal++;
                }
                for (int i = 0; i < BS_node.Length; i++)
                {
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(total, nTotal, BS_node[i])) continue;
                    total[nTotal] = BS_node[i];
                    nTotal++;
                }
                int[] eNode = new int[nTotal];
                for (int i = 0; i < nTotal; i++) eNode[i] = total[i];

                make_subLink_Cyclic(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }

                list_nodes_return = new int[nSearchNode];
                for (int i = 0; i < nSearchNode; i++)
                    list_nodes_return[i] = searchNode[i];
            }

            if (Type == "eBwd_IL") //find new ENTTj also
            {
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header; //??
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];

                int[] sNode = new int[1];

                //int[] fwd_node = gProAnalyzer.Ultilities.checkGraph.add_two_array(list_nodes_return, list_nodes_return.Length, list_nodes_return2, list_nodes_return2.Length);
                int[] PdF_node = list_nodes_return;
                int[] fwd_node = list_nodes_return2;
                find_subNode_IL_Subgraph(ref graph, fromN, toN, ref clsLoop, clsLoop.reduceLoop, currLoop, "eBwd_IL", currentCC_indx, ref clsSESE, CID, ref curr_CIPd, ref PdF_node, ref fwd_node, ref sNode, ref is_firstEnter); //fill up searchNode

                //Working in new graph
                int addNum = 4; //add 4 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual XOR split of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "XOR"; //this CID will START all BS & Exits of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_S";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "OR"; //this CID will START all BS & Exits of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_E";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Find eNode
                int[] temp_ENTT_node = find_BackwardSplits_ENTT_IL(ref graph, fromN, searchNode, nSearchNode);
                int[] tempArr = new int[temp_ENTT_node.Length];
                int nTemArr = 0;
                for (int i = 0; i < temp_ENTT_node.Length; i++)
                {
                    if (!gProAnalyzer.Ultilities.checkGraph.Node_In_Set(clsLoop.Loop[workLoop].Loop[currLoop].Entry, clsLoop.Loop[workLoop].Loop[currLoop].nEntry, temp_ENTT_node[i])) continue;
                    tempArr[nTemArr] = temp_ENTT_node[i];
                    nTemArr++;
                }
                int[] ENTT_node = new int[nTemArr];
                for (int i = 0; i < nTemArr; i++) ENTT_node[i] = tempArr[i];
                int[] total = new int[ENTT_node.Length];
                int nTotal = 0;
                for (int i = 0; i < ENTT_node.Length; i++)
                {
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(total, nTotal, ENTT_node[i])) continue;
                    total[nTotal] = ENTT_node[i];
                    nTotal++;
                }
                int[] eNode = new int[nTotal];
                for (int i = 0; i < nTotal; i++) eNode[i] = total[i];

                make_subLink_Cyclic_eBwd(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header, ref imLink, ref nImLink);

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }

                //assgin new ENTTi for new iteration of IL verification
                newENTTi = eNode;
            }

            //OLD eBwd_PdFlow
            #region
            if (Type == "eBwd_PdFlow")
            {
                int header = clsLoop.Loop[workLoop].Loop[currLoop].header; //??
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];

                int[] sNode = new int[1];
                int[] fwd_node = list_nodes_return;
                find_subNode_IL_Subgraph(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, clsLoop.reduceLoop, currLoop, "eBwd_PdFlow", currentCC_indx, ref clsSESE, CID, ref curr_CIPd, ref fwd_node, ref fwd_node, ref sNode, ref is_firstEnter); //fill up searchNode

                //Working in new graph
                int addNum = 3; //add 3 nodes to make acyclic graph of Loop                
                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode + addNum;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                //Make subNode ==
                for (int i = 0; i < nSearchNode; i++)
                {
                    graph.Network[toN].Node[i] = graph.Network[fromN].Node[searchNode[i]];
                    graph.Network[toN].Node[i].orgNum = searchNode[i];
                }
                // Add details of new node (node VS - Virtual Start)              
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "START";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add type of the new node (node VE - Virtual End)               
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "END";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "DM";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                addNum--;
                //Add Virtual Header of NL
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Kind = "XOR"; //this CID will START all BS & Exits of eBwd
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Name = "V_CID";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].parentNum = -1;
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].orgNum = -1;// "D";
                graph.Network[toN].Node[graph.Network[toN].nNode - addNum].Special = "";
                //Assign header of NETWORK
                graph.Network[toN].header = graph.Network[toN].nNode - 3; //node START as "DM"

                //Add LINK (index should be original)
                //================================================
                //sNode[0] = curr_CIPd; //we already have sNode (list of Backward splits)
                int[] eNode = new int[1];
                eNode[0] = curr_CIPd;
                make_subLink_Cyclic_2_CID(ref graph, fromN, toN, ref clsLoop, workLoop, currLoop, true, sNode, true, eNode, header, ref imLink, ref nImLink); //must remove link between bs!!!!!!!!!!!!!!!!!!!!!!

                //Update node Info
                for (int i = 0; i < graph.Network[toN].nNode; i++)
                {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
                }
            }
            #endregion //eBwd_PdFlow
        }

        public static void find_subNode_IL_Subgraph(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop,
            int currLoop, string Type, int currentCC_indx, ref GraphVariables.clsSESE clsSESE, int CID, ref int CIPd, ref int[] PdF_node, ref int[] fwd_node, ref int[] sNode, ref bool is_firstEnter)
        {
            if (Type == "ePdFlow")
            {
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];

                //Using DFS to find DFlow()
                int[] getPath = new int[graph.Network[fromN].nNode];
                int nGetPath = 0;
                bool[] Mark = new bool[graph.Network[fromN].nNode];
                int[][] adjList = null;
                bool flag = true;
                gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, fromN, ref adjList);
                //gProAnalyzer.Ultilities.checkGraph.DFS_Fwd_Bwd(adjList, ref Mark, gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID, clsLoop.Loop[workLoop].Loop[currLoop].Entry, ref getPath, ref nGetPath);
                gProAnalyzer.Ultilities.checkGraph.DFS_Recursive_GetPathOnly_ePdFlow(adjList, ref Mark, CID,
                    clsLoop.Loop[workLoop].Loop[currLoop].Entry, ref getPath, ref nGetPath, ref searchNode, ref nSearchNode);

                int[] calDomRev = null; //find PdFlow(currentCC_indx)

                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++)
                {
                    //if (clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[k] != currentCC_indx) continue; //NO NEED TO CHECK => BECAUSE ONLY ONE ENTTi IS TRIGGERED only
                    calDomRev = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[fromN].nNode, calDomRev, graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Entry[k]].DomRev);
                }

                if (calDomRev.Length > 0)
                {
                    CIPd = -1;
                    int header = calDomRev[0];
                    CIPd = header;

                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                    {
                        //if (clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[k] != currentCC_indx) continue; //NO NEED TO CHECK => BECAUSE ONLY ONE ENTTi IS TRIGGERED only
                        gProAnalyzer.Ultilities.findReachNode.find_Reach_2(ref graph, fromN, ref clsLoop, workLoop, currLoop,
                            clsLoop.Loop[workLoop].Loop[currLoop].Entry[k], CIPd, "", ref searchNode, ref nSearchNode); //remove redundancy in searchNode
                    }

                    //finally, add the CIPd into SearchNode
                    if (!Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, CIPd))
                    {
                        searchNode[nSearchNode] = CIPd;
                        nSearchNode++;
                    }
                }

                //Remove all nodes in DFlow(ENTT) in searchNode[]====
                int[] tempArr = new int[nSearchNode];
                int nTempArr = 0;
                for (int i = 0; i < nSearchNode; i++)
                {
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, searchNode[i], currLoop))
                    {
                        tempArr[nTempArr] = searchNode[i];
                        nTempArr++;
                    }
                }
                searchNode = new int[nTempArr];
                nSearchNode = nTempArr;
                for (int i = 0; i < nSearchNode; i++) searchNode[i] = tempArr[i];
                //===================================================
            }

            if (Type == "eFwd_IL")
            {
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];
                //Find nodes from CIPd to all exits only
                //searchNode[nSearchNode] = CIPd;
                //nSearchNode++;


                int[] getPath = new int[graph.Network[fromN].nNode];
                int nGetPath = 0;
                bool[] Mark = new bool[graph.Network[fromN].nNode];
                int[][] adjList = null;
                gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, fromN, ref adjList);
                //gProAnalyzer.Ultilities.checkGraph.DFS_Fwd_Bwd(adjList, ref Mark, CIPd, clsLoop.Loop[workLoop].Loop[currLoop].Exit, ref getPath, ref nGetPath);

                gProAnalyzer.Ultilities.checkGraph.DFS_Recursive_GetPathOnly(adjList, ref Mark, CIPd,
                    clsLoop.Loop[workLoop].Loop[currLoop].Exit, ref getPath, ref nGetPath, ref searchNode, ref nSearchNode); //searchNode updated                

                //for (int k = 0; k < nGetPath; k++)
                //{
                //searchNode[nSearchNode] = getPath[k];
                //nSearchNode++;
                //}


                //gProAnalyzer.Ultilities.findReachNode.find_Reach_2(ref graph, fromN, ref clsLoop, workLoop, currLoop, CIPd, clsLoop.Loop[workLoop].Loop[currLoop].Exit[k], "", ref searchNode, ref nSearchNode); //remove redundancy in searchNode                    
                //finally, add the CIPd into SearchNode
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nExit; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                {
                    if (Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, clsLoop.Loop[workLoop].Loop[currLoop].Exit[k]) == false)
                    {
                        searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Exit[k];
                        nSearchNode++;
                    }
                }
            }

            if (Type == "eBwd_IL") //left_over structure of IL
            {
                int[] Fwd = fwd_node;
                int nFwd = fwd_node.Length;
                int[] bs = new int[nFwd];
                int nBs = 0;
                int dummy_n = 0;

                int[] PdF_fwd_node = gProAnalyzer.Ultilities.checkGraph.add_two_array(PdF_node, PdF_node.Length, fwd_node, fwd_node.Length, ref dummy_n);

                //find all edges not in Fwd
                imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2]; // link in bwd
                nImLink = 0;
                gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink_2 = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2]; //link in fwd
                int nImLink_2 = 0;
                for (int i = 0; i < graph.Network[fromN].nLink; i++)
                {
                    int fromNode = graph.Network[fromN].Link[i].fromNode;
                    int toNode = graph.Network[fromN].Link[i].toNode;
                    if (!(Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, fromNode, currLoop) && Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, toNode, currLoop)))
                        continue; //edges must be in IL

                    if (!(Ultilities.checkGraph.Node_In_Set(PdF_node, PdF_node.Length, fromNode) && Ultilities.checkGraph.Node_In_Set(PdF_node, PdF_node.Length, toNode))) //find all link not in pdf + fwd
                    {
                        if (!(Ultilities.checkGraph.Node_In_Set(Fwd, nFwd, fromNode) && Ultilities.checkGraph.Node_In_Set(Fwd, nFwd, toNode))) //find all link not in pdf + fwd
                        {
                            imLink[nImLink] = graph.Network[fromN].Link[i];
                            nImLink++;
                        }
                    }
                    if (Ultilities.checkGraph.Node_In_Set(PdF_fwd_node, PdF_fwd_node.Length, fromNode) && Ultilities.checkGraph.Node_In_Set(PdF_fwd_node, PdF_fwd_node.Length, toNode)) //find all link in fwd
                    {
                        imLink_2[nImLink_2] = graph.Network[fromN].Link[i];
                        nImLink_2++;

                    }
                }

                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];//all node in bwd by eliminate the edges not in ImLink[] above

                for (int i = 0; i < graph.Network[fromN].nNode; i++)
                {
                    for (int j = 0; j < nImLink; j++)
                    {
                        if (i == imLink[j].fromNode || i == imLink[j].toNode)
                        {
                            searchNode[nSearchNode] = i;
                            nSearchNode++;
                            break;
                        }
                    }
                }

                //find bs_ => nodes having no incoming edges from bwd_node
                for (int i = 0; i < nSearchNode; i++)
                {
                    if (graph.Network[fromN].Node[searchNode[i]].nPost < 2) continue; //bs_ must be a split
                    for (int j = 0; j < nImLink_2; j++)
                    {
                        if (imLink_2[j].toNode == searchNode[i])
                        {
                            bs[nBs] = searchNode[i];
                            nBs++;
                        }
                    }
                }
                sNode = new int[nBs];
                for (int i = 0; i < nBs; i++)
                    sNode[i] = bs[i];

                //find all edges in Bwd_IL = IL - Fwd_IL
                //find all nodes in Bwd_IL
                //find all BS (Bs&Ex)
            }

            //OLD eBwd_PdFlow
            #region
            if (Type == "eBwd_PdFlow") //left_over structure of IL
            {
                int[] Fwd = fwd_node;
                int nFwd = fwd_node.Length;
                int[] bs = new int[nFwd];
                int nBs = 0;

                //find all edges not in Fwd
                imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2]; // link in bwd
                nImLink = 0;
                gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink_2 = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2]; //link in fwd
                int nImLink_2 = 0;
                for (int i = 0; i < graph.Network[fromN].nLink; i++)
                {
                    int fromNode = graph.Network[fromN].Link[i].fromNode;
                    int toNode = graph.Network[fromN].Link[i].toNode;
                    if (!(Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, fromNode, currLoop) && Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, toNode, currLoop)))
                        continue; //edges must be in IL
                    if (!(Ultilities.checkGraph.Node_In_Set(Fwd, nFwd, fromNode) && Ultilities.checkGraph.Node_In_Set(Fwd, nFwd, toNode))) //find all link not in fwd
                    {
                        imLink[nImLink] = graph.Network[fromN].Link[i];
                        nImLink++;
                    }
                    if (Ultilities.checkGraph.Node_In_Set(Fwd, nFwd, fromNode) && Ultilities.checkGraph.Node_In_Set(Fwd, nFwd, toNode)) //find all link in fwd
                    {
                        imLink_2[nImLink_2] = graph.Network[fromN].Link[i];
                        nImLink_2++;
                    }
                }

                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];//all node in bwd
                for (int i = 0; i < graph.Network[fromN].nNode; i++)
                {
                    for (int j = 0; j < nImLink; j++)
                    {
                        if (i == imLink[j].fromNode || i == imLink[j].toNode)
                        {
                            searchNode[nSearchNode] = i;
                            nSearchNode++;
                            break;
                        }
                    }
                }

                //find bs_ => nodes having no incoming edges from bwd_node
                for (int i = 0; i < nSearchNode; i++)
                {
                    if (graph.Network[fromN].Node[searchNode[i]].nPost < 2) continue; //bs_ must be a split
                    for (int j = 0; j < nImLink_2; j++)
                    {
                        if (imLink_2[j].toNode == searchNode[i])
                        {
                            bs[nBs] = searchNode[i];
                            nBs++;
                        }
                    }
                }
                sNode = new int[nBs];
                for (int i = 0; i < nBs; i++)
                    sNode[i] = bs[i];

                //find all edges in Bwd_IL = IL - Fwd_IL
                //find all nodes in Bwd_IL
                //find all BS (Bs&Ex)
            }
            #endregion

            //Unused - backup ==============
            #region
            if (Type == "DFlow_PdFlow")
            {
                nSearchNode = 0;
                searchNode = new int[graph.Network[fromN].nNode];

                //Using DFS to find DFlow()
                int[] getPath = new int[graph.Network[fromN].nNode];
                int nGetPath = 0;
                bool[] Mark = new bool[graph.Network[fromN].nNode];
                int[][] adjList = null;
                bool flag = true;
                gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, fromN, ref adjList);
                //gProAnalyzer.Ultilities.checkGraph.DFS_Fwd_Bwd(adjList, ref Mark, gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID, clsLoop.Loop[workLoop].Loop[currLoop].Entry, ref getPath, ref nGetPath);
                gProAnalyzer.Ultilities.checkGraph.DFS_Recursive_GetPathOnly(adjList, ref Mark, gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID,
                    clsLoop.Loop[workLoop].Loop[currLoop].Entry, ref getPath, ref nGetPath, ref searchNode, ref nSearchNode);

                int[] calDomRev = null; //find PdFlow(currentCC_indx)
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++)
                {
                    if (clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[k] != currentCC_indx) continue;
                    calDomRev = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[fromN].nNode, calDomRev, graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Entry[k]].DomRev);
                }

                if (calDomRev.Length > 0)
                {
                    CIPd = -1;
                    int header = calDomRev[0];
                    CIPd = header;

                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++) //From here "searchNode[]" will store the path from sNode to Entries => We have path
                    {
                        //gProAnalyzer.Ultilities.findReachNode.find_Reach(ref graph, fromN, ref clsLoop, workLoop, currLoop, header,
                        //    clsLoop.Loop[workLoop].Loop[currLoop].Entry[k], Type, ref searchNode, ref nSearchNode); //find reach from HEADER to ENTRY[k]

                        if (clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[k] != currentCC_indx) continue;
                        gProAnalyzer.Ultilities.findReachNode.find_Reach_2(ref graph, fromN, ref clsLoop, workLoop, currLoop,
                            clsLoop.Loop[workLoop].Loop[currLoop].Entry[k], CIPd, "", ref searchNode, ref nSearchNode); //remove redundancy in searchNode
                    }
                    //finally, add the CIPd into SearchNode
                    if (!Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, CIPd))
                    {
                        searchNode[nSearchNode] = CIPd;
                        nSearchNode++;
                    }
                }
            }
            #endregion
        }

        public static int find_CIPd_IL(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsLoop clsLoop, int workLoop, int currLoop)
        {
            if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry == 1) return -1;
            int CIPd = -1;
            int[] calDomRev = null;

            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++)
            {
                calDomRev = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDomRev, graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Entry[k]].DomRev);
            }

            if (calDomRev.Length > 0)
            {
                int header = calDomRev[0];
                CIPd = header;
            }
            else return -1;
            return CIPd;
        }

        //new way to find BS in IL given [searchNode] <<== Set of node in Fwd(ENTTi)
        public static int[] find_BackwardSplits_ENTT_IL(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] searchNode, int nSearchNode)
        {
            int[] add_BS = new int[nSearchNode];
            int nAdd = 0;
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                for (int j = 0; j < nSearchNode; j++)
                {
                    if (graph.Network[currentN].Link[i].fromNode == searchNode[j])
                        if (!gProAnalyzer.Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, graph.Network[currentN].Link[i].toNode) &&
                            gProAnalyzer.Ultilities.checkGraph.Node_In_Set(add_BS, nAdd, searchNode[j]) == false)
                        {

                            add_BS[nAdd] = searchNode[j];
                            nAdd++;
                        }
                }
            }
            int[] new_BS = new int[nAdd];
            for (int i = 0; i < nAdd; i++) new_BS[i] = add_BS[i];
            return new_BS;
        }
        //new way to find newENTT in IL given [searchNode]
        public static int[] find_newENTT_IL(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] searchNode, int nSearchNode)
        {
            int[] add_BS = new int[nSearchNode];
            int nAdd = 0;
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                for (int j = 0; j < nSearchNode; j++)
                {
                    if (graph.Network[currentN].Link[i].toNode == searchNode[j])
                        if (!gProAnalyzer.Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, graph.Network[currentN].Link[i].toNode) &&
                            gProAnalyzer.Ultilities.checkGraph.Node_In_Set(add_BS, nAdd, searchNode[j]) == false)
                        {

                            add_BS[nAdd] = searchNode[j];
                            nAdd++;
                        }
                }
            }
            int[] new_BS = new int[nAdd];
            for (int i = 0; i < nAdd; i++) new_BS[i] = add_BS[i];
            return new_BS;
        }


        //=========================
        public static void make_subGraph_DFlow(ref gProAnalyzer.GraphVariables.clsGraph graph, int fromN, int toN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int curLoop, int[] DFlow, int nDFlow, int[] boundaryNodes, int orgCID)
        {
            //fill out searchNode of makeSubNetwork (fromN network)
            searchNode = new int[nDFlow];
            nSearchNode = 0;
            int newSubgraph_Index = -1;
            for (int i = 0; i < nDFlow; i++)
            {
                searchNode[i] = DFlow[i]; //DFlow contain original index of fromN.. (Loops reduced if any)
                nSearchNode++;
                if (DFlow[i] == orgCID)
                    newSubgraph_Index = i;
            }

            //make subNetwork (like SESE)
            make_subNode(ref graph, fromN, toN, false, "", true, "OR"); //only add 1 node to subNetwork
            graph.Network[toN].header = newSubgraph_Index;
            make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, curLoop, "", false, null, false, null, -1);
            //One more thing. => connect all boundary nodes in DFlow to virtual new "OR" join
            makeSubLink_Untangling(ref graph, fromN, toN, clsLoop, workLoop, curLoop, boundaryNodes, orgCID); //multiple end subgraph is allowed

            for (int i = 0; i < graph.Network[toN].nNode; i++)
            {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
            }
        }

        public static void makeSubLink_Untangling(ref GraphVariables.clsGraph graph, int fromN, int toN, GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, int[] boundaryNodes, int orgCID)
        {
            gProAnalyzer.Ultilities.checkGraph checkG = new gProAnalyzer.Ultilities.checkGraph();

            //(Type) for untangling IL => Connect all boundary node (split) except CID to new OR join
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];


            //remove out-going edges of all loop entries (only remove the curLoop)
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                int orgEntry = graph.Network[toN].Node[graph.Network[toN].Link[i].fromNode].orgNum;
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, orgEntry, curLoop) == false) continue; //not remove other loops
                if (gProAnalyzer.Ultilities.checkGraph.isLoopEntries(ref clsLoop, workLoop, orgEntry))
                {
                    graph.Network[toN].Link[i].toNode = graph.Network[toN].Link[i].fromNode;
                }
            }


            int imNum = 0;
            //temporary transfer to imLink for import additional link form boundary nodes to OR-join
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                if (graph.Network[toN].Link[i].fromNode == graph.Network[toN].Link[i].toNode) continue; //only get valid link
                imLink[imNum] = graph.Network[toN].Link[i];
                imNum++;
            }

            int to_OR = graph.Network[toN].nNode - 1; //OR-join index
            for (int i = 0; i < graph.Network[toN].nNode; i++)
            {
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(boundaryNodes, boundaryNodes.Length, graph.Network[toN].Node[i].orgNum) && graph.Network[toN].Node[i].orgNum != orgCID)
                {
                    imLink[imNum].toNode = to_OR;
                    imNum++;
                }
            }
            //transfer back to Link_ToN
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

        public static void combine_subGraph_Untangling(ref GraphVariables.clsGraph graph, int orgNet, int untangleNet, int conNet, int fromN_subNet, int toN, GraphVariables.clsLoop clsLoop,
            int workLoop, int curLoop, int[] expand_boundaryNodes, int[] boundaryNode, int orgCID_index)
        {
            //orgNet: original network contained empty space
            //conNet: original network after reducing loops
            //fromN_SubNet: subgraph from CID to IL (loops reduced)
            //toN: branchNet - contain each branch of untangling IL (NEW GRAPH)

            //create 1 XOR-split for exclusive brach of Untangling. (It will replace CID(entries) of original model) - one of boundary node

            //for each concurrencyInst[] //create Untangling_subgraph from CID to current IL (expand loop in DFlow if any)
            bool firstRun = true;
            for (int i = 0; i <= clsLoop.Loop[workLoop].Loop[curLoop].nConcurrency; i++)
            {
                //make_subNode
                searchNode = clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[i]; //must be index of orgNetwork/finalNet
                nSearchNode = clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[i];

                if (searchNode == null || nSearchNode == 0) continue;

                //expand searchNode if it have loops
                expand_SearchNode(graph, conNet, clsLoop, workLoop, curLoop, ref searchNode, ref nSearchNode);

                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                for (int j = 0; j < nSearchNode; j++)
                {
                    graph.Network[toN].Node[j] = graph.Network[untangleNet].Node[searchNode[j]]; //toN_header in searchNode[0];
                    graph.Network[toN].Node[j].orgNum = searchNode[j];
                }
                graph.Network[toN].header = 0; //first index of toN graph.

                //make_subLink
                make_subLink(ref graph, orgNet, toN, ref clsLoop, workLoop, curLoop, "", false, null, false, null, -1); // IT WORK?? => just replace orgNet (NO NEED FIX)
                //remove Link to entries which are not initiated yet.
                adjust_SubLink_mergeUntangling(ref graph, orgNet, toN, clsLoop, workLoop, curLoop, i, 0);

                //merger Untangling_subgraph into original model (Merge toN(expanded) to orgNet (having empty space already)                    
                merge_Untangling(ref graph, untangleNet, toN, orgCID_index, boundaryNode, firstRun); //-> connect XOR-split to it CID, connect its boundaryNodes to original boundary node (also done)
                firstRun = false;

                //remember to marking node[i].orgNum of original model.
            }

            //Merge exclusive entries also
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nEntry; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[curLoop].Concurrency[i] != 0) continue; //only get exclusive entries

                //make_subNode
                searchNode = clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[i]; //must be index of orgNetwork/finalNet
                nSearchNode = clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[i];

                if (searchNode == null || nSearchNode == 0) continue;

                //expand searchNode if it have loops + IL itself
                expand_SearchNode(graph, conNet, clsLoop, workLoop, curLoop, ref searchNode, ref nSearchNode);

                graph.Network[toN] = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
                graph.Network[toN].nNode = nSearchNode;
                graph.Network[toN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[toN].nNode];
                for (int j = 0; j < nSearchNode; j++)
                {
                    graph.Network[toN].Node[j] = graph.Network[untangleNet].Node[searchNode[j]]; //toN_header in searchNode[0];
                    graph.Network[toN].Node[j].orgNum = searchNode[j];
                }
                graph.Network[toN].header = 0; //first index of toN graph.

                //make_subLink
                make_subLink(ref graph, orgNet, toN, ref clsLoop, workLoop, curLoop, "", false, null, false, null, -1); // IT WORK?? => just replace orgNet (NO NEED FIX)
                //remove Link to entries which are not initiated yet.
                adjust_SubLink_mergeUntangling(ref graph, orgNet, toN, clsLoop, workLoop, curLoop, 0, clsLoop.Loop[workLoop].Loop[curLoop].Entry[i]);

                /*
                frmAnalysisNetwork frmAnl;
                frmAnl = new frmAnalysisNetwork();
                GraphVariables.clsSESE clsSESE = new GraphVariables.clsSESE();
                frmAnl.displayProcessModel(ref graph, toN, ref clsLoop, -1, ref clsSESE, -1);
                frmAnl.Show();
                return;
                */

                //merger Untangling_subgraph into original model (Merge toN(expanded) to orgNet (having empty space already)                    
                merge_Untangling(ref graph, untangleNet, toN, orgCID_index, boundaryNode, firstRun); //-> connect XOR-split to it CID, connect its boundaryNodes to original boundary node (also done)
                firstRun = false;

                //remember to marking node[i].orgNum of original model.
            }

            //finish!!
        }

        public static void merge_Untangling(ref GraphVariables.clsGraph graph, int subNet, int branchNet, int orgCID, int[] expand_boundaryNodes, bool initiate_XOR)
        {
            if (initiate_XOR)
            {
                graph.Network[subNet].Node[orgCID].Kind = "XOR";
                graph.Network[subNet].Node[orgCID].Name = "New_CID";
                graph.Network[subNet].Node[orgCID].orgNum = orgCID;
                graph.Network[subNet].Node[orgCID].Type_I = "";
                graph.Network[subNet].Node[orgCID].Type_II = "";
                graph.Network[subNet].Node[orgCID].done = false;
            }

            //merger node
            //merge link
            //gProAnalyzer.Ultilities.extendGraph extendG = new gProAnalyzer.Ultilities.extendGraph();
            int nNode_subNet = graph.Network[subNet].nNode;
            int nLink_subNet = graph.Network[subNet].nLink;
            int addNode = graph.Network[branchNet].nNode;
            int addLink = graph.Network[branchNet].nLink + 1 + expand_boundaryNodes.Length; //+1 for orgCID - branchNet_header
            int branchNet_header = nNode_subNet;
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, subNet, addNode, addLink);

            //add nodes from branchNet to subNet
            for (int i = nNode_subNet; i < graph.Network[subNet].nNode; i++)
            {
                graph.Network[subNet].Node[i].Kind = graph.Network[branchNet].Node[i - nNode_subNet].Kind;
                graph.Network[subNet].Node[i].Name = graph.Network[branchNet].Node[i - nNode_subNet].Name;
                graph.Network[subNet].Node[i].orgNum = graph.Network[branchNet].Node[i - nNode_subNet].orgNum;
                graph.Network[subNet].Node[i].Type_I = graph.Network[branchNet].Node[i - nNode_subNet].Type_I;
                graph.Network[subNet].Node[i].Type_II = graph.Network[branchNet].Node[i - nNode_subNet].Type_II;
                graph.Network[subNet].Node[i].header = graph.Network[branchNet].Node[i - nNode_subNet].header;
                graph.Network[subNet].Node[i].headerOfLoop = graph.Network[branchNet].Node[i - nNode_subNet].headerOfLoop;
                graph.Network[subNet].Node[i].done = false;
            }

            //add links from branchNet to subNet ===>> CHECK AGAIN
            for (int i = nLink_subNet; i < (graph.Network[subNet].nLink - 1 - expand_boundaryNodes.Length); i++)
            {
                graph.Network[subNet].Link[i].fromNode = graph.Network[branchNet].Link[i - nLink_subNet].fromNode + nNode_subNet;
                graph.Network[subNet].Link[i].toNode = graph.Network[branchNet].Link[i - nLink_subNet].toNode + nNode_subNet;
                graph.Network[subNet].Link[i].bBackJ = graph.Network[branchNet].Link[i - nLink_subNet].bBackJ;
                graph.Network[subNet].Link[i].bBackS = graph.Network[branchNet].Link[i - nLink_subNet].bBackS;
                graph.Network[subNet].Link[i].bInstance = graph.Network[branchNet].Link[i - nLink_subNet].bInstance;
            }

            //connect boundaryNodes to orgBoundaryNode????
            int curLink = graph.Network[subNet].nLink - 1 - expand_boundaryNodes.Length;
            for (int i = 0; i < expand_boundaryNodes.Length; i++)
            {
                int branchNet_Indx = get_indexFromOrgNum(graph, branchNet, expand_boundaryNodes[i]);
                if (branchNet_Indx != -1)
                {
                    graph.Network[subNet].Link[curLink].fromNode = branchNet_Indx + nNode_subNet;
                    graph.Network[subNet].Link[curLink].toNode = expand_boundaryNodes[i];
                    //graph.Network[subNet].Link[curLink].bBackJ = 
                    //graph.Network[subNet].Link[curLink].bBackS = graph.Network[branchNet].Link[i - nLink_subNet].bBackS;
                    //graph.Network[subNet].Link[curLink].bInstance = graph.Network[branchNet].Link[i - nLink_subNet].bInstance;
                    curLink++;
                }
            }

            //connect orgCID_XOR to new branchNet_header
            graph.Network[subNet].Link[graph.Network[subNet].nLink - 1].fromNode = orgCID;
            graph.Network[subNet].Link[graph.Network[subNet].nLink - 1].toNode = branchNet_header;

            //====== IT DONE =======
        }

        public static int get_indexFromOrgNum(GraphVariables.clsGraph graph, int currentN, int orgNum)
        {
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].orgNum == orgNum)
                    return i;
            }
            return -1;
        }

        public static void expand_SearchNode(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int workLoop, int curLoop,
            ref int[] searchNode_, ref int nSearchNode_)
        {
            int[] tempSet = new int[graph.Network[currentN].nNode];
            int nTempSet = 0;

            for (int i = 0; i < nSearchNode_; i++)
            {
                tempSet[i] = searchNode_[i];
                nTempSet++;
            }

            gProAnalyzer.Ultilities.checkGraph checkG = new gProAnalyzer.Ultilities.checkGraph();
            int[] listCID_Loop = new int[clsLoop.Loop[workLoop].nLoop];
            int nList = 0;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].orgNum != -1)
                {
                    if (graph.Network[currentN].Node[i].header == true)
                    {
                        listCID_Loop[nList] = graph.Network[currentN].Node[i].headerOfLoop;
                        nList++;
                    }
                }
            }

            //combine tempSet + Loops (Expanded from reduced before)
            for (int i = 0; i < nList; i++)
            {
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].nNode; j++)
                {
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].Node[j]) == true) continue;
                    tempSet[nTempSet] = clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].Node[j];
                    nTempSet++;
                }
            }

            //combine tempSet + current IL
            if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, clsLoop.Loop[workLoop].Loop[curLoop].header) == false)
            {
                tempSet[nTempSet] = clsLoop.Loop[workLoop].Loop[curLoop].header;
                nTempSet++;
            }
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nNode; i++)
            {
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, clsLoop.Loop[workLoop].Loop[curLoop].Node[i]) == false)
                {
                    tempSet[nTempSet] = clsLoop.Loop[workLoop].Loop[curLoop].Node[i];
                    nTempSet++;
                }
            }

            searchNode_ = new int[nTempSet];
            nSearchNode_ = nTempSet;
            for (int i = 0; i < nTempSet; i++)
            {
                searchNode_[i] = tempSet[i];
            }
        }

        //remove Link to entries which are not initiated yet.
        public static void adjust_SubLink_mergeUntangling(ref GraphVariables.clsGraph graph, int fromN, int toN, GraphVariables.clsLoop clsLoop, int workLoop, int curLoop,
            int cur_Concurrency, int enInst)
        {
            //gProAnalyzer.Ultilities.checkGraph checkG = new gProAnalyzer.Ultilities.checkGraph();

            //(Type) for untangling IL => Connect all boundary node (split) except CID to new OR join
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[fromN].nLink + 2];

            //remove out-going edges of all non-active loop entries
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                int orgEntry = graph.Network[toN].Node[graph.Network[toN].Link[i].toNode].orgNum;
                int fromNode = graph.Network[toN].Node[graph.Network[toN].Link[i].fromNode].orgNum;
                //if ()
                if (gProAnalyzer.Ultilities.checkGraph.isLoopEntries(ref clsLoop, workLoop, orgEntry) && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, fromNode, curLoop) == false &&
                    gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, orgEntry, curLoop) && gProAnalyzer.Ultilities.checkGraph.isConcurrentEntry(clsLoop, workLoop, curLoop, cur_Concurrency, orgEntry, enInst) == false)
                {
                    graph.Network[toN].Link[i].toNode = graph.Network[toN].Link[i].fromNode; //removing
                }
            }

            int imNum = 0;
            //temporary transfer to imLink for import additional link form boundary nodes to OR-join
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                if (graph.Network[toN].Link[i].fromNode == graph.Network[toN].Link[i].toNode) continue; //only get valid link
                imLink[imNum] = graph.Network[toN].Link[i];
                imNum++;
            }

            //transfer back to Link_ToN
            graph.Network[toN].nLink = imNum;
            graph.Network[toN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[toN].nLink];
            for (int i = 0; i < graph.Network[toN].nLink; i++)
            {
                graph.Network[toN].Link[i] = imLink[i];
            }
        }

    }
}
