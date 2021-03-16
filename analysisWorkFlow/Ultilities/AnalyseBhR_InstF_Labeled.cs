using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class AnalyseBhR_InstF_Labeled
    {
        public static int[][] adjList = null;

        private gProAnalyzer.Ultilities.mappingGraph mapping;
        private gProAnalyzer.Ultilities.makeInstanceFlow makInst;

        public static void Initialize_All(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN)
        {
            //makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
            //mapping = new gProAnalyzer.Ultilities.mappingGraph();
            gProAnalyzer.Ultilities.mappingGraph.to_adjList_bInstance(ref graph, currentN, ref adjList);
            //makInst = new gProAnalyzer.Ultilities.makeInstanceFlow();
        }

        public static void check_InstanceBehavior(GraphVariables.clsGraph graph, int currentN, int x, int y, ref bool[] returnBhPrfl, bool[] flag_Check)
        {
            Initialize_All(ref graph, currentN);

            //if want to manipulate for experiment of computation time for each Relation => Switch to TRUE to DISABLE
            bool totalConcurrent = false;
            bool existConcurrent = false;
            bool totalCausal = false;
            bool existCausal = false;
            bool canConflict = false;
            bool NotcanConflict = false;
            bool canCoocur = false;
            bool notCancoocur = false;

            int[] SearchXOR = new int[graph.Network[currentN].nNode];
            int nSearchXOR = 0;
            int nCurrentXOR = 0;
            int nInst = 0;

            int x_cc = 0;
            int y_cc = 0;
            int x_cs = 0;
            int y_cs = 0;

            DateTime stTime = new DateTime(); stTime = DateTime.Now; double Run_Times = 0;
            do
            {
                nCurrentXOR = 0;
                for (int j = 0; j < graph.Network[currentN].nLink; j++) graph.Network[currentN].Link[j].bInstance = false;

                int[] InstantNode = new int[graph.Network[currentN].nNode];
                int nInstantNode = 0;
                string[] strCombination_OR = new string[graph.Network[currentN].nNode];
                int sNode = graph.Network[currentN].header;
                int dm_END = graph.Network[currentN].nNode - 2; //virtual End index in MakeSubgraph
                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                if (gProAnalyzer.Ultilities.makeInstanceFlow.find_InstanceNode(ref graph, currentN, sNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR))
                {
                    string[] readable = new string[nInstantNode];
                    //readable = convert_Readable(ref graph, currentN, InstantNode, nInstantNode);                 

                    //====== analyzer for each instance ======

                    //check    (exist conflict)
                    if (canConflict == false && flag_Check[4] == false)
                        canConflict = check_CanConflict(InstantNode, nInstantNode, x, y, dm_END);
                    //check not(exist conflict)
                    if (NotcanConflict == false && flag_Check[5] == false)
                        NotcanConflict = check_NotCanConflict(InstantNode, nInstantNode, x, y);

                    //check    (exist coocur)
                    if (canCoocur == false && flag_Check[6] == false)
                        canCoocur = check_CanCooccur(InstantNode, nInstantNode, x, y);
                    //check not(exist coocur)
                    if (notCancoocur == false && flag_Check[7] == false)
                        notCancoocur = check_NotCanCooccur(InstantNode, nInstantNode, x, y, dm_END);

                    //check (exist causal)&&
                    //check (total causal)
                    if (check_Causal(graph, currentN, InstantNode, nInstantNode, ref adjList, x, y) && (flag_Check[2] == false || flag_Check[3] == false))
                    {
                        x_cs++;
                        y_cs++;
                    }

                    //check (exist concurrent) &&
                    //check (total concurrent)                    
                    if (check_Concurrency(graph, currentN, InstantNode, nInstantNode, ref adjList, x, y) && (flag_Check[0] == false || flag_Check[1] == false))
                    {
                        x_cc++;
                        y_cc++;
                    }

                    if (nInst == 100) //Testing
                    { }

                    //not optimize yet => just for check of unique trigger of each realtion (not for combination of 2 or 3 .. relation as the same time)
                    if (canConflict && flag_Check[4] == false && flag_Check[6] == true) break; //just workaround, it mean all relation not be executed => single only
                    if (NotcanConflict && flag_Check[5] == false && flag_Check[6] == true) break;
                    if (canCoocur && flag_Check[6] == false && flag_Check[4] == true) break;
                    if (notCancoocur && flag_Check[7] == false && flag_Check[4] == true) break;
                    if (x_cs > 0 && y_cs > 0 && flag_Check[3] == false && flag_Check[1] == true) break;
                    if (x_cc > 0 && y_cc > 0 && flag_Check[1] == false && flag_Check[3] == true) break;

                    nInst++;
                }
            } while (nSearchXOR > 0);

            if (nInst == x_cc && nInst == y_cc)
            {
                totalConcurrent = true;
                existConcurrent = true;
            }
            else
                if (x_cc > 0 && y_cc > 0)
                    existConcurrent = true;

            if (nInst == x_cs && nInst == y_cs)
            {
                totalCausal = true;
                existCausal = true;
            }
            else
                if (x_cs > 0 && y_cs > 0)
                    existCausal = true;

            Run_Times = (DateTime.Now - stTime).TotalSeconds;
            //MessageBox.Show(nInst.ToString() + " in " + Run_Times.ToString() + "seconds", "Number of Instance");

            returnBhPrfl[0] = totalConcurrent;
            returnBhPrfl[1] = existConcurrent;
            returnBhPrfl[2] = totalCausal;
            returnBhPrfl[3] = existCausal;
            returnBhPrfl[4] = canConflict;
            returnBhPrfl[5] = NotcanConflict;
            returnBhPrfl[6] = canCoocur;
            returnBhPrfl[7] = notCancoocur;
        }

        //#1
        public static bool check_CanConflict(int[] InstantNode, int nInstantNode, int x, int y, int dm_END)
        {
            bool x_exist = false;
            bool y_exist = false;
            bool checkEnd = false;
            for (int i = 0; i < nInstantNode; i++)
            {
                if (InstantNode[i] == x) x_exist = true;
                if (InstantNode[i] == y) y_exist = true;
                if (InstantNode[i] == dm_END) checkEnd = true;
            }
            if (x_exist == true && y_exist == false && checkEnd == true)
                return true;
            return false; //it cover the case notCanConflict??
        }

        //#2
        public static bool check_NotCanConflict(int[] InstantNode, int nInstantNode, int x, int y)
        {
            //bool a = check_CanConflict(InstantNode, nInstantNode, y, x);
            bool b = check_CanCooccur(InstantNode, nInstantNode, x, y);
            //if (a == true && b == true)
            //  return true;
            if (b == true)
                return true;
            return false;
        }

        //#3
        public static bool check_CanCooccur(int[] InstantNode, int nInstantNode, int x, int y)
        {
            bool x_exist = false;
            bool y_exist = false;
            for (int i = 0; i < nInstantNode; i++)
            {
                if (InstantNode[i] == x) x_exist = true;
                if (InstantNode[i] == y) y_exist = true;
            }
            if (x_exist == true && y_exist == true)
                return true;
            return false;
        }

        //#4
        public static bool check_NotCanCooccur(int[] InstantNode, int nInstantNode, int x, int y, int dm_End)
        {
            bool a = check_CanConflict(InstantNode, nInstantNode, x, y, dm_End);
            bool b = check_CanConflict(InstantNode, nInstantNode, y, x, dm_End);
            if (a == true && b == true)
                return true;
            return false;
        }

        //#5
        public static bool check_Causal(GraphVariables.clsGraph graph, int currentN, int[] InstantNode, int nInstantNode, ref int[][] adjList, int x, int y)
        {
            int[] getPath = new int[nInstantNode];
            int nGetPath = 0;
            bool[] Mark = new bool[graph.Network[currentN].nNode];
            gProAnalyzer.Ultilities.mappingGraph.to_adjList_bInstance(ref graph, currentN, ref adjList); //We already buil an instance subgraph based on InstanceNode !! Huaahhh~

            DFS(adjList, ref Mark, x, ref getPath, ref nGetPath);
            for (int i = 0; i < nGetPath; i++)
            {
                if (getPath[i] == y) return true;
            }
            return false;
        }

        //#6
        public static bool check_Concurrency(GraphVariables.clsGraph graph, int currentN, int[] InstantNode, int nInstantNode, ref int[][] adjList, int x, int y)
        {
            //not  causal relation
            //not conflict relation
            bool causal_x_y = check_Causal(graph, currentN, InstantNode, nInstantNode, ref adjList, x, y);
            bool causal_y_x = check_Causal(graph, currentN, InstantNode, nInstantNode, ref adjList, y, x);
            bool CanCooccur = check_CanCooccur(InstantNode, nInstantNode, x, y);

            if (!causal_x_y && !causal_y_x && CanCooccur)
                return true;
            return false;
        }

        public static void DFS(int[][] adjList, ref bool[] Mark, int Entry, ref int[] getPath, ref int nGetPath)
        {
            Stack stack = new Stack();
            Mark[Entry] = true;
            stack.Push(Entry);
            while (stack.Count > 0)
            {
                int curNode = Convert.ToInt32(stack.Pop());
                //do nothing
                for (int i = 1; i <= adjList[curNode][0]; i++)
                {
                    int v = adjList[curNode][i];

                    if (!Mark[v])
                    {
                        stack.Push(v);
                        Mark[v] = true;
                        getPath[nGetPath] = v;
                        nGetPath++;
                    }
                }
            }
        }
    }
}
