using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class checkGraph
    {
        public static bool check_SameSet(int[] A, int[] B)
        {
            bool same = false;

            if (A == null && B == null)
            {
                same = true;
            }
            else if (A == null)
            {
                same = false;
            }
            else if (B == null)
            {
                same = false;
            }
            else
            {
                if (A.Length != B.Length)
                {
                    same = false;
                }
                else
                {
                    same = true;
                    for (int i = 0; i < A.Length; i++)
                    {
                        bool isIn = false;
                        for (int j = 0; j < B.Length; j++)
                        {
                            if (A[i] == B[j])
                            {
                                isIn = true;
                                break;
                            }
                        }

                        if (!isIn)
                        {
                            same = false;
                            break;
                        }
                    }
                }


            }
            return same;
        }

        public static bool check_Overlap(int[] A, int[] B)
        {
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < B.Length; j++)
                {
                    if (A[i] == B[j])
                        return true;
                }
            }
            return false;
        }

        public static bool isLoopEntries(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int curNode)
        {
            int numLoop = clsLoop.Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[i].nEntry; j++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].Entry[j] == curNode)
                        return true;
                }
            }
            return false;
        }

        public static bool isLoopExits(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int curNode)
        {
            int numLoop = clsLoop.Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                for (int j = 0; j < clsLoop.Loop[workLoop].Loop[i].nExit; j++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].Exit[j] == curNode)
                        return true;
                }
            }
            return false;
        }

        public static bool isLoopSingleExit(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int curNode, int workLoop) //it mean this curNode must be single loop exit
        {
            int numLoop = clsLoop.Loop[workLoop].nLoop;
            for (int i = 0; i < numLoop; i++)
            {
                if (clsLoop.Loop[workLoop].Loop[i].nExit == 1)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].Exit[0] == curNode)
                        return true;
                }
            }
            return false;
        }

        public static bool isLoopExit(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int curNode, int workLoop) //it mean this curNode must be single loop exit
        {
            int numLoop = clsLoop.Loop[workLoop].nLoop;
            int numExit;
            for (int i = 0; i < numLoop; i++)
            {
                numExit = clsLoop.Loop[workLoop].Loop[i].nExit;
                for (int j = 0; j < numExit; j++)
                {
                    if (clsLoop.Loop[workLoop].Loop[i].Exit[j] == curNode)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool isLoopHeader(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workloop, int node) //check "node" is header (reduced (if any) or not
        {
            for (int i = 0; i < clsLoop.Loop[workloop].nLoop; i++)
            {
                if (clsLoop.Loop[workloop].Loop[i].header == node) return true;
            }
            return false;
        }

        public static bool Node_In_Loop(gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int node, int loop) //node in loop => return true; not in loop => return false
        {
            bool inLoop = false;

            if (node == clsLoop.Loop[workLoop].Loop[loop].header) {
                inLoop = true;
            }
            else {
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nNode; i++) {
                    if (node == clsLoop.Loop[workLoop].Loop[loop].Node[i]) {
                        inLoop = true;
                        break;
                    }
                }
            }
            if (!inLoop) {
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nChild; i++) {
                    inLoop = Node_In_Loop(clsLoop, workLoop, node, clsLoop.Loop[workLoop].Loop[loop].child[i]);
                    if (inLoop) break;
                }
            }
            return inLoop;
        }

        public static bool Node_In_Set(int[] A, int n, int node)
        {
            if (A != null || n > 0)
                for (int i = 0; i < n; i++) {
                    if (node == A[i])
                        return true;
                }
            return false;
        }

        public static bool isConcurrentEntry(GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, int cur_concurrency, int entry, int curEnIndex)
        {
            if (cur_concurrency == 0) {
                if (entry == curEnIndex)
                    return true;
            }   
            else
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nEntry; i++) {
                    if (clsLoop.Loop[workLoop].Loop[curLoop].Entry[i] == entry) {
                        if (clsLoop.Loop[workLoop].Loop[curLoop].Concurrency[i] == cur_concurrency)
                            return true;

                    }
                }
            return false;
        }

        public static bool Node_In_SESE(GraphVariables.clsSESE clsSESE, int workSESE, int node, int currSESE)
        {
            bool inside = false;
            for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currSESE].nNode; i++)
            {
                if (clsSESE.SESE[workSESE].SESE[currSESE].Node[i] == node)
                {
                    inside = true;
                    break;
                }
            }
            if (inside)
            {
                for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currSESE].nChild; i++)
                {
                    if (currSESE != clsSESE.SESE[workSESE].SESE[currSESE].child[i])
                    {
                        for (int j = 0; j < clsSESE.SESE[workSESE].SESE[clsSESE.SESE[workSESE].SESE[currSESE].child[i]].nNode; j++)
                        {
                            if (clsSESE.SESE[workSESE].SESE[clsSESE.SESE[workSESE].SESE[currSESE].child[i]].Node[j] == node)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            else return false;
            return true;
        }

        public static string Bond_Check(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsSESE clsSESE, int workSESE, int currentSESE, ref GraphVariables.clsHWLS clsHWLS) //should upgrade bond check ===
        {
            int count_gateway = 0;
            //check SESE nested with Loops (2 cases) (if SESE have at least 1 directly child is loop => rigid.
            int f_index = getFblockIndex(currentSESE, clsHWLS);
            for (int i = 0; i < clsHWLS.FBLOCK.FBlock[f_index].nChild; i++)
            {
                int child = clsHWLS.FBLOCK.FBlock[f_index].child[i];
                if (clsHWLS.FBLOCK.FBlock[child].SESE == false && !(clsHWLS.FBLOCK.FBlock[child].nEntry == 1 && clsHWLS.FBLOCK.FBlock[child].nExit == 1)) return "R";
            }

            for (int i = 0; i < clsSESE.SESE[workSESE].SESE[currentSESE].nNode; i++)
            {
                int node =clsSESE.SESE[workSESE].SESE[currentSESE].Node[i];
                if (Node_In_SESE(clsSESE, workSESE, node, currentSESE))
                {
                    if (graph.Network[currentN].Node[node].nPre > 1 || graph.Network[currentN].Node[node].nPost > 1)
                    {
                        count_gateway++;
                        if (count_gateway > 2) return "R";
                    }
                }
            }
            if (count_gateway == 0) return "P";

            return "B";
        }

        public static int getFblockIndex(int currentSESE, GraphVariables.clsHWLS clsHWLS)
        {
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++)
            {
                if (currentSESE == clsHWLS.FBLOCK.FBlock[i].refIndex)
                    return i;
            }
            return -1;
        }

        public static int check_real_errors(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int currentLoop, ref GraphVariables.clsError clsError, int errorNode) //return(1) => Real; return(2) => Potential; return(3) => Dom
        {
            int[][] adjList = null;
            if (true) //acyclic
            {
                int[] getPath = new int[graph.Network[currentN].nNode];
                int START = graph.Network[currentN].header;
                int nGetPath = 0;
                bool[] Mark = new bool[graph.Network[currentN].nNode];
                gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, currentN, ref adjList); //We already buil an instance subgraph based on InstanceNode !! Huaahhh~
                //Check "Real error" first
                for (int i = 0; i < clsError.nError; i++) //filtering the DL or LoS of acyclic only
                {                    
                    if (!check_Error_Num(clsError, i)) continue;//filtering acylci error
                    bool flag_Real_error = true; //0: real; 
                    int EXIT = Convert.ToInt32(clsError.Error[i].Node);
                    DFS_Recursive(adjList, ref Mark, START, EXIT, ref getPath, ref nGetPath, clsError, ref flag_Real_error);
                    if (flag_Real_error == true)
                        clsError.Error[i].TypeOfError = "R";
                    else
                        clsError.Error[i].TypeOfError = "P";
                }
                //Check "Domiance error" from "Potential error"
                for (int i = 0; i < clsError.nError; i++) //filtering the DL or LoS of acyclic only
                {
                    if (!check_Error_Num(clsError, i)) continue;//filtering acylci error
                    if (clsError.Error[i].TypeOfError != "R") continue; //only pick REAL errors for referencing
                    int currError = Convert.ToInt32(clsError.Error[i].Node); //This is a real error
                    for (int k = 0; k < clsError.nError; k++) //filtering the DL or LoS of acyclic only
                    {
                        if (!check_Error_Num(clsError, k)) continue;//filtering acylci error
                        if (clsError.Error[k].Node == clsError.Error[i].Node) continue;
                        if (Ultilities.checkGraph.Node_In_Set(graph.Network[currentN].Node[currError].DomEI, graph.Network[currentN].Node[currError].nDomEI, Convert.ToInt32(clsError.Error[k].Node)))
                        {
                            clsError.Error[k].TypeOfError = "D";
                        }
                    }
                }  
                //Check "Dominance_by_Potential error" of Potential errors (From the leftover potential errors)
                for (int i = 0; i < clsError.nError; i++) //filtering the DL or LoS of acyclic only
                {
                    if (!check_Error_Num(clsError, i)) continue;//filtering acyclic error
                    if (clsError.Error[i].TypeOfError != "P") continue;
                    int currError = Convert.ToInt32(clsError.Error[i].Node);
                    for (int k = 0; k < clsError.nError; k++) //filtering the DL or LoS of acyclic only
                    {
                        if (!check_Error_Num(clsError, k)) continue;//filtering acylci error
                        if (clsError.Error[k].Node == clsError.Error[i].Node) continue;
                        if (Ultilities.checkGraph.Node_In_Set(graph.Network[currentN].Node[currError].DomEI, graph.Network[currentN].Node[currError].nDomEI, Convert.ToInt32(clsError.Error[k].Node)))
                        {
                            clsError.Error[k].TypeOfError = "DP";
                        }
                    }
                }
            }
            else //if cylic graph
            {
                //First, check Error type DL, LoS, same with above (Must reduce all loop) => Compute the dominance relations
                //If error from inside NL => check (IF REDUCE LOOP => MUST MARK other reducedLoop as having Error for easily check).
                    //If current NL => Take out NL subgraph => run DFS to check "Real_Error" and "Potential_errors" -> Check "Dominance_Errors"
                        //If current NL Header is "Real" => keep everything original
                        //If current NL Header is "Dom" => All error wil be "Dominace_Errors"
                        //If current NL Header is "Potential" => All error will be "Potential_Errors"
                #region
                int curDepth = clsHWLS.FBLOCK.maxDepth;
                do
                {
                    for (int k = 0; k < clsHWLS.FBLOCK.nFBlock; k++)
                    {
                        if (clsHWLS.FBLOCK.FBlock[k].depth != curDepth) continue;
                        int orgIndx = clsHWLS.FBLOCK.FBlock[k].refIndex;
                        if (clsHWLS.FBLOCK.FBlock[k].SESE == false && clsLoop.Loop[currentLoop].Loop[orgIndx].nEntry == 1)
                        {
                            gProAnalyzer.Ultilities.makeSubNetwork.make_AcyclicSubGraph(ref graph, currentN, graph.subNet, ref clsLoop, currentLoop, orgIndx, "NL");

                            int[] getPath = new int[graph.Network[graph.subNet].nNode];
                            int START = graph.Network[graph.subNet].header;
                            int nGetPath = 0;
                            bool[] Mark = new bool[graph.Network[graph.subNet].nNode];
                            gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, graph.subNet, ref adjList); //We already buil an instance subgraph based on InstanceNode !! Huaahhh~
                            //Check "Real error" first
                            for (int i = 0; i < clsError.nError; i++) //filtering the DL or LoS of acyclic only
                            {
                                if (!(clsError.Error[k].messageNum == 3 || clsError.Error[k].messageNum == 4 || clsError.Error[k].messageNum == 29 ||
                                    clsError.Error[k].messageNum == 5 || clsError.Error[k].messageNum == 6)) continue; //filtering Loop error only [3,4,5,6,29]
                                bool flag_Real_error = true; //0: real; 
                                int EXIT = Convert.ToInt32(clsError.Error[i].Node);
                                DFS_Recursive(adjList, ref Mark, START, EXIT, ref getPath, ref nGetPath, clsError, ref flag_Real_error);
                                if (flag_Real_error == true)
                                    clsError.Error[i].TypeOfError = "R";
                                else
                                    clsError.Error[i].TypeOfError = "P";
                            }
                        }
                    }
                    curDepth--;
                } while (curDepth > 0);
                #endregion
            }

            return 1;
        }

        public static bool check_Error_Num(GraphVariables.clsError clsError, int idx_error) //LoS. DL in acyclic (including Fwd and Bwd)
        {
            if (clsError.Error[idx_error].messageNum == 27 || clsError.Error[idx_error].messageNum == 28 ||
                clsError.Error[idx_error].messageNum == 3 || clsError.Error[idx_error].messageNum == 4 ||
                clsError.Error[idx_error].messageNum == 5 || clsError.Error[idx_error].messageNum == 6) return true;
            return false;
        }

        public static void DFS_Fwd_Bwd(int[][] adjList, ref bool[] Mark, int Entry, int[] Exit, ref int[] getPath, ref int nGetPath)
        {
            Stack stack = new Stack();
            Mark[Entry] = true;
            stack.Push(Entry);
            getPath[nGetPath] = Entry;
            nGetPath++;
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());
                //do nothing

                for (int i = 1; i <= adjList[curNode][0]; i++)
                {
                    int v = adjList[curNode][i];

                    if (!Mark[v] && !Node_In_Set(Exit, Exit.Length, v))
                    {
                        stack.Push(v);
                        Mark[v] = true;
                        getPath[nGetPath] = v;
                        nGetPath++;
                    }
                    else
                    {
                        if (Node_In_Set(Exit, Exit.Length, v))
                        {
                            getPath[nGetPath] = v;
                            nGetPath++;
                        }
                    }
                }
            }
        }

        public static void DFS_Fwd_Bwd_Recursive(int[][] adjList, ref bool[] Mark, int Entry, int Exit, ref int[] getPath, ref int nGetPath, GraphVariables.clsError clsError, ref bool flag)
        {
            if (flag == false) return;  //if check error is not real => exit

            Stack stack = new Stack();
            Mark[Entry] = true;
            stack.Push(Entry);
            getPath[nGetPath] = Entry;
            nGetPath++;
            for (int i = 1; i <= adjList[Entry][0]; i++)
            {
                int v = adjList[Entry][i];
                if (!Mark[v] && v != Exit)
                {
                    DFS_Recursive(adjList, ref Mark, v, Exit, ref getPath, ref nGetPath, clsError, ref flag);
                }
                else
                    if (v == Exit)
                    {
                        //Get the trace from START to EXIT
                        //getPath[nGetPath] = v;
                        //nGetPath++;
                        //Check real error
                        bool check_real = check_DFS_real_error(clsError, getPath, nGetPath, v);
                        if (check_real == false) flag = false;
                    }
            }
            //remove from path
            Mark[Entry] = false;
            nGetPath--;
        }

        public static void DFS_Recursive(int[][] adjList, ref bool[] Mark, int Entry, int Exit, ref int[] getPath, ref int nGetPath, GraphVariables.clsError clsError, ref bool flag)
        {
            if (flag == false) return;  //if check error is not real => exit

            Stack stack = new Stack();
            Mark[Entry] = true;
            stack.Push(Entry);
            getPath[nGetPath] = Entry;
            nGetPath++;
            for (int i = 1; i <= adjList[Entry][0]; i++)
            {
                int v = adjList[Entry][i];
                if (!Mark[v] && v != Exit)
                {
                    DFS_Recursive(adjList, ref Mark, v, Exit, ref getPath, ref nGetPath, clsError, ref flag);
                }
                else
                    if (v == Exit)
                    {
                        //Get the trace from START to EXIT
                        //getPath[nGetPath] = v;
                        //nGetPath++;
                        //Check real error
                        bool check_real = check_DFS_real_error(clsError, getPath, nGetPath, v);
                        if (check_real == false) flag = false;
                    }
            }
            //remove from path
            Mark[Entry] = false;
            nGetPath--;
        }

        public static bool check_DFS_real_error(GraphVariables.clsError clsError, int[] getPath, int nGetpath, int v)
        {
            bool check_real = true;
            for (int i = 0; i < nGetpath; i++) //remove last node as error
            {
                for (int j = 0; j < clsError.nError; j++)
                {
                    if (!check_Error_Num(clsError, j)) continue;//filtering error
                    if (Convert.ToInt32(clsError.Error[j].Node) == v) continue;
                    if (getPath[i] == Convert.ToInt32(clsError.Error[j].Node))
                    {
                        check_real = false;                        
                    }
                }
                
            }
            return check_real;
        }

        public static int edges_in_Node_Set(GraphVariables.clsGraph graph, int currentN, int[] nodeSet, int nNodeSet)
        {
            int countEdges = 0;
            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                int fromNode = graph.Network[currentN].Link[i].fromNode;
                int toNode = graph.Network[currentN].Link[i].toNode;
                if (Node_In_Set(nodeSet, nNodeSet, fromNode) && Node_In_Set(nodeSet, nNodeSet, toNode))
                    countEdges++;
            }
            return countEdges;
        }

        public static void find_ConnectedComponents(GraphVariables.clsGraph graph, int baseNet, ref int CCs, ref int[] mark)
        {
            CCs = 0;
            int[,] A = new int[1, 1]; //just place holder
            Ultilities.mappingGraph.to_adjacencyMatrix_Undirected(ref graph, baseNet, ref A, ref graph.Network[baseNet].nNode);

            mark = new int[graph.Network[baseNet].nNode]; //autofill = false;
            for (int i = 0; i < graph.Network[baseNet].nNode; i++)
            {
                if (mark[i] == 0)
                {
                    CCs++;
                    ConnectedComponent(graph, baseNet, i, ref mark, ref A, CCs);
                }
            }
        }
        public static void ConnectedComponent(GraphVariables.clsGraph graph, int baseNet, int header, ref int[] mark, ref int[,] A, int CCs)
        {
            if (mark[header] == 0)
            {
                mark[header] = CCs;
                for (int i = 0; i < graph.Network[baseNet].nNode; i++)
                {
                    if (A[header, i] == 1)
                    {
                        ConnectedComponent(graph, baseNet, i, ref mark, ref A, CCs);
                    }
                }
            }
        }

        public static void DFS_Recursive_Simple(int[][] adjList, ref bool[] Mark, int Entry, int Exit, ref int[] getPath, ref int nGetPath, ref int[] Temp_searchNode, ref int n_Temp_SearchNode)
        {
            //if (flag == false) return;  //if check error is not real => exit
            Stack stack = new Stack();
            Mark[Entry] = true;
            stack.Push(Entry);
            getPath[nGetPath] = Entry;
            nGetPath++;
            for (int i = 1; i <= adjList[Entry][0]; i++)
            {
                int v = adjList[Entry][i];
                if (!Mark[v] && !(Exit == v)) //v != Exit
                {
                    DFS_Recursive_Simple(adjList, ref Mark, v, Exit, ref getPath, ref nGetPath, ref Temp_searchNode, ref n_Temp_SearchNode);
                }
                else
                    if (Exit == v) //v == Exit
                    {
                        //Get the trace from START to EXIT
                        getPath[nGetPath] = v;
                        nGetPath++;
                        for (int k = 0; k < nGetPath; k++)
                        {
                            if (!Ultilities.checkGraph.Node_In_Set(Temp_searchNode, n_Temp_SearchNode, getPath[k]))
                            {
                                Temp_searchNode[n_Temp_SearchNode] = getPath[k];
                                n_Temp_SearchNode++;
                            }
                        }
                        //Check real error
                        //bool check_real = check_DFS_real_error(clsError, getPath, nGetPath, v);
                        //if (check_real == false) flag = false;
                    }
            }
            //remove from path
            Mark[Entry] = false;
            nGetPath--;
        }

        public static void DFS_Recursive_GetPathOnly(int[][] adjList, ref bool[] Mark, int Entry, int[] Exit, ref int[] getPath, ref int nGetPath, ref int[] searchNode, ref int nSearchNode)
        {
            getPath = new int[getPath.Length];
            nGetPath = 0;
            Mark = new bool[Mark.Length];
            int maxLength = searchNode.Length;
            for (int i = 0; i < Exit.Length; i++)
            {
                int[] temp_SearchNode = new int[maxLength];
                int nTemp_s = 0;
                DFS_Recursive_Simple(adjList, ref Mark, Entry, Exit[i], ref getPath, ref nGetPath, ref temp_SearchNode, ref nTemp_s);
                searchNode = add_two_array(searchNode, nSearchNode, temp_SearchNode, nTemp_s, ref nSearchNode);
            }

            //return searchNode & nSearchNode
        }

        public static void DFS_Recursive_GetPathOnly_ePdFlow(int[][] adjList, ref bool[] Mark, int Entry, int[] Exit, ref int[] getPath, ref int nGetPath, ref int[] searchNode, ref int nSearchNode)
        {
            //if (flag == false) return;  //if check error is not real => exit
            Stack stack = new Stack();
            Mark[Entry] = true;
            stack.Push(Entry);
            getPath[nGetPath] = Entry;
            nGetPath++;
            for (int i = 1; i <= adjList[Entry][0]; i++)
            {
                int v = adjList[Entry][i];
                if (!Mark[v] && !Node_In_Set(Exit, Exit.Length, v)) //v != Exit
                {
                    DFS_Recursive_GetPathOnly_ePdFlow(adjList, ref Mark, v, Exit, ref getPath, ref nGetPath, ref searchNode, ref nSearchNode);
                }
                else
                    if (Node_In_Set(Exit, Exit.Length, v)) //v == Exit
                    {
                        //Get the trace from START to EXIT
                        getPath[nGetPath] = v;
                        nGetPath++;
                        for (int k = 0; k < nGetPath; k++)
                        {
                            if (!Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, getPath[k]))
                            {
                                searchNode[nSearchNode] = getPath[k];
                                nSearchNode++;
                            }
                        }
                        //Check real error
                        //bool check_real = check_DFS_real_error(clsError, getPath, nGetPath, v);
                        //if (check_real == false) flag = false;
                    }
            }
            //remove from path
            Mark[Entry] = false;
            nGetPath--;
        }

        public static int[] add_two_array(int[] A, int nA, int[] B, int nB, ref int dummy_n)
        {
            int[] tempArr = new int[nA + nB];
            int nTemp = 0;

            for (int i = 0; i < nA; i++)
            {
                if (!Node_In_Set(tempArr, nTemp, A[i]))
                {
                    tempArr[nTemp] = A[i];
                    nTemp++;
                }
            }

            for (int i = 0; i < nB; i++)
            {
                if (!Node_In_Set(tempArr, nTemp, B[i]))
                {
                    tempArr[nTemp] = B[i];
                    nTemp++;
                }     
            }
            int[] newArr = new int[nTemp];
            for (int i = 0; i < nTemp; i++) newArr[i] = tempArr[i];
            dummy_n = nTemp;
            return newArr;
        }
    }
}
