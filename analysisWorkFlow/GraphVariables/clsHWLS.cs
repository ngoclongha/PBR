using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * //The hierarchy of a workflow graph refined by loops and SESE regions (HWLS) 
 * 
 * The name came from Pauline Thesis (2015)
 * 
 */

namespace gProAnalyzer.GraphVariables
{
    class clsHWLS
    {
        public struct strFBlock
        {
            public int[] child;
            public int nChild;
            public int depth;
            public string type;

            public int[] Entry;
            public int[] Exit;

            public int nEntry;
            public int nExit;

            public int[] Node;
            public int nNode;

            public int parentBlock;
            public bool SESE;
            public int refIndex;

            public int CIPd;

            //Behavior relation for the header of BLOCK (Header is: SESE_Entry; NL_Entry; IL_CIPd)
            public int[,] Dom_BhPrfl; //Dominator behavior profile between the current header and its childs.
            public int nDomBh;

            public int[,] Pdom_BhPrfl; //Could be NULL if BLOCK is SESE
            public int nPdomBh;

            public bool isProcessed; //check a SESE (e.g., rigid) is actual compute by instance subgraph or not? // Computation time issues
        }
        public struct STRBLOCK
        {
            public strFBlock[] FBlock;
            public int nFBlock;
            public int maxDepth;
        }
        
        public STRBLOCK FBLOCK;
    }
}