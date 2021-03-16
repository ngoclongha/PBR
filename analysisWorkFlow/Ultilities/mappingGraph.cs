using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class mappingGraph
    {
        public static void to_adjList(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int[][] adjList) //must use the last index of each List ( <= N)
        {
            int nNode = graph.Network[currentN].nNode;
            int nLink = graph.Network[currentN].nLink;
            adjList = new int[nNode][];
            //initiate each List
            for (int i = 0; i < nNode; i++)
            {
                adjList[i] = new int[nNode];
                adjList[i][0] = 0;
            }
            int fromNode, toNode;
            for (int i = 0; i < nLink; i++)
            {
                fromNode = graph.Network[currentN].Link[i].fromNode;
                toNode = graph.Network[currentN].Link[i].toNode;

                adjList[fromNode][0]++;
                adjList[fromNode][adjList[fromNode][0]] = toNode;

                adjList[toNode][0]++;
                adjList[toNode][adjList[toNode][0]] = fromNode;
            }
        }
        public static void to_adjList_Directed(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int[][] adjList) //must use the last index of each List ( <= N)
        {
            int nNode = graph.Network[currentN].nNode;
            int nLink = graph.Network[currentN].nLink;
            adjList = new int[nNode][];
            //initiate each List
            for (int i = 0; i < nNode; i++)
            {
                adjList[i] = new int[nNode];
                adjList[i][0] = 0;
            }
            int fromNode, toNode;
            for (int i = 0; i < nLink; i++)
            {
                fromNode = graph.Network[currentN].Link[i].fromNode;
                toNode = graph.Network[currentN].Link[i].toNode;
                adjList[fromNode][0]++;
                adjList[fromNode][adjList[fromNode][0]] = toNode;
                //adjList[toNode][0]++;
                //adjList[toNode][adjList[toNode][0]] = fromNode;
            }
        }

        public static void to_adjList_bInstance(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int[][] adjList) //must use the last index of each List ( <= N)
        {
            int nNode = graph.Network[currentN].nNode;
            int nLink = graph.Network[currentN].nLink;
            adjList = new int[nNode][];
            //initiate each List
            for (int i = 0; i < nNode; i++)
            {
                adjList[i] = new int[nNode];
                adjList[i][0] = 0;
            }
            int fromNode, toNode;
            for (int i = 0; i < nLink; i++)
            {
                if (graph.Network[currentN].Link[i].bInstance == false) continue;

                fromNode = graph.Network[currentN].Link[i].fromNode;
                toNode = graph.Network[currentN].Link[i].toNode;
                adjList[fromNode][0]++;
                adjList[fromNode][adjList[fromNode][0]] = toNode;

                //adjList[toNode][0]++;
                //adjList[toNode][adjList[toNode][0]] = fromNode;
            }
        }

        public static void to_adjacencyMatrix(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int[,] A, ref int nNode)
        {
            nNode = graph.Network[currentN].nNode;
            //initiate adjacency matrix
            A = new int[nNode, nNode];
            for (int i = 0; i < nNode; i++)
                for (int j = 0; j < nNode; j++)
                    A[i, j] = -1;

            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                A[graph.Network[currentN].Link[i].fromNode, graph.Network[currentN].Link[i].toNode] = 1;
            }
        }

        public static void to_adjacencyMatrix_Undirected(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref int[,] A, ref int nNode)
        {
            nNode = graph.Network[currentN].nNode;
            //initiate adjacency matrix
            A = new int[nNode, nNode];
            for (int i = 0; i < nNode; i++)
                for (int j = 0; j < nNode; j++)
                    A[i, j] = -1;

            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                A[graph.Network[currentN].Link[i].fromNode, graph.Network[currentN].Link[i].toNode] = 1;
                A[graph.Network[currentN].Link[i].toNode, graph.Network[currentN].Link[i].fromNode] = 1;
            }
        }
    }
}
