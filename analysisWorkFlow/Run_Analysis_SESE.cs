using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer
{
    class Run_Analysis_SESE
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

        public double[] informList = new double[30];

        public frmAnalysisNetwork frmAnl;

        bool totalConcurrent = false;
        bool existConcurrent = false;
        bool totalCausal = false;
        bool existCausal = false;
        bool canConflict = false;
        bool NotcanConflict = false;
        bool canCoocur = false;
        bool notCancoocur = false;

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
        }

        public void run_SESE(ref GraphVariables.clsGraph graph, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsHWLS clsHWLS_Untangle,
            ref GraphVariables.clsLoop clsLoop, ref GraphVariables.clsSESE clsSESE)
        {
            HiPerfTimer pt = new HiPerfTimer();
            pt.Start();
            double time = 0;

            DateTime dt = new DateTime();
            DateTime dt2 = new DateTime();
            double duration = 0;
            double duration_SESE = 0;
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            Initialize_All();

            for (int i = 0; i < 1; i++)
            {                
                dt = DateTime.Now;
                watch.Start();

                gProAnalyzer.Functionalities.NodeSplittingType1.Run_Split_Type1(ref graph, graph.orgNet, graph.midNet);

                gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.orgLoop, ref clsLoop.IrreducibleError);

                graph.Network[graph.finalNet] = graph.Network[graph.midNet];
                gProAnalyzer.Functionalities.NodeSplittingType2.Run_Split_Type2(ref graph, graph.midNet, graph.finalNet, ref clsLoop, clsLoop.orgLoop);

                gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.finalNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.finalNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.finalNet, -2);
                gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.finalNet);

                dt2 = DateTime.Now;
                gProAnalyzer.Functionalities.SESEIdentification.find_SESE_Dummy(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);
                duration_SESE = duration_SESE + (DateTime.Now - dt2).TotalMilliseconds;

                gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, true);

                gProAnalyzer.Functionalities.PolygonIdentification.polygonIdentification(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE);

                //Make nesting forest
                gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph, graph.finalNet, ref clsHWLS, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE);
                
                duration = duration + (DateTime.Now - dt).TotalMilliseconds;
                watch.Stop();
            }

            informList = new double[30];
            count_Bonds_Rigids(ref graph, graph.finalNet, ref clsSESE, clsSESE.finalSESE, ref clsLoop, clsLoop.orgLoop, ref clsHWLS);

            MessageBox.Show("The SESE identification is in: " + (duration_SESE).ToString() + " milisecond", "Finish-SESE");
            MessageBox.Show("The System has finished identify the SESE region in: " + (watch.ElapsedMilliseconds/ 1).ToString() + " milisecond", "Finish");

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
                            gProAnalyzer.Ultilities.reduceGraph.reduce_IrLoop(ref graph, currentN, clsLoop, workLoop, i);
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
                                    informList[13]++;
                                    clsHWLS.FBLOCK.FBlock[j].type = "B";
                                }
                                else
                                {
                                    informList[14]++;
                                    clsHWLS.FBLOCK.FBlock[j].type = "R";
                                }
                            }
                            gProAnalyzer.Ultilities.reduceGraph.reduce_Loop(ref graph, currentN, ref clsLoop, workLoop, i, "", true);
                        }
                    }
                }
                curDepth--;
            } while (curDepth > 0);
        }      
    }
}
