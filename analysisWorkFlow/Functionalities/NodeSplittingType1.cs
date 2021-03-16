using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Functionalities
{
    class NodeSplittingType1
    {
        //Make A Gateways in to Single Exit, Single Entry (1 into 2 nodes)
        //Using midNet to store the results
        //This is Global Variables
        public static int nSearchNode = 0;
        public static int[] searchNode;
        private gProAnalyzer.Ultilities.clsFindNodeInfo fninfo;
        private gProAnalyzer.Preprocessing.clsExtendNetwork extNetwork;

        public static void Initialize_All()
        {
            //fninfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
            //extNetwork = new gProAnalyzer.Preprocessing.clsExtendNetwork();
        }

        public static void Run_Split_Type1(ref gProAnalyzer.GraphVariables.clsGraph graph, int orgNet, int midNet)
        {
            Initialize_All();

            // Network(0)로 작업하여 Network(1) 생성

            int nNode = graph.Network[orgNet].nNode;
            int nLink = graph.Network[orgNet].nLink;

            searchNode = new int[nNode];
            nSearchNode = 0;

            //nPre ~ Predecessor; nPost ~ Sucessor
            for (int i = 0; i < nNode; i++)
            {
                if (graph.Network[orgNet].Node[i].nPre >= 2 && graph.Network[orgNet].Node[i].nPost >= 2)
                {
                    searchNode[nSearchNode] = i;
                    nSearchNode++;
                }
            }

            // 새네트워크 생성 (복제)
            graph.Network[midNet] = graph.Network[orgNet];
            //midNet => save the extent_Network. Create more node for split
            //just extent the number (nSearchNode) of Node and Link with null value
            gProAnalyzer.Preprocessing.clsExtendNetwork.extent_Network(ref graph, midNet, nSearchNode);

            if (nSearchNode > 0)
            {
                Type_I_Split(ref graph, midNet, nNode, nLink);
            }
        }

        public static void Type_I_Split(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int nNode, int nLink)
        {            
            for (int i = 0; i < nSearchNode; i++)
            {
                //For example searchNode{5, 6, 7, 10} (4 nodes) => ( [0] = 5, [1] = 6) => sNode = 4 + i => 4, 5, 6, 7
                int jNode = searchNode[i];
                int sNode = nNode + i; //node i ~ join Node => node s ~ split Node = n + i

                //Split Node - 추가
                graph.Network[currentN].Node[sNode].Kind = graph.Network[currentN].Node[jNode].Kind;
                graph.Network[currentN].Node[sNode].Name = graph.Network[currentN].Node[jNode].Name;// jNode.ToString();
                graph.Network[currentN].Node[sNode].orgNum = jNode;
                graph.Network[currentN].Node[sNode].parentNum = sNode;
                graph.Network[currentN].Node[sNode].Type_I = "_s";
                graph.Network[currentN].Node[sNode].Special = "";
                //Join Node - 변경
                graph.Network[currentN].Node[jNode].Type_I = "_j";

                //New Link 추가
                graph.Network[currentN].Link[nLink + i].fromNode = jNode;
                graph.Network[currentN].Link[nLink + i].toNode = sNode;

                //기존 Link 정보 변경
                for (int j = 0; j < nLink; j++)
                {
                    if (graph.Network[currentN].Link[j].fromNode == jNode)
                    {
                        graph.Network[currentN].Link[j].fromNode = sNode;
                        //Find all the rest of information of a Node. Such as => Predecessors, Sucessor                        
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[j].toNode);
                    }
                }
                //Find all the rest of information of a Node. Such as => Predecessors, Sucessor
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, jNode);
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, sNode);
            }

        }

    }
}
