using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Functionalities
{
    class DominanceIdentification
    {
        private gProAnalyzer.Ultilities.findIntersection fndIntersect;
        private gProAnalyzer.Ultilities.checkGraph checkGraph;

        public static void Initialize_All()
        {
            //fndIntersect = new gProAnalyzer.Ultilities.findIntersection();
            //checkGraph = new gProAnalyzer.Ultilities.checkGraph();
        }

        public static void find_Dom(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN)
        {
            Initialize_All();
            //초기 Dom
            for (int i = 0; i < graph.Network[currentN].nNode; i++) //Visit all node in graph (index range from 0 to nNode)
            {
                if (graph.Network[currentN].Node[i].nPre == 0) //시작점 //Starting Point
                {
                    graph.Network[currentN].Node[i].nDom = 1;
                    graph.Network[currentN].Node[i].Dom = new int[1];
                    graph.Network[currentN].Node[i].Dom[0] = i; //If it is the first node => there is only its node dominate itself
                }
                else
                {
                    graph.Network[currentN].Node[i].nDom = 0;
                    graph.Network[currentN].Node[i].Dom = null; //initiating all Dom[] array by null and 0 for further evaluation
                }
            }
            bool change;
            do
            {
                change = false;
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    // i from 0 to Total number of Node
                    int[] calDom = null;
                    for (int k = 0; k < graph.Network[currentN].Node[i].nPre; k++)
                    {
                        //k from 0 to Total number of "nPre"
                        //find the intersection of (calDom[] and B[]) => return calDom[]
                        calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDom, graph.Network[currentN].Node[graph.Network[currentN].Node[i].Pre[k]].Dom); // calDOm = caDom (intersection) Dom(j)
                    }

                    if (calDom != null)
                    {
                        int[] newDom = new int[calDom.Length + 1];

                        for (int k = 0; k < calDom.Length; k++) newDom[k] = calDom[k]; //교집합
                        newDom[calDom.Length] = i; //자기자신 // Node i dominated its self
                        //Check same and transfer the result
                        if (!gProAnalyzer.Ultilities.checkGraph.check_SameSet(newDom, graph.Network[currentN].Node[i].Dom))
                        {
                            graph.Network[currentN].Node[i].Dom = newDom;

                            change = true;
                        }

                        graph.Network[currentN].Node[i].nDom = graph.Network[currentN].Node[i].Dom.Length;
                    }
                }

            } while (change);            
        }

        //Post Dominance (find_DomRev)
        public static void find_Pdom(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN)
        {
            Initialize_All();
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (graph.Network[currentN].Node[i].nPost == 0) 
                {
                    graph.Network[currentN].Node[i].nDomRev = 1;
                    graph.Network[currentN].Node[i].DomRev = new int[1];
                    graph.Network[currentN].Node[i].DomRev[0] = i;
                }
                else
                {
                    graph.Network[currentN].Node[i].nDomRev = 0;
                    graph.Network[currentN].Node[i].DomRev = null;
                }
            }
            bool change;
            do
            {
                change = false;
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    int[] calDom = null;
                    for (int k = 0; k < graph.Network[currentN].Node[i].nPost; k++)
                    {
                        calDom = gProAnalyzer.Ultilities.findIntersection.find_Intersection(graph.Network[currentN].nNode, calDom, graph.Network[currentN].Node[graph.Network[currentN].Node[i].Post[k]].DomRev);
                    }

                    if (calDom != null)
                    {
                        int[] newDom = new int[calDom.Length + 1];

                        newDom[0] = i; //자기자신
                        for (int k = 0; k < calDom.Length; k++) newDom[k + 1] = calDom[k]; //교집합


                        if (!gProAnalyzer.Ultilities.checkGraph.check_SameSet(newDom, graph.Network[currentN].Node[i].DomRev))
                        {
                            graph.Network[currentN].Node[i].DomRev = newDom;

                            change = true;
                        }
                        graph.Network[currentN].Node[i].nDomRev = graph.Network[currentN].Node[i].DomRev.Length;
                    }
                }

            } while (change);
        }

        // Extended Inverse Dominance eDom^-1 
        public static void find_DomEI(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, int nSS) // nSS = -1 이면 split node만.....아니면 nSS만
        {
            for (int k = 0; k < graph.Network[currentN].nNode; k++)
            {
                //if (nSS != -1 && nSS != k) continue; ????
                int cntFind = 0;
                int[] find_Node = new int[graph.Network[currentN].nNode]; //store Dom^-1 array find_Node[] was created, which length was nNode
                int[] calDom = null; //store the eDom^-1

                // Find Inverse Dominanators of a split Node K in here. (Dom^-1 (k))
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    bool isCon = false;
                    for (int j = 0; j < graph.Network[currentN].Node[i].nDom; j++)
                    {
                        if (k == graph.Network[currentN].Node[i].Dom[j]) //if node K existing in a set of node in Dom(i), then i is dominated by K
                        {
                            isCon = true;
                            break; // node inside Dom(i) have only 1 node = K, so break is unquestionable.
                        }
                    }
                    if (isCon)
                    {
                        find_Node[cntFind] = i; //find_Node used for saving all node which Node K dominate its.
                        cntFind++;
                    }

                }
                // extension => 
                int numCandidate = 0;
                int candidateNode = 0;
                int[] df = new int[graph.Network[currentN].nNode];
                int ndf = 0;
                // Search for all of entire set of Node in flow graph
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    //Filter a node which have at least 2 Predecessors
                    if (graph.Network[currentN].Node[i].nPre < 2) continue;  // Join 아니면
                    //Find dominance Frontier (DF)
                    bool isCon = false;
                    //search this node (have at least 2 predecessors), if this node (i) existing in Inverse Dominance Find_node => this is Dominance Frontier (DF)
                    for (int find = 0; find < cntFind; find++)
                    {
                        //If node i belong to Inverse Dominance (k) - eDom^-1 => mark and skip this step
                        if (i == find_Node[find])
                        {
                            isCon = true;
                            break;
                        }
                    }
                    if (isCon) continue; //we do again the for loop, skip all command below.
                    // We already a Node i which have at least 2 predecessors. (Toi day=> loc ra nhung nut i co 2 cha tro len va nut i ko nam trong Inverse Dom(k)
                    int numFrom = 0;
                    for (int pre = 0; pre < graph.Network[currentN].Node[i].nPre; pre++) //Luot qua tat ca cac cha cua nut I
                    {
                        for (int find = 0; find < cntFind; find++)
                        {
                            if (graph.Network[currentN].Node[i].Pre[pre] == find_Node[find]) numFrom++; //??
                        }
                    }
                    if (numFrom > 1)
                    {
                        candidateNode = i;
                        numCandidate++;
                        //Need add this node to eDom^-1 => (find_Node)
                        find_Node[cntFind] = candidateNode; //Done explanation //ADD LAm` gi vay chu'???????
                        cntFind++;
                        df[ndf] = candidateNode;
                        ndf++;
                    }
                }
                calDom = new int[cntFind];
                for (int j = 0; j < cntFind; j++) calDom[j] = find_Node[j];
                graph.Network[currentN].Node[k].DF = df;
                graph.Network[currentN].Node[k].nDF = ndf;
                graph.Network[currentN].Node[k].nDomEI = cntFind;
                graph.Network[currentN].Node[k].DomEI = calDom;
            }
        }

        //Extended Inverse Post Dominance ePdom^-1 (find_DomRevEI)
        public static void find_PdomEI(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN) // join node만.....
        {
            for (int k = 0; k < graph.Network[currentN].nNode; k++)
            {
                int cntFind = 0;
                int[] find_Node = new int[graph.Network[currentN].nNode];
                int[] calDom = null;
                // Inverse - 나 자신과 나를 포함하는 노드
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    bool isCon = false;
                    for (int j = 0; j < graph.Network[currentN].Node[i].nDomRev; j++)
                    {
                        if (k == graph.Network[currentN].Node[i].DomRev[j])
                        {
                            isCon = true;
                            break;
                        }
                    }
                    if (isCon)
                    {
                        find_Node[cntFind] = i;
                        cntFind++;
                    }
                }
                // extension
                int numCandidate = 0;
                int candidateNode = 0;
                int[] pdf = new int[graph.Network[currentN].nNode];
                int npdf = 0;
                for (int i = 0; i < graph.Network[currentN].nNode; i++)
                {
                    if (graph.Network[currentN].Node[i].nPost < 2) continue;  // Split 아니면
                    bool isCon = false;
                    for (int find = 0; find < cntFind; find++)
                    {
                        if (i == find_Node[find])
                        {
                            isCon = true;
                            break;
                        }
                    }
                    if (isCon) continue;
                    int numFrom = 0;
                    for (int post = 0; post < graph.Network[currentN].Node[i].nPost; post++)
                    {
                        for (int find = 0; find < cntFind; find++)
                        {
                            if (graph.Network[currentN].Node[i].Post[post] == find_Node[find]) numFrom++;
                        }
                    }
                    if (numFrom > 1)
                    {
                        candidateNode = i;
                        numCandidate++;
                        //Need add this node to eDom^-1 => (find_Node)
                        find_Node[cntFind] = candidateNode; //Done explanation
                        cntFind++;

                        pdf[npdf] = candidateNode;
                        npdf++;
                    }
                }
                calDom = new int[cntFind];
                for (int j = 0; j < cntFind; j++) calDom[j] = find_Node[j];
                graph.Network[currentN].Node[k].PdF = pdf;
                graph.Network[currentN].Node[k].nPdF = npdf;
                graph.Network[currentN].Node[k].nDomRevEI = cntFind;
                graph.Network[currentN].Node[k].DomRevEI = calDom;
            }
        }
    }
}
