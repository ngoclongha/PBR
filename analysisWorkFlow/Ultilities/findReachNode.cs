using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class findReachNode
    {
        //노드 검색 결과 // Node Result
        //public static int nSearchNode;
        //public static int[] searchNode;
        //Irreducible Check
        public static int nSearchNode_P, nSearchNode_F, nSearchNode_B;
        public static int[] searchNode_P, searchNode_F, searchNode_B;
        //Concurrent Check
        private static int nReachNode;
        private static int[] reachNode;

        //public gProAnalyzer.Ultilities.findNodeInLoop fndNodeLoop;

        private static void Initialize_All()
        {
            //fndNodeLoop = new gProAnalyzer.Ultilities.findNodeInLoop();
        }

        public static void find_Reach(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int loop, int sNode, int toNode, string Type, ref int[] searchNode, ref int nSearchNode) //check can reach from "sNode" to "toNode" //Type = "CC" or ...
        {
            Initialize_All();
            if (sNode == toNode) return; //Start node = end path Node

            for (int i = 0; i < graph.Network[currentN].Node[sNode].nPost; i++) //Visit all potential path from sNode (Start)
            {
                /////////////////////////////////
                bool bEnd = false;

                if (Type == "CC") //sNode for this case is Header (searchNode[0])
                {
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++) //Check nut lien ke tu sNode (header) toi entries dc hay ko (Duong di truc tiep)
                    {
                        if (graph.Network[currentN].Node[sNode].Post[i] == clsLoop.Loop[workLoop].Loop[loop].Entry[k])
                        {
                            bEnd = true;
                            break;
                        }
                    }
                }

                if (bEnd) continue;
                ////////////////////////////////////////

                int fromNode = graph.Network[currentN].Node[sNode].Post[i]; //begining with a single path

                nReachNode = 0;
                reachNode = new int[graph.Network[currentN].nNode]; //initiate reachNode by a array with scale of nNode (it will store the node, in "sequentially" - just guess)
                reachNode[nReachNode] = fromNode; //Begining with the sucessor of start (sNode)
                nReachNode++;

                if (can_Reach(ref graph, currentN, ref clsLoop, workLoop, loop, fromNode, toNode, Type))
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == fromNode)
                        {
                            bSame = true;
                            break;
                        }
                    }

                    if (!bSame)
                    {
                        searchNode[nSearchNode] = fromNode;
                        nSearchNode++;
                    }

                    find_Reach(ref graph, currentN, ref clsLoop, workLoop, loop, fromNode, toNode, Type, ref searchNode, ref nSearchNode);
                }

            }
        }

        //New one for the case there are loops in the DFlow ============================================
        public static void find_Reach_2(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int loop, int sNode, int toNode, string Type, ref int[] searchNode, ref int nSearchNode) //it OK -> just change the Can_Reach_2() function
        {
            Initialize_All();

            if (sNode == toNode) return; //Start node = end path Node
            for (int i = 0; i < graph.Network[currentN].Node[sNode].nPost; i++) //Visit all potential path from sNode (Start)
            {
                /////////////////////////////////
                bool bEnd = false;

                if (Type == "CC") //sNode for this case is Header (searchNode[0])
                {
                    for (int k = 0; k < clsLoop.Loop[workLoop].Loop[loop].nEntry; k++) //Check nut lien ke tu sNode (header) toi entries dc hay ko (Duong di truc tiep)
                    {
                        if (graph.Network[currentN].Node[sNode].Post[i] == clsLoop.Loop[workLoop].Loop[loop].Entry[k])
                        {
                            bEnd = true;
                            break;
                        }
                    }
                }
                if (bEnd) continue;
                ////////////////////////////////////////

                int fromNode = graph.Network[currentN].Node[sNode].Post[i]; //begining with a single path

                nReachNode = 0;
                //initiate reachNode by a array with scale of nNode (it will store the node, in "sequentially" - just guess)
                reachNode = new int[graph.Network[currentN].nNode]; 
                reachNode[nReachNode] = fromNode; //Begining with the sucessor of start (sNode)
                nReachNode++;
                bool[] mark_reach = new bool[graph.Network[currentN].nNode*2];

                if (can_Reach_2(ref graph, currentN, ref clsLoop, workLoop, loop, fromNode, toNode, Type, ref mark_reach))
                {
                    bool bSame = false;
                    for (int j = 0; j < nSearchNode; j++)
                    {
                        if (searchNode[j] == fromNode)
                        {
                            bSame = true;
                            break;
                        }
                    }
                    if (!bSame)
                    {
                        searchNode[nSearchNode] = fromNode;
                        nSearchNode++;
                    }
                    find_Reach_2(ref graph, currentN, ref clsLoop, workLoop, loop, fromNode, toNode, Type, ref searchNode, ref nSearchNode);
                }

            }
        }
        public static bool can_Reach_2(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop,
            int workLoop, int loop, int fromNode, int toNode, string Type, ref bool[] mark_reach)
        {
            bool bReach = false;
            if (graph.Network[currentN].Node[fromNode].nPost == 0) return bReach;
            mark_reach[fromNode] = true;

            for (int i = 0; i < graph.Network[currentN].Node[fromNode].nPost; i++)
            {
                bool bSame = false;
                for (int j = 0; j < nReachNode; j++)
                {
                    if (graph.Network[currentN].Node[fromNode].Post[i] == reachNode[j])
                    {
                        bSame = true;
                        break;
                    }
                }
                if (bSame) continue;

                reachNode[nReachNode] = graph.Network[currentN].Node[fromNode].Post[i];
                nReachNode++;

                if (graph.Network[currentN].Node[fromNode].Post[i] == toNode)
                {
                    bReach = true;
                    break;
                }
                else if (Type == "CC")
                {
                    if (!gProAnalyzer.Ultilities.findNodeInLoop.Node_In_Loop(ref clsLoop, workLoop, graph.Network[currentN].Node[fromNode].Post[i], loop))  //???????????
                    {
                        if (mark_reach[graph.Network[currentN].Node[fromNode].Post[i]]) continue;
                        if (can_Reach_2(ref graph, currentN, ref clsLoop, workLoop, loop, graph.Network[currentN].Node[fromNode].Post[i], toNode, Type, ref mark_reach))
                        {
                            bReach = true;
                            //break;
                        }
                    }
                }
                else //we don't need it =>
                {
                    if (can_Reach_2(ref graph, currentN, ref clsLoop, workLoop, loop, graph.Network[currentN].Node[fromNode].Post[i], toNode, Type, ref mark_reach))
                    {
                        bReach = true;
                        break;
                    }
                }
                nReachNode--; ;
            }
            mark_reach[fromNode] = false;

            return bReach;
        }
        //================================================================================================================================

        public static bool can_Reach(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, 
            int workLoop, int loop, int fromNode, int toNode, string Type)
        {
            bool bReach = false;
            if (graph.Network[currentN].Node[fromNode].nPost == 0) return bReach;

            for (int i = 0; i < graph.Network[currentN].Node[fromNode].nPost; i++)
            {
                bool bSame = false;
                for (int j = 0; j < nReachNode; j++)
                {
                    if (graph.Network[currentN].Node[fromNode].Post[i] == reachNode[j])
                    {
                        bSame = true;
                        break;
                    }
                }
                if (bSame) continue;

                reachNode[nReachNode] = graph.Network[currentN].Node[fromNode].Post[i];
                nReachNode++;

                if (graph.Network[currentN].Node[fromNode].Post[i] == toNode)
                {
                    bReach = true;
                    break;
                }
                else if (Type == "CC")
                {
                    if (!gProAnalyzer.Ultilities.findNodeInLoop.Node_In_Loop(ref clsLoop, workLoop, graph.Network[currentN].Node[fromNode].Post[i], loop))  //???????????
                    {
                        if (can_Reach(ref graph, currentN, ref clsLoop, workLoop, loop, graph.Network[currentN].Node[fromNode].Post[i], toNode, Type))
                        {
                            bReach = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (can_Reach(ref  graph, currentN, ref clsLoop, workLoop, loop, graph.Network[currentN].Node[fromNode].Post[i], toNode, Type))
                    {
                        bReach = true;
                        break;
                    }
                }
            }
            return bReach;
        }
    }
}
