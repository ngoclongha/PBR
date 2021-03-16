using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer
{
    class Testing
    {
        //private gProAnalyzer.GraphVariables.clsGraph graph;
        //private gProAnalyzer.GraphVariables.clsLoop clsLoop;
        //private gProAnalyzer.GraphVariables.clsSESE clsSESE;

        private gProAnalyzer.Functionalities.NodeSplittingType1 SplitType1;
        private gProAnalyzer.Functionalities.NodeSplittingType2 SplitType2;
        private gProAnalyzer.Functionalities.NodeSplittingType3 SplitType3;
        private gProAnalyzer.Functionalities.LoopIdentification findLoop;
        private gProAnalyzer.Functionalities.DominanceIdentification fndDomRel;
        private gProAnalyzer.Functionalities.SESEIdentification sese;
        private gProAnalyzer.Ultilities.makeInstanceFlow makInst;
        private gProAnalyzer.Ultilities.makeNestingForest makNestingForest;
        private gProAnalyzer.Ultilities.extendGraph extendG;
        private gProAnalyzer.Functionalities.IndexingPM indexing;

        public double duration;

        public void Initialize_All()
        {
            //clsLoop = new GraphVariables.clsLoop();
            //clsSESE = new GraphVariables.clsSESE();
            SplitType1 = new gProAnalyzer.Functionalities.NodeSplittingType1();
            SplitType2 = new gProAnalyzer.Functionalities.NodeSplittingType2();
            SplitType3 = new gProAnalyzer.Functionalities.NodeSplittingType3();
            findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
            fndDomRel = new gProAnalyzer.Functionalities.DominanceIdentification();
            sese = new gProAnalyzer.Functionalities.SESEIdentification();
            makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
            makNestingForest = new gProAnalyzer.Ultilities.makeNestingForest();
            extendG = new gProAnalyzer.Ultilities.extendGraph();

            indexing = new Functionalities.IndexingPM();

        }
        public int RunTest(ref gProAnalyzer.GraphVariables.clsGraph graph, ref gProAnalyzer.GraphVariables.clsHWLS clsHWLS, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, 
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int numRun)
        {
            duration = 0;
            HiPerfTimer pt = new HiPerfTimer();
            pt.Start();

            for (int i = 0; i < numRun; i++) {

                Initialize_All();

                //SplitType1.Run_Split_Type1(ref graph, graph.orgNet, graph.midNet);

                //findLoop.Run_FindLoop(ref graph, graph.midNet, ref clsLoop, clsLoop.orgLoop, ref clsLoop.IrreducibleError);

                graph.Network[graph.finalNet] = graph.Network[graph.orgNet];
                //SplitType2.Run_Split_Type2(ref graph, graph.midNet, graph.finalNet, ref clsLoop, clsLoop.orgLoop);

                gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, graph.finalNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, graph.finalNet);
                gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, graph.finalNet, -2);
                gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, graph.finalNet);

                //sese.find_SESE_WithLoop(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);
                gProAnalyzer.Functionalities.SESEIdentification.find_SESE_Dummy(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, -2);


                gProAnalyzer.Functionalities.NodeSplittingType3.Run_Split_Type3(ref graph, graph.finalNet, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE, true);

                //Decompose cyclic subgraphs

                //Make nesting forest?
                gProAnalyzer.Ultilities.makeNestingForest.make_NestingForest(ref graph, graph.finalNet, ref clsHWLS, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE);
            }

            pt.Stop();
            duration += pt.Duration;

            duration = duration/ (double)numRun;
            duration = duration * 1000;

            //makInst.make_InstanceFlow(ref graph, graph.finalNet, 0, "", "");

            //==================================================
            //== get initial behavior profile ==
            //graph.Network[graph.reduceNet] = graph.Network[graph.finalNet];
            //extendG.full_extentNetwork(ref graph, graph.reduceNet, 0, 0);
            //indexing.get_InitialBehaviorProfile(ref graph, graph.reduceNet, ref clsHWLS, ref clsLoop, clsLoop.orgLoop, ref clsSESE, clsSESE.finalSESE);
            //Database storing

            return 0;
        }

    }
}
