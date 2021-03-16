using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class makeNestingForest
    {
        public static void make_NestingForest(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsHWLS clsHWLS, ref GraphVariables.clsLoop clsLoop, 
            int workLoop, ref GraphVariables.clsSESE clsSESE, int workSESE)
        {
            int bIndex = 0;
            //FBLOCK.FBlock = new strFBlock[SESE[currentSESE].nSESE + Loop[currentLoop].nLoop];
            if (workSESE >= 0 && workLoop >= 0) {
                clsHWLS.FBLOCK.FBlock = new GraphVariables.clsHWLS.strFBlock[clsSESE.SESE[workSESE].nSESE + clsLoop.Loop[workLoop].nLoop];                
            }
            else {
                if (workSESE >= 0) {
                    clsHWLS.FBLOCK.FBlock = new GraphVariables.clsHWLS.strFBlock[clsSESE.SESE[workSESE].nSESE];
                }
                else {
                    clsHWLS.FBLOCK.FBlock = new GraphVariables.clsHWLS.strFBlock[clsLoop.Loop[workLoop].nLoop];
                }
            }

            //Copy SESE
            if (workSESE >= 0 && clsSESE.SESE[workSESE].nSESE > 0) 
            {
                //copy SESE and LOOP to FBlock;
                for (int i = 0; i < clsSESE.SESE[workSESE].nSESE; i++) {
                    //copy SESE to FBlock
                    //FBlock[bIndex].child = SESE[currentSESE].SESE[i].child;
                    //FBlock[bIndex].depth = SESE[currentSESE].SESE[i].depth;
                    clsHWLS.FBLOCK.FBlock[bIndex].Entry = new int[1];
                    clsHWLS.FBLOCK.FBlock[bIndex].Entry[0] = clsSESE.SESE[workSESE].SESE[i].Entry;
                    clsHWLS.FBLOCK.FBlock[bIndex].Exit = new int[1];
                    clsHWLS.FBLOCK.FBlock[bIndex].Exit[0] = clsSESE.SESE[workSESE].SESE[i].Exit;
                    clsHWLS.FBLOCK.FBlock[bIndex].nEntry = 1;
                    clsHWLS.FBLOCK.FBlock[bIndex].nExit = 1;
                    clsHWLS.FBLOCK.FBlock[bIndex].Node = clsSESE.SESE[workSESE].SESE[i].Node;
                    clsHWLS.FBLOCK.FBlock[bIndex].nNode = clsSESE.SESE[workSESE].SESE[i].nNode;
                    //FBlock[bIndex].parentBlock ??
                    clsHWLS.FBLOCK.FBlock[bIndex].SESE = true;
                    clsHWLS.FBLOCK.FBlock[bIndex].refIndex = i;
                    bIndex++;
                }
            }

            //Copy Loop
            if (workLoop >= 0 && clsLoop.Loop[workLoop].nLoop > 0) {
                copy_Loop(ref clsLoop, workLoop, clsLoop.newTempLoop);
                //modify tempLoop => expand the node
                for (int i = 0; i < clsLoop.Loop[clsLoop.newTempLoop].nLoop; i++)
                {
                    if (clsLoop.Loop[clsLoop.newTempLoop].Loop[i].parentLoop != -1) continue; //only start with the outter loop //Log: Otc28_2020
                    //MAKE A SLIGHT CHANGE HERE ======================================
                    //if (clsLoop.Loop[clsLoop.newTempLoop].Loop[i].depth != 1) continue;
                    int n = 0;
                    int[] temp = GetFullNode(ref graph, currentN, ref clsLoop, clsLoop.newTempLoop, i, ref n); //get full node of Loop (newTempLoop) update this loop and all child (including header)
                    //remove redundancy in Loop[i].Node[];
                    //temp = removeRedundancyNode(temp);
                }
                //copy Loop to FBlock
                for (int i = 0; i < clsLoop.Loop[clsLoop.newTempLoop].nLoop; i++) {
                    //FBlock[bIndex].child = ??
                    //FBlock[bIndex].depth = ??
                    clsHWLS.FBLOCK.FBlock[bIndex].Entry = clsLoop.Loop[clsLoop.newTempLoop].Loop[i].Entry;
                    clsHWLS.FBLOCK.FBlock[bIndex].Exit = clsLoop.Loop[clsLoop.newTempLoop].Loop[i].Exit;
                    clsHWLS.FBLOCK.FBlock[bIndex].nEntry = clsLoop.Loop[clsLoop.newTempLoop].Loop[i].nEntry;
                    clsHWLS.FBLOCK.FBlock[bIndex].nExit = clsLoop.Loop[clsLoop.newTempLoop].Loop[i].nExit;
                    clsHWLS.FBLOCK.FBlock[bIndex].Node = clsLoop.Loop[clsLoop.newTempLoop].Loop[i].Node;
                    clsHWLS.FBLOCK.FBlock[bIndex].nNode = clsLoop.Loop[clsLoop.newTempLoop].Loop[i].nNode;
                    //FBlock[bIndex].parentBlock = ??
                    clsHWLS.FBLOCK.FBlock[bIndex].SESE = false;
                    clsHWLS.FBLOCK.FBlock[bIndex].refIndex = i;
                    bIndex++;
                }
            }

            clsHWLS.FBLOCK.nFBlock = bIndex;

            //remove same block
            check_Block_Same(ref clsHWLS);
            //Find parent block
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                int j = find_nearestParentBlock(ref clsHWLS, i);
                clsHWLS.FBLOCK.FBlock[i].parentBlock = j;
            }
            //find Children
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                int[] child_of_i = new int[clsHWLS.FBLOCK.nFBlock];
                int nChild = 0;
                for (int j = 0; j < clsHWLS.FBLOCK.nFBlock; j++) {
                    if (clsHWLS.FBLOCK.FBlock[j].parentBlock == i) {
                        child_of_i[nChild] = j;
                        nChild++;
                    }
                }
                clsHWLS.FBLOCK.FBlock[i].child = child_of_i;
                clsHWLS.FBLOCK.FBlock[i].nChild = nChild;
            }
            //find Depth       
            clsHWLS.FBLOCK.maxDepth = 0;
            //int maxDepth = 0;
            int curDepth = 0;
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                curDepth = 1;
                //FBLOCK.maxDepth = 1;
                if (clsHWLS.FBLOCK.FBlock[i].parentBlock != -1) continue;
                clsHWLS.FBLOCK.FBlock[i].depth = curDepth;
                find_BlockDepth(ref clsHWLS, i, ref curDepth);
            }
            if (clsHWLS.FBLOCK.nFBlock > 0 && clsHWLS.FBLOCK.maxDepth == 0) clsHWLS.FBLOCK.maxDepth = 1;
        }

        public static void find_BlockDepth(ref GraphVariables.clsHWLS clsHWLS, int i, ref int curDepth)
        {
            if (clsHWLS.FBLOCK.FBlock[i].nChild > 0) {
                for (int j = 0; j < clsHWLS.FBLOCK.FBlock[i].nChild; j++) {
                    curDepth++;
                    if (clsHWLS.FBLOCK.maxDepth < curDepth) clsHWLS.FBLOCK.maxDepth = curDepth;
                    clsHWLS.FBLOCK.FBlock[clsHWLS.FBLOCK.FBlock[i].child[j]].depth = curDepth;
                    find_BlockDepth(ref clsHWLS, clsHWLS.FBLOCK.FBlock[i].child[j], ref curDepth);
                    curDepth--;
                }
            }
        }

        public static void check_Block_Same(ref GraphVariables.clsHWLS clsHWLS)
        {
            bool check = false;
            do {
                check = false;
                for (int i = 0; i < clsHWLS.FBLOCK.nFBlock - 1; i++) {
                    for (int j = i + 1; j < clsHWLS.FBLOCK.nFBlock; j++) {
                        if ((clsHWLS.FBLOCK.FBlock[i].nNode == clsHWLS.FBLOCK.FBlock[j].nNode) &&
                            (check_sameBlock(clsHWLS.FBLOCK.FBlock[i].Node, clsHWLS.FBLOCK.FBlock[i].nNode, clsHWLS.FBLOCK.FBlock[j].Node, clsHWLS.FBLOCK.FBlock[j].nNode)))
                        {
                            //remove FBLOCK.FBlock[j];
                            if (clsHWLS.FBLOCK.FBlock[j].SESE == true)
                            {
                                for (int k = j + 1; k < clsHWLS.FBLOCK.nFBlock; k++)
                                {
                                    clsHWLS.FBLOCK.FBlock[k - 1] = clsHWLS.FBLOCK.FBlock[k];
                                }
                            }
                            else
                            {
                                for (int k = i + 1; k < clsHWLS.FBLOCK.nFBlock; k++)
                                {
                                    clsHWLS.FBLOCK.FBlock[k - 1] = clsHWLS.FBLOCK.FBlock[k];
                                }
                            }
                            clsHWLS.FBLOCK.nFBlock--;
                            check = true;
                            break;
                        }
                    }
                    if (check) break;
                }
            } while (check);
        }

        public static int find_nearestParentBlock(ref GraphVariables.clsHWLS clsHWLS, int i)
        {
            int[] parentCandidates = new int[clsHWLS.FBLOCK.nFBlock];
            int nParentCandidate = 0;
            for (int j = 0; j < clsHWLS.FBLOCK.nFBlock; j++) {
                if ((clsHWLS.FBLOCK.FBlock[i].nNode < clsHWLS.FBLOCK.FBlock[j].nNode) &&
                    (check_sameBlock(clsHWLS.FBLOCK.FBlock[i].Node, clsHWLS.FBLOCK.FBlock[i].nNode, clsHWLS.FBLOCK.FBlock[j].Node, clsHWLS.FBLOCK.FBlock[j].nNode))) {
                    //bool check = false;
                    parentCandidates[nParentCandidate] = j;
                    nParentCandidate++;
                }
            }
            if (nParentCandidate > 0) {
                int min = clsHWLS.FBLOCK.FBlock[parentCandidates[0]].nNode;
                int minIndex = parentCandidates[0];
                for (int j = 0; j < nParentCandidate; j++) {
                    if (min > clsHWLS.FBLOCK.FBlock[parentCandidates[j]].nNode) {
                        min = clsHWLS.FBLOCK.FBlock[parentCandidates[j]].nNode;
                        minIndex = parentCandidates[j];
                    }
                }
                return minIndex;
            }
            else {
                return -1;
            }
        }
        public static bool check_sameBlock(int[] A, int n, int[] B, int m) //n < m
        {
            for (int i = 0; i < n; i++) {
                bool tempcheck = false;
                for (int j = 0; j < m; j++) {
                    if (A[i] == B[j]) {
                        tempcheck = true;
                    }
                }
                if (!tempcheck) return false;
            }
            return true;
        }

        //recursive update Full node of loop[i] and all its child loop[j,k,l]
        public static int[] GetFullNode(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsLoop clsLoop, int workLoop, int i, ref int n)
        {
            int depth = clsLoop.Loop[workLoop].maxDepth;
            int loop = -1;
            //bool check = false;
            int[] tempArr = new int[graph.Network[currentN].nNode];
            int nTempArr = 0;
            int[] tmpLoop = new int[graph.Network[currentN].nNode];
            //1 header
            // for each node of this loop => if node is header => find again; else => add to arrNode
            for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nNode; k++) {
                tmpLoop = new int[graph.Network[currentN].nNode];
                n = 0;
                if (check_header(ref clsLoop, workLoop, i, k, ref loop)) {
                    tmpLoop = GetFullNode(ref graph, currentN, ref clsLoop, workLoop, loop, ref n);
                    addArr(tmpLoop, ref tempArr, n, ref nTempArr);
                }
                else {
                    tempArr[nTempArr] = clsLoop.Loop[workLoop].Loop[i].Node[k];
                    nTempArr++;
                }
            }
            
            tempArr[nTempArr] = clsLoop.Loop[workLoop].Loop[i].header;
            nTempArr++;
            clsLoop.Loop[workLoop].Loop[i].nNode = nTempArr;
            clsLoop.Loop[workLoop].Loop[i].Node = tempArr;
            n = nTempArr;
            return tempArr;
        }
        public static void addArr(int[] tmpLoop, ref int[] tempArr, int n, ref int nTempArr)
        {
            for (int i = 0; i < n; i++) {
                tempArr[nTempArr] = tmpLoop[i];
                nTempArr++;
            }
        }
        public static bool check_header(ref GraphVariables.clsLoop clsLoop, int currentLoop, int i, int k, ref int loop)
        {
            for (int j = 0; j < clsLoop.Loop[currentLoop].Loop[i].nChild; j++) {
                if (clsLoop.Loop[currentLoop].Loop[i].Node[k] == clsLoop.Loop[currentLoop].Loop[clsLoop.Loop[currentLoop].Loop[i].child[j]].header) {
                    loop = clsLoop.Loop[currentLoop].Loop[i].child[j];
                    return true;
                }
            }
            return false;
        }

        private static int[] removeRedundancyNode(int[] TempArr)
        {
            int[] temp = new int[TempArr.Length];
            int nTemp = 0;

            for (int i = 0; i < TempArr.Length; i++)
            {
                if (!gProAnalyzer.Ultilities.checkGraph.Node_In_Set(temp, 0, TempArr[i]))
                {
                    temp[nTemp] = TempArr[i];
                    nTemp++;
                }
            }
            int[] temp2 = new int[nTemp];
            for (int i = 0; i < nTemp; i++) temp2[i] = temp[i];
            return temp2;
        }

        private static void copy_Loop(ref GraphVariables.clsLoop clsLoop, int fromLoop, int toLoop)
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
            }

        }
    }
}
