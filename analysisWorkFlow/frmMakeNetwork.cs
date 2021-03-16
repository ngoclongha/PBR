using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace gProAnalyzer
{
    public partial class frmMakeNetwork : Form
    {
        public frmMakeNetwork()
        {
            InitializeComponent();
        }

        //Read all parament to "net" from the textboxes
        private void read_Parameter(clsMakeNetwork net)
        {
            net.nNode = Convert.ToInt32(txtN.Text);

            net.nNodeF = Convert.ToInt32(txtFormNF.Text);
            net.nNodeT = Convert.ToInt32(Convert.ToDouble(txtToNF_Rate.Text) * net.nNode);

            net.rStructure = Convert.ToDouble(txtSF_Rate.Text);

            net.nSplitF = Convert.ToInt32(txtFormSS.Text);
            net.nSplitT = Convert.ToInt32(txtToSS.Text);

            net.nForwardF = Convert.ToInt32(txtFormFF.Text);
            net.nForwardT = Convert.ToInt32(txtToFF.Text);

            net.nBackwardF = Convert.ToInt32(txtFormBF.Text);
            net.nBackwardT = Convert.ToInt32(txtToBF.Text);

            net.rOR = Convert.ToDouble(textOR_Rate.Text);
            net.rXOR = Convert.ToDouble(textXOR_Rate.Text);
        }


        private void Save_Network(clsMakeNetwork net, string sFilePath)
        {
            StreamWriter sw = new StreamWriter(sFilePath);

            //Node Information
            string imLine = net.Network.nNode.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < net.Network.nNode; i++)
            {
                imLine = i.ToString() + " " + net.Network.Node[i].Kind;
                sw.WriteLine(imLine);
            }

            //Link Information
            imLine = net.Network.nLink.ToString();
            sw.WriteLine(imLine);

            for (int i = 0; i < net.Network.nLink; i++)
            {
                imLine = net.Network.Link[i].fromNode.ToString() + " " + net.Network.Link[i].toNode.ToString();

                sw.WriteLine(imLine);
            }

            //Condition;
            sw.WriteLine("Making Condition");

            imLine = txtN.Text;
            imLine += " " + txtFormNF.Text;
            imLine += " " + txtToNF_Rate.Text;
            imLine += " " + txtSF_Rate.Text;
            imLine += " " + txtFormSS.Text;
            imLine += " " + txtToSS.Text;
            imLine += " " + txtFormFF.Text;
            imLine += " " + txtToFF.Text;
            imLine += " " + txtFormBF.Text;
            imLine += " " + txtToBF.Text;
            imLine += " " + textXOR_Rate.Text;

            sw.WriteLine(imLine);

            sw.Close();
        }


        private void mnuMakeNetwork_Click(object sender, EventArgs e)
        {
            if (txtFolder.Text == "") return;

            int nFile = Convert.ToInt32(txtFileN.Text);
            int sNum = Convert.ToInt32(txtFileB.Text);

            for (int i = 0; i < nFile; i++)
            {
                //Create main 
                clsMakeNetwork net = new clsMakeNetwork();

                //set parameter
                read_Parameter(net);

                //Make Network
                net.make_Network();

                //Save Network
                string sFilePath = txtFolder.Text + @"\";
                sFilePath += txtFileA.Text + sNum.ToString() + @".net";
                Save_Network(net, sFilePath);
                sNum++;

                net = null;

            }

            MessageBox.Show(txtFileN.Text + " Network files were made");
        }

        private void btnSetFolder_Click(object sender, EventArgs e)
        {
            folderBrowserMake.ShowDialog();

            txtFolder.Text = folderBrowserMake.SelectedPath;
        }

        private void textOR_Rate_Leave(object sender, EventArgs e)
        {
            double tot = 1 - Convert.ToDouble(textOR_Rate.Text) - Convert.ToDouble(textXOR_Rate.Text);
            textAND_Rate.Text = tot.ToString();
        }

        private void textXOR_Rate_Leave(object sender, EventArgs e)
        {
            double tot = 1 - Convert.ToDouble(textOR_Rate.Text) - Convert.ToDouble(textXOR_Rate.Text);
            textAND_Rate.Text = tot.ToString();
        }


    }
}
