using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer.Ultilities
{
    // This function input an acyclic subgraph and then find all instance flow
    class makeInstanceFlow
    {
        public static int[] SearchXOR;
        public static int nSearchXOR = 0;
        public static int nCurrentXOR = 0;
        public static int nInst = 0;

        //private gProAnalyzer.Ultilities.checkGraph checkG;

        public static void Initialize_All()
        {
            //checkG = new gProAnalyzer.Ultilities.checkGraph();
        }

        //MAIN()
        public static int make_InstanceFlow(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int loop, string errType, string strLoop, ref gProAnalyzer.GraphVariables.clsError clsError) //loop = -1 => final-Acyclic ; loop = -2 => SESE (new)
        {
            SearchXOR = new int[graph.Network[currentN].nNode]; // 0-탐색
            nSearchXOR = 0;
            nCurrentXOR = 0;
            nInst = 0;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            do
            {
                nCurrentXOR = 0;
                for (int j = 0; j < graph.Network[currentN].nLink; j++) //fill all this subnetwork is un-visit
                {
                    graph.Network[currentN].Link[j].bInstance = false; //bInstance is used for mark the node which have token (instance flow)
                }

                int[] InstantNode = new int[graph.Network[currentN].nNode];
                int nInstantNode = 0;
                string[] strCombination_OR = new string[graph.Network[currentN].nNode]; //for indexing the current propagation of OR split e.g., which successors are activated.
                //int nOR_SplitIndex = 0;

                int sNode = graph.Network[currentN].header;
                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                if (find_InstanceNode(ref graph, currentN, sNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR)) //find_InstanceNode() => output will be stored in InstantNode[] (Global variable!) - all the node which is activated in currentN.
                {
                    string[] readable = new string[nInstantNode];
                    readable = convert_Readable(ref graph, currentN, InstantNode, nInstantNode);
                    //Make_InstantNetwork(currentN, dummyNet); // 삭제 ??????????????

                    check_InstanceFlow(ref graph, currentN, loop, errType, strLoop, ref InstantNode, ref nInstantNode, ref clsError);  //bFor = true; => Forward// bFor = false; => Backward error reporting. (Errors[])
                    if (nInst == 100)
                    {
                    }

                    nInst++;
                }
            } while (nSearchXOR > 0);
            
            //MessageBox.Show(nInst.ToString() + " in " + watch.ElapsedMilliseconds.ToString() + "miliseconds", "Number of Instance");            
            return nInst;
        }

        public static string[] convert_Readable(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int[] InstantNode, int nInstantNode)
        {
            string[] readable = new string[nInstantNode];

            for (int i = 0; i < nInstantNode; i++)
            {
                readable[i] = graph.Network[currentN].Node[InstantNode[i]].Name;
            }
            return (readable);
        }

        public static bool find_InstanceNode(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int sNode, ref int[] InstantNode, ref int nInstantNode,
            ref int[] SearchXOR, ref int nSearchXOR, ref int nCurrentXOR, ref string[] strCombination_OR)// bool conAND // OR를 AND로 간주 - Concurrency Check 시
        {
            bool retBool = true;
            int tNode;

            if (graph.Network[currentN].Node[sNode].Kind == "OR" && graph.Network[currentN].Node[sNode].nPost > 1)
            {
                int num = 0;
                if (nCurrentXOR < nSearchXOR - 1)
                {
                    num = SearchXOR[nCurrentXOR];
                }
                else if (nCurrentXOR == nSearchXOR - 1)
                {
                    //num += 1;
                    SearchXOR[nCurrentXOR] += 1;
                    num = SearchXOR[nCurrentXOR];

                    if (SearchXOR[nCurrentXOR] >= Math.Pow(2, graph.Network[currentN].Node[sNode].nPost) - 1) //(2^n -1) case 
                    {
                        num = -1;
                        nSearchXOR--;
                        retBool = false;
                    }
                }
                else
                {
                    num = 0;
                    SearchXOR[nSearchXOR] = 0;
                    nSearchXOR++;
                }

                if (num >= 0)
                {
                    nCurrentXOR++;

                    string strCombination = Convert.ToString(num + 1, 2).PadLeft(graph.Network[currentN].Node[sNode].nPost, '0'); //đệm vào bên trái nPost số "0"

                    //Store strCombination in a parallel array with InstantNode with index is A[nInstantNode-1] = strCombination;
                    strCombination_OR[nInstantNode - 1] = strCombination;

                    for (int i = 0; i < graph.Network[currentN].Node[sNode].nPost; i++)
                    {
                        if (strCombination.Substring(i, 1) == "0") continue; //use binary string 000101010.. if curren index of string is 0 => no instance case => continue

                        tNode = graph.Network[currentN].Node[sNode].Post[i];

                        bool bSame = false;
                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (InstantNode[k] == tNode)
                            {
                                bSame = true;
                                break;
                            }
                        }
                        for (int j = 0; j < graph.Network[currentN].nLink; j++)
                        {
                            if (graph.Network[currentN].Link[j].fromNode == sNode && graph.Network[currentN].Link[j].toNode == tNode)
                            {
                                graph.Network[currentN].Link[j].bInstance = true;
                                break;
                            }
                        }

                        if (!bSame)
                        {
                            InstantNode[nInstantNode] = tNode;
                            nInstantNode++;

                            retBool = find_InstanceNode(ref graph, currentN, tNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR);
                            if (!retBool) return retBool;
                        }
                    }
                }
            }
            else if (graph.Network[currentN].Node[sNode].Kind == "XOR" && graph.Network[currentN].Node[sNode].nPost > 1)
            {
                int num = 0;
                if (nCurrentXOR < nSearchXOR - 1)
                {
                    num = SearchXOR[nCurrentXOR];
                }
                else if (nCurrentXOR == nSearchXOR - 1)
                {
                    //num += 1;
                    SearchXOR[nCurrentXOR] += 1;
                    num = SearchXOR[nCurrentXOR];

                    if (SearchXOR[nCurrentXOR] >= graph.Network[currentN].Node[sNode].nPost)
                    {
                        num = -1;
                        nSearchXOR--;
                        retBool = false;
                    }
                }
                else
                {
                    num = 0;
                    SearchXOR[nSearchXOR] = 0;
                    nSearchXOR++;
                }
                if (num >= 0)
                {
                    nCurrentXOR++;

                    tNode = graph.Network[currentN].Node[sNode].Post[num];

                    bool bSame = false;
                    for (int k = 0; k < nInstantNode; k++)
                    {
                        if (InstantNode[k] == tNode)
                        {
                            bSame = true;
                            break;
                        }
                    }

                    for (int j = 0; j < graph.Network[currentN].nLink; j++)
                    {
                        if (graph.Network[currentN].Link[j].fromNode == sNode && graph.Network[currentN].Link[j].toNode == tNode)
                        {
                            graph.Network[currentN].Link[j].bInstance = true;
                            break;
                        }
                    }

                    if (!bSame)
                    {
                        InstantNode[nInstantNode] = tNode;
                        nInstantNode++;

                        retBool = find_InstanceNode(ref graph, currentN, tNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR);
                        if (!retBool) return retBool;
                    }
                }
            }
            else if (graph.Network[currentN].Node[sNode].nPost > 0)
            {
                if (graph.Network[currentN].Node[sNode].Kind == "AND" && graph.Network[currentN].Node[sNode].nPost > 1)
                    strCombination_OR[nInstantNode - 1] = "1".PadLeft(graph.Network[currentN].Node[sNode].nPost, '1');

                for (int i = 0; i < graph.Network[currentN].Node[sNode].nPost; i++)
                {
                    tNode = graph.Network[currentN].Node[sNode].Post[i];

                    bool bSame = false;
                    for (int k = 0; k < nInstantNode; k++)
                    {
                        if (InstantNode[k] == tNode)
                        {
                            bSame = true;
                            break;
                        }
                    }
                    for (int j = 0; j < graph.Network[currentN].nLink; j++)
                    {
                        if (graph.Network[currentN].Link[j].fromNode == sNode && graph.Network[currentN].Link[j].toNode == tNode)
                        {
                            graph.Network[currentN].Link[j].bInstance = true;
                            break;
                        }
                    }
                    if (!bSame)
                    {
                        

                        InstantNode[nInstantNode] = tNode;
                        nInstantNode++;

                        retBool = find_InstanceNode(ref graph, currentN, tNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR);
                        if (!retBool) return retBool;
                    }
                }
            }
            return retBool;
        }

        //might not belong to this class. 
        public static void check_InstanceFlow(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int loop, string errType, string strLoop, 
            ref int[] InstantNode, ref int nInstantNode, ref GraphVariables.clsError clsError)
        {            
            for (int i = 0; i < nInstantNode; i++)
            {
                //XOR Join에 여러 Instant In이면 error
                if (graph.Network[currentN].Node[InstantNode[i]].Kind == "XOR" && graph.Network[currentN].Node[InstantNode[i]].nPre > 1)
                {
                    int numIn = 0;
                    for (int j = 0; j < graph.Network[currentN].Node[InstantNode[i]].nPre; j++)
                    {
                        bool bLink = false;
                        for (int k = 0; k < graph.Network[currentN].nLink; k++)
                        {
                            if (graph.Network[currentN].Link[k].fromNode == graph.Network[currentN].Node[InstantNode[i]].Pre[j] && graph.Network[currentN].Link[k].toNode == InstantNode[i])
                            {
                                if (graph.Network[currentN].Link[k].bInstance) bLink = true;
                                break;
                            }
                        }
                        if (!bLink) continue;

                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (graph.Network[currentN].Node[InstantNode[i]].Pre[j] == InstantNode[k]) numIn++;
                        }
                    }

                    if (numIn > 1) //error
                    {
                        clsError.Error[clsError.nError].Loop = strLoop;
                        clsError.Error[clsError.nError].Node = graph.Network[currentN].Node[InstantNode[i]].parentNum.ToString();
                        clsError.Error[clsError.nError].currentKind = graph.Network[currentN].Node[InstantNode[i]].Kind;

                        if (loop == -1)
                        {
                            clsError.Error[clsError.nError].messageNum = 10;
                        }
                        else
                        {
                            if (errType == "SESE")
                            {
                                clsError.Error[clsError.nError].Loop = "";
                                clsError.Error[clsError.nError].SESE = strLoop;
                                clsError.Error[clsError.nError].messageNum = 27; //SESE lack of synchronization
                            }
                            if (errType == "eFwd")
                                if (clsError.Error[clsError.nError].Node == "-1")
                                    clsError.Error[clsError.nError].messageNum = 29; //parallel error
                                else
                                    clsError.Error[clsError.nError].messageNum = 3; //rule 2.1
                            if (errType == "eBwd") clsError.Error[clsError.nError].messageNum = 4; //rule 2.1
                            if (errType == "ePdFlow") clsError.Error[clsError.nError].messageNum = 24; //rule 6.1
                            if (errType == "eFwd_IL") clsError.Error[clsError.nError].messageNum = 25; //rule 6.2
                            if (errType == "eBwd_IL") clsError.Error[clsError.nError].messageNum = 26; //rule 6.3
                        }
                        //nError++;
                         gProAnalyzer.Ultilities.recordData.add_Error(ref clsError);
                    }
                }
                //AND Join에  Instant In이 부족하면 error //===========DEADLOCK============
                if (graph.Network[currentN].Node[InstantNode[i]].Kind == "AND" && graph.Network[currentN].Node[InstantNode[i]].nPre > 1)
                {
                    int numIn = 0;
                    for (int j = 0; j < graph.Network[currentN].Node[InstantNode[i]].nPre; j++)
                    {
                        bool bLink = false;
                        for (int k = 0; k < graph.Network[currentN].nLink; k++)
                        {
                            if (graph.Network[currentN].Link[k].fromNode == graph.Network[currentN].Node[InstantNode[i]].Pre[j] && graph.Network[currentN].Link[k].toNode == InstantNode[i])
                            {
                                if (graph.Network[currentN].Link[k].bInstance) bLink = true;
                                break;
                            }
                        }
                        if (!bLink) continue;

                        for (int k = 0; k < nInstantNode; k++)
                        {
                            if (graph.Network[currentN].Node[InstantNode[i]].Pre[j] == InstantNode[k]) numIn++;
                        }
                    }
                    if (numIn < graph.Network[currentN].Node[InstantNode[i]].nPre) //error
                    {
                        clsError.Error[clsError.nError].Loop = strLoop;
                        clsError.Error[clsError.nError].Node = graph.Network[currentN].Node[InstantNode[i]].parentNum.ToString();
                        clsError.Error[clsError.nError].currentKind = graph.Network[currentN].Node[InstantNode[i]].Kind;

                        if (loop == -1)
                        {
                            clsError.Error[clsError.nError].messageNum = 11;
                        }
                        else
                        {
                            if (errType == "SESE")
                            {
                                clsError.Error[clsError.nError].Loop = "";
                                clsError.Error[clsError.nError].SESE = strLoop;
                                clsError.Error[clsError.nError].messageNum = 28; //SESE deadlock 
                            }

                            if (errType == "eFwd") clsError.Error[clsError.nError].messageNum = 5;
                            if (errType == "eBwd") clsError.Error[clsError.nError].messageNum = 6;
                            //same for XOR errors
                            if (errType == "ePdFlow") clsError.Error[clsError.nError].messageNum = 37; //rule 6.1
                            if (errType == "eFwd_IL") clsError.Error[clsError.nError].messageNum = 38; //rule 6.2
                            if (errType == "eBwd_IL") clsError.Error[clsError.nError].messageNum = 39; //rule 6.3
                            //if (errType == "SESE") Error[nError].messageNum = 28;
                        }
                        //nError++;
                        gProAnalyzer.Ultilities.recordData.add_Error(ref clsError);
                    }
                }
            }
        }

        public int[] get_InstanceFlow(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsLoop clsLoop, int workLoop, int curLoop, int concurEn)
        {
            SearchXOR = new int[graph.Network[currentN].nNode]; // 0-탐색
            nSearchXOR = 0;
            nCurrentXOR = 0;
            nInst = 0;

            int[] searchNode = new int[graph.Network[currentN].nNode];
            int nSearchNode = 0;

            do {
                nCurrentXOR = 0;
                for (int j = 0; j < graph.Network[currentN].nLink; j++) //fill all this subnetwork is un-visit
                {
                    graph.Network[currentN].Link[j].bInstance = false; //bInstance is used for mark the node which have token (instance flow)
                }

                int[] InstantNode = new int[graph.Network[currentN].nNode];
                int nInstantNode = 0;
                string[] strCombination_OR = new string[graph.Network[currentN].nNode]; //for indexing the current propagation of OR split e.g., which successors are activated.
                //int nOR_SplitIndex = 0;

                int sNode = graph.Network[currentN].header;
                InstantNode[nInstantNode] = sNode;
                nInstantNode++;

                if (find_InstanceNode(ref graph, currentN, sNode, ref InstantNode, ref nInstantNode, ref SearchXOR, ref nSearchXOR, ref nCurrentXOR, ref strCombination_OR)) //find_InstanceNode() => output will be stored in InstantNode[] (Global variable!) - all the node which is activated in currentN.
                {
                    string[] readable = new string[nInstantNode];
                    nInst++;
                }

                //check whether current InstanceNode contains concurrentEntries or not
                int countE = 0;
                int countInst = 0;
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[curLoop].nConcurrency; i++) {
                    if (clsLoop.Loop[workLoop].Loop[curLoop].Concurrency[i] == concurEn) {
                        countE++;
                        for (int j = 0; j < nInstantNode; j++) {
                            if (graph.Network[currentN].Node[InstantNode[j]].orgNum == clsLoop.Loop[workLoop].Loop[curLoop].Entry[i]) {
                                countInst++;
                            }
                        }
                    }
                }
                //if current Inst contain cc_entries set => store it
                if (countE == countInst) {
                    //set Union of all instance node 
                    for (int j = 0; j < nInstantNode; j++) {
                        if (gProAnalyzer.Ultilities.checkGraph.Node_In_Set(searchNode, nSearchNode, InstantNode[j]) == false)
                        {
                            searchNode[nSearchNode] = InstantNode[j];
                            nSearchNode++;
                        }
                    }
                }
            } while (nSearchXOR > 0);

            int[] tempArr = new int[nSearchNode];
            for (int i = 0; i < nSearchNode; i++)
                tempArr[i] = searchNode[i];

            return tempArr;

            //cardinality = nInst;
        }
    }
}
