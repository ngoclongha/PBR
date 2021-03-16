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
    public partial class frmProcessQuery : Form
    {
        gProAnalyzer.Preprocessing.clsLoadGraph loadGraph;
        gProAnalyzer.Functionalities.LoopIdentification loopNode;
        gProAnalyzer.Functionalities.IndexingPM indexing;

        gProAnalyzer.Functionalities.ProcessQuery PQ;
        gProAnalyzer.GraphVariables.clsRepository rp;
        
        
        //Initialize all global parameter
        public frmProcessQuery()
        {
            InitializeComponent();
            System.Windows.Forms.ToolTip toolTip1 = new System.Windows.Forms.ToolTip();
            //toolTip1.SetToolTip(this.startBtn, "Run Query");
            //toolTip1.SetToolTip(this.clearBtn, "Clear Editor");            

            //Initialized All            
            rp = new gProAnalyzer.GraphVariables.clsRepository();
            loopNode = new gProAnalyzer.Functionalities.LoopIdentification();
            loadGraph = new gProAnalyzer.Preprocessing.clsLoadGraph();
            indexing = new gProAnalyzer.Functionalities.IndexingPM();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {
             
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {

        }

        private void startBtn_Click(object sender, EventArgs e) //QUERY BUTTON 
        {           
            bool check_untangle = true;
            //listView1.Columns.Add("Models Name", 100);
            //listView1.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            if (rp.repository == null) {
                MessageBox.Show("Repository not found", "Error");
                return;
            }
            //Initialized
            PQ = new gProAnalyzer.Functionalities.ProcessQuery();
            bool[] flag_Check = new bool[8];
            string[] Results_List = new string[rp.nModel];
            int nResults = 0; 

            //get (X, Y)
            if (textBox2.Text == "" || textBox1.Text == "" || comboBox_BhR.Text == "" || comboBox_BhR.Text == "Select BhR")
            {
                MessageBox.Show("Input Empty", "Warnning");
                //(X, Y) must be ACTIVITY or TASK
                return;
            }
            int x = Int32.Parse(textBox1.Text);
            int y = Int32.Parse(textBox2.Text);

            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = true; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur

            if (Convert.ToInt32(comboBox_BhR.Text[0].ToString()) == 0)
                flag_Check[0] = false;
            if (Convert.ToInt32(comboBox_BhR.Text[0].ToString()) == 1)
                flag_Check[1] = false;
            if (Convert.ToInt32(comboBox_BhR.Text[0].ToString()) == 2)
                flag_Check[2] = false;
            if (Convert.ToInt32(comboBox_BhR.Text[0].ToString()) == 3)
                flag_Check[3] = false;
            if (Convert.ToInt32(comboBox_BhR.Text[0].ToString()) == 4)
                flag_Check[4] = false;
            if (Convert.ToInt32(comboBox_BhR.Text[0].ToString()) == 6)
                flag_Check[6] = false;

            //get Scope of the query (FROM *) - how many model need to check.
            //For each MODEL => Perform query
                //retrieve the model from DATABASE (or retrieve in BATH)
                //perform the query technique
                //if MODEL satisfy the query => store in a list RESULTS[]         
            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            for (int i = 0; i < rp.nModel; i++) {

                int currentN = rp.repository[i].graph.finalNet;
                GraphVariables.clsHWLS curr_HWLS = rp.repository[i].clsHWLS;
                int workLoop = rp.repository[i].clsLoop.orgLoop;
                int workSESE = rp.repository[i].clsSESE.finalSESE;
                if (rp.repository[i].graph.check_untangle) {
                    currentN = rp.repository[i].graph.untangleNet;
                    curr_HWLS = rp.repository[i].clsHWLS_Untangle;
                    workLoop = rp.repository[i].clsLoop.untangleLoop;
                    workSESE = rp.repository[i].clsSESE.untangleSESE;
                }

                if (check_XY(rp, currentN, i, x, y) == false) {
                    //MessageBox.Show("Index illegal", "Warning");
                    continue;
                }
                //check loop //NOT CONSIDER LOOP
                //if (rp.repository[i].clsLoop.Loop[0].nLoop > 0) continue;

                bool hold_PQ;

                if (rp.repository[i].graph.check_untangle)
                    hold_PQ = PQ.start_ProcessQuery(ref rp.repository[i].graph, currentN, ref rp.repository[i].clsHWLS_Untangle, rp.repository[i].clsLoop, workLoop, rp.repository[i].clsSESE, workSESE, x, y, flag_Check);
                else
                    hold_PQ = PQ.start_ProcessQuery(ref rp.repository[i].graph, currentN, ref rp.repository[i].clsHWLS, rp.repository[i].clsLoop, workLoop, rp.repository[i].clsSESE, workSESE, x, y, flag_Check);

                if (hold_PQ) {
                    Results_List[nResults] = rp.repository[i].ID_model;
                    //add to listview
                    //ListViewItem itm;
                    //itm = new ListViewItem(Results_List[nResults]);
                    //listView1.Items.Add(itm);
                    dataGridView1.Rows.Add(Results_List[nResults]);
                    nResults++;
                }                
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            //MessageBox.Show("Finish in " + Run_Times_total.ToString() + " seconds", "Message");

            //display the Results_List[] - list of satisfied models to user.
            Results.Text = nResults.ToString() + " model(s)";
            string st = "Execution Time: ";
            ExeTime.Text = st + Run_Times_total.ToString() + "s";
            QueryStatus.Text = "Success";
        }

        private bool check_XY(gProAnalyzer.GraphVariables.clsRepository rp, int currentN, int i, int x, int y)
        {
            int maxNode = rp.repository[i].graph.Network[currentN].nNode;
            if (!(x < maxNode && y < maxNode)) return false;

            if (rp.repository[i].graph.Network[currentN].Node[x].nPost > 1 ||
                rp.repository[i].graph.Network[currentN].Node[x].nPre > 1 ||

                rp.repository[i].graph.Network[currentN].Node[y].nPost > 1 ||
                rp.repository[i].graph.Network[currentN].Node[y].nPre > 1 ||

                rp.repository[i].graph.Network[currentN].Node[x].nPost == 0 ||
                rp.repository[i].graph.Network[currentN].Node[x].nPre == 0 ||

                rp.repository[i].graph.Network[currentN].Node[y].nPost == 0 ||
                rp.repository[i].graph.Network[currentN].Node[y].nPre == 0)
                return false;
            return true;
        }
        private void Initialize_Details_Model(ref GraphVariables.clsRepository rp, int i)
        {
            rp.repository[i].graph = new GraphVariables.clsGraph();
            rp.repository[i].clsHWLS = new GraphVariables.clsHWLS();
            rp.repository[i].clsHWLS_Untangle = new GraphVariables.clsHWLS();
            rp.repository[i].clsLoop = new GraphVariables.clsLoop();
            rp.repository[i].clsSESE = new GraphVariables.clsSESE();

        }

        private void IndexingBtn_Click(object sender, EventArgs e) //INDEXING
        {
            string[] sFilePaths;
            string[] sFileNames;

            openFileDialog1.Title = "Browse";
            openFileDialog1.Filter = "Network Documents (*.net) | *.net";
            openFileDialog1.FileName = "";
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName == "") return;

            sFilePaths = openFileDialog1.FileNames;
            sFileNames = openFileDialog1.SafeFileNames;
            rp.repository = new GraphVariables.clsRepository.processModel[sFileNames.Length];
            rp.nModel = sFileNames.Length;

            bool[] flag_Check = new bool[8];
            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = false; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur

            //flag_Check = new bool[8];

            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            for (int run = 0; run < sFileNames.Length; run++) {

                //if (rp.repository[run].clsLoop.Loop[0].nLoop > 0) continue; //not consider loops
                DateTime stTime = new DateTime(); stTime = DateTime.Now; double Run_Times = 0;

                Initialize_Details_Model(ref rp, run);
                loadGraph.Load_Data(ref rp.repository[run].graph, rp.repository[run].graph.orgNet, sFilePaths[run], true);
                rp.repository[run].ID_model = sFileNames[run];
                gProAnalyzer.Functionalities.IndexingPM.start_Indexing_Acyclic(ref rp.repository[run].graph, ref rp.repository[run].clsHWLS, ref rp.repository[run].clsHWLS_Untangle, ref rp.repository[run].clsLoop, ref rp.repository[run].clsSESE, flag_Check);

                Run_Times = (DateTime.Now - stTime).TotalMilliseconds;
                rp.repository[run].Time_Index = Run_Times;
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            MessageBox.Show("Finish in " + Run_Times_total.ToString() + " seconds", "Message");
        }

        //@"I:\WriteLines1.txt"
        private void store_File(string SourceFilePath, string saveFileName, double[] Session)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(SourceFilePath, true)) {
                file.WriteLine(saveFileName + ";" + Session[0].ToString() + ";" + Session[1].ToString() + ";" + Session[2].ToString() + ";" + Session[3].ToString() + ";"
                    + Session[4].ToString() + ";" + Session[5].ToString() + ";" + Session[6].ToString() + ";" + Session[7].ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e) //store data button
        {
            int maxIteration = 1000;            

            if (rp.repository == null) {
                MessageBox.Show("Repository not found", "Error");
                return;
            }
            //Initialized
            PQ = new gProAnalyzer.Functionalities.ProcessQuery();
            bool[] flag_Check = new bool[8];
            string[] Results_List = new string[rp.nModel];
            int nResults = 0;            

            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = true; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur
    
            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            DateTime tempTime = new DateTime();

            for (int i = 0; i < rp.nModel; i++) {
                double[] SessionTime = new double[8];
                for (int j = 0; j < 8; j++) {
                    flag_Check[j] = false;                                        
                    int RandCount = 0;

                    //Generate a pair of X and Y based on Random(1, nNode) //100 times                    
                    do {
                        Random rand = new Random();
                        int x = rand.Next(0, rp.repository[i].graph.Network[3].nNode);
                        int y = rand.Next(0, rp.repository[i].graph.Network[3].nNode);

                        int currentN = rp.repository[i].graph.finalNet;
                        //GraphVariables.clsHWLS curr_HWLS = rp.repository[i].clsHWLS;
                        int workLoop = rp.repository[i].clsLoop.orgLoop;
                        int workSESE = rp.repository[i].clsSESE.finalSESE;
                        if (rp.repository[i].graph.check_untangle) {
                            currentN = rp.repository[i].graph.untangleNet;
                            //curr_HWLS = rp.repository[i].clsHWLS_Untangle;
                            workLoop = rp.repository[i].clsLoop.untangleLoop;
                            workSESE = rp.repository[i].clsSESE.untangleSESE;
                        }

                        if (check_XY(rp, currentN, i, x, y) == true) {
                            tempTime = DateTime.Now;
                            bool hold_PQ;
                            if (rp.repository[i].graph.check_untangle)
                                hold_PQ = PQ.start_ProcessQuery(ref rp.repository[i].graph, currentN, ref rp.repository[i].clsHWLS_Untangle, rp.repository[i].clsLoop, workLoop, rp.repository[i].clsSESE, workSESE, x, y, flag_Check);
                            else
                                hold_PQ = PQ.start_ProcessQuery(ref rp.repository[i].graph, currentN, ref rp.repository[i].clsHWLS, rp.repository[i].clsLoop, workLoop, rp.repository[i].clsSESE, workSESE, x, y, flag_Check);

                            SessionTime[j] = SessionTime[j] + (DateTime.Now - tempTime).TotalMilliseconds;
                            RandCount++;
                        }

                    } while (RandCount < maxIteration);                   

                    SessionTime[j] = SessionTime[j] / maxIteration; //not divide by maxIteration =>> lower than that
                    flag_Check[j] = true;
                }
                //store file at once model
                store_File(@"I:\PQ_Simulation.csv", rp.repository[i].ID_model, SessionTime);
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            MessageBox.Show("Finish in " + Run_Times_total.ToString() + " seconds", "Message");

            //display the Results_List[] - list of satisfied models to user.
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string modelName = row.Cells[0].Value.ToString();
                //string modelName = listView1.SelectedItems.ToString();
                string filePath = openFileDialog1.FileNames[0]; //directory from Indexing folder
                string directoryPath = Path.GetDirectoryName(filePath);
                string inputFileName = directoryPath + "\\" + modelName;
                frmShowFullModel frmShow = new frmShowFullModel(inputFileName);
                //frmShow.Parent = this;
                //frmShow.Show(this);
                frmShow.ShowDialog(this);
            }                        
        }

        private void exp1_indexingPerformance()
        {
            //work with rp.repository only
            //experimental setup:
            //run indexing 10 times, get average (only input flag_check[4] for indexing)
            string[] sFilePaths;
            string[] sFileNames;
            int maxInteration = 1;

            openFileDialog1.Title = "Browse";
            openFileDialog1.Filter = "Network Documents (*.net) | *.net";
            openFileDialog1.FileName = "";
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName == "") return;

            sFilePaths = openFileDialog1.FileNames;
            sFileNames = openFileDialog1.SafeFileNames;
            rp.repository = new GraphVariables.clsRepository.processModel[sFileNames.Length];
            rp.nModel = sFileNames.Length;

            bool[] flag_Check = new bool[8];
            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = false; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur

            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            for (int run = 0; run < sFileNames.Length; run++)
            {
                DateTime stTime = new DateTime();  double Run_Times = 0;
                for (int i = 0; i < maxInteration; i++)
                {
                    stTime = DateTime.Now;
                    Initialize_Details_Model(ref rp, run);
                    loadGraph.Load_Data(ref rp.repository[run].graph, rp.repository[run].graph.orgNet, sFilePaths[run], true);
                    rp.repository[run].ID_model = sFileNames[run];
                    gProAnalyzer.Functionalities.IndexingPM.start_Indexing_Acyclic(ref rp.repository[run].graph, ref rp.repository[run].clsHWLS, ref rp.repository[run].clsHWLS_Untangle, ref rp.repository[run].clsLoop, ref rp.repository[run].clsSESE, flag_Check);

                    Run_Times = (DateTime.Now - stTime).TotalMilliseconds;                    
                }
                rp.repository[run].Time_Index = Run_Times / maxInteration;
                //store in file
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"F:\PQ_Simulation_index.csv", true))
                {
                    file.WriteLine(rp.repository[run].ID_model + ";" + rp.repository[run].graph.Network[3].nNode + ";" + rp.repository[run].Time_Index.ToString());
                }
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            MessageBox.Show("Finish indexing with " + maxInteration + "runs in: " + Run_Times_total.ToString() + " seconds", "Message");            
        }

        private void exp2_computingPerformance()
        {
            //get random 10 pair of activities in current models => run for each query (random 6 queries) => average it
            //store one datapoint for one model only     

            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            for (int run = 0; run < 2; run++)
            {
                for (int model_index = 0; model_index < rp.nModel; model_index++)
                {
                    //get 10 random pair of activities
                    int[,] randPair = new int[10, 2];
                    //======
                    randPair = getRand_pair_xy(model_index);

                    double Run_Times = 0;
                    DateTime stTime = new DateTime();
                    stTime = DateTime.Now;

                    //for each random pair of x,y in current G.
                    for (int i = 0; i < 10; i++)
                    {
                        //=>perform Q0
                        recordQueryTime(model_index, 0, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q1
                        recordQueryTime(model_index, 1, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q2
                        recordQueryTime(model_index, 2, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q3
                        recordQueryTime(model_index, 3, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q4
                        recordQueryTime(model_index, 4, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q6
                        recordQueryTime(model_index, 6, randPair[i, 0], randPair[i, 1]);
                    }
                    Run_Times = (DateTime.Now - stTime).TotalMilliseconds / 60; //for average 1 query time
                    string SaveFile = @"F:\PQ_Simulation_Query_" + run.ToString() + ".csv";
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(SaveFile, true))
                    {
                        file.WriteLine(rp.repository[model_index].ID_model + ";" + rp.repository[model_index].graph.Network[3].nNode + ";" + Run_Times.ToString());
                    }
                }
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            MessageBox.Show("Finish query experiment with " + Run_Times_total.ToString() + " seconds", "Message");
        }

        private void exp3_Combine_indexingQuery() //FOR TEMPORARY USE ==> GET MODEL SIZE AFTER PREPROCESSING
        {
            //work with rp.repository only
            //experimental setup:
            //run indexing 10 times, get average (only input flag_check[4] for indexing)
            string[] sFilePaths;
            string[] sFileNames;
            int maxInteration = 1;

            openFileDialog1.Title = "Browse";
            openFileDialog1.Filter = "Network Documents (*.net) | *.net";
            openFileDialog1.FileName = "";
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName == "") return;

            sFilePaths = openFileDialog1.FileNames;
            sFileNames = openFileDialog1.SafeFileNames;
            rp.repository = new GraphVariables.clsRepository.processModel[sFileNames.Length];
            rp.nModel = sFileNames.Length;

            bool[] flag_Check = new bool[8];
            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = false; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur

            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            for (int run = 0; run < sFileNames.Length; run++)
            {
                DateTime stTime = new DateTime(); double Run_Times = 0;
                for (int i = 0; i < maxInteration; i++)
                {
                    stTime = DateTime.Now;
                    Initialize_Details_Model(ref rp, run);
                    loadGraph.Load_Data(ref rp.repository[run].graph, rp.repository[run].graph.orgNet, sFilePaths[run], true);
                    rp.repository[run].ID_model = sFileNames[run];
                    gProAnalyzer.Functionalities.IndexingPM.start_Indexing_Acyclic(ref rp.repository[run].graph, ref rp.repository[run].clsHWLS, ref rp.repository[run].clsHWLS_Untangle, ref rp.repository[run].clsLoop, ref rp.repository[run].clsSESE, flag_Check);

                    Run_Times = (DateTime.Now - stTime).TotalMilliseconds;
                }
                rp.repository[run].Time_Index = Run_Times / maxInteration;
                //store in file
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"F:\PQ_Simulation_MODEL_SIZE.csv", true))
                {
                    file.WriteLine(rp.repository[run].ID_model + ";" + rp.repository[run].graph.Network[3].nNode + ";" + rp.repository[run].Time_Index.ToString());
                }
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            MessageBox.Show("Finish indexing with " + maxInteration + "runs in: " + Run_Times_total.ToString() + " seconds", "Message");
        }

        private void exp3_pureModelchecking()
        {
            //indexing time (average) & computing time (same setup with exp2) (average) =>> one datapoint for one model
            //==> Store in file
            //Proposition 2 (only instance subgraph (for input G as a subgraph) (same setup wiht exp2) (average) => one datapoint for one model.
            //==> Store in file

            //get random 10 pair of activities in current models => run for each query (random 6 queries) => average it
            //store one datapoint for one model only     

            DateTime totalTime = new DateTime(); totalTime = DateTime.Now; double Run_Times_total = 0;
            for (int run = 0; run < 2; run++)
            {
                for (int model_index = 0; model_index < rp.nModel; model_index++)
                {
                    //get 10 random pair of activities
                    int[,] randPair = new int[10, 2];
                    //======
                    randPair = getRand_pair_xy(model_index);

                    double Run_Times = 0;
                    DateTime stTime = new DateTime();
                    stTime = DateTime.Now;

                    //for each random pair of x,y in current G.
                    for (int i = 0; i < 10; i++)
                    {
                        //=>perform Q0
                        recordQueryTime_pureModelchecking(model_index, 0, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q1
                        recordQueryTime_pureModelchecking(model_index, 1, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q2
                        recordQueryTime_pureModelchecking(model_index, 2, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q3
                        recordQueryTime_pureModelchecking(model_index, 3, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q4
                        recordQueryTime_pureModelchecking(model_index, 4, randPair[i, 0], randPair[i, 1]);
                        //=>perform Q6
                        recordQueryTime_pureModelchecking(model_index, 6, randPair[i, 0], randPair[i, 1]);
                    }
                    Run_Times = (DateTime.Now - stTime).TotalMilliseconds / 60; //for average 1 query time
                    string SaveFile = @"F:\PQ_Simulation_Query_PureModelChecking_" + run.ToString() + ".csv";
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(SaveFile, true))
                    {
                        file.WriteLine(rp.repository[model_index].ID_model + ";" + rp.repository[model_index].graph.Network[3].nNode + ";" + Run_Times.ToString());
                    }
                }
            }
            Run_Times_total = (DateTime.Now - totalTime).TotalSeconds;
            MessageBox.Show("Finish query experiment with " + Run_Times_total.ToString() + " seconds", "Message");
        }

        private void recordQueryTime_pureModelchecking(int model_index, int BhR_index, int x, int y)
        {
            if (rp.repository == null)
            {
                MessageBox.Show("Repository not found", "Error");
                return;
            }
            //Initialized
            PQ = new gProAnalyzer.Functionalities.ProcessQuery();
            bool[] flag_Check = new bool[8];
            string[] Results_List = new string[rp.nModel];

            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = true; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur

            if (BhR_index == 0)
                flag_Check[0] = false;
            if (BhR_index == 1)
                flag_Check[1] = false;
            if (BhR_index == 2)
                flag_Check[2] = false;
            if (BhR_index == 3)
                flag_Check[3] = false;
            if (BhR_index == 4)
                flag_Check[4] = false;
            if (BhR_index == 6)
                flag_Check[6] = false;

            int i = model_index;
            int currentN = rp.repository[i].graph.finalNet;
            //GraphVariables.clsHWLS curr_HWLS = rp.repository[i].clsHWLS;
            int workLoop = rp.repository[i].clsLoop.orgLoop;
            int workSESE = rp.repository[i].clsSESE.finalSESE;
            //if (rp.repository[i].graph.check_untangle)
            //{
            //    currentN = rp.repository[i].graph.untangleNet;
            //    //curr_HWLS = rp.repository[i].clsHWLS_Untangle;
            //    workLoop = rp.repository[i].clsLoop.untangleLoop;
            //    workSESE = rp.repository[i].clsSESE.untangleSESE;
            //}
            if (check_XY(rp, currentN, i, x, y) == true)
            {
                bool hold_PQ;
                hold_PQ = PQ.start_ProcessQuery_pureModelChecking(ref rp.repository[i].graph, currentN, ref rp.repository[i].clsHWLS, rp.repository[i].clsLoop, workLoop, rp.repository[i].clsSESE, workSESE, x, y, flag_Check);
            }
            //store file at once model
            //store_File(@"I:\PQ_Simulation.csv", rp.repository[i].ID_model, "__");

        }

        private void recordQueryTime(int model_index, int BhR_index, int x, int y)
        {
            if (rp.repository == null)
            {
                MessageBox.Show("Repository not found", "Error");
                return;
            }
            //Initialized
            PQ = new gProAnalyzer.Functionalities.ProcessQuery();
            bool[] flag_Check = new bool[8];
            string[] Results_List = new string[rp.nModel];

            //get query request (what behavior relation need to check) =>> flag_Check[] = false => get this relation      
            flag_Check[0] = true; //totalConcurrency
            flag_Check[1] = true; //existConcurrency
            flag_Check[2] = true; //totalCausal
            flag_Check[3] = true; //existCausal
            //====================
            flag_Check[4] = true; //canConflict
            flag_Check[5] = true; //NOTcanConflict
            flag_Check[6] = true; //canCoocur
            flag_Check[7] = true; //NOTcanCoocur

            if (BhR_index == 0)
                flag_Check[0] = false;
            if (BhR_index == 1)
                flag_Check[1] = false;
            if (BhR_index == 2)
                flag_Check[2] = false;
            if (BhR_index == 3)
                flag_Check[3] = false;
            if (BhR_index == 4)
                flag_Check[4] = false;
            if (BhR_index == 6)
                flag_Check[6] = false;

            int i = model_index;
            int currentN = rp.repository[i].graph.finalNet;
            //GraphVariables.clsHWLS curr_HWLS = rp.repository[i].clsHWLS;
            int workLoop = rp.repository[i].clsLoop.orgLoop;
            int workSESE = rp.repository[i].clsSESE.finalSESE;
            //if (rp.repository[i].graph.check_untangle)
            //{
            //    currentN = rp.repository[i].graph.untangleNet;
            //    //curr_HWLS = rp.repository[i].clsHWLS_Untangle;
            //    workLoop = rp.repository[i].clsLoop.untangleLoop;
            //    workSESE = rp.repository[i].clsSESE.untangleSESE;
            //}
            if (check_XY(rp, currentN, i, x, y) == true)
            {
                bool hold_PQ;
                hold_PQ = PQ.start_ProcessQuery(ref rp.repository[i].graph, currentN, ref rp.repository[i].clsHWLS, rp.repository[i].clsLoop, workLoop, rp.repository[i].clsSESE, workSESE, x, y, flag_Check);
            }
            //store file at once model
            //store_File(@"I:\PQ_Simulation.csv", rp.repository[i].ID_model, "__");

        }
        private int[,] getRand_pair_xy(int model_index) //get random pair of 10
        {
            int[,] randPair = new int[10, 2];
            int max_count = 0;
            int currentN = rp.repository[model_index].graph.finalNet;            
            do
            {
                Random rand = new Random();
                Random rand2 = new Random();
                int x = rand.Next(0, rp.repository[model_index].graph.Network[currentN].nNode);
                int y = rand2.Next(0, rp.repository[model_index].graph.Network[currentN].nNode);
                rand = new Random();
                rand2 = new Random();
                if (check_XY(rp, currentN, model_index, x, y) == true && (x != y))
                {
                    randPair[max_count, 0] = x;
                    randPair[max_count, 1] = y;
                    max_count++;
                }
            } while (max_count < 10);
            return randPair;
            /*
            for (int i = 0; i < rp.repository[model_index].graph.Network[currentN].nNode; i++)
            {
                for (int j = 0; j < rp.repository[model_index].graph.Network[currentN].nNode; j++)
                {
                    if (i != j)
                    {
                        if (max_count == 10) return randPair;
                        if (check_XY(rp, currentN, model_index, i, j) == true)
                        {
                            randPair[max_count, 0] = i;
                            randPair[max_count, 1] = j;
                            max_count++;
                        }
                    }
                }
            }
            return randPair;
            */
        }

        private void b_indexTime_Click(object sender, EventArgs e)
        {
            exp1_indexingPerformance();
        }

        private void b_queryTime_Click(object sender, EventArgs e)
        {
            exp2_computingPerformance();
        }

        private void b_combine_Click(object sender, EventArgs e)
        {
            exp3_Combine_indexingQuery();
        }

        private void b_Prop2_Click(object sender, EventArgs e)
        {
            exp3_pureModelchecking();
        }

        private void frmProcessQuery_Load(object sender, EventArgs e)
        {
            //this.BackColor = Color.LimeGreen;
            //this.TransparencyKey = BackColor;
        }
    }
}
