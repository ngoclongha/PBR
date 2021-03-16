using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer.Functionalities
{
    class UntanglingIL
    {
        gProAnalyzer.Ultilities.findConcurrencyEntriesIL fndConcurrencyIL;
        gProAnalyzer.Ultilities.findIntersection interSect;
        gProAnalyzer.Ultilities.extendGraph extendG;
        gProAnalyzer.Ultilities.checkGraph checkG;
        gProAnalyzer.Ultilities.clsFindNodeInfo fndNodeInfo;
        gProAnalyzer.Ultilities.makeSubNetwork makSubNet;
        gProAnalyzer.Ultilities.makeInstanceFlow makInstF;
        gProAnalyzer.Ultilities.copyLoop copyL;
        gProAnalyzer.Functionalities.LoopIdentification findLoop;
        gProAnalyzer.Functionalities.NodeSplittingType2 SplitType2;
        gProAnalyzer.Functionalities.NodeSplittingType1 SplitType1;
        public static  void Initialize_All()
        {
            //fndConcurrencyIL = new gProAnalyzer.Ultilities.findConcurrencyEntriesIL();
            //interSect = new gProAnalyzer.Ultilities.findIntersection();
            //extendG = new gProAnalyzer.Ultilities.extendGraph();
            //checkG = new gProAnalyzer.Ultilities.checkGraph();
            //fndNodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
            //makSubNet = new gProAnalyzer.Ultilities.makeSubNetwork();
            //makInstF = new gProAnalyzer.Ultilities.makeInstanceFlow();
            //copyL = new gProAnalyzer.Ultilities.copyLoop();
            //findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
            //SplitType2 = new gProAnalyzer.Functionalities.NodeSplittingType2();
            //SplitType1 = new gProAnalyzer.Functionalities.NodeSplittingType1();
        }

        public static bool make_UntanglingIL(ref GraphVariables.clsGraph graph, int currentN, int untangleNet, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE)
        {
            Initialize_All();

            bool existUntangle = false;
            //scan for all IL and analyze at the lowest lever first.

            graph.Network[untangleNet] = graph.Network[currentN];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, untangleNet, 0, 0); //now work in untangleNet

            gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop, workLoop, clsLoop.untangleLoop);

            int curDepth = clsHWLS.FBLOCK.maxDepth;
            do {
                for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                    if (clsHWLS.FBLOCK.FBlock[i].depth != curDepth || clsHWLS.FBLOCK.FBlock[i].nEntry == 1) continue; //only get IL
                    int orgLoopIndx = clsHWLS.FBLOCK.FBlock[i].refIndex;

                    int newLoopIndx = find_newLoopIndex(graph, untangleNet, clsLoop, workLoop, clsLoop.untangleLoop, orgLoopIndx);

                    gProAnalyzer.Ultilities.findConcurrencyEntriesIL.check_Concurrency(ref graph, untangleNet, graph.conNet, graph.subNet, clsLoop, clsLoop.untangleLoop, newLoopIndx, ref clsSESE); //concurrency[] and concurrencyInst[] were found in clsLoop.

                    //find CID again => 
                    int CID = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID;
                    //we can access searchNode[] in fndConcurrencyIL to get DF => then find boundaries nodes of current IL
                    int[] DFlow_IL_Flow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.DFlow_IL_Flow; ;
                    int nNodeDFlow_IL = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.nNodeDFlow_IL;
                    int[] expand_DFlow_IL_Flow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.expand_DFlow_IL_Flow;
                    int expand_nNodeDFlow_IL = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.expand_nNodeDFlow_IL;

                    int[] DFlow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.DFlow;
                    int nDFlow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.nDFlow;
                    int[] expand_DFlow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.expand_DFlow;
                    int expand_nDFlow = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.expand_nDFlow;

                    int[] boundaryNodes = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.boundaryNodes;//get the boundaryNode of currentN graph.
                    int[] expand_boundaryNode = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.expand_boundaryNodes;

                    //check whether need to untangle the IL or not? => if need -> next step (Check PdFlow? also)
                    if (!check_UntanglingIL(clsLoop, clsLoop.untangleLoop, newLoopIndx)) continue;

                    graph.Network[graph.nickNet] = graph.Network[untangleNet];
                    gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.nickNet, 0, 0); //Using NICKNET for reference of Link
                    remove_DFlow_IL(ref graph, untangleNet, ref clsLoop, clsLoop.untangleLoop, newLoopIndx, expand_boundaryNode, CID, expand_DFlow_IL_Flow, expand_nNodeDFlow_IL); //must use expand version

                    //Next: make_subNet of new Untangling subGraph based on concurrencyInst[] of current IL
                    //Next: plug into original network + expand loops in DFlow (if any) - links/ edges' index should be modified also
                    gProAnalyzer.Ultilities.makeSubNetwork.combine_subGraph_Untangling(ref graph, graph.nickNet, untangleNet, graph.conNet, graph.subNet, graph.dummyNet, clsLoop, clsLoop.untangleLoop, newLoopIndx,
                        expand_boundaryNode, boundaryNodes, CID); //using subNet as referrence    (also //Using NICKNET for reference of Link)                                             

                    //return true;

                    //final step => polishing the subgraph by removing some unnecessary node (gateways only have 1 incoming/ outgoing edge?)
                    //remove disconnected nodes (~ mark Done = true)
                    removing_disconnectedNode(ref graph, untangleNet);

                    existUntangle = true; //At least done 1 untangle of 1 IL
                    //return true;;


                    for (int k = 0; k < graph.Network[untangleNet].nNode; k++) {
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, untangleNet, k);
                    }
                    gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.untangleNet, graph.midNet);
                    gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.untangleLoop, ref clsLoop.IrreducibleError);
                    gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph, untangleNet, graph.untangleNet, ref clsLoop, clsLoop.untangleLoop);
                }
                curDepth--;
            } while (curDepth > 0);

            //find PRE/POST DOM/ PDOM + LOOP + SESE + Node splitting again
            for (int i = 0; i < graph.Network[untangleNet].nNode; i++) {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, untangleNet, i);
            }

            return existUntangle;
        }

        public static bool check_UntanglingIL(GraphVariables.clsLoop clsLoop, int workLoop, int curLoop)
        {
            //if no exclusive entries exist (concurrency = 0) => no Untangling
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].Concurrency.Length; i++) {
                if (clsLoop.Loop[workLoop].Loop[curLoop].Concurrency[i] == 0)
                    return true;
            }
            return false;
        }

        //make an empty space of CID+IL for Untangling the IL latter.
        public static void remove_DFlow_IL(ref GraphVariables.clsGraph graph, int tempNet, ref GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, 
            int[] boundaryNodes, int CID, int[] DFlow_IL_Flow, int nNodeDFlow)
        {
            GraphVariables.clsEdge.strEdge[] imLink = new GraphVariables.clsEdge.strEdge[graph.Network[tempNet].nLink];
            int nImLink = 0;

            //remove the links of  boundary nodes except CID
            for (int i = 0; i < graph.Network[tempNet].nLink; i++) {
                int fromNode = graph.Network[tempNet].Link[i].fromNode;
                int toNode = graph.Network[tempNet].Link[i].toNode;
                if (!(gProAnalyzer.Ultilities.checkGraph.Node_In_Set(DFlow_IL_Flow, nNodeDFlow, fromNode) == true && gProAnalyzer.Ultilities.checkGraph.Node_In_Set(DFlow_IL_Flow, nNodeDFlow, toNode) == true))
                {
                    imLink[nImLink] = graph.Network[tempNet].Link[i];
                    nImLink++;
                }
            }
            //mark DONE for removed nodes
            for (int i = 0; i < graph.Network[tempNet].nNode; i++) {
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(DFlow_IL_Flow, nNodeDFlow, i) && gProAnalyzer.Ultilities.checkGraph.Node_In_Set(boundaryNodes, boundaryNodes.Length, i) == false && graph.Network[tempNet].Node[i].orgNum != CID)
                    graph.Network[tempNet].Node[i].done = true; //need i?
            }
            graph.Network[tempNet].nLink = nImLink;
            graph.Network[tempNet].Link = new GraphVariables.clsEdge.strEdge[nImLink];
            for (int i = 0; i < graph.Network[tempNet].nLink; i++) {
                graph.Network[tempNet].Link[i] = imLink[i];
            }
            //update pre/post of each nodes 
            for (int i = 0; i < graph.Network[tempNet].nNode; i++) {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, tempNet, i);
            }
        }

        //fromN = currentN (as referrence)
        private void make_UntanglingSubGraph(ref GraphVariables.clsGraph graph, int fromN, int toN, ref GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, 
            int[] boundaryNodes, int[] DFlow_IL_Flow, int nNodeDFlow, int[] DFlow, int nDFlow, int orgCID)
        {
            //fill out searchNode of makeSubNetwork (fromN network)
            gProAnalyzer.Ultilities.makeSubNetwork.searchNode = new int[nDFlow];
            gProAnalyzer.Ultilities.makeSubNetwork.nSearchNode = 0;
            int newSubgraph_Index = -1;
            for (int i = 0; i < nDFlow; i++) {
                gProAnalyzer.Ultilities.makeSubNetwork.searchNode[i] = DFlow[i]; //DFlow contain original index of fromN..
                if (DFlow[i] == orgCID)
                    newSubgraph_Index = i;
            }

            //make subNetwork (like SESE)
            gProAnalyzer.Ultilities.makeSubNetwork.make_subNode(ref graph, fromN, toN, false, "", true, "OR"); //only add 1 node to subNetwork
            graph.Network[toN].header = newSubgraph_Index;
            gProAnalyzer.Ultilities.makeSubNetwork.make_subLink(ref graph, fromN, toN, ref clsLoop, workLoop, curLoop, "", false, null, false, null, -1);
            //One more thing. => connect all boundary nodes in DFlow to virtual new "OR" join
            
            //makeSubLink_Untangling(ref graph, fromN, toN, boundaryNodes, orgCID); //multiple end subgraph is allowed

            for (int i = 0; i < graph.Network[toN].nNode; i++) {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, toN, i);
            }

            //============== Do UNTANGLING ===================        (NOW WE HAVE A FULL CLONE SUBGRAPH WITH DFLOW + IL + Boundary nodes => do again everything (include findLoop-reduceLoop)                                                  

            int nConcur = clsLoop.Loop[workLoop].Loop[curLoop].nConcurrency;
            int[] Combine_searchNode = new int[graph.Network[toN].nNode * nConcur];
            int nCombine = 0;

            //for each concurrency Entry set
            for (int i = 0; i < nConcur; i++) {
                //find searchNode by InstanceFlow from CID to cc_Entries (Remember to mark different node for duplication)
                int[] searchNode = makInstF.get_InstanceFlow(ref graph, toN, clsLoop, workLoop, curLoop, i); //CheckAgain
                int nSearchNode = searchNode.Length;
                //combine searchNode with current IL nodes (Remember marking different node for duplication)

                //make_subNetwork for current branch 
                //insert node into nodeList (makeSubNode())
                //insert links into linkList (Using searchNode to makeSubLink()) - (Remember to adjust the node index - e.g traceable node[i].orgNum)
                //connect to/with virtual boundary nodes => when merging to original => just replace virtual boundary nodes
                //do again with other branch =>
            }

            //====Make subLink of all nodes in Untangling subgraph (Remember duplication of nodes and Link index when duplication)

            //find nodeInfo  

            //================================================
        }    

        private int getCID_IL(GraphVariables.clsGraph graph, int fromN, GraphVariables.clsLoop clsLoop, int workLoop, int loop)
        {
            //gProAnalyzer.Ultilities.reduceGraph reduceG = new gProAnalyzer.Ultilities.reduceGraph();            
            int[] calDom = null;
            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++) {
                calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[fromN].nNode, calDom, graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Entry[k]].Dom);
            }

            if (calDom.Length > 0) {
                int CID = -1;
                //pick another CID if current CID
                for (int i = calDom.Length - 1; i > 0; i++)
                    if (gProAnalyzer.Ultilities.reduceGraph.check_CID_In_Loop(clsLoop, workLoop, loop, calDom[i]) == false)
                    {
                        CID = calDom[i];
                    }
                if (CID == -1) {
                    MessageBox.Show("Find_IncludeNode of DFlow error", "Error");
                    return -1; //error
                }
                int header = CID;
                return header;
            }
            return -1;
        }

        public static void removing_disconnectedNode(ref GraphVariables.clsGraph graph, int currentN)
        {
            int[] nodeList = new int[graph.Network[currentN].nNode];
            for (int i = 0; i < nodeList.Length; i++) nodeList[i] = i;

            gProAnalyzer.GraphVariables.clsNode.strNode[] imNode = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[currentN].nNode];
            int nImNode = 0;

            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                if (graph.Network[currentN].Node[i].done == false) {
                    imNode[nImNode] = graph.Network[currentN].Node[i];

                    if (nImNode != i)
                        nodeList[i] = nImNode;

                    nImNode++;

                }
            }

            for (int i = 0; i < nodeList.Length; i++) {
                if (nodeList[i] != i) {
                    int newIndx = nodeList[i];
                    int oldIndex = i;
                    for (int j = 0; j < graph.Network[currentN].nLink; j++) {
                        if (graph.Network[currentN].Link[j].fromNode == oldIndex) {
                            graph.Network[currentN].Link[j].fromNode = newIndx;
                        }
                        if (graph.Network[currentN].Link[j].toNode == oldIndex) {
                            graph.Network[currentN].Link[j].toNode = newIndx;
                        }
                    }
                }
            }

            graph.Network[currentN].nNode = nImNode;
            graph.Network[currentN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[currentN].nNode];
            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                graph.Network[currentN].Node[i] = imNode[i];
            }
        }

        public static int find_newLoopIndex(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int old_workLoop, int new_workLoop, int loopIndx)
        {
            for (int i = 0; i < clsLoop.Loop[new_workLoop].nLoop; i++) {
                if (clsLoop.Loop[new_workLoop].Loop[i].nEntry == clsLoop.Loop[old_workLoop].Loop[loopIndx].nEntry) {
                    int countE = 0;
                    for (int j = 0; j < clsLoop.Loop[new_workLoop].Loop[i].nEntry; j++) {
                        if (graph.Network[currentN].Node[clsLoop.Loop[new_workLoop].Loop[i].Entry[j]].orgNum == clsLoop.Loop[old_workLoop].Loop[loopIndx].Entry[j]) {
                            countE++;
                        }
                        if (countE == clsLoop.Loop[new_workLoop].Loop[i].nEntry) return i;                        
                    }
                    countE = 0;
                }
            }
            return -1;
        }
    }
}
