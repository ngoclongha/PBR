using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Functionalities
{
    class LoopIdentification
    {
        public struct strBlock
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
        public static strBlock[] Block;
        //노드 검색 결과 // Node Result
        public static int nSearchNode;
        public static int[] searchNode;

        //public gProAnalyzer.Ultilities.makeSubNetwork makSubNet;
        public static gProAnalyzer.GraphVariables.clsSESE clsSESE;

        public static void Initialize_All()
        {
            //makSubNet = new gProAnalyzer.Ultilities.makeSubNetwork();
        }

        public static void Run_FindLoop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int orgLoop, ref bool IrreducibleError)
        {
            Initialize_All();

            //int orgLoop = loop.orgLoop;

            //search_Loop(midNet, orgLoop); //originally, he use midNet
            search_Loop(ref graph, currentN, ref clsLoop, orgLoop);

            if (check_Irreducible(ref graph, currentN, ref clsLoop, orgLoop))
            {
                //MessageBox.Show("Irreducible Error : This network can not be handled");
                IrreducibleError = true;
                //return;
            }
            //Irreducible loop merging (if two IL directly nesting => Merge into one
            merge_Irreducible(ref clsLoop, orgLoop);

            //Irreducible loop내  헤드공유하는 Natural Loop 찾기 //find Natural Loop that share header with Irreducible loop
            inspect_Irreducible(ref graph, currentN, ref clsLoop, orgLoop);

            //find Special Node (Loop Entry: E, Loop Exit: X, Backward Split: B, BS and Exit: T)
            find_SpecialNode(ref graph, currentN, ref clsLoop, orgLoop); //update all loops
        }
        public static void inspect_Irreducible(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop)
        {
            int curDepth = 1;
            do
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (!clsLoop.Loop[workLoop].Loop[i].Irreducible) continue;

                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++)
                    {
                        if (clsLoop.Loop[workLoop].Loop[i].Entry[k] == clsLoop.Loop[workLoop].Loop[i].header) continue;

                        gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.subNet, ref clsLoop, workLoop, i, ref clsSESE, "II", clsLoop.Loop[workLoop].Loop[i].Entry[k]);    //5는 SubNetwork

                        add_subNatural(ref graph, graph.subNet, currentN, ref clsLoop, clsLoop.subLoop, workLoop, i);

                        break; //header가 다른 한 경우만 고려하면 됨....
                    }

                }
                curDepth++;
            } while (curDepth <= clsLoop.Loop[workLoop].maxDepth);
        }

        //check sub_Irreducible loop
        public static bool check_Irreducible(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop)
        {
            bool bError = false;
            int curDepth = 1;
            do
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (!clsLoop.Loop[workLoop].Loop[i].Irreducible) continue;

                    int nIrr = 0;
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nChild; k++)
                    {
                        if (clsLoop.Loop[workLoop].Loop[clsLoop.Loop[workLoop].Loop[i].child[k]].Irreducible) nIrr++;
                    }
                    if (nIrr == 0) continue;

                    gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.subNet, ref clsLoop, workLoop, i, ref clsSESE, "CI", -1);    //5는 SubNetwork

                    if (check_subIrreducible(ref graph, graph.subNet, ref clsLoop, clsLoop.subLoop))
                    {
                        bError = true;
                        break;
                    }
                }
                curDepth++;
            } while (curDepth <= clsLoop.Loop[workLoop].maxDepth && !bError);
            return bError;
        }


        public static void merge_Irreducible(ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop) //Otc27_2020 This might have some bugs
        {
            int curDepth = loop.Loop[workLoop].maxDepth;
            do
            {
                for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
                {
                    if (loop.Loop[workLoop].Loop[i].depth != curDepth) continue;
                    if (!loop.Loop[workLoop].Loop[i].Irreducible) continue; //if it is natural loop => not merge

                    int parent = loop.Loop[workLoop].Loop[i].parentLoop;
                    if (parent < 0) continue;
                    if (!loop.Loop[workLoop].Loop[parent].Irreducible) continue;

                    //check different loop entries set? => If 2 IL Nested not having same set of entries => get it
                    if (!gProAnalyzer.Ultilities.checkGraph.check_Overlap(loop.Loop[workLoop].Loop[parent].Entry, loop.Loop[workLoop].Loop[i].Entry)) continue; //NEW CODE

                    merge_Loop(ref loop, workLoop, parent, i);
                }
                curDepth--;
            } while (curDepth > 0);

            loop.Loop[workLoop].maxDepth = 0;
            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                if (loop.Loop[workLoop].Loop[i].depth > loop.Loop[workLoop].maxDepth) loop.Loop[workLoop].maxDepth = loop.Loop[workLoop].Loop[i].depth;
            }
        }

        public static void merge_Loop(ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop, int parent, int child)
        {
            //loop포함 노드
            int nNewNode = loop.Loop[workLoop].Loop[parent].nNode + loop.Loop[workLoop].Loop[child].nNode;
            int[] newNode = new int[nNewNode];
            nNewNode = 0;
            for (int i = 0; i < loop.Loop[workLoop].Loop[parent].nNode; i++)
            {
                newNode[nNewNode] = loop.Loop[workLoop].Loop[parent].Node[i];
                nNewNode++;
            }
            for (int i = 0; i < loop.Loop[workLoop].Loop[child].nNode; i++)
            {
                newNode[nNewNode] = loop.Loop[workLoop].Loop[child].Node[i];
                nNewNode++;
            }

            loop.Loop[workLoop].Loop[parent].nNode = nNewNode;
            loop.Loop[workLoop].Loop[parent].Node = newNode;

            // loop child loop
            int nNewChild = loop.Loop[workLoop].Loop[parent].nChild - 1; //PROBLEM HERE
            nNewChild = loop.Loop[workLoop].Loop[child].nChild; //NEW CODE to store it child of child

            int[] newChild = new int[nNewChild];
            nNewChild = 0;
            for (int i = 0; i < loop.Loop[workLoop].Loop[parent].nChild; i++)
            {
                if (loop.Loop[workLoop].Loop[parent].child[i] == child) continue;
                newChild[nNewChild] = loop.Loop[workLoop].Loop[parent].child[i];
                nNewChild++;
            }

            //add child_loop of its child
            for (int i = 0; i < loop.Loop[workLoop].Loop[child].nChild; i++)
            {
                newChild[nNewChild] = loop.Loop[workLoop].Loop[child].child[i];
                nNewChild++;
            }

            loop.Loop[workLoop].Loop[parent].nChild = nNewChild;
            loop.Loop[workLoop].Loop[parent].child = newChild;

            //child loop 제거 (Remove child loop)
            gProAnalyzer.GraphVariables.clsLoop.strLoopInform[] newLoop = new gProAnalyzer.GraphVariables.clsLoop.strLoopInform[loop.Loop[workLoop].nLoop - 1];
            int nNewLoop = 0;
            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                if (i == child) continue;
                newLoop[nNewLoop] = loop.Loop[workLoop].Loop[i];
                nNewLoop++;
            }
            loop.Loop[workLoop].nLoop = nNewLoop;
            loop.Loop[workLoop].Loop = newLoop;
            //child reNumbering
            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                for (int k = 0; k < loop.Loop[workLoop].Loop[i].nChild; k++)
                {
                    if (loop.Loop[workLoop].Loop[i].child[k] >= child) loop.Loop[workLoop].Loop[i].child[k] -= 1;
                }
            }

            //parent reNumbering
            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                if (loop.Loop[workLoop].Loop[i].parentLoop >= child) loop.Loop[workLoop].Loop[i].parentLoop -= 1;
            }
        }

        //Entry point of FIND LOOP
        public static void search_Loop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop)
        {
            //Find Backedges
            make_Loop(ref graph, currentN);

            loop.Loop[workLoop] = new gProAnalyzer.GraphVariables.clsLoop.strLoop();
            // Loop생성
            int cnt = 0;
            for (int i = 0; i < Block.Length; i++)
            {
                if (Block[i].LoopHeader) cnt++;
            }

            loop.Loop[workLoop].Loop = new gProAnalyzer.GraphVariables.clsLoop.strLoopInform[cnt];
            loop.Loop[workLoop].nLoop = 0;
            loop.Loop[workLoop].maxDepth = 0;

            find_Loop(ref graph, currentN, ref loop, workLoop, -1, 1); //This is the KEY to find all loop-node <= from Block = nNode


            //Depth Search 
            int depth = 1;
            int count = graph.Network[currentN].nNode;
            do
            {
                find_DepthNode(ref graph, currentN, depth);
                count = count_UnDepth(ref graph, currentN);
                depth++;

            } while (count > 0);


            // Loop별 Entry, Exit, Back SplitNode 검색
            find_LoopInform(ref graph, currentN, ref loop, workLoop);
        }

        //identify NL or IL after identify Block[i]
        public static void find_Loop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop, int header, int depth)
        {
            for (int i = 0; i < Block.Length; i++)
            {
                if (!Block[i].LoopHeader) continue;
                if (Block[i].iloop_header != header) continue;

                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].header = i;
                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].Irreducible = Block[i].Irreducible;
                //if (Loop[workLoop].Loop[Loop[workLoop].nLoop].Irreducible) Network[currentN].Node[i].Special = "hL";
                //else Network[currentN].Node[i].Special = "hN";
                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].depth = depth;

                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].parentLoop = -1;
                for (int k = 0; k < loop.Loop[workLoop].nLoop; k++)
                {
                    if (loop.Loop[workLoop].Loop[k].depth != depth - 1) continue;
                    if (loop.Loop[workLoop].Loop[k].header == Block[i].iloop_header)
                    {
                        loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].parentLoop = k;
                        break;
                    }
                }
                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nBackEdge = Block[i].nBackEdge;
                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].linkBack = new int[loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nBackEdge];
                for (int j = 0; j < Block[i].nBackEdge; j++)
                {
                    for (int k = 0; k < graph.Network[currentN].nLink; k++)
                    {
                        if (graph.Network[currentN].Link[k].fromNode == Block[i].fromNodeBack[j] && graph.Network[currentN].Link[k].toNode == i)
                        {
                            if (!loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].Irreducible) graph.Network[currentN].Link[k].bBackJ = true; //Natural Loop[workLoop].Loop만 BackEdge;
                            loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].linkBack[j] = k;
                            break;
                        }
                    }
                }
                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nNode = 0;
                for (int j = 0; j < Block.Length; j++)
                {
                    if (Block[j].iloop_header != loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].header) continue;
                    loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nNode++;
                }

                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].Node = new int[loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nNode];
                loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nNode = 0;
                for (int j = 0; j < Block.Length; j++)
                {
                    if (Block[j].iloop_header != loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].header) continue;

                    loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].Node[loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nNode] = j;
                    loop.Loop[workLoop].Loop[loop.Loop[workLoop].nLoop].nNode++;
                }

                loop.Loop[workLoop].nLoop++;

                if (depth > loop.Loop[workLoop].maxDepth) loop.Loop[workLoop].maxDepth = depth;
                find_Loop(ref graph, currentN, ref loop, workLoop, i, depth + 1);
            }
        }

        public static void make_Loop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN)
        {
            //초기화 //intialize
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                graph.Network[currentN].Node[i].depth = 0;
                graph.Network[currentN].Node[i].Special = "";
                graph.Network[currentN].Node[i].done = false;
            }
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                graph.Network[currentN].Link[i].bBackJ = false;
                graph.Network[currentN].Link[i].bBackS = false;
            }

            //모든 Loop찾아서 // FIND ALL LOOP
            int tempN = graph.Network[currentN].nNode;
            Block = new strBlock[tempN];
            for (int i = 0; i < tempN; i++)
            {
                Block[i].LoopHeader = false;
                Block[i].Irreducible = false;
                Block[i].tranversed = false;
                Block[i].DFSP_pos = 0;
                Block[i].iloop_header = -1;

                Block[i].nBackEdge = 0;
                Block[i].fromNodeBack = new int[graph.Network[currentN].Node[i].nPre];//.nPost];
                Block[i].nReentry = 0;
                Block[i].fromNodeReentry = new int[graph.Network[currentN].Node[i].nPre];//.nPost]; //Reentry => Loop-closing edges (or Backedges)
            }
            trav_loops_DFS(ref graph, currentN, graph.Network[currentN].header, 1);
        }

        //Duyet sau bat dau tu header. //DFS to find backedges (or loop-closing edges)
        public static int trav_loops_DFS(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int b0, int DFSP_pos)
        {
            Block[b0].tranversed = true;     //mark node as trnversed
            Block[b0].DFSP_pos = DFSP_pos;   //mark node's position in DFSP

            for (int i = 0; i < graph.Network[currentN].Node[b0].nPost; i++)
            {
                int b = graph.Network[currentN].Node[b0].Post[i];

                if (!Block[b].tranversed)
                {
                    //CASE A - New
                    int nh = trav_loops_DFS(ref graph, currentN, b, DFSP_pos + 1);
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
                            Block[b].fromNodeReentry[Block[b].nReentry] = b0; // mark (b0,b) as ReENTRY??
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

        public static void tag_lhead(int b, int h)
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

        public static void find_SpecialNode(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop)
        {
            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                //Etry
                for (int j = 0; j < loop.Loop[workLoop].Loop[i].nEntry; j++)
                {
                    int k = loop.Loop[workLoop].Loop[i].Entry[j];
                    graph.Network[currentN].Node[k].Special = "E";
                }

                //Exit
                for (int j = 0; j < loop.Loop[workLoop].Loop[i].nExit; j++)
                {
                    int k = loop.Loop[workLoop].Loop[i].Exit[j];
                    graph.Network[currentN].Node[k].Special = "X";
                }
            }

            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                if (!graph.Network[currentN].Link[i].bBackS) continue;

                int from = graph.Network[currentN].Link[i].fromNode;

                if (graph.Network[currentN].Node[from].Special == "X")
                {
                    graph.Network[currentN].Node[from].Special = "T";
                }
                else
                {
                    if (graph.Network[currentN].Node[from].Special != "T") //New line here (We add new condition for the check Special Node for BS, in order to avoid the reassetment of node "T"
                    {
                        graph.Network[currentN].Node[from].Special = "B";
                    }
                }
            }
        }

        public static void find_SpecialNode_ReducedLoop(ref gProAnalyzer.GraphVariables.clsGraph graph, int reduceN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int curLoop)
        {
            int[] node_set = new int[clsLoop.Loop[workLoop].Loop[curLoop].nNode + 1];
            node_set[0] = clsLoop.Loop[workLoop].Loop[curLoop].header;
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nNode; i++) node_set[i + 1] = clsLoop.Loop[workLoop].Loop[curLoop].Node[i];

            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nNode; i++)
            {
                int node = clsLoop.Loop[workLoop].Loop[curLoop].Node[i];
                if (graph.Network[reduceN].Node[node].Special == "E")
                {
                    for (int j = 0; j < graph.Network[reduceN].nLink; j++)
                    {
                        if (graph.Network[reduceN].Link[j].fromNode == node)
                            if (Ultilities.checkGraph.Node_In_Set(node_set, node_set.Length, graph.Network[reduceN].Link[j].toNode) == false)
                                graph.Network[reduceN].Node[node].Special = "X";
                    }
                }
            }

            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nNode; i++)
            {
                int node = clsLoop.Loop[workLoop].Loop[curLoop].Node[i];
                for (int j = 0; j < graph.Network[reduceN].nLink; j++)
                {
                    if (graph.Network[reduceN].Link[j].fromNode == node)
                    {
                        if (graph.Network[reduceN].Node[node].Special == "X")
                        {
                            if (graph.Network[reduceN].Link[j].bBackS == true) //backward split
                                graph.Network[reduceN].Node[node].Special = "T";
                        }
                        else
                            if (graph.Network[reduceN].Link[j].bBackS == true && graph.Network[reduceN].Node[node].Special != "T")
                                graph.Network[reduceN].Node[node].Special = "B";
                    }
                }
            }
        }

        public static void find_LoopInform(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop)
        {
            int cntFind = 0;
            int[] find_Node = new int[loop.Loop[workLoop].nLoop];


            // Child Loop[workLoop].Loop

            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                cntFind = 0;
                for (int j = 0; j < loop.Loop[workLoop].nLoop; j++)
                {
                    if (loop.Loop[workLoop].Loop[j].parentLoop == i)
                    {
                        find_Node[cntFind] = j;
                        cntFind++;
                    }

                }

                loop.Loop[workLoop].Loop[i].nChild = cntFind;
                loop.Loop[workLoop].Loop[i].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) loop.Loop[workLoop].Loop[i].child[k] = find_Node[k];

            }

            for (int i = 0; i < loop.Loop[workLoop].nLoop; i++)
            {
                //Loop[workLoop].Loop내 포함된 모든 노드 찾아서
                nSearchNode = 0;
                searchNode = new int[graph.Network[currentN].nNode];

                searchNode[nSearchNode] = loop.Loop[workLoop].Loop[i].header;
                nSearchNode++;
                find_LoopNode(ref loop, workLoop, i);

                // Entry

                cntFind = 0;
                find_Node = new int[graph.Network[currentN].nNode];

                for (int r = 0; r < nSearchNode; r++) // Loop[workLoop].Loop내 모든 노드에 대해
                {
                    bool isEntry = true;

                    for (int j = 0; j < graph.Network[currentN].Node[searchNode[r]].nPre; j++) // 노드의 모든 pre 노드에 대해
                    {
                        int nodeP = graph.Network[currentN].Node[searchNode[r]].Pre[j];

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
                loop.Loop[workLoop].Loop[i].nEntry = cntFind;
                loop.Loop[workLoop].Loop[i].Entry = new int[cntFind];
                for (int k = 0; k < cntFind; k++) loop.Loop[workLoop].Loop[i].Entry[k] = find_Node[k];
                //Exit

                cntFind = 0;
                find_Node = new int[graph.Network[currentN].nNode];

                for (int r = 0; r < nSearchNode; r++) // Loop[workLoop].Loop내 모든 노드에 대해
                {
                    bool isExit = true;

                    for (int j = 0; j < graph.Network[currentN].Node[searchNode[r]].nPost; j++) // 노드의 모든 post 노드에 대해
                    {
                        int nodeP = graph.Network[currentN].Node[searchNode[r]].Post[j];

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

                loop.Loop[workLoop].Loop[i].nExit = cntFind;
                loop.Loop[workLoop].Loop[i].Exit = new int[cntFind];
                for (int k = 0; k < cntFind; k++) loop.Loop[workLoop].Loop[i].Exit[k] = find_Node[k];

                //BackSplit
                cntFind = 0;
                find_Node = new int[graph.Network[currentN].nNode];

                int depth = graph.Network[currentN].Node[loop.Loop[workLoop].Loop[i].header].depth;
                for (int r = 0; r < nSearchNode; r++) // Loop[workLoop].Loop내 모든 노드에 대해
                {
                    int node = searchNode[r];
                    if (graph.Network[currentN].Node[node].depth != depth) continue;

                    bool isBack = false;
                    for (int k = 0; k < graph.Network[currentN].Node[node].nPost; k++)
                    {
                        int nodeP = graph.Network[currentN].Node[node].Post[k];

                        if (graph.Network[currentN].Node[nodeP].depth > depth)
                        {
                            for (int j = 0; j < graph.Network[currentN].nLink; j++)
                            {
                                if (graph.Network[currentN].Link[j].fromNode == node && graph.Network[currentN].Link[j].toNode == nodeP)
                                {
                                    graph.Network[currentN].Link[j].bBackS = true;
                                    isBack = true;
                                    break;
                                }
                            }
                        }
                        else if (graph.Network[currentN].Node[nodeP].depth == depth) //바로 BackEdge 면
                        {
                            for (int j = 0; j < graph.Network[currentN].nLink; j++)
                            {
                                if (!graph.Network[currentN].Link[j].bBackJ) continue;

                                if (graph.Network[currentN].Link[j].fromNode == node && graph.Network[currentN].Link[j].toNode == nodeP)
                                {
                                    graph.Network[currentN].Link[j].bBackS = true;
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
                loop.Loop[workLoop].Loop[i].nBackSplit = cntFind;
                loop.Loop[workLoop].Loop[i].BackSplit = new int[cntFind];
                for (int k = 0; k < cntFind; k++) loop.Loop[workLoop].Loop[i].BackSplit[k] = find_Node[k];
            }
        }

        public static void find_LoopNode(ref gProAnalyzer.GraphVariables.clsLoop loop, int workLoop, int kLoop)
        {

            for (int i = 0; i < loop.Loop[workLoop].Loop[kLoop].nNode; i++)
            {
                searchNode[nSearchNode] = loop.Loop[workLoop].Loop[kLoop].Node[i];
                nSearchNode++;
            }

            for (int k = 0; k < loop.Loop[workLoop].Loop[kLoop].nChild; k++)
            {
                find_LoopNode(ref loop, workLoop, loop.Loop[workLoop].Loop[kLoop].child[k]);
            }
        }

        public static void find_DepthNode(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int d)
        {
            int nEnd = 0;
            int[] EndNode = new int[graph.Network[currentN].nNode];

            if (d == 1) //종료노드 찾기
            {
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    if (graph.Network[currentN].Node[i].nPost == 0)
                    {
                        EndNode[nEnd] = i;
                        graph.Network[currentN].Node[i].depth = d;
                        nEnd++;
                    }
                }
            }
            else
            {
                for (int j = 0; j < graph.Network[currentN].nLink; j++)
                {
                    if (!graph.Network[currentN].Link[j].bBackJ) continue; //BackEdge 아니면

                    int fromNode = graph.Network[currentN].Link[j].fromNode;
                    int toNode = graph.Network[currentN].Link[j].toNode;

                    if (graph.Network[currentN].Node[toNode].depth == d - 1 && graph.Network[currentN].Node[fromNode].depth == 0)
                    {
                        EndNode[nEnd] = fromNode;
                        graph.Network[currentN].Node[fromNode].depth = d;
                        nEnd++;
                    }

                }
            }
            do
            {
                int nFrom = 0;
                int[] FromNode = new int[graph.Network[currentN].nNode];
                for (int i = 0; i < nEnd; i++)
                {
                    for (int k = 0; k < graph.Network[currentN].Node[EndNode[i]].nPre; k++)
                    {
                        int node = graph.Network[currentN].Node[EndNode[i]].Pre[k];

                        if (graph.Network[currentN].Node[node].depth != 0) continue; //이미 depth가 결정됬으면

                        bool isBack = false;

                        for (int j = 0; j < graph.Network[currentN].nLink; j++)
                        {
                            if (!graph.Network[currentN].Link[j].bBackJ) continue; //BackEdge 아니면

                            if (graph.Network[currentN].Link[j].fromNode == node && graph.Network[currentN].Link[j].toNode == EndNode[i])
                            {
                                isBack = true;
                                break;
                            }
                        }
                        if (isBack) continue; //backEdge면

                        graph.Network[currentN].Node[node].depth = d;
                        FromNode[nFrom] = node;
                        nFrom++;
                    }
                }
                nEnd = nFrom;
                EndNode = FromNode;
            } while (nEnd > 0);
        }

        public static int count_UnDepth(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN)
        {
            int count = 0;

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].depth == 0) count++;
            }
            return count;
        }
        public static void add_subNatural(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int saveN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int saveLoop, int loop)
        {
            //Loop[workLoop].Loop찾기
            search_Loop(ref graph, currentN, ref clsLoop, workLoop);
            for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[i].parentLoop == -1) continue;
                if (clsLoop.Loop[workLoop].Loop[i].Irreducible) continue;

                int orgHeader = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[i].header].parentNum;
                if (orgHeader != clsLoop.Loop[saveLoop].Loop[loop].header) continue; //원 loop의 헤더를 공유하는 것만 추가

                // Loop[workLoop].Loop 추가 ----------------------
                gProAnalyzer.GraphVariables.clsLoop.strLoopInform addLoop = new gProAnalyzer.GraphVariables.clsLoop.strLoopInform();
                int orgNode, orgNode2;

                addLoop = clsLoop.Loop[workLoop].Loop[i]; //일단 copy

                addLoop.depth = clsLoop.Loop[saveLoop].Loop[loop].depth + 1;
                addLoop.parentLoop = loop;
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nChild; k++)
                {
                    orgNode = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[clsLoop.Loop[workLoop].Loop[i].child[k]].header].parentNum;
                    int findLoop = -1;
                    for (int k2 = 0; k2 < clsLoop.Loop[saveLoop].nLoop; k2++)
                    {
                        if (orgNode == clsLoop.Loop[saveLoop].Loop[k2].header)
                        {
                            findLoop = k2;
                            break;
                        }
                    }
                    addLoop.child[k] = findLoop;
                    clsLoop.Loop[saveLoop].Loop[findLoop].parentLoop = clsLoop.Loop[saveLoop].nLoop;
                }

                orgNode = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[i].header].parentNum;
                addLoop.header = orgNode;

                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nBackEdge; k++)
                {
                    orgNode = graph.Network[currentN].Link[clsLoop.Loop[workLoop].Loop[i].linkBack[k]].fromNode;
                    orgNode = graph.Network[currentN].Node[orgNode].parentNum;
                    orgNode2 = graph.Network[currentN].Link[clsLoop.Loop[workLoop].Loop[i].linkBack[k]].toNode;
                    orgNode2 = graph.Network[currentN].Node[orgNode2].parentNum;

                    int orgLink = -1;
                    for (int k2 = 0; k2 < graph.Network[saveN].nLink; k2++)
                    {
                        if (orgNode == graph.Network[saveN].Link[k2].fromNode && orgNode2 == graph.Network[saveN].Link[k2].toNode)
                        {
                            orgLink = k2;
                            break;
                        }
                    }
                    addLoop.linkBack[k] = orgLink;
                }

                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++)
                {
                    orgNode = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[i].Entry[k]].parentNum;
                    addLoop.Entry[k] = orgNode;
                }

                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nExit; k++)
                {
                    orgNode = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[i].Exit[k]].parentNum;
                    addLoop.Exit[k] = orgNode;
                }

                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nNode; k++)
                {
                    orgNode = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[i].Node[k]].parentNum;
                    addLoop.Node[k] = orgNode;
                }

                int nNewLoop = clsLoop.Loop[saveLoop].nLoop + 1;
                gProAnalyzer.GraphVariables.clsLoop.strLoop newLoop = new gProAnalyzer.GraphVariables.clsLoop.strLoop();
                newLoop.Loop = new gProAnalyzer.GraphVariables.clsLoop.strLoopInform[nNewLoop];

                for (int k = 0; k < clsLoop.Loop[saveLoop].nLoop; k++)
                {
                    newLoop.Loop[k] = clsLoop.Loop[saveLoop].Loop[k];
                    if (k == loop) //추가Loop[workLoop].Loop의 parents면
                    {
                        //----Child  추가 및 제거
                        int numAdd = 0;
                        int[] add = new int[clsLoop.Loop[saveLoop].Loop[k].nChild + 1];
                        for (int k2 = 0; k2 < clsLoop.Loop[saveLoop].Loop[k].nChild; k2++)
                        {
                            bool bDel = false;
                            for (int k3 = 0; k3 < addLoop.nChild; k3++)
                            {
                                if (clsLoop.Loop[saveLoop].Loop[k].child[k2] == addLoop.child[k3])
                                {
                                    bDel = true;
                                    break;
                                }
                            }
                            if (!bDel)
                            {
                                add[numAdd] = clsLoop.Loop[saveLoop].Loop[k].child[k2];
                                numAdd++;
                            }
                        }
                        add[numAdd] = clsLoop.Loop[saveLoop].nLoop;
                        numAdd++;

                        newLoop.Loop[k].nChild = numAdd;
                        newLoop.Loop[k].child = add;

                        //-----포함 Node 제거
                        numAdd = 0;
                        add = new int[clsLoop.Loop[saveLoop].Loop[k].nNode];
                        for (int k2 = 0; k2 < clsLoop.Loop[saveLoop].Loop[k].nNode; k2++)
                        {
                            bool bDel = false;
                            for (int k3 = 0; k3 < addLoop.nNode; k3++)
                            {
                                if (clsLoop.Loop[saveLoop].Loop[k].Node[k2] == addLoop.Node[k3])
                                {
                                    bDel = true;
                                    break;
                                }
                            }
                            if (!bDel)
                            {
                                add[numAdd] = clsLoop.Loop[saveLoop].Loop[k].Node[k2];
                                numAdd++;
                            }
                        }
                        newLoop.Loop[k].nNode = numAdd;
                        newLoop.Loop[k].Node = add;
                    }
                }
                newLoop.Loop[clsLoop.Loop[saveLoop].nLoop] = addLoop;
                clsLoop.Loop[saveLoop].Loop = newLoop.Loop;
                clsLoop.Loop[saveLoop].nLoop = nNewLoop;
            }
            clsLoop.Loop[saveLoop].maxDepth = 0;
            for (int i = 0; i < clsLoop.Loop[saveLoop].nLoop; i++)
            {
                if (clsLoop.Loop[saveLoop].Loop[i].depth > clsLoop.Loop[saveLoop].maxDepth) clsLoop.Loop[saveLoop].maxDepth = clsLoop.Loop[saveLoop].Loop[i].depth;
            }
        }

        public static bool check_subIrreducible(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop)
        {

            //Loop[workLoop].Loop찾기
            search_Loop(ref graph, currentN, ref clsLoop, workLoop);

            bool bError = false;


            for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[i].depth > 1) continue;
                if (clsLoop.Loop[workLoop].Loop[i].Irreducible)
                {
                    bError = true;
                    break;
                }
            }
            return bError;
        }
    }
}
