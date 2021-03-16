using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    class clsDAPST
    {
        public int baseNet, orgNet, midNet, finalNet, reduceNet, nickNet, subNet, 
            dummyNet, conNet, seseNet, redSeNet, acyclicNet, tempNet, DFlow_PdFlow, iDFlow_PdFlow, theRestFlow, reduceTempNet, reduceTempNet2;        

        //public gProAnalyzer.GraphVariables.clsGraph.strGraph DAG;
        public clsDAPST()
        {
            //DAG = new gProAnalyzer.GraphVariables.clsGraph.strGraph();
        }

        public struct strDAPSTinfo
        {
            gProAnalyzer.GraphVariables.clsGraph.strGraph DAG;            

            // basic info
            public int[] Node;
            public int nNode;

            public int parentDAG; 
           
            public int depthDAG;
            public int nChildDAG;
            public int[] childDAG;

            public int typeDAG; // "fwd" ; "bwd" ; "df" ; "icid_cipd" "cipd_icid" / "sese" ; "nl" ; "il"
            public bool isSESE;
            public bool isLoop;
            public bool isDAG;
            public int orgIndex;

            public int[] Entry;
            public int[] Exit;
            public int nEntry;
            public int nExit;
        }


        public struct strDAPST
        {
            public int nDAPST;
            public int maxDepth;
            public strDAPSTinfo[] DAPST;
        }
    }
}
