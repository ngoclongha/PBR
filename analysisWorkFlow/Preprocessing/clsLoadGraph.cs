using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gProAnalyzer.Preprocessing
{
    class clsLoadGraph
    {
        public static string file_check = "";
        public static string fileName_check;        
        private gProAnalyzer.Ultilities.clsFindNodeInfo fninfo;

        public static void Initialize_All()
        {
            //fninfo = new gProAnalyzer.Ultilities.clsFindNodeInfo();
        }

        public void Load_Data_ForBreakModel(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, string sFilePath, bool readOrigin)
        {
            Initialize_All();

            string rText;
            string[] words;

            fileName_check = sFilePath;

            StreamReader sr = new StreamReader(sFilePath);
            file_check = sFilePath;
            //노드 수
            rText = sr.ReadLine();
            graph.Network[currentN].nNode = Convert.ToInt32(rText);
            graph.Network[currentN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[currentN].nNode]; //create enough size of the graph

            //노드 정보 //read file, line by line and add NODE to "graph.Network"
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' '); //words is a array of string word[]
                //MessageBox.Show(words[0].ToString());
                graph.Network[currentN].Node[i].orgNum = i;
                graph.Network[currentN].Node[i].parentNum = i;
                graph.Network[currentN].Node[i].Type_I = "";
                graph.Network[currentN].Node[i].Type_II = "";

                graph.Network[currentN].Node[i].Kind = words[1];
                if (words.Length > 2)
                    for (int w = 2; w < words.Length; w++)
                        graph.Network[currentN].Node[i].Kind = graph.Network[currentN].Node[i].Kind + " " + words[w]; //add more label

                if (graph.Network[currentN].Node[i].Kind == "START")
                {
                    graph.Network[currentN].header = i;
                    graph.Network[currentN].Node[i].Name = words[0];// +"(S)";
                }
                else if (graph.Network[currentN].Node[i].Kind == "END")
                {
                    graph.Network[currentN].Node[i].Name = words[0];// +"(E)";
                }
                else
                {
                    graph.Network[currentN].Node[i].Name = words[0];// i.ToString();
                }
                //if (graph.Network[currentN].Node[i].Name == "SS")
                {
                    //graph.Network[currentN].Node[i].Kind = "XOR";
                }
            }
            // 링크 수 Add link
            rText = sr.ReadLine();
            graph.Network[currentN].nLink = Convert.ToInt32(rText);
            graph.Network[currentN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[currentN].nLink];

            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' ');
                graph.Network[currentN].Link[i].fromNode = Convert.ToInt32(words[0]);
                graph.Network[currentN].Link[i].toNode = Convert.ToInt32(words[1]);
            }
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, i);
            }
            // Original NETWORK 읽기 //Read Original graph.Network, if it did not have. Move to next step
            if (readOrigin && !sr.EndOfStream)
            {
                rText = sr.ReadLine();
                if (rText.Substring(0, 3) == "ADD")
                {
                    //노드 수
                    rText = sr.ReadLine();
                    graph.Network[graph.baseNet].nNode = Convert.ToInt32(rText);
                    graph.Network[graph.baseNet].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[graph.baseNet].nNode];

                    //노드 정보
                    for (int i = 0; i < graph.Network[graph.baseNet].nNode; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');
                        graph.Network[graph.baseNet].Node[i].orgNum = i;
                        graph.Network[graph.baseNet].Node[i].parentNum = i;
                        graph.Network[graph.baseNet].Node[i].Type_I = "";
                        graph.Network[graph.baseNet].Node[i].Type_II = "";

                        graph.Network[graph.baseNet].Node[i].Kind = words[1];
                        if (words.Length > 2)
                            for (int w = 2; w < words.Length; w++)
                                graph.Network[graph.baseNet].Node[i].Kind = graph.Network[graph.baseNet].Node[i].Kind + " " + words[w]; //add more label

                        if (graph.Network[graph.baseNet].Node[i].Kind == "START")
                        {
                            graph.Network[graph.baseNet].header = i;
                            graph.Network[graph.baseNet].Node[i].Name = words[0];// +"(S)";
                        }
                        else if (graph.Network[graph.baseNet].Node[i].Kind == "END")
                        {
                            graph.Network[graph.baseNet].Node[i].Name = words[0];// +"(E)";
                        }
                        else
                        {
                            graph.Network[graph.baseNet].Node[i].Name = words[0];// i.ToString();
                        }
                    }
                    // 링크 수
                    rText = sr.ReadLine();
                    graph.Network[graph.baseNet].nLink = Convert.ToInt32(rText);
                    graph.Network[graph.baseNet].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[graph.baseNet].nLink];

                    for (int i = 0; i < graph.Network[graph.baseNet].nLink; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');

                        graph.Network[graph.baseNet].Link[i].fromNode = Convert.ToInt32(words[0]);
                        graph.Network[graph.baseNet].Link[i].toNode = Convert.ToInt32(words[1]);
                    }
                    for (int i = 0; i < graph.Network[graph.baseNet].nNode; i++)
                    {
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, graph.baseNet, i);
                    }
                }
            }
            sr.Close();
        }
        //
        public void Load_Data(ref gProAnalyzer.GraphVariables.clsGraph graph, int currentN, string sFilePath, bool readOrigin)
        {
            Initialize_All();
            string rText;
            string[] words;

            fileName_check = sFilePath;

            StreamReader sr = new StreamReader(sFilePath);
            file_check = sFilePath;

            //노드 수
            rText = sr.ReadLine();
            graph.Network[currentN].nNode = Convert.ToInt32(rText);
            graph.Network[currentN].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[currentN].nNode]; //create enough size of the graph

            //노드 정보 //read file, line by line and add NODE to "graph.Network"
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' '); //words is a array of string word[]
                //MessageBox.Show(words[0].ToString());
                graph.Network[currentN].Node[i].orgNum = i;
                graph.Network[currentN].Node[i].parentNum = i;
                graph.Network[currentN].Node[i].Type_I = "";
                graph.Network[currentN].Node[i].Type_II = "";
                graph.Network[currentN].Node[i].Kind = words[1];
                //store node label
                if (words.Length > 2)
                {
                    graph.Network[currentN].Node[i].nodeLabel = "";
                    for (int length = 2; length < words.Length; length++)
                    {
                        graph.Network[currentN].Node[i].nodeLabel = graph.Network[currentN].Node[i].nodeLabel + words[length];
                        if (length < words.Length - 1)
                            graph.Network[currentN].Node[i].nodeLabel = graph.Network[currentN].Node[i].nodeLabel + " ";
                    }
                }
                if (graph.Network[currentN].Node[i].Kind == "START")
                {
                    graph.Network[currentN].header = i;
                    graph.Network[currentN].Node[i].Name = words[0];// +"(S)";
                }
                else if (graph.Network[currentN].Node[i].Kind == "END")
                {
                    graph.Network[currentN].Node[i].Name = words[0];// +"(E)";
                }
                else
                {
                    graph.Network[currentN].Node[i].Name = words[0];// i.ToString();
                }
                //if (graph.Network[currentN].Node[i].Name == "SS")
                {
                    //graph.Network[currentN].Node[i].Kind = "XOR";
                }
            }

            // 링크 수 Add link
            rText = sr.ReadLine();
            graph.Network[currentN].nLink = Convert.ToInt32(rText);
            graph.Network[currentN].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[currentN].nLink];

            for (int i = 0; i < graph.Network[currentN].nLink; i++)
            {
                rText = sr.ReadLine();
                words = rText.Split(' ');

                graph.Network[currentN].Link[i].fromNode = Convert.ToInt32(words[0]);
                graph.Network[currentN].Link[i].toNode = Convert.ToInt32(words[1]);
            }

            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, currentN, i);
            }

            // Original NETWORK 읽기 //Read Original graph.Network, if it did not have. Move to next step
            if (readOrigin && !sr.EndOfStream)
            {
                rText = sr.ReadLine();
                if (rText.Substring(0, 3) == "ADD")
                {
                    //노드 수
                    rText = sr.ReadLine();
                    graph.Network[graph.baseNet].nNode = Convert.ToInt32(rText);
                    graph.Network[graph.baseNet].Node = new gProAnalyzer.GraphVariables.clsNode.strNode[graph.Network[graph.baseNet].nNode];

                    //노드 정보
                    for (int i = 0; i < graph.Network[graph.baseNet].nNode; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');
                        graph.Network[graph.baseNet].Node[i].orgNum = i;
                        graph.Network[graph.baseNet].Node[i].parentNum = i;
                        graph.Network[graph.baseNet].Node[i].Type_I = "";
                        graph.Network[graph.baseNet].Node[i].Type_II = "";
                        graph.Network[graph.baseNet].Node[i].Kind = words[1];

                        if (graph.Network[graph.baseNet].Node[i].Kind == "START")
                        {
                            graph.Network[graph.baseNet].header = i;
                            graph.Network[graph.baseNet].Node[i].Name = words[0];// +"(S)";
                        }
                        else if (graph.Network[graph.baseNet].Node[i].Kind == "END")
                        {
                            graph.Network[graph.baseNet].Node[i].Name = words[0];// +"(E)";
                        }
                        else
                        {
                            graph.Network[graph.baseNet].Node[i].Name = words[0];// i.ToString();
                        }
                    }

                    // 링크 수
                    rText = sr.ReadLine();
                    graph.Network[graph.baseNet].nLink = Convert.ToInt32(rText);
                    graph.Network[graph.baseNet].Link = new gProAnalyzer.GraphVariables.clsEdge.strEdge[graph.Network[graph.baseNet].nLink];

                    for (int i = 0; i < graph.Network[graph.baseNet].nLink; i++)
                    {
                        rText = sr.ReadLine();
                        words = rText.Split(' ');

                        graph.Network[graph.baseNet].Link[i].fromNode = Convert.ToInt32(words[0]);
                        graph.Network[graph.baseNet].Link[i].toNode = Convert.ToInt32(words[1]);
                    }

                    for (int i = 0; i < graph.Network[graph.baseNet].nNode; i++)
                    {
                        gProAnalyzer.Ultilities.clsFindNodeInfo.find_NodeInfo(ref graph, graph.baseNet, i);
                    }
                }
            }
            sr.Close();
        }
    }
}
