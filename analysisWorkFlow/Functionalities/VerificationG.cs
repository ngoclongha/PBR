using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer.Functionalities
{
    class VerificationG
    {
        public static int total_int = 0;
        public static bool iL_concurrency_Error = false;
        public static System.Diagnostics.Stopwatch watch_Bond = new System.Diagnostics.Stopwatch();
        public static System.Diagnostics.Stopwatch watch_Rigid = new System.Diagnostics.Stopwatch();
        public static System.Diagnostics.Stopwatch watch_IL = new System.Diagnostics.Stopwatch();
        public static frmShowFullModel_Debug frmShow;

        public static void Initialize_Verification(ref gProAnalyzer.GraphVariables.clsGraph graph, ref gProAnalyzer.GraphVariables.clsError clsError, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, ref gProAnalyzer.GraphVariables.clsHWLS clsHWLS)
        {
            //if (IrreducibleError || ConcurrencyError) return; //concurrencyError == true => Return
            //Network(3) 생성후 작업
            iL_concurrency_Error = false;
            graph.Network[graph.reduceNet] = graph.Network[graph.finalNet];
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop, clsLoop.orgLoop, clsLoop.reduceLoop);

            clsError.nError = 0;
            clsError.Error = new gProAnalyzer.GraphVariables.clsError.strError[graph.Network[graph.reduceNet].nNode * 4];
            initiate_Error(ref clsError, graph.Network[graph.reduceNet].nNode * 3);

            check_by_Rule2(ref graph, graph.reduceNet, ref clsLoop, clsLoop.reduceLoop, ref clsSESE, clsSESE.finalSESE, ref clsHWLS, ref clsError, "");
        }

        public static void initiate_Error(ref gProAnalyzer.GraphVariables.clsError clsError, int n)
        {
            for (int i = 0; i < n; i++)
            {
                clsError.Error[i].currentKind = "";
                clsError.Error[i].Loop = "";
                clsError.Error[i].Node = "";
                clsError.Error[i].SESE = "";
            }
        }

        public static void check_by_Rule2(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, 
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, ref gProAnalyzer.GraphVariables.clsHWLS clsHWLS, ref gProAnalyzer.GraphVariables.clsError clsError, string strLoop)
        {
            watch_Bond = new System.Diagnostics.Stopwatch();
            watch_Rigid = new System.Diagnostics.Stopwatch();
            watch_IL = new System.Diagnostics.Stopwatch();
            watch_Bond.Reset();
            watch_Rigid.Reset();
            watch_IL.Reset();

            string strOrgLoop = strLoop;            
            int curDepth = clsHWLS.FBLOCK.maxDepth;
            total_int = 0;
            //informList[13] = 0; //# of bonds
            //informList[14] = 0; //# of rigids
            
            do
            {                
                for (int j = 0; j < clsHWLS.FBLOCK.nFBlock; j++)
                {   
                    if (clsHWLS.FBLOCK.FBlock[j].depth != curDepth) continue;
                    int i = clsHWLS.FBLOCK.FBlock[j].refIndex;
                    if (strOrgLoop == "") strLoop = i.ToString();
                    else strLoop = strOrgLoop + "-" + i.ToString();

                    if (clsHWLS.FBLOCK.FBlock[j].SESE) //If SESE => verify SESE
                    {
                        if (i == 12)
                        {  }
                        if (gProAnalyzer.Ultilities.checkGraph.Bond_Check(ref graph, currentN, ref clsSESE, workSESE, i, ref clsHWLS) == "B") //bond model
                        {
                            watch_Bond.Start();
                            //verify some easy Bond (SS, EE) Example => 1Ex_dwdy.net (just for SESE which have Entry or Exit are SS or EE
                            if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Name == "SS")
                            {
                                if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind != graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind)
                                {
                                    graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind;
                                    graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                    graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind = graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind;
                                    graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                }
                            }
                            //if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE") //not do this
                            //{
                            //graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind;
                            //graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind = graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind;
                            //}
                            //check rule for the rest SESE
                            //informList[13]++;
                            if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind == graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind) { }
                            else if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "OR") { }
                            else if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "XOR")
                            {
                                clsError.Error[clsError.nError].currentKind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind;
                                clsError.Error[clsError.nError].messageNum = 27;
                                clsError.Error[clsError.nError].Node = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].parentNum.ToString();
                                clsError.Error[clsError.nError].SESE = i.ToString();
                                gProAnalyzer.Ultilities.recordData.add_Error(ref clsError);
                            }
                            else if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind == "AND")
                            {
                                clsError.Error[clsError.nError].currentKind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind;
                                clsError.Error[clsError.nError].messageNum = 28;
                                clsError.Error[clsError.nError].Node = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].parentNum.ToString();
                                clsError.Error[clsError.nError].SESE = i.ToString();
                                gProAnalyzer.Ultilities.recordData.add_Error(ref clsError);
                            }
                            watch_Bond.Stop();
                            gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, currentN, clsSESE, workSESE, i);
                        }
                        else //rigid model
                        {
                            if (gProAnalyzer.Ultilities.checkGraph.Bond_Check(ref graph, currentN, ref clsSESE, workSESE, i, ref clsHWLS) == "R")
                            {
                                watch_Rigid.Start();
                                //informList[14]++; //count rigids
                                if (all_same_kind_ExceptHeader(ref graph, currentN, ref clsSESE, workSESE, i) && graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Name == "SS")
                                {
                                    if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind != graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind)
                                    {
                                        graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind;
                                        graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind = graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind;
                                        graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                        graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].SOS_Corrected = true;
                                    }

                                    //if (graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Name == "EE")
                                    //{
                                    //  graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind;
                                    //graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Exit].Kind = graph.Network[graph.finalNet].Node[clsSESE.SESE[workSESE].SESE[i].Entry].Kind;
                                    //}
                                } //no errors for rigid
                                else if (all_OR_join(ref graph, currentN, ref clsSESE, workSESE, i) || all_same_kind(ref graph, currentN, ref clsSESE, workSESE, i))
                                {
                                    //Do somethings?? No-error
                                }
                                else
                                {
                                    //count_OR_Split_Rigids(ref graph, graph.finalNet, ref clsSESE, workSESE, i); //Count OR-split in this Rigid - just simple first
                                    gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.subNet, ref clsLoop, workSESE, i, ref clsSESE, "SESE", -1);
                                    total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.subNet, -2, "SESE", strLoop, ref clsError);

                                    //mark this rigid is processed
                                    clsHWLS.FBLOCK.FBlock[j].isProcessed = true;
                                }
                                watch_Rigid.Stop();
                                gProAnalyzer.Ultilities.reduceGraph.reduce_SESE(ref graph, currentN, clsSESE, workSESE, i);
                            }                           
                        }                        
                    }
                    else //if Loop => verify Loop
                    {
                        if (clsLoop.Loop[workLoop].Loop[i].Irreducible) // Irreducible Loop
                        {
                            watch_IL.Start();
                            //irreducibleLoop_Verify(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE);
                            irreducibleLoop_Verify_Updated(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE);
                            watch_IL.Stop();
                            //if (iL_concurrency_Error == true) return; Keep working to count
                        }
                        else //Natural Loop
                        {
                            naturalLoop_Verify(ref graph, currentN, ref clsLoop, workLoop, i, strLoop,ref clsError, ref clsSESE);
                        }
                    }
                }

                curDepth--;
            } while (curDepth > 0);
        }
        public static void naturalLoop_Verify(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int i, string strLoop, 
            ref gProAnalyzer.GraphVariables.clsError clsError, ref gProAnalyzer.GraphVariables.clsSESE clsSESE)
        {
            check_by_Rule1(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError);

            //Forward Check
            //1) Loop내 FF Network 만들어            
            gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, ref clsSESE, "FF", -1);
            //check_ParallelStructure(seseNet, "efwd", strLoop);
            total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.seseNet, i, "eFwd", strLoop, ref clsError); //New code => must check the vE node

            //Backward Check ====================
            gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, ref clsSESE, "BF", -1);
            //check_ParallelStructure(seseNet, "ebwd", strLoop);
            total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.seseNet, i, "eBwd", strLoop, ref clsError); //new code

            gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, true);
        }

        public static void irreducibleLoop_Verify(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int i, string strLoop,
            ref gProAnalyzer.GraphVariables.clsError clsError, ref gProAnalyzer.GraphVariables.clsSESE clsSESE)
        {            
            //Do somthing
            
            //CHEECK RULE 5
            check_by_Rule5(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError);

            //Find DFlow(entries) =>
            //Figure out each concurrent entry set CEi; (Def 10.3 10.4)
            gProAnalyzer.Ultilities.findConcurrencyEntriesIL.check_Concurrency(ref graph, currentN, graph.conNet, graph.subNet, clsLoop, workLoop, i, ref clsSESE); //=> Get clsLoop.Concurrency
            int CID = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID;
            int[] node_return = null;
            int[] ENTTi = new int[clsLoop.Loop[workLoop].Loop[i].nEntry];
                        
            //Foreach concurrent entry set CEi;
            for (int cc = 1; cc <= clsLoop.Loop[workLoop].Loop[i].nConcurrency; cc++) //for every ENTTi
            {
                //if exclusive => Handle exclusive
                if (cc == 0)
                    for (int exEntry = 0; exEntry < clsLoop.Loop[workLoop].Loop[i].nConcurrency; exEntry++)
                    {
                        //Make a independence NL of IL (Separated loop) <<====== using ENTTi (node name)
                        //Find nested loop => make hierarchy of loops (if any) => Verify nested loop first
                        //STOP => Because it is natural loop => No further structure will be introduced when recycling.
                    }

                //if concurrency => Handle concurrency ENTTi as normal
                //Check the CPId(CEi) => must inside the loop;
                int CIPd = find_CIPd_IL(ref graph, currentN, ref clsLoop, workLoop, i, cc);
                if (gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, CIPd, i) == false)
                {
                    iL_concurrency_Error = true;
                    break;
                }
                             
                //MAKE A SEPARATED GRAPH WITH DFLOW(CEi) + IL =>> for further reduction of IL or SESE ======> replace "currentN" to "new_currentN"
                    //Identify loops inside => verify and reduce it; =>> Store in "new_workLoop"

                //Calculate depth of IL of node (based on CIPd(CEi)) => Calculate depth of loop of edge;

                //Check all nesting loops first (recall procedure)

                bool is_firstEnter = true;

                //Verify the DFlow(CEi), PdFlow(CEi); in new_currentN
                gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "ePdFlow", cc, ref clsSESE, CID, ref CIPd, ref node_return, ref node_return, ref ENTTi, ref is_firstEnter);
                int[] PdF_node = node_return;
                //verify graph.seseNet
                total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.seseNet, i, "ePdFlow", strLoop, ref clsError); //New code => must check the vE node

                //Verify the eFwd(CEi);
                gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "eFwd_IL", cc, ref clsSESE, CID, ref CIPd, ref node_return, ref node_return, ref ENTTi, ref is_firstEnter);
                int[] Fwd_node = node_return;
                total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.seseNet, i, "eFwd_IL", strLoop, ref clsError); //New code => must check the vE node

                //Verify the eBwd(CEi);
                gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "eBwd_IL", cc, ref clsSESE, CID, ref CIPd, ref PdF_node, ref Fwd_node, ref ENTTi, ref is_firstEnter);
                total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph, graph.seseNet, i, "eBwd_IL", strLoop, ref clsError); //New code => must check the vE node

                //convert ENTTi into usable form??
                int[] newENTT = ENTTi;

                //find new concurrent Entry set
                int[] Rec_concurrency = new int[clsLoop.Loop[workLoop].Loop[i].nEntry];
                int n_Rec_concurrency = 0;
                Rec_concurrency = find_ConcurrentEntrySet_Recycling(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, ref n_Rec_concurrency);

                // Make new subgraph with only IL with new === INPUT PARAMETER =>> ENTTi
                gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "IL_ENTTi", cc, ref clsSESE, CID, ref CIPd, ref PdF_node, ref Fwd_node, ref ENTTi, ref is_firstEnter);
                
                //Verify the vicious circle case of "restarting entry" => Fix it? or Report it?;
                //Verify the exits of IL (Must be XOR)
            }
            //Reduce IL
            gProAnalyzer.Ultilities.reduceGraph.reduce_IrLoop_Preprocessing(ref graph, currentN, clsLoop, workLoop, i);
        }

        public static void irreducibleLoop_Verify_Updated(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int i, 
            string strLoop, ref gProAnalyzer.GraphVariables.clsError clsError, ref gProAnalyzer.GraphVariables.clsSESE clsSESE)
        {
            //CHEECK RULE 5
            check_by_Rule5(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError);

            //Find DFlow(entries) =>
            gProAnalyzer.Ultilities.findConcurrencyEntriesIL.check_Concurrency(ref graph, currentN, graph.conNet, graph.subNet, clsLoop, workLoop, i, ref clsSESE); //=> Get clsLoop.Concurrency
            int CID = gProAnalyzer.Ultilities.findConcurrencyEntriesIL.orgCID;
            int[] node_return = null;
            int[] ENTTi = new int[clsLoop.Loop[workLoop].Loop[i].nEntry];

            bool is_firstEnter = true;
            //call VerifyIL_usingConcurrentEntry(ENTT)
            call_Verify_IL_Recs(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE, CID, ref node_return, ref ENTTi, ref is_firstEnter);

            gProAnalyzer.Ultilities.reduceGraph.reduce_IrLoop_Preprocessing(ref graph, currentN, clsLoop, workLoop, i);
        }

        public static void call_Verify_IL_Recs(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int i,
            string strLoop, ref gProAnalyzer.GraphVariables.clsError clsError, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int CID, ref int[] node_return, ref int[] ENTTi, ref bool is_firstEnter)
        {
            for (int cc = 0; cc <= clsLoop.Loop[workLoop].Loop[i].nConcurrency; cc++) //for every ENTTi
            {
                int[] PdF_node = new int[1];
                int[] Fwd_node = new int[1];
                int CIPd = -1;
                //if exclusive => Handle exclusive
                if (cc == 0)
                {
                    for (int exEntry = 0; exEntry < clsLoop.Loop[workLoop].Loop[i].nEntry; exEntry++)
                    {
                        if (clsLoop.Loop[workLoop].Loop[i].Concurrency[exEntry] != 0) continue; //only pick exclusive entries

                        // Make new subgraph with only IL with new === INPUT PARAMETER =>> ENTTi ////Make a independence NL of IL (Separated loop) <<====== using ENTTi (node name)
                        ENTTi = new int[1];
                        ENTTi[0] = clsLoop.Loop[workLoop].Loop[i].Entry[exEntry];

                        gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "IL_ENTTi", cc, ref clsSESE, CID,
                            ref CIPd, ref PdF_node, ref Fwd_node, ref ENTTi, ref is_firstEnter); //===============================================//Remember to check ENTTi

                        //TEST SHOW NETWORK DEBUG ==========================================================
                        frmShow = new frmShowFullModel_Debug("", graph, graph.seseNet); frmShow.ShowDialog();

                        gProAnalyzer.GraphVariables.clsGraph graph_2 = new gProAnalyzer.GraphVariables.clsGraph();
                        gProAnalyzer.GraphVariables.clsSESE clsSESE_2 = new gProAnalyzer.GraphVariables.clsSESE();
                        gProAnalyzer.GraphVariables.clsLoop clsLoop_2 = new gProAnalyzer.GraphVariables.clsLoop();
                        gProAnalyzer.GraphVariables.clsHWLS clsHWLS_2 = new gProAnalyzer.GraphVariables.clsHWLS();

                        //Copy to new graph to be safe - when verifying nesting loop (IL/NL)
                        graph_2.Network[graph.seseNet] = graph.Network[graph_2.seseNet];
                        gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph_2, graph.seseNet, 0, 0);

                        //use seseNet <= orgNet;  reduceTempNet = 16 <= midNet; reduceTempNet2 = 17 <= finalNet;
                        gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph_2, graph_2.seseNet, graph_2.reduceTempNet);
                        gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph_2, graph_2.reduceTempNet, ref clsLoop_2, clsLoop_2.orgLoop, ref clsLoop_2.IrreducibleError);
                        graph_2.Network[graph_2.reduceTempNet2] = graph_2.Network[graph_2.reduceTempNet];
                        gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph_2, graph_2.reduceTempNet, graph_2.reduceTempNet2, ref clsLoop_2, clsLoop_2.orgLoop);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph_2, graph_2.reduceTempNet2);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph_2, graph_2.reduceTempNet2);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph_2, graph_2.reduceTempNet2, -2);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph_2, graph_2.reduceTempNet2);
                        //gProAnalyzer.Functionalities.SESEIdentification.find_SESE_Dummy(ref graph, graph.reduceTempNet2, ref clsLoop_2, clsLoop_2.orgLoop, ref clsSESE_2, clsSESE_2.finalSESE, -2);
                        gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph_2, graph_2.reduceTempNet2, ref clsLoop_2, clsLoop_2.orgLoop, ref clsSESE_2, clsSESE_2.finalSESE, true);
                        //gProAnalyzer.Functionalities.PolygonIdentification.polygonIdentification(ref graph, graph.reduceTempNet2, ref clsSESE_2, clsSESE_2.finalSESE);
                        gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph_2, graph_2.reduceTempNet2, ref clsHWLS_2, ref clsLoop_2, clsLoop_2.orgLoop, ref clsSESE_2, clsSESE_2.finalSESE);

                        iL_concurrency_Error = false;
                        graph_2.Network[graph_2.reduceTempNet] = graph_2.Network[graph_2.reduceTempNet2]; //use reduceTempNet AGAIN
                        gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph_2, graph_2.reduceTempNet, 0, 0);
                        gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop_2, clsLoop_2.orgLoop, clsLoop_2.reduceLoop);
                        //This => Verification Loop nesting except the outer one (The current consider)
                        gProAnalyzer.Functionalities.VerificationG.check_by_Rule2_Nested_in_IL(ref graph_2, graph_2.reduceTempNet, ref clsLoop_2, clsLoop_2.reduceLoop, ref clsSESE_2, clsSESE_2.finalSESE, ref clsHWLS_2, ref clsError, "");

                        //TEST SHOW NETWORK DEBUG ==========================================================
                        frmShow = new frmShowFullModel_Debug("", graph_2, graph_2.reduceTempNet); frmShow.ShowDialog();

                        //===========BE CAREFUL with LOOP i ======================== Need to find i ========== (I TOLD YOU) ===========
                        int newIL_i = -1;
                        for (int k = 0; k < clsLoop_2.Loop[clsLoop_2.reduceLoop].nLoop; k++)
                        {
                            if (clsLoop_2.Loop[clsLoop_2.reduceLoop].Loop[k].depth == 1) newIL_i = k;
                        }

                        //Verify Current Natural Loop
                        naturalLoop_Verify(ref graph_2, graph_2.reduceTempNet, ref clsLoop_2, clsLoop_2.reduceLoop, newIL_i, strLoop, ref clsError, ref clsSESE_2);

                        frmShow = new frmShowFullModel_Debug("", graph_2, graph_2.reduceTempNet); frmShow.ShowDialog();

                        //Find nested loop => make hierarchy of loops (if any) => Verify nested loop first.
                        //STOP => Because it is natural loop => No further structure will be introduced when recycling.
                    }
                }
                else
                {
                    if (cc > 0)
                    {
                        //if concurrency => Handle concurrency ENTTi as normal
                        CIPd = find_CIPd_IL(ref graph, currentN, ref clsLoop, workLoop, i, cc);
                        if (gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, CIPd, i) == false)   //Check the CPId(CEi) => must inside the loop;
                        {
                            iL_concurrency_Error = true;
                            break;
                        }

                        int[] temp_ENTTi = new int[clsLoop.Loop[workLoop].Loop[i].nEntry];
                        int nTemp_ENTTi = 0;
                        for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++)
                        {
                            if (cc == clsLoop.Loop[workLoop].Loop[i].Concurrency[k])
                            {
                                temp_ENTTi[nTemp_ENTTi] = clsLoop.Loop[workLoop].Loop[i].Entry[k];
                                nTemp_ENTTi++;
                            }
                        }

                        ENTTi = new int[nTemp_ENTTi];
                        for (int k = 0; k < nTemp_ENTTi; k++) ENTTi[k] = temp_ENTTi[k];

                        //Make new subgraph with only IL with new === INPUT PARAMETER =>> ENTTi
                        gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "IL_ENTTi", cc, ref clsSESE, CID,
                                    ref CIPd, ref PdF_node, ref Fwd_node, ref ENTTi, ref is_firstEnter); //===============================================//Remember to check ENTTi

                        //TEST SHOW NETWORK DEBUG ==========================================================
                        frmShow = new frmShowFullModel_Debug("", graph, graph.seseNet); frmShow.ShowDialog();

                        gProAnalyzer.GraphVariables.clsGraph graph_2 = new gProAnalyzer.GraphVariables.clsGraph();
                        gProAnalyzer.GraphVariables.clsSESE clsSESE_2 = new gProAnalyzer.GraphVariables.clsSESE();
                        gProAnalyzer.GraphVariables.clsLoop clsLoop_2 = new gProAnalyzer.GraphVariables.clsLoop();
                        gProAnalyzer.GraphVariables.clsHWLS clsHWLS_2 = new gProAnalyzer.GraphVariables.clsHWLS();

                        //Copy to new graph to be safe - when verifying nesting loop (IL/NL) ===>>>>>>>> CHANGE ALL TO GRAPH_2 - SIMILAR TO NATURAL LOOP CASE
                        graph_2.Network[graph.seseNet] = graph.Network[graph.seseNet];
                        gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph_2, graph_2.seseNet, 0, 0);

                        //use seseNet <= orgNet;  reduceTempNet = 16 <= midNet; reduceTempNet2 = 17 <= finalNet;
                        gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph_2, graph_2.seseNet, graph_2.reduceTempNet);
                        gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph_2, graph_2.reduceTempNet, ref clsLoop_2, clsLoop_2.orgLoop, ref clsLoop_2.IrreducibleError);
                        graph_2.Network[graph_2.reduceTempNet2] = graph_2.Network[graph_2.reduceTempNet];
                        gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph_2, graph_2.reduceTempNet, graph_2.reduceTempNet2, ref clsLoop_2, clsLoop_2.orgLoop);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph_2, graph_2.reduceTempNet2);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph_2, graph_2.reduceTempNet2);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph_2, graph_2.reduceTempNet2, -2);
                        gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph_2, graph_2.reduceTempNet2);
                        //gProAnalyzer.Functionalities.SESEIdentification.find_SESE_Dummy(ref graph, graph.reduceTempNet2, ref clsLoop_2, clsLoop_2.orgLoop, ref clsSESE_2, clsSESE_2.finalSESE, -2);
                        gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph_2, graph_2.reduceTempNet2, ref clsLoop_2, clsLoop_2.orgLoop, ref clsSESE_2, clsSESE_2.finalSESE, true);
                        //gProAnalyzer.Functionalities.PolygonIdentification.polygonIdentification(ref graph, graph.reduceTempNet2, ref clsSESE_2, clsSESE_2.finalSESE);
                        gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph_2, graph_2.reduceTempNet2, ref clsHWLS_2, ref clsLoop_2, clsLoop_2.orgLoop, ref clsSESE_2, clsSESE_2.finalSESE);

                        iL_concurrency_Error = false;
                        graph_2.Network[graph_2.reduceTempNet] = graph_2.Network[graph_2.reduceTempNet2]; //use reduceTempNet AGAIN
                        gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph_2, graph_2.reduceTempNet, 0, 0);
                        gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop_2, clsLoop_2.orgLoop, clsLoop_2.reduceLoop);
                        //This => Verification Loop nesting except the outer one (The current consider)
                        gProAnalyzer.Functionalities.VerificationG.check_by_Rule2_Nested_in_IL(ref graph_2, graph_2.reduceTempNet, ref clsLoop_2, clsLoop_2.reduceLoop, ref clsSESE_2, clsSESE_2.finalSESE, ref clsHWLS_2, ref clsError, "");

                        int newIL_i = -1;
                        for (int k = 0; k < clsLoop_2.Loop[clsLoop_2.reduceLoop].nLoop; k++)
                        {
                            if (clsLoop_2.Loop[clsLoop_2.reduceLoop].Loop[k].depth == 1) newIL_i = k; //VERY IMPORTANCE ========== CHECK CAREFUL WHEN USING (newIL_i) or (i)
                        }

                        //TEST SHOW NETWORK DEBUG ========================================================== (AFTER VERIFYING ALL NESTING STRUCTURE) ==================
                        frmShow = new frmShowFullModel_Debug("", graph_2, graph_2.reduceTempNet); frmShow.ShowDialog();




                        //FROM NOW ON, ONLY WORK WITH reduceTempNet =================== and reduceLoop???==========      
                        //================ WORK WITH EACH ACYCLIC SUBGRAPH OF CURRENT IRREDUCIBLE LOOP ============
                        //=========================================================================================


                        //if concurrency => Handle concurrency ENTTi as normal
                        //CIPd = find_CIPd_IL(ref graph, graph.reduceTempNet, ref clsLoop, workLoop, i, cc);
                        //if (gProAnalyzer.Ultilities.checkGraph.Node_In_Loop(clsLoop, workLoop, CIPd, i) == false)   //Check the CPId(CEi) => must inside the loop;
                        //{
                        //    iL_concurrency_Error = true;
                        //    break;
                        //}

                        //copy concurrenEntry set from clsLoop (to clsLoop_2)
                        clsLoop_2.Loop[clsLoop_2.reduceLoop].Loop[newIL_i].Concurrency = clsLoop.Loop[workLoop].Loop[i].Concurrency;
                        clsLoop_2.Loop[clsLoop_2.reduceLoop].Loop[newIL_i].nConcurrency = clsLoop.Loop[workLoop].Loop[i].nConcurrency;

                        //BREAK EACH ACYCLIC SUBGRAPH NO NOT NEED ANY CONCURRENT ENTRY INFORMATION <<============================================

                        //CIPd <== no need
                        CID = graph_2.Network[graph_2.reduceTempNet].Node[graph_2.Network[graph_2.reduceTempNet].header].Post[0]; // get V_S as CID of IL

                        //Verify the DFlow(CEi), PdFlow(CEi); in new_currentN
                        gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph_2, graph_2.reduceTempNet, graph_2.seseNet, ref clsLoop_2, clsLoop_2.reduceLoop, newIL_i, "ePdFlow", cc, ref clsSESE, CID, ref CIPd, ref node_return, ref node_return, ref ENTTi, ref is_firstEnter);
                        PdF_node = node_return;
                        //verify graph.seseNet
                        total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph_2, graph_2.seseNet, i, "ePdFlow", strLoop, ref clsError); //New code => must check the vE node

                        //TEST SHOW NETWORK DEBUG ==========================================================
                        frmShow = new frmShowFullModel_Debug("", graph_2, graph_2.seseNet); frmShow.ShowDialog();

                        //Verify the eFwd(CEi);
                        gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph_2, graph_2.reduceTempNet, graph_2.seseNet, ref clsLoop_2, clsLoop_2.reduceLoop, newIL_i, "eFwd_IL", cc, ref clsSESE, CID, ref CIPd, ref node_return, ref node_return, ref ENTTi, ref is_firstEnter);
                        Fwd_node = node_return;
                        total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph_2, graph_2.seseNet, i, "eFwd_IL", strLoop, ref clsError); //New code => must check the vE node

                        //TEST SHOW NETWORK DEBUG ==========================================================
                        frmShow = new frmShowFullModel_Debug("", graph_2, graph_2.seseNet); frmShow.ShowDialog();

                        //Verify the eBwd(CEi);
                        gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph_2, graph_2.reduceTempNet, graph_2.seseNet, ref clsLoop_2, clsLoop_2.reduceLoop, newIL_i, "eBwd_IL", cc, ref clsSESE, CID, ref CIPd, ref PdF_node, ref Fwd_node, ref ENTTi, ref is_firstEnter);
                        total_int += gProAnalyzer.Ultilities.makeInstanceFlow.make_InstanceFlow(ref graph_2, graph_2.seseNet, i, "eBwd_IL", strLoop, ref clsError); //New code => must check the vE node

                        //TEST SHOW NETWORK DEBUG ==========================================================
                        frmShow = new frmShowFullModel_Debug("", graph_2, graph_2.seseNet); frmShow.ShowDialog();

                        //convert ENTTi into usable form??
                        int[] newENTT = ENTTi;
                        //find new concurrent Entry set
                        int[] Rec_concurrency = new int[clsLoop_2.Loop[clsLoop_2.reduceLoop].Loop[newIL_i].nEntry];
                        int n_Rec_concurrency = 0;
                        //int[] Rec_concurrency_org = new int[clsLoop_2.Loop[clsLoop_2.reduceLoop].Loop[i].nEntry];
                        //int n_Rec_concurrency_org = 0;
                        Rec_concurrency = find_ConcurrentEntrySet_Recycling(ref graph_2, graph_2.reduceTempNet, graph_2.seseNet, ref clsLoop_2, clsLoop_2.reduceLoop, newIL_i, ref n_Rec_concurrency);
                        

                        //CALL RECURSIVE STRUCTURE =>> BE CAREFUL => CHECK CAREFULLY 
                        //============================================================== RECUSIVER ONLY WORK WITH ORGINAL LOOP AND GRAPH ========================================= //                    
                        int n_old_Concurrency = clsLoop.Loop[workLoop].Loop[i].nConcurrency;
                        int[] old_Concurrency = new int[clsLoop.Loop[workLoop].Loop[i].nEntry]; //reset
                        for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++) old_Concurrency[k] = clsLoop.Loop[workLoop].Loop[i].Concurrency[k];

                        for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++) clsLoop.Loop[workLoop].Loop[i].Concurrency[k] = -1;

                        //this two will be embedded to the clsLoop.Loop[workloop].loop[i] when RECURSIVE =============================================================
                        add_new_Concurrency(ref graph_2, ref clsLoop, ref clsLoop_2, workLoop, i, newIL_i, Rec_concurrency, n_Rec_concurrency);

                        //FIX Rec_concurrency (If all is ZERO)
                        FIX_Re_Concurrency(ref graph_2, graph_2.reduceTempNet, ref clsLoop, workLoop, i, newENTT);

                        //CHECK IF IS NECESSARY FOR RECURSIVE?? IF {_} THEN {_}
                        if (!check_out_Recursive_Upgraded(old_Concurrency, clsLoop.Loop[workLoop].Loop[i].Concurrency))
                        {
                            is_firstEnter = false;
                            //call Recursive  //NEED TO CHANGE [currentN] TO [reduceTempNet] ????? ========================
                            call_Verify_IL_Recs(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE, CID, ref node_return, ref ENTTi, ref is_firstEnter);
                            is_firstEnter = true;
                        }

                        //RETURN the original state of concurrency for next FOR loop     
                        clsLoop.Loop[workLoop].Loop[i].Concurrency = new int[clsLoop.Loop[workLoop].Loop[i].nEntry]; //reset
                        for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++) clsLoop.Loop[workLoop].Loop[i].Concurrency[k] = old_Concurrency[k];
                        clsLoop.Loop[workLoop].Loop[i].nConcurrency = n_old_Concurrency;
                        //==============================================================================================================================================

                        //Return state of  CurrentN??

                        // Make new subgraph with only IL with new === INPUT PARAMETER =>> ENTTi
                        //gProAnalyzer.Ultilities.makeSubNetwork.make_IL_Subgraph(ref graph, currentN, graph.seseNet, ref clsLoop, workLoop, i, "IL_ENTTi", cc, ref clsSESE, CID, ref CIPd, ref PdF_node, ref Fwd_node, ref ENTTi);
                    }
                }
            }            
        }

        //REMOVE ANNOYING EXCLUSIVE ENTRY (ZERO: 0-0-0) WHICH SOME IS NEVER ACTIVE => Negative
        public static void FIX_Re_Concurrency(ref GraphVariables.clsGraph graph, int reduceNet, ref GraphVariables.clsLoop clsLoop, int workLoop, int newIL_i, int[] newENTT)
        {
            //Convert index of newENTT to original index of CLSLOOP
            int[] org_newENTT = new int[newENTT.Length];
            for (int k = 0; k < newENTT.Length; k++) org_newENTT[k] = graph.Network[reduceNet].Node[newENTT[k]].orgNum;       
     
            //processing loop[i].concurrency
            int[] nonExclusive_en = new int[clsLoop.Loop[workLoop].Loop[newIL_i].nEntry];
            int n_nonEx = 0;
            if (org_newENTT.Length < clsLoop.Loop[workLoop].Loop[newIL_i].nEntry)
            {
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[newIL_i].nEntry; k++)
                {
                    bool check = false;
                    for (int l = 0; l < org_newENTT.Length; l++)
                    {
                        if (clsLoop.Loop[workLoop].Loop[newIL_i].Entry[k] == org_newENTT[l])
                        {
                            check = true;
                            break;
                        }
                    }
                    if (check == false)
                        clsLoop.Loop[workLoop].Loop[newIL_i].Concurrency[k] = -1;
                }
            }
            else
                return;
        }

        //Add Loop[i].Concurrency with new Rec_Cocurrency
        public static void add_new_Concurrency(ref GraphVariables.clsGraph graph, ref GraphVariables.clsLoop clsLoop, ref GraphVariables.clsLoop clsLoop_2, int workLoop, 
            int i, int newIL_i, int[] Rec_concurrency, int n_Rec_concurrency)
        {
            clsLoop.Loop[workLoop].Loop[i].Concurrency = new int[clsLoop.Loop[workLoop].Loop[i].nEntry]; //reset Concurrency
            clsLoop.Loop[workLoop].Loop[i].nConcurrency = n_Rec_concurrency;

            //copy new CONCURRENCY SET to WORKLOOP again ==>> Ready for new iteration
            for (int k = 0; k < clsLoop_2.Loop[clsLoop.reduceLoop].Loop[newIL_i].nEntry; k++)
            {
                int orgEntry = graph.Network[graph.reduceTempNet].Node[clsLoop_2.Loop[clsLoop.reduceLoop].Loop[newIL_i].Entry[k]].orgNum;
                for (int m = 0; m < clsLoop.Loop[workLoop].Loop[i].nEntry; m++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].Entry[m] == orgEntry)
                    {
                        clsLoop.Loop[workLoop].Loop[i].Concurrency[m] = Rec_concurrency[k];
                        break;
                    }
                }
            }
        }

        public static void check_by_Rule2_Nested_in_IL(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, ref gProAnalyzer.GraphVariables.clsHWLS clsHWLS, ref gProAnalyzer.GraphVariables.clsError clsError, string strLoop)
        {
            watch_Bond = new System.Diagnostics.Stopwatch();
            watch_Rigid = new System.Diagnostics.Stopwatch();
            watch_IL = new System.Diagnostics.Stopwatch();
            watch_Bond.Reset();
            watch_Rigid.Reset();
            watch_IL.Reset();

            string strOrgLoop = strLoop;
            int curDepth = clsLoop.Loop[workLoop].maxDepth;
            total_int = 0;
            //informList[13] = 0; //# of bonds
            //informList[14] = 0; //# of rigids

            if (clsLoop.Loop[workLoop].nLoop == 1) return; //if there is no more loop (except itself) => RETURN

            do
            {
                for (int j = 0; j < clsLoop.Loop[workLoop].nLoop; j++)
                {
                    if (clsLoop.Loop[workLoop].Loop[j].depth != curDepth) continue;
                    int i = j;
                    if (strOrgLoop == "") strLoop = i.ToString();
                    else strLoop = strOrgLoop + "-" + i.ToString();

                    if (clsLoop.Loop[workLoop].Loop[i].Irreducible) // Irreducible Loop
                    {
                        //watch_IL.Start();

                        irreducibleLoop_Verify_Updated(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE);

                        //irreducibleLoop_Verify_Updated(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE);
                        //watch_IL.Stop();
                    }
                    else //Natural Loop
                    {
                        naturalLoop_Verify(ref graph, currentN, ref clsLoop, workLoop, i, strLoop, ref clsError, ref clsSESE);
                    }
                }
                curDepth--;
            } while (curDepth > 1); //not verify the outer loop (IL for example)
        }

        public static bool check_out_Recursive(int[] oldArr, int[] newArr, int cc)
        {
            int count_old = 0;
            int count_old2 = 0;
            int count_new = 0;
            for (int i = 0; i < newArr.Length; i++)
            {
                if (newArr[i] > 0) newArr[i] = cc;
            }
            for (int i = 0; i < oldArr.Length; i++)
            {
                if (oldArr[i] == cc)
                {
                    if (newArr[i] > 0) count_old++;
                    count_old2++;
                }
            }
            if ((count_old != count_new) || count_old != count_old2) return false;
            return true;
        }

        public static bool check_out_Recursive_Upgraded(int[] oldArr, int[] newArr) //check same => out recursive => true; //check diff => not out => false
        {
            int maxOld = max(oldArr);
            int maxNew = max(newArr);
            string[] s_oldPattern = new string[maxOld + 1];
            string[] s_newPattern = new string[maxNew + 1];

            int count = 1;
            //Extract pattern of oldArr
            for (int i = 1; i <= maxOld; i++)
            {
                for (int j = 0; j < oldArr.Length; j++)
                {
                    if (oldArr[j] == i) s_oldPattern[i] = s_oldPattern[i] + j.ToString() + ".";
                }
            }
            //Extract pattern of newArr
            for (int i = 1; i <= maxNew; i++)
            {
                for (int j = 0; j < newArr.Length; j++)
                {
                    if (newArr[j] == i) s_newPattern[i] = s_newPattern[i] + j.ToString() + ".";
                }
            }

            bool check_same = false;
            for (int i = 1; i < s_newPattern.Length; i++)
            {
                for (int j = 1; j < s_oldPattern.Length; j++)
                {
                    if (s_oldPattern[j] == s_newPattern[i])
                    {
                        check_same = true;
                        break;
                    }
                }
            }

            //if (count1 == count2) return true;
            return check_same;
        }

        public static int max(int[] A)
        {
            int max = -1;
            for (int i = 0; i < A.Length; i++)
            {
                if (max < A[i]) max = A[i];
            }
            return max;
        }
        //===========================================================================
        public static bool all_same_kind_ExceptHeader(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, int currentSESE)
        {
            string Gateway_kind = "";
            Gateway_kind = graph.Network[currentN].Node[clsSESE.SESE[workSESE].SESE[currentSESE].Exit].Kind;
            int entry = clsSESE.SESE[workSESE].SESE[currentSESE].Entry;
            for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = clsSESE.SESE[workSESE].SESE[currentSESE].Node[i];
                //if (i == 0) Gateway_kind = graph.Network[currentN].Node[node].Kind;
                //if (graph.Network[currentN].Node[node].done)
                if (graph.Network[currentN].Node[node].nPre > 1 || graph.Network[currentN].Node[node].nPost > 1)
                {
                    if (graph.Network[currentN].Node[node].Kind != Gateway_kind && node != entry) return false;
                }
            }
            return true;
        }

        public static bool all_same_kind(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, int currentSESE)
        {
            string Gateway_kind = "";
            for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = clsSESE.SESE[workSESE].SESE[currentSESE].Node[i];
                if (i == 0) Gateway_kind = graph.Network[currentN].Node[node].Kind;
                if (graph.Network[currentN].Node[node].nPre > 1 || graph.Network[currentN].Node[node].nPost > 1)
                {
                    if (graph.Network[currentN].Node[node].Kind != Gateway_kind) return false;
                }
            }
            return true;
        }
        public static bool all_OR_join(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, int currentSESE)
        {
            for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = clsSESE.SESE[workSESE].SESE[currentSESE].Node[i];
                if (graph.Network[currentN].Node[node].nPre > 1) //consider Join-node
                {
                    if (graph.Network[currentN].Node[node].Kind != "OR") return false;
                }
            }
            return true;
        }

        /*
        public static void count_OR_Split_Rigids(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int workSESE, int currentSESE)
        {
            //if (checkReRun) return;

            for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node = clsSESE.SESE[workSESE].SESE[currentSESE].Node[i];
                if (graph.Network[currentN].Node[node].nPost > 0 && graph.Network[currentN].Node[node].nPre > 0)
                {
                    if (graph.Network[currentN].Node[node].Kind == "OR" && graph.Network[currentN].Node[node].nPost > 1)
                    {
                        if (graph.Network[currentN].Node[node].Name == "SS") // for SS only
                        {
                            informList[18]++; //# of OR split rigids for SS
                            informList[19] = informList[19] + graph.Network[currentN].Node[node].nPost; //# of Edges from OR split for SS
                        }
                        else if (graph.Network[currentN].Node[node].Name != "SS")// for non_SS only
                        {
                            informList[20]++; //# of OR split rigids for non-SS
                            informList[21] = informList[19] + graph.Network[currentN].Node[node].nPost; //# of Edges from OR split for SS
                        }
                    }
                }
            }
        }
        public static void check_Concurrency(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int loop)
        {
            //make_ConcurrencyFlow
            graph.Network[graph.conNet] = graph.Network[currentN]; //CIDFlow begining is stored in conNet (Network[8])
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.conNet, 0, 0);

            gProAnalyzer.Ultilities.copyLoop.copy_Loop(ref clsLoop, workLoop, clsLoop.tempLoop);

            prepare_Concurrency(graph.conNet, clsLoop.tempLoop, loop); //reduce all loop except "loop" and its parents.

            make_subNetwork(graph.conNet, graph.subNet, clsLoop.tempLoop, loop, "CC", -1);    //5는 SubNetwork //"CC" ~ concurrent entries => after that we will have iDFlow()

            make_ConcurrencyInstance(graph.subNet, clsLoop.tempLoop, loop); //tempLoop ~ Loop[3]

            //Prepare the entry conEntry[,] array => Simply set the value by [1 0 0] [0 1 0] or ...
            prepare_Find_CC(graph.subNet, clsLoop.tempLoop, loop); //new

            //Everything related with CE will be solved in here =====================================
            //find_ConcurrencyEntrySet(subNet, tempLoop, loop); //=====================================
            //=======================================================================================

            //copy concurrency inform => IT WAS WRONG!!!!
            clsLoop.Loop[workLoop].Loop[loop].nConcurrency = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].nConcurrency; //tranfer all the concurrency value from tempLoop [3] to workloop [2]
            if (clsLoop.Loop[tempLoop].Loop[loop].Concurrency != null)
            {
                clsLoop.Loop[workLoop].Loop[loop].Concurrency = new int[clsLoop.Loop[workLoop].Loop[loop].nEntry];
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++)
                    clsLoop.Loop[workLoop].Loop[loop].Concurrency[k] = clsLoop.Loop[clsLoop.tempLoop].Loop[loop].Concurrency[k];
            }
        }
        public static void prepare_Concurrency(int currentN, int workLoop, int loop)
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
        
        */
          
        public static void check_by_Rule1(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int loop, string strLoop, ref gProAnalyzer.GraphVariables.clsError clsError)
        {
            if (clsLoop.Loop[workLoop].Loop[loop].Irreducible) return;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                //if (!Node_In_Loop(workLoop, i, loop)) continue; //check node in loop;                
                bool check = false;
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nNode; k++)
                {
                    if (clsLoop.Loop[workLoop].Loop[loop].header == i)
                    {
                        check = true;
                        break;
                    }
                    if (clsLoop.Loop[workLoop].Loop[loop].Node[k] == i)
                    {
                        check = true;
                        break;
                    }
                }
                if (check == false) continue;
                if (graph.Network[currentN].Node[i].Special == "" || graph.Network[currentN].Node[i].Special == null) continue;
                if (graph.Network[currentN].Node[i].Kind == "XOR") continue; // error 아님 (//free)

                clsError.Error[clsError.nError].Loop = strLoop;
                clsError.Error[clsError.nError].Node = graph.Network[currentN].Node[i].parentNum.ToString();
                clsError.Error[clsError.nError].currentKind = graph.Network[currentN].Node[i].Kind;

                if (graph.Network[currentN].Node[i].Special == "E") clsError.Error[clsError.nError].messageNum = 0;
                else if (graph.Network[currentN].Node[i].Special == "X" || graph.Network[currentN].Node[i].Special == "T") clsError.Error[clsError.nError].messageNum = 1;
                else clsError.Error[clsError.nError].messageNum = 2;

                //nError++;
                gProAnalyzer.Ultilities.recordData.add_Error(ref clsError);
            }
        }

        public static void check_by_Rule5(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int loop, string strLoop, ref gProAnalyzer.GraphVariables.clsError clsError)
        {
            //if (clsLoop.Loop[workLoop].Loop[loop].Irreducible) return;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                //if (!Node_In_Loop(workLoop, i, loop)) continue; //check node in loop;                
                bool check = false;
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nNode; k++)
                {
                    if (clsLoop.Loop[workLoop].Loop[loop].header == i)
                    {
                        check = true;
                        break;
                    }
                    if (clsLoop.Loop[workLoop].Loop[loop].Node[k] == i)
                    {
                        check = true;
                        break;
                    }
                }
                if (check == false) continue;
                if (graph.Network[currentN].Node[i].Special == "" || graph.Network[currentN].Node[i].Special == null) continue;
                if (graph.Network[currentN].Node[i].Kind == "XOR") continue; // error 아님 (//free)
                
                //if (graph.Network[currentN].Node[i].Special == "E") clsError.Error[clsError.nError].messageNum = 0;
                //else if (graph.Network[currentN].Node[i].Special == "X" || graph.Network[currentN].Node[i].Special == "T") clsError.Error[clsError.nError].messageNum = 1;
                //else clsError.Error[clsError.nError].messageNum = 2;

                if (graph.Network[currentN].Node[i].Special == "E")
                {
                    //Not check Entry not XOR-join for now
                    //clsError.Error[clsError.nError].messageNum = 16;
                }

                if (graph.Network[currentN].Node[i].Special == "X")
                {
                    clsError.Error[clsError.nError].Loop = strLoop;
                    clsError.Error[clsError.nError].Node = graph.Network[currentN].Node[i].parentNum.ToString();
                    clsError.Error[clsError.nError].currentKind = graph.Network[currentN].Node[i].Kind;
                    clsError.Error[clsError.nError].messageNum = 17;

                    //nError++;
                    gProAnalyzer.Ultilities.recordData.add_Error(ref clsError);
                }

                
            }
        }

        //find CIPd IL for each given concurrent entry [mark]
        public static int find_CIPd_IL(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsLoop clsLoop, int workLoop, int currLoop, int currentCC_indx)
        {
            if (clsLoop.Loop[workLoop].Loop[currLoop].nEntry == 1) return -1;
            int CIPd = -1;
            int[] calDomRev = null;

            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++)
            {
                if (clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[k] != currentCC_indx) continue;
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

        public static int[] find_ConcurrentEntrySet_Recycling(ref GraphVariables.clsGraph graph, int currentN, int subNet, ref GraphVariables.clsLoop clsLoop, int workLoop, int currLoop, 
            ref int nConcurrency)
        {
            //INPUT: subNET = eBwd(ENTT)
            //OUTPUT: assign to the field loop[i].ConcurrenEntry[];

            gProAnalyzer.Ultilities.copyLoop.copy_Loop_Simplified(ref clsLoop, workLoop, clsLoop.tempLoop);

            gProAnalyzer.Ultilities.findConcurrencyEntriesIL.make_ConcurrencyInstance(graph, subNet, ref clsLoop, clsLoop.tempLoop, currLoop);

            //copy concurrency inform =============== THIS IS FOR THE VERIFICATION PAPER ONLY
            //TRANSFER all the concurrency value from TEMPLOOP [3] to WORKLOOP [2]

            //clsLoop.Loop[workLoop].Loop[currLoop].nConcurrency = clsLoop.Loop[clsLoop.tempLoop].Loop[currLoop].nConcurrency; 
            //if (clsLoop.Loop[clsLoop.tempLoop].Loop[currLoop].Concurrency != null)
            //{
            //    clsLoop.Loop[workLoop].Loop[currLoop].Concurrency = new int[clsLoop.Loop[workLoop].Loop[currLoop].nEntry];
            //    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[currLoop].nEntry; k++)
            //        clsLoop.Loop[workLoop].Loop[currLoop].Concurrency[k] = clsLoop.Loop[clsLoop.tempLoop].Loop[currLoop].Concurrency[k];
            //}


            //return value:
            nConcurrency = clsLoop.Loop[clsLoop.tempLoop].Loop[currLoop].nConcurrency;
            return clsLoop.Loop[clsLoop.tempLoop].Loop[currLoop].Concurrency;

        }
    }
}
