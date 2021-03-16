using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    public class clsGraph
    {
        public int baseNet, orgNet, midNet, finalNet, reduceNet, nickNet, subNet, 
            dummyNet, conNet, seseNet, redSeNet, acyclicNet, tempNet, DFlow_PdFlow, iDFlow_PdFlow, theRestFlow, reduceTempNet, reduceTempNet2, untangleNet;
        public bool check_untangle;
        public gProAnalyzer.GraphVariables.clsGraph.strGraph[] Network;
        public clsGraph()
        {
            baseNet = 0; //원 Network
            orgNet = 1; //Original Network
            midNet = 2; // Network
            finalNet = 3; //최종 전체 Network


            reduceNet = 4; // Loop축소후 Network
            nickNet = 5;
            subNet = 6; // 부분  Network
            dummyNet = 7;
            conNet = 8; // Concurency Check시 사용

            seseNet = 9; //SESE Network
            redSeNet = 10; //  SESE 축소후 Network
            acyclicNet = 11; //마지막 acyclic Network

            tempNet = 12; //New code
            DFlow_PdFlow = 13;
            iDFlow_PdFlow = 14;
            theRestFlow = 15;
            reduceTempNet = 16;
            reduceTempNet2 = 17;

            untangleNet = 18;
            
            Network = new gProAnalyzer.GraphVariables.clsGraph.strGraph[19];

            check_untangle = false;
        }

        public struct strGraph
        {
            public int header; //starting Node            

            public int nNode;
            public gProAnalyzer.GraphVariables.clsNode.strNode[] Node;

            public int nLink;
            public gProAnalyzer.GraphVariables.clsEdge.strEdge[] Link;
        }
    }
}
