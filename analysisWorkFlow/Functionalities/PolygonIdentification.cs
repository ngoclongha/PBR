using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace gProAnalyzer.Functionalities
{
    class PolygonIdentification
    {
        private gProAnalyzer.Ultilities.mappingGraph mapping;
        private gProAnalyzer.Ultilities.copySESE copySE;
        private gProAnalyzer.Ultilities.checkGraph check;
        private gProAnalyzer.Ultilities.findIntersection fndIntersect;

        public static int[][] adjList = null;
        public static int[] getPolygons;
        public static int nPolygons = 0;
        public static int firstNode;
        public static int curDepth;
        public static bool[] Mark;

        private static void Initialize_All(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE)
        {
            //mapping = new gProAnalyzer.Ultilities.mappingGraph();
            //copySE = new gProAnalyzer.Ultilities.copySESE();
            //check = new gProAnalyzer.Ultilities.checkGraph();
            //fndIntersect = new gProAnalyzer.Ultilities.findIntersection();


            gProAnalyzer.Ultilities.mappingGraph.to_adjList_Directed(ref graph, currentN, ref adjList);
            Mark = new bool[graph.Network[currentN].nNode];
            Mark = Array.ConvertAll<bool, bool>(Mark, b => b = false);
            curDepth = clsSESE.SESE[currentSESE].maxDepth;

            getPolygons = new int[graph.Network[currentN].nNode * 2];
            nPolygons = 0;

            gProAnalyzer.Ultilities.copySESE.copy_SESE(ref clsSESE, currentSESE, clsSESE.tempSESE);
        }

        public static void polygonIdentification(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE)
        {
            Initialize_All(ref graph, currentN, ref clsSESE, currentSESE);
            //output => polygon which contain parentSESE information

            //convert model to adjList[][]
            do
            {
                for (int i = 0; i < clsSESE.SESE[currentSESE].nSESE; i++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[i].depth == curDepth)
                    {
                        int curEntry = clsSESE.SESE[currentSESE].SESE[i].Entry;
                        int curExit = clsSESE.SESE[currentSESE].SESE[i].Exit;
                        if (i == 3)
                        { }
                        //reduce all nested SESE in adjList
                        for (int k = 0; k < clsSESE.SESE[currentSESE].SESE[i].nChild; k++)
                        {
                            int child = clsSESE.SESE[currentSESE].SESE[i].child[k];
                            int en = clsSESE.SESE[currentSESE].SESE[child].Entry;
                            int ex = clsSESE.SESE[currentSESE].SESE[child].Exit;
                            adjList[en] = new int[2];
                            adjList[en][0] = 1;
                            adjList[en][1] = ex;
                            for (int z = 0; z < clsSESE.SESE[currentSESE].SESE[child].nNode; z++)
                            {
                                int temp_node = clsSESE.SESE[currentSESE].SESE[child].Node[z];
                                Mark[temp_node] = true;
                                if ((temp_node == clsSESE.SESE[currentSESE].SESE[child].Entry) || (temp_node == clsSESE.SESE[currentSESE].SESE[child].Exit))
                                    Mark[temp_node] = false;
                            }
                            int[] temp = new int[graph.Network[currentN].Node[ex].nPost];
                            int nTemp = 0;
                            for (int w = 0; w < graph.Network[currentN].Node[ex].nPost; w++)
                            {
                                if (Mark[graph.Network[currentN].Node[ex].Post[w]] == false)
                                {
                                    temp[nTemp] = graph.Network[currentN].Node[ex].Post[w];
                                    nTemp++;
                                }
                            }
                            adjList[ex][0] = nTemp;
                            for (int w = 0; w < nTemp; w++)
                                adjList[ex][w + 1] = temp[w];

                        }
                        //DFS_Polygon()
                        for (int j = 0; j < graph.Network[currentN].Node[curEntry].nPost; j++)
                        {
                            if (i == 3)
                            { }
                            getPolygons = new int[graph.Network[currentN].nNode * 2];
                            nPolygons = 0;
                            firstNode = graph.Network[currentN].Node[curEntry].Post[j];
                            int after_node = get_post_exitSESE(ref graph, currentN, ref clsSESE, currentSESE, i, curExit);
                            DFS_Polygon(adjList, ref Mark, firstNode, curEntry, after_node, ref getPolygons, ref nPolygons); //=> 0011110000111001110001110 sample
                            if (nPolygons > 1)
                            {
                                //process polygon
                                get_calSESE_Polygon(getPolygons, nPolygons, ref graph, currentN, ref clsSESE, clsSESE.tempSESE, curEntry, curExit, curDepth, i);
                            }
                        }
                        Mark = Array.ConvertAll<bool, bool>(Mark, b => b = false);
                    }
                }
                curDepth--;
            }
            while (curDepth > 0);

            for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++)
            {
                if (clsSESE.SESE[currentSESE].SESE[k].depth == 1)
                {
                    int en = clsSESE.SESE[currentSESE].SESE[k].Entry;
                    int ex = clsSESE.SESE[currentSESE].SESE[k].Exit;

                    adjList[en] = new int[2];
                    adjList[en][0] = 1;
                    adjList[en][1] = ex;
                    for (int z = 0; z < clsSESE.SESE[currentSESE].SESE[k].nNode; z++)
                    {
                        Mark[clsSESE.SESE[currentSESE].SESE[k].Node[z]] = true;
                        if ((clsSESE.SESE[currentSESE].SESE[k].Node[z] == clsSESE.SESE[currentSESE].SESE[k].Entry) || 
                            (clsSESE.SESE[currentSESE].SESE[k].Node[z] == clsSESE.SESE[currentSESE].SESE[k].Exit))
                            Mark[clsSESE.SESE[currentSESE].SESE[k].Node[z]] = false;
                    }
                    int[] temp = new int[graph.Network[currentN].Node[ex].nPost];
                    int nTemp = 0;
                    for (int w = 0; w < graph.Network[currentN].Node[ex].nPost; w++)
                    {
                        if (Mark[graph.Network[currentN].Node[ex].Post[w]] == false)
                        {
                            temp[nTemp] = graph.Network[currentN].Node[ex].Post[w];
                            nTemp++;
                        }
                    }
                    adjList[ex][0] = nTemp;
                    for (int w = 0; w < nTemp; w++)
                        adjList[ex][w + 1] = temp[w];
                }
            }
            //find polygon outside from START to END
            int START_plg = -1;
            int END_plg = -1;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].Kind == "START") START_plg = i;
                if (graph.Network[currentN].Node[i].Kind == "END") END_plg = i;
            }
            getPolygons = new int[graph.Network[currentN].nNode * 2];
            nPolygons = 0;
            firstNode = START_plg;
            DFS_Polygon(adjList, ref Mark, firstNode, START_plg, END_plg, ref getPolygons, ref nPolygons); //=> 0011110000111001110001110 sample
            getPolygons[nPolygons - 1] = END_plg; //modify
            getPolygons[nPolygons] = -1; //modify
            nPolygons++; //modify
            if (nPolygons > 1)
            {
                get_calSESE_Polygon(getPolygons, nPolygons, ref graph, currentN, ref clsSESE, clsSESE.tempSESE, -1, -1, curDepth, -1);
            }

            //update the hierarchy of SESE (copy SESE from tempSESE to currentSESE)
            gProAnalyzer.Ultilities.copySESE.copy_SESE(ref clsSESE, clsSESE.tempSESE, currentSESE);
            make_SESE_hierarchy(ref graph, currentN, ref clsSESE, currentSESE);
        }

        public static void make_SESE_hierarchy(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE)
        {
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[iSE].nNode >= clsSESE.SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = gProAnalyzer.Ultilities.findIntersection.find_Intersection(clsSESE.SESE[currentSESE].SESE[jSE].nNode, clsSESE.SESE[currentSESE].SESE[iSE].Node, clsSESE.SESE[currentSESE].SESE[jSE].Node);

                    if (gProAnalyzer.Ultilities.checkGraph.check_SameSet(calSESE, clsSESE.SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (clsSESE.SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (clsSESE.SESE[currentSESE].SESE[jSE].nNode > clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = clsSESE.SESE[currentSESE].SESE[iSE].parentSESE;
                        }
                        clsSESE.SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            int max_Depth = 0;
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (clsSESE.SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = clsSESE.SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[clsSESE.SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                clsSESE.SESE[currentSESE].SESE[iSE].depth = depth;
                clsSESE.SESE[currentSESE].SESE[iSE].nChild = cntFind;
                clsSESE.SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) clsSESE.SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            clsSESE.SESE[currentSESE].maxDepth = max_Depth;
            //================================
            //modify SESE (SESE[currentSESE].SESE[i].parentSESE);
            //modify_SESE_Hierarchy(currentSESE);
            //================================
            for (int i = 0; i < clsSESE.SESE[currentSESE].nSESE; i++) clsSESE.SESE[currentSESE].SESE[i].parentSESE = -1;

            // Make hierarchy
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[iSE].nNode >= clsSESE.SESE[currentSESE].SESE[jSE].nNode) continue;

                    int[] calSESE = gProAnalyzer.Ultilities.findIntersection.find_Intersection(clsSESE.SESE[currentSESE].SESE[jSE].nNode, clsSESE.SESE[currentSESE].SESE[iSE].Node, clsSESE.SESE[currentSESE].SESE[jSE].Node);

                    if (gProAnalyzer.Ultilities.checkGraph.check_SameSet(calSESE, clsSESE.SESE[currentSESE].SESE[iSE].Node)) // iSE가 jSE의 child면 // check the same of calSESE (after intersection) and iSE
                    {
                        //until here, we have just dealed with the .parentSESE
                        int pSE = jSE;
                        if (clsSESE.SESE[currentSESE].SESE[iSE].parentSESE != -1)
                        {
                            if (clsSESE.SESE[currentSESE].SESE[jSE].nNode > clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].SESE[iSE].parentSESE].nNode)
                                pSE = clsSESE.SESE[currentSESE].SESE[iSE].parentSESE;
                        }

                        clsSESE.SESE[currentSESE].SESE[iSE].parentSESE = pSE;
                    }
                }
            }
            //========================
            max_Depth = 0;
            for (int iSE = 0; iSE < clsSESE.SESE[currentSESE].nSESE; iSE++)
            {
                if (iSE == 17)
                {
                    //int stop;
                }
                //find Depth
                int depth = 1;
                int cSE = iSE;
                do
                {
                    if (clsSESE.SESE[currentSESE].SESE[cSE].parentSESE == -1) break;
                    cSE = clsSESE.SESE[currentSESE].SESE[cSE].parentSESE;
                    depth++;
                } while (true);

                //find child
                int cntFind = 0;
                int[] get_SESE = new int[clsSESE.SESE[currentSESE].nSESE]; //store a child of iSE SESE

                for (int jSE = 0; jSE < clsSESE.SESE[currentSESE].nSESE; jSE++)
                {
                    if (clsSESE.SESE[currentSESE].SESE[jSE].parentSESE == iSE)
                    {
                        get_SESE[cntFind] = jSE;
                        cntFind++;
                    }
                }
                clsSESE.SESE[currentSESE].SESE[iSE].depth = depth;
                clsSESE.SESE[currentSESE].SESE[iSE].nChild = cntFind;
                clsSESE.SESE[currentSESE].SESE[iSE].child = new int[cntFind];
                for (int k = 0; k < cntFind; k++) clsSESE.SESE[currentSESE].SESE[iSE].child[k] = get_SESE[k];

                if (depth > max_Depth) max_Depth = depth;
            }
            clsSESE.SESE[currentSESE].maxDepth = max_Depth;
        }

        public static int get_post_exitSESE(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int sese, int node)
        {
            if (graph.Network[currentN].Node[node].nPost > 1)
            {
                for (int i = 0; i < graph.Network[currentN].Node[node].nPost; i++)
                {
                    if (node_insideSESE(clsSESE.SESE[currentSESE].SESE[sese].Node, graph.Network[currentN].Node[node].Post[i]) == false) return graph.Network[currentN].Node[node].Post[i];
                }
                //node_insideSESE(int[] calSESE, int node)
            }
            else
                if (graph.Network[currentN].Node[node].nPost == 1)
                    return graph.Network[currentN].Node[node].Post[0];
            return -1;
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

        public static void get_calSESE_Polygon(int[] getPolygons, int nPolygons, ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, 
            ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int nodeD, int nodeR, int depth, int parentSESE)
        {
            bool flag = true;
            int[] tempList = new int[nPolygons];
            int[] tempLabel = new int[nPolygons];
            int count_temp = 0;
            for (int i = 0; i < nPolygons; i++)
            {
                if (flag)
                {
                    tempList = new int[nPolygons];
                    tempLabel = new int[nPolygons];
                    count_temp = 0;
                }
                int node_type = type_Node(ref graph, currentN, ref clsSESE, currentSESE, getPolygons[i], nodeD, nodeR);
                if (node_type > 0)
                {
                    tempList[count_temp] = getPolygons[i];
                    tempLabel[count_temp] = node_type;
                    count_temp++;
                    if (i < (nPolygons - 1))
                    {
                        if (type_Node(ref graph, currentN, ref clsSESE, currentSESE, getPolygons[i + 1], nodeD, nodeR) > 0) flag = false;
                        else flag = true;
                    }
                    else flag = true;
                }
                else flag = true; //cut 01110001 string and extract a polygon

                if (flag == true && count_temp > 1)
                {
                    int[] calSESE = new int[graph.Network[currentN].nNode];
                    int curIndex = 0;
                    //extend calSESE
                    int[][] listSESE = new int[clsSESE.SESE[currentSESE].nSESE][];
                    int nListSESE = 0;
                    bool isOK = false;
                    for (int k = 0; k < count_temp; k++)
                    {
                        if (tempLabel[k] == 2 && (k + 1) < count_temp)
                            if (tempLabel[k + 1] == 3)
                            {
                                listSESE[nListSESE] = new int[2];
                                listSESE[nListSESE][0] = tempList[k];
                                listSESE[nListSESE][1] = tempList[k + 1];
                                nListSESE++;
                            }
                        if (tempLabel[k] == 1)
                        {
                            calSESE[curIndex] = tempList[k];
                            curIndex++;
                            isOK = true;
                        }
                    }
                    for (int k = 0; k < nListSESE; k++)
                    {
                        int en, ex;
                        en = listSESE[k][0];
                        ex = listSESE[k][1];
                        int seseIndex = getSESE_index(ref clsSESE, currentSESE, en, ex);
                        if (seseIndex > -1)
                        {
                            clsSESE.SESE[currentSESE].SESE[seseIndex].Node.CopyTo(calSESE, curIndex);
                            curIndex = curIndex + clsSESE.SESE[currentSESE].SESE[seseIndex].nNode;
                        }
                    }
                    int[] old_calSESE = new int[curIndex];
                    for (int k = 0; k < curIndex; k++) old_calSESE[k] = calSESE[k];
                    calSESE = new int[curIndex];
                    for (int k = 0; k < curIndex; k++) calSESE[k] = old_calSESE[k];
                    if (nListSESE > 1 || isOK)
                        add_SESE(ref clsSESE, currentSESE, tempList[0], tempList[count_temp - 1], calSESE, (depth + 1), parentSESE);
                }
            }
        }
        public static int getSESE_index(ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int entry, int exit)
        {
            for (int i = 0; i < clsSESE.SESE[currentSESE].nSESE; i++)
            {
                if (clsSESE.SESE[currentSESE].SESE[i].Entry == entry && clsSESE.SESE[currentSESE].SESE[i].Exit == exit)
                    return i;
            }
            return -1;
        }
        public static int type_Node(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int node, int nodeD, int nodeR) // return 1 for TASK/EVENT, return 2 for ENTRY SESE, return 3 for EXIT SESE
        {
            if (node != -1)
            {
                if (node == nodeD || node == nodeR) return 0;
                if (graph.Network[currentN].Node[node].nPre > 1 || graph.Network[currentN].Node[node].nPost > 1)
                {
                    for (int i = 0; i < clsSESE.SESE[currentSESE].nSESE; i++)
                    {
                        if (node == clsSESE.SESE[currentSESE].SESE[i].Entry) return 2;
                        if (node == clsSESE.SESE[currentSESE].SESE[i].Exit) return 3;
                    }
                }
                else
                    if (graph.Network[currentN].Node[node].Kind == "TASK" || graph.Network[currentN].Node[node].Kind == "EVENT" || 
                        graph.Network[currentN].Node[node].Kind == "START" || graph.Network[currentN].Node[node].Kind == "END")
                        return 1;
            }
            return 0;
        }

        public static void add_SESE(ref gProAnalyzer.GraphVariables.clsSESE clsSESE, int currentSESE, int nodeD, int nodeR, int[] calSESE, int depth, int parentSESE)
        {

            //final checking rigid and add more SESE (1 or more rigids inside)            
            GraphVariables.clsSESE.strSESEInform[] oldSESE = new GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE];
            for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) oldSESE[k] = clsSESE.SESE[currentSESE].SESE[k];

            clsSESE.SESE[currentSESE].SESE = new GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[currentSESE].nSESE + 1];
            for (int k = 0; k < clsSESE.SESE[currentSESE].nSESE; k++) clsSESE.SESE[currentSESE].SESE[k] = oldSESE[k];

            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].depth = depth;
            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].parentSESE = parentSESE;
            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Entry = nodeD;
            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Exit = nodeR;
            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].nNode = calSESE.Length;
            clsSESE.SESE[currentSESE].SESE[clsSESE.SESE[currentSESE].nSESE].Node = calSESE;

            clsSESE.SESE[currentSESE].nSESE++;
        }
        public static void DFS_Polygon(int[][] adjList, ref bool[] Mark, int firstNode, int Entry, int Exit, ref int[] getRigids, ref int nRigids)
        {
            Stack stack = new Stack();
            Mark[firstNode] = true;
            stack.Push(firstNode);
            //getRigids[nRigids] = firstNode;
            //nRigids++;
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());
                //do nothing
                getRigids[nRigids] = curNode;
                nRigids++;

                //do something!!
                if (curNode != -1)
                {
                    for (int i = 1; i <= adjList[curNode][0]; i++)
                    {
                        int v = adjList[curNode][i];

                        if (!Mark[v] && v != Entry && v != Exit)
                        {
                            stack.Push(v);
                            Mark[v] = true;

                            //if v is not gateway => marking 1
                            //if v is SESE_entry => marking 2, SESE_exit => marking 3                        
                        }
                        else //put break branch point
                        {
                            stack.Push(-1);
                        }
                    }
                }
            }
        }
    }
}
