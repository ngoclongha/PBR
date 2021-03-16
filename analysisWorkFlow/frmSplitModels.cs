using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace gProAnalyzer
{    
    public partial class frmSplitModels : Form
    {
        private clsAnaysisNetwork m_Network;
        private string[] sFilePaths;
        private string[] sFileNames;
        public frmSplitModels()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadFileDialog.Title = "Browse";
            loadFileDialog.Filter = "Network Documents (*.net) | *.net";

            loadFileDialog.FileName = "";
            loadFileDialog.ShowDialog();

            sFilePaths = loadFileDialog.FileNames;
            sFileNames = loadFileDialog.SafeFileNames;          

            if (loadFileDialog.FileName == "") return;

            //After_Load();
        }

        //Processing here
        private void mnuMakeNetwork_Click(object sender, EventArgs e)
        {
            int count = 0;
            int count_newGraph = 0;
            for (int run = 0; run < sFileNames.Length; run++)
            {
                m_Network = new clsAnaysisNetwork();

                m_Network.Load_Data_ForBreakModel(m_Network.orgNet, sFilePaths[run], true);
                
                string saveFilePath = txtSaveFolder.Text + @"\" + sFileNames[run];                
                //Check Disconnected model
                int CCs = 0;
                int[] mark = null;
                m_Network.find_ConnectedComponents(m_Network.baseNet, ref CCs, ref mark); //We calculate on base net only.!!.
                if (CCs > 1) //Split model and store it
                {
                    count++;
                    for (int CC = 1; CC <= CCs; CC++)
                    {
                        count_newGraph++;
                        saveFilePath = txtSaveFolder.Text + @"\" + sFileNames[run].Replace(".net", "") + @"_" + CC + @".net";

                        int[] nodeList = new int[m_Network.Network[m_Network.orgNet].nNode];
                        int nNodeList = 0;
                        //get node list
                        for (int j = 0; j < mark.Length; j++)
                        {
                            if (mark[j] == CC)
                            {
                                nodeList[nNodeList] = j;
                                nNodeList++;
                            }
                        }
                        //get edge list
                        clsAnaysisNetwork.strLink[] LinkList = new clsAnaysisNetwork.strLink[m_Network.Network[m_Network.orgNet].nLink];
                        int nLinkList = 0;
                        for (int j = 0; j < m_Network.Network[m_Network.orgNet].nLink; j++)
                        {
                            for (int k = 0; k < nNodeList; k++)
                            {
                                if (nodeList[k] == m_Network.Network[m_Network.orgNet].Link[j].fromNode)
                                {
                                    for (int m = 0; m < nNodeList; m++)
                                    {
                                        if (nodeList[m] == m_Network.Network[m_Network.orgNet].Link[j].toNode)
                                        {
                                            LinkList[nLinkList].fromNode = k;
                                            LinkList[nLinkList].toNode = m;
                                            nLinkList++;
                                        }
                                    }
                                }
                            }
                        }
                        //Save temporary to file
                        Save_Network_1(m_Network.baseNet, nodeList, nNodeList, LinkList, nLinkList, saveFilePath, false, "ADD --- Original Network ---");
                        string[] tempLines = File.ReadAllLines(saveFilePath);

                        //Transfer to nodeList_S
                        string[] nodeList_S = new string[m_Network.Network[m_Network.orgNet].nNode];
                        int nNodeList_S = nNodeList;
                        for (int j = 0; j < nNodeList; j++)
                        {
                            nodeList_S[nNodeList] = nodeList[nNodeList].ToString();
                        }
                        //build Original Network
                            //find SS
                        int[] SS_List = new int[nNodeList];
                        int nSS = 0;
                        for (int j = 0; j < nNodeList; j++)
                        {
                            int count_Pre = 0;
                            for (int k = 0; k < nLinkList; k++)
                            {
                                if (j == LinkList[k].toNode)
                                    count_Pre++;
                            }
                            if (count_Pre == 0)
                            {
                                SS_List[nSS] = j;
                                nSS++;
                            }
                        }
                            //find EE
                        int[] EE_List = new int[nNodeList];
                        int nEE = 0;
                        for (int j = 0; j < nNodeList; j++)
                        {
                            int count_Post = 0;
                            for (int k = 0; k < nLinkList; k++)
                            {
                                if (j == LinkList[k].fromNode)
                                    count_Post++;
                            }
                            if (count_Post == 0)
                            {
                                EE_List[nEE] = j;
                                nEE++;
                            }
                        }
                        // SS -> -1; START -> -2; EE -> -3; END -> -4
                        //Create SS and EE links
                        if (nSS > 1)
                        {
                            //Create node SS
                            nodeList[nNodeList] = -1;
                            nNodeList++;
                            for (int j = 0; j < nSS; j++ )
                            {
                                LinkList[nLinkList].fromNode = nNodeList - 1;
                                LinkList[nLinkList].toNode = SS_List[j];
                                nLinkList++;
                            }
                            //Create START link to SS
                            nodeList[nNodeList] = -2; //START node
                            nNodeList++;
                            LinkList[nLinkList].fromNode = nNodeList - 1;
                            LinkList[nLinkList].toNode = nNodeList - 2;
                            nLinkList++;
                        }
                        else
                        {
                            //Create START link to the only start EVENT
                            nodeList[nNodeList] = -2; //START node
                            nNodeList++;
                            LinkList[nLinkList].fromNode = nNodeList - 1;
                            LinkList[nLinkList].toNode = SS_List[nSS - 1];
                            nLinkList++;
                        }

                        if (nEE > 1)
                        {
                            //Create node EE
                            nodeList[nNodeList] = -3;
                            nNodeList++;
                            for (int j = 0; j < nEE; j++)
                            {
                                LinkList[nLinkList].fromNode =EE_List[j];
                                LinkList[nLinkList].toNode = nNodeList - 1;
                                nLinkList++;
                            }
                            //Create END to be linked by EE
                            nodeList[nNodeList] = -4; //START node
                            nNodeList++;
                            LinkList[nLinkList].fromNode = nNodeList - 2;
                            LinkList[nLinkList].toNode = nNodeList - 1;
                            nLinkList++;
                        }
                        else
                        {
                            //Create END to be linked by the only end EVENT
                            nodeList[nNodeList] = -4; //END node
                            nNodeList++;
                            LinkList[nLinkList].fromNode = EE_List[nEE - 1];
                            LinkList[nLinkList].toNode = nNodeList - 1;
                            nLinkList++;
                        }
                        //build convert Network
                        //save file "abc.net" to "abc_CCs.net"                        
                        Save_Network_2(m_Network.baseNet, nodeList_S, nNodeList_S, nodeList, nNodeList, LinkList, nLinkList, saveFilePath, false, "");
                        //add more line in current IO File
                        StreamWriter sw = new StreamWriter(saveFilePath, true);
                        for (int i = 0; i < tempLines.Length; i++)
                        {
                            sw.WriteLine(tempLines[i]);                            
                        }
                        sw.Close();

                    }
                }
                else
                {
                    System.IO.File.Copy(sFilePaths[run], saveFilePath, true);
                }                                                                                                                                                                     
                m_Network = null;                
            }
            MessageBox.Show(count.ToString(), "Disconnected Component Count");
            MessageBox.Show(count_newGraph.ToString(), "New Graph Created");
        }

        private void Save_Network_1(int currentN, int[] nodeList, int nNodeList, clsAnaysisNetwork.strLink[] LinkList, int nLinkList, string sFilePath, bool isOrg, string appendText)
        {
            StreamWriter sw = new StreamWriter(sFilePath, isOrg);

            if (appendText != "")
            {
                sw.WriteLine(appendText);
            }

            //Node Information
            string imLine = nNodeList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nNodeList; i++)
            {
                imLine = m_Network.Network[currentN].Node[nodeList[i]].Name + " " + m_Network.Network[currentN].Node[nodeList[i]].Kind;
                sw.WriteLine(imLine);
            }

            //Link Information
            imLine = nLinkList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nLinkList; i++)
            {
                imLine = LinkList[i].fromNode.ToString() + " " + LinkList[i].toNode.ToString();
                sw.WriteLine(imLine);
            }
            sw.Close();
        }

        private void Save_Network_2(int currentN, string[] nodeList_S, int nNodeList_S, int[] nodeList, int nNodeList, clsAnaysisNetwork.strLink[] LinkList, int nLinkList, string sFilePath, bool isOrg, string appendText)
        {
            StreamWriter sw = new StreamWriter(sFilePath, isOrg);

            if (appendText != "")
            {
                sw.WriteLine(appendText);
            }

            //Node Information
            string imLine = nNodeList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nNodeList; i++)
            {
                if (nodeList[i] != -1 && nodeList[i] != -2 && nodeList[i] != -3 && nodeList[i] != -4)
                {
                    imLine = nodeList[i] + " " + m_Network.Network[currentN].Node[nodeList[i]].Kind;
                    sw.WriteLine(imLine);
                }
            }
            //Add START END SS EE
            // SS -> -1; START -> -2; EE -> -3; END -> -4
            for (int i = 0; i < nNodeList; i++)
            {
                if (nodeList[i] == -2)
                {
                    imLine = "S" + " " + "START";
                    sw.WriteLine(imLine);
                }
                if (nodeList[i] == -4)
                {
                    imLine = "E" + " " + "END";
                    sw.WriteLine(imLine);
                }
                if (nodeList[i] == -1)
                {
                    imLine = "SS" + " " + "OR";
                    sw.WriteLine(imLine);
                }
                if (nodeList[i] == -3)
                {
                    imLine = "EE" + " " + "OR";
                    sw.WriteLine(imLine);
                }
            }

            //Link Information
            imLine = nLinkList.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < nLinkList; i++)
            {
                imLine = LinkList[i].fromNode.ToString() + " " + LinkList[i].toNode.ToString();
                sw.WriteLine(imLine);
            }
            sw.Close();
        }

        private void btnSetFolder_Click(object sender, EventArgs e)
        {
            folderBrowserMake.SelectedPath = txtLoadFolder.Text;

            folderBrowserMake.ShowDialog();

            txtSaveFolder.Text = folderBrowserMake.SelectedPath;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int count = 0;
            int count_newGraph = 0;
            StreamWriter sw1 = new StreamWriter("c:\\DisconnectedCount.txt", true);
            for (int run = 0; run < sFileNames.Length; run++)
            {
                m_Network = new clsAnaysisNetwork();

                m_Network.Load_Data_ForBreakModel(m_Network.orgNet, sFilePaths[run], true);
                
                string saveFilePath = txtSaveFolder.Text + @"\" + sFileNames[run];                
                //Check Disconnected model
                int CCs = 0;
                int[] mark = null;
                m_Network.find_ConnectedComponents(m_Network.baseNet, ref CCs, ref mark); //We calculate on base net only.!!.

                
                sw1.WriteLine(sFileNames[run] + ";" + CCs);

                count++;
                
            }
            sw1.Close();
            MessageBox.Show(count.ToString(), "Number of total model");
            
        
        }
    }
}
