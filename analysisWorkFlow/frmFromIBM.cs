using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace gProAnalyzer
{
    public partial class frmFromIBM : Form
    {
        private static int maxXmlNode = 200;
        private static int maxXmlLink = 300;

        private string[] sFilePaths;
        private string[] sFileNames; //string array

        public struct strNode
        {
            public string Name; //실 이름

            public string Kind; //Node 종류  S:start E: end T:Task AND:And  XOR:OR

        }

        public struct strLink
        {
            public int fromNode; //시작 Node
            public int toNode; //끝 Node
        }

        public struct strNetwork
        {
            public int nNode; //number of nodes
            public strNode[] Node; 

            public int nLink; //number of link
            public strLink[] Link;
        }
        private strNetwork Network;

        public frmFromIBM()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e) //get a list of files
        {
            loadFileDialog.Title = "Browse";
            loadFileDialog.Filter = "XML Documents (*.xml) | *.xml";

            loadFileDialog.FileName = "";
            loadFileDialog.ShowDialog();

            if (loadFileDialog.FileName == "") return;

            sFilePaths = loadFileDialog.FileNames; //get the array of file path which was selected
            sFileNames = loadFileDialog.SafeFileNames; //get the array of file name which was selected

            txtLoadFolder.Text = loadFileDialog.FileName.Remove(loadFileDialog.FileName.Length - loadFileDialog.SafeFileName.Length); //shorted file path, get folder path


            txtFileN.Text = sFileNames.Length.ToString(); //display the number of selected files to a textBox
        }

        private void btnSetFolder_Click(object sender, EventArgs e) //set the destination of saved files
        {
            folderBrowserMake.SelectedPath = txtLoadFolder.Text;

            folderBrowserMake.ShowDialog();

            txtSaveFolder.Text = folderBrowserMake.SelectedPath;
        }

        //CORE OF THE FORM
        private void mnuMakeNetwork_Click(object sender, EventArgs e) //convert network button
        {
            for (int run = 0; run < sFileNames.Length; run++)
            {
                Network = new strNetwork();

                string sFilePath = txtSaveFolder.Text + @"\";
                sFilePath += sFileNames[run].Remove(sFileNames[run].Length - 4) + @".net";

                //Made Network
                Load_XML(sFilePaths[run], false); //load and convert XML file named sFilePath[run] example: (f\project\ibm\s000345.bpmn.net)
                Save_Network(sFilePath, false, "");


                //Original Network
                Load_XML(sFilePaths[run], true);

                Save_Network(sFilePath, true, "ADD --- Original Network ---");


            }

            MessageBox.Show(txtFileN.Text + " Network files were made");
        }

        public void Load_XML(string sFilePath, bool isOrg)
        {
            int nStart = 0, nEnd = 0;
            int xNode = 0, xLink = 0;
            string[,] xmlNode = new string[maxXmlNode, 2]; //ID, Kind
            string[,] xmlLink = new string[maxXmlLink, 3]; //ID, From, To

            XmlTextReader xr = new XmlTextReader(sFilePath); //readXML
            // xr.Read() => begin read a XML file
            // xr.Name() => read a tag

            while (xr.Read())
            {
                if (xr.NodeType == System.Xml.XmlNodeType.Element)
                {
                    //visit each element and attribute of XML file. Get name tag
                    //set array xmlNode[,]
                    if (xr.Name == "startEvent" || xr.Name == "endEvent" || xr.Name == "task" || xr.Name == "exclusiveGateway" || xr.Name == "parallelGateway")
                    {
                        if (xr.Name == "startEvent")
                        {
                            xmlNode[xNode, 1] = "START";
                            nStart++;
                        }
                        else if (xr.Name == "endEvent")
                        {
                            xmlNode[xNode, 1] = "END";
                            nEnd++;
                        }
                        else if (xr.Name == "task") xmlNode[xNode, 1] = "TASK";
                        else if (xr.Name == "exclusiveGateway") xmlNode[xNode, 1] = "XOR";
                        else if (xr.Name == "parallelGateway") xmlNode[xNode, 1] = "AND";

                        while (xr.MoveToNextAttribute())
                        {
                            if (xr.Name == "id") xmlNode[xNode, 0] = xr.Value;
                        }
                        xNode++;
                    }
                        //check whether this tag has the attributes
                    else if (xr.Name == "sequenceFlow")
                    {
                        while (xr.MoveToNextAttribute())
                        {
                            if (xr.Name == "id") xmlLink[xLink, 0] = xr.Value;
                            else if (xr.Name == "sourceRef") xmlLink[xLink, 1] = xr.Value;
                            else if (xr.Name == "targetRef") xmlLink[xLink, 2] = xr.Value;
                        }
                        // check if there are a node with the same "from" and "to" => just do not increase xLink;
                        bool isSame = false;
                        for (int i = 0; i < xLink; i++)
                        {
                            if (xmlLink[xLink, 1] == xmlLink[i, 1] && xmlLink[xLink, 2] == xmlLink[i, 2])
                            {
                                isSame = true;
                                break;
                            }
                        }
                        if (!isSame) xLink++;
                    }

                }
            }

            //checked flag (for specified use) - if isOrg == true => this is original netwrok
            if (!isOrg)
            {
                //don't know what is this, but it seem to modify matrix "xmlNode" and "xmlLink"
                //what is nStart mean??
                if (nStart > 1) //if there are many entry point
                {

                    xmlNode[xNode, 0] = "S";
                    xmlNode[xNode, 1] = "START";
                    xNode++;

                    xmlNode[xNode, 0] = "SS"; //dummy strat
                    xmlNode[xNode, 1] = "OR";
                    xNode++;

                    xmlLink[xLink, 0] = "SL0";
                    xmlLink[xLink, 1] = "S";
                    xmlLink[xLink, 2] = "SS";
                    xLink++;

                    for (int i = 0; i < xNode - 2; i++)
                    {
                        if (xmlNode[i, 1] == "START")
                        {
                            xmlNode[i, 1] = "SD";

                            xmlLink[xLink, 0] = "SL1";
                            xmlLink[xLink, 1] = "SS";
                            xmlLink[xLink, 2] = xmlNode[i, 0];
                            xLink++;
                        }
                    }
                }

                if (nEnd > 1) //if there are many ending point
                {

                    xmlNode[xNode, 0] = "E";
                    xmlNode[xNode, 1] = "END";
                    xNode++;

                    xmlNode[xNode, 0] = "EE"; // dummy end
                    xmlNode[xNode, 1] = "OR";
                    xNode++;

                    xmlLink[xLink, 0] = "EL0";
                    xmlLink[xLink, 1] = "EE";
                    xmlLink[xLink, 2] = "E";
                    xLink++;

                    for (int i = 0; i < xNode - 2; i++) //back to 2 unit for what?
                    {
                        if (xmlNode[i, 1] == "END")
                        {
                            xmlNode[i, 1] = "ED";

                            xmlLink[xLink, 0] = "EL1";
                            xmlLink[xLink, 1] = xmlNode[i, 0];
                            xmlLink[xLink, 2] = "EE";
                            xLink++;
                        }
                    }
                }
            }

            //                  =====================
            //=====Create Network from xmlNode and xmlLink variables======
            //                  =====================

            //노드 수
            Network.nNode = xNode; //number of nodes
            Network.Node = new strNode[xNode]; //initiate a array of Node


            //노드 정보
            for (int i = 0; i < xNode; i++)
            {
                //transfer from xmlNode to 
                Network.Node[i].Kind = xmlNode[i, 1];

                if (xmlNode[i, 0] == "S" || xmlNode[i, 0] == "E" || xmlNode[i, 0] == "SS" || xmlNode[i, 0] == "EE")
                {
                    Network.Node[i].Name = xmlNode[i, 0];
                }
                else
                {
                    if (xmlNode[i, 0].Substring(0, 1) == "_")
                    {
                        if (xmlNode[i, 1] == "SD") Network.Node[i].Name = "SD";
                        else if (xmlNode[i, 1] == "ED") Network.Node[i].Name = "ED";
                        else Network.Node[i].Name = "DD";
                    }
                    else
                    {
                        string[] words = xmlNode[i, 0].Split('_'); //split string s00000309_12648 to s0000000309 and 12648
                        Network.Node[i].Name = words[1];
                    }

                }

                //if (Network.Node[i].Kind == "START")
                //{
                //    Network.Node[i].Name = "S";
                //}
                //else if (Network.Node[i].Kind == "END")
                //{
                //    Network.Node[i].Name = "E";
                //}
                //else
                //{
                //    if (xmlNode[i, 0] == "SS") Network.Node[i].Name = "SS";
                //    else if (xmlNode[i, 0] == "EE") Network.Node[i].Name = "EE";
                //    else
                //    {
                //        if (xmlNode[i, 0].Substring(0, 1) == "_")
                //        {
                //            if (xmlNode[i, 1] == "SD") Network.Node[i].Name = "SD";
                //            else if (xmlNode[i, 1] == "ED") Network.Node[i].Name = "ED";
                //            else Network.Node[i].Name = "DD";
                //        }
                //        else
                //        {
                //            string[] words = xmlNode[i, 0].Split('_');
                //            Network.Node[i].Name = words[1];
                //        }
                //    }
                //}

            }

            // 링크 수
            Network.nLink = xLink; //number of links
            Network.Link = new strLink[xLink];

            for (int i = 0; i < xLink; i++)
            {
                int fromN = -1, toN = -1;

                //find the position of the node which is the same of xmlLink FROM and TO in xmlNode matrix
                for (int j = 0; j < xNode; j++)
                {
                    //if the ID of "node From" = ID (or exist) of node in xmlNode => save this index
                    if (xmlLink[i, 1] == xmlNode[j, 0]) fromN = j;
                    //if the ID of "node To" = ID (or exist) of node in xmlNode => save this index
                    if (xmlLink[i, 2] == xmlNode[j, 0]) toN = j;

                }

                if (fromN >= 0 && toN >= 0)
                {
                    //just save the index of a row of xmlNode

                    Network.Link[i].fromNode = fromN;
                    Network.Link[i].toNode = toN;
                }
            }

            //추가
            for (int i = 0; i < Network.nNode; i++) //scan each of node
            {
                int cntPre = 0;
                int cntPost = 0;
                for (int j = 0; j < Network.nLink; j++) //scan each of link
                {
                    // if there are a node which is the same vertice, the cntPost and cntPre variables will be increased (1 unit)
                    if (Network.Link[j].fromNode == i) cntPost++;
                    if (Network.Link[j].toNode == i) cntPre++;
                }

                if (Network.Node[i].Kind == "SD")
                {   
                    //don't what is that mean?
                    if (cntPost > 1) Network.Node[i].Kind = "OR";
                    else Network.Node[i].Kind = "EVENT";
                }
                else if (Network.Node[i].Kind == "ED")
                {
                    if (cntPre > 1) Network.Node[i].Kind = "OR";
                    else Network.Node[i].Kind = "EVENT";
                }


            }

        }

        private void Save_Network(string sFilePath, bool isOrg, string appendText)
        {
            StreamWriter sw = new StreamWriter(sFilePath, isOrg);

            if (appendText != "")
            {
                sw.WriteLine(appendText);
            }
            //Node Information
            string imLine = Network.nNode.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < Network.nNode; i++)
            {
                imLine = Network.Node[i].Name + " " + Network.Node[i].Kind;
                sw.WriteLine(imLine);
            }

            //Link Information
            imLine = Network.nLink.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < Network.nLink; i++)
            {
                imLine = Network.Link[i].fromNode.ToString() + " " + Network.Link[i].toNode.ToString();

                sw.WriteLine(imLine);
            }

            sw.Close();
        }
    }
}
