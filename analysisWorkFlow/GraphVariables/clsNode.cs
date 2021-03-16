using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    public class clsNode
    {
        public struct strNode
        {
            public string Kind; //Node 종류  S:start E: end T:Task AND:And  XOR:OR

            public string Name; // Node명

            public int orgNum; //원 노드번호 - Split시 사용 -- SubNetwork경우 Parent Network 번호
            public int parentNum; //Original Network 노드번호 - Error 점검시 사용

            public string Type_I; // "" or j(join)  or s (split) c (Con_AND)
            public string Type_II; // "" or fj(forward join)  or bj(backward join) or fs(forward split) or bs(backward split)

            public string Special; //special Node??   E : Entry,  X : Exit, B : BackSplit, T ; Exit & BS

            public int nPre; //선 Node수 //Number of node which were Predecessor of current Node
            public int[] Pre; //선 Node
            public int nPost; //후 Node수 //Number of node which were Successor of current Node
            public int[] Post; //후 Node

            public int nDom; // dominator 수
            public int[] Dom; //dominator 집합 // d[n] a set of node dom this node
            public int nDomRev; // 역dominator 수 // post dominator
            public int[] DomRev; //역dominator 집합 // set of node d[n]

            public int nDomEI; // dominator 수
            public int[] DomEI; //dominator 집합
            public int nDomRevEI; // 역dominator 수
            public int[] DomRevEI; //역dominator 집합

            public int nDomInverse;
            public int[] DomInverse;
            public int nDomRevInverse;
            public int[] DomRevInverse;

            public int[] DF;
            public int nDF;
            public int[] PdF;
            public int nPdF;

            public int depth; //1부터 //depth of this node
            public bool done; //loop축소시 사용하지 않는 노드 //Maybe used for mark this node is use for reduce network ("true" for the node which is reduce, "false" in reversed)

            public int[,] conEntry; //New code
            public int nConEntry; //New code

            public int DepthDom; //New code
            public int DepthPdom; //New code

            public bool SOS_Corrected; //mark the SOS is corrected or NOT // SOS_Corrected = true => corrected, SOS_Corrected = false => not corrected.

            public string nodeLabel;
            public bool header; //Using for preprocessing
            public int headerOfLoop; //Using for preprocessing

            //public int[][] behaviorProfile; //omit first index
        }
    }
}
