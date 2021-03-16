using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class searchGraph
    {
        public static int get_depthBFS(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, bool[,] Tree, bool isEntrySet)
        {
            int Start = find_nodeName(ref graph, currentN, "START");
            int End = find_nodeName(ref graph, currentN, "END");

            Queue<int> Q = new Queue<int>();
            if (isEntrySet)
            {
                Q.Enqueue(graph.Network[currentN].Node[Start].Post[0]);
                graph.Network[currentN].Node[Start].DepthDom = 0;
                graph.Network[currentN].Node[Q.Peek()].DepthDom = 1;
            }
            else
            {
                Q.Enqueue(graph.Network[currentN].Node[End].Pre[0]);
                graph.Network[currentN].Node[End].DepthPdom = 0;
                graph.Network[currentN].Node[Q.Peek()].DepthPdom = 1;
            }
            int maxDepth = 0;
            do
            {
                int u = Q.Dequeue();
                for (int v = 0; v < graph.Network[currentN].nNode; v++)
                {
                    if (isEntrySet && Tree[u, v] == true)
                    {
                        graph.Network[currentN].Node[v].DepthDom = graph.Network[currentN].Node[u].DepthDom + 1;
                        if (graph.Network[currentN].Node[v].DepthDom > maxDepth) maxDepth = graph.Network[currentN].Node[v].DepthDom;
                        Q.Enqueue(v);
                    }
                    else if (!isEntrySet && Tree[v, u] == true)
                    {
                        graph.Network[currentN].Node[v].DepthPdom = graph.Network[currentN].Node[u].DepthPdom + 1;
                        if (graph.Network[currentN].Node[v].DepthPdom > maxDepth) maxDepth = graph.Network[currentN].Node[v].DepthPdom;
                        Q.Enqueue(v);
                    }
                }
            } while (Q.Count != 0);
            return maxDepth;
        }
        public static int find_nodeName(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, string name)
        {
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Kind == name)
                    return i;
            }
            return -1;
        }
    }
}
