using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Functionalities
{
    class NodeSplittingType2
    {
        public static gProAnalyzer.Preprocessing.clsExtendNetwork extNet;
        public static gProAnalyzer.Functionalities.LoopIdentification findLoop;
        public static gProAnalyzer.Ultilities.findNodeInLoop nodeInLoop;
        public static gProAnalyzer.Ultilities.clsFindNodeInfo nodeInfo;

        public static void Initialize_All()
        {
            //extNet = new gProAnalyzer.Preprocessing.clsExtendNetwork();
            findLoop = new gProAnalyzer.Functionalities.LoopIdentification();
            nodeInLoop = new gProAnalyzer.Ultilities.findNodeInLoop();
            nodeInfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
        }
        public static void Run_Split_Type2(ref gProAnalyzer.GraphVariables.clsGraph graph, int midNet, int finalNet, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int orgLoop)
        {
            Initialize_All();

            // Network(2) 생성후 작업
            int nNode = graph.Network[midNet].nNode;
            int nLink = graph.Network[midNet].nLink;
            // 새네트워크 생성 (복제)
            graph.Network[finalNet] = graph.Network[midNet];

            //Loop없으면 건너뛰기
            if (clsLoop.Loop[orgLoop].nLoop == 0)
            {
                gProAnalyzer.Preprocessing.clsExtendNetwork.extent_Network(ref graph, finalNet, 0);
                return;
            }
            //Extent_network with the new number of node. (because we assign the fixed number of node of model when we read the model.
            //For example: nNode = 10; nLink = 14 => after extent => nNode = 10 + 10 (nNode); nLink = 14 + 10 (nNode)
            gProAnalyzer.Preprocessing.clsExtendNetwork.extent_Network(ref graph, finalNet, nNode);

            //Extension Type II split in Entry of Loop => (2 new node and 2 new link maybe create)
            int nSplit = Type_II_Split_Entry(ref graph, finalNet, ref clsLoop, orgLoop, nNode, nLink);
            nNode += nSplit;
            nLink += nSplit;

            //we need identify the loop after split entry (extension)
            //resize_Network(finalNet, nNode, nLink);
            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, finalNet, ref clsLoop, orgLoop, ref clsLoop.IrreducibleError); //===> Because the extension of type_2 create nodes inside the loops => we need add it into a Loop[] structure.
            //extent_Network(finalNet, nNode);

            nSplit = 0;
            nSplit = Type_II_Split_Exit(ref graph, finalNet, ref clsLoop, orgLoop, nNode, nLink);
            nNode += nSplit;
            nLink += nSplit;

            //fix CIPd Exit (Direct join of exits)
            add_Dummy_Task(ref graph, finalNet, ref clsLoop, orgLoop, ref nNode, ref nLink);

            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, finalNet, ref clsLoop, orgLoop, ref clsLoop.IrreducibleError);
            
            nSplit = 0;
            nSplit = Type_II_Split_Back(ref graph, finalNet, nNode, nLink); //(No split backward split)
            nNode += nSplit;
            nLink += nSplit;

            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, finalNet, ref clsLoop, orgLoop, ref clsLoop.IrreducibleError);

            resize_Network(ref graph, finalNet, nNode, nLink);

            //After_Type_II_Split(ref graph, finalNet, ref clsLoop, orgLoop); //Remove on Otc27_2020 (Might turn back latter)
            gProAnalyzer.Functionalities.LoopIdentification.Run_FindLoop(ref graph, finalNet, ref clsLoop, orgLoop, ref clsLoop.IrreducibleError);
        }

        public static int Type_II_Split_Entry(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN,ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int nNode, int nLink)
        {
            int nSplit = 0;
            for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++) //visit each loop in model
            {
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nEntry; k++) //vist each entry of current loop.
                {
                    int fjNode;
                    int bjNode;
                    int node = clsLoop.Loop[workLoop].Loop[i].Entry[k]; //get current entry and then split this node - if necessary
                    int[] preF = new int[nNode]; //store all node outside the loop
                    int[] preB = new int[nNode]; //store all node from inside the current loop (i)
                    int cntF = 0, cntB = 0;
                    for (int j = 0; j < nLink; j++)
                    {
                        if (graph.Network[currentN].Link[j].toNode == node) //if find a node which is go to current entry (node)
                        {
                            if (gProAnalyzer.Ultilities.findNodeInLoop.Node_In_Loop(ref clsLoop, workLoop, graph.Network[currentN].Link[j].fromNode, i)) //If the predecessor of current entry belong to this loop(i).
                            {
                                preB[cntB] = graph.Network[currentN].Link[j].fromNode; //store all node from inside the current loop (i) go to this entry (k) (node)
                                cntB++; //increase index - number of node inside the loop
                            }
                            else
                            {
                                preF[cntF] = graph.Network[currentN].Link[j].fromNode; //store all node which is predecessor of this entry (node)
                                cntF++;
                            }
                        }
                    }
                    if (cntF > 1) // For join이 2 미만이면 //it mean if all node go to entry outside the loop is just 1 or 0 => don't need to split.
                    {
                        fjNode = nNode;
                        bjNode = node;
                        //For Join Node - 추가 //Tao nut moi
                        graph.Network[currentN].Node[fjNode].Kind = graph.Network[currentN].Node[node].Kind;
                        graph.Network[currentN].Node[fjNode].Name = graph.Network[currentN].Node[node].Name;// node.ToString();
                        graph.Network[currentN].Node[fjNode].orgNum = node;
                        graph.Network[currentN].Node[fjNode].parentNum = fjNode;
                        graph.Network[currentN].Node[fjNode].Type_I = graph.Network[currentN].Node[node].Type_I;
                        graph.Network[currentN].Node[fjNode].Type_II = graph.Network[currentN].Node[node].Type_II + "_fj";

                        //We need focus on this node => split this node
                        //Back Join Node - 변경 //Thay doi info cua nut current entry
                        graph.Network[currentN].Node[bjNode].Type_II = graph.Network[currentN].Node[node].Type_II + "_bj"; //just add "_bj" for current entry
                        graph.Network[currentN].Node[fjNode].Special = ""; //What is that mean

                        //New Link 추가
                        graph.Network[currentN].Link[nLink].fromNode = fjNode;
                        graph.Network[currentN].Link[nLink].toNode = bjNode;

                        nSplit++;
                        nNode++; //nNode < nNode in extend_network
                        nLink++;

                        //기존 Link 정보 변경
                        //This code just consider the node outside the loop come to this entry. (adjust the link coming from outside the loop)
                        for (int j = 0; j < nLink; j++)
                        {
                            for (int j2 = 0; j2 < cntF; j2++)
                            {
                                //ajust the link from outside the loop to this entry node.
                                if (graph.Network[currentN].Link[j].fromNode == preF[j2] && graph.Network[currentN].Link[j].toNode == node)
                                {
                                    graph.Network[currentN].Link[j].toNode = fjNode;
                                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[j].fromNode);
                                }
                            }
                        }
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, fjNode);
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, bjNode);
                        if (clsLoop.Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(ref clsLoop, workLoop, node, fjNode, clsLoop.Loop[workLoop].Loop[i].parentLoop);
                    }
                    //========== EXTENTION ========================================================================(1 bai hoc nho doi)
                    #region hide
                    //if there are at least 2 incoming edges from inside the loop to this entry => extend type II 

                    /*if (isInsideLoop(currentN, workLoop, i, node) == true)
                    {
                        //==New code===
                        fjNode = nNode; //New Node
                        bjNode = node; //Current Entry
                        //create new node
                        Network[currentN].Node[fjNode].Kind = Network[currentN].Node[node].Kind;
                        Network[currentN].Node[fjNode].Name = Network[currentN].Node[node].Name;// node.ToString();
                        Network[currentN].Node[fjNode].orgNum = node;
                        Network[currentN].Node[fjNode].parentNum = fjNode;
                        Network[currentN].Node[fjNode].Type_I = Network[currentN].Node[node].Type_I;
                        Network[currentN].Node[fjNode].Type_II = Network[currentN].Node[node].Type_II + "_2";
                        //create new link
                        Network[currentN].Link[nLink].fromNode = fjNode;
                        Network[currentN].Link[nLink].toNode = bjNode;

                        nSplit++;
                        nNode++; //nNode < nNode in extend_network
                        nLink++;

                        //Consider all incoming edges from inside the current loop to this current entry === (new move from above) crazy <===
                        for (int j = 0; j < nLink; j++)
                        {
                            for (int j2 = 0; j2 < cntB; j2++) //consider the node inside the loop which have outgoing edges to loop entry
                            {
                                //ajust the link from outside the loop to this entry node. preB[] set node inside loop ; preF[] set of node outside the loop
                                if (Network[currentN].Link[j].fromNode == preB[j2] && Network[currentN].Link[j].toNode == node)
                                {
                                    Network[currentN].Link[j].toNode = fjNode; //fjNode is the new node
                                    find_NodeInform(currentN, Network[currentN].Link[j].fromNode);
                                }
                            }
                        }
                        find_NodeInform(currentN, fjNode); //inform for new node (after split type 2 extension)
                        find_NodeInform(currentN, bjNode); //inform for this loop's entry
                        if (Loop[workLoop].Loop[i].parentLoop > 0) Loop_Inform(workLoop, node, fjNode, Loop[workLoop].Loop[i].parentLoop);
                    }
                    */
                    #endregion
                    //========== END  ==============================================================================
                    //Type_II_Extension_Entry(currentN, workLoop, i, nNode, nLink, node, ref nSplit, preB, cntB);
                }
            }
            return nSplit;
        }
        public static int Type_II_Split_Exit(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int nNode, int nLink)
        {
            int nSplit = 0;

            for (int i = 0; i < clsLoop.Loop[workLoop].nLoop; i++)
            {
                for (int k = 0; k < clsLoop.Loop[workLoop].Loop[i].nExit; k++)
                {
                    int node = clsLoop.Loop[workLoop].Loop[i].Exit[k];

                    int[] postF = new int[nNode];
                    int[] postB = new int[nNode];
                    int cntF = 0, cntB = 0;
                    for (int j = 0; j < nLink; j++)
                    {
                        if (graph.Network[currentN].Link[j].fromNode == node)
                        {
                            if (gProAnalyzer.Ultilities.findNodeInLoop.Node_In_Loop(ref clsLoop, workLoop, graph.Network[currentN].Link[j].toNode, i))
                            {
                                postB[cntB] = graph.Network[currentN].Link[j].toNode;
                                cntB++;
                            }
                            else
                            {
                                postF[cntF] = graph.Network[currentN].Link[j].toNode;
                                cntF++;
                            }
                        }
                    }
                    if (cntF > 1) // For join이 2 미만이면
                    {
                        int fsNode = nNode;
                        int bsNode = node;
                        //For Spilt Node - 추가
                        graph.Network[currentN].Node[fsNode].Kind = graph.Network[currentN].Node[node].Kind;
                        graph.Network[currentN].Node[fsNode].Name = graph.Network[currentN].Node[node].Name;// node.ToString();
                        graph.Network[currentN].Node[fsNode].orgNum = node;
                        graph.Network[currentN].Node[fsNode].parentNum = fsNode;
                        graph.Network[currentN].Node[fsNode].Type_I = graph.Network[currentN].Node[node].Type_I;
                        graph.Network[currentN].Node[fsNode].Type_II = graph.Network[currentN].Node[node].Type_II + "_xfs";
                        graph.Network[currentN].Node[fsNode].Special = "";

                        //Back Spilt Node - 변경
                        graph.Network[currentN].Node[bsNode].Type_II = graph.Network[currentN].Node[node].Type_II + "_xbs";

                        //New Link 추가
                        graph.Network[currentN].Link[nLink].fromNode = bsNode;
                        graph.Network[currentN].Link[nLink].toNode = fsNode;

                        nSplit++;
                        nNode++;
                        nLink++;
                        //기존 Link 정보 변경

                        for (int j = 0; j < nLink; j++)
                        {
                            for (int j2 = 0; j2 < cntF; j2++)
                            {
                                if (graph.Network[currentN].Link[j].fromNode == node && graph.Network[currentN].Link[j].toNode == postF[j2])
                                {
                                    graph.Network[currentN].Link[j].fromNode = fsNode;
                                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[j].toNode);
                                }
                            }
                        }
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, fsNode);
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, bsNode);

                        if (clsLoop.Loop[workLoop].Loop[i].parentLoop >= 0) Loop_Inform(ref clsLoop, workLoop, node, fsNode, clsLoop.Loop[workLoop].Loop[i].parentLoop);
                    }
                    //Extention Type 2 Exit
                    //Type_II_Extension_Exit(currentN, workLoop, i, nNode, nLink, node, ref nSplit, postB, cntB);
                }
            }
            return nSplit;
        }

        public static void resize_Network(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int nNode, int nLink)
        {
            graph.Network[currentN].nNode = nNode;
            gProAnalyzer.GraphVariables.clsNode.strNode[] newNode = new gProAnalyzer.GraphVariables.clsNode.strNode[nNode];
            for (int i = 0; i < nNode; i++)
            {
                newNode[i] = graph.Network[currentN].Node[i];
            }
            graph.Network[currentN].nLink = nLink;
            gProAnalyzer.GraphVariables.clsEdge.strEdge[] newLink = new gProAnalyzer.GraphVariables.clsEdge.strEdge[nLink];
            for (int i = 0; i < nLink; i++)
            {
                newLink[i] = graph.Network[currentN].Link[i];
            }
            graph.Network[currentN].Node = newNode;
            graph.Network[currentN].Link = newLink;
        }

        public static void After_Type_II_Split(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop)
        {
            //Loop 재탐색
            gProAnalyzer.Functionalities.LoopIdentification.search_Loop(ref graph, currentN, ref clsLoop, workLoop);
            gProAnalyzer.Functionalities.LoopIdentification.merge_Irreducible(ref clsLoop, workLoop);
            gProAnalyzer.Functionalities.LoopIdentification.find_SpecialNode(ref graph, currentN, ref clsLoop, workLoop);            
        }

        private static int Type_II_Split_Back(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int nNode, int nLink)
        {
            int nSplit = 0;

            //Split - Back Split Node 이고 For split이 2 이상이면

            for (int i = 0; i < nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Special != "B" && graph.Network[currentN].Node[i].Special != "T") continue; // Back 아니면
                //find the backward split node
                int[] postF = new int[nNode];
                int[] postB = new int[nNode];
                int cntF = 0, cntB = 0;
                for (int j = 0; j < nLink; j++)
                {
                    if (graph.Network[currentN].Link[j].fromNode == i)
                    {
                        if (graph.Network[currentN].Link[j].bBackS) //just consider the split node
                        {
                            postB[cntB] = graph.Network[currentN].Link[j].toNode;
                            cntB++;
                        }
                        else
                        {
                            postF[cntF] = graph.Network[currentN].Link[j].toNode;
                            cntF++;
                        }
                    }
                }
                if (cntF > 1) //if (cntF < 2) continue; // For split이 2 미만이면
                {

                    int fsNode = nNode;
                    int bsNode = i;
                    //For Spilt Node - 추가
                    graph.Network[currentN].Node[fsNode].Kind = graph.Network[currentN].Node[i].Kind;
                    graph.Network[currentN].Node[fsNode].Name = graph.Network[currentN].Node[i].Name; // i.ToString();
                    graph.Network[currentN].Node[fsNode].orgNum = i;
                    graph.Network[currentN].Node[fsNode].parentNum = fsNode;
                    graph.Network[currentN].Node[fsNode].Type_I = graph.Network[currentN].Node[i].Type_I;
                    graph.Network[currentN].Node[fsNode].Type_II = graph.Network[currentN].Node[i].Type_II + "_fs";
                    graph.Network[currentN].Node[fsNode].Special = "";
                    //Back Join Node - 변경
                    graph.Network[currentN].Node[bsNode].Type_II = graph.Network[currentN].Node[i].Type_II + "_bs";
                    //New Link 추가
                    graph.Network[currentN].Link[nLink].fromNode = bsNode;
                    graph.Network[currentN].Link[nLink].toNode = fsNode;
                    nSplit++;
                    nNode++;
                    nLink++;
                    //기존 Link 정보 변경
                    for (int j = 0; j < nLink; j++)
                    {
                        for (int j2 = 0; j2 < cntF; j2++)
                        {
                            if (graph.Network[currentN].Link[j].fromNode == i && graph.Network[currentN].Link[j].toNode == postF[j2])
                            {
                                graph.Network[currentN].Link[j].fromNode = fsNode;
                                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, graph.Network[currentN].Link[j].toNode);
                            }
                        }
                    }
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, fsNode);
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, bsNode);
                }
                //Extension Type II Backward split
                //Type_II_Extension_Back(currentN, nNode, nLink, i, ref nSplit, postB, cntB); //PostB[] and cntB (Store all BS of node i)
            }
            return nSplit;
        }

        public static void Loop_Inform(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int rNode, int node, int loop)
        {
            //포함 노드 수정
            int nNew = clsLoop.Loop[workLoop].Loop[loop].nNode + 1;
            int[] New = new int[nNew];
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nNode; i++)
            {
                New[i] = clsLoop.Loop[workLoop].Loop[loop].Node[i];
            }
            New[nNew - 1] = node;
            clsLoop.Loop[workLoop].Loop[loop].nNode = nNew;
            clsLoop.Loop[workLoop].Loop[loop].Node = New;
            //entry 수정
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nEntry; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[loop].Entry[i] == rNode) clsLoop.Loop[workLoop].Loop[loop].Entry[i] = node;
            }
            //exit 수정
            for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nExit; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[loop].Exit[i] == rNode) clsLoop.Loop[workLoop].Loop[loop].Exit[i] = node;
            }
        }

        public static void add_Dummy_Task(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workloop, ref int nNode_org, ref int nLink_org)
        {
            int nNode = graph.Network[currentN].nNode;
            int[,] pairingMatrix = new int[nNode, 2]; //paringMatrix[x][0] ~ the post of Exit, pairingMatrix[x][1] ~ the exit of loop.
            int nPair = 0; //get all pair of [Exit -> CIPd] and then compare, add dummy!
            for (int i = 0; i < clsLoop.Loop[workloop].nLoop; i++)
            {
                for (int j = 0; j < clsLoop.Loop[workloop].Loop[i].nExit; j++)
                {
                    for (int p = 0; p < graph.Network[currentN].Node[clsLoop.Loop[workloop].Loop[i].Exit[j]].nPost; p++)
                    {
                        int post = graph.Network[currentN].Node[clsLoop.Loop[workloop].Loop[i].Exit[j]].Post[p];
                        if (Ultilities.checkGraph.Node_In_Loop(clsLoop, workloop, post, i) == false)
                        {
                            //check duplicated
                            if (check_Duplicated_pairMatrix(pairingMatrix, nPair, post, clsLoop.Loop[workloop].Loop[i].Exit[j])) break;
                            pairingMatrix[nPair, 0] = post;
                            pairingMatrix[nPair, 1] = clsLoop.Loop[workloop].Loop[i].Exit[j];
                            nPair++;
                            break;
                        }
                    }
                }
            }

            int[,] pairingMatrix_2 = new int[nNode, nNode]; //paringMatrix[0][x] ~ the direct CIPd of entries x
            populate_matrix(pairingMatrix_2, nNode); //populate matrix ~ -1
            int npair_row = 0;
            int npair_col = 0;
            int count = 1;
            for (int i = 0; i < nPair; i++)
            {
                if (pairingMatrix[i, 0] < 0) continue;
                count = 1;
                for (int j = 0; j < nPair; j++)
                    if (pairingMatrix[i, 0] == pairingMatrix[j, 0] && i != j)
                    {
                        count++;
                        if (count == 2)
                        {
                            pairingMatrix_2[npair_row, npair_col] = pairingMatrix[i, 0];
                            npair_col++;
                            pairingMatrix_2[npair_row, npair_col] = pairingMatrix[i, 1];
                            npair_col++;
                            //pairingMatrix[i, 0] = -1;
                            pairingMatrix_2[npair_row, npair_col] = pairingMatrix[j, 1];
                            npair_col++;
                            pairingMatrix[j, 0] = -1;
                        }
                        else
                        {
                            pairingMatrix_2[npair_row, npair_col] = pairingMatrix[j, 1];
                            npair_col++;
                            pairingMatrix[j, 0] = -1;
                        }
                        //marking to avoid redundancy of 2 colm matrix                                               
                    }
                if (count > 1)
                {                    
                    npair_row++;
                }
                npair_col = 0;
            }

            //add more task
            for (int i = 0; i < npair_row; i++)
            {
                int num_Task = 0;
                for (int j = 0; j < nNode; j++)
                    if (pairingMatrix_2[i, j] != -1) num_Task++;
                //Add a task in middle of 2 node
                for (int j = 1; j < num_Task - 1; j++)
                {
                    int node_1 = pairingMatrix_2[i, 0];
                    int node_2 = pairingMatrix_2[i, j];
                    add_aTask(ref graph, currentN, ref clsLoop, workloop, node_1, node_2, ref nNode_org, ref nLink_org);
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, node_1);
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, node_2);
                    gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, nNode_org - 1);
                }
            }
        }

        public static bool check_Duplicated_pairMatrix(int[,] pairMatrix, int nPair, int first, int second)
        {
            for (int i = 0; i < nPair; i++)
                if (pairMatrix[i, 0] == first && pairMatrix[i, 1] == second)
                    return true;
            return false;

        }

        public static void populate_matrix(int[,] Mx, int n)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    Mx[i, j] = -1;
        }

        public static void add_aTask(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workloop, int first_node, int second_node, ref int nNode, ref int nLink)
        {
            //int nNode = graph.Network[currentN].nNode;
            //int nLink = graph.Network[currentN].nLink;

            graph.Network[currentN].Node[nNode].Kind = "TASK";
            graph.Network[currentN].Node[nNode].Name = "Dummy";// node.ToString();
            graph.Network[currentN].Node[nNode].orgNum = nNode;
            graph.Network[currentN].Node[nNode].parentNum = nNode;
            graph.Network[currentN].Node[nNode].Type_I = "";
            graph.Network[currentN].Node[nNode].Type_II = "";
            graph.Network[currentN].Node[nNode].Special = "";

            //New Link 추가
            //find old link from Exit to its common Post[]
            for (int i = 0; i < nLink; i++)
            {
                if (graph.Network[currentN].Link[i].fromNode == second_node && graph.Network[currentN].Link[i].toNode == first_node)
                {
                    graph.Network[currentN].Link[i].fromNode = second_node; //exit
                    graph.Network[currentN].Link[i].toNode = nNode; //Dummy
                }
            }
            graph.Network[currentN].Link[nLink].fromNode = nNode;
            graph.Network[currentN].Link[nLink].toNode = first_node;
            nLink++;

            nNode++;            
        }
    }
}
