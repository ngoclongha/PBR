using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    class clsLoopDAG
    {
        public int orgLoopDAG, reduceLoopDAG, finalLoopDAG, tempLoopDAG;
        public gProAnalyzer.GraphVariables.clsLoopDAG.strLoopDAG[] loopDAG;
        public clsLoopDAG()
        {
            orgLoopDAG = 0;
            reduceLoopDAG = 1;
            finalLoopDAG = 2;
            tempLoopDAG = 3;
            loopDAG = new gProAnalyzer.GraphVariables.clsLoopDAG.strLoopDAG[4];
        }

        public struct strLoopDAGInfo
        {
            public GraphVariables.clsGraph.strGraph DAG;

            public int depth; //depth of Loop.
            public int parentSESE; //It alway has a parent
            public string type; // "fwd" ; "bwd" ; "df" ; "icid_cipd" "cipd_icid"

            public int nChild; // Child structure (SESE or Loop) in ePST
            public int[] child; // index in DAPST

            public int Entry; //Entry node
            public int Exit; //Exit node

            public int nNode; //Node member (including its Entry, Exit)
            public bool[] isVirtualNode;
            public int[] Node; //Node index in clsGraph
        }
        public struct strLoopDAG
        {
            public int nLoopDAG; //
            public strLoopDAGInfo[] loopDAG;            
        }
    }
}
