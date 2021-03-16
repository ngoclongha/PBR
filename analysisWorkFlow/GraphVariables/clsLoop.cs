using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    class clsLoop
    {
        public int orgLoop, subLoop, reduceLoop, tempLoop, newTempLoop, untangleLoop, recur_Loop1, recur_Loop2;
        public bool IrreducibleError;
        public gProAnalyzer.GraphVariables.clsLoop.strLoop[] Loop;

        public clsLoop()
        {
            orgLoop = 0;
            subLoop = 1;
            reduceLoop = 2;
            tempLoop = 3;
            newTempLoop = 4;

            untangleLoop = 5;

            recur_Loop1 = 6;
            recur_Loop2 = 7;
            
            Loop = new gProAnalyzer.GraphVariables.clsLoop.strLoop[8];
        }

        public struct strLoopInform
        {
            public bool Irreducible; //True 면 irreducible Loop, False면 Natural loop
            public int depth; //loop 계층 - 1부터

            public int parentLoop; //부모 loop - 없으면(최상위계층) -1;
            public int nChild; // Child Loop 수
            public int[] child; // Child Loop

            public int header; //header

            public int nBackEdge; // BackEdge수
            public int[] linkBack; // BackEdge

            public int nBackSplit; //BackSplit수
            public int[] BackSplit; //BackSplit node

            public int nEntry; //Entry수
            public int[] Entry; //Entry node

            public int nExit; //Exit수
            public int[] Exit; //Exit node

            public int nNode; //포함 Node수
            public int[] Node; //Node
            
            //=====================
            public int nConcurrency; //Concurrency수
            public int[] Concurrency; //Concurrency 구분번호 0:없음 1,2,3...

            public int[] nConcurrInst; //share index with nConcurrency; //= number of node in each concurrencyCombination from CID to current concurrencySet
            public int[][] concurrInst;

            public int[] nExclusiveInst; //using index of ENTRY[] => storing instance node from CID to each exclusive entry.
            public int[][] exclusiveInst; 
            //=====================

            public int nConEntry; //New code
            public int[,] conEntry; //New code

            public int[] etnCandEn; //external candidate entries; //new code
            public int nEtnCandEn;
            public int[] etnCandEx;
            public int nEtnCandEx;

            //==================
            public int loopType; //[1] 1en, 1ex&bs; [2] 1en, >2exit; [3] ex&bs bs; [4] >2ex, bs, ex&bs.
        }

        public struct strLoop
        {
            public int nLoop; //loop 갯수
            public strLoopInform[] Loop;

            public int maxDepth; //loop 최대 계층
        }
    }
}
