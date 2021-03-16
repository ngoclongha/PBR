using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Functionalities
{
    class NodeSplittingType3
    {
        private gProAnalyzer.Functionalities.DominanceIdentification fndDomRel;
        private gProAnalyzer.Preprocessing.clsExtendNetwork extNet;
        private gProAnalyzer.Ultilities.clsFindNodeInfo nodeInfo;
        private gProAnalyzer.Functionalities.LoopIdentification findLoop;

        private static void Initialize_All()
        {
            //fndDomRel = new gProAnalyzer.Functionalities.DominanceIdentification();
            //extNet = new gProAnalyzer.Preprocessing.clsExtendNetwork();
            //nodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
            //findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
        }

        public static void Run_Split_Type3(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsLoop clsLoop, int orgLoop,
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, bool isRigidSplit) //isRigidSplit is used for flag to determine whether to split entry of rigid containing start events or not!!
        {
            Initialize_All();

            bool mStop = false;
            int sNode;
            int eNode;            
            int curDepth = clsSESE.SESE[currentSESE].maxDepth;
            do
            {
                for (int j = 0; j < clsSESE.SESE[currentSESE].nSESE; j++) //Visit all the SESE flow
                {
                    if (clsSESE.SESE[currentSESE].SESE[j].depth != curDepth) continue; 

                    sNode = clsSESE.SESE[currentSESE].SESE[j].Entry;
                    eNode = clsSESE.SESE[currentSESE].SESE[j].Exit;

                    if (!check_SESE_Entry(ref graph, currentN, ref clsSESE, currentSESE, sNode, j))
                        Type_III_Split_Entry(ref graph, currentN, ref clsSESE, currentSESE, j, 1, sNode, eNode, "");

                    if (!check_SESE_Entry(ref graph, currentN, ref clsSESE, currentSESE, eNode, j))
                        Type_III_Split_Exit(ref graph, currentN, ref clsSESE, currentSESE, j, 1, eNode, eNode);
                }
                curDepth--;
            } while (curDepth > 0 && !mStop);

            gProAnalyzer.Functionalities.DominanceIdentification.find_Dom(ref graph, currentN); //Dom
            gProAnalyzer.Functionalities.DominanceIdentification.find_Pdom(ref graph, currentN); //Postdom

            gProAnalyzer.Functionalities.DominanceIdentification.find_DomEI(ref graph, currentN, -1); //SS split node만..... //eDom^-1(NodeD)
            gProAnalyzer.Functionalities.DominanceIdentification.find_PdomEI(ref graph, currentN); // join node만..... //ePdom^-1(NodeR)

            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, currentN, ref clsLoop, orgLoop, ref clsLoop.IrreducibleError);
        }

        private static void Type_III_Split_Entry(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, 
            int kSESE, int addNum, int sNode, int cNode, string sName)
        {
            gProAnalyzer.Preprocessing.clsExtendNetwork.extent_Network(ref graph, currentN, addNum);
            int addNode = graph.Network[currentN].nNode - addNum;
                        
            graph.Network[currentN].Node[addNode].Kind = graph.Network[currentN].Node[sNode].Kind; //new change => to keep the node after split same it father.
            graph.Network[currentN].Node[addNode].orgNum = sNode;
            graph.Network[currentN].Node[addNode].parentNum = addNode;
            graph.Network[currentN].Node[addNode].Special = "";
            if (sName == "SSD")
            {
                graph.Network[currentN].Node[addNode].Name = "SSD";// addNode.ToString();
                graph.Network[currentN].Node[addNode].Type_I = "";
            }
            else
            {
                graph.Network[currentN].Node[addNode].Name = graph.Network[currentN].Node[sNode].Name;// sNode.ToString();
                graph.Network[currentN].Node[addNode].Type_I = graph.Network[currentN].Node[sNode].Type_I;
                graph.Network[currentN].Node[addNode].Type_II = graph.Network[currentN].Node[sNode].Type_II + "_se";
            }
            
            int addLink = graph.Network[currentN].nLink - addNum;            
            graph.Network[currentN].Link[addLink].fromNode = sNode;
            graph.Network[currentN].Link[addLink].toNode = addNode;
            
            //Noi link trong SESE
            for (int k = 0; k < graph.Network[currentN].nLink - addNum; k++)
            {
                if (graph.Network[currentN].Link[k].fromNode == sNode)
                {
                    bool inSESE = false;
                    for (int m = 0; m < clsSESE.SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        //Neu moi Link K trong SESE => inSESE = true => tạo kết nối với addNode 
                        if (graph.Network[currentN].Link[k].toNode == clsSESE.SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        graph.Network[currentN].Link[k].fromNode = addNode;
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[k].toNode);
                    }
                }
                if (graph.Network[currentN].Link[k].toNode == sNode)
                {
                    bool inSESE = false;
                    for (int m = 0; m < clsSESE.SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        if (graph.Network[currentN].Link[k].fromNode == clsSESE.SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        graph.Network[currentN].Link[k].toNode = addNode;
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[k].fromNode);
                    }
                }
            }

            gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, addNode); //find the new predecessors and sucessor of this node
            gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, sNode); //find the new predecessors and sucessor of this node => after split type III into 2 node (sNode and addNode)

            //Đổi entry bằng addNode cho SESE
            clsSESE.SESE[currentSESE].SESE[kSESE].Entry = addNode;
            for (int m = 0; m < clsSESE.SESE[currentSESE].SESE[kSESE].nNode; m++)
            {
                if (clsSESE.SESE[currentSESE].SESE[kSESE].Node[m] == sNode)
                {
                    clsSESE.SESE[currentSESE].SESE[kSESE].Node[m] = addNode;
                    break;
                }
            }

            //parent정보 변경
            if (clsSESE.SESE[currentSESE].SESE[kSESE].parentSESE != -1)
            {
                Add_Parent_SESE(ref clsSESE, currentSESE, clsSESE.SESE[currentSESE].SESE[kSESE].parentSESE, addNum, addNode);
            }

            //==== ADD CODE HERE ==========
            //We need to check whether this SESE (currentSESE and kSESE) is a simple strucuture or not.
            //if it is a simple structure we will find the way to change the SESE Entry gateway.
            //A simple structure is a structure which have 2 gateways, Entry and Exit (of SESE)
            //So First, we will check SESE is simple structure, Second, we will change the gateway type

            //verifySimpleStructure(currentN, currentSESE, kSESE, SESE[currentSESE].SESE[kSESE].Entry, SESE[currentSESE].SESE[kSESE].Exit, true);    

            //==== END OF NEW CODE ========
        }       

        public static void Type_III_Split_Exit(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, 
            int kSESE, int addNum, int eNode, int cNode)
        {
            gProAnalyzer.Preprocessing.clsExtendNetwork.extent_Network(ref graph, currentN, addNum);
            int addNode = graph.Network[currentN].nNode - addNum;

            graph.Network[currentN].Node[addNode].Kind = graph.Network[currentN].Node[cNode].Kind;
            graph.Network[currentN].Node[addNode].orgNum = eNode;
            graph.Network[currentN].Node[addNode].parentNum = addNode;
            graph.Network[currentN].Node[addNode].Special = "";

            graph.Network[currentN].Node[addNode].Name = graph.Network[currentN].Node[eNode].Name;// eNode.ToString();
            graph.Network[currentN].Node[addNode].Type_I = graph.Network[currentN].Node[eNode].Type_I;
            graph.Network[currentN].Node[addNode].Type_II = graph.Network[currentN].Node[eNode].Type_II + "_sx";

            int addLink = graph.Network[currentN].nLink - addNum;
            
            graph.Network[currentN].Link[addLink].fromNode = addNode;
            graph.Network[currentN].Link[addLink].toNode = eNode;

            //기존 Link 정보 변경
            for (int k = 0; k < graph.Network[currentN].nLink - addNum; k++)
            {
                if (graph.Network[currentN].Link[k].toNode == eNode) //For normal SESE (Acyclic)
                {
                    bool inSESE = false;
                    for (int m = 0; m < clsSESE.SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        if (graph.Network[currentN].Link[k].fromNode == clsSESE.SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        graph.Network[currentN].Link[k].toNode = addNode;
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[k].fromNode);
                    }
                }
                if (graph.Network[currentN].Link[k].fromNode == eNode) //For special SESE (Weird NL)
                {
                    bool inSESE = false;
                    for (int m = 0; m < clsSESE.SESE[currentSESE].SESE[kSESE].nNode; m++)
                    {
                        if (graph.Network[currentN].Link[k].toNode == clsSESE.SESE[currentSESE].SESE[kSESE].Node[m])
                        {
                            inSESE = true;
                            break;
                        }
                    }
                    if (inSESE)
                    {
                        graph.Network[currentN].Link[k].fromNode = addNode;
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[k].toNode);
                    }
                }
            }

            gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, addNode);
            gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, eNode);

            clsSESE.SESE[currentSESE].SESE[kSESE].Exit = addNode;
            for (int m = 0; m < clsSESE.SESE[currentSESE].SESE[kSESE].nNode; m++)
            {
                if (clsSESE.SESE[currentSESE].SESE[kSESE].Node[m] == eNode)
                {
                    clsSESE.SESE[currentSESE].SESE[kSESE].Node[m] = addNode;
                    break;
                }
            }
            
            if (clsSESE.SESE[currentSESE].SESE[kSESE].parentSESE != -1)
            {
                Add_Parent_SESE(ref clsSESE, currentSESE, clsSESE.SESE[currentSESE].SESE[kSESE].parentSESE, addNum, addNode);
            }
            //Do the same entry of SESE => We check simple structure of this current SESE and change the gateway type
            //verifySimpleStructure(currentN, currentSESE, kSESE, SESE[currentSESE].SESE[kSESE].Entry, SESE[currentSESE].SESE[kSESE].Exit, false);
        }

        public static bool check_SESE_Entry(ref gProAnalyzer.GraphVariables.clsGraph graph,  int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int curNode, int curSESE) //check outter edges of an entrySESE or exitSESE (if more than 1 => should split)
        {
            int count_outterEdges = 0;
            for (int i = 0; i < graph.Network[currentN].Node[curNode].nPost; i++)
            {
                int node = graph.Network[currentN].Node[curNode].Post[i];
                if (!node_insideSESE(clsSESE.SESE[currentSESE].SESE[curSESE].Node, node))
                    count_outterEdges++;
            }
            for (int i = 0; i < graph.Network[currentN].Node[curNode].nPre; i++)
            {
                int node = graph.Network[currentN].Node[curNode].Pre[i];
                if (!node_insideSESE(clsSESE.SESE[currentSESE].SESE[curSESE].Node, node))
                    count_outterEdges++;
            }
            if (count_outterEdges == 1) return true;
            else
                return false;
        }

        public static bool node_insideSESE(int[] calSESE, int node)
        {
            for (int i = 0; i < calSESE.Length; i++)
            {
                if (calSESE[i] == node)
                    return true;
            }
            return false;
        }

        public static void Add_Parent_SESE(ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int pSESE, int addNum, int addNode)
        {
            int[] imSESE = clsSESE.SESE[currentSESE].SESE[pSESE].Node;

            clsSESE.SESE[currentSESE].SESE[pSESE].nNode += addNum;
            clsSESE.SESE[currentSESE].SESE[pSESE].Node = new int[clsSESE.SESE[currentSESE].SESE[pSESE].nNode];
            for (int m = 0; m < imSESE.Length; m++) clsSESE.SESE[currentSESE].SESE[pSESE].Node[m] = imSESE[m];
            clsSESE.SESE[currentSESE].SESE[pSESE].Node[clsSESE.SESE[currentSESE].SESE[pSESE].nNode - addNum] = addNode;

            if (clsSESE.SESE[currentSESE].SESE[pSESE].parentSESE != -1)
            {
                Add_Parent_SESE(ref clsSESE, currentSESE, clsSESE.SESE[currentSESE].SESE[pSESE].parentSESE, addNum, addNode);
            }
        }

        private void verifySimpleStructure(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int kSESE, int entrySESE, int exitSESE, bool flag)
        {
            int index;
            string currName = null;
            if (flag == true)
            {
                index = graph.Network[currentN].Node[entrySESE].Name.Length;
                if (index > 1)
                    currName = graph.Network[currentN].Node[entrySESE].Name.Substring(0, 2);
                if (isSimpleStructure(ref graph, currentN, ref clsSESE, currentSESE, kSESE) == true && currName == "SS")
                {
                    graph.Network[currentN].Node[entrySESE].Kind = graph.Network[currentN].Node[exitSESE].Kind;
                }
            }
            else
            {
                index = graph.Network[currentN].Node[exitSESE].Name.Length;
                if (index > 1)
                    currName = graph.Network[currentN].Node[exitSESE].Name.Substring(0, 2);
                if (isSimpleStructure(ref graph, currentN, ref clsSESE, currentSESE, kSESE) == true && currName == "EE")
                {
                    graph.Network[currentN].Node[exitSESE].Kind = graph.Network[currentN].Node[entrySESE].Kind;
                }
            }
        }

        private bool isSimpleStructure(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int kSESE)
        {
            int nNodeSESE = clsSESE.SESE[currentSESE].SESE[kSESE].nNode;
            int countNode = 0;
            int currNode;
            for (int i = 0; i < nNodeSESE; i++)
            {
                currNode = clsSESE.SESE[currentSESE].SESE[kSESE].Node[i];
                if (graph.Network[currentN].Node[currNode].Kind == "OR" || graph.Network[currentN].Node[currNode].Kind == "XOR" || graph.Network[currentN].Node[currNode].Kind == "AND")
                    countNode = countNode + 1;
            }
            if (countNode > 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
