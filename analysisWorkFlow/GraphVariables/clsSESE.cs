using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    class clsSESE
    {
        public int orgSESE, reduceSESE, finalSESE, tempSESE, untangleSESE;
        public gProAnalyzer.GraphVariables.clsSESE.strSESE[] SESE;
        public clsSESE()
        {
            orgSESE = 0;
            reduceSESE = 1;
            finalSESE = 2;
            tempSESE = 3;

            untangleSESE = 4;

            SESE = new gProAnalyzer.GraphVariables.clsSESE.strSESE[5];
        }

        public struct strSESEInform
        {
            public int depth; //loop 계층 -> 1부터
            public string type;

            public int parentSESE; //부모 loop - 없으면(최상위계층) -1;
            public int nChild; // Child Loop 수
            public int[] child; // Child Loop

            public int Entry; //Entry node
            public int Exit; //Exit node

            public int nNode; //포함 Node수
            public int[] Node; //Node            
        }
        public struct strSESE
        {
            public int nSESE; //SESE 갯수
            public strSESEInform[] SESE;

            public int maxDepth; //loop 최대 계층
        }
    }
}
