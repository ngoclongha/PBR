using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;

namespace gProAnalyzer.Functionalities
{
    class SESEIdentification
    {
        public static int[][] adjList = null;
        public static int[,] AjM_Network;
        public static int[,] AjM_Network_Undirected;
        public static int[][] adjList_Temp = null;
        public static bool[,] makeLinkDomTree;
        public static bool[,] makeLinkPdomTree;
        //private bool[,] makeLink_eDomTree; //?
        //private bool[,] makeLink_ePdomTree; //?
        public static int[,] checkEdges;
        public static int maxDepth_DomTree;
        public static int maxDepth_PdomTree;

        public static System.Diagnostics.Stopwatch watch_detail = new System.Diagnostics.Stopwatch();
        public static double detail_watch = 0;
        //public static gProAnalyzer.Ultilities.mappingGraph mapping;
        //public static gProAnalyzer.Preprocessing.clsExtendNetwork extNet;
        //public static gProAnalyzer.Ultilities.searchGraph searchG;
        //public static gProAnalyzer.Ultilities.findIntersection fndIntersect;
        //public static gProAnalyzer.Ultilities.findUnion fndUnion;
        //public static gProAnalyzer.Ultilities.checkGraph check;
        //public static gProAnalyzer.Ultilities.reduceGraph reduceG;

        public static void Initialize_All(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, bool withLoop)
        {
            //mapping = new gProAnalyzer.Ultilities.mappingGraph();
            //extNet = new gProAnalyzer.Preprocessing.clsExtendNetwork();
            //searchG = new gProAnalyzer.Ultilities.searchGraph();
            //fndIntersect = new gProAnalyzer.Ultilities.findIntersection();
            //check = new gProAnalyzer.Ultilities.checkGraph();
            //reduceG = new gProAnalyzer.Ultilities.reduceGraph();
            //fndUnion = new gProAnalyzer.Ultilities.findUnion();

            // preprocessing data for SESE identify
            clsSESE.SESE[currentSESE] = new gProAnalyzer.GraphVariables.clsSESE.strSESE();
            clsSESE.SESE[currentSESE].nSESE = 0;

            //Convert to adjacency list (faster computation)
            gProAnalyzer.Ultilities.mappingGraph.to_adjList(ref graph, currentN, ref adjList);
            gProAnalyzer.Ultilities.mappingGraph.to_adjList(ref graph, currentN, ref adjList_Temp);

            gProAnalyzer.Ultilities.mappingGraph.to_adjacencyMatrix(ref graph, currentN, ref AjM_Network, ref graph.Network[currentN].nNode);
            gProAnalyzer.Ultilities.mappingGraph.to_adjacencyMatrix_Undirected(ref graph, currentN, ref AjM_Network_Undirected, ref graph.Network[currentN].nNode);

            //LinkSESE = new strLink[Network[currentN].nLink];

            //==Make DomTree for identifying the candidate exit //Copy from original code (MAKE DOOM/POSTDOM TREE)
            int nNode = graph.Network[currentN].nNode;
            makeLinkDomTree = new bool[nNode, nNode];
            int nLink = 0;
            for (int i = 0; i < nNode; i++)
            {
                for (int k = 1; k < graph.Network[currentN].Node[i].nDom; k++)
                {
                    makeLinkDomTree[graph.Network[currentN].Node[i].Dom[k - 1], graph.Network[currentN].Node[i].Dom[k]] = true;
                    nLink++;
                }
            }
            // Make Postdom Tree
            makeLinkPdomTree = new bool[nNode, nNode];
            int nLink2 = 0;
            for (int i = 0; i < nNode; i++)
            {
                for (int k = 1; k < graph.Network[currentN].Node[i].nDomRev; k++)
                {
                    makeLinkPdomTree[graph.Network[currentN].Node[i].DomRev[k - 1], graph.Network[currentN].Node[i].DomRev[k]] = true;
                    nLink2++;
                }
            }

            //reduce Loop in model (for easier to identify which node is in or out the loop) => store in [reduceTempNet]
            graph.Network[graph.reduceTempNet] = graph.Network[currentN];
            gProAnalyzer.Preprocessing.clsExtendNetwork.extent_Network(ref graph, graph.reduceTempNet, 0);

            if (withLoop == true) {
                //copy_Loop(orgLoop, reduceLoop);
                int curDepth = clsLoop.Loop[clsLoop.orgLoop].maxDepth;
                do {
                    for (int i = 0; i < clsLoop.Loop[clsLoop.orgLoop].nLoop; i++) {
                        if (clsLoop.Loop[clsLoop.orgLoop].Loop[i].depth != curDepth) continue;
                        //reduceLoop => store in reduceNetwork
                        gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, graph.reduceTempNet, ref clsLoop, clsLoop.orgLoop, i, "", false);
                    }
                    curDepth--;
                } while (curDepth > 0);
            }

            maxDepth_DomTree = 0;
            maxDepth_PdomTree = 0;
            //BFS and get the depth of EntrySESE or ExitSESE
            maxDepth_DomTree = gProAnalyzer.Ultilities.searchGraph.get_depthBFS(ref graph, currentN, makeLinkDomTree, true);
            maxDepth_PdomTree = gProAnalyzer.Ultilities.searchGraph.get_depthBFS(ref graph, currentN, makeLinkPdomTree, false);
        }

