using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Functionalities
{
    class IndexingPM
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

        public frmAnalysisNetwork frmAnl;

        bool totalConcurrent = false;
        bool existConcurrent = false;
        bool totalCausal = false;
        bool existCausal = false;
        bool canConflict = false;
        bool NotcanConflict = false;
        bool canCoocur = false;
        bool notCancoocur = false;

        public static void Initialize_All()
        {
            //SplitType1 = new gProAnalyzer.Functionalities.NodeSplittingType1();
            //SplitType2 = new gProAnalyzer.Functionalities.NodeSplittingType2();
            //SplitType3 = new gProAnalyzer.Functionalities.NodeSplittingType3();

            //findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
            //fndDomRel = new gProAnalyzer.Functionalities.DominanceIdentification();
            //sese = new gProAnalyzer.Functionalities.SESEIdentification();
            //makSubNet = new gProAnalyzer.Ultilities.makeSubNetwork();

            //makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
            //anlyzBh_InstF = new gProAnalyzer.Ultilities.AnalyseBehavior_InstF();
            //reduceG = new gProAnalyzer.Ultilities.reduceGraph();
            //extendG = new gProAnalyzer.Ultilities.extendGraph();
            //makNestingForest = new gProAnalyzer.Ultilities.makeNestingForest();
            //makUntangling = new gProAnalyzer.Functionalities.UntanglingIL();


            //frmAnl = new frmAnalysisNetwork();
            
        }
        public static void start_Indexing(ref GraphVariables.clsGraph graph, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsHWLS clsHWLS_Untangle,
            ref GraphVariables.clsLoop clsLoop, ref GraphVariables.clsSESE clsSESE, bool[] flag_Check)
        {
            Initialize_All();

            gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.orgNet, graph.midNet);

            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.orgLoop, ref clsLoop.IrreducibleError);

            graph.Network[graph.finalNet] = graph.Network[graph.midNet];
            gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph, graph.midNet, graph.finalNet, ref clsLoop, clsLoop.orgLoop);

            gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.finalNet);
            gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.finalNet);
            gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.finalNet, -2);
            gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.finalNet);

            gProAnalyzer.Functionalities.SESEIdentification.find_SESE_WithLoop(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);
            gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, true);                        

            //Make nesting forest
            gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph, graph.finalNet, ref clsHWLS, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE);

            int workNet = graph.finalNet;
            int workLoop = clsLoop.orgLoop;
            int workSESE = clsSESE.finalSESE;
            bool checkUntangle = false;

            //Untangling of Irreducible loops ==> USING UNTANGLE NET only (new graph)
            if (gProAnalyzer.Functionalities.UntanglingIL.make_UntanglingIL(ref graph, graph.finalNet, graph.untangleNet, clsHWLS, clsLoop, clsLoop.orgLoop, clsSESE, clsSESE.finalSESE))
            {

                //return;

                //frmAnl.displayProcessModel(ref graph, graph.untangleNet, ref clsLoop, -1, ref clsSESE, -1);
                //frmAnl.Show();
                //return;
                gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.untangleNet, graph.midNet);
                gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.untangleLoop, ref clsLoop.IrreducibleError);
                gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph, graph.midNet, graph.untangleNet, ref clsLoop, clsLoop.untangleLoop);

                gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.untangleNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.untangleNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.untangleNet, -2);
                gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.untangleNet);

                gProAnalyzer.Functionalities.SESEIdentification.find_SESE_WithLoop(ref graph, graph.untangleNet, ref clsLoop, clsLoop.untangleLoop, ref clsSESE, clsSESE.untangleSESE, -2);
                gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.untangleNet, ref clsLoop, clsLoop.untangleLoop, ref clsSESE, clsSESE.untangleSESE, true);
                gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph, graph.untangleNet, ref clsHWLS_Untangle, ref clsLoop, clsLoop.untangleLoop, ref clsSESE, clsSESE.untangleSESE);
                
                workNet = graph.untangleNet;
                workLoop = clsLoop.untangleLoop;
                workSESE = clsSESE.untangleSESE;
                checkUntangle = true;

                graph.check_untangle = true;

                //frmAnl.displayProcessModel(ref graph, graph.untangleNet, ref clsLoop, clsLoop.untangleLoop, ref clsSESE, clsSESE.untangleSESE);
                //frmAnl.Show();                
            }

            //== get initial behavior profile ==
            graph.Network[graph.reduceNet] = graph.Network[workNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            if (checkUntangle)
                get_InitialBehaviorProfile(ref graph, graph.reduceNet, ref clsHWLS_Untangle, ref clsLoop, workLoop, ref clsSESE, workSESE, flag_Check);
            else
                get_InitialBehaviorProfile(ref graph, graph.reduceNet, ref clsHWLS, ref clsLoop, workLoop, ref clsSESE, workSESE, flag_Check);
            //Database storing
        }

        public static void start_Indexing_Acyclic(ref GraphVariables.clsGraph graph, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsHWLS clsHWLS_Untangle,
            ref GraphVariables.clsLoop clsLoop, ref GraphVariables.clsSESE clsSESE, bool[] flag_Check)
        {
            Initialize_All();

            gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.orgNet, graph.midNet);

            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.orgLoop, ref clsLoop.IrreducibleError);

            graph.Network[graph.finalNet] = graph.Network[graph.midNet];
            gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph, graph.midNet, graph.finalNet, ref clsLoop, clsLoop.orgLoop);

            gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.finalNet);
            gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.finalNet);
            gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.finalNet, -2);
            gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.finalNet);

            gProAnalyzer.Functionalities.SESEIdentification.find_SESE_WithLoop(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);
            gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, true);

            //Make nesting forest
            gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph, graph.finalNet, ref clsHWLS, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE);

            GraphVariables.clsError clsError = new gProAnalyzer.GraphVariables.clsError();  
            
            //Fix each SOS entry for Bond and Rigid
            gProAnalyzer.Functionalities.VerificationG.Initialize_Verification(ref graph, ref clsError, ref clsLoop, ref clsSESE, ref clsHWLS);
            

            //verify bond first =>>

            int workNet = graph.finalNet;
            int workLoop = clsLoop.orgLoop;
            int workSESE = clsSESE.finalSESE;
            
            //== get initial behavior profile ==
            graph.Network[graph.reduceNet] = graph.Network[workNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            get_InitialBehaviorProfile(ref graph, graph.reduceNet, ref clsHWLS, ref clsLoop, workLoop, ref clsSESE, workSESE, flag_Check);
            //Database storing
        }

        public void build_loopDAG(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsLoop clsLoop, int currentLoop, 
            ref GraphVariables.clsSESE clsSESE, ref GraphVariables.clsLoopDAG clsLoopDAG, int currentLoopDAG)
        {
            int init_n = Initialize_loopDAG(graph, currentN, clsLoop, currentLoop);
            clsLoopDAG.loopDAG[currentLoopDAG].loopDAG = new GraphVariables.clsLoopDAG.strLoopDAGInfo[init_n];
            clsLoopDAG.loopDAG[currentLoopDAG].nLoopDAG = 0;

            //clsLoopDAG.loopDAG[currentLoopDAG].loopDAG[1].DAG.header;

            //int count_loopDAG = 0;

            for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++)
            {
                gProAnalyzer.GraphVariables.clsLoop.strLoopInform loop = clsLoop.Loop[currentLoop].Loop[i];
                if (loop.nEntry == 1) //NL
                {
                    gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.acyclicNet, ref clsLoop, currentLoop, i, ref clsSESE, "FF", -1);
                }
                else //IL
                {

                }
            }
        }

        public void build_DAPST(ref GraphVariables.clsGraph graph, int currentN)
        {
            //Combine HWLS and Loop decomposition subgraphs. (Proved unique?)
        }

        private int Initialize_loopDAG(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int currentLoop)
        {
            int countDAG = 0;
            for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++)
            {
                if (clsLoop.Loop[currentLoop].Loop[i].nEntry == 1) countDAG += 2;
                else countDAG += 3;
            }
            return countDAG;
        }

        private void store_loopDAG(ref GraphVariables.clsGraph graph, int currentLoop, ref GraphVariables.clsLoopDAG clsLoopDAG, 
            int currentLoopDAG, string typeDAG) //SESE?
        {
            //store loopDAG from Network[] to loopDAG[]
            //clsLoopDAG.loopDAG[currentLoopDAG].loopDAG
        }

        //Build behavior profile for each header of each decomposed acyclic structure. each matrix for each pair?
        public static void get_InitialBehaviorProfile(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsHWLS clsHWLS,
            ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsSESE clsSESE, int workSESE, bool[] flag_Check)
        {
            //input: a decomposed DAG
            //output: behavior profile of it header and its children

            //makeSubNetwork of clsDASPT
            //make instantFlow (AnalyseBehavior_InstF) of HEADER and its childs.
            //create behaviorProfile matrix for HEADER

            Initialize_All();
            //bool[] flag_Check = new bool[8];

            int curDepth = clsHWLS.FBLOCK.maxDepth;
            //curDepth = 1;
            do {
                for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                    if (clsHWLS.FBLOCK.FBlock[i].depth != curDepth) continue;
                    int orgIndx = clsHWLS.FBLOCK.FBlock[i].refIndex;

                    //If SESE => Make subnetwork and find Dominator Behavior relation matrix (1 matrix)
                    if (clsHWLS.FBLOCK.FBlock[i].SESE) {
                        //Make subnet 
                        gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.subNet, ref clsLoop, workSESE, orgIndx, ref clsSESE, "SESE", -1);

                        int nNode = graph.Network[graph.subNet].nNode;
                        int x = graph.Network[graph.subNet].Node[graph.Network[graph.subNet].header].Post[0];
                        bool[] returnBhPrfl = new bool[8];

                        clsHWLS.FBLOCK.FBlock[i].Dom_BhPrfl = new int[9, nNode + 1];
                        clsHWLS.FBLOCK.FBlock[i].Dom_BhPrfl[0, 0] = graph.Network[graph.subNet].Node[x].orgNum; //ENTRY SESE org index.
                        clsHWLS.FBLOCK.FBlock[i].nDomBh = 1;
                        //get behavior matrix of SESE ENTRY
                        for (int j = 0; j < graph.Network[graph.subNet].nNode; j++) {
                            int y = j;
                            if (x == y || graph.Network[graph.subNet].Node[x].orgNum == -1) continue; //Exclude VS, VE
                            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x, y, ref returnBhPrfl, flag_Check);
                            store_BehaviorProfile(ref clsHWLS, i, graph.Network[graph.subNet].Node[x].orgNum, graph.Network[graph.subNet].Node[y].orgNum, returnBhPrfl, true);
                        }
                        //reduce current Block LASTLY (already imply by depth pick)
                        gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, currentN, clsSESE, workSESE, orgIndx);                       
                    }

                    //If NL => Make subnetwork and find Dom/ Pdom Behavior relation matrix (2 matrices)
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsLoop.Loop[workLoop].Loop[orgIndx].nEntry == 1) {
                        //Make subnet
                        gProAnalyzer.Ultilities.makeSubNetwork.make_AcyclicSubGraph(ref graph, currentN, graph.subNet, ref clsLoop, workLoop, orgIndx, "NL");

                        int nNode = graph.Network[graph.subNet].nNode;
                        int x, y;

                        //get behavior matrix of HEADER ENTRY and its childs
                        x = graph.Network[graph.subNet].Node[graph.Network[graph.subNet].header].Post[0]; //Get NL Loop HEADER
                        bool[] returnBhPrfl = new bool[8];
                        clsHWLS.FBLOCK.FBlock[i].Dom_BhPrfl = new int[9, nNode + 2]; //store itself behavior
                        clsHWLS.FBLOCK.FBlock[i].Dom_BhPrfl[0, 0] = graph.Network[graph.subNet].Node[x].orgNum; //Header org index.
                        clsHWLS.FBLOCK.FBlock[i].nDomBh = 1;
                        for (int j = 0; j < graph.Network[graph.subNet].nNode; j++) {
                            y = j;
                            if (x == y || graph.Network[graph.subNet].Node[x].orgNum == -1) continue;
                            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x, y, ref returnBhPrfl, flag_Check);
                            store_BehaviorProfile(ref clsHWLS, i, graph.Network[graph.subNet].Node[x].orgNum, graph.Network[graph.subNet].Node[y].orgNum, returnBhPrfl, true);
                        }

                        //get behavior matrix of Its childs and HEADER (Pdom)                       
                        y = get_NodeByName(graph, graph.subNet, "V_HEADER");
                        if (y == -1) continue;
                        returnBhPrfl = new bool[8];
                        clsHWLS.FBLOCK.FBlock[i].Pdom_BhPrfl = new int[9, nNode + 2];
                        clsHWLS.FBLOCK.FBlock[i].Pdom_BhPrfl[0, 0] = graph.Network[graph.subNet].Node[y].orgNum; //Header index.
                        clsHWLS.FBLOCK.FBlock[i].nPdomBh = 1;
                        for (int j = 0; j < graph.Network[graph.subNet].nNode; j++) {
                            x = j;
                            if (x == y || graph.Network[graph.subNet].Node[x].orgNum == -1) continue;
                            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x, y, ref returnBhPrfl, flag_Check);
                            store_BehaviorProfile(ref clsHWLS, i, graph.Network[graph.subNet].Node[y].orgNum, graph.Network[graph.subNet].Node[x].orgNum, returnBhPrfl, false);
                        }
                        //reduce current Block (NL)
                        gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, orgIndx, "", false);
                    }

                    //if IL => Find CIPd => Make subnetwork with CIPd and find Dom/ Pdom Behavior relation matrix (2 matrices)
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsLoop.Loop[workLoop].Loop[orgIndx].nEntry > 1) {
                        //Make subnet
                        gProAnalyzer.Ultilities.makeSubNetwork.make_AcyclicSubGraph(ref graph, currentN, graph.subNet, ref clsLoop, workLoop, orgIndx, "IL");

                        int nNode = graph.Network[graph.subNet].nNode;
                        int x, y;

                        //get behavior matrix of HEADER ENTRY and its childs
                        x = graph.Network[graph.subNet].Node[graph.Network[graph.subNet].header].Post[0]; //Get CIPd of IL
                        clsHWLS.FBLOCK.FBlock[i].CIPd = graph.Network[graph.subNet].Node[x].orgNum;
                        bool[] returnBhPrfl = new bool[8];
                        clsHWLS.FBLOCK.FBlock[i].Dom_BhPrfl = new int[9, nNode + 2]; //store itself behavior
                        clsHWLS.FBLOCK.FBlock[i].Dom_BhPrfl[0, 0] = graph.Network[graph.subNet].Node[x].orgNum; //CIPd org index.
                        clsHWLS.FBLOCK.FBlock[i].nDomBh = 1;
                        for (int j = 0; j < graph.Network[graph.subNet].nNode; j++) {
                            y = j;
                            if (x == y || graph.Network[graph.subNet].Node[x].orgNum == -1) continue;
                            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x, y, ref returnBhPrfl, flag_Check);
                            store_BehaviorProfile(ref clsHWLS, i, graph.Network[graph.subNet].Node[x].orgNum, graph.Network[graph.subNet].Node[y].orgNum, returnBhPrfl, true);
                        }
                        //combine from CID(x, y) ==>> MISSING => SOLVED (no need)

                        //get behavior matrix of Its childs and HEADER (Pdom)                       
                        y = get_NodeByName(graph, graph.subNet, "V_CIPd");
                        if (y == -1) continue;
                        returnBhPrfl = new bool[8];
                        clsHWLS.FBLOCK.FBlock[i].Pdom_BhPrfl = new int[9, nNode + 2];
                        clsHWLS.FBLOCK.FBlock[i].Pdom_BhPrfl[0, 0] = graph.Network[graph.subNet].Node[y].orgNum; //Header index.
                        clsHWLS.FBLOCK.FBlock[i].nPdomBh = 1;
                        for (int j = 0; j < graph.Network[graph.subNet].nNode; j++) {
                            x = j;
                            if (x == y || graph.Network[graph.subNet].Node[x].orgNum == -1) continue;
                            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x, y, ref returnBhPrfl, flag_Check);
                            store_BehaviorProfile(ref clsHWLS, i, graph.Network[graph.subNet].Node[y].orgNum, graph.Network[graph.subNet].Node[x].orgNum, returnBhPrfl, false);
                        }
                        //combine from CID(x, y) ==>> MISSING => SOLVED (no need)

                        //reduce current Block (IL)
                        gProAnalyzer.Ultilities.reduceGraph.reduce_IrLoop(ref graph, currentN, clsLoop, workLoop, orgIndx);
                    }
                }
                curDepth--;
            } while (curDepth > 0);
        }

        public static void store_BehaviorProfile(ref GraphVariables.clsHWLS clsHWLS, int currHWLS, int orgX, int orgY, bool[] returnBhPrfl, bool isDomBH)
        {            
            int nDomBhIndx = clsHWLS.FBLOCK.FBlock[currHWLS].nDomBh;
            int nPdomBhIndx = clsHWLS.FBLOCK.FBlock[currHWLS].nPdomBh;

            if (isDomBH) {
                //#row of Dom_BhPrfl
                for (int i = 0; i < 9; i++) {
                    if (i == 0)
                        clsHWLS.FBLOCK.FBlock[currHWLS].Dom_BhPrfl[i, nDomBhIndx] = orgY;
                    else {
                        if (returnBhPrfl[i - 1] == true)
                            clsHWLS.FBLOCK.FBlock[currHWLS].Dom_BhPrfl[i, nDomBhIndx] = 1;
                        else
                            clsHWLS.FBLOCK.FBlock[currHWLS].Dom_BhPrfl[i, nDomBhIndx] = 0;
                    }
                }
                nDomBhIndx++;
                clsHWLS.FBLOCK.FBlock[currHWLS].nDomBh = nDomBhIndx;
            }
            else {
                //#row of Pdom_BhPrfl
                for (int i = 0; i < 9; i++) {
                    if (i == 0)
                        clsHWLS.FBLOCK.FBlock[currHWLS].Pdom_BhPrfl[i, nPdomBhIndx] = orgY;
                    else {
                        if (returnBhPrfl[i - 1] == true)
                            clsHWLS.FBLOCK.FBlock[currHWLS].Pdom_BhPrfl[i, nPdomBhIndx] = 1;
                        else
                            clsHWLS.FBLOCK.FBlock[currHWLS].Pdom_BhPrfl[i, nPdomBhIndx] = 0;
                    }
                }
                nPdomBhIndx++;
                clsHWLS.FBLOCK.FBlock[currHWLS].nPdomBh = nPdomBhIndx;
            }
        }

        public static int get_NodeByName(GraphVariables.clsGraph graph, int currentN, string name)
        {
            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                if (graph.Network[currentN].Node[i].Name == name)
                    return i;
            }
            return -1;
        }

        //might not use
        private void reduceAllChild(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, int currBlock, GraphVariables.clsLoop clsLoop, int workLoop, 
            GraphVariables.clsSESE clsSESE, int workSESE, int orgIndex)
        {
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                if (clsHWLS.FBLOCK.FBlock[i].parentBlock == currBlock) {
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == true)
                        gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, currentN, clsSESE, workSESE, orgIndex);
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsLoop.Loop[workLoop].Loop[orgIndex].nEntry == 1)
                        gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, orgIndex, "", false);
                    //if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsLoop.Loop[workLoop].Loop[orgIndex].nEntry > 1)
                        //reduceG.reduce_IrLoop(ref graph, currentN, clsLoop, workLoop, orgIndex);
                }
            }
        }

    }
}