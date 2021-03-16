using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class reduceGraph
    {
        private gProAnalyzer.Ultilities.clsFindNodeInfo nodeInfo;
        private gProAnalyzer.Ultilities.findIntersection fndIntersec;
        private gProAnalyzer.Ultilities.checkGraph chkGraph;

        private static void Initialized_All()
        {
            //fndIntersec = new gProAnalyzer.Ultilities.findIntersection();
            //chkGraph = new gProAnalyzer.Ultilities.checkGraph();
        }

        // =========== THIS NEED TO BE FIX (WHEN NESTED LOOP SHARED EXIT (ENTRY??) ARE REDUCED???????) ===>> THE IL_REDUCITION ALSO NEED TO BE CHECKED
        public static void reduce_Loop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, 
            int workLoop, int loop, string strLoop, bool checkError) //Reduce natural Loops
        {
            //Multi entry - Multi Exit이면 분리????????????????????????
            bool doSplit = false;
            //if (Loop[workLoop].Loop[loop].nEntry > 1 && Loop[workLoop].Loop[loop].nExit > 1) doSplit = true; //why split here??

            // Loop내 노드만 구성
            int[] imNode = new int[graph.Network[currentN].nNode];
            int num = 0;

            imNode[num] = clsLoop.Loop[workLoop].Loop[loop].header;
            num++;
            for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nNode; j++)
            {
                graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[loop].Node[j]].done = true; // header제외한 loop내 노드 축소
                imNode[num] = clsLoop.Loop[workLoop].Loop[loop].Node[j];
                num++;
            }
            int loopNode = clsLoop.Loop[workLoop].Loop[loop].header; //Loop[workLoop].Loop[loop].header를 대표 Node 로
            graph.Network[currentN].Node[loopNode].Kind = "XOR"; // 대표 Node는 XOR노드로........
            graph.Network[currentN].Node[loopNode].Name = "L[" + loop.ToString() + "]";
            graph.Network[currentN].Node[loopNode].Type_I = "";
            graph.Network[currentN].Node[loopNode].Type_II = "";
            graph.Network[currentN].Node[loopNode].header = true;
            graph.Network[currentN].Node[loopNode].headerOfLoop = loop;

            int exitNode = loopNode;
            if (doSplit)
            {
                exitNode = clsLoop.Loop[workLoop].Loop[loop].Exit[0];
                graph.Network[currentN].Node[exitNode].done = false;
                graph.Network[currentN].Node[exitNode].Type_I = "_j";
                graph.Network[currentN].Node[exitNode].Kind = "XOR"; // 대표 Node는 XOR노드로........
                graph.Network[currentN].Node[exitNode].Name = "L[" + loop.ToString() + "]";
                graph.Network[currentN].Node[exitNode].Type_I = "_s";
                graph.Network[currentN].Node[exitNode].Type_II = "";
            }
            //irreducible loop이면
            //헤더아닌 entry노드로 들어오는 링크를 헤더로 변경*******************************
            if (clsLoop.Loop[workLoop].Loop[loop].Irreducible)
            {
                int fromNode = -1;
                for (int k = 0; k < graph.Network[currentN].nLink; k++)
                {
                    if (graph.Network[currentN].Link[k].toNode == loopNode)
                    {
                        fromNode = graph.Network[currentN].Link[k].fromNode;
                        break;
                    }
                }
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nEntry; i++)
                {
                    if (clsLoop.Loop[workLoop].Loop[loop].Entry[i] == loopNode) continue;

                    for (int k = 0; k < graph.Network[currentN].nLink; k++)
                    {
                        if (graph.Network[currentN].Link[k].toNode == clsLoop.Loop[workLoop].Loop[loop].Entry[i])
                        {
                            if (graph.Network[currentN].Link[k].fromNode == fromNode)
                            {
                                graph.Network[currentN].Link[k].toNode = fromNode;
                            }
                            else
                            {
                                graph.Network[currentN].Link[k].toNode = loopNode;
                            }
                        }
                    }
                }
            }
            //**************************************************************************
            //대표 Node 정보 변경
            int pLoop = clsLoop.Loop[workLoop].Loop[loop].parentLoop;
            bool makeSplitLink = false;
            for (int k = 0; k < graph.Network[currentN].nLink; k++)
            {
                if (graph.Network[currentN].Link[k].fromNode == loopNode) //대표노드로 부터 나가는 링크 제거
                {
                    if (doSplit && !makeSplitLink) // Split 연결
                    {
                        graph.Network[currentN].Link[k].toNode = exitNode;
                        makeSplitLink = true;
                    }
                    else
                    {
                        graph.Network[currentN].Link[k].fromNode = graph.Network[currentN].Link[k].toNode;
                    }
                }
                if (graph.Network[currentN].Link[k].toNode == loopNode) //대표노드로 들어오는 내부 링크 제거
                {
                    bool bInLoop = false;
                    for (int j = 0; j < num; j++)
                    {
                        if (graph.Network[currentN].Link[k].fromNode == imNode[j])
                        {
                            bInLoop = true;
                            break;
                        }
                    }
                    if (bInLoop)
                    {
                        graph.Network[currentN].Link[k].toNode = graph.Network[currentN].Link[k].fromNode;
                    }
                }
            }

            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nExit; i++)
            {
                if (pLoop >= 0)
                {
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[pLoop].nExit; j++)
                    {
                        if (clsLoop.Loop[workLoop].Loop[loop].Exit[i] == clsLoop.Loop[workLoop].Loop[pLoop].Exit[j]) //Parent와 Exit 공유하면
                        {
                            clsLoop.Loop[workLoop].Loop[pLoop].Exit[j] = exitNode; //대표노드를 Parent의 Exit으로 변경
                            graph.Network[currentN].Node[exitNode].Special = graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[loop].Exit[i]].Special;
                            break;
                        }
                    }
                }

                for (int k = 0; k < graph.Network[currentN].nLink; k++) //exit노드로 부터 loop밖으로 나가는 링크를 대표노드로 부터 나가도록 변경
                {
                    if (graph.Network[currentN].Link[k].fromNode == clsLoop.Loop[workLoop].Loop[loop].Exit[i])
                    {
                        bool bInLoop = false;
                        for (int j = 0; j < num; j++)
                        {
                            if (graph.Network[currentN].Link[k].toNode == imNode[j])
                            {
                                bInLoop = true;
                                break;
                            }
                        }

                        if (!bInLoop)
                        {
                            graph.Network[currentN].Link[k].fromNode = exitNode;
                        }
                    }
                }
            }

            //동일한 노드로의 Exit이면 Error (Check and report error??)
            if (checkError)
            {
                /*
                for (int k = 0; k < graph.Network[currentN].nLink; k++)
                {
                    if (graph.Network[currentN].Link[k].fromNode != exitNode) continue;

                    for (int k2 = k + 1; k2 < graph.Network[currentN].nLink; k2++)
                    {
                        if (graph.Network[currentN].Link[k2].fromNode != exitNode) continue;

                        if (graph.Network[currentN].Link[k].toNode == graph.Network[currentN].Link[k2].toNode)
                        {
                            graph.Network[currentN].Link[k2].fromNode = graph.Network[currentN].Link[k2].toNode;

                            if (graph.Network[currentN].Node[graph.Network[currentN].Link[k].toNode].Kind == "END") continue;

                            if (graph.Network[currentN].Node[graph.Network[currentN].Link[k].toNode].Kind == "XOR") continue;

                            //Error
                            Error[nError].Loop = strLoop;
                            Error[nError].Node = graph.Network[currentN].Node[graph.Network[currentN].Link[k].toNode].parentNum.ToString();

                            Error[nError].currentKind = graph.Network[currentN].Node[graph.Network[currentN].Link[k].toNode].Kind;
                            Error[nError].messageNum = 9;

                            //nError++;
                            add_Error();

                        }
                    }
                }
                */
            }
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].done)
                {
                    graph.Network[currentN].Node[i].nPre = 0;
                    graph.Network[currentN].Node[i].nPost = 0;
                    graph.Network[currentN].Node[i].Pre = null;
                    graph.Network[currentN].Node[i].Post = null;
                    graph.Network[currentN].Node[i].Special = "";
                }
                else
                {
                    //nodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, i);
                }
            }
        }

        //Need it?
        public void reduce_NaLoop()
        {

        }

        //Reduce SESE into a single node => Represent as SESE_Entry
        public static void reduce_SESE(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, int currSESE)
        {
            int[] imNode = new int[graph.Network[currentN].nNode + 1];
            int num = 0; 

            //imNode[num] = clsSESE.SESE[workSESE].SESE[currSESE].Entry;
            //num++;
            //Mark node in SESE as will be isolated
            for (int j = 0; j < clsSESE.SESE[workSESE].SESE[currSESE].nNode; j++) {
                graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[currSESE].Node[j]].done = true; // Collapse nodes in SESE except ENTRY
                imNode[num] = clsSESE.SESE[workSESE].SESE[currSESE].Node[j];
                num++;
            }

            int Entry = clsSESE.SESE[workSESE].SESE[currSESE].Entry;
            graph.Network[currentN].Node[Entry].done = false;
            graph.Network[currentN].Node[Entry].Kind = "SESE";
            graph.Network[currentN].Node[Entry].Name = "SESE[" + currSESE.ToString() + "]";
            graph.Network[currentN].Node[Entry].Type_I = "";
            graph.Network[currentN].Node[Entry].Type_II = "";
            int exitNode = clsSESE.SESE[workSESE].SESE[currSESE].Exit;

            //Edit the EDGES == (REMOVE UNECESSARY EDGES)
            int pSESE = clsSESE.SESE[workSESE].SESE[currSESE].parentSESE;            

            for (int k = 0; k < graph.Network[currentN].nLink; k++) {
                if (graph.Network[currentN].Link[k].fromNode == Entry) //Remove out-edges from ENTRY
                {
                    //if (clsSESE.SESE[workSESE].SESE[currSESE].nNode == 2)
                    //{
                        //graph.Network[currentN].Link[k].fromNode = graph.Network[currentN].Link[k].fromNode;
                    //}
                    //else
                        graph.Network[currentN].Link[k].fromNode = graph.Network[currentN].Link[k].toNode;
                }

                //Remove edges in node which join as ENTRY
                if (graph.Network[currentN].Link[k].toNode == Entry && graph.Network[currentN].Node[Entry].nPre > 1) {
                    bool bInLoop = false;
                    for (int j = 0; j < num; j++) {
                        if (graph.Network[currentN].Link[k].fromNode == imNode[j]) {
                            bInLoop = true;
                            break;
                        }
                    }
                    if (bInLoop)
                        graph.Network[currentN].Link[k].toNode = graph.Network[currentN].Link[k].fromNode;
                }
                //Remove in-edge from inside to EXIT  
                if (graph.Network[currentN].Link[k].toNode == exitNode)
                    graph.Network[currentN].Link[k].toNode = graph.Network[currentN].Link[k].fromNode;
                //Remove edges in node which split as EXIT 
                if (graph.Network[currentN].Link[k].fromNode == exitNode && graph.Network[currentN].Node[exitNode].nPost > 1) {
                    bool bInLoop = false;
                    for (int j = 0; j < num; j++) {
                        if (graph.Network[currentN].Link[k].toNode == imNode[j]) {
                            bInLoop = true;
                            break;
                        }
                    }
                    if (bInLoop)
                        graph.Network[currentN].Link[k].fromNode = graph.Network[currentN].Link[k].toNode;
                }
            }

            //Edit out-edges of ENTRY.
            for (int k = 0; k < graph.Network[currentN].nLink; k++) {
                if (graph.Network[currentN].Link[k].fromNode == exitNode && graph.Network[currentN].Link[k].fromNode != graph.Network[currentN].Link[k].toNode)
                {
                    graph.Network[currentN].Link[k].fromNode = Entry;
                }
            }

            //Finally - update node information
            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                if (graph.Network[currentN].Node[i].done) {
                    graph.Network[currentN].Node[i].nPre = 0;
                    graph.Network[currentN].Node[i].nPost = 0;
                    graph.Network[currentN].Node[i].Pre = null;
                    graph.Network[currentN].Node[i].Post = null;
                    graph.Network[currentN].Node[i].Special = "";
                }
                else {
                    //nodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, i);
                }
            }
        }

        //Reduce IL into a combination of 2 node => Represent as CIPd and EXITs ==========>> NEED TO CHECK WHETHER UPDATE EXITS FOR NESTED CASE??
        public static void reduce_IrLoop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int currLoop)
        {
            Initialized_All();
            //Find CIPd of IL            
            if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry == 1) return;
            int CIPd = -1;
            int[] searchNode = new int[clsLoop.Loop[workLoop].Loop[currLoop].nNode];
            int nSearchNode = 0;

            int[] calDomRev = null;
            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++) {
                calDomRev = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDomRev, graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Entry[k]].DomRev);
            }

            if (calDomRev.Length > 0) {
                int header = calDomRev[0];
                CIPd = header;
                graph.Network[currentN].Node[CIPd].done = false;
                graph.Network[currentN].Node[CIPd].Kind = "IL";
                graph.Network[currentN].Node[CIPd].Name = "IL[" + currLoop.ToString() + "]";
                graph.Network[currentN].Node[CIPd].Type_I = "";
                graph.Network[currentN].Node[CIPd].Type_II = "";

                searchNode[nSearchNode] = header;
                nSearchNode++;

                //From here "searchNode[]" will store the path from sNode to Entries => We have path
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++) {
                    //mark[i] fill = false; x[i]; searchNode; nSearchNode;
                    bool[] mark = new bool[graph.Network[currentN].nNode];
                    searchNode[nSearchNode] = clsLoop.Loop[workLoop].Loop[currLoop].Entry[k];
                    nSearchNode++;
                    prepare_find_Path(graph, currentN, ref mark, false);
                    if (header != clsLoop.Loop[workLoop].Loop[currLoop].Entry[k]) {
                        //find_Path(graph, currentN, clsLoop, workLoop, currLoop, clsLoop.Loop[workLoop].Loop[currLoop].Entry[k], header, ref mark, ref X, header, ref searchNode, ref nSearchNode);
                        //getResult(ref X, clsLoop.Loop[workLoop].Loop[currLoop].Entry[k], header, ref searchNode, ref nSearchNode);
                        find_Path_CIPd(graph, currentN, clsLoop.Loop[workLoop].Loop[currLoop].Entry[k], CIPd, ref mark);
                        getResult(ref mark, ref searchNode, ref nSearchNode);
                    }
                }
            }
            else return;

            //Mark all node in IL as to be isolated (some will be included, latter)   
            int[] imNode = new int[graph.Network[currentN].nNode];
            int num = 0;
            imNode[num] = clsLoop.Loop[workLoop].Loop[currLoop].header; //Header of IL
            num++;
            for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nNode; j++) {
                graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Node[j]].done = true; // Collapse nodes in SESE except ENTRY
                imNode[num] = clsLoop.Loop[workLoop].Loop[currLoop].Node[j];
                num++;
            }
            for (int j = 0; j < nSearchNode; j++) {
                graph.Network[currentN].Node[searchNode[j]].done = false;
            }

            int[] outEdges_Exit = new int[graph.Network[currentN].nNode];
            int nOutedges = 0;
            int luckyExit = clsLoop.Loop[workLoop].Loop[currLoop].Exit[0];
            graph.Network[currentN].Node[luckyExit].done = false; //Keep this exit and connected it with CIPd

            for (int i = 0; i < graph.Network[currentN].nLink; i++) {
                int fromNode = graph.Network[currentN].Link[i].fromNode;
                int toNode = graph.Network[currentN].Link[i].toNode;

                //Remove edges in node which split as EXIT 
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nExit; j++) {
                    int currExit = clsLoop.Loop[workLoop].Loop[currLoop].Exit[j];
                    if (graph.Network[currentN].Link[i].fromNode == currExit && graph.Network[currentN].Node[currExit].nPost > 1) {
                        bool bInLoop = false;
                        for (int k = 0; k < num; k++) {
                            if (graph.Network[currentN].Link[i].toNode == imNode[k]) {
                                bInLoop = true;
                                break;
                            }
                        }
                        if (bInLoop)
                            graph.Network[currentN].Link[i].fromNode = graph.Network[currentN].Link[i].toNode;
                        else{
                            //Record ougoing edges of Exits
                            outEdges_Exit[nOutedges] = graph.Network[currentN].Link[i].toNode;
                            nOutedges++;
                        }
                    }                    
                }
            }

            for (int i = 0; i < graph.Network[currentN].nLink; i++) {
                int fromNode = graph.Network[currentN].Link[i].fromNode;
                int toNode = graph.Network[currentN].Link[i].toNode;
                //remove return edges of IL Entry
                if (nodeInSet(imNode, num, fromNode) && nodeInSet(imNode, num, toNode))
                    if ((nodeInSet(searchNode, nSearchNode, fromNode) && !nodeInSet(searchNode, nSearchNode, toNode)) || 
                        (nodeInSet(searchNode, nSearchNode, toNode) && !nodeInSet(searchNode, nSearchNode, fromNode)))
                        graph.Network[currentN].Link[i].fromNode = graph.Network[currentN].Link[i].toNode;
            }

            //Edit out-edges of LuckyExit.
            for (int i = 0; i < graph.Network[currentN].nLink; i++) {               
                for (int j = 0; j < nOutedges; j++) {
                    if (graph.Network[currentN].Link[i].toNode == outEdges_Exit[j] && nodeInSet(imNode, num, graph.Network[currentN].Link[i].fromNode)) {
                        graph.Network[currentN].Link[i].fromNode = luckyExit;
                    }
                }
            }

            //connect CIPd to LuckyExit
            for (int i = 0; i < graph.Network[currentN].nLink; i++) {
                if (graph.Network[currentN].Link[i].toNode == luckyExit) {
                    graph.Network[currentN].Link[i].fromNode = CIPd;

                }
            }

            //nodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
            //Finally - update node information
            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                if (graph.Network[currentN].Node[i].done) {
                    graph.Network[currentN].Node[i].nPre = 0;
                    graph.Network[currentN].Node[i].nPost = 0;
                    graph.Network[currentN].Node[i].Pre = null;
                    graph.Network[currentN].Node[i].Post = null;
                }
                else {
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, i);
                }
            }
        }

        public static void reduce_IrLoop_Preprocessing(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int currLoop)
        {   //Reduce IL into an OR-join and XOR-split
            Initialized_All();
            int[] calDom = null;
            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++) {
                calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDom, graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Entry[k]].Dom);
            }

            int CID = -1;
            if (calDom.Length > 0) {
                for (int i = calDom.Length - 1; i > 0; i--) 
                    if (check_CID_In_Loop(clsLoop, workLoop, currLoop, calDom[i]) == false) {
                        CID = calDom[i];
                        break;
                    }
                if (CID == -1) return; //error

                //Mustcheck CID not belong to any nesting Loop (only belong to parent of IL[x]) (or natural loop header at the same depth - inside current parent also)
                //
                //

                //create 2 node entry_OR and exit_XOR of new reduced IL
                int[] imNode = new int[graph.Network[currentN].nNode];
                int num = 0;

                imNode[num] = clsLoop.Loop[workLoop].Loop[currLoop].header;
                num++;
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nNode; j++) {
                    graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[currLoop].Node[j]].done = true; // header제외한 loop내 노드 축소
                    imNode[num] = clsLoop.Loop[workLoop].Loop[currLoop].Node[j];
                    num++;
                }
                int entry_OR = clsLoop.Loop[workLoop].Loop[currLoop].header; //Loop[workLoop].Loop[loop].header를 대표 Node 로
                graph.Network[currentN].Node[entry_OR].Kind = "OR"; // 대표 Node는 XOR노드로........
                graph.Network[currentN].Node[entry_OR].Name = "IL[" + currLoop.ToString() + "]";
                graph.Network[currentN].Node[entry_OR].Type_I = "";
                graph.Network[currentN].Node[entry_OR].Type_II = "";
                graph.Network[currentN].Node[entry_OR].done = false;
                graph.Network[currentN].Node[entry_OR].header = true;
                graph.Network[currentN].Node[entry_OR].headerOfLoop = currLoop;
                graph.Network[currentN].Node[entry_OR].Special = ""; //remove the role as loop-gateways (now it is a normal acyclic nodes)

                int exit_XOR = clsLoop.Loop[workLoop].Loop[currLoop].Exit[0]; //Loop[workLoop].Loop[loop].header를 대표 Node 로
                graph.Network[currentN].Node[exit_XOR].Kind = "XOR"; // 대표 Node는 XOR노드로........
                graph.Network[currentN].Node[exit_XOR].Name = "IL[" + currLoop.ToString() + "]";
                graph.Network[currentN].Node[exit_XOR].Type_I = "";
                graph.Network[currentN].Node[exit_XOR].Type_II = "";
                graph.Network[currentN].Node[exit_XOR].done = false;
                graph.Network[currentN].Node[exit_XOR].header = false;
                graph.Network[currentN].Node[exit_XOR].Special = "";

                int indx_Link = -1;
                for (int i = 0; i < graph.Network[currentN].nLink; i++) {
                    int fromNode = graph.Network[currentN].Link[i].fromNode;
                    int toNode = graph.Network[currentN].Link[i].toNode;
                    //routing incoming edges of IL to entry_OR
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; j++){
                        if (toNode == clsLoop.Loop[workLoop].Loop[currLoop].Entry[j] && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, fromNode, currLoop) == false && entry_OR != toNode)
                            graph.Network[currentN].Link[i].toNode = entry_OR;
                    }
                    //routing outgoing edges of IL from exit_XOR
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nExit; j++) {
                        if (fromNode == clsLoop.Loop[workLoop].Loop[currLoop].Exit[j] && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, toNode, currLoop) == false && exit_XOR != fromNode)
                            graph.Network[currentN].Link[i].fromNode = exit_XOR;
                    }

                    //cut-off inner edges with entry_OR
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; j++) {
                        if (toNode == entry_OR && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, fromNode, currLoop) == true)
                        {
                            graph.Network[currentN].Link[i].toNode = graph.Network[currentN].Link[i].fromNode;
                        }
                        if (fromNode == entry_OR && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, toNode, currLoop) == true)
                        {
                            graph.Network[currentN].Link[i].fromNode = graph.Network[currentN].Link[i].toNode;
                            indx_Link = i;
                        }
                    }
                    //cut-off inner edges with exit_XOR
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[currLoop].nExit; j++) {
                        if (toNode == exit_XOR && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, fromNode, currLoop) == true)
                        {
                            graph.Network[currentN].Link[i].toNode = graph.Network[currentN].Link[i].fromNode;
                        }
                        if (fromNode == exit_XOR && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, toNode, currLoop) == true)
                        {
                            graph.Network[currentN].Link[i].fromNode = graph.Network[currentN].Link[i].toNode;
                        }
                    }
                }

                //connect entry_OR to exit_XOR of new reduced IL
                graph.Network[currentN].Link[indx_Link].fromNode = entry_OR;
                graph.Network[currentN].Link[indx_Link].toNode = exit_XOR;

                //remove duplicated link (could happend when CID directly to all Entries)
                gProAnalyzer.GraphVariables.clsEdge.strEdge[] imLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[currentN].nLink];
                int imNum = 0;
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {
                    bool isAdd = true;
                    for (int j = 0; j < imNum; j++)
                        if (graph.Network[currentN].Link[i].fromNode == imLink[j].fromNode &&
                            graph.Network[currentN].Link[i].toNode == imLink[j].toNode)
                        {
                            isAdd = false;
                            break;
                        }

                    if (isAdd)
                    {
                        imLink[imNum] = graph.Network[currentN].Link[i];
                        imNum++;
                    }
                }
                //transfer LINK to new Graph
                graph.Network[currentN].nLink = imNum;
                graph.Network[currentN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[imNum];
                for (int i = 0; i < imNum; i++)
                {
                    graph.Network[currentN].Link[i] = imLink[i];
                }



                //Finally - update node information
                for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                    if (graph.Network[currentN].Node[i].done) {
                        graph.Network[currentN].Node[i].nPre = 0;
                        graph.Network[currentN].Node[i].nPost = 0;
                        graph.Network[currentN].Node[i].Pre = null;
                        graph.Network[currentN].Node[i].Post = null;
                    }
                    else {
                        //nodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, i);
                    }
                }
            }
        }

        public static bool check_CID_In_Loop(GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, int canCID)
        {
            Initialized_All();

            int curDepth = clsLoop.Loop[workLoop].Loop[curLoop].depth;
            for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++) {
                if (clsLoop.Loop[workLoop].Loop[i].depth == curDepth)
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, canCID, i))
                        if (!(clsLoop.Loop[workLoop].Loop[i].header == canCID && clsLoop.Loop[workLoop].Loop[i].nEntry == 1))
                            return true;
            }
            return false;
        }

        public void total_reduceSubgraph(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int stopDepth) //Reduce all the loops inside (NL, IL)
        {            
            int curDepth = clsHWLS.FBLOCK.maxDepth;
            if (curDepth == stopDepth) return; 
            //curDepth = 1;
            do {
                for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                    if (clsHWLS.FBLOCK.FBlock[i].depth != curDepth) continue;
                    int orgIndx = clsHWLS.FBLOCK.FBlock[i].refIndex;

                    //If SESE => Make subnetwork and find Dominator Behavior relation matrix (1 matrix)
                    if (clsHWLS.FBLOCK.FBlock[i].SESE) {
                        reduce_SESE(ref graph, currentN, clsSESE, workSESE, orgIndx);
                    }

                    //If NL => Make subnetwork and find Dom/ Pdom Behavior relation matrix (2 matrices)
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsLoop.Loop[workLoop].Loop[orgIndx].nEntry == 1) {
                        reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, orgIndx, "", false);
                    }

                    //if IL => Find CIPd => Make subnetwork with CIPd and find Dom/ Pdom Behavior relation matrix (2 matrices)
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsLoop.Loop[workLoop].Loop[orgIndx].nEntry > 1) {
                        reduce_IrLoop(ref graph, currentN, clsLoop, workLoop, orgIndx);
                    }
                }
                curDepth--;
            } while (curDepth > stopDepth);
        }

        //prepare for Concurrency Check and Subgraph making (ONLY REDUCE LOOPS)
        public static void Preprocessing_total_reduceSubgraph(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int workLoop, int curLoop)
        {
            int npLoopS = 0; //number of parent
            int[] pLoopS = new int[clsLoop.Loop[workLoop].nLoop]; //pLoopS => parent(s) of this loop
            find_ParentLoop(clsLoop, workLoop, curLoop, ref pLoopS, ref npLoopS); // return value of pLoops

            int curDepth = clsLoop.Loop[workLoop].maxDepth;            
            //curDepth = 1;
            do {
                for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++) {
                    if (clsLoop.Loop[workLoop].Loop[i].depth != curDepth) continue;
                    int orgIndx = i;

                    if (clsLoop.Loop[workLoop].Loop[orgIndx].depth != curDepth) continue;
                    if (graph.Network[currentN].Node[clsLoop.Loop[workLoop].Loop[orgIndx].header].Name.Substring(0, 1) == "L") continue;
                    if (orgIndx == curLoop) continue;
                    bool bParent = false;
                    for (int k = 0; k < npLoopS; k++) {
                        if (orgIndx == pLoopS[k]) {
                            bParent = true;
                            break;
                        }
                    }
                    if (bParent) continue;

                    //If SESE =>NO REDUCE SESE
                    //if (clsHWLS.FBLOCK.FBlock[i].SESE) {
                    //    reduce_SESE(ref graph, currentN, clsSESE, workSESE, orgIndx);
                    //}

                    //If NL => Make subnetwork and find Dom/ Pdom Behavior relation matrix (2 matrices)
                    if (clsLoop.Loop[workLoop].Loop[orgIndx].nEntry == 1) {
                        reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, orgIndx, "", false);
                    }

                    //if IL => Find CIPd => Make subnetwork with CIPd and find Dom/ Pdom Behavior relation matrix (2 matrices)
                    if (clsLoop.Loop[workLoop].Loop[orgIndx].nEntry > 1) {
                        reduce_IrLoop_Preprocessing(ref graph, currentN, clsLoop, workLoop, orgIndx);
                    }
                }
                curDepth--;
            } while (curDepth > 0);
        }
        //====================================================================================================
        
        public static void prepare_find_Path(GraphVariables.clsGraph graph, int fromN, ref bool[] mark, bool type)
        {
            for (int i = 0; i < graph.Network[fromN].nNode; i++) {
                mark[i] = type;
            }
        }
        public void find_Path(GraphVariables.clsGraph graph, int fromN, GraphVariables.clsLoop clsLoop, int workLoop, int loop, int fromNode, int toNode, 
            ref bool[] mark, ref int[] X, int header, ref int[] searchNode, ref int nSearchNode)
        {
            //must guarantee no EXIT in PdFlow()
            for (int j = 0; j < graph.Network[fromN].Node[fromNode].nPost; j++) {
                int postNode = graph.Network[fromN].Node[fromNode].Post[j];
                if (mark[postNode] == false && gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, postNode, loop))
                {
                    X[fromNode] = postNode;
                    mark[postNode] = true;
                    if (postNode == toNode) {
                        //getResult(ref X, header, toNode, ref searchNode, ref nSearchNode);
                        mark[postNode] = false;
                    }
                    else {
                        mark[postNode] = true;
                        find_Path(graph, fromN, clsLoop, workLoop, loop, postNode, toNode, ref mark, ref X, header, ref searchNode, ref nSearchNode);
                        mark[postNode] = false;
                    }
                }
            }            
        }
        public static void find_Path_CIPd(GraphVariables.clsGraph graph, int currentN, int fromNode, int toNode, ref bool[] mark)
        {
            for (int i = 0; i < graph.Network[currentN].Node[fromNode].nPost; i++) {
                int post = graph.Network[currentN].Node[fromNode].Post[i];
                if (post != toNode) {
                    mark[post] = true;
                    find_Path_CIPd(graph, currentN, post, toNode, ref mark);
                }
            }
        }
        public static void getResult(ref bool[] mark, ref int[] searchNode, ref int nSearchNode) //remove duplicate also
        {

            for (int i = 0; i < mark.Length; i++) {
                if (mark[i] == true) {
                    if (!nodeInSet(searchNode, nSearchNode, i)) {
                        searchNode[nSearchNode] = i;
                        nSearchNode++;
                    }
                }
            }
        }
        public static bool nodeInSet(int[] A, int n, int node)
        {
            for (int i = 0; i < n; i++) {
                if (node == A[i])
                    return true;
            }
            return false;
        }

        public static void find_ParentLoop(GraphVariables.clsLoop clsLoop, int workLoop, int loop, ref int[] pLoopS, ref int npLoopS)
        {
            if (clsLoop.Loop[workLoop].Loop[loop].parentLoop == -1) return;

            pLoopS[npLoopS] = clsLoop.Loop[workLoop].Loop[loop].parentLoop;
            npLoopS++;

            find_ParentLoop(clsLoop, workLoop, clsLoop.Loop[workLoop].Loop[loop].parentLoop, ref pLoopS, ref npLoopS);
        }
    }
}
