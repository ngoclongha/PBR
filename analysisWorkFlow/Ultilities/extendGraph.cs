using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class extendGraph
    {
        public static void full_extentNetwork(ref GraphVariables.clsGraph graph, int currentN, int add_nNode, int add_nLink)
        {
            //AddNum is the number of node are added
            //A full Graph (extended)
            GraphVariables.clsGraph.strGraph saveNetwork = graph.Network[currentN];

            graph.Network[currentN] = new GraphVariables.clsGraph.strGraph();

            graph.Network[currentN].header = saveNetwork.header;

            graph.Network[currentN].nNode = saveNetwork.nNode + add_nNode;
            graph.Network[currentN].Node = new GraphVariables.clsNode.strNode[graph.Network[currentN].nNode];
            for (int i = 0; i < saveNetwork.nNode; i++) {
                //Network[currentN].Node[i] = saveNetwork.Node[i];

                graph.Network[currentN].Node[i].Kind = saveNetwork.Node[i].Kind;
                graph.Network[currentN].Node[i].Name = saveNetwork.Node[i].Name;
                graph.Network[currentN].Node[i].orgNum = saveNetwork.Node[i].orgNum;
                graph.Network[currentN].Node[i].parentNum = saveNetwork.Node[i].parentNum;
                graph.Network[currentN].Node[i].Type_I = saveNetwork.Node[i].Type_I;
                graph.Network[currentN].Node[i].Type_II = saveNetwork.Node[i].Type_II;
                graph.Network[currentN].Node[i].Special = saveNetwork.Node[i].Special;
                graph.Network[currentN].Node[i].nodeLabel = saveNetwork.Node[i].nodeLabel;
                graph.Network[currentN].Node[i].SOS_Corrected = saveNetwork.Node[i].SOS_Corrected;

                graph.Network[currentN].Node[i].depth = saveNetwork.Node[i].depth;
                graph.Network[currentN].Node[i].done = saveNetwork.Node[i].done;

                graph.Network[currentN].Node[i].nPre = saveNetwork.Node[i].nPre;
                graph.Network[currentN].Node[i].Pre = new int[graph.Network[currentN].Node[i].nPre];
                for (int k = 0; k < graph.Network[currentN].Node[i].nPre; k++)
                    graph.Network[currentN].Node[i].Pre[k] = saveNetwork.Node[i].Pre[k];

                graph.Network[currentN].Node[i].nPost = saveNetwork.Node[i].nPost;
                graph.Network[currentN].Node[i].Post = new int[graph.Network[currentN].Node[i].nPost];
                for (int k = 0; k < graph.Network[currentN].Node[i].nPost; k++)
                    graph.Network[currentN].Node[i].Post[k] = saveNetwork.Node[i].Post[k];

                graph.Network[currentN].Node[i].nDom = saveNetwork.Node[i].nDom;
                graph.Network[currentN].Node[i].Dom = new int[graph.Network[currentN].Node[i].nDom];
                for (int k = 0; k < graph.Network[currentN].Node[i].nDom; k++)
                    graph.Network[currentN].Node[i].Dom[k] = saveNetwork.Node[i].Dom[k];

                graph.Network[currentN].Node[i].nDomRev = saveNetwork.Node[i].nDomRev;
                graph.Network[currentN].Node[i].DomRev = new int[graph.Network[currentN].Node[i].nDomRev];
                for (int k = 0; k < graph.Network[currentN].Node[i].nDomRev; k++)
                    graph.Network[currentN].Node[i].DomRev[k] = saveNetwork.Node[i].DomRev[k];

                graph.Network[currentN].Node[i].nDomEI = saveNetwork.Node[i].nDomEI;
                graph.Network[currentN].Node[i].DomEI = new int[graph.Network[currentN].Node[i].nDomEI];
                for (int k = 0; k < graph.Network[currentN].Node[i].nDomEI; k++)
                    graph.Network[currentN].Node[i].DomEI[k] = saveNetwork.Node[i].DomEI[k];

                graph.Network[currentN].Node[i].nDomRevEI = saveNetwork.Node[i].nDomRevEI;
                graph.Network[currentN].Node[i].DomRevEI = new int[graph.Network[currentN].Node[i].nDomRevEI];
                for (int k = 0; k < graph.Network[currentN].Node[i].nDomRevEI; k++)
                    graph.Network[currentN].Node[i].DomRevEI[k] = saveNetwork.Node[i].DomRevEI[k];

                graph.Network[currentN].Node[i].nodeLabel = saveNetwork.Node[i].nodeLabel;
                graph.Network[currentN].Node[i].header = saveNetwork.Node[i].header;
                graph.Network[currentN].Node[i].headerOfLoop = saveNetwork.Node[i].headerOfLoop;
            }

            graph.Network[currentN].nLink = saveNetwork.nLink + add_nLink;
            graph.Network[currentN].Link = new GraphVariables.clsEdge.strEdge[graph.Network[currentN].nLink];
            for (int i = 0; i < saveNetwork.nLink; i++) {
                //Network[currentN].Link[i] = saveNetwork.Link[i];

                graph.Network[currentN].Link[i].fromNode = saveNetwork.Link[i].fromNode;
                graph.Network[currentN].Link[i].toNode = saveNetwork.Link[i].toNode;
                graph.Network[currentN].Link[i].bBackJ = saveNetwork.Link[i].bBackJ;
                graph.Network[currentN].Link[i].bBackS = saveNetwork.Link[i].bBackS;
                graph.Network[currentN].Link[i].bInstance = saveNetwork.Link[i].bInstance;
            }
        }
    }
}