        public static void find_SESE_WithLoop(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int orgLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int nodeSS)
        {
            Initialize_All(ref graph, currentN, ref clsLoop, ref clsSESE, currentSESE, true);
            //find SESE
            //build loop-nesting forest
            //build 
            //from bottom-up layer             
                //=> identify SESE which node inside the loop (except loop construct ~ split and join only)
                //=>intersection dom(-1) and pdom(-1)
                //=> identify SESE which node outside the loop 
                //if NL have multiple exit => header = entrySESE; the exitSESE only in the next layer of NL (join) (CIPd of NL exits) or IL having single exit in this layer (2 option only) ===>> with 2 option, we must choice which one identify first to avoid the case: NL have CIPd is the CID of IL in the same layer!!!
                //if IL have single exit => IL exit = exitSESE; the entrySESE only in the next layer of IL (split) or or header of NL having multiple exit in this layer            

            bool[] reduceSESE = new bool[graph.Network[currentN].nNode];

            //transfer_AdjacencyMatrix(currentN, ref AjM_Network, ref graph.Network[currentN].nNode);

            checkEdges = new int[graph.Network[currentN].nNode, graph.Network[currentN].nNode]; //mark which edges belong to different SESE

            int[] entrySESE = new int[graph.Network[currentN].nNode]; //store current candidate entries of SESE
            int nEntrySESE = 0;
            int[] exitSESE = new int[graph.Network[currentN].nNode]; //store current candidate exits of SESE
            int nExitSESE = 0;
            bool NL = false;
            int curDepth;

            //initiate SESE
            clsSESE.SESE[currentSESE] = new gProAnalyzer.GraphVariables.clsSESE.strSESE();

            if (nodeSS == -2)
            {
                //Reset counting variable to 0
                //lemma2_C = 0;
                //prop4_C = 0;
                //lemma3_C = 0;
                //lemma4_C = 0;
                //lemma5_C = 0;
            }

            if (clsLoop.Loop[orgLoop].nLoop != 0 && nodeSS == -2) {
                curDepth = clsLoop.Loop[orgLoop].maxDepth;
                do {
                    for (int i = 0; i < clsLoop.Loop[orgLoop].nLoop; i++) {
                        if (clsLoop.Loop[orgLoop].Loop[i].depth != curDepth) continue;
                        {
                            //OK we will work here======
                            bool checkIL = false;
                            if (clsLoop.Loop[orgLoop].Loop[i].nEntry == 1) NL = true;
                            if (clsLoop.Loop[orgLoop].Loop[i].nEntry > 1) NL = false;
                            entrySESE = new int[graph.Network[currentN].nNode]; //store current candidate entries of SESE
                            nEntrySESE = 0;
                            exitSESE = new int[graph.Network[currentN].nNode]; //store current candidate exits of SESE
                            nExitSESE = 0;

                            //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                            get_arrange_CandEnt_CandExt(ref graph, currentN, graph.reduceTempNet, ref clsLoop, orgLoop, i, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
                            if (nEntrySESE > 0 && nExitSESE > 0) {
                                identifySESE_NewApproach(ref graph, currentN, ref clsSESE, currentSESE, ref clsLoop, orgLoop, i, NL, true, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE);
                            }
                            //If Loop = NL => if nExits > 1 => find CIPd(exits) ~ CIPd must inside the next layer or single exit of ILs of this layer ONLY. => break()
                            if (NL) {
                                if (clsLoop.Loop[orgLoop].Loop[i].nExit > 1) {
                                    int CIPd_exits = -1;
                                    #region find CIPd(exits)
                                    int[] calDom = null;
                                    for (int k = 0; k < clsLoop.Loop[orgLoop].Loop[i].nExit; k++) {
                                        calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDom, graph.Network[currentN].Node[clsLoop.Loop[orgLoop].Loop[i].Exit[k]].DomRev);
                                    }
                                    if (calDom.Length > 0) {
                                        bool check_CIPd = false;
                                        for (int cal = 0; cal < calDom.Length; cal++) //find until have the suitable CIPd
                                        {
                                            if (check_CIPd == false) CIPd_exits = calDom[cal];

                                            //CIPd_exits = header;

                                            int parent_i = clsLoop.Loop[orgLoop].Loop[i].parentLoop;
                                            //check CIPd(exits) in parent or not                                            
                                            if (parent_i != -1) {
                                                for (int k = 0; k < clsLoop.Loop[orgLoop].Loop[parent_i].nNode; k++) {
                                                    if (clsLoop.Loop[orgLoop].Loop[parent_i].Node[k] == CIPd_exits) { check_CIPd = true; break; }

                                                }
                                                if (check_CIPd == false) {
                                                    for (int il = 0; il < clsLoop.Loop[orgLoop].Loop[parent_i].nChild; il++) {
                                                        if (il != i) {
                                                            if (clsLoop.Loop[orgLoop].Loop[il].nEntry > 1 && clsLoop.Loop[orgLoop].Loop[il].nExit == 1)
                                                                if (CIPd_exits == clsLoop.Loop[orgLoop].Loop[il].Exit[0]) { check_CIPd = true; break; }
                                                        }
                                                    }
                                                    if (check_CIPd) break;
                                                }
                                            }
                                            else //there are no loop outside
                                            {
                                                for (int k = 0; k < graph.Network[currentN].nNode; k++) {
                                                    if (graph.Network[graph.reduceTempNet].Node[k].nPost > 0 && graph.Network[graph.reduceTempNet].Node[k].nPre > 0)
                                                        if (CIPd_exits == k && !gProAnalyzer.Ultilities.checkGraph.isLoopEntries(ref clsLoop, orgLoop, k)) { check_CIPd = true; break; }
                                                }
                                                if (check_CIPd == false) {
                                                    for (int j = 0; j < clsLoop.Loop[orgLoop].nLoop; j++) {
                                                        if (clsLoop.Loop[orgLoop].Loop[j].depth == 1 && i != j) {
                                                            if (clsLoop.Loop[orgLoop].Loop[j].nEntry > 1 && clsLoop.Loop[orgLoop].Loop[j].nExit == 1)
                                                                if (CIPd_exits == clsLoop.Loop[orgLoop].Loop[j].Exit[0]) { check_CIPd = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (check_CIPd) {
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
                                            clsLoop.Loop[orgLoop].Loop[i].etnCandEn = new int[1];
                                            clsLoop.Loop[orgLoop].Loop[i].nEtnCandEn = 0;
                                            clsLoop.Loop[orgLoop].Loop[i].etnCandEn[clsLoop.Loop[orgLoop].Loop[i].nEtnCandEn] = clsLoop.Loop[orgLoop].Loop[i].header;
                                            clsLoop.Loop[orgLoop].Loop[i].nEtnCandEn++;

                                            clsLoop.Loop[orgLoop].Loop[i].etnCandEx = new int[1];
                                            clsLoop.Loop[orgLoop].Loop[i].nEtnCandEx = 0;
                                            clsLoop.Loop[orgLoop].Loop[i].etnCandEx[clsLoop.Loop[orgLoop].Loop[i].nEtnCandEx] = CIPd_exits;
                                            clsLoop.Loop[orgLoop].Loop[i].nEtnCandEx++;
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
                                //else lemma3_C++;
                            }
                            //If loop = IL => if nExits = 1 => find CID(entries) ~ CID must inside the next layer or header of NL of this layer only => break()
                            else {
                                if (clsLoop.Loop[orgLoop].Loop[i].nExit == 1) {
                                    int CID_entries = -1;
                                    #region find CID(entries)
                                    int[] calDom = null;
                                    for (int k = 0; k < clsLoop.Loop[orgLoop].Loop[i].nEntry; k++) {
                                        calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDom, graph.Network[currentN].Node[clsLoop.Loop[orgLoop].Loop[i].Entry[k]].Dom);
                                    }
                                    if (calDom.Length > 0) {
                                        bool check_CID = false;
                                        for (int cal = calDom.Length - 1; cal > 0; cal--) {
                                            if (check_CID == false) CID_entries = calDom[cal];
                                            else break;

                                            int parent_i = clsLoop.Loop[orgLoop].Loop[i].parentLoop;
                                            //check CID(entries) in parent or not

                                            if (parent_i != -1) //if it have parent loop
                                            {
                                                for (int k = 0; k < clsLoop.Loop[orgLoop].Loop[parent_i].nNode; k++) {
                                                    if (clsLoop.Loop[orgLoop].Loop[parent_i].Node[k] == CID_entries) { check_CID = true; break; }
                                                }
                                                if (check_CID == false) {
                                                    for (int nl = 0; nl < clsLoop.Loop[orgLoop].Loop[parent_i].nChild; nl++) {
                                                        if (nl != i) {
                                                            if (clsLoop.Loop[orgLoop].Loop[nl].nEntry == 1 && clsLoop.Loop[orgLoop].Loop[nl].nExit > 1)
                                                                if (CID_entries == clsLoop.Loop[orgLoop].Loop[nl].header) { check_CID = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                            else //if there are no loop outside
                                            {
                                                for (int k = 0; k < graph.Network[currentN].nNode; k++) {
                                                    if (graph.Network[graph.reduceTempNet].Node[k].nPost > 0 && graph.Network[graph.reduceTempNet].Node[k].nPre > 0)
                                                        if (CID_entries == k && !gProAnalyzer.Ultilities.checkGraph.isLoopExits(ref clsLoop, orgLoop, k)) { check_CID = true; break; }
                                                }
                                                if (check_CID == false) {
                                                    for (int j = 0; j < clsLoop.Loop[orgLoop].nLoop; j++) {
                                                        if (clsLoop.Loop[orgLoop].Loop[j].depth == 1 && i != j) {
                                                            if (clsLoop.Loop[orgLoop].Loop[j].nEntry == 1 && clsLoop.Loop[orgLoop].Loop[j].nExit > 1)
                                                                if (CID_entries == clsLoop.Loop[orgLoop].Loop[j].header) { check_CID = true; break; }
                                                        }
                                                    }
                                                }
                                            }
                                            if (check_CID) {
                                                //store 2 nodes - 1 from the loop 1 from outside the loop => loop.etnCandEn and loop.etnCandEx
                                                clsLoop.Loop[orgLoop].Loop[i].etnCandEn = new int[1];
                                                clsLoop.Loop[orgLoop].Loop[i].nEtnCandEn = 0;
                                                clsLoop.Loop[orgLoop].Loop[i].etnCandEn[clsLoop.Loop[orgLoop].Loop[i].nEtnCandEn] = CID_entries;
                                                clsLoop.Loop[orgLoop].Loop[i].nEtnCandEn++;

                                                clsLoop.Loop[orgLoop].Loop[i].etnCandEx = new int[1];
                                                clsLoop.Loop[orgLoop].Loop[i].nEtnCandEx = 0;
                                                clsLoop.Loop[orgLoop].Loop[i].etnCandEx[clsLoop.Loop[orgLoop].Loop[i].nEtnCandEx] = clsLoop.Loop[orgLoop].Loop[i].Exit[0];
                                                clsLoop.Loop[orgLoop].Loop[i].nEtnCandEx++;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else {
                                    checkIL = true;
                                }
                            }
                            if (!checkIL) {
                                int[] calSESE = new int[clsLoop.Loop[orgLoop].Loop[i].nNode + 1];
                                for (int m = 0; m < clsLoop.Loop[orgLoop].Loop[i].nNode; m++) calSESE[m] = clsLoop.Loop[orgLoop].Loop[i].Node[m];
                                calSESE[clsLoop.Loop[orgLoop].Loop[i].nNode] = clsLoop.Loop[orgLoop].Loop[i].header;
                                // markingEdges_calSESE(currentN, calSESE, nodeD, nodeR); (Partialy remove?, need to be restored?????
                            }
                        }
                    }
                    curDepth--;
                } while (curDepth > 0);
                //for the rest of the node outside the loops (in cyclic model)
                entrySESE = new int[graph.Network[currentN].nNode]; //store current candidate entries of SESE
                nEntrySESE = 0;
                exitSESE = new int[graph.Network[currentN].nNode]; //store current candidate exits of SESE
                nExitSESE = 0;
                //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                get_arrange_CandEnt_CandExt(ref graph, currentN, graph.reduceTempNet, ref clsLoop, orgLoop, -3, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree); //use reduceTempNet for identify which nodes is outside the loops
                if (nEntrySESE > 0 && nExitSESE > 0) {
                    identifySESE_NewApproach(ref graph, currentN, ref clsSESE, currentSESE, ref clsLoop, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                }
            }
            else {
                //acyclic model
                entrySESE = new int[graph.Network[currentN].nNode]; //store current candidate entries of SESE
                nEntrySESE = 0;
                exitSESE = new int[graph.Network[currentN].nNode]; //store current candidate exits of SESE
                nExitSESE = 0;

                //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
                get_arrange_CandEnt_CandExt(ref graph, currentN, graph.reduceTempNet, ref clsLoop, orgLoop, -1, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
                if (nEntrySESE > 0 && nExitSESE > 0) {
                    identifySESE_NewApproach(ref graph, currentN, ref clsSESE, currentSESE, ref clsLoop, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                }
            }
            //make hierarchy
            make_SESE_hierarchy(ref graph, currentN, ref clsSESE, currentSESE);
        }

        public static void find_SESE_Dummy(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int orgLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int nodeSS)
        {
            Initialize_All(ref graph, currentN, ref clsLoop, ref clsSESE, currentSESE, false);

            bool[] reduceSESE = new bool[graph.Network[currentN].nNode];
            gProAnalyzer.Ultilities.mappingGraph.to_adjacencyMatrix(ref graph, currentN, ref AjM_Network, ref graph.Network[currentN].nNode);
            checkEdges = new int[graph.Network[currentN].nNode, graph.Network[currentN].nNode]; //mark which edges belong to different SESE

            int[] entrySESE = new int[graph.Network[currentN].nNode]; //store current candidate entries of SESE
            int nEntrySESE = 0;
            int[] exitSESE = new int[graph.Network[currentN].nNode]; //store current candidate exits of SESE
            int nExitSESE = 0;
            bool NL = false;
            int curDepth;
            //initiate SESE
            clsSESE.SESE[currentSESE] = new gProAnalyzer.GraphVariables.clsSESE.strSESE();

            if (nodeSS == -2)
            {
                //Reset counting variable to 0
                //lemma2_C = 0;
                //prop4_C = 0;
                //lemma3_C = 0;
                //lemma4_C = 0;
                //lemma5_C = 0;
            }
            //acyclic model
            entrySESE = new int[graph.Network[currentN].nNode]; //store current candidate entries of SESE
            nEntrySESE = 0;
            exitSESE = new int[graph.Network[currentN].nNode]; //store current candidate exits of SESE
            nExitSESE = 0;

            System.Diagnostics.Stopwatch watch_SESE = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch watch_detail = new System.Diagnostics.Stopwatch();
            watch_SESE.Reset();
            watch_SESE.Start();
            watch_detail.Reset();
            detail_watch = 0;


            //first, list all related nodes in this layer, then sorting it based on Dom and Pdom tree.                            
            get_arrange_CandEnt_CandExt(ref graph, currentN, graph.reduceTempNet, ref clsLoop, orgLoop, -5, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE, maxDepth_DomTree, maxDepth_PdomTree);
            if (nEntrySESE > 0 && nExitSESE > 0)
            {
                //DateTime dt2 = new DateTime();
                //dt2 = DateTime.Now;
                identifySESE_NewApproach(ref graph, currentN, ref clsSESE, currentSESE, ref clsLoop, orgLoop, -1, false, false, makeLinkDomTree, makeLinkPdomTree, adjList, maxDepth_DomTree, maxDepth_PdomTree, entrySESE, nEntrySESE, exitSESE, nExitSESE, ref checkEdges, false, ref reduceSESE); //currentLoop = -1; mean no loop in model
                //MessageBox.Show("The System has initialized in: " + (DateTime.Now - dt2).TotalMilliseconds.ToString() + " milisecond", "Finish");
            }

            watch_SESE.Stop();
            double time = watch_SESE.ElapsedMilliseconds;
            double time_details = detail_watch;
            //make hierarchy
            make_SESE_hierarchy(ref graph, currentN, ref clsSESE, currentSESE);
        }

        private static void identifySESE_NewApproach(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE,
            ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, bool NL, bool sameSide, bool[,] makeLinkDomTree,
            bool[,] makeLinkPdomTree, int[][] adjList, int maxDepth_DomTree, int maxDepth_PdomTree, int[] entrySESE, int nEntrySESE, int[] exitSESE, int nExitSESE, ref int[,]
            checkEdges, bool isLoop, ref bool[] reduceSESE) //isLoop mean we will ignore checking CandEn and CandExt in eDom Tree and EPdomTree
        {
            int countEdge = 0;

            //Identifying SESE
            for (int en = 0; en < nEntrySESE; en++)
            {
                for (int ex = 0; ex < nExitSESE; ex++)
                {
                    int nodeD = entrySESE[en];
                    int nodeR = exitSESE[ex];
                    if (nodeD == nodeR) continue;

                    bool isEntry = true, isExit = true, isOut = false;

                    int[] calSESE = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, graph.Network[currentN].Node[nodeD].DomEI, graph.Network[currentN].Node[nodeR].DomRevEI);

                    if (calSESE.Length == 0) continue;

                    int[] EnEx = { nodeD, nodeR };
                    calSESE = gProAnalyzer.Ultilities.findUnion.find_Union(graph.Network[currentN].nNode, calSESE, EnEx);

                    if (!en_ex_inSESE(calSESE, nodeD, nodeR)) continue; //candidate entry and exit should exist in calSESE ==>> also mean => 
                    //entrySESE weakly dominates exitSESE and exitSESE weakly postdominates entrySESE

                    if (!isOut)
                    {
                        int nNode = graph.Network[currentN].nNode;
                        int[] SourceOfError = new int[nNode];
                        int[] DecendantOfError = new int[nNode];
                        bool[] Mark = new bool[nNode];
                        Mark = Array.ConvertAll<bool, bool>(Mark, b => b = true); //MARK FOR DFS()
                        int nS = 0;

                        for (int k = 0; k < calSESE.Length; k++)
                        {
                            Mark[calSESE[k]] = false; //mark for DFS

                            if (calSESE[k] == nodeD || calSESE[k] == nodeR) continue;
                            //check the rest of candiate SESE (whether its have predecessor or succesors inside the candiate SESE)                                
                            //int sourceNode = -1;
                            int nodeSESE = calSESE[k];
                            for (int j = 0; j < graph.Network[currentN].Node[calSESE[k]].nPost; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, graph.Network[currentN].Node[calSESE[k]].Post[j]))
                                {
                                    SourceOfError[nS] = graph.Network[currentN].Node[calSESE[k]].Post[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                            for (int j = 0; j < graph.Network[currentN].Node[calSESE[k]].nPre; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, graph.Network[currentN].Node[calSESE[k]].Pre[j]))
                                {
                                    SourceOfError[nS] = graph.Network[currentN].Node[calSESE[k]].Pre[j];
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
                                DFS(adjList, ref Mark, i, SourceOfError, DecendantOfError, nodeD, nodeR);
                            }

                            int nRemove = 0;
                            for (int i = 0; i < calSESE.Length; i++)
                            {
                                if (Mark[calSESE[i]] == true)
                                {
                                    calSESE[i] = -1;
                                    nRemove++;
                                }
                            }

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

                        //Check Entry and Exit which have at least 2 SUC and 2 PRE (validity check)
                        isOut = isOut_EnEx_calSESE(ref graph, currentN, nodeD, nodeR, calSESE);
                    }

                    if (check_allEdgesInside(ref graph, currentN, nodeD, nodeR, calSESE, checkEdges, ref countEdge)) continue; //CHECK DUPLICATED SESE (PREPOSITION 4) // DEFINITION B (new)

                    if (isEntry && isExit && !isOut)  // Add
                    {
                        //count lemma  4 5
                        bool notCountLemma2 = false;
                        if (isLoopHeader(ref clsLoop, clsLoop.orgLoop, nodeD)) { /*lemma4_C++;*/ notCountLemma2 = true; }
                        if (isLoopSingleExit(ref clsLoop, nodeR, clsLoop.orgLoop)) { /*lemma5_C++;*/ notCountLemma2 = true; }

                        
                        identify_more_rigids_BTC(ref graph, ref clsSESE, currentN, currentSESE, calSESE, nodeD, nodeR, adjList, adjList_Temp, ref reduceSESE);
                       
                        gProAnalyzer.GraphVariables.clsSESE.strSESEInform[] oldSESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE];
                        for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) oldSESE[k] = clsSESE.SESE[currentSESE].SESE[k];

                        if (!(NL == true && clsLoop.Loop[workLoop].Loop[curLoop].nExit == 1 && clsLoop.Loop[workLoop].Loop[curLoop].Entry[0] == nodeD && clsLoop.Loop[workLoop].Loop[curLoop].Exit[0] == nodeR))
                        {
                            clsSESE.SESE[currentSESE].SESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE + 1];
                            for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) clsSESE.SESE[currentSESE].SESE[k] = oldSESE[k];

                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].depth = -1;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].parentSESE = -1;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Entry = nodeD;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Exit = nodeR;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].nNode = calSESE.Length;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Node = calSESE;

                            clsSESE.SESE[currentSESE].nSESE++;

                            //if (notCountLemma2 == false) lemma2_C++;

                            watch_detail.Reset();
                            watch_detail.Start();

                            markingEdges_calSESE(ref graph, currentN, calSESE, ref AjM_Network, nodeD, nodeR);

                            markingEdges_calSESE_Undirected(ref graph, currentN, calSESE, ref AjM_Network_Undirected, nodeD, nodeR);

                            

                            //UPDATE AdjList:
                            adjList_Update(ref graph, currentN, AjM_Network_Undirected, ref adjList_Temp);
                            
                            watch_detail.Stop();
                            detail_watch += watch_detail.ElapsedMilliseconds;
                        
                            
                        }
                    }
                }
            }
        }
        
        //remove
        private void __identifySESE_NewApproach(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, 
            ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, bool NL, bool sameSide, bool[,] makeLinkDomTree,
            bool[,] makeLinkPdomTree, int[][] adjList, int maxDepth_DomTree, int maxDepth_PdomTree, int[] entrySESE, int nEntrySESE, int[] exitSESE, int nExitSESE, ref int[,] 
            checkEdges, bool isLoop, ref bool[] reduceSESE) //isLoop mean we will ignore checking CandEn and CandExt in eDom Tree and EPdomTree
        {
            int countEdge = 0;
            //use the set => get the SESE
            //bool[] notVisitExit = new bool[nNode];
            //Identifying SESE
            for (int en = 0; en < nEntrySESE; en++) {
                for (int ex = 0; ex < nExitSESE; ex++) {
                    int nodeD = entrySESE[en];
                    int nodeR = exitSESE[ex];
                    if (nodeD == nodeR) continue;
                    if (nodeD == 46 && nodeR == 10) {

                    }
                    bool isEntry = true, isExit = true, isOut = false;
                    //SESE = eDom^-1(NodeD) Intersect ePdom^-1(NodeR)
                    int[] calSESE = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, graph.Network[currentN].Node[nodeD].DomEI, graph.Network[currentN].Node[nodeR].DomRevEI);
                    if (calSESE == null) continue;

                    if (!en_ex_inSESE(calSESE, nodeD, nodeR)) continue; //candidate entry and exit should exist in calSESE ==>> also mean => 
                    //entrySESE weakly dominates exitSESE and exitSESE weakly postdominates entrySESE

                    if (!isOut) {
                        //check_En_Ex_inSESE(nodeD, nodeR, calSESE_temp); <<== remove node in Loop (
                        int nNode = graph.Network[currentN].nNode;
                        int[] SourceOfError = new int[nNode];
                        int[] DecendantOfError = new int[nNode];
                        bool[] Mark = new bool[nNode];
                        Mark = Array.ConvertAll<bool, bool>(Mark, b => b = true); //MARK FOR DFS()
                        int nS = 0;
                        int arrEr = 0;

                        for (int k = 0; k < calSESE.Length; k++) {
                            Mark[calSESE[k]] = false; //mark for DFS

                            if (calSESE[k] == nodeD || calSESE[k] == nodeR) continue;
                            //check the rest of candiate SESE (whether its have predecessor or succesors inside the candiate SESE)                                
                            //int sourceNode = -1;
                            int nodeSESE = calSESE[k];
                            for (int j = 0; j < graph.Network[currentN].Node[calSESE[k]].nPost; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, graph.Network[currentN].Node[calSESE[k]].Post[j])) {
                                    SourceOfError[nS] = graph.Network[currentN].Node[calSESE[k]].Post[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                            for (int j = 0; j < graph.Network[currentN].Node[calSESE[k]].nPre; j++) //check for the case 1Ex_e43l.net  <= pair 66, 51
                            {
                                if (!node_insideSESE(calSESE, graph.Network[currentN].Node[calSESE[k]].Pre[j])) {
                                    SourceOfError[nS] = graph.Network[currentN].Node[calSESE[k]].Pre[j];
                                    DecendantOfError[nS] = calSESE[k];
                                    nS++;
                                    break;
                                }
                            }
                        }

                        //Find all relate node
                        if (nS > 0) {
                            //Foreach SOURCEOFERR[i]
                            for (int i = 0; i < nS; i++) {
                                DFS(adjList, ref Mark, i, SourceOfError, DecendantOfError, nodeD, nodeR); //=> COULD be LINEAR O(n) => Mark[] = true for node was visited
                            }

                            //Remove unnecessary Node in SESE to get the SESE
                            int nRemove = 0;
                            for (int i = 0; i < calSESE.Length; i++) {
                                if (Mark[calSESE[i]] == true) //mark the nodes should be removed in calSESE[] // asign = -1; //=> LINEAR
                                {
                                    calSESE[i] = -1;
                                    nRemove++;
                                }
                            }

                            //Re-create a new calSESE for next step; => LINEAR
                            int[] tempSESE = new int[calSESE.Length - nRemove];
                            int nTemp = 0;
                            for (int i = 0; i < calSESE.Length; i++) {
                                if (calSESE[i] != -1) {
                                    tempSESE[nTemp] = calSESE[i];
                                    nTemp++;
                                }
                            }
                            calSESE = tempSESE;
                        }

                        #region Check Entry and Exit which have at least 2 SUC and 2 PRE
                        //check Entries
                        for (int k = 0; k < calSESE.Length; k++) {
                            //Check nodeD - Entry whether belong inside calSESE
                            if (calSESE[k] == nodeD) {
                                isEntry = true;
                                int inPost = 0;
                                for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPost; i++) //check successor of SESE Entry
                                {

                                    for (int j = 0; j < calSESE.Length; j++) {
                                        if (calSESE[j] == graph.Network[currentN].Node[nodeD].Post[i]) {
                                            inPost++;
                                            //break;
                                        }
                                    }
                                }


                                //Check entry have at least 1 incoming edge from OUTSIDE calSESE
                                int inPre = 0;
                                for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPre; i++) //check predecessor of SESE Exit
                                {
                                    for (int j = 0; j < calSESE.Length; j++) {
                                        if (calSESE[j] == graph.Network[currentN].Node[nodeD].Pre[i]) {
                                            inPre++;
                                            //break;
                                        }
                                    }
                                }
                                //Final check (if it have 2 succ or at least 1 succ and 1 pre from inside SESE => out = false
                                if ((inPost + inPre) < 2) isOut = true;
                            }
                            else if (calSESE[k] == nodeR) {
                                isExit = true;

                                if (!isOut) {
                                    int inPre = 0;
                                    for (int i = 0; i < graph.Network[currentN].Node[nodeR].nPre; i++) //check predecessor of SESE Exit
                                    {
                                        //if (Network[currentN].Node[i].Kind != "XOR" && Network[currentN].Node[i].Kind != "OR" && Network[currentN].Node[i].Kind != "AND") continue;
                                        for (int j = 0; j < calSESE.Length; j++) {
                                            if (calSESE[j] == graph.Network[currentN].Node[nodeR].Pre[i]) {
                                                inPre++;
                                                //break;
                                            }
                                        }
                                    }
                                    int inPost = 0;
                                    for (int i = 0; i < graph.Network[currentN].Node[nodeR].nPost; i++) //check successor of SESE Entry
                                    {

                                        for (int j = 0; j < calSESE.Length; j++) {
                                            if (calSESE[j] == graph.Network[currentN].Node[nodeR].Post[i]) {
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
                    if (check_allEdgesInside(ref graph, currentN, nodeD, nodeR, calSESE, checkEdges, ref countEdge)) continue; //CHECK DUPLICATED SESE (PREPOSITION 4) // DEFINITION B (new)

                    if (isEntry && isExit && !isOut)  // Add
                    {
                        //count lemma  4 5
                        bool notCountLemma2 = false;
                        //if (check.isLoopHeader(ref clsLoop, clsLoop.orgLoop, nodeD)) { lemma4_C++; notCountLemma2 = true; }
                        //if (check.isLoopSingleExit(ref clsLoop, nodeR, clsLoop.orgLoop)) { lemma5_C++; notCountLemma2 = true; }

                        //final checking rigid and add more SESE (1 or more rigids inside)
                        identify_more_rigids_BTC(ref graph, ref clsSESE, currentN, currentSESE, calSESE, nodeD, nodeR, adjList, adjList_Temp, ref reduceSESE);
                        gProAnalyzer.GraphVariables.clsSESE.strSESEInform[] oldSESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE];

                        for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) oldSESE[k] = clsSESE.SESE[currentSESE].SESE[k];

                        if (!(NL == true && clsLoop.Loop[workLoop].Loop[curLoop].nExit == 1 && clsLoop.Loop[workLoop].Loop[curLoop].Entry[0] == nodeD && clsLoop.Loop[workLoop].Loop[curLoop].Exit[0] == nodeR)) {
                            clsSESE.SESE[currentSESE].SESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE + 1];
                            for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) clsSESE.SESE[currentSESE].SESE[k] = oldSESE[k];

                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].depth = -1;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].parentSESE = -1;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Entry = nodeD;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Exit = nodeR;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].nNode = calSESE.Length;
                            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Node = calSESE;
                            clsSESE.SESE[currentSESE].nSESE++;

                            //if (notCountLemma2 == false) lemma2_C++;

                          //  markingEdges_calSESE(ref graph, currentN, calSESE, nodeD, nodeR); //fix on 2018/1/27 => OK refactoring at 2018/6/1                            
                        }
                    }
                }
            }
        }
        

        public static void get_arrange_CandEnt_CandExt(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int reduceTempNet, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int currentLoop, ref int[] entrySESE, ref int nEntrySESE, ref int[] exitSESE, ref int nExitSESE, int maxDepth_DomTree, int maxDepth_PdomTree)
        {
            //get all node inside this layer (loop nesting forest) ==> for Cyclic model //ONLY SPLIT AND JOIN?
            if (currentLoop >= 0)
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currentLoop].nNode; i++)
                {
                    int node = clsLoop.Loop[workLoop].Loop[currentLoop].Node[i];
                    {
                        //if (!isInLoopConstruct(workLoop, currentLoop, node))                    
                        if (graph.Network[currentN].Node[node].nPost > 1)
                        {
                            entrySESE[nEntrySESE] = node;
                            nEntrySESE++;
                        }
                        if (graph.Network[currentN].Node[node].nPre > 1)
                        {
                            exitSESE[nExitSESE] = node;
                            nExitSESE++;
                        }
                    }
                }
                exitSESE[nExitSESE] = clsLoop.Loop[workLoop].Loop[currentLoop].header;
                nExitSESE++;
                //more added to deal with type-2 SESE
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currentLoop].nEntry; i++)
                {
                    int node = clsLoop.Loop[workLoop].Loop[currentLoop].Entry[i];
                    entrySESE[nEntrySESE] = node;
                    nEntrySESE++;
                }
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[currentLoop].nExit; i++)
                {
                    int node = clsLoop.Loop[workLoop].Loop[currentLoop].Exit[i];
                    exitSESE[nExitSESE] = node;
                    nExitSESE++;
                }
            }

            if (currentLoop == -1) //for Acyclic models ONLY (because we use find_SESE 3 times?)
            {
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    //if (isLoopHeader(workLoop, i))
                    if (graph.Network[currentN].Node[i].nPost > 1)
                    {
                        entrySESE[nEntrySESE] = i;
                        nEntrySESE++;
                    }
                    if (graph.Network[currentN].Node[i].nPre > 1)
                    {
                        exitSESE[nExitSESE] = i;
                        nExitSESE++;
                    }
                }
            }

            //============= New test -- Use all gateway for candidate entrySESE and candidate exitSESE
            if (currentLoop == -5) //for Testing Acyclic and Cyclic model purpose
            {
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    if (graph.Network[currentN].Node[i].Kind == "START" || graph.Network[currentN].Node[i].Kind == "END") continue;
                    if (graph.Network[currentN].Node[i].nPre == 1 && graph.Network[currentN].Node[i].nPost == 1) continue;

                    entrySESE[nEntrySESE] = i;
                    nEntrySESE++;

                    exitSESE[nExitSESE] = i;
                    nExitSESE++;
                }
            }
            //===========================================================================================
            if (currentLoop == -3) // mean nodes outside all loops (of a cyclic model)
            {
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    if (graph.Network[reduceTempNet].Node[i].nPost == 0 && graph.Network[reduceTempNet].Node[i].nPre == 0 || gProAnalyzer.Ultilities.checkGraph.isLoopHeader(ref clsLoop, workLoop, i)) continue;
                    if (graph.Network[currentN].Node[i].nPost > 1)
                    {
                        entrySESE[nEntrySESE] = i;
                        nEntrySESE++;
                    }
                    if (graph.Network[currentN].Node[i].nPre > 1)
                    {
                        exitSESE[nExitSESE] = i;
                        nExitSESE++;
                    }
                }
            }
            //add more node from the lower hyerarchy of loop forest
            if (currentLoop >= 0 || currentLoop == -3)
            {
                add_more_CandEn_CandEx(ref clsLoop, workLoop, currentLoop, ref entrySESE, ref nEntrySESE, ref exitSESE, ref nExitSESE);
            }
            //re-ordering CandEnt and CandExit
            reOrdering_candidate_EnEx(ref graph, currentN, ref entrySESE, ref exitSESE, nEntrySESE, nExitSESE, true, maxDepth_DomTree, maxDepth_PdomTree);
            reOrdering_candidate_EnEx(ref graph, currentN, ref entrySESE, ref exitSESE, nEntrySESE, nExitSESE, false, maxDepth_DomTree, maxDepth_PdomTree);
        }

        public static void add_more_CandEn_CandEx(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int loop, ref int[] entrySESE, ref int nEntrySESE, ref int[] exitSESE, ref int nExitSESE)
        {
            //add more candidate entries and candidate exits to current entrySESE and exitSESE
            if (loop >= 0)
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nChild; i++)
                {
                    int child = clsLoop.Loop[workLoop].Loop[loop].child[i];
                    if (clsLoop.Loop[workLoop].Loop[child].etnCandEn != null && check_Exist_in_aSet(clsLoop.Loop[workLoop].Loop[child].etnCandEn[0], entrySESE, nEntrySESE) == false)
                    {
                        //add etnCandEn[0] to entrySESE, increase index
                        entrySESE[nEntrySESE] = clsLoop.Loop[workLoop].Loop[child].etnCandEn[0];
                        nEntrySESE++;
                    }
                    if (clsLoop.Loop[workLoop].Loop[child].etnCandEx != null && check_Exist_in_aSet(clsLoop.Loop[workLoop].Loop[child].etnCandEx[0], exitSESE, nExitSESE) == false)
                    {
                        //add etnCandEx[0] t exitSESE, increase index
                        exitSESE[nExitSESE] = clsLoop.Loop[workLoop].Loop[child].etnCandEx[0];
                        nExitSESE++;
                    }
                }
            }
            if (loop == -3) //no more loop outside
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].parentLoop == -1)
                    {
                        int child = i;

                        if (clsLoop.Loop[workLoop].Loop[child].etnCandEn != null && check_Exist_in_aSet(clsLoop.Loop[workLoop].Loop[child].etnCandEn[0], entrySESE, nEntrySESE) == false)
                        {
                            //add etnCandEn[0] to entrySESE, increase index
                            entrySESE[nEntrySESE] = clsLoop.Loop[workLoop].Loop[child].etnCandEn[0];
                            nEntrySESE++;
                        }
                        if (clsLoop.Loop[workLoop].Loop[child].etnCandEx != null && check_Exist_in_aSet(clsLoop.Loop[workLoop].Loop[child].etnCandEx[0], exitSESE, nExitSESE) == false)
                        {
                            //add etnCandEx[0] t exitSESE, increase index
                            exitSESE[nExitSESE] = clsLoop.Loop[workLoop].Loop[child].etnCandEx[0];
                            nExitSESE++;
                        }
                    }
                }
            }
        }

        public static bool check_Exist_in_aSet(int node, int[] set, int nSet)
        {
            for (int i = 0; i < nSet; i++)
            {
                if (node == set[i])
                {
                    return true;
                }
            }
            return false;
        }

        public static void reOrdering_candidate_EnEx(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int[] EntrySESE, ref int[] ExitSESE, int nEntrySESE, int nExitSESE, bool isEntrySet, int maxDepth_DomTree, int maxDepth_PdomTree)
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
                        if (graph.Network[currentN].Node[EntrySESE[i]].DepthDom != curDepth) continue;
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
                        if (graph.Network[currentN].Node[ExitSESE[i]].DepthPdom != curDepth) continue;
                        Q.Enqueue(ExitSESE[i]);
                    }
                    curDepth--;
                } while (curDepth > 0);
                ExitSESE = new int[Q.Count];
                Q.CopyTo(ExitSESE, 0);
            }
        }

        public static bool en_ex_inSESE(int[] calSESE, int nodeD, int nodeR)
        {
            bool check_1 = false;
            for (int i = 0; i < calSESE.Length; i++)
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

        public static bool check_allEdgesInside(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int nodeD, int nodeR, int[] calSESE, int[,] checkEdges, ref int countEdge)
        {
            countEdge++;
            bool checkOut = true;
            bool flag = true;
            int curEdge = 0;

            for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPost; i++)
            {
                if (node_insideSESE(calSESE, graph.Network[currentN].Node[nodeD].Post[i]))
                {
                    if (flag) curEdge = checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Post[i]];

                    if (checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Post[i]] != curEdge || checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Post[i]] == 0)
                    {
                        checkOut = false;
                        break;
                    }
                    flag = false;
                }
            }
            for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPre; i++)
            {
                if (node_insideSESE(calSESE, graph.Network[currentN].Node[nodeD].Pre[i]))
                {
                    if (flag) curEdge = checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Pre[i]];

                    //if the entry have at least 1 edge to the calSESE which is not consider before => good SESE
                    if (checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Pre[i]] != curEdge || checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Pre[i]] == 0)
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
                for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPost; i++)
                {
                    if (node_insideSESE(calSESE, graph.Network[currentN].Node[nodeD].Post[i]))
                    {
                        checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Post[i]] = countEdge;
                    }
                }
                for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPre; i++)
                {
                    if (node_insideSESE(calSESE, graph.Network[currentN].Node[nodeD].Pre[i]))
                    {
                        checkEdges[nodeD, graph.Network[currentN].Node[nodeD].Pre[i]] = countEdge;
                    }
                }
            }


            if (checkOut) countEdge--;
            return checkOut;
        }

        public static bool node_insideSESE(int[] calSESE, int node)
        {
            for (int i = 0; i < calSESE.Length; i++)
            {
                if (calSESE[i] == node)
                    return true;
            }
            return false;
        }

        private static bool isLoopSingleExit(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int curNode, int workLoop)
        {
            int numLoop = clsLoop.Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[i].nExit == 1)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].Exit[0] == curNode)
                        return true;
                }
            }
            return false;
        }

        public static bool isLoopHeader(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workloop, int node)
        {
            for (int i = 0; i < clsLoop.Loop[workloop].nLoop; i++)
            {
                if (clsLoop.Loop[workloop].Loop[i].header == node) return true;
            }
            return false;
        }

        public static void identify_more_rigids_BTC(ref gProAnalyzer.GraphVariables.clsGraph graph, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentN, int currentSESE, int[] calSESE, 
            int nodeD, int nodeR, int[][] adjList, int[][] adjList_Temp, ref bool[] reduceSESE)
        {
            int[] aloneNode = new int[graph.Network[currentN].nNode];
            int nAloneNode = 0;

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (AjM_Network[nodeD, i] == 1)
                    if (node_insideSESE(calSESE, i) && i != nodeR)
                    {
                        aloneNode[nAloneNode] = i;
                        nAloneNode++;
                    }
            }

            int nbond = 0;
            bool[] mark = new bool[graph.Network[currentN].nNode];
            bool[] mark_org = new bool[graph.Network[currentN].nNode];
            int[] bondsArr = new int[calSESE.Length];
            int nbondsArr = 0;
            int nrigid = 0;
            for (int i = 0; i < nAloneNode; i++)
            {
                if (mark[aloneNode[i]] == false)
                {
                    int[] getRigids = new int[calSESE.Length];
                    int[] getRigids_org = new int[calSESE.Length];
                    int nRigids = 0;
                    int nRigids_org = 0;
                    getRigids[nRigids] = aloneNode[i]; //add first node to rigids
                    nRigids++;
                    getRigids_org[nRigids_org] = aloneNode[i]; //add first node to rigids
                    nRigids_org++;

                    int remainedEdges = 0;
                    bool stop = false;

                    int pathEdges_calibrate = 0;
                    bool is_newSESE = false;
                    bool is_bond = false;

                    for (int k = 0; k < mark.Length; k++)
                        mark_org[k] = mark[k];

                    DFS_Rigids(adjList_Temp, ref mark, aloneNode[i], nodeD, nodeR, ref getRigids, ref nRigids);

                    DFS_Rigids(adjList, ref mark_org, aloneNode[i], nodeD, nodeR, ref getRigids_org, ref nRigids_org);

                    if (nRigids_org < (calSESE.Length - 2)) //if it is not the existing SESE (calsese)
                    {
                        count_Edges(ref graph, currentN, getRigids, nRigids, ref remainedEdges, nodeD, nodeR); //count all edges in the [getRigids] region.
                        bool[] mark_2 = new bool[graph.Network[currentN].nNode];

                        //first_path(currentN, ref pathEdges_calibrate, aloneNode[i], nodeD, nodeR, ref stop, ref mark_2);

                        if (remainedEdges > (nRigids + 2 - 1)) is_newSESE = true;

                        //if (AjM_Network[nodeD, aloneNode[i]] != 0) pathEdges_calibrate++;
                        //if (remainedEdges > pathEdges_calibrate) is_newSESE = true;
                        //if (remainedEdges == pathEdges_calibrate) is_bond = true;

                        if (is_newSESE)
                        {
                            getRigids_org[nRigids_org] = nodeD;
                            nRigids_org++;
                            getRigids_org[nRigids_org] = nodeR;
                            nRigids_org++;
                            int[] tempRigids = new int[nRigids_org];
                            for (int k = 0; k < nRigids_org; k++) tempRigids[k] = getRigids_org[k];

                            if (isOut_EnEx_calSESE(ref graph, currentN, nodeD, nodeR, tempRigids) == false)
                            {
                                gProAnalyzer.GraphVariables.clsSESE.strSESEInform[] oldSESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE];
                                for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) oldSESE[k] = clsSESE.SESE[currentSESE].SESE[k];

                                clsSESE.SESE[currentSESE].SESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE + 1];
                                for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) clsSESE.SESE[currentSESE].SESE[k] = oldSESE[k];
                                clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].depth = -1;
                                clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].parentSESE = -1;
                                clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Entry = nodeD;
                                clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Exit = nodeR;
                                clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].nNode = nRigids_org;
                                clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Node = tempRigids;
                                clsSESE.SESE[currentSESE].nSESE++;

                                //prop4_C++;
                                nrigid++;
                            }
                            else
                                is_newSESE = false;
                        }
                        if (!is_newSESE)
                        {
                            nbond++;
                            //store element in getRigids array!!!
                            for (int k = 0; k < nRigids; k++)
                            {
                                bondsArr[nbondsArr] = getRigids[k];
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
        }

        /*
        public void markingEdges_calSESE(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] calSESE, int nodeD, int nodeR)
        {
            //get edges from calSESE
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                if (node_insideSESE(calSESE, graph.Network[currentN].Link[i].fromNode) && node_insideSESE(calSESE, graph.Network[currentN].Link[i].toNode))
                {
                    AjM_Network[graph.Network[currentN].Link[i].fromNode, graph.Network[currentN].Link[i].toNode] = 0;
                }
            }
            AjM_Network[nodeD, nodeR] = 1; //newly added for fixing the refinement glitch
        }
         */

        public static void markingEdges_calSESE(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] calSESE, ref int[,] AjM_Network, int nodeD, int nodeR)
        {
            //get edges from calSESE
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                if (node_insideSESE(calSESE, graph.Network[currentN].Link[i].fromNode) && node_insideSESE(calSESE, graph.Network[currentN].Link[i].toNode))
                {
                    AjM_Network[graph.Network[currentN].Link[i].fromNode, graph.Network[currentN].Link[i].toNode] = 0;
                    AjM_Network[graph.Network[currentN].Link[i].toNode, graph.Network[currentN].Link[i].fromNode] = 0;
                }
            }

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                for (int j = 0; j < graph.Network[currentN].nNode; j++)
                {
                    if (i == j) continue;
                    if (node_insideSESE(calSESE, i) && node_insideSESE(calSESE, j))
                    {
                        if (AjM_Network[i, j] == 1)
                            AjM_Network[i, j] = 0;
                    }
                }
            }

            AjM_Network[nodeD, nodeR] = 1;
            //AjM_Network[nodeR, nodeD] = 1;
        }

        public static void markingEdges_calSESE_Undirected(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] calSESE, ref int[,] AjM_Network_U, int nodeD, int nodeR)
        {
            //get edges from calSESE
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                if (node_insideSESE(calSESE, graph.Network[currentN].Link[i].fromNode) && node_insideSESE(calSESE, graph.Network[currentN].Link[i].toNode))
                {
                    AjM_Network_U[graph.Network[currentN].Link[i].fromNode, graph.Network[currentN].Link[i].toNode] = 0;
                    AjM_Network_U[graph.Network[currentN].Link[i].toNode, graph.Network[currentN].Link[i].fromNode] = 0;
                }
            }

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                for (int j = 0; j < graph.Network[currentN].nNode; j++)
                {
                    if (i == j) continue;
                    if (node_insideSESE(calSESE, i) && node_insideSESE(calSESE, j))
                    {
                        if (AjM_Network_U[i, j] == 1)
                            AjM_Network_U[i, j] = 0;
                    }
                }
            }

            AjM_Network_U[nodeD, nodeR] = 1;
            AjM_Network_U[nodeR, nodeD] = 1;
        }

        public static void adjList_Update(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[,] AjM_Network, ref int[][] adjList_Temp)
        {
            int nNode = graph.Network[currentN].nNode;
            for (int i = 0; i < nNode; i++)
            {
                bool check = false;
                int[] temArr = new int[nNode];
                int nTemArr = 0;
                for (int j = 0; j < nNode; j++)
                {
                    if (AjM_Network[i, j] == 0)
                        check = true;
                    if (AjM_Network[i, j] == 1)
                    {
                        temArr[nTemArr] = j;
                        nTemArr++;
                    }
                }
                if (check == true) // change adList[][]
                {
                    adjList_Temp[i] = new int[nTemArr + 1];
                    adjList_Temp[i][0] = nTemArr;
                    for (int k = 0; k < nTemArr; k++)
                        adjList_Temp[i][k + 1] = temArr[k];
                }
            }
        }

        public static void DFS(int[][] adjList, ref bool[] Mark, int index, int[] SourceOfError, int[] DecendantOfError, int Entry, int Exit)
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
                    if (!Mark[v] && v != Entry && v != Exit && v != SourceOfError[index])
                    {
                        stack.Push(v);
                        Mark[v] = true;
                    }
                }
            }
        }

        public static void DFS_Rigids(int[][] adjList, ref bool[] Mark, int firstNode, int Entry, int Exit, ref int[] getRigids, ref int nRigids)
        {
            Stack stack = new Stack();
            Mark[firstNode] = true;
            stack.Push(firstNode);
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());
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

        public static void count_Edges(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] calSESE, int nCalSESE, ref int remainedEdges, int nodeD, int nodeR)
        {
            calSESE[nCalSESE] = nodeD;
            nCalSESE++;
            calSESE[nCalSESE] = nodeR;
            nCalSESE++;

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                for (int j = 0; j < graph.Network[currentN].nNode; j++)
                {
                    if (i == j) continue;
                    int frNode = i;
                    int toNode = j;
                    if (node_insideSET(calSESE, nCalSESE, frNode) && node_insideSET(calSESE, nCalSESE, toNode))
                    {
                        if ((AjM_Network[frNode, toNode] != 0) &&
                            (AjM_Network[frNode, toNode] != -1) && !(frNode == nodeR && toNode == nodeD))
                        {
                            remainedEdges++;
                        }
                    }
                }
            }
            nCalSESE = nCalSESE - 2; //roll-back to original
        }

        public static bool isOut_EnEx_calSESE(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int nodeD, int nodeR, int[] calSESE)
        {
            bool isOut = false;
            //check Entries
            for (int k = 0; k < calSESE.Length; k++)
            {
                //Check nodeD - Entry whether belong inside calSESE
                if (calSESE[k] == nodeD)
                {
                    int inPost = 0;
                    for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPost; i++) //check successor of SESE Entry
                    {

                        for (int j = 0; j < calSESE.Length; j++)
                        {
                            if (calSESE[j] == graph.Network[currentN].Node[nodeD].Post[i])
                            {
                                inPost++;
                            }
                        }
                    }

                    int inPre = 0;
                    for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPre; i++) //check predecessor of SESE entry
                    {
                        for (int j = 0; j < calSESE.Length; j++)
                        {
                            if (calSESE[j] == graph.Network[currentN].Node[nodeD].Pre[i])
                            {
                                inPre++;
                                //break;
                            }
                        }
                    }

                    if ((inPost + inPre) < 2)
                        isOut = true;

                    //Entry of SESE must have at least 1 incoming edge from OUTSIDE
                    int outPre = 0;
                    for (int i = 0; i < graph.Network[currentN].Node[nodeD].nPre; i++)
                    {
                        if (node_insideSET(calSESE, calSESE.Length, graph.Network[currentN].Node[nodeD].Pre[i]) == false)
                        {
                            outPre++;
                            break;
                        }
                    }
                    if (outPre == 0) isOut = true;
                }
                else if (calSESE[k] == nodeR)
                {
                    if (!isOut)
                    {
                        int inPre = 0;
                        for (int i = 0; i < graph.Network[currentN].Node[nodeR].nPre; i++) //check predecessor of SESE Exit
                        {
                            for (int j = 0; j < calSESE.Length; j++)
                            {
                                if (calSESE[j] == graph.Network[currentN].Node[nodeR].Pre[i])
                                {
                                    inPre++;
                                    //break;
                                }
                            }
                        }
                        int inPost = 0;
                        for (int i = 0; i < graph.Network[currentN].Node[nodeR].nPost; i++) //check successor of SESE Entry
                        {

                            for (int j = 0; j < calSESE.Length; j++)
                            {
                                if (calSESE[j] == graph.Network[currentN].Node[nodeR].Post[i])
                                {
                                    inPost++;
                                    //break;
                                }
                            }
                        }
                        if ((inPost + inPre) < 2) isOut = true;

                        //Exit of SESE must have at least 1 outgoing edge to OUTSIDE
                        int outPost = 0;
                        for (int i = 0; i < graph.Network[currentN].Node[nodeR].nPost; i++)
                        {
                            if (node_insideSET(calSESE, calSESE.Length, graph.Network[currentN].Node[nodeR].Post[i]) == false)
                            {
                                outPost++;
                                break;
                            }
                        }
                        if (outPost == 0) isOut = true;
                    }
                }
            }
            return isOut;
        }


        public static bool node_insideSET(int[] calSESE, int nCalSESE, int node)
        {
            for (int i = 0; i < nCalSESE; i++)
            {
                if (calSESE[i] == node)
                    return true;
            }
            return false;
        }

        public void first_path(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int pathEdges_calibrate, int startNode, int nodeR, ref bool stop, ref bool[] mark_2)
        {
            mark_2[startNode] = true;
            for (int i = 0; i < graph.Network[currentN].Node[startNode].nPost; i++)
            {
                if (stop) return;
                if (mark_2[graph.Network[currentN].Node[startNode].Post[i]] == true) continue;
                if (graph.Network[currentN].Node[startNode].Post[i] != nodeR)
                {
                    if (AjM_Network[startNode, graph.Network[currentN].Node[startNode].Post[i]] != 0) pathEdges_calibrate++;
                    first_path(ref graph, currentN, ref pathEdges_calibrate, graph.Network[currentN].Node[startNode].Post[i], nodeR, ref stop, ref mark_2);
                }
                else
                {
                    stop = true;
                    pathEdges_calibrate++;
                }
            }
        }

        public static void make_SESE_hierarchy(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE)
        {
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[iSE].nNode >= clsSESE.SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = gProAnalyzer.Ultilities.findIntersection.find_Intersection(clsSESE.SESE[currentSESE].SESE[jSE].nNode, clsSESE.SESE[currentSESE].SESE[iSE].Node, clsSESE.SESE[currentSESE].SESE[jSE].Node);

                    if (gProAnalyzer.Ultilities.checkGraph.check_SameSet(calSESE, clsSESE.SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (clsSESE.SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (clsSESE.SESE[currentSESE].SESE[jSE].nNode > clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = clsSESE.SESE[currentSESE].SESE[iSE].parentSESE;
                        }
                        clsSESE.SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            int max_Depth = 0;
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (clsSESE.SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = clsSESE.SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[clsSESE.SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                clsSESE.SESE[currentSESE].SESE[iSE].depth = depth;
                clsSESE.SESE[currentSESE].SESE[iSE].nChild = cntFind;
                clsSESE.SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) clsSESE.SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            clsSESE.SESE[currentSESE].maxDepth = max_Depth;
            //================================
            //modify SESE (SESE[currentSESE].SESE[i].parentSESE);
            //modify_SESE_Hierarchy(currentSESE);
            //================================
            for (int i = 0; i < clsSESE.SESE[currentSESE].nSESE; i++) clsSESE.SESE[currentSESE].SESE[i].parentSESE = -1;

            // Make hierarchy
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[iSE].nNode >= clsSESE.SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = gProAnalyzer.Ultilities.findIntersection.find_Intersection(clsSESE.SESE[currentSESE].SESE[jSE].nNode, clsSESE.SESE[currentSESE].SESE[iSE].Node, clsSESE.SESE[currentSESE].SESE[jSE].Node);

                    if (gProAnalyzer.Ultilities.checkGraph.check_SameSet(calSESE, clsSESE.SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (clsSESE.SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (clsSESE.SESE[currentSESE].SESE[jSE].nNode > clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = clsSESE.SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        clsSESE.SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            max_Depth = 0;
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
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
                    if (clsSESE.SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = clsSESE.SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[clsSESE.SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                clsSESE.SESE[currentSESE].SESE[iSE].depth = depth;
                clsSESE.SESE[currentSESE].SESE[iSE].nChild = cntFind;
                clsSESE.SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) clsSESE.SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            clsSESE.SESE[currentSESE].maxDepth = max_Depth;
        }

    }
}
