using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class clsFindNodeInfo
    {
        public static void find_NodeInfo(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int node)
        {
            int cntPre = 0;
            int[] find_Pre = new int[graph.Network[currentN].nNode];
            int cntPost = 0;
            int[] find_Post = new int[graph.Network[currentN].nNode];

            graph.Network[currentN].Node[node].nPre = 0;
            graph.Network[currentN].Node[node].nPost = 0;
            graph.Network[currentN].Node[node].Pre = null;
            graph.Network[currentN].Node[node].Post = null;

            for (int j = 0; j < graph.Network[currentN].nLink; j++)
            {
                if (graph.Network[currentN].Link[j].fromNode == node && graph.Network[currentN].Link[j].toNode != node)
                {
                    find_Post[cntPost] = graph.Network[currentN].Link[j].toNode;
                    cntPost++;
                }
                if (graph.Network[currentN].Link[j].toNode == node && graph.Network[currentN].Link[j].fromNode != node)
                {
                    find_Pre[cntPre] = graph.Network[currentN].Link[j].fromNode;
                    cntPre++;
                }
            }

            if (cntPre > 0)
            {
                graph.Network[currentN].Node[node].nPre = cntPre;
                graph.Network[currentN].Node[node].Pre = new int[cntPre];
                for (int k = 0; k < cntPre; k++) graph.Network[currentN].Node[node].Pre[k] = find_Pre[k];
            }

            if (cntPost > 0)
            {
                graph.Network[currentN].Node[node].nPost = cntPost;
                graph.Network[currentN].Node[node].Post = new int[cntPost];
                for (int k = 0; k < cntPost; k++) graph.Network[currentN].Node[node].Post[k] = find_Post[k];
            }
            if (cntPre == 0 && cntPost > 0) graph.Network[currentN].header = node;
        }
    }
}
