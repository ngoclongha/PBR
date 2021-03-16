using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace gProAnalyzer
{
    class Run_Analysis_Verification
    {
        private gProAnalyzer.Functionalities.NodeSplittingType1 SplitType1;
        private gProAnalyzer.Functionalities.NodeSplittingType2 SplitType2;
        private gProAnalyzer.Functionalities.NodeSplittingType3 SplitType3;
        private gProAnalyzer.Functionalities.LoopIdentification findLoop;
        private gProAnalyzer.Functionalities.DominanceIdentification fndDomRel;
        private gProAnalyzer.Functionalities.SESEIdentification sese;
        private gProAnalyzer.Functionalities.PolygonIdentification polygon;
        private gProAnalyzer.Ultilities.makeInstanceFlow makInst;
        private gProAnalyzer.Ultilities.makeSubNetwork makSubNet;
        private gProAnalyzer.Ultilities.AnalyseBehavior_InstF anlyzBh_InstF;
        private gProAnalyzer.Ultilities.reduceGraph reduceG;
        private gProAnalyzer.Ultilities.extendGraph extendG;
        private gProAnalyzer.Ultilities.makeNestingForest makNestingForest;
        private gProAnalyzer.Functionalities.UntanglingIL makUntangling;
        private gProAnalyzer.Ultilities.copyLoop copyL;
        private gProAnalyzer.Ultilities.copySESE copySE;
        private gProAnalyzer.Ultilities.extendGraph extGraph;
        private gProAnalyzer.Ultilities.checkGraph checkG;

        public double[] informList = new double[30]; //now max[23]
        public int[] CIPd_SS_node;
        public double[] ext_informlist = new double[10];
        public double[] ext_informlist_2 = new double[10];
        public double[] ext_informlist_3_IL = new double[10];
        public int[] type_1_4 = new int[8];
        public double[] run_Time = new double[6];
        public int total_inst = 0;

        public int[] rig;
        public int[] rig_01;
        public int[] GW_Count_S_J; //total gateway S/J in rigid only (exclude it childs)
        public int[] bond_rigid_PdF;
        public int[] OR_join_total;  //[0] Bond, [1] Rigid
        public int[] XOR_join_total; //count total XOR-join of exit of BOND/ RIGID
        public int[] XOR_join_err;   //[0] Bond; [1] Rigid; [2] Other
        public int[] AND_join_total; //count total AND-join of exit of BOND/ RIGID
        public int[] AND_join_err;   //[0] Bond; [1] Rigid; [2] Other

        public int[] XOR_entry; //[0] Bond_Entry [1] Bond_Entry_Corrected [2] Rigid_entry [3] Rigid_entry_corrected;
        public int[] AND_entry;
        public int[] OR_entry;

        public int[] XOR_Loop; //[0] entry_total; [1] entry_error; [2] ex_total; [3] ex_err; [4] bs_total; [5] bs_err; [6] bsEx_total; [7] bsEx_err
        public int[] AND_Loop;
        public int[] OR_Loop;
        public int[] En_Ex_Type;

        public int[] Real_err;      //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; [4] fwd; [5] bwd
        public int[] Potential_err; //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; [4] fwd; [5] bwd
        public int[] Dominated_err; //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; [4] fwd; [5] bwd
        public int[] Dominated_err_Potential;

        public int[] XOR_err_efwd_ebwd; //[0][1] only
        public int[] AND_err_efwd_ebwd;

        public int CCs; //connected components
        public bool dup_label;

        public double finalTime;

        public bool SyntaxError_GW;
        public bool SyntaxError_EV;

        public frmAnalysisNetwork frmAnl;

        public void Initialize_All()
        {
            //SplitType1 = new gProAnalyzer.Functionalities.NodeSplittingType1();
            //SplitType2 = new gProAnalyzer.Functionalities.NodeSplittingType2();
            //SplitType3 = new gProAnalyzer.Functionalities.NodeSplittingType3();

            //findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
            //fndDomRel = new gProAnalyzer.Functionalities.DominanceIdentification();
            //sese = new gProAnalyzer.Functionalities.SESEIdentification();
            //polygon = new gProAnalyzer.Functionalities.PolygonIdentification();
            //makSubNet = new gProAnalyzer.Ultilities.makeSubNetwork();

            //makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
            //anlyzBh_InstF = new gProAnalyzer.Ultilities.AnalyseBehavior_InstF();
            //reduceG = new gProAnalyzer.Ultilities.reduceGraph();
            //extendG = new gProAnalyzer.Ultilities.extendGraph();
            //makNestingForest = new gProAnalyzer.Ultilities.makeNestingForest();
            //makUntangling = new gProAnalyzer.Functionalities.UntanglingIL();
            //copyL = new gProAnalyzer.Ultilities.copyLoop();
            //copySE = new gProAnalyzer.Ultilities.copySESE();
            //checkG = new gProAnalyzer.Ultilities.checkGraph();

            //frmAnl = new frmAnalysisNetwork();

            ext_informlist = new double[10];
        }

        public int run_VerificationG(ref GraphVariables.clsGraph graph, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsLoop clsLoop, 
            ref GraphVariables.clsSESE clsSESE, ref GraphVariables.clsError clsError, int numRun)
        {
            int errorNum = 0;
            HiPerfTimer pt = new HiPerfTimer();
            pt.Start();
            double time = 0;

            changeSS_Type(graph, graph.orgNet, "OR"); //no need to record computation time.
            
            System.Diagnostics.Stopwatch watch_total = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch watch_verify = new System.Diagnostics.Stopwatch();
            //System.Diagnostics.Stopwatch watch_Pre = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch watch_SESE = new System.Diagnostics.Stopwatch();            
            run_Time = new double[6];

            //Initialize_All();
            
            //if (SyntaxError_EV) errorNum += 4;
            int[] Mark = new int[graph.Network[graph.baseNet].nNode];
            CCs = 1;
            Ultilities.checkGraph.find_ConnectedComponents(graph, graph.baseNet, ref CCs, ref Mark);
            if (CCs > 1) errorNum = 6;

            check_SyntaxError(graph, graph.orgNet);
            //check_SyntaxError_2(graph, graph.orgNet);           
            if (SyntaxError_GW) errorNum = 3; //priority of SYNTAX ERROR for filtering
            
            for (int i = 0; i < numRun; i++)
            {
                watch_total.Reset();
                watch_total.Start();

                watch_SESE.Reset();
                watch_SESE.Start();
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
                watch_SESE.Stop();
                run_Time[1] += watch_SESE.ElapsedMilliseconds;

                watch_verify.Reset();
                watch_verify.Start();
                gProAnalyzer.Functionalities.VerificationG.Initialize_Verification(ref graph, ref clsError, ref clsLoop, ref clsSESE, ref clsHWLS); //Verification checking

                watch_verify.Reset();
                watch_verify.Start();

                int total_int = 0;
                total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.finalNet, -2, "SESE", "", ref clsError);

                watch_verify.Stop();
                run_Time[2] += watch_verify.ElapsedMilliseconds;

                run_Time[3] += Functionalities.VerificationG.watch_Bond.ElapsedMilliseconds; //time prcess bond RUN ONLY ONCE BOND
                run_Time[4] += Functionalities.VerificationG.watch_Rigid.ElapsedMilliseconds; //time process rigid RUNE ONLY ONCE RIGID
                run_Time[5] += Functionalities.VerificationG.watch_IL.ElapsedMilliseconds;

                watch_total.Stop();
                run_Time[0] += watch_total.ElapsedMilliseconds;
            }

            if (gProAnalyzer.Functionalities.VerificationG.iL_concurrency_Error == true && SyntaxError_GW == false) errorNum = 2; //concurrency errors_num = 2;


            informList = new double[30];
            count_Bonds_Rigids(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS); //label B,R,P //#B, R, P
            informRecording(graph, graph.finalNet); //informList[]
            multiStart_CIPD_Count(graph, graph.orgNet, graph.orgNet, clsSESE.finalSESE);
            multiStart_CIPD_Count_FinalNet(graph, graph.finalNet, graph.finalNet, clsSESE, clsSESE.finalSESE); //only get the list_of_node_CIPd(SS)

            //June10_2020: count type 1-4 bonds/rigid
            count_Bonds_Rigids_Type_1_4(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS);


            //Record data for each rigid (get the big one only)
            rig = new int[6];
            rig = diagnostic_rigids(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS);
            if (rig == null) rig = new int[6];

            //couting more on Rigids
            graph.Network[graph.reduceNet] = graph.Network[graph.finalNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            rig_01 = new int[2];
            GW_Count_S_J = new int[2];
            rig_01 = diagnostic_rigids_2(ref graph, graph.reduceNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS, ref GW_Count_S_J);
            
            //Error counting for Bond and Rigid
            //counting_Error_type(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS, ref clsError); // Count all SESE
            //counting_Error_type_SOS_SESE(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS, ref clsError); // Count only SOS SESE
            counting_Error_type_NOT_SOS_SESE(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS, ref clsError); // Count not_SOS SESE

            int temp = Ultilities.checkGraph.check_real_errors(graph, graph.finalNet, clsHWLS, clsLoop, clsLoop.orgLoop, ref clsError, -1);
            //counting error (real, potential, dominance)
            counting_Error_type_Real_Potential_Dominated(ref graph, graph.finalNet, ref clsError);

            graph.Network[graph.reduceNet] = graph.Network[graph.finalNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            more_NL_Info(graph, graph.reduceNet, clsLoop, clsLoop.orgLoop, clsSESE, clsError);

            graph.Network[graph.reduceNet] = graph.Network[graph.finalNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            more_NL_Info_2(graph, graph.reduceNet, clsLoop, clsLoop.orgLoop, clsSESE, clsError);

            //optional-check label duplication
            dup_label = check_labelDuplication(graph, graph.orgNet);

            //more_IL_Info_2

            run_Time[0] = run_Time[0] / numRun; //total time
            run_Time[1] = run_Time[1] / numRun;
            run_Time[2] = run_Time[2] / numRun;
            run_Time[3] = run_Time[3] / numRun;
            run_Time[4] = run_Time[4] / numRun;
            run_Time[5] = run_Time[5] / numRun;
            
            total_inst = gProAnalyzer.Functionalities.VerificationG.total_int;
            
            //MessageBox.Show("The verification finish in: " + watch_verify.ElapsedMilliseconds.ToString() + " milisecond", "Verification of process model");
            //MessageBox.Show("The Whole System has finished in: " + (watch.ElapsedMilliseconds / numRun).ToString() + " milisecond", "Whole system");
             
            return errorNum;
        }

        public void count_Bonds_Rigids(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsHWLS clsHWLS)
        {
            gProAnalyzer.Ultilities.copySESE.copy_SESE(ref clsSESE, workSESE, clsSESE.tempSESE);
            int currentN = graph.tempNet; //just assign a temporary variable.
            graph.Network[currentN] = graph.Network[finalNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, currentN, 0, 0);
            int curDepth = clsHWLS.FBLOCK.maxDepth;
            do
            {
                for (int j = 0; j < clsHWLS.FBLOCK.nFBlock; j++)
                {
                    if (clsHWLS.FBLOCK.FBlock[j].depth != curDepth) continue;

                    int i = clsHWLS.FBLOCK.FBlock[j].refIndex;

                    if (clsHWLS.FBLOCK.FBlock[j].SESE)
                    {                        
                        if (gProAnalyzer.Ultilities.checkGraph.Bond_Check(ref graph, currentN, ref clsSESE, workSESE, i, ref clsHWLS) == "B") //bond model
                        {
                            informList[13]++;
                            clsHWLS.FBLOCK.FBlock[j].type = "B";
                            clsSESE.SESE[workSESE].SESE[i].type = "B";
                        }
                        if (gProAnalyzer.Ultilities.checkGraph.Bond_Check(ref graph, currentN, ref clsSESE, workSESE, i, ref clsHWLS) == "R") //rigid model
                        {
                            informList[14]++;
                            clsHWLS.FBLOCK.FBlock[j].type = "R";
                            clsSESE.SESE[workSESE].SESE[i].type = "R";
                        }
                        if (gProAnalyzer.Ultilities.checkGraph.Bond_Check(ref graph, currentN, ref clsSESE, workSESE, i, ref clsHWLS) == "P")
                        {
                            informList[12]++;
                            clsHWLS.FBLOCK.FBlock[j].type = "P";
                            clsSESE.SESE[workSESE].SESE[i].type = "P";
                        }
                        gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, currentN, clsSESE, clsSESE.tempSESE, i);
                    }
                    else
                    {
                        if (clsLoop.Loop[workLoop].Loop[i].Irreducible) // Irreducible Loop면
                        {
                            //gProAnalyzer.Ultilities.reduceGraph.reduce_IrLoop(ref graph, currentN, clsLoop, workLoop, i);
                            //reduce_Network(currentN, workLoop, i, "", true);
                        }
                        else // Natural Loop면
                        {
                            //Natural Loop have single exit also an SESE; solution => Check L<h> whether the children is single entry single exit or not. 
                            if (clsLoop.Loop[workLoop].Loop[i].nEntry == 1 && clsLoop.Loop[workLoop].Loop[i].nExit == 1)
                            {
                                bool checkBond_Loop = true;
                                int count_gateway = 0;
                                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nNode; k++)
                                {
                                    int node = clsLoop.Loop[workLoop].Loop[i].Node[k];
                                    if (graph.Network[currentN].Node[node].nPre > 1 || graph.Network[currentN].Node[node].nPost > 1)
                                    {
                                        count_gateway++;
                                        if (count_gateway > 2) checkBond_Loop = false;
                                    }
                                }
                                if (checkBond_Loop)
                                {
                                    for (int k = 0; k < clsHWLS.FBLOCK.FBlock[j].nChild; k++)
                                    {
                                        int childLoop = clsHWLS.FBLOCK.FBlock[j].child[k]; //maybe it is SESE
                                        if (clsHWLS.FBLOCK.FBlock[childLoop].nEntry > 1 || clsHWLS.FBLOCK.FBlock[childLoop].nExit > 1)
                                        {
                                            checkBond_Loop = false;
                                            break;
                                        }
                                    }
                                }
                                if (checkBond_Loop)
                                {
                                    //informList[13]++;
                                    //clsHWLS.FBLOCK.FBlock[j].type = "B";
                                }
                                else
                                {
                                    //informList[14]++;
                                    //clsHWLS.FBLOCK.FBlock[j].type = "R";
                                }
                            }
                            gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, i, "", true);
                        }
                    }
                }
                curDepth--;
            } while (curDepth > 0);
        }

        public void count_Bonds_Rigids_Type_1_4(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsHWLS clsHWLS)
        {
            type_1_4 = new int[10]; //Global variable
            int type1 = 0; //NL bonds
            int type2 = 0; //NL rigids
            int type3 = 0; //NL IL rigids
            int type4 = 0; //acyckuc bond/ rigids
            int type4_b = 0;
            int type4_r = 0;
            //=========<<====
            int type4_b_nl = 0;
            int type4_b_il = 0;
            int type4_r_nl = 0;
            int type4_r_il = 0;

            gProAnalyzer.Ultilities.copySESE.copy_SESE(ref clsSESE, workSESE, clsSESE.tempSESE);
            int currentN = graph.tempNet; //just assign a temporary variable.
            graph.Network[currentN] = graph.Network[finalNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, currentN, 0, 0); //work with currentN only

            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
            {
                //type1 & type2
                if (clsHWLS.FBLOCK.FBlock[i].SESE == false && clsHWLS.FBLOCK.FBlock[i].nEntry == 1 && clsHWLS.FBLOCK.FBlock[i].nExit == 1)
                {
                    int curLoop = clsHWLS.FBLOCK.FBlock[i].refIndex;
                    //check the number of bs node could decide it is the bond or rigid
                    if (clsLoop.Loop[workLoop].Loop[curLoop].nBackSplit == 1) type1++;
                    else type2++;
                }
                //type3 & refined type4
                if (clsHWLS.FBLOCK.FBlock[i].SESE == true)
                {
                    if (clsHWLS.FBLOCK.FBlock[i].type == "P") continue; //not consider polygon
                    int en = clsHWLS.FBLOCK.FBlock[i].Entry[0];
                    int ex = clsHWLS.FBLOCK.FBlock[i].Exit[0];
                    if (graph.Network[currentN].Node[en].nPre > 1 || graph.Network[currentN].Node[ex].nPost > 1)
                        type3++;
                    else
                    {
                        type4++;
                        if (clsHWLS.FBLOCK.FBlock[i].type == "B")                        
                            type4_b++;                            
                        if (clsHWLS.FBLOCK.FBlock[i].type == "R")                        
                            type4_r++;                                                    
                        //count NL IL inside this rigid ON_HOLD_FOR_FURTHER_INSTRUCTION
                        count_loop_insideSESE(graph, currentN, clsHWLS, i, ref type4_b_nl, ref type4_b_il, ref type4_r_nl, ref type4_r_il);
                    }  
                }                    
            }
            type_1_4[0] = type1;
            type_1_4[1] = type2;
            type_1_4[2] = type3;
            type_1_4[3] = type4;
            type_1_4[4] = type4_b;
            type_1_4[5] = type4_r;
            type_1_4[6] = type4_b_nl;
            type_1_4[7] = type4_b_il;
            type_1_4[8] = type4_r_nl;
            type_1_4[9] = type4_r_il;

        }

        public void count_loop_insideSESE(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, int currSESE, ref int b_nl, ref int b_il, ref int r_nl, ref int r_il)
        {
            int[] curSESE_child = new int[clsHWLS.FBLOCK.nFBlock];
            int nCurSESE_child = 0;
            //build a list of direct child of currSESE (bypass polygon childs)
            for (int i = 0; i < clsHWLS.FBLOCK.FBlock[currSESE].nChild; i++)
            {
                int child = clsHWLS.FBLOCK.FBlock[currSESE].child[i];
                if (clsHWLS.FBLOCK.FBlock[child].type != "P")
                {
                    curSESE_child[nCurSESE_child] = child;
                    nCurSESE_child++;
                }
                else
                {
                    //collect the child of directed polygon of currSESE
                    for (int j = 0; j < clsHWLS.FBLOCK.FBlock[child].nChild; j++)
                    {
                        int child2 = clsHWLS.FBLOCK.FBlock[currSESE].child[i];
                        if (clsHWLS.FBLOCK.FBlock[child2].type != "P")
                        {
                            curSESE_child[nCurSESE_child] = child2;
                            nCurSESE_child++;
                        }
                    }
                }
            }

            for (int i = 0; i < nCurSESE_child; i++)
            {
                int child = curSESE_child[i];
                int parent = clsHWLS.FBLOCK.FBlock[child].parentBlock;
                if (clsHWLS.FBLOCK.FBlock[parent].type == "P") parent = clsHWLS.FBLOCK.FBlock[parent].parentBlock;
                //if i is a polygon => jump to its child
                if (clsHWLS.FBLOCK.FBlock[child].SESE == false)
                {
                    //only check direct child of currentSESE
                    if (clsHWLS.FBLOCK.FBlock[child].nEntry == 1 && clsHWLS.FBLOCK.FBlock[child].nExit > 1 && clsHWLS.FBLOCK.FBlock[parent].type == "B") b_nl++;
                    if (clsHWLS.FBLOCK.FBlock[child].nEntry > 1 && clsHWLS.FBLOCK.FBlock[parent].type == "B") b_il++;

                    if (clsHWLS.FBLOCK.FBlock[child].nEntry == 1 && clsHWLS.FBLOCK.FBlock[child].nExit > 1 && clsHWLS.FBLOCK.FBlock[parent].type == "R") r_nl++;
                    if (clsHWLS.FBLOCK.FBlock[child].nEntry > 1 && clsHWLS.FBLOCK.FBlock[parent].type == "R") r_il++;
                    //count_loop_insideSESE(graph, currentN, clsHWLS, child, ref nl, ref il);
                }
            }
        }


        public int[] diagnostic_rigids(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, 
            int workLoop, ref GraphVariables.clsHWLS clsHWLS)
        {
            int nNodeMaxRigid = 0;
            int nNodeRigid_Reduce = 0;
            int nSESEInside = 0;
            int nNodeSESE = 0;
            int nLoopInside = 0;
            int nNodeLoop = 0;

            //find biggest rigid
            int idx_rigid = -1;
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
            {
                if (clsHWLS.FBLOCK.FBlock[i].type == "R" && nNodeMaxRigid < clsHWLS.FBLOCK.FBlock[i].nNode)
                {
                    nNodeMaxRigid = clsHWLS.FBLOCK.FBlock[i].nNode;
                    idx_rigid = i;
                }
            }

            if (idx_rigid == -1) return null;
            //check that rigid
            //count bond/ rigid inside
            int adj_nNodeSESE = 0;
            int adj_nNodeLoop = 0;
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
            {
                if (clsHWLS.FBLOCK.FBlock[i].parentBlock == idx_rigid)
                {
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == true && clsHWLS.FBLOCK.FBlock[i].type != "P")
                    {
                        nSESEInside++;
                        adj_nNodeSESE++;
                        nNodeSESE = nNodeSESE + clsHWLS.FBLOCK.FBlock[i].nNode;
                    }
                    if (clsHWLS.FBLOCK.FBlock[i].SESE == false)
                    {
                        nLoopInside++;
                        if (clsHWLS.FBLOCK.FBlock[i].nEntry > 1) adj_nNodeLoop += 2;
                        else adj_nNodeLoop++;
                        nNodeLoop = nNodeLoop + clsHWLS.FBLOCK.FBlock[i].nNode;
                    }
                }
            }
            
            //count loop inside
            nNodeRigid_Reduce = nNodeMaxRigid - nNodeSESE - nNodeLoop + (adj_nNodeSESE + adj_nNodeLoop);
            int[] rg = new int[6];
            rg[0] = nNodeMaxRigid;
            rg[1] = nNodeRigid_Reduce;
            rg[2] = nSESEInside;
            rg[3] = nNodeSESE;
            rg[4] = nLoopInside;
            rg[5] = nNodeLoop;

            return rg;
        }

        public int[] diagnostic_rigids_2(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsHWLS clsHWLS, ref int[] GW_Count_S_J)
        {
            int[] OR_Edge_Rigid = new int[2]; //[0]: OR-split [1]: #ofOut edges
            GW_Count_S_J = new int[2]; //[0] split; [1] join
            int curDepth = clsHWLS.FBLOCK.maxDepth;
            string strLoop = "";
            string strOrgLoop = strLoop;

            do
            {
                for (int j = 0; j < clsHWLS.FBLOCK.nFBlock; j++)
                {
                    int i = clsHWLS.FBLOCK.FBlock[j].refIndex;
                    if (strOrgLoop == "") strLoop = i.ToString();
                    else strLoop = strOrgLoop + "-" + i.ToString();
                    if (clsHWLS.FBLOCK.FBlock[j].depth != curDepth) continue;
                    if (clsHWLS.FBLOCK.FBlock[j].SESE)
                    {
                        if (clsHWLS.FBLOCK.FBlock[j].type == "R" && clsHWLS.FBLOCK.FBlock[j].isProcessed == true)
                        {                            
                            for (int k = 0; k < clsHWLS.FBLOCK.FBlock[j].nNode; k++)
                            {
                                //count OR-split //Count outgoing edges of OR-split
                                if (graph.Network[finalNet].Node[clsHWLS.FBLOCK.FBlock[j].Node[k]].Kind == "OR" && graph.Network[finalNet].Node[clsHWLS.FBLOCK.FBlock[j].Node[k]].nPost > 1)
                                {
                                    OR_Edge_Rigid[0]++;
                                    OR_Edge_Rigid[1] += graph.Network[finalNet].Node[clsHWLS.FBLOCK.FBlock[j].Node[k]].nPost;
                                }
                                if (graph.Network[finalNet].Node[clsHWLS.FBLOCK.FBlock[j].Node[k]].nPost > 1)
                                    GW_Count_S_J[0]++;
                                if (graph.Network[finalNet].Node[clsHWLS.FBLOCK.FBlock[j].Node[k]].nPre > 1)
                                    GW_Count_S_J[1]++;
                            }                            
                        }
                        gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, finalNet, clsSESE, workSESE, i);
                    }
                    if (clsHWLS.FBLOCK.FBlock[j].SESE == false)
                    {
                        gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, finalNet, ref clsLoop, workLoop, i, strLoop, true);
                    }
                }                
                curDepth--;
            } while (curDepth > 0);

            return OR_Edge_Rigid;
        }

        //Check error[27] [28] //Bond-Rigid
        public void counting_Error_type(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsError clsError) 
        {
            OR_join_total = new int[5]; // Bond/rigid/other //[3] OR_Bond_EOJ [4] OR_RIGID_EOJ
            XOR_join_total = new int[3]; //count total XOR-join of exit of BOND/ RIGID 
            XOR_join_err = new int[3]; //[0] Bond; [1] Rigid; [2] Other
            AND_join_total = new int[3]; //count total AND-join of exit of BOND/ RIGID
            AND_join_err = new int[3]; //[0] Bond; [1] Rigid; [2] Other

            XOR_entry = new int[4]; //[0] Bond_Entry [1] Bond_Entry_Corrected [2] Rigid_entry [3] Rigid_entry_corrected;
            AND_entry = new int[4];
            OR_entry = new int[4];

            for(int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].SESE == "") continue;
                int cur_SESE = Convert.ToInt32(clsError.Error[i].SESE);
                int cur_Node = Convert.ToInt32(clsError.Error[i].Node);
                
                if (clsError.Error[i].messageNum == 27) 
                {                   
                    //check exit of bond/rigid with LoS error
                    if (clsSESE.SESE[workSESE].SESE[cur_SESE].Exit == cur_Node)
                    {
                        if (clsSESE.SESE[workSESE].SESE[cur_SESE].type == "B")
                            XOR_join_err[0]++;
                        else
                            XOR_join_err[1]++;
                    }
                    else
                        XOR_join_err[2]++;
                }
                if (clsError.Error[i].messageNum == 28)
                {
                    //check exit of bond/rigid with DL error
                    if (clsSESE.SESE[workSESE].SESE[cur_SESE].Exit == cur_Node)
                    {
                        if (clsSESE.SESE[workSESE].SESE[cur_SESE].type == "B")
                            AND_join_err[0]++;
                        else
                            AND_join_err[1]++;
                    }
                    else
                        AND_join_err[2]++;
                }                
            }

            //Count total case;
            for (int i = 0; i < clsSESE.SESE[workSESE].nSESE; i++) //Might need to upgrade to check non-join SESE exit (e.g., loop exit as Split)
            {
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR")         
                    XOR_join_total[0]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR")
                    XOR_join_total[1]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                    AND_join_total[0]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                    AND_join_total[1]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR")
                {
                    OR_join_total[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                        OR_join_total[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR")
                {
                    OR_join_total[1]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                        OR_join_total[4]++;
                }
                //==== New for Entry of SESE (and corrected)
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                {
                    XOR_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) XOR_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                {
                    XOR_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) XOR_entry[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                {
                    AND_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) AND_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                {
                    AND_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) AND_entry[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                {
                    OR_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) OR_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                {
                    OR_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) OR_entry[3]++;
                }
            }

            //recode total XOR/ AND-join of [Other] - minus XOR_join_total[0] and XOR_join_total[1]; as well as AND_join_total;
            for (int i = 0; i < graph.Network[finalNet].nNode; i++)
            {
                if (graph.Network[finalNet].Node[i].Kind == "XOR" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                    XOR_join_total[2]++;
                if (graph.Network[finalNet].Node[i].Kind == "AND" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                    AND_join_total[2]++;
                if (graph.Network[finalNet].Node[i].Kind == "OR" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                    OR_join_total[2]++;

                //just for validation
                if (graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                    informList[25]++;
            }
            //final calibration:
            XOR_join_total[2] = XOR_join_total[2] - XOR_join_total[1] - XOR_join_total[0];
            AND_join_total[2] = AND_join_total[2] - AND_join_total[1] - AND_join_total[0];
            OR_join_total[2] = OR_join_total[2] - OR_join_total[1] - OR_join_total[0];

            //=================== Count loop's error in eFwd and eBwd
            XOR_err_efwd_ebwd = new int[2];
            AND_err_efwd_ebwd = new int[2];
            for (int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].messageNum == 3)
                    XOR_err_efwd_ebwd[0]++;
                if (clsError.Error[i].messageNum == 4)
                    XOR_err_efwd_ebwd[1]++;
                if (clsError.Error[i].messageNum == 5)
                    AND_err_efwd_ebwd[0]++;
                if (clsError.Error[i].messageNum == 6)
                    AND_err_efwd_ebwd[1]++;
            }
        }

        public void counting_Error_type_SOS_SESE(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsError clsError)
        {
            OR_join_total = new int[5]; // Bond/rigid/other // [3] OR_BOND_EOJ [4] OR_RIGID_EOJ
            XOR_join_total = new int[3]; //count total XOR-join of exit of BOND/ RIGID
            XOR_join_err = new int[3]; //[0] Bond; [1] Rigid; [2] Other
            AND_join_total = new int[3]; //count total AND-join of exit of BOND/ RIGID
            AND_join_err = new int[3]; //[0] Bond; [1] Rigid; [2] Other

            XOR_entry = new int[4]; //[0] Bond_Entry [1] Bond_Entry_Corrected [2] Rigid_entry [3] Rigid_entry_corrected;
            AND_entry = new int[4];
            OR_entry = new int[4];

            for (int i = 0; i < clsError.nError; i++)// only for SoS SESE
            {
                if (clsError.Error[i].SESE == "") continue;
                int cur_SESE = Convert.ToInt32(clsError.Error[i].SESE);
                int cur_Node = Convert.ToInt32(clsError.Error[i].Node);

                if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[cur_SESE].Entry].Name != "SS") continue; //only get SS_SESE Rigids only errors

                if (clsError.Error[i].messageNum == 27)
                {
                    //check exit of bond/rigid with LoS error
                    if (clsSESE.SESE[workSESE].SESE[cur_SESE].Exit == cur_Node)
                    {
                        if (clsSESE.SESE[workSESE].SESE[cur_SESE].type == "B")
                            XOR_join_err[0]++;
                        else
                            XOR_join_err[1]++;
                    }
                    else
                        XOR_join_err[2]++;
                }
                if (clsError.Error[i].messageNum == 28)
                {
                    //check exit of bond/rigid with DL error
                    if (clsSESE.SESE[workSESE].SESE[cur_SESE].Exit == cur_Node)
                    {
                        if (clsSESE.SESE[workSESE].SESE[cur_SESE].type == "B")
                            AND_join_err[0]++;
                        else
                            AND_join_err[1]++;
                    }
                    else
                        AND_join_err[2]++;
                }
            }

            //Count total case;
            for (int i = 0; i < clsSESE.SESE[workSESE].nSESE; i++) //Might need to upgrade to check non-join SESE exit (e.g., loop exit as Split)
            {
                if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Name != "SS") continue; //only get SS_SESE errors

                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR")                
                    XOR_join_total[0]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR")                
                    XOR_join_total[1]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                    AND_join_total[0]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                    AND_join_total[1]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR")
                {
                    OR_join_total[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                        OR_join_total[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR")
                {
                    OR_join_total[1]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                        OR_join_total[4]++;
                }
                //==== New for Entry of SESE (and corrected)
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                {
                    XOR_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) XOR_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                {
                    XOR_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) XOR_entry[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                {
                    AND_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) AND_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                {
                    AND_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) AND_entry[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                {
                    OR_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) OR_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                {
                    OR_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) OR_entry[3]++;
                }
            }

            //recode total XOR/ AND-join of [Other] - minus XOR_join_total[0] and XOR_join_total[1]; as well as AND_join_total;
            for (int k = 0; k < clsSESE.SESE[workSESE].nSESE; k++)
            {
                if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[k].Entry].Name != "SS") continue; //only get SS_SESE errors

                for (int i = 0; i < graph.Network[finalNet].nNode; i++)
                {
                    if (!Ultilities.checkGraph.Node_In_SESE(clsSESE, workSESE, i, k)) continue; //if this node not in current SS_rigid => ignore
                    if (i == clsSESE.SESE[workSESE].SESE[k].Entry || i == clsSESE.SESE[workSESE].SESE[k].Exit) continue; //not counting any entry/exit of SESE
                    if (graph.Network[finalNet].Node[i].Kind == "XOR" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        XOR_join_total[2]++;
                    if (graph.Network[finalNet].Node[i].Kind == "AND" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        AND_join_total[2]++;
                    if (graph.Network[finalNet].Node[i].Kind == "OR" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        OR_join_total[2]++;

                    //just for validation
                    if (graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        informList[25]++;
                }
            }
            //final calibration:
            //XOR_join_total[2] = XOR_join_total[2] - XOR_join_total[1];// -XOR_join_total[0]; not count bond => not minus
            //AND_join_total[2] = AND_join_total[2] - AND_join_total[1];// -AND_join_total[0];
            //OR_join_total[2] = OR_join_total[2] - OR_join_total[1];// -OR_join_total[0];

            //=================== Count loop's error in eFwd and eBwd
            XOR_err_efwd_ebwd = new int[2];
            AND_err_efwd_ebwd = new int[2];
            for (int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].messageNum == 3)
                    XOR_err_efwd_ebwd[0]++;
                if (clsError.Error[i].messageNum == 4)
                    XOR_err_efwd_ebwd[1]++;
                if (clsError.Error[i].messageNum == 5)
                    AND_err_efwd_ebwd[0]++;
                if (clsError.Error[i].messageNum == 6)
                    AND_err_efwd_ebwd[1]++;
            }
        }

        public void counting_Error_type_NOT_SOS_SESE(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsSESE clsSESE, int workSESE, ref GraphVariables.clsLoop clsLoop, int workLoop, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsError clsError)
        {
            OR_join_total = new int[5]; // Bond/rigid/other // [3] OR_BOND_EOJ [4] OR_RIGID_EOJ
            XOR_join_total = new int[3]; //count total XOR-join of exit of BOND/ RIGID
            XOR_join_err = new int[3]; //[0] Bond; [1] Rigid; [2] Other
            AND_join_total = new int[3]; //count total AND-join of exit of BOND/ RIGID
            AND_join_err = new int[3]; //[0] Bond; [1] Rigid; [2] Other

            XOR_entry = new int[4]; //[0] Bond_Entry [1] Bond_Entry_Corrected [2] Rigid_entry [3] Rigid_entry_corrected;
            AND_entry = new int[4];
            OR_entry = new int[4];

            En_Ex_Type = new int[13]; //count cross-table table for entry-exit type

            for (int i = 0; i < clsError.nError; i++)// only for SoS SESE
            {
                if (clsError.Error[i].SESE == "") continue;
                int cur_SESE = Convert.ToInt32(clsError.Error[i].SESE);
                int cur_Node = Convert.ToInt32(clsError.Error[i].Node);

                if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[cur_SESE].Entry].Name == "SS") continue; //only get SS_SESE Rigids only errors

                if (clsError.Error[i].messageNum == 27)
                {
                    //check exit of bond/rigid with LoS error
                    if (clsSESE.SESE[workSESE].SESE[cur_SESE].Exit == cur_Node)
                    {
                        if (clsSESE.SESE[workSESE].SESE[cur_SESE].type == "B")
                            XOR_join_err[0]++;
                        else
                            XOR_join_err[1]++;
                    }
                    else
                        XOR_join_err[2]++;
                }
                if (clsError.Error[i].messageNum == 28)
                {
                    //check exit of bond/rigid with DL error
                    if (clsSESE.SESE[workSESE].SESE[cur_SESE].Exit == cur_Node)
                    {
                        if (clsSESE.SESE[workSESE].SESE[cur_SESE].type == "B")
                            AND_join_err[0]++;
                        else
                            AND_join_err[1]++;
                    }
                    else
                        AND_join_err[2]++;
                }
            }

            //Count total case;
            for (int i = 0; i < clsSESE.SESE[workSESE].nSESE; i++) //Might need to upgrade to check non-join SESE exit (e.g., loop exit as Split)
            {
                if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Name == "SS") continue; //only get SS_SESE errors

                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR" )
                    XOR_join_total[0]++;                
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR")
                    XOR_join_total[1]++;                
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                    AND_join_total[0]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                    AND_join_total[1]++;
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR")
                {
                    OR_join_total[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                        OR_join_total[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR")
                {
                    OR_join_total[1]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                        OR_join_total[4]++;
                }
                //==== New for Entry of SESE (and corrected)
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                {
                    XOR_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) XOR_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                {
                    XOR_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) XOR_entry[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                {
                    AND_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) AND_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                {
                    AND_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) AND_entry[3]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "B" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                {
                    OR_entry[0]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) OR_entry[1]++;
                }
                if (clsSESE.SESE[workSESE].SESE[i].type == "R" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                {
                    OR_entry[2]++;
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected == true) OR_entry[3]++;
                }

                //count Bond cross-table table for entry exit type: XOR-XOR; XOR-AND; XOR-OR; XOR-SOS_OR
                if (clsSESE.SESE[workSESE].SESE[i].type == "B")
                {
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "XOR")
                    {
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR") En_Ex_Type[1]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND") En_Ex_Type[2]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR") En_Ex_Type[3]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                            En_Ex_Type[4]++;
                    }
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "AND")
                    {
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR") En_Ex_Type[5]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND") En_Ex_Type[6]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR") En_Ex_Type[7]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                            En_Ex_Type[8]++;
                    }
                    if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == "OR")
                    {
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR") En_Ex_Type[9]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND") En_Ex_Type[10]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR") En_Ex_Type[11]++;
                        if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR" && graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                            En_Ex_Type[12]++;
                    }                   
                }
            }

            //recode total XOR/ AND-join of [Other] - minus XOR_join_total[0] and XOR_join_total[1]; as well as AND_join_total;
            for (int k = 0; k < clsSESE.SESE[workSESE].nSESE; k++)
            {
                if (graph.Network[finalNet].Node[clsSESE.SESE[workSESE].SESE[k].Entry].Name == "SS") continue; //only get SS_SESE errors

                for (int i = 0; i < graph.Network[finalNet].nNode; i++)
                {
                    if (!Ultilities.checkGraph.Node_In_SESE(clsSESE, workSESE, i, k)) continue; //if this node not in current SS_rigid => ignore
                    if (i == clsSESE.SESE[workSESE].SESE[k].Entry || i == clsSESE.SESE[workSESE].SESE[k].Exit) continue; //not counting any entry/exit of SESE
                    if (graph.Network[finalNet].Node[i].Kind == "XOR" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        XOR_join_total[2]++;
                    if (graph.Network[finalNet].Node[i].Kind == "AND" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        AND_join_total[2]++;
                    if (graph.Network[finalNet].Node[i].Kind == "OR" && graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        OR_join_total[2]++;

                    //just for validation
                    if (graph.Network[finalNet].Node[i].nPost == 1 && graph.Network[finalNet].Node[i].nPre > 1)
                        informList[25]++;
                }
            }
            //final calibration:
            //XOR_join_total[2] = XOR_join_total[2] - XOR_join_total[1];// -XOR_join_total[0]; not count bond => not minus
            //AND_join_total[2] = AND_join_total[2] - AND_join_total[1];// -AND_join_total[0];
            //OR_join_total[2] = OR_join_total[2] - OR_join_total[1];// -OR_join_total[0];

            //=================== Count loop's error in eFwd and eBwd
            XOR_err_efwd_ebwd = new int[2];
            AND_err_efwd_ebwd = new int[2];
            for (int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].messageNum == 3)
                    XOR_err_efwd_ebwd[0]++;
                if (clsError.Error[i].messageNum == 4)
                    XOR_err_efwd_ebwd[1]++;
                if (clsError.Error[i].messageNum == 5)
                    AND_err_efwd_ebwd[0]++;
                if (clsError.Error[i].messageNum == 6)
                    AND_err_efwd_ebwd[1]++;
            }
        }

        public void counting_Error_type_Real_Potential_Dominated(ref GraphVariables.clsGraph graph, int finalNet, ref GraphVariables.clsError clsError)
        {
            Real_err = new int[4]; //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; =====not now==== [4] fwd; [5] bwd
            Potential_err = new int[4]; //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; =====not now==== [4] fwd; [5] bwd
            Dominated_err = new int[4]; //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; =====not now==== [4] fwd; [5] bwd
            Dominated_err_Potential = new int[4]; //[0] LoS; [1] DL; [2] LoS_PdFlow; [3] DL_PdFlow; =====not now==== [4] fwd; [5] bwd

            //XOR_err_efwd_ebwd = new int[2]; //[0]efwd [1]ebwd
            //AND_err_efwd_ebwd = new int[2];

            for (int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].messageNum == 27) //LOS
                {
                    if (clsError.Error[i].TypeOfError == "R") 
                    {
                        Real_err[0]++; //LoS
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Real_err[2]++;
                    }
                    if (clsError.Error[i].TypeOfError == "P")
                    {
                        Potential_err[0]++;
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Potential_err[2]++;
                    }
                    if (clsError.Error[i].TypeOfError == "D")
                    {
                        Dominated_err[0]++;
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Dominated_err[2]++;
                    }
                    if (clsError.Error[i].TypeOfError == "DP")
                    {
                        Dominated_err_Potential[0]++;
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Dominated_err_Potential[2]++;
                    }                    
                }
                if (clsError.Error[i].messageNum == 28) //DL
                {
                    if (clsError.Error[i].TypeOfError == "R")
                    {
                        Real_err[1]++; //dl
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Real_err[3]++;
                    }
                    if (clsError.Error[i].TypeOfError == "P")
                    {
                        Potential_err[1]++;
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Potential_err[3]++;
                    }
                    if (clsError.Error[i].TypeOfError == "D")
                    {
                        Dominated_err[1]++;
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Dominated_err[3]++;
                    }
                    if (clsError.Error[i].TypeOfError == "DP")
                    {
                        Dominated_err_Potential[1]++;
                        if (Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, Convert.ToInt32(clsError.Error[i].Node)) == true)
                            Dominated_err_Potential[3]++;
                    }
                }                
            }
        }


        public void check_SyntaxError( GraphVariables.clsGraph graph, int orgNet) //Check ACTIVITIES/EVENTS as GATEWAYS - More critical
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

        public void check_SyntaxError_2(GraphVariables.clsGraph graph, int orgNet) //Check GATEWAYS as ACTIVITIES
        {
            SyntaxError_EV = false;
            for (int i = 0; i < graph.Network[orgNet].nNode; i++)
            {
                if (graph.Network[orgNet].Node[i].nPre == 1 && graph.Network[orgNet].Node[i].nPost == 1)
                {
                    if (graph.Network[orgNet].Node[i].Kind == "EVENT" || graph.Network[orgNet].Node[i].Kind == "TASK") continue;

                    SyntaxError_EV = true;
                    break;
                }
            }
        }

        public void informRecording(GraphVariables.clsGraph graph, int currentN)
        {
            //informList = new double[20]; //plus 5 to avoid error introducing by model less than 5 nodes
            //Record Type 1 split (number of split node, join node, split node)
            int countOrgI = 0;
            int countJoinI = 0;
            int countSplitI = 0;
            int[] type_I_bool = new int[graph.Network[currentN].nNode];
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Type_I == "" || graph.Network[currentN].Node[i].Type_I == null) continue;
                {
                    if (i == graph.Network[currentN].Node[i].orgNum) countOrgI++;
                    if (graph.Network[currentN].Node[i].Type_I == "_j") countJoinI++;
                    if (graph.Network[currentN].Node[i].Type_I == "_s") countSplitI++;
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
            int[] type_II_bool = new int[graph.Network[currentN].nNode];
            string[] type_III_en = new string[graph.Network[currentN].nNode];
            //int type_III_en_Index = 0;
            string[] type_III_ex = new string[graph.Network[currentN].nNode];
            //int type_III_ex_Index = 0;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Type_II == "" || graph.Network[currentN].Node[i].Type_II == null) continue;
                {
                    //type_II_bool[Network[currentN].Node[i].orgNum] = 1;

                    //Type-III
                    string type = graph.Network[currentN].Node[i].Type_II;

                    if (type.Length > 2)
                    {
                        type = type.Substring(type.Length - 3, 3);
                        if (type == "_se" || type == "_sx")
                        {
                            //check_count(type_III_en, ref countEnIII)
                            //countOrgIII++; //just sum
                            string name = graph.Network[currentN].Node[i].Name + type;
                            if (type == "_se") countEnIII++;//check_count_TypeIII(ref type_III_en, name, ref countEnIII); //count all split node
                            else countExIII++;//check_count_TypeIII(ref type_III_ex, name , ref countExIII); //count all split node
                        }
                        else type = graph.Network[currentN].Node[i].Type_II;
                    }
                    if (i != graph.Network[currentN].Node[i].orgNum)
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

        public void multiStart_CIPD_Count(GraphVariables.clsGraph graph, int currentN, int finalNetwork, int currentSESE) //get % of CIPd; # of entries; # of exits; (Original model only??)
        {
            //Preprocessing OrgNet (e.g., Dom Pdom)
            gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, currentN);
            gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, currentN);
            gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, currentN, -2);
            gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, currentN);
                
            int[] entries = new int[graph.Network[currentN].nNode];
            int nEntries = 0;
            //find node START 
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].nPre == 0)
                {
                    entries[nEntries] = i;
                    nEntries++;
                }
            }//=> node SS = Start.Pre[0];
            //entries[0] ~ START node of model
            if (graph.Network[finalNetwork].Node[graph.Network[currentN].Node[entries[0]].Post[0]].Name == "SS") //Model have multiple start event
            {
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {
                    if (graph.Network[currentN].Link[i].fromNode == graph.Network[currentN].Node[entries[0]].Post[0])
                    {
                        entries[nEntries] = graph.Network[currentN].Link[i].toNode;
                        nEntries++;
                    }
                }
            }
            else //Model have not multiple start event
            {
                int tempNodeEn = graph.Network[currentN].Node[entries[0]].Post[0];
                if (graph.Network[currentN].Node[tempNodeEn].Kind == "EVENT")
                {
                    //entries[nEntries] = graph.Network[currentN].Node[entries[0]].Post[0];
                    //nEntries++;
                }
            }
            if (nEntries > 1) //Prevent the case model have not Start with EVENT (Gateway or Function Instead)
            {
                //#region find CIPD
                //find 
                bool checkIn = false;
                int CIPd = 0;
                int minNode = graph.Network[finalNetwork].nNode;
                for (int i = 0; i < graph.Network[finalNetwork].nNode; i++)
                {
                    for (int j = 1; j < nEntries; j++)
                    {
                        checkIn = false;
                        for (int k = 0; k < graph.Network[finalNetwork].Node[i].nDomRevEI; k++)
                        {
                            if (entries[j] == graph.Network[finalNetwork].Node[i].DomRevEI[k])
                            {
                                checkIn = true;
                                break;
                            }
                        }
                        if (checkIn == false) break;
                        else if (j == nEntries - 1)
                        {
                            if (minNode > graph.Network[finalNetwork].Node[i].nDomRevEI - 1)
                            {
                                minNode = graph.Network[finalNetwork].Node[i].nDomRevEI - 1;
                                CIPd = i;
                            }
                        }
                    }
                }
                informList[10] = Convert.ToDouble(graph.Network[finalNetwork].Node[CIPd].nDomRevEI - 1);
                informList[11] = nEntries - 1;

                //Get list of node in PdFlow(SS)
                int[] list_node = new int[graph.Network[finalNetwork].Node[CIPd].nDomRevEI];
                int nList_node = 0;
                for (int i = 0; i < graph.Network[finalNetwork].Node[CIPd].nDomRevEI; i++)
                {
                    list_node[nList_node] = graph.Network[finalNetwork].Node[CIPd].DomRevEI[i];
                    nList_node++;
                }
                informList[23] = nList_node - 1; //number of node in PdFlow(SOS) - START node
                informList[24] = graph.Network[finalNetwork].nNode;
                //CIPd_SS_node = list_node;
                //List of node in PdFlow(SS) => List of edges in PdFlow(SS); //Re-purpose index informList[10]; informList[22]
                informList[10] = 0; //number of edeges in PdFlow
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {
                    int fromNode = graph.Network[currentN].Link[i].fromNode;
                    int toNode = graph.Network[currentN].Link[i].toNode;
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(list_node, nList_node, fromNode) && gProAnalyzer.Ultilities.checkGraph.Node_In_Set(list_node, nList_node, toNode))
                    {
                        informList[10]++;
                    }
                }
                informList[10] = informList[10] - 1; //Count edges in PdFlow(SS) except START edge
                informList[22] = graph.Network[currentN].nLink - informList[10] - 2; //the edges outside PdFlow()

                #region
                /*
                //find PdFlow(SOS, SOS_se)
                int[] SOS = new int[graph.Network[finalNetwork].nNode]; // SOS ~ Starting OR Splits
                int nSOS = 0;
                //int CIPd_restSOS = -1;
                for (int i = 0; i < graph.Network[finalNetwork].nNode; i++) //finalNetwork ~ local variable // Find the set of SOS and SOS_se
                {
                    if (graph.Network[finalNetwork].Node[i].Name == "SS" & graph.Network[finalNetwork].Node[i].Kind == "OR" && graph.Network[finalNetwork].Node[i].SOS_Corrected == false)
                    {
                        SOS[nSOS] = i;
                        nSOS++;
                    }
                if (graph.Network[finalNetwork].Node[entries[1]].Name != "SS") continue;
                SOS[nSOS] = entries[1];
                nSOS++;
                if (nSOS > 0)
                {
                    int Min_SOS = graph.Network[finalNetwork].nNode;

                    int[] calDomRev = null;
                    int[] calDomRevI = null;
                    for (int k = 0; k < nSOS; k++)
                    {
                        calDomRev = Ultilities.findIntersection.find_Intersection(graph.Network[finalNetwork].nNode, calDomRev, graph.Network[finalNetwork].Node[SOS[k]].DomRev);
                    }
                    if (nSOS == 1) //just a trick for the single SS_OR split, because calDomRev[0] include itself inside, we need to move to another index
                    {
                        //Functionalities.DominanceIdentification.find_DomEI(ref graph, finalNetwork, calDomRev[1], ref calDomRevI, ref Min_SOS);
                    }
                    else
                    {
                        //Functionalities.DominanceIdentification.find_DomEI(ref graph, finalNetwork, calDomRev[0], ref calDomRevI, ref Min_SOS);
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
                }*/
                #endregion
            }
            else
            {
                informList[23] = 0; //no node in PdFlow(SS)
                informList[24] = graph.Network[finalNetwork].nNode;
            }
            

            //find node exits;
            int[] exits = new int[graph.Network[currentN].nNode];
            int nExits = 0;
            //find virtual node exit 
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].nPost == 0)
                {
                    exits[nExits] = i;
                    nExits++;
                }
            }//=> node EE = Start.Pre[0];
            if (graph.Network[finalNetwork].Node[graph.Network[currentN].Node[exits[0]].Pre[0]].Name == "EE")
            {
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {
                    if (graph.Network[currentN].Link[i].toNode == graph.Network[currentN].Node[exits[0]].Pre[0])
                    {
                        exits[nExits] = graph.Network[currentN].Link[i].fromNode;
                        nExits++;
                    }
                }
            }
            else
            {
                int tempNodeExit = graph.Network[currentN].Node[exits[0]].Pre[0];
                if (graph.Network[currentN].Node[tempNodeExit].Kind == "EVENT")
                {
                    entries[nExits] = graph.Network[currentN].Node[exits[0]].Pre[0];
                    nExits++;
                }
            }
            if (nExits > 1) //In case there are no Events as exits (Gateways or Function Instead)
            {
                //informList[12] = nExits - 1;
            }
        }

        public void multiStart_CIPD_Count_FinalNet(GraphVariables.clsGraph graph, int currentN, int finalNetwork, GraphVariables.clsSESE clsSESE, int currentSESE) //get % of CIPd; # of entries; # of exits; (Original model only??)
        {
            //Preprocessing OrgNet (e.g., Dom Pdom)
            gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, currentN);
            gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, currentN);
            gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, currentN, -2);
            gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, currentN);

            int[] entries = new int[graph.Network[currentN].nNode];
            int nEntries = 0;
            //find node START 
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].nPre == 0)
                {
                    entries[nEntries] = i;
                    nEntries++;
                }
            }//=> node SS = Start.Pre[0];
            if (graph.Network[finalNetwork].Node[graph.Network[currentN].Node[entries[0]].Post[0]].Name == "SS") //Model have multiple start event
            {
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {
                    if (graph.Network[currentN].Link[i].fromNode == graph.Network[currentN].Node[entries[0]].Post[0])
                    {
                        entries[nEntries] = graph.Network[currentN].Link[i].toNode;
                        nEntries++;
                    }
                }
            }
            else //Model have not multiple start event
            {
                int tempNodeEn = graph.Network[currentN].Node[entries[0]].Post[0];
                if (graph.Network[currentN].Node[tempNodeEn].Kind == "EVENT")
                {
                    entries[nEntries] = graph.Network[currentN].Node[entries[0]].Post[0];
                    nEntries++;
                }
                else
                    if (graph.Network[currentN].Node[tempNodeEn].Kind != "EVENT")
                    {
                        entries[nEntries] = graph.Network[currentN].Node[entries[0]].Post[0];
                        nEntries++;
                    }
            }
            if (nEntries > 1) //Prevent the case model have not Start with EVENT (Gateway or Function Instead)
            {
                //#region find CIPD
                //find 
                bool checkIn = false;
                int CIPd = 0;
                int minNode = graph.Network[finalNetwork].nNode;
                for (int i = 0; i < graph.Network[finalNetwork].nNode; i++)
                {
                    for (int j = 1; j < nEntries; j++)
                    {
                        checkIn = false;
                        for (int k = 0; k < graph.Network[finalNetwork].Node[i].nDomRevEI; k++)
                        {
                            if (entries[j] == graph.Network[finalNetwork].Node[i].DomRevEI[k])
                            {
                                checkIn = true;
                                break;
                            }
                        }
                        if (checkIn == false) break;
                        else if (j == nEntries - 1)
                        {
                            if (minNode > graph.Network[finalNetwork].Node[i].nDomRevEI - 1)
                            {
                                minNode = graph.Network[finalNetwork].Node[i].nDomRevEI - 1;
                                CIPd = i;
                            }
                        }
                    }
                }
                //informList[10] = Convert.ToDouble(graph.Network[finalNetwork].Node[CIPd].nDomRevEI - 1);
                //informList[11] = nEntries - 1;

                //Get list of node in PdFlow(SS)
                int[] list_node = new int[graph.Network[finalNetwork].Node[CIPd].nDomRevEI];
                int nList_node = 0;
                for (int i = 0; i < graph.Network[finalNetwork].Node[CIPd].nDomRevEI; i++)
                {
                    list_node[nList_node] = graph.Network[finalNetwork].Node[CIPd].DomRevEI[i];
                    nList_node++;
                }
                CIPd_SS_node = list_node;
                //List of node in PdFlow(SS) => List of edges in PdFlow(SS); //Re-purpose index informList[10]; informList[22]
                //informList[10] = 0; //number of edeges in PdFlow
                for (int i = 0; i < graph.Network[currentN].nLink; i++)
                {
                    int fromNode = graph.Network[currentN].Link[i].fromNode;
                    int toNode = graph.Network[currentN].Link[i].toNode;
                    if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(list_node, nList_node, fromNode) && gProAnalyzer.Ultilities.checkGraph.Node_In_Set(list_node, nList_node, toNode))
                    {
                        //informList[10]++;
                    }
                }
                //informList[10] = informList[10] - 1; //Count edges in PdFlow(SS) except START edge
                //informList[22] = graph.Network[currentN].nLink - informList[10] - 2; //the edges outside PdFlow()
            }

            /*
            bond_rigid_PdF = new int[2];
            //count #SESE in PdFlow (exit in PdFlow(SoS) only?)
            for (int i = 0; i < clsSESE.SESE[currentSESE].nSESE; i++)
            {
                if (clsSESE.SESE[currentSESE].SESE[i].type == "B" && Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, clsSESE.SESE[currentSESE].SESE[i].Exit) == true)
                {
                    bond_rigid_PdF[0]++;
                }
                if (clsSESE.SESE[currentSESE].SESE[i].type == "R" && Ultilities.checkGraph.Node_In_Set(CIPd_SS_node, CIPd_SS_node.Length, clsSESE.SESE[currentSESE].SESE[i].Exit) == true)
                {
                    bond_rigid_PdF[1]++;
                }
            }
            */
        }

        public void more_NL_Info(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int currentLoop, GraphVariables.clsSESE clsSESE, GraphVariables.clsError clsError)
        {
            //ext_informlist[0] = #exits
            //ext_informlist[1] = #bs
            //ext_informlist[2] = #bs & exit
            //ext_informlist[3] = #dummy nodes (for CIPd(exits))
            //ext_informlist[4] = #edges in Fwd //=> Take out the loop => fwd = DFS(header to exits) => get edges
            //ext_informlist[5] = #edges in Bwd //=> Take out the loop => bwd = {Loop \ G} => get edges            
            ext_informlist = new double[10];
            XOR_Loop = new int[8];
            AND_Loop = new int[8];
            OR_Loop = new int[8];

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                //Special; //special Node??   E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
                if (graph.Network[currentN].Node[i].Special == "X")
                    ext_informlist[0] += 1;
                if (graph.Network[currentN].Node[i].Special == "B")
                    ext_informlist[1] += 1;
                if (graph.Network[currentN].Node[i].Special == "T")
                    ext_informlist[2] += 1;
                if (graph.Network[currentN].Node[i].Name == "Dummy")
                    ext_informlist[3] += 1;
            }

            int curDepth = clsLoop.Loop[currentLoop].maxDepth;
            string strLoop = "";
            do
            {
                for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++) //count edges in Fwd, Bwd
                {
                    if (clsLoop.Loop[currentLoop].Loop[i].depth != curDepth) continue;
                    strLoop = i.ToString();

                    int header = clsLoop.Loop[currentLoop].Loop[i].header;
                    //int[] exits = new int[clsLoop.Loop[currentLoop].Loop[i].nExit];
                    //exits = clsLoop.Loop[currentLoop].Loop[i].Exit;
                    //int nBS = clsLoop.Loop[currentLoop].Loop[i].nBackSplit;
                    //nBS = nBS - exits.Length;

                    int[] node_inLoop = new int[clsLoop.Loop[currentLoop].Loop[i].nNode + 1];
                    node_inLoop[0] = header;
                    for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nNode; j++) node_inLoop[j + 1] = clsLoop.Loop[currentLoop].Loop[i].Node[j];
                    int edges_NL = Ultilities.checkGraph.edges_in_Node_Set(graph, currentN, node_inLoop, node_inLoop.Length);

                    //DFS_Fwd_Bwd(header, exits)
                    int[][] adjList = null;
                    int[] getPath = new int[graph.Network[currentN].nNode]; //node in Fwd
                    int START = graph.Network[currentN].header;
                    int nGetPath = 0;
                    bool[] Mark = new bool[graph.Network[currentN].nNode];
                    gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, currentN, ref adjList);

                    gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.seseNet, ref clsLoop, currentLoop, i, ref clsSESE, "FF", -1);
                    int ex_bs = 0;
                    for (int j = 0; j < graph.Network[graph.seseNet].nNode; j++) if (graph.Network[graph.seseNet].Node[j].Special == "T" || graph.Network[graph.seseNet].Node[j].Special == "X") ex_bs++;                    
                    
                    int[] node_in_eFwd = new int[graph.Network[graph.seseNet].nNode];
                    for (int j = 0; j < graph.Network[graph.seseNet].nNode; j++) node_in_eFwd[j] = j;
                    int edges_Fwd = Ultilities.checkGraph.edges_in_Node_Set(graph, graph.seseNet, node_in_eFwd, node_in_eFwd.Length);
                    if (ex_bs > 1) edges_Fwd = edges_Fwd - ex_bs; //minus the number of artifical edges from eFwd

                    int edges_Bwd = edges_NL - edges_Fwd;

                    ext_informlist[4] += edges_Fwd;
                    ext_informlist[5] += edges_Bwd;

                    //Check loop type;
                    find_LoopType_NL(graph, currentN, ref clsLoop, currentLoop, i);

                    gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, currentLoop, i, strLoop, true);
                    if (clsLoop.Loop[currentLoop].Loop[i].parentLoop != -1) //find special node in reduced graph
                        Functionalities.LoopIdentification.find_SpecialNode_ReducedLoop(ref graph, currentN, ref clsLoop, currentLoop, clsLoop.Loop[currentLoop].Loop[i].parentLoop);                     
                }
                curDepth--;
            } while (curDepth > 0);

            //count total first Loop related error with table (XOR, AND, OR vs En, Ex, BS) [0] [2] [4] [6]
            /*for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++)
            {
                //XOR
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nEntry; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].Entry[j];
                    if (graph.Network[currentN].Node[node].Special == "E" && graph.Network[currentN].Node[node].Kind == "XOR")
                        XOR_Loop[0]++; //total entry
                }
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nExit; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].Exit[j];
                    if (graph.Network[currentN].Node[node].Special == "X" && graph.Network[currentN].Node[node].Kind == "XOR")
                        XOR_Loop[2]++; //total exit (not bs)
                    if (graph.Network[currentN].Node[node].Special == "T" && graph.Network[currentN].Node[node].Kind == "XOR")
                        XOR_Loop[6]++; //total exit & bs

                }
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nBackSplit; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].BackSplit[j];
                    if (graph.Network[currentN].Node[node].Special == "B" && graph.Network[currentN].Node[node].Kind == "XOR")
                        XOR_Loop[4]++; //total bs
                }
                //AND
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nEntry; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].Entry[j];
                    if (graph.Network[currentN].Node[node].Special == "E" && graph.Network[currentN].Node[node].Kind == "AND")
                        AND_Loop[0]++; //total entry
                }
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nExit; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].Exit[j];
                    if (graph.Network[currentN].Node[node].Special == "X" && graph.Network[currentN].Node[node].Kind == "AND")
                        AND_Loop[2]++; //total exit (not bs)
                    if (graph.Network[currentN].Node[node].Special == "T" && graph.Network[currentN].Node[node].Kind == "AND")
                        AND_Loop[6]++; //total exit & bs

                }
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nBackSplit; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].BackSplit[j];
                    if (graph.Network[currentN].Node[node].Special == "B" && graph.Network[currentN].Node[node].Kind == "AND")
                        AND_Loop[4]++; //total bs
                }
                //OR
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nEntry; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].Entry[j];
                    if (graph.Network[currentN].Node[node].Special == "E" && graph.Network[currentN].Node[node].Kind == "OR")
                        OR_Loop[0]++; //total entry
                }
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nExit; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].Exit[j];
                    if (graph.Network[currentN].Node[node].Special == "X" && graph.Network[currentN].Node[node].Kind == "OR")
                        OR_Loop[2]++; //total exit (not bs)
                    if (graph.Network[currentN].Node[node].Special == "T" && graph.Network[currentN].Node[node].Kind == "OR")
                        OR_Loop[6]++; //total exit & bs

                }
                for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nBackSplit; j++)
                {
                    int node = clsLoop.Loop[currentLoop].Loop[i].BackSplit[j];
                    if (graph.Network[currentN].Node[node].Special == "B" && graph.Network[currentN].Node[node].Kind == "OR")
                        OR_Loop[4]++; //total bs
                }
            } */

            for (int i = 0; i < graph.Network[graph.finalNet].nNode; i++)
            {
                if (graph.Network[graph.finalNet].Node[i].Kind == "XOR" && graph.Network[graph.finalNet].Node[i].Special == "E")
                    XOR_Loop[1]++;
                if (graph.Network[graph.finalNet].Node[i].Kind == "XOR" && graph.Network[graph.finalNet].Node[i].Special == "X")
                    XOR_Loop[3]++;
                if (graph.Network[graph.finalNet].Node[i].Kind == "XOR" && graph.Network[graph.finalNet].Node[i].Special == "B")
                    XOR_Loop[5]++;
                if (graph.Network[graph.finalNet].Node[i].Kind == "XOR" && graph.Network[graph.finalNet].Node[i].Special == "T")
                    XOR_Loop[7]++;
            } 
            //count error of Loop_gateways by rule-1 // [1] [3] [5] [7]
            for (int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].Loop == "") continue;
                int cur_Loop = Convert.ToInt32(clsError.Error[i].Loop);
                int cur_Node = Convert.ToInt32(clsError.Error[i].Node);
                if (clsError.Error[i].messageNum <= 2)
                {
                    //AND =>> Must use finalNet
                    //if (Ultilities.checkGraph.Node_In_Set(clsLoop.Loop[currentLoop].Loop[cur_Loop].Entry, clsLoop.Loop[currentLoop].Loop[cur_Loop].nEntry, cur_Node))
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "AND" && graph.Network[graph.finalNet].Node[cur_Node].Special == "E")
                        AND_Loop[1]++;
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "AND" && graph.Network[graph.finalNet].Node[cur_Node].Special == "X")
                        AND_Loop[3]++;
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "AND" && graph.Network[graph.finalNet].Node[cur_Node].Special == "B")
                        AND_Loop[5]++;
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "AND" && graph.Network[graph.finalNet].Node[cur_Node].Special == "T")
                        AND_Loop[7]++;
                    //OR
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "OR" && graph.Network[graph.finalNet].Node[cur_Node].Special == "E")
                        OR_Loop[1]++;
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "OR" && graph.Network[graph.finalNet].Node[cur_Node].Special == "X")
                        OR_Loop[3]++;
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "OR" && graph.Network[graph.finalNet].Node[cur_Node].Special == "B")
                        OR_Loop[5]++;
                    if (graph.Network[graph.finalNet].Node[cur_Node].Kind == "OR" && graph.Network[graph.finalNet].Node[cur_Node].Special == "T")
                        OR_Loop[7]++;
                }
            }

            //count loop type;
        }

        public void more_NL_Info_2(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int currentLoop, GraphVariables.clsSESE clsSESE, GraphVariables.clsError clsError)
        {        
            ext_informlist_2 = new double[30]; //use index from [1] to [28] // [7 x 4]
            //XOR_Loop = new int[8];
            //AND_Loop = new int[8];
            //OR_Loop = new int[8];
            int basis = 7;
            for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++) //count #NL for each type-1->4
            {
                if (clsLoop.Loop[currentLoop].Loop[i].nEntry > 1) continue; //not count IL
                int multi = clsLoop.Loop[currentLoop].Loop[i].loopType - 1;                
                //if (clsLoop.Loop[currentLoop].Loop[i].loopType == 1)
                ext_informlist_2[1 + multi*basis] += 1; //jump index by 7 (if any)

            }

            /*
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                //Special; //special Node??   E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
                for (int j = 0; j < clsLoop.Loop[currentLoop].nLoop; j++)
                {
                    if (clsLoop.Loop[currentLoop].Loop[j].nEntry > 1) continue; //not count IL
                    if (!Ultilities.checkGraph.Node_In_Loop(clsLoop, currentLoop, i, j)) continue; //only collect the node in current loop.
                    //Record data.
                    int multi = clsLoop.Loop[currentLoop].Loop[j].loopType - 1;                                     

                    if (graph.Network[currentN].Node[i].Special == "X")
                        ext_informlist_2[2 + multi * basis] += 1;
                    if (graph.Network[currentN].Node[i].Special == "B")
                        ext_informlist_2[3 + multi * basis] += 1;
                    if (graph.Network[currentN].Node[i].Special == "T")
                        ext_informlist_2[4 + multi * basis] += 1;
                    if (graph.Network[currentN].Node[i].Name == "Dummy")
                        ext_informlist_2[5 + multi * basis] += 1;
                }                
            }
            */

            int curDepth = clsLoop.Loop[currentLoop].maxDepth;
            string strLoop = "";
            do
            {
                for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++) //count edges in Fwd, Bwd
                {
                    if (clsLoop.Loop[currentLoop].Loop[i].depth != curDepth) continue;
                    strLoop = i.ToString();

                    if (clsLoop.Loop[currentLoop].Loop[i].nEntry > 1) continue; //only get NL ======================

                    int header = clsLoop.Loop[currentLoop].Loop[i].header;
                    //int[] exits = new int[clsLoop.Loop[currentLoop].Loop[i].nExit];
                    //exits = clsLoop.Loop[currentLoop].Loop[i].Exit;
                    //int nBS = clsLoop.Loop[currentLoop].Loop[i].nBackSplit;
                    //nBS = nBS - exits.Length;

                    int[] node_inLoop = new int[clsLoop.Loop[currentLoop].Loop[i].nNode + 1];
                    node_inLoop[0] = header;
                    for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nNode; j++) node_inLoop[j + 1] = clsLoop.Loop[currentLoop].Loop[i].Node[j];
                    int edges_NL = Ultilities.checkGraph.edges_in_Node_Set(graph, currentN, node_inLoop, node_inLoop.Length);

                    //DFS_Fwd_Bwd(header, exits)
                    int[][] adjList = null;
                    int[] getPath = new int[graph.Network[currentN].nNode]; //node in Fwd
                    int START = graph.Network[currentN].header;
                    int nGetPath = 0;
                    bool[] Mark = new bool[graph.Network[currentN].nNode];
                    gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, currentN, ref adjList);

                    gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.seseNet, ref clsLoop, currentLoop, i, ref clsSESE, "FF", -1);
                    int ex_bs = 0;
                    for (int j = 0; j < graph.Network[graph.seseNet].nNode; j++) if (graph.Network[graph.seseNet].Node[j].Special == "T" || graph.Network[graph.seseNet].Node[j].Special == "X") ex_bs++;

                    int[] node_in_eFwd = new int[graph.Network[graph.seseNet].nNode];
                    for (int j = 0; j < graph.Network[graph.seseNet].nNode; j++) node_in_eFwd[j] = j;
                    int edges_Fwd = Ultilities.checkGraph.edges_in_Node_Set(graph, graph.seseNet, node_in_eFwd, node_in_eFwd.Length);
                    if (ex_bs > 1) edges_Fwd = edges_Fwd - ex_bs; //minus the number of artifical edges from eFwd

                    int edges_Bwd = edges_NL - edges_Fwd;

                    int multi = clsLoop.Loop[currentLoop].Loop[i].loopType - 1; 
                    ext_informlist_2[6 + multi * basis] += edges_Fwd;
                    ext_informlist_2[7 + multi * basis] += edges_Bwd;

                    //Count others
                    for (int k = 0; k < graph.Network[currentN].nNode; k++)
                    {
                        if (graph.Network[currentN].Node[k].Name == "Dummy") //dummy outside the loop
                            ext_informlist_2[5 + multi * basis] += 1;

                        //Special; //special Node??   E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
                        if (!Ultilities.checkGraph.Node_In_Loop(clsLoop, currentLoop, k, i)) continue; //only collect the node in current loop.
                        //Record data.
                        if (graph.Network[currentN].Node[k].Special == "X")
                            ext_informlist_2[2 + multi * basis] += 1;
                        if (graph.Network[currentN].Node[k].Special == "B")
                            ext_informlist_2[3 + multi * basis] += 1;
                        if (graph.Network[currentN].Node[k].Special == "T")
                            ext_informlist_2[4 + multi * basis] += 1;                        
                    }

                    //Check loop type;
                    //find_LoopType(graph, currentN, ref clsLoop, currentLoop, i);

                    gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, currentLoop, i, strLoop, true);
                    if (clsLoop.Loop[currentLoop].Loop[i].parentLoop != -1) //find special node in reduced graph
                        Functionalities.LoopIdentification.find_SpecialNode_ReducedLoop(ref graph, currentN, ref clsLoop, currentLoop, clsLoop.Loop[currentLoop].Loop[i].parentLoop);
                }
                curDepth--;
            } while (curDepth > 0);              

            //count loop type;
        }

        public void more_IL_Info(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int currentLoop, GraphVariables.clsSESE clsSESE, GraphVariables.clsError clsError)
        {
            //Count #entries
            //Count #exits
            //Count #bs
            //Count #ex & BS
            //Count dummy nodes ??
            //Count edges in Fwd
            //Count edges in Bwd + PdFlow

            //Count errors in DFlow_PdFlow
            //Count errors in eFwd
            //Count errors in eBwd

            int curDepth = clsLoop.Loop[currentLoop].maxDepth;
            string strLoop = "";
            ext_informlist_3_IL = new double[30]; //use index from [1] to [28] // [7 x 4]
            //XOR_Loop = new int[8];
            //AND_Loop = new int[8];
            //OR_Loop = new int[8];
            int basis = 7;
            for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++) //count #NL for each type-1->4
            {
                if (clsLoop.Loop[currentLoop].Loop[i].nEntry > 1) continue; //not count IL
                int multi = clsLoop.Loop[currentLoop].Loop[i].loopType - 1;
                ext_informlist_2[1 + multi * basis] += 1; //jump index by 7 (if any)

            }

            do
            {
                for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++) //count edges in Fwd, Bwd
                {
                    if (clsLoop.Loop[currentLoop].Loop[i].depth != curDepth) continue;
                    strLoop = i.ToString();
                    if (clsLoop.Loop[currentLoop].Loop[i].nEntry > 1) continue; //only get NL ======================
                    int header = clsLoop.Loop[currentLoop].Loop[i].header;

                    //find_LoopType(graph, currentN, ref clsLoop, currentLoop, i);
                }
                curDepth--;
            } while (curDepth > 0); 
            

            
            do
            {
                for (int i = 0; i < clsLoop.Loop[currentLoop].nLoop; i++) //count edges in Fwd, Bwd
                {
                    if (clsLoop.Loop[currentLoop].Loop[i].depth != curDepth) continue;
                    strLoop = i.ToString();

                    if (clsLoop.Loop[currentLoop].Loop[i].nEntry > 1) continue; //only get NL ======================

                    int header = clsLoop.Loop[currentLoop].Loop[i].header;

                    int[] node_inLoop = new int[clsLoop.Loop[currentLoop].Loop[i].nNode + 1];
                    node_inLoop[0] = header;
                    for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nNode; j++) node_inLoop[j + 1] = clsLoop.Loop[currentLoop].Loop[i].Node[j];
                    int edges_NL = Ultilities.checkGraph.edges_in_Node_Set(graph, currentN, node_inLoop, node_inLoop.Length);

                    //DFS_Fwd_Bwd(header, exits)
                    int[][] adjList = null;
                    int[] getPath = new int[graph.Network[currentN].nNode]; //node in Fwd
                    int START = graph.Network[currentN].header;
                    int nGetPath = 0;
                    bool[] Mark = new bool[graph.Network[currentN].nNode];
                    gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, currentN, ref adjList);

                    gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.seseNet, ref clsLoop, currentLoop, i, ref clsSESE, "FF", -1);
                    int ex_bs = 0;
                    for (int j = 0; j < graph.Network[graph.seseNet].nNode; j++) 
                        if (graph.Network[graph.seseNet].Node[j].Special == "T" || graph.Network[graph.seseNet].Node[j].Special == "X") ex_bs++;

                    int[] node_in_eFwd = new int[graph.Network[graph.seseNet].nNode];
                    for (int j = 0; j < graph.Network[graph.seseNet].nNode; j++) node_in_eFwd[j] = j;
                    int edges_Fwd = Ultilities.checkGraph.edges_in_Node_Set(graph, graph.seseNet, node_in_eFwd, node_in_eFwd.Length);
                    if (ex_bs > 1) edges_Fwd = edges_Fwd - ex_bs; //minus the number of artifical edges from eFwd

                    int edges_Bwd = edges_NL - edges_Fwd;

                    int multi = clsLoop.Loop[currentLoop].Loop[i].loopType - 1;
                    ext_informlist_2[6 + multi * basis] += edges_Fwd;
                    ext_informlist_2[7 + multi * basis] += edges_Bwd;

                    //Count others
                    for (int k = 0; k < graph.Network[currentN].nNode; k++)
                    {
                        if (graph.Network[currentN].Node[k].Name == "Dummy") //dummy outside the loop
                            ext_informlist_2[5 + multi * basis] += 1;

                        //Special; //special Node??   E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS
                        if (!Ultilities.checkGraph.Node_In_Loop(clsLoop, currentLoop, k, i)) continue; //only collect the node in current loop.
                        //Record data.
                        if (graph.Network[currentN].Node[k].Special == "X")
                            ext_informlist_2[2 + multi * basis] += 1;
                        if (graph.Network[currentN].Node[k].Special == "B")
                            ext_informlist_2[3 + multi * basis] += 1;
                        if (graph.Network[currentN].Node[k].Special == "T")
                            ext_informlist_2[4 + multi * basis] += 1;
                    }

                    //Check loop type;
                    //find_LoopType(graph, currentN, ref clsLoop, currentLoop, i);

                    gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, currentLoop, i, strLoop, true);
                    if (clsLoop.Loop[currentLoop].Loop[i].parentLoop != -1) //find special node in reduced graph
                        Functionalities.LoopIdentification.find_SpecialNode_ReducedLoop(ref graph, currentN, ref clsLoop, currentLoop, clsLoop.Loop[currentLoop].Loop[i].parentLoop);
                }
                curDepth--;
            } while (curDepth > 0);      
        }

        public void find_LoopType_NL(GraphVariables.clsGraph graph, int reduceNet, ref GraphVariables.clsLoop clsLoop, int currentLoop, int loop)
        {
            if (clsLoop.Loop[currentLoop].Loop[loop].nEntry > 1) return; //only check NL
            int countEx = 0;
            int countExBS = 0;
            int countBS = 0;
            for (int i = 0; i < clsLoop.Loop[currentLoop].Loop[loop].nNode; i++)
            {
                int node = clsLoop.Loop[currentLoop].Loop[loop].Node[i];
                if (graph.Network[reduceNet].Node[node].Special == "X")
                    countEx++;
                if (graph.Network[reduceNet].Node[node].Special == "T")
                    countExBS++;
                if (graph.Network[reduceNet].Node[node].Special == "B")
                    countBS++;
            }
            if (countExBS == 1 && countEx == 0 && countBS == 0) //one exit - (SESE)
                clsLoop.Loop[currentLoop].Loop[loop].loopType = 1;
            else
                if (countExBS == 1 && countEx == 0 && countBS >= 1) //one exit, multiple BS - (SESE)
                    clsLoop.Loop[currentLoop].Loop[loop].loopType = 2;
                else
                    if (countExBS >= 1 && countEx >= 1 && countBS == 0) //no pure-BS (multiple exit)
                        clsLoop.Loop[currentLoop].Loop[loop].loopType = 3;
                    else
                        //if (countExBS >= 1 && countEx >= 1 && countBS >= 1) //multiple exit, multiple bs, multipe ex_bs
                        clsLoop.Loop[currentLoop].Loop[loop].loopType = 4;
        }

        public void find_LoopType_IL(GraphVariables.clsGraph graph, int reduceNet, ref GraphVariables.clsLoop clsLoop, int currentLoop, int loop)
        {
            if (clsLoop.Loop[currentLoop].Loop[loop].nEntry == 1) return; //only check IL
            int countEx = 0;
            int countExBS = 0;
            int countBS = 0;
            int countEn = 0;

            for (int i = 0; i < clsLoop.Loop[currentLoop].Loop[loop].nNode; i++)
            {
                int node = clsLoop.Loop[currentLoop].Loop[loop].Node[i];
                if (graph.Network[reduceNet].Node[node].Special == "X")
                    countEx++;
                if (graph.Network[reduceNet].Node[node].Special == "E")
                    countEn++;
            }

            /*
            if (countExBS == 1 && countEx == 0 && countBS == 0) //one exit - (SESE)
                clsLoop.Loop[currentLoop].Loop[loop].loopType = 1;
            else
                if (countExBS == 1 && countEx == 0 && countBS >= 1) //one exit, multiple BS - (SESE)
                    clsLoop.Loop[currentLoop].Loop[loop].loopType = 2;
                else
                    if (countExBS >= 1 && countEx >= 1 && countBS == 0) //no pure-BS (multiple exit)
                        clsLoop.Loop[currentLoop].Loop[loop].loopType = 3;
                    else
                        //if (countExBS >= 1 && countEx >= 1 && countBS >= 1) //multiple exit, multiple bs, multipe ex_bs
                        clsLoop.Loop[currentLoop].Loop[loop].loopType = 4;
             */ 
        }

        public void changeSS_Type(GraphVariables.clsGraph graph, int currentN, string TYPE) //change type of SS to {XOR, AND, OR} for experiment scenarios
        {
            //TYPE must = {AND, XOR, OR}
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Name == "SS")
                {
                    graph.Network[currentN].Node[i].Kind = TYPE;
                }
            }
        }

        public bool check_labelDuplication(GraphVariables.clsGraph graph, int currentN)
        {
            for (int i = 0; i < graph.Network[currentN].nNode - 1; i++)
            {
                for (int j = i + 1; j < graph.Network[currentN].nNode; j++)
                {
                    if (graph.Network[currentN].Node[i].nodeLabel == null || graph.Network[currentN].Node[i].nodeLabel == null) 
                        continue;
                    if (graph.Network[currentN].Node[i].nodeLabel == graph.Network[currentN].Node[j].nodeLabel) 
                        return true;                                           
                }
            }
            return false;
        }

        public int run_TestG(ref GraphVariables.clsGraph graph, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsLoop clsLoop,
            ref GraphVariables.clsSESE clsSESE, ref GraphVariables.clsError clsError, int numRun)
        {
            //optional-check label duplication
            dup_label = check_labelDuplication(graph, graph.orgNet);
            return 1;
        }

    }
}
