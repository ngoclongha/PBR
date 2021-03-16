using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer.Ultilities
{
    class findConcurrencyEntriesIL
    {
        gProAnalyzer.Ultilities.copyLoop copyL;
        gProAnalyzer.Ultilities.extendGraph extG;
        gProAnalyzer.Ultilities.reduceGraph reduceG;
        gProAnalyzer.Ultilities.makeSubNetwork makSubNet;
        gProAnalyzer.Ultilities.makeInstanceFlow makInst;
        gProAnalyzer.Ultilities.checkGraph checkG;

        public static int[] DFlow_IL_Flow = null;
        public static int nNodeDFlow_IL = 0;
        public static int[] expand_DFlow_IL_Flow = null;
        public static int expand_nNodeDFlow_IL = 0;

        public static int[] DFlow = null;
        public static int nDFlow = 0;
        public static int[] expand_DFlow = null;
        public static int expand_nDFlow = 0;

        public static int orgCID = -1;
        public static int[] boundaryNodes;
        public static int[] expand_boundaryNodes;

        public static void Initialize_All()
        {
            //copyL = new gProAnalyzer.Ultilities.copyLoop();
            //extG = new gProAnalyzer.Ultilities.extendGraph();
            //reduceG = new gProAnalyzer.Ultilities.reduceGraph();
            //makSubNet = new gProAnalyzer.Ultilities.makeSubNetwork();
            //makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
            //checkG = new gProAnalyzer.Ultilities.checkGraph();
        }

        //Get concurrency combination of IL<entries> from their CID (Output => SubNet)
        public static void check_Concurrency(ref GraphVariables.clsGraph graph, int currentN, int conNet, int subNet, GraphVariables.clsLoop clsLoop, int workLoop, int loop, ref GraphVariables.clsSESE clsSESE)
        {
            Initialize_All();

            //make_ConcurrencyFlow
            graph.Network[conNet] = graph.Network[currentN]; //CIDFlow begining is stored in conNet (Network[8])
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, conNet, 0, 0);

            gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop, workLoop, clsLoop.tempLoop);

            //reduce all loop except "loop" and its parents. (Applied NEW TYPE OF IL REDUCTION) ==>> REDUCE SAME DEPTH??? => ITS OK!!
            gProAnalyzer.Ultilities.reduceGraph.Preprocessing_total_reduceSubgraph(ref graph, conNet, clsLoop, workLoop, loop);

            //make DFlow() + CID in this procedure.
            gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, conNet, subNet, ref clsLoop, clsLoop.tempLoop, loop, ref clsSESE, "CC", -1); //find concurrency set
            //============ (by pass) using "CC" subgraph to find other parameter (just lazy) ==============
            DFlow_IL_Flow = null;
            nNodeDFlow_IL = 0;
            expand_DFlow_IL_Flow = null; //combine DFlow+IL and expand nested loops (if any)
            expand_nNodeDFlow_IL = 0;

            DFlow = null;
            nDFlow = 0;
            expand_DFlow = null;
            expand_nDFlow = 0;

            orgCID = getCID_IL(graph, conNet, clsLoop, workLoop, loop); //CID of original graph index (All entries)

            expand_boundaryNodes = getBoundaryNode(graph, conNet, subNet, clsLoop, workLoop, loop, ref expand_DFlow_IL_Flow, ref expand_nNodeDFlow_IL, ref expand_DFlow, ref expand_nDFlow, true);
            boundaryNodes = getBoundaryNode(graph, conNet, subNet, clsLoop, workLoop, loop, ref DFlow_IL_Flow, ref nNodeDFlow_IL, ref DFlow, ref nDFlow, false);
            //==============================================================================================

            gProAnalyzer.Ultilities.makeSubNetwork.make_subGraph_DFlow(ref graph, conNet, subNet, ref clsLoop, clsLoop.tempLoop, loop, DFlow, nDFlow, boundaryNodes, orgCID); //subgraph contains DFlow + boundaryNode

            //========>>>>> NEED FIX THE CONCURRENT ENTRY SET IDENTIFICATION => Recording all set, not optimize anything
            make_ConcurrencyInstance(graph, subNet, ref clsLoop, clsLoop.tempLoop, loop); //find concurrency entry sets using DFlow() => store in clsLoop.
            //========>>>>> MUST FIX <<<<<============

            //some ERRORS here, check it for QUERY paper (Next paper)
            make_ConcurrencyInstance_Untangling(ref graph, subNet, ref clsLoop, clsLoop.tempLoop, loop); //find the instanceNode to each concurrencySet => store in clsLoop also

            //copy concurrency inform =============== THIS IS FOR THE VERIFICATION PAPER ONLY
            clsLoop.Loop[workLoop].Loop[loop].nConcurrency = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].nConcurrency; //tranfer all the concurrency value from tempLoop [3] to workloop [2]
            if (clsLoop.Loop[clsLoop.tempLoop].Loop[loop].Concurrency != null) {
                clsLoop.Loop[workLoop].Loop[loop].Concurrency = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++)
                    clsLoop.Loop[workLoop].Loop[loop].Concurrency[k] = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].Concurrency[k];
            }

            //copy InstanceNode_DFlow to workLoop - Concurrent Entry
            clsLoop.Loop[workLoop].Loop[loop].nConcurrInst = new int[clsLoop.Loop[clsLoop.tempLoop].Loop[loop].nConcurrInst.Length]; //tranfer all the concurrency value from tempLoop [3] to workloop [2]
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nConcurrInst.Length; i++)
                clsLoop.Loop[workLoop].Loop[loop].nConcurrInst[i] = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].nConcurrInst[i];

            if (clsLoop.Loop[clsLoop.tempLoop].Loop[loop].concurrInst != null) {
                clsLoop.Loop[workLoop].Loop[loop].concurrInst = new int[clsLoop.Loop[workLoop].Loop[loop].nConcurrInst.Length][];
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nConcurrInst.Length; k++) {
                    if (clsLoop.Loop[clsLoop.tempLoop].Loop[loop].concurrInst[k] == null) continue;
                    clsLoop.Loop[workLoop].Loop[loop].concurrInst[k] = new int[clsLoop.Loop[clsLoop.tempLoop].Loop[loop].concurrInst[k].Length];
                    for (int l = 0; l < clsLoop.Loop[workLoop].Loop[loop].nConcurrInst[k]; l++)
                        clsLoop.Loop[workLoop].Loop[loop].concurrInst[k][l] = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].concurrInst[k][l];
                }
            }

            //copy InstanceNode_DFlow to workLoop - Exclusive entries
            clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst = new int[clsLoop.Loop[clsLoop.tempLoop].Loop[loop].nExclusiveInst.Length]; //tranfer all the concurrency value from tempLoop [3] to workloop [2]
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst.Length; i++)
                clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst[i] = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].nExclusiveInst[i];

            if (clsLoop.Loop[clsLoop.tempLoop].Loop[loop].exclusiveInst != null) {
                clsLoop.Loop[workLoop].Loop[loop].exclusiveInst = new int[clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst.Length][];
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst.Length; k++) {
                    if (clsLoop.Loop[clsLoop.tempLoop].Loop[loop].exclusiveInst[k] == null) continue;
                    clsLoop.Loop[workLoop].Loop[loop].exclusiveInst[k] = new int[clsLoop.Loop[clsLoop.tempLoop].Loop[loop].exclusiveInst[k].Length];
                    for (int l = 0; l < clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst[k]; l++)
                        clsLoop.Loop[workLoop].Loop[loop].exclusiveInst[k][l] = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].exclusiveInst[k][l];
                }
            }
            //output is the model in Network.SUBNET
            //output: Get Concurrency[] and ConcurrencyInst of DFlow().
        }

        public static void make_ConcurrencyInstance(GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsLoop clsLoop, int workLoop, int loop) //Must check here !!!!!!!!!!!!
        {
            //INPUT: A acyclic graph with single start 
            //OUTPUT: Mark into CONCURRENCY field of the clsLoop.Loop[workLoop].Loop[loop];

            int conNum = 0;
            int[][] conEntry = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry][];
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nEntry; i++) {
                conEntry[i] = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];
            }

            //set the initiated value for SearchXOR, nSearch, nCurrentXOR
            int[] SearchXOR = new int[graph.Network[currentN].nNode]; // 0-탐색 //navigation??
            int nSearchXOR = 0;
            int nCurrentXOR = 0;

            do {
                nCurrentXOR = 0;
                for (int j = 0; j < graph.Network[currentN].nLink; j++) graph.Network[currentN].Link[j].bInstance = false;

                int[] InstantNode = new int[graph.Network[currentN].nNode];
                int nInstantNode = 0;
                string[] strCombination_OR = new string[graph.Network[currentN].nNode];
                int sNode = graph.Network[currentN].header;
                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                //Instant Flow 찾으면
                //SearchXOR[] will be use in here //instanceNode() => will find the path of each instance flow (i.g. 1->4->5->8)
                if (gProAnalyzer.Ultilities.makeInstanceFlow.find_InstanceNode(ref graph, currentN, sNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR))
                {
                    //find
                    int[] imEntry = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];
                    bool isError = true;
                    for (int i = 0; i < nInstantNode; i++) {
                        for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                            if (graph.Network[currentN].Node[InstantNode[i]].orgNum == clsLoop.Loop[workLoop].Loop[loop].Entry[j]) {
                                imEntry[j] = 1; //import entry cua node thu j (ko phai node j) = 1 sau nay no co the = 2 3 4 ...
                                isError = false;
                                break;
                            }
                        }
                    }
                    if (!isError) {
                        //Same Check
                        int sameCon = 0; //sameConcurrent??
                        bool[] kCon = new bool[conNum];
                        for (int k = 0; k < conNum; k++) {
                            for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                                if (imEntry[j] == 1 && conEntry[k][j] == 1) {
                                    sameCon++;
                                    kCon[k] = true;
                                    break;
                                }
                            }
                        }
                        if (sameCon == 0) {
                            conEntry[conNum] = imEntry;
                            conNum++;
                        }
                        else {
                            int conNum_T = conNum;
                            int[][] conEntry_T = new int[conNum][];
                            for (int k = 0; k < conNum_T; k++) {
                                conEntry_T[k] = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];
                                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                                    conEntry_T[k][j] = conEntry[k][j];
                                }
                            }
                            conNum = 0;
                            for (int k = 0; k < conNum_T; k++) {
                                if (kCon[k]) continue;
                                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                                    conEntry[conNum][j] = conEntry[k][j];
                                }
                                conNum++;
                            }
                            for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                                conEntry[conNum][j] = 0;
                                if (imEntry[j] == 1) {
                                    conEntry[conNum][j] = 1;
                                }
                                for (int k = 0; k < conNum_T; k++) {
                                    if (!kCon[k]) continue;
                                    if (conEntry_T[k][j] == 1) {
                                        conEntry[conNum][j] = 1;
                                    }
                                }
                            }
                            conNum++;
                        }
                    }
                } // if error
            } while (nSearchXOR > 0);

            //here => what I need to know //number of Concurrency = number of Loop Entry
            clsLoop.Loop[workLoop].Loop[loop].Concurrency = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];

            int numType = 1;
            for (int k = 0; k < conNum; k++) {
                int cntTrue = 0;
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                    if (conEntry[k][j] == 1) cntTrue++;
                }
                if (cntTrue > 1) {
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[loop].nEntry; j++) {
                        if (conEntry[k][j] == 1) {
                            clsLoop.Loop[workLoop].Loop[loop].Concurrency[j] = numType;
                        }
                    }
                    numType++;
                }
            }
            clsLoop.Loop[workLoop].Loop[loop].nConcurrency = numType - 1;
        }

        public static void make_ConcurrencyInstance_Untangling(ref GraphVariables.clsGraph graph, int tempNet, ref GraphVariables.clsLoop clsLoop, int workLoop, int loop)
        {
            //set the initiated value for SearchXOR, nSearch, nCurrentXOR
            int[] SearchXOR = new int[graph.Network[tempNet].nNode]; // 0-탐색 //navigation??
            int nSearchXOR = 0;
            int nCurrentXOR = 0;
            clsLoop.Loop[workLoop].Loop[loop].nConcurrInst = new int[clsLoop.Loop[workLoop].Loop[loop].nConcurrency + 1]; //+1 for compatable of concurrencyIndex (loop)
            clsLoop.Loop[workLoop].Loop[loop].concurrInst = new int[clsLoop.Loop[workLoop].Loop[loop].nConcurrency + 1][];

            clsLoop.Loop[workLoop].Loop[loop].nExclusiveInst = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];
            clsLoop.Loop[workLoop].Loop[loop].exclusiveInst = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry][];

            do {
                nCurrentXOR = 0;
                for (int j = 0; j < graph.Network[tempNet].nLink; j++) graph.Network[tempNet].Link[j].bInstance = false;

                int[] InstantNode = new int[graph.Network[tempNet].nNode];
                int nInstantNode = 0;
                string[] strCombination_OR = new string[graph.Network[tempNet].nNode];
                int sNode = graph.Network[tempNet].header;
                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                //Instant Flow
                //SearchXOR[] will be use in here //instanceNode() => will find the path of each instance flow (i.g. 1->4->5->8)
                if (gProAnalyzer.Ultilities.makeInstanceFlow.find_InstanceNode(ref graph, tempNet, sNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR))
                {
                    string[] readable = new string[nInstantNode];
                    readable = convert_Readable(ref graph, tempNet, InstantNode, nInstantNode);

                    for (int k = 0; k <= clsLoop.Loop[workLoop].Loop[loop].nConcurrency; k++) {
                        int countE = 0;
                        int countInst = 0;
                        for (int m = 0; m < clsLoop.Loop[workLoop].Loop[loop].nEntry; m++) {
                            if (clsLoop.Loop[workLoop].Loop[loop].Concurrency[m] != k) continue;
                            countE++;
                            int curEntry = get_NewNodeIndex(graph, tempNet, clsLoop.Loop[workLoop].Loop[loop].Entry[m]);
                            if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(InstantNode, nInstantNode, curEntry) == true)
                            {
                                countInst++;
                                if (k == 0) {
                                    add_to_exclusiveInst(graph, tempNet, ref clsLoop, workLoop, loop, m, InstantNode, nInstantNode);
                                }
                            }
                        }
                        if (countE == countInst) {
                            //store InstanceNode
                            //set Union of all instance node 
                            add_to_concurrInst(graph, tempNet, ref clsLoop, workLoop, loop, k, InstantNode, nInstantNode);
                        }
                        if (true) {                            
                        }
                    }
                }
            } while (nSearchXOR > 0);
        }

        public static void add_to_concurrInst(GraphVariables.clsGraph graph, int tempNet, ref GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, int cur_concurrency, int[] InstantNode, int nInstantNode)
        {
            int temp_nExclusive = clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency];
            int[] temp_exclusiveInst = new int[temp_nExclusive];

            temp_exclusiveInst = clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[cur_concurrency];

            clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency] = 0;
            clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[cur_concurrency] = new int[nInstantNode];

            for (int j = 0; j < nInstantNode; j++) {
                int orgInstNode = graph.Network[tempNet].Node[InstantNode[j]].orgNum;
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[cur_concurrency], clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency], orgInstNode) == false)
                {
                    if (clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency] < 1)
                        clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency] = 0;

                    int instIndex = clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency];
                    clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[cur_concurrency][instIndex] = orgInstNode;
                    instIndex++;
                    clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency] = instIndex;
                }
            }

            int temp_nExclusive_2 = clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency];
            int[] temp_exclusiveInst_2 = new int[temp_nExclusive_2];
            temp_exclusiveInst_2 = clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[cur_concurrency];

            int[] totalTemp = new int[temp_nExclusive_2 + temp_nExclusive];
            int nTotal = 0;

            for (int i = 0; i < temp_nExclusive; i++) {
                totalTemp[nTotal] = temp_exclusiveInst[i];
                nTotal++;
            }
            for (int i = 0; i < temp_nExclusive_2; i++) {
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(totalTemp, nTotal, temp_exclusiveInst_2[i]) == false)
                {
                    totalTemp[nTotal] = temp_exclusiveInst_2[i];
                    nTotal++;
                }
            }
            //cleaning array
            int[] newArray = new int[nTotal];
            for (int i = 0; i < nTotal; i++) {
                newArray[i] = totalTemp[i];
            }

            clsLoop.Loop[workLoop].Loop[curLoop].nConcurrInst[cur_concurrency] = nTotal;
            clsLoop.Loop[workLoop].Loop[curLoop].concurrInst[cur_concurrency] = newArray;
        }

        public static void add_to_exclusiveInst(GraphVariables.clsGraph graph, int tempNet, ref GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, int curEnIndx, int[] InstantNode, int nInstantNode)
        {
            int temp_nExclusive = clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx];
            int[] temp_exclusiveInst = new int[temp_nExclusive];
            
            temp_exclusiveInst = clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx];

            clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx] = 0;
            clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx] = new int[nInstantNode];

            for (int j = 0; j < nInstantNode; j++) {
                int orgInstNode = graph.Network[tempNet].Node[InstantNode[j]].orgNum;
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx], clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx], orgInstNode) == false)
                {
                    if (clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx] < 1)
                        clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx] = 0;

                    int instIndex = clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx];
                    clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx][instIndex] = orgInstNode;
                    instIndex++;
                    clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx] = instIndex;
                }
            }

            int temp_nExclusive_2 = clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx];
            int[] temp_exclusiveInst_2 = new int[temp_nExclusive_2];
            temp_exclusiveInst_2 = clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx];

            int[] totalTemp = new int[temp_nExclusive_2 + temp_nExclusive];
            int nTotal = 0;

            for (int i = 0; i < temp_nExclusive; i++) {
                totalTemp[nTotal] = temp_exclusiveInst[i];
                nTotal++;
            }
            for (int i = 0; i < temp_nExclusive_2; i++) {
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(totalTemp, nTotal, temp_exclusiveInst_2[i]) == false)
                {
                    totalTemp[nTotal] = temp_exclusiveInst_2[i];
                    nTotal++;
                }
            }
            //cleaning array
            int[] newArray = new int[nTotal];
            for (int i = 0; i < nTotal; i++) {
                newArray[i] = totalTemp[i];
            }

            clsLoop.Loop[workLoop].Loop[curLoop].nExclusiveInst[curEnIndx] = nTotal;
            clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx] = newArray;
            //clsLoop.Loop[workLoop].Loop[curLoop].exclusiveInst[curEnIndx] = 

        }

        public static int get_NewNodeIndex(GraphVariables.clsGraph graph, int currentN, int node)
        {
            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                if (graph.Network[currentN].Node[i].orgNum == node)
                    return i;
            }
            return -1;
        }

        public static int[] getBoundaryNode(GraphVariables.clsGraph graph, int finalNet, int subNet, GraphVariables.clsLoop clsLoop, int workLoop, int curLoop,
            ref int[] tempSet, ref int nTempSet, ref int[] DFlow, ref int nDFlow, bool CheckCombineLoops)
        {
            DFlow = new int[graph.Network[finalNet].nNode];
            nDFlow = 0;
            tempSet = new int[graph.Network[finalNet].nNode];
            nTempSet = 0;
            int[] listCID_Loop = new int[clsLoop.Loop[workLoop].nLoop]; //index of reduced loops
            int nList = 0;

            for (int i = 0; i < graph.Network[subNet].nNode; i++) {
                if (graph.Network[subNet].Node[i].orgNum != -1) {
                    tempSet[nTempSet] = graph.Network[subNet].Node[i].orgNum;
                    nTempSet++;
                    DFlow[nDFlow] = graph.Network[subNet].Node[i].orgNum;
                    nDFlow++;
                    if (graph.Network[subNet].Node[i].header == true) {
                        listCID_Loop[nList] = graph.Network[subNet].Node[i].headerOfLoop;
                        nList++;
                    }
                }
            }

            if (CheckCombineLoops) {
                //combine tempSet + Loops (Expanded from reduced before)
                for (int i = 0; i < nList; i++) {
                    for (int j = 0; j < clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].nNode; j++) {
                        if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].Node[j]) == true) continue;
                        tempSet[nTempSet] = clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].Node[j];
                        nTempSet++;
                        DFlow[nDFlow] = clsLoop.Loop[workLoop].Loop[listCID_Loop[i]].Node[j];
                        nDFlow++;
                    }
                }
            }

            //combine tempSet + current IL
            if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, clsLoop.Loop[workLoop].Loop[curLoop].header) == false)
            {
                tempSet[nTempSet] = clsLoop.Loop[workLoop].Loop[curLoop].header;
                nTempSet++;
            }  
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nNode; i++) {
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, clsLoop.Loop[workLoop].Loop[curLoop].Node[i]) == false)
                {
                    tempSet[nTempSet] = clsLoop.Loop[workLoop].Loop[curLoop].Node[i];
                    nTempSet++;
                }
            }

            int[] boundarySet = new int[nTempSet];
            int nBoundary = 0;
            for (int i = 0; i < nTempSet; i++) {
                int node = tempSet[i];
                for (int j = 0; j < graph.Network[finalNet].nLink; j++)
                    if (graph.Network[finalNet].Link[j].fromNode == node) //boundary nodes is always split-node
                        if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(tempSet, nTempSet, graph.Network[finalNet].Link[j].toNode) == false)
                        {
                            boundarySet[nBoundary] = node;
                            nBoundary++;
                        }
            }

            int[] returnSet = new int[nBoundary];
            for (int i = 0; i < nBoundary; i++) returnSet[i] = boundarySet[i];
            return returnSet;
        }

        public static int getCID_IL(GraphVariables.clsGraph graph, int fromN, GraphVariables.clsLoop clsLoop, int workLoop, int loop)
        {
            //gProAnalyzer.Ultilities.findIntersection interSect = new gProAnalyzer.Ultilities.findIntersection();
            //gProAnalyzer.Ultilities.reduceGraph reduceG = new gProAnalyzer.Ultilities.reduceGraph();

            int[] calDom = null;
            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++) {
                calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[fromN].nNode, calDom, graph.Network[fromN].Node[clsLoop.Loop[workLoop].Loop[loop].Entry[k]].Dom);
            }

            if (calDom.Length > 0) {
                int CID = -1;
                //pick another CID if current CID
                for (int i = calDom.Length - 1; i > -1; i--)
                    if (gProAnalyzer.Ultilities.reduceGraph.check_CID_In_Loop(clsLoop, workLoop, loop, calDom[i]) == false)
                    {
                        CID = calDom[i];
                        break;
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

        public static string[] convert_Readable(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] InstantNode, int nInstantNode)
        {
            string[] readable = new string[nInstantNode];

            for (int i = 0; i < nInstantNode; i++) {
                readable[i] = graph.Network[currentN].Node[InstantNode[i]].Name;
            }
            return (readable);
        }
    }
}
