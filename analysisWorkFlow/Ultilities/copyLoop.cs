using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class copyLoop
    {
        public static void copy_Loop(ref GraphVariables.clsLoop clsLoop, int fromLoop, int toLoop)
        {
            clsLoop.Loop[toLoop] = new GraphVariables.clsLoop.strLoop();
            clsLoop.Loop[toLoop].maxDepth = clsLoop.Loop[fromLoop].maxDepth;
            clsLoop.Loop[toLoop].nLoop = clsLoop.Loop[fromLoop].nLoop;
            clsLoop.Loop[toLoop].Loop = new GraphVariables.clsLoop.strLoopInform[clsLoop.Loop[toLoop].nLoop];

            for (int i = 0; i < clsLoop.Loop[toLoop].nLoop; i++) {
                clsLoop.Loop[toLoop].Loop[i].Irreducible = clsLoop.Loop[fromLoop].Loop[i].Irreducible;
                clsLoop.Loop[toLoop].Loop[i].depth = clsLoop.Loop[fromLoop].Loop[i].depth;

                clsLoop.Loop[toLoop].Loop[i].parentLoop = clsLoop.Loop[fromLoop].Loop[i].parentLoop;
                clsLoop.Loop[toLoop].Loop[i].nChild = clsLoop.Loop[fromLoop].Loop[i].nChild;
                clsLoop.Loop[toLoop].Loop[i].child = new int[clsLoop.Loop[toLoop].Loop[i].nChild];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nChild; k++)
                    clsLoop.Loop[toLoop].Loop[i].child[k] = clsLoop.Loop[fromLoop].Loop[i].child[k];

                clsLoop.Loop[toLoop].Loop[i].header = clsLoop.Loop[fromLoop].Loop[i].header;

                clsLoop.Loop[toLoop].Loop[i].nBackEdge = clsLoop.Loop[fromLoop].Loop[i].nBackEdge;
                clsLoop.Loop[toLoop].Loop[i].linkBack = new int[clsLoop.Loop[toLoop].Loop[i].nBackEdge];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nBackEdge; k++)
                    clsLoop.Loop[toLoop].Loop[i].linkBack[k] = clsLoop.Loop[fromLoop].Loop[i].linkBack[k];

                clsLoop.Loop[toLoop].Loop[i].nBackSplit = clsLoop.Loop[fromLoop].Loop[i].nBackSplit;
                clsLoop.Loop[toLoop].Loop[i].BackSplit = new int[clsLoop.Loop[toLoop].Loop[i].nBackSplit];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nBackSplit; k++)
                    clsLoop.Loop[toLoop].Loop[i].BackSplit[k] = clsLoop.Loop[fromLoop].Loop[i].BackSplit[k];

                clsLoop.Loop[toLoop].Loop[i].nEntry = clsLoop.Loop[fromLoop].Loop[i].nEntry;
                clsLoop.Loop[toLoop].Loop[i].Entry = new int[clsLoop.Loop[toLoop].Loop[i].nEntry];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nEntry; k++)
                    clsLoop.Loop[toLoop].Loop[i].Entry[k] = clsLoop.Loop[fromLoop].Loop[i].Entry[k];

                clsLoop.Loop[toLoop].Loop[i].nExit = clsLoop.Loop[fromLoop].Loop[i].nExit;
                clsLoop.Loop[toLoop].Loop[i].Exit = new int[clsLoop.Loop[toLoop].Loop[i].nExit];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nExit; k++)
                    clsLoop.Loop[toLoop].Loop[i].Exit[k] = clsLoop.Loop[fromLoop].Loop[i].Exit[k];

                clsLoop.Loop[toLoop].Loop[i].nNode = clsLoop.Loop[fromLoop].Loop[i].nNode;
                clsLoop.Loop[toLoop].Loop[i].Node = new int[clsLoop.Loop[toLoop].Loop[i].nNode];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nNode; k++)
                    clsLoop.Loop[toLoop].Loop[i].Node[k] = clsLoop.Loop[fromLoop].Loop[i].Node[k];

                clsLoop.Loop[toLoop].Loop[i].nConcurrency = clsLoop.Loop[fromLoop].Loop[i].nConcurrency;
                if (clsLoop.Loop[fromLoop].Loop[i].Concurrency != null) {
                    clsLoop.Loop[toLoop].Loop[i].Concurrency = new int[clsLoop.Loop[toLoop].Loop[i].nEntry];
                    for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nEntry; k++)
                        clsLoop.Loop[toLoop].Loop[i].Concurrency[k] = clsLoop.Loop[fromLoop].Loop[i].Concurrency[k];
                }

                //new ============ Untangling IL InstanceNode ===== 
                clsLoop.Loop[toLoop].Loop[i].nConcurrInst = clsLoop.Loop[fromLoop].Loop[i].nConcurrInst;
                if (clsLoop.Loop[fromLoop].Loop[i].nConcurrency != 0) {
                    clsLoop.Loop[toLoop].Loop[i].concurrInst = new int[clsLoop.Loop[toLoop].Loop[i].nEntry][];
                    for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nEntry; k++)
                        clsLoop.Loop[toLoop].Loop[i].concurrInst[k] = clsLoop.Loop[fromLoop].Loop[i].concurrInst[k];
                }
            }
        }

        //REMOVE UNTANGLING FEATURES
        public static void copy_Loop_Simplified(ref GraphVariables.clsLoop clsLoop, int fromLoop, int toLoop)
        {
            clsLoop.Loop[toLoop] = new GraphVariables.clsLoop.strLoop();
            clsLoop.Loop[toLoop].maxDepth = clsLoop.Loop[fromLoop].maxDepth;
            clsLoop.Loop[toLoop].nLoop = clsLoop.Loop[fromLoop].nLoop;
            clsLoop.Loop[toLoop].Loop = new GraphVariables.clsLoop.strLoopInform[clsLoop.Loop[toLoop].nLoop];

            for (int i = 0; i < clsLoop.Loop[toLoop].nLoop; i++)
            {
                clsLoop.Loop[toLoop].Loop[i].Irreducible = clsLoop.Loop[fromLoop].Loop[i].Irreducible;
                clsLoop.Loop[toLoop].Loop[i].depth = clsLoop.Loop[fromLoop].Loop[i].depth;

                clsLoop.Loop[toLoop].Loop[i].parentLoop = clsLoop.Loop[fromLoop].Loop[i].parentLoop;
                clsLoop.Loop[toLoop].Loop[i].nChild = clsLoop.Loop[fromLoop].Loop[i].nChild;
                clsLoop.Loop[toLoop].Loop[i].child = new int[clsLoop.Loop[toLoop].Loop[i].nChild];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nChild; k++)
                    clsLoop.Loop[toLoop].Loop[i].child[k] = clsLoop.Loop[fromLoop].Loop[i].child[k];

                clsLoop.Loop[toLoop].Loop[i].header = clsLoop.Loop[fromLoop].Loop[i].header;

                clsLoop.Loop[toLoop].Loop[i].nBackEdge = clsLoop.Loop[fromLoop].Loop[i].nBackEdge;
                clsLoop.Loop[toLoop].Loop[i].linkBack = new int[clsLoop.Loop[toLoop].Loop[i].nBackEdge];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nBackEdge; k++)
                    clsLoop.Loop[toLoop].Loop[i].linkBack[k] = clsLoop.Loop[fromLoop].Loop[i].linkBack[k];

                clsLoop.Loop[toLoop].Loop[i].nBackSplit = clsLoop.Loop[fromLoop].Loop[i].nBackSplit;
                clsLoop.Loop[toLoop].Loop[i].BackSplit = new int[clsLoop.Loop[toLoop].Loop[i].nBackSplit];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nBackSplit; k++)
                    clsLoop.Loop[toLoop].Loop[i].BackSplit[k] = clsLoop.Loop[fromLoop].Loop[i].BackSplit[k];

                clsLoop.Loop[toLoop].Loop[i].nEntry = clsLoop.Loop[fromLoop].Loop[i].nEntry;
                clsLoop.Loop[toLoop].Loop[i].Entry = new int[clsLoop.Loop[toLoop].Loop[i].nEntry];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nEntry; k++)
                    clsLoop.Loop[toLoop].Loop[i].Entry[k] = clsLoop.Loop[fromLoop].Loop[i].Entry[k];

                clsLoop.Loop[toLoop].Loop[i].nExit = clsLoop.Loop[fromLoop].Loop[i].nExit;
                clsLoop.Loop[toLoop].Loop[i].Exit = new int[clsLoop.Loop[toLoop].Loop[i].nExit];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nExit; k++)
                    clsLoop.Loop[toLoop].Loop[i].Exit[k] = clsLoop.Loop[fromLoop].Loop[i].Exit[k];

                clsLoop.Loop[toLoop].Loop[i].nNode = clsLoop.Loop[fromLoop].Loop[i].nNode;
                clsLoop.Loop[toLoop].Loop[i].Node = new int[clsLoop.Loop[toLoop].Loop[i].nNode];
                for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nNode; k++)
                    clsLoop.Loop[toLoop].Loop[i].Node[k] = clsLoop.Loop[fromLoop].Loop[i].Node[k];

                clsLoop.Loop[toLoop].Loop[i].nConcurrency = clsLoop.Loop[fromLoop].Loop[i].nConcurrency;
                if (clsLoop.Loop[fromLoop].Loop[i].Concurrency != null)
                {
                    clsLoop.Loop[toLoop].Loop[i].Concurrency = new int[clsLoop.Loop[toLoop].Loop[i].nEntry];
                    for (int k = 0; k < clsLoop.Loop[toLoop].Loop[i].nEntry; k++)
                        clsLoop.Loop[toLoop].Loop[i].Concurrency[k] = clsLoop.Loop[fromLoop].Loop[i].Concurrency[k];
                }

                //new ============ Untangling IL InstanceNode ===== 
                //REMOVED
            }
        }
    }
}
