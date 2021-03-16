using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gProAnalyzer.Functionalities
{
    //Input a query "text" and output the list of models as a result.
    class ProcessQuery
    {
        //QUERY INTENT//
        // CanCoocur;
        // CanConflict;

        // CanCausal;
        // TotalCausal;

        // CanConcurrent;
        // TotalConcurrent;

        private gProAnalyzer.Ultilities.findIntersection fndIntersec;
        private gProAnalyzer.Ultilities.reduceGraph reduceG;
        private gProAnalyzer.Ultilities.extendGraph extendG;
        private gProAnalyzer.Ultilities.makeSubNetwork mkSubNet;
        private gProAnalyzer.Ultilities.AnalyseBehavior_InstF anlyzBh_InstF;
        public static frmShowFullModel_Debug frmShow;

        private string[] Attrs;
        private void Initialize_All()
        {
            fndIntersec = new Ultilities.findIntersection();
            reduceG = new Ultilities.reduceGraph();
            extendG = new Ultilities.extendGraph();
            mkSubNet = new Ultilities.makeSubNetwork();
            anlyzBh_InstF = new Ultilities.AnalyseBehavior_InstF();
        }
        
        //Parse a query text to the "Binary* Expression Tree" as output.
        private void QueryParser(string queryText)
        {
            //keywords detection, 
        }

        //Based on QueryParser => Decide the priorities of each expression => Query Guidance.
        private void get_executionPlan()
        {

        }

        //Locate the attribute Attr in the "DECOMPOSED TREE of PROCESS MODEL"
        //Attr is in Bwd, Fwd, PdF, iDF, SESE?
        private void LocateAttrs(string Attr)
        {

        }

        //temporary use ONLY
        //return TRUE or FALSE whether a given process model satisfy the query or not!
        public bool start_ProcessQuery(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int x, int y, bool[] flag_Check)
        {
            Initialize_All();

            //Check occurence of X or/and Y => return false if not satisfy
            bool check = false;
            bool check_2 = false;
            for (int i = 0; i < graph.Network[currentN].nNode; i++) {
                if (x == i) check = true;
                if (y == i) check_2 = true;
            }
            if (!(check && check_2)) return false;

            //(X and Y) MUST occur in Process Model to be processed further
            //bool[] final_BhPrfl = get_Final_BhPrfl(ref graph, currentN, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, x, y, flag_Check);

            //bool[] final_BhPrfl = get_Final_BhPrfl_Acyclic(ref graph, currentN, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, x, y, flag_Check); //now, only acyclic
            bool[] final_BhPrfl = get_Final_BhPrfl_Acyclic_Updated(ref graph, currentN, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, x, y, flag_Check); //now, only acyclic
            
            
            if (final_BhPrfl == null) return false;

            //checking for only single behavior relation of (x, y)
            for (int i = 0; i < flag_Check.Length; i++) {
                if (flag_Check[i] == false)
                    if (final_BhPrfl[i] == true)
                        return true;
            }
            return false;
        }

        //CORE FUNCTION => get_Final_BehaviorRelation of given (X, Y) //Cyclic and Acyclic
        private bool[] get_Final_BhPrfl(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int x, int y, bool[] flag_Check)
        {
            bool[] final_BhPrfl = new bool[8];
            bool[] returnBhPrfl = new bool[8];
            //bool[] flag_Check = new bool[8]; //=> Set index = TRUE if want to DISABLE checking relation

            graph.Network[graph.reduceNet] = graph.Network[currentN]; //currentN should be finalNet
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0); //copy to reduceNet

            //Check x, y in exclusive relation based on Dominator tree //MIGHT NOT NEED, but it interesting //if (x, y) in total conflict relation => stop calculation. Return();
            
            //(x, y) must exist coocurrence
            int x_HWLS = get_index_HWLS(clsHWLS, x);
            int y_HWLS = get_index_HWLS(clsHWLS, y);
            int common_HWLS = -1;
            int x_nearestBlock = -1;
            int x_nearestBlock_1 = -1; //child of x_nearestBlock which contain (x)
            int y_nearestBlock = -1;
            int y_nearestBlock_1 = -1;
            int R_Block = -1;
            common_HWLS = get_commonImmediate_HWLS(clsHWLS, x_HWLS, y_HWLS, ref x_nearestBlock, ref y_nearestBlock, ref x_nearestBlock_1, ref y_nearestBlock_1);

            if (x_HWLS != -1 && y_HWLS != -1 && common_HWLS != -1) {                
                int x_header = -1; //from x_nearestBlock
                int x_header_R = -1; //child block of x_header
                int y_header = -1; //from y_nearestBlock
                int y_header_R = -1;
                bool x_PdF = false; //x in PdF() of nearest block (IL)
                bool y_PdF = false;

                if (clsHWLS.FBLOCK.FBlock[x_nearestBlock].SESE == true || clsHWLS.FBLOCK.FBlock[x_nearestBlock].nEntry == 1)
                    x_header = clsHWLS.FBLOCK.FBlock[x_nearestBlock].Entry[0];
                if (clsHWLS.FBLOCK.FBlock[y_nearestBlock].SESE == true || clsHWLS.FBLOCK.FBlock[y_nearestBlock].nEntry == 1)
                    y_header = clsHWLS.FBLOCK.FBlock[y_nearestBlock].Entry[0];
                //===Search (x, y) in current  reduction graph. If exist x , y => marking {x_exist = true} {y_exist = false} ==========
                if (clsHWLS.FBLOCK.FBlock[x_nearestBlock].nEntry > 1) {
                    x_header = clsHWLS.FBLOCK.FBlock[x_nearestBlock].CIPd;
                    //if (x_HWLS == x_nearestBlock)
                    x_PdF = inPdFlow(graph, currentN, x, x_header); //check x in PdFlow or not - in finalNet - (Simple, just check in Dominator tree of CIPd)                        
                }
                if (clsHWLS.FBLOCK.FBlock[y_nearestBlock].nEntry > 1) {
                    y_header = clsHWLS.FBLOCK.FBlock[y_nearestBlock].CIPd;
                    //if (y_HWLS == y_nearestBlock)
                    y_PdF = inPdFlow(graph, currentN, y, y_header);
                }

                //Manipulate the position of x_HWLS and y_HWLS in HWLS tree (The position between 5 variable x_HWLS, y_HWLS, x_nearest, y_nearest, cmmon_HWLS)
                if (common_HWLS == x_nearestBlock)
                    x_header = x;
                if (common_HWLS == y_nearestBlock)
                    y_header = y;
                //CREATE (x_header_R and y_header_R) which is only used for the case (x or y) in IL PdFlow()
                if (x_PdF == true)
                    if (x_nearestBlock != x_nearestBlock_1)
                        x_header_R = clsHWLS.FBLOCK.FBlock[x_nearestBlock_1].Entry[0]; //no IL nested allowed
                    else
                        x_header_R = x;                
                if (y_PdF == true) 
                    if (y_nearestBlock != y_nearestBlock_1)
                        y_header_R = clsHWLS.FBLOCK.FBlock[y_nearestBlock_1].Entry[0];
                    else
                        y_header_R = y;

                //====================== COMPUTING ========================
                //SESE (use x_header_R and y_header_R)
                if (clsHWLS.FBLOCK.FBlock[common_HWLS].SESE == true) {
                    get_BhR_xy_SESE(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, x_header_R, y_header_R, flag_Check);
                    //return null;
                }
                //NL (use x_header_R and y_header_R)
                if (clsHWLS.FBLOCK.FBlock[common_HWLS].SESE == false && clsHWLS.FBLOCK.FBlock[common_HWLS].nEntry == 1) {
                    get_BhR_xy_NL(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, x_header_R, y_header_R, flag_Check);
                }
                //IL (no use x_header_R and y_header_R)
                if (clsHWLS.FBLOCK.FBlock[common_HWLS].SESE == false && clsHWLS.FBLOCK.FBlock[common_HWLS].nEntry > 1) {
                    if (x_PdF && y_PdF) {
                        int p_common_HWLS = clsHWLS.FBLOCK.FBlock[common_HWLS].parentBlock;
                        if (clsHWLS.FBLOCK.FBlock[p_common_HWLS].SESE == true) //SESE
                            get_BhR_xy_SESE(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, p_common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, x_header, y_header, flag_Check);
                        if (clsHWLS.FBLOCK.FBlock[p_common_HWLS].SESE == false && clsHWLS.FBLOCK.FBlock[p_common_HWLS].nEntry == 1) //NL
                            get_BhR_xy_NL(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, p_common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, x_header, y_header, flag_Check);
                    }
                    if (x_PdF && !y_PdF) {
                        get_BhR_xy_IL_Simple(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, flag_Check);
                    }
                    if (!x_PdF && y_PdF) {
                        get_BhR_xy_IL_Simple(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, flag_Check);
                    }
                    if (!x_PdF && !y_PdF) {
                        get_BhR_xy_IL_Simple(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, flag_Check);
                    }
                }

                //====================== TRANSITIVE RELATION //======================
                //from now, we have behavior relation of (x_header, y_header) => NEXT find behavior relation of x and y
                //Deduct behavior relations of x and y.
                //using x_header and y_header to determine BH of (x, y)
                //===================================================================
                bool[] x_BhPrfl_d = new bool[8];
                bool[] y_BhPrfl_d = new bool[8];

                //get_behaviorProfile(x_header, x)  
                if (common_HWLS == x_HWLS)               
                    for (int i = 0; i < 8; i++) 
                        if (i != 4) 
                            x_BhPrfl_d[i] = true; //manipulate the position of x_HWLS and y_HWLS in domTree
                        else
                            x_BhPrfl_d[i] = false;                                                                             
                else
                    x_BhPrfl_d = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, x_nearestBlock, x, x_HWLS);

                //get_behaviorProfile(y_header, y) 
                if (common_HWLS == y_HWLS)               
                    for (int i = 0; i < 8; i++)
                        if (i != 4)
                            y_BhPrfl_d[i] = true; //manipulate the position of x_HWLS and y_HWLS in domTree
                        else
                            y_BhPrfl_d[i] = false;                                            
                else
                    y_BhPrfl_d = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, y_nearestBlock, y, y_HWLS);

                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_d, y_BhPrfl_d);

                //======================== COMPUTE LEFTOVER STRUCTURE //========================
                //Compute final behavior relation from START to common_HWLS or it parent (in case it is IL with x_PdF, y_PdF) //ALWAY EXIST a outter BLOCK (common_HWLS or it's parent) => alway SESE (not IL)
                //It simple: Get_behaviorProfile from ROOT header (called R_header) to common_HWLS header or it's parent header (called cc_header) => get_BehaviorProfile(R_header, cc_header)            
                int p_cc_HWLS = clsHWLS.FBLOCK.FBlock[common_HWLS].parentBlock;
                int cc_Block;
                if (clsHWLS.FBLOCK.FBlock[common_HWLS].nEntry > 1)
                    cc_Block = p_cc_HWLS;
                else
                    cc_Block = common_HWLS;
                R_Block = get_Root_Block(clsHWLS, cc_Block); //get R_header of cc_header

                //if (R_Block == cc_header) => no need to compute? if (cc_header = IL) => no need to compute; because with IL = common_HWLS, we alway compute outter BLOCK alltogether.                
                if (R_Block == cc_Block) {
                    final_BhPrfl = returnBhPrfl;
                }
                if (R_Block != cc_Block) {
                    final_BhPrfl = get_BehaviorProfile_All_Block(graph, currentN, clsHWLS, R_Block, cc_Block); //need to retrieve from indexing
                    //combine BhPrfl??
                    if (final_BhPrfl[2] == false) {
                        returnBhPrfl[0] = false;
                        returnBhPrfl[2] = false;
                    }
                    final_BhPrfl = returnBhPrfl;
                }
                //======================= END COMPUTE LEFTOVER STRUCTURE //=======================
            }            

            // =================== SPECIAL CASE ===================
            if (x_HWLS != -1 && y_HWLS != -1 && common_HWLS == -1) {
                //no common parent of 2 blocks
                int R_Block_x = get_Root_Block(clsHWLS, x_HWLS);
                int R_Block_y = get_Root_Block(clsHWLS, y_HWLS);
                bool[] x_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block_x, x, x_HWLS);
                bool[] y_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block_y, y, y_HWLS);

                bool canReach = checkPrecedence_xy(graph, currentN, clsHWLS.FBLOCK.FBlock[R_Block_x].Entry[0], clsHWLS.FBLOCK.FBlock[R_Block_y].Entry[0]);
                returnBhPrfl = get_BhR_sequence(canReach);
                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_temp, y_BhPrfl_temp);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS == -1 && y_HWLS != -1) {
                R_Block = get_Root_Block(clsHWLS, y_HWLS);
                bool canReach = checkPrecedence_xy(graph, currentN, x, clsHWLS.FBLOCK.FBlock[R_Block].Entry[0]);
                bool[] x_BhPrfl_temp = new bool[8];
                bool[] y_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block, y, y_HWLS);   
                for (int i = 0; i < 8; i++) x_BhPrfl_temp[i] = true;  

                returnBhPrfl = get_BhR_sequence(canReach);
                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_temp, y_BhPrfl_temp);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS != -1 && y_HWLS == -1) {
                R_Block = get_Root_Block(clsHWLS, x_HWLS);
                bool canReach = checkPrecedence_xy(graph, currentN, clsHWLS.FBLOCK.FBlock[R_Block].Entry[0], y);
                bool[] x_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block, x, x_HWLS);
                bool[] y_BhPrfl_temp = new bool[8];
                for (int i = 0; i < 8; i++) x_BhPrfl_temp[i] = true;

                returnBhPrfl = get_BhR_sequence(canReach);
                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_temp, y_BhPrfl_temp);
                final_BhPrfl = returnBhPrfl;
            } 
            if (x_HWLS == -1 && y_HWLS == -1) {
                bool canReach = checkPrecedence_xy(graph, currentN, x, y);
                returnBhPrfl = get_BhR_sequence(canReach);
                final_BhPrfl = returnBhPrfl;
            }
            // ================= END SPECIAL CASE ==================

            return final_BhPrfl;
        }

        private bool[] get_Final_BhPrfl_Acyclic(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int x, int y, bool[] flag_Check)
        {
            bool[] final_BhPrfl = new bool[8];
            bool[] returnBhPrfl = new bool[8];
            //bool[] flag_Check = new bool[8]; //=> Set index = TRUE if want to DISABLE checking relation

            graph.Network[graph.reduceNet] = graph.Network[currentN]; //currentN should be finalNet
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0); //copy to reduceNet

            //Check x, y in exclusive relation based on Dominator tree //MIGHT NOT NEED, but it interesting //if (x, y) in total conflict relation => stop calculation. Return();

            //(x, y) must exist coocurrence
            int x_HWLS = get_index_HWLS(clsHWLS, x);
            int y_HWLS = get_index_HWLS(clsHWLS, y);
            int common_HWLS = -1;
            int x_nearestBlock = -1;
            int x_nearestBlock_1 = -1; //child of x_nearestBlock which contain (x)
            int y_nearestBlock = -1;
            int y_nearestBlock_1 = -1;
            int R_Block = -1;
            common_HWLS = get_commonImmediate_HWLS(clsHWLS, x_HWLS, y_HWLS, ref x_nearestBlock, ref y_nearestBlock, ref x_nearestBlock_1, ref y_nearestBlock_1);

            if (x_HWLS != -1 && y_HWLS != -1 && common_HWLS != -1)
            {
                int x_header = -1; //from x_nearestBlock
                int x_header_R = -1; //child block of x_header
                int y_header = -1; //from y_nearestBlock
                int y_header_R = -1;
                bool x_PdF = false; //x in PdF() of nearest block (IL)
                bool y_PdF = false;

                if (clsHWLS.FBLOCK.FBlock[x_nearestBlock].SESE == true || clsHWLS.FBLOCK.FBlock[x_nearestBlock].nEntry == 1)
                    x_header = clsHWLS.FBLOCK.FBlock[x_nearestBlock].Entry[0];
                if (clsHWLS.FBLOCK.FBlock[y_nearestBlock].SESE == true || clsHWLS.FBLOCK.FBlock[y_nearestBlock].nEntry == 1)
                    y_header = clsHWLS.FBLOCK.FBlock[y_nearestBlock].Entry[0];
                                

                //Manipulate the position of x_HWLS and y_HWLS in HWLS tree (The position between 5 variable x_HWLS, y_HWLS, x_nearest, y_nearest, cmmon_HWLS)
                if (common_HWLS == x_nearestBlock)
                    x_header = x;
                if (common_HWLS == y_nearestBlock)
                    y_header = y;
                //CREATE (x_header_R and y_header_R) which is only used for the case (x or y) in IL PdFlow()
                if (x_PdF == true)
                    if (x_nearestBlock != x_nearestBlock_1)
                        x_header_R = clsHWLS.FBLOCK.FBlock[x_nearestBlock_1].Entry[0]; //no IL nested allowed
                    else
                        x_header_R = x;
                if (y_PdF == true)
                    if (y_nearestBlock != y_nearestBlock_1)
                        y_header_R = clsHWLS.FBLOCK.FBlock[y_nearestBlock_1].Entry[0];
                    else
                        y_header_R = y;

                //====================== COMPUTING ========================
                //SESE (use x_header_R and y_header_R)
                if (clsHWLS.FBLOCK.FBlock[common_HWLS].SESE == true)
                {
                    get_BhR_xy_SESE(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, x_header_R, y_header_R, flag_Check);
                    //return null;
                }                

                //====================== TRANSITIVE RELATION //======================
                //from now, we have behavior relation of (x_header, y_header) => NEXT find behavior relation of x and y
                //Deduct behavior relations of x and y.
                //using x_header and y_header to determine BH of (x, y)
                //===================================================================
                if (flag_Check[6] == false) //only transitive for conFlict relation
                {

                    bool[] x_BhPrfl_d = new bool[8];
                    bool[] y_BhPrfl_d = new bool[8];

                    //get_behaviorProfile(x_header, x)  
                    if (common_HWLS == x_HWLS)
                        for (int i = 0; i < 8; i++)
                            if (i != 4)
                                x_BhPrfl_d[i] = true; //manipulate the position of x_HWLS and y_HWLS in domTree
                            else
                                x_BhPrfl_d[i] = false;
                    else
                        x_BhPrfl_d = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, x_nearestBlock, x, x_HWLS);

                    //get_behaviorProfile(y_header, y) 
                    if (common_HWLS == y_HWLS)
                        for (int i = 0; i < 8; i++)
                            if (i != 4)
                                y_BhPrfl_d[i] = true; //manipulate the position of x_HWLS and y_HWLS in domTree
                            else
                                y_BhPrfl_d[i] = false;
                    else
                        y_BhPrfl_d = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, y_nearestBlock, y, y_HWLS);

                    final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_d, y_BhPrfl_d);
                }

                //======================== COMPUTE LEFTOVER STRUCTURE //========================
                //Compute final behavior relation from START to common_HWLS or it parent (in case it is IL with x_PdF, y_PdF) //ALWAY EXIST a outter BLOCK (common_HWLS or it's parent) => alway SESE (not IL)
                //It simple: Get_behaviorProfile from ROOT header (called R_header) to common_HWLS header or it's parent header (called cc_header) => get_BehaviorProfile(R_header, cc_header)   

                final_BhPrfl = returnBhPrfl; //not dealing with ROOT structure
                
                //======================= END COMPUTE LEFTOVER STRUCTURE //=======================
            }

            // =================== SPECIAL CASE ===================
            if (x_HWLS != -1 && y_HWLS != -1 && common_HWLS == -1)
            {
                //no common parent of 2 blocks
                int R_Block_x = get_Root_Block(clsHWLS, x_HWLS);
                int R_Block_y = get_Root_Block(clsHWLS, y_HWLS);
                bool[] x_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block_x, x, x_HWLS);
                bool[] y_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block_y, y, y_HWLS);

                bool canReach = checkPrecedence_xy(graph, currentN, clsHWLS.FBLOCK.FBlock[R_Block_x].Entry[0], clsHWLS.FBLOCK.FBlock[R_Block_y].Entry[0]);
                returnBhPrfl = get_BhR_sequence(canReach);
                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_temp, y_BhPrfl_temp);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS == -1 && y_HWLS != -1)
            {
                R_Block = get_Root_Block(clsHWLS, y_HWLS);
                bool canReach = checkPrecedence_xy(graph, currentN, x, clsHWLS.FBLOCK.FBlock[R_Block].Entry[0]);
                bool[] x_BhPrfl_temp = new bool[8];
                bool[] y_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block, y, y_HWLS);
                for (int i = 0; i < 8; i++) x_BhPrfl_temp[i] = false;

                returnBhPrfl = get_BhR_sequence(canReach);
                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_temp, y_BhPrfl_temp);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS != -1 && y_HWLS == -1)
            {
                R_Block = get_Root_Block(clsHWLS, x_HWLS);
                bool canReach = checkPrecedence_xy(graph, currentN, clsHWLS.FBLOCK.FBlock[R_Block].Entry[0], y);
                bool[] x_BhPrfl_temp = get_BehaviorProfile_All(graph, currentN, clsHWLS, -1, R_Block, x, x_HWLS);
                bool[] y_BhPrfl_temp = new bool[8];
                for (int i = 0; i < 8; i++) y_BhPrfl_temp[i] = false;

                returnBhPrfl = get_BhR_sequence(canReach);
                final_TransitiveRelation(ref returnBhPrfl, x_BhPrfl_temp, y_BhPrfl_temp);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS == -1 && y_HWLS == -1)
            {
                //bool canReach = checkPrecedence_xy(graph, currentN, x, y);
                //returnBhPrfl = get_BhR_sequence(canReach);
                //final_BhPrfl = returnBhPrfl;
            }
            // ================= END SPECIAL CASE ==================
            return final_BhPrfl;
        }

        private bool[] get_Final_BhPrfl_Acyclic_Updated(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int x, int y, bool[] flag_Check)
        {
            //ALL USING REDUCE_NET ONLY??

            bool[] final_BhPrfl = new bool[8];
            bool[] returnBhPrfl = new bool[8];
            //bool[] flag_Check = new bool[8]; //=> Set index = TRUE if want to DISABLE checking relation

            graph.Network[graph.reduceNet] = graph.Network[currentN]; //currentN should be finalNet
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0); //copy to reduceNet

            //Check x, y in exclusive relation based on Dominator tree //MIGHT NOT NEED, but it interesting //if (x, y) in total conflict relation => stop calculation. Return();

            //(x, y) must exist coocurrence
            int x_HWLS = get_index_HWLS(clsHWLS, x);
            int y_HWLS = get_index_HWLS(clsHWLS, y);
            int common_HWLS = -1;
            int x_nearestBlock = -1;
            int x_nearestBlock_1 = -1; //child of x_nearestBlock which contain (x)
            int y_nearestBlock = -1;
            int y_nearestBlock_1 = -1;
            int R_Block = -1;
            common_HWLS = get_commonImmediate_HWLS(clsHWLS, x_HWLS, y_HWLS, ref x_nearestBlock, ref y_nearestBlock, ref x_nearestBlock_1, ref y_nearestBlock_1);

            if (x_HWLS != -1 && y_HWLS != -1 && common_HWLS != -1)
            {
                int x_header = -1; //from x_nearestBlock
                int x_header_R = -1; //child block of x_header
                int y_header = -1; //from y_nearestBlock
                int y_header_R = -1;
                bool x_PdF = false; //x in PdF() of nearest block (IL)
                bool y_PdF = false;

                if (clsHWLS.FBLOCK.FBlock[x_nearestBlock].SESE == true || clsHWLS.FBLOCK.FBlock[x_nearestBlock].nEntry == 1)
                    x_header = clsHWLS.FBLOCK.FBlock[x_nearestBlock].Entry[0];
                if (clsHWLS.FBLOCK.FBlock[y_nearestBlock].SESE == true || clsHWLS.FBLOCK.FBlock[y_nearestBlock].nEntry == 1)
                    y_header = clsHWLS.FBLOCK.FBlock[y_nearestBlock].Entry[0];


                //Manipulate the position of x_HWLS and y_HWLS in HWLS tree (The position between 5 variable x_HWLS, y_HWLS, x_nearest, y_nearest, cmmon_HWLS)
                if (common_HWLS == x_nearestBlock)
                    x_header = x;
                if (common_HWLS == y_nearestBlock)
                    y_header = y;
                //CREATE (x_header_R and y_header_R) which is only used for the case (x or y) in IL PdFlow()
                if (x_PdF == true)
                    if (x_nearestBlock != x_nearestBlock_1)
                        x_header_R = clsHWLS.FBLOCK.FBlock[x_nearestBlock_1].Entry[0]; //no IL nested allowed
                    else
                        x_header_R = x;
                if (y_PdF == true)
                    if (y_nearestBlock != y_nearestBlock_1)
                        y_header_R = clsHWLS.FBLOCK.FBlock[y_nearestBlock_1].Entry[0];
                    else
                        y_header_R = y;

                //====================== COMPUTING ========================
                //SESE (use x_header_R and y_header_R)
                if (clsHWLS.FBLOCK.FBlock[common_HWLS].SESE == true)
                {
                    get_BhR_xy_SESE(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, x_PdF, y_PdF, x_header, y_header, x_header_R, y_header_R, flag_Check);
                    //return null;
                }            
                final_BhPrfl = returnBhPrfl; //not dealing with ROOT structure
            }

            // =================== SPECIAL CASE ===================
            if (x_HWLS != -1 && y_HWLS != -1 && common_HWLS == -1)
            {
                //no common parent of 2 blocks
                int R_Block_x = get_Root_Block(clsHWLS, x_HWLS);
                int R_Block_y = get_Root_Block(clsHWLS, y_HWLS);
                int header_R_Block_x = clsHWLS.FBLOCK.FBlock[R_Block_x].Entry[0];
                int header_R_Block_y = clsHWLS.FBLOCK.FBlock[R_Block_y].Entry[0];
                get_BhR_xy_Root(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, common_HWLS, ref returnBhPrfl, flag_Check, header_R_Block_x, header_R_Block_y);
                bool canReach = false;
                if (returnBhPrfl[2] == true || returnBhPrfl[3] == true || returnBhPrfl[6] == true)
                    canReach = true;
                returnBhPrfl = get_BhR_sequence(canReach);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS == -1 && y_HWLS != -1)
            {
                //x not in any SESE, y is in SESE
                R_Block = get_Root_Block(clsHWLS, y_HWLS);
                bool canReach = checkPrecedence_xy(graph, currentN, x, clsHWLS.FBLOCK.FBlock[R_Block].Entry[0]);
                returnBhPrfl = get_BhR_sequence(canReach);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS != -1 && y_HWLS == -1)
            {
                R_Block = get_Root_Block(clsHWLS, x_HWLS);
                bool canReach = checkPrecedence_xy(graph, currentN, clsHWLS.FBLOCK.FBlock[R_Block].Entry[0], y);
                returnBhPrfl = get_BhR_sequence(canReach);
                final_BhPrfl = returnBhPrfl;
            }
            if (x_HWLS == -1 && y_HWLS == -1)
            {
                bool canReach = checkPrecedence_xy(graph, currentN, x, y);
                returnBhPrfl = get_BhR_sequence(canReach);
                final_BhPrfl = returnBhPrfl;
            }
            // ================= END SPECIAL CASE ==================

            return final_BhPrfl;
        }

        private bool checkPrecedence_xy(GraphVariables.clsGraph graph, int currentN, int x_R, int y_R)
        {
            //precedence test in sequence? => just use the dominator(-1) tree of (x) - Dom-1(x).
            bool canReach_xy = false;
            for (int i = 0; i < graph.Network[currentN].Node[x_R].nDomEI; i++)
                if (y_R == graph.Network[currentN].Node[x_R].DomEI[i]) {
                    canReach_xy = true;
                    break;
                }
            return canReach_xy;
        }

        private bool[] get_BhR_sequence(bool canReach_xy)
        {
            bool[] TempBhRl = new bool[8];
            if (canReach_xy) {
                TempBhRl[0] = false;
                TempBhRl[1] = false;
                TempBhRl[2] = true;
                TempBhRl[3] = true;
                TempBhRl[4] = false;
                TempBhRl[5] = true;
                TempBhRl[6] = true;
                TempBhRl[7] = false;
            }
            else {
                TempBhRl[0] = false;
                TempBhRl[1] = false;
                TempBhRl[2] = false;
                TempBhRl[3] = false;
                TempBhRl[4] = false;
                TempBhRl[5] = true;
                TempBhRl[6] = true;
                TempBhRl[7] = false;
            }
            return TempBhRl;
        }

        private void final_TransitiveRelation(ref bool[] returnBhPrfl, bool[] x_BhPrfl_d, bool[] y_BhPrfl_d)
        {   
            //totalConcurrent: if (x_header, y_header) == totalCC(TRUE) => if (x_header/y_header) == totalCausal(x/y) => TRUE else => FALSE
            //returnBhPrfl[0] = (returnBhPrfl[0] && x_BhPrfl_d[2] && y_BhPrfl_d[2]); //[2] is totalCausal
            //existConcurrent: if (x_header, y_header) == existCC(TRUE) => whatever inside x/y => TRUE;
            returnBhPrfl[1] = returnBhPrfl[1];

            //totalCausal: if (x_header, y_header) == totalCausal(TRUE) => if (x_header/ y_header) == totalCausal => TRUE else => FALSE
            //returnBhPrfl[2] = (returnBhPrfl[2] && x_BhPrfl_d[2] && y_BhPrfl_d[2]);
            //existCausal: if (x_header, y_header) == existCausal(TRUE) => whatever inside x/y => TRUE;
            returnBhPrfl[3] = returnBhPrfl[3];

            //canConflict: if (x_header, y_header) == canConflict(TRUE) => whatever inside x/y => TRUE;
            returnBhPrfl[4] = (returnBhPrfl[4] || x_BhPrfl_d[4] || y_BhPrfl_d[4]);
            //not(__):  ?? Need it?

            //canCoocur: if (x_header, y_header) == canCoocur(TRUE) => whatever inside x/y => TRUE
            returnBhPrfl[6] = returnBhPrfl[6];
            //not(__):            
        }

        private void get_BhR_xy_Root(ref GraphVariables.clsGraph graph, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop, GraphVariables.clsSESE clsSESE, int workSESE,
            int common_HWLS, ref bool[] returnBhPrfl, bool[] flag_Check, int x_root, int y_root)
        {
            //reduce all subgraph except "common_HWLS" (could be retrieve from Indexing)
            reduceG.total_reduceSubgraph(ref graph, graph.reduceNet, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, -1); //(up to O(n3))                  
            gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, -1, -1, ref clsSESE, "AC", -1); //(up to O(n2))

            int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_root);
            int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_root);

            //frmShow = new frmShowFullModel_Debug("", graph, graph.subNet); frmShow.ShowDialog();

            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior_Updated(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);           
        }

        private void get_BhR_xy_SESE(ref GraphVariables.clsGraph graph, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop, GraphVariables.clsSESE clsSESE, int workSESE,
            int common_HWLS, ref bool[] returnBhPrfl, bool x_PdF, bool y_PdF, int x_header, int y_header, int x_R, int y_R, bool[] flag_Check)
        {
            int orgIndex = clsHWLS.FBLOCK.FBlock[common_HWLS].refIndex;
            //reduce all subgraph except "common_HWLS" (could be retrieve from Indexing)
            reduceG.total_reduceSubgraph(ref graph, graph.reduceNet, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, clsHWLS.FBLOCK.FBlock[common_HWLS].depth); //(up to O(n3))
            gProAnalyzer.Ultilities.makeSubNetwork.make_subNetwork(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, workSESE, orgIndex, ref clsSESE, "SESE", -1); //(up to O(n2))

            //frmAnalysisNetwork frmAnl = new frmAnalysisNetwork();
            //frmAnl.displayProcessModel(ref graph, graph.subNet, ref clsLoop, -1, ref clsSESE, -1);
            //frmAnl.Show();

            if (x_PdF && y_PdF) {
                int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_R);
                int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_R);
                if (x_subNet == -1 && y_subNet == -1) return;
                //gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);
                gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior_Updated(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);                
            }
            if (x_PdF && !y_PdF) {
                int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_R);
                int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_header);
                if (x_subNet == -1 && y_subNet == -1) return;
                //gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x_subNet, x_subNet, ref returnBhPrfl, flag_Check);
                gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior_Updated(graph, graph.subNet, x_subNet, x_subNet, ref returnBhPrfl, flag_Check);
            }
            if (!x_PdF && y_PdF) {
                int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_header);
                int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_R);
                if (x_subNet == -1 && y_subNet == -1) return;
                //gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);
                gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior_Updated(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);
            }
            if (!x_PdF && !y_PdF) {
                int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_header);
                int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_header);
                if (x_subNet == -1 && y_subNet == -1) return;
                //gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);
                gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior_Updated(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);
            }
        }

        private void get_BhR_xy_NL(ref GraphVariables.clsGraph graph, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop, GraphVariables.clsSESE clsSESE, int workSESE,
            int common_HWLS, ref bool[] returnBhPrfl, bool x_PdF, bool y_PdF, int x_header, int y_header, int x_R, int y_R, bool[] flag_Check)
        {
            int orgIndex = clsHWLS.FBLOCK.FBlock[common_HWLS].refIndex;
            //reduce all subgraph except "common_HWLS" (could be retrieve from Indexing)
            reduceG.total_reduceSubgraph(ref graph, graph.reduceNet, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, clsHWLS.FBLOCK.FBlock[common_HWLS].depth);
            gProAnalyzer.Ultilities.makeSubNetwork.make_AcyclicSubGraph(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, workLoop, orgIndex, "NL");
            int x_h_temp = -1;
            int y_h_temp = -1;
            if (x_PdF && y_PdF) {
                x_h_temp = x_R;
                y_h_temp = y_R;
            }
            if (x_PdF && !y_PdF) {
                x_h_temp = x_R;
                y_h_temp = y_header;
            }
            if (!x_PdF && y_PdF) {
                x_h_temp = x_header;
                y_h_temp = y_R;
            }
            if (!x_PdF && !y_PdF) {
                x_h_temp = x_header;
                y_h_temp = y_header;
            }
            int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_h_temp);
            int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_h_temp);
            if (x_subNet == -1 && y_subNet == -1) return;
            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check); //SubNet
            //get_behaviorProfile(x_header
            //if (x_h_temp, y_h_temp) not exist concurrency nor causal => 
            bool[] BhPrfl_p = get_BehaviorProfile(clsHWLS, common_HWLS, x_h_temp, clsHWLS.FBLOCK.FBlock[common_HWLS].Pdom_BhPrfl[0, 0], false); //get BhPrfl(x, header)
            bool[] BhPrfl_d = get_BehaviorProfile(clsHWLS, common_HWLS, clsHWLS.FBLOCK.FBlock[common_HWLS].Dom_BhPrfl[0, 0], y_h_temp, true); //get BhPrfl(header, y) => FinalNet
            //combine BhPrfl(x, header) and BhPrfl(header, y)
            bool[] returnBhPrfl_temp = combine_Transitive_BhPrfl(BhPrfl_p, BhPrfl_d);
            returnBhPrfl = refresh_BhPrfl(returnBhPrfl, returnBhPrfl_temp);
        }

        private void get_BhR_xy_IL_Simple(ref GraphVariables.clsGraph graph, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop, GraphVariables.clsSESE clsSESE, int workSESE,
            int common_HWLS, ref bool[] returnBhPrfl, bool x_PdF, bool y_PdF, int x_header, int y_header, bool[] flag_Check)
        {
            bool[] BhPrfl_p_temp = new bool[8];            
            //calculate behavior relation BhPrfl(X, header) from outside of IL (CID(x, header) to header)
            int p_common_HWLS = clsHWLS.FBLOCK.FBlock[common_HWLS].parentBlock;
            int CIPd_IL = clsHWLS.FBLOCK.FBlock[common_HWLS].CIPd;
            if (clsHWLS.FBLOCK.FBlock[p_common_HWLS].SESE && x_PdF) //only when x (x_header) in PdFlow
                get_BhR_xy_SESE(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, p_common_HWLS, ref BhPrfl_p_temp, x_PdF, y_PdF, x_header, CIPd_IL, x_header, CIPd_IL, flag_Check);
            if ((clsHWLS.FBLOCK.FBlock[p_common_HWLS].SESE == false) && x_PdF)
                get_BhR_xy_NL(ref graph, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, p_common_HWLS, ref BhPrfl_p_temp, x_PdF, y_PdF, x_header, CIPd_IL, x_header, CIPd_IL, flag_Check);

            //Compute behavior relation inside IL
            int orgIndex = clsHWLS.FBLOCK.FBlock[common_HWLS].refIndex;
            reduceG.total_reduceSubgraph(ref graph, graph.reduceNet, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, clsHWLS.FBLOCK.FBlock[common_HWLS].depth);
            gProAnalyzer.Ultilities.makeSubNetwork.make_AcyclicSubGraph(ref graph, graph.reduceNet, graph.subNet, ref clsLoop, workLoop, orgIndex, "IL");
            //current method assumpt that no nested IL
            int x_subNet = get_NodeIndex_SubNet(graph, graph.subNet, x_header); //convert to subNet index
            int y_subNet = get_NodeIndex_SubNet(graph, graph.subNet, y_header); //convert to subNet index
            if (x_subNet == -1 && y_subNet == -1) return;
            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.subNet, x_subNet, y_subNet, ref returnBhPrfl, flag_Check);

            //new_BhPrfl(x, y) = combine BhPrfl(x, h), BhPrfl(h, y) 
            bool[] BhPrfl_p = new bool[8];
            if (x_PdF)
                BhPrfl_p = BhPrfl_p_temp;
            else
                BhPrfl_p = get_BehaviorProfile(clsHWLS, common_HWLS, x_header, clsHWLS.FBLOCK.FBlock[common_HWLS].Pdom_BhPrfl[0, 0], false); //get BhPrfl(x, header) (INTERNAL IL)
            bool[] BhPrfl_d = get_BehaviorProfile(clsHWLS, common_HWLS, clsHWLS.FBLOCK.FBlock[common_HWLS].Dom_BhPrfl[0, 0], y_header, true); //get BhPrfl(header, y)
            
            //combine transitive relation of x_header, y_header.
            bool[] returnBhPrfl_temp = combine_Transitive_BhPrfl(BhPrfl_p, BhPrfl_d);
            //refreshing(BhPrfl(x, y), newBhPrfl(x, y))
            returnBhPrfl = refresh_BhPrfl(returnBhPrfl, returnBhPrfl_temp);
        }

        private bool[] refresh_BhPrfl(bool[] old_BhPrfl, bool[] new_BhPrfl)
        {
            //because we only get the extreme case (exist/ total)
            bool[] arrTemp = new bool[8];
            for (int i = 0; i < old_BhPrfl.Length; i++) {
                arrTemp[i] = (old_BhPrfl[i] || new_BhPrfl[i]);
            }
            return arrTemp;
        }

        private bool[] combine_Transitive_BhPrfl(bool[] BhPrfl_a, bool[] BhPrfl_b)
        {
            bool[] arrTemp = new bool[8];
            for (int i = 0; i < BhPrfl_a.Length; i++) {
                arrTemp[i] = (BhPrfl_a[i] && BhPrfl_b[i]);
            }
            return arrTemp;
        }

        private int get_index_HWLS(GraphVariables.clsHWLS clsHWLS, int node)
        {
            int tBlock = -1; //target block index
            bool candidate = false;
            for (int i = 0; i < clsHWLS.FBLOCK.nFBlock; i++) {
                for (int j = 0; j < clsHWLS.FBLOCK.FBlock[i].nNode; j++) {
                    int fNode = clsHWLS.FBLOCK.FBlock[i].Node[j];
                    if (node == fNode) {
                        candidate = true;
                        break;
                    }
                }
                if (candidate) {
                    if (tBlock != -1) {
                        if (clsHWLS.FBLOCK.FBlock[i].depth > clsHWLS.FBLOCK.FBlock[tBlock].depth)
                            tBlock = i;
                    }
                    else tBlock = i;
                    candidate = false;
                }
            }
            return tBlock;
        }

        private int get_commonImmediate_HWLS(GraphVariables.clsHWLS clsHWLS, int x_block, int y_block, ref int x_nearest_block, ref int y_nearest_block, 
            ref int x_nearest_block_1, ref int y_nearest_block_1)
        {
            if (x_block == y_block) {
                x_nearest_block = x_block;
                y_nearest_block = x_block;
                x_nearest_block_1 = x_block;
                y_nearest_block_1 = x_block;
                return x_block;
            }
            if (x_block == -1 || y_block == -1)
                return -1;

            int[] x_ancestor = new int[clsHWLS.FBLOCK.FBlock[x_block].depth];
            int n_X = 0;
            int[] y_ancestor = new int[clsHWLS.FBLOCK.FBlock[y_block].depth];
            int n_Y = 0;
            int curDepth = 0;
            int curBlock = -1;
            x_ancestor[n_X] = x_block;
            n_X++;
            y_ancestor[n_Y] = y_block;
            n_Y++;

            curDepth = clsHWLS.FBLOCK.FBlock[x_block].depth;
            curBlock = x_block;
            do {
                if (clsHWLS.FBLOCK.FBlock[curBlock].parentBlock != -1) {                   
                    curBlock = clsHWLS.FBLOCK.FBlock[curBlock].parentBlock;
                    x_ancestor[n_X] = curBlock;
                    n_X++;
                }
                curDepth--;
            } while (curDepth > 0);

            curDepth = clsHWLS.FBLOCK.FBlock[y_block].depth;
            curBlock = y_block;
            do {
                if (clsHWLS.FBLOCK.FBlock[curBlock].parentBlock != -1) {
                    curBlock = clsHWLS.FBLOCK.FBlock[curBlock].parentBlock;
                    y_ancestor[n_Y] = curBlock;
                    n_Y++;
                }
                curDepth--;
            } while (curDepth > 0);

            int[] commonBlock = new int[clsHWLS.FBLOCK.nFBlock];
            commonBlock = gProAnalyzer.Ultilities.findIntersection.find_Intersection(commonBlock.Length, x_ancestor, y_ancestor);

            //Fix Bug
            if (commonBlock.Length == 0) commonBlock = null;

            if (commonBlock != null) {
                if (commonBlock[0] != x_block) {
                    for (int i = -1; i < (n_X - 1); i++) {
                        if (x_ancestor[i + 1] == commonBlock[0]) {
                            x_nearest_block = x_ancestor[i];
                            break;
                        }
                    }
                    if (x_nearest_block != x_block) {
                        for (int i = -1; i < (n_X - 1); i++) {
                            if (x_ancestor[i + 1] == x_nearest_block) {
                                x_nearest_block_1 = x_ancestor[i];
                                break;
                            }
                        }
                    }
                    else
                        x_nearest_block_1 = x_nearest_block;
                }
                else {
                    x_nearest_block = commonBlock[0];
                    x_nearest_block_1 = commonBlock[0];
                }
                if (commonBlock[0] != y_block) {
                    for (int i = -1; i < (n_Y - 1); i++) {
                        if (y_ancestor[i + 1] == commonBlock[0]) {
                            y_nearest_block = y_ancestor[i];
                            break;
                        }
                    }
                    if (y_nearest_block != y_block) {
                        for (int i = -1; i < (n_Y - 1); i++) {
                            if (y_ancestor[i + 1] == y_nearest_block) {
                                y_nearest_block_1 = y_ancestor[i];
                                break;
                            }
                        }
                    }
                    else
                        y_nearest_block_1 = y_nearest_block;
                }
                else {
                    y_nearest_block = commonBlock[0];
                    y_nearest_block_1 = commonBlock[0];
                }
                return commonBlock[0]; //first index is the common parent.
            }
            else return -1;
        }

        private bool[] get_BehaviorProfile(GraphVariables.clsHWLS clsHWLS, int currHWLS, int fromX, int toY, bool isDomBH)
        {
            if (currHWLS == -1) MessageBox.Show("Error index in clsHWLS", "Error index in clsHWLS");
            int nDomBhIndx = clsHWLS.FBLOCK.FBlock[currHWLS].nDomBh;
            int nPdomBhIndx = clsHWLS.FBLOCK.FBlock[currHWLS].nPdomBh;
            bool[] tempArr = null;
            int nTempArr = 0;

            if (isDomBH) {
                for (int i = 1; i < nDomBhIndx; i++) {
                    if (toY == clsHWLS.FBLOCK.FBlock[currHWLS].Dom_BhPrfl[0, i]) {
                        tempArr = new bool[8];
                        for (int j = 1; j < 9; j++) {
                            if (clsHWLS.FBLOCK.FBlock[currHWLS].Dom_BhPrfl[j, i] == 1) {
                                tempArr[nTempArr] = true; //set output behaviorProfile true
                                nTempArr++;
                            }
                            if (clsHWLS.FBLOCK.FBlock[currHWLS].Dom_BhPrfl[j, i] == 0) {
                                tempArr[nTempArr] = false; //set output behaviorProfile false
                                nTempArr++;
                            }
                        }
                        break;
                    }
                }
            }
            else {
                for (int i = 1; i < nPdomBhIndx; i++) {
                    if (fromX == clsHWLS.FBLOCK.FBlock[currHWLS].Pdom_BhPrfl[0, i]) {
                        tempArr = new bool[8];
                        for (int j = 1; j < 9; j++) {
                            if (clsHWLS.FBLOCK.FBlock[currHWLS].Pdom_BhPrfl[j, i] == 1) {
                                tempArr[nTempArr] = true; //set output behaviorProfile true
                                nTempArr++;
                            }
                            if (clsHWLS.FBLOCK.FBlock[currHWLS].Pdom_BhPrfl[j, i] == 0) {
                                tempArr[nTempArr] = false; //set output behaviorProfile false
                                nTempArr++;
                            }
                        }
                        break;
                    }
                }
            }
            return tempArr;
        }//get behavior profile of a given block only

        private bool[] get_BehaviorProfile_All(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, int fromX, int fromX_block, int toY, int toY_block)
        {
            int curDepth = 0;
            bool[] returnBhPrfl_d = new bool[8];
            bool[] old_returnBhPrfl_d = new bool[8];            
            bool[] arrTemp = new bool[8];
            int curBlock = toY_block;
            int childBlock = -1;
            bool flag_PdF = false;

            for (int i = 0; i < old_returnBhPrfl_d.Length; i++) //manipulate the CONFLICT relationship
                if (i == 4)
                    old_returnBhPrfl_d[i] = false;
                else
                    old_returnBhPrfl_d[i] = true;

            curDepth = clsHWLS.FBLOCK.FBlock[toY_block].depth;
            do {
                //skip this computation in current IL => can get that relation in upper lever.
                if (!(clsHWLS.FBLOCK.FBlock[curBlock].nEntry > 1 && inPdFlow(graph, currentN, toY, clsHWLS.FBLOCK.FBlock[curBlock].CIPd))) {
                    if (curDepth == clsHWLS.FBLOCK.FBlock[curBlock].depth && curBlock == toY_block)
                        returnBhPrfl_d = get_BehaviorProfile(clsHWLS, curBlock, -1, toY, true);
                    else {
                        if (flag_PdF == true) {
                            returnBhPrfl_d = get_BehaviorProfile(clsHWLS, curBlock, -1, toY, true);
                            flag_PdF = false;
                        }
                        else
                            returnBhPrfl_d = get_BehaviorProfile(clsHWLS, curBlock, -1, clsHWLS.FBLOCK.FBlock[childBlock].Entry[0], true);
                    }
                }
                else
                    flag_PdF = true;

                //Combine Behavior matrix for each iteration.
                arrTemp = combine_BhPrfl(returnBhPrfl_d, old_returnBhPrfl_d);
                old_returnBhPrfl_d = returnBhPrfl_d;
                returnBhPrfl_d = arrTemp;

                childBlock = curBlock;
                curBlock = clsHWLS.FBLOCK.FBlock[curBlock].parentBlock;

                if (childBlock == fromX_block) break;
                curDepth--;
            } while (curDepth > 0);
            return returnBhPrfl_d;
        }

        private bool[] get_BehaviorProfile_All_Block(GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, int fromX_block, int toY_block)
        {
            int curDepth = 0;
            bool[] returnBhPrfl_d = new bool[8];
            bool[] old_returnBhPrfl_d = new bool[8];
            for (int i = 0; i < old_returnBhPrfl_d.Length; i++) old_returnBhPrfl_d[i] = true;
            bool[] arrTemp = new bool[8];
            int curBlock = clsHWLS.FBLOCK.FBlock[toY_block].parentBlock;
            int childBlock = toY_block;            

            curDepth = clsHWLS.FBLOCK.FBlock[toY_block].depth - 1;
            do {
                //NO NEED (No IL from (Root to Common_Block) => skip this computation in current IL => can get that relation in upper lever.
                returnBhPrfl_d = get_BehaviorProfile(clsHWLS, curBlock, -1, clsHWLS.FBLOCK.FBlock[childBlock].Entry[0], true);

                //Combine Behavior matrix for each iteration.
                arrTemp = combine_BhPrfl(returnBhPrfl_d, old_returnBhPrfl_d);
                old_returnBhPrfl_d = returnBhPrfl_d;
                returnBhPrfl_d = arrTemp;

                childBlock = curBlock;
                curBlock = clsHWLS.FBLOCK.FBlock[curBlock].parentBlock;

                if (childBlock == fromX_block) break;
                curDepth--;
            } while (curDepth > 0);
            return returnBhPrfl_d;
        }

        private int get_Root_Block(GraphVariables.clsHWLS clsHWLS, int currentBlock)
        {
            int RootB = -1;
            int curDepth = clsHWLS.FBLOCK.FBlock[currentBlock].depth;
            do {
                RootB = currentBlock;
                currentBlock = clsHWLS.FBLOCK.FBlock[currentBlock].parentBlock;
                curDepth--;
            } while (curDepth > 0);
            return RootB;
        }

        private bool inPdFlow(GraphVariables.clsGraph graph, int currentN, int node, int CIPd)
        {
            for (int i = 0; i < graph.Network[currentN].Node[CIPd].nDomRevEI; i++) {
                if (node == graph.Network[currentN].Node[CIPd].DomRevEI[i])
                    return true;
            }
            return false;
        }

        private bool[] combine_BhPrfl(bool[] returnBhPrfl_d, bool[] old_returnBhPrfl_d) //CORRECT!
        {
            bool[] arrTemp = new bool[8];
            for (int i = 0; i < returnBhPrfl_d.Length; i++) {
                if (i == 4) //conflict keep for all
                    arrTemp[i] = (returnBhPrfl_d[i] || old_returnBhPrfl_d[i]);
                else
                    arrTemp[i] = (returnBhPrfl_d[i] && old_returnBhPrfl_d[i]);
            }
            return arrTemp;
        }
        //After locate Attrs => reduce unnecessary subgraph => run instanceFlow()

        private int get_NodeIndex_SubNet(GraphVariables.clsGraph graph, int subNet, int node)
        {
            for (int i = 0; i < graph.Network[subNet].nNode; i++) {
                if (node == graph.Network[subNet].Node[i].orgNum)
                    return i;
            }
            return -1;
        }

        //=============== PURE MODEL CHECKING EXPERIMENT ==================
        public bool start_ProcessQuery_pureModelChecking(ref GraphVariables.clsGraph graph, int currentN, ref GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int x, int y, bool[] flag_Check)
        {
            Initialize_All();

            //Check occurence of X or/and Y => return false if not satisfy
            bool check = false;
            bool check_2 = false;
            for (int i = 0; i < graph.Network[currentN].nNode; i++)
            {
                if (x == i) check = true;
                if (y == i) check_2 = true;
            }
            if (!(check && check_2)) return false;

            //(X and Y) MUST occur in Process Model to be processed further
            //bool[] final_BhPrfl = get_Final_BhPrfl(ref graph, currentN, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, x, y, flag_Check);
            bool[] final_BhPrfl = get_Final_BhPrfl_Acyclic_pureModelChecking(ref graph, currentN, clsHWLS, clsLoop, workLoop, clsSESE, workSESE, x, y, flag_Check); //now, only acyclic

            if (final_BhPrfl == null) return false;

            //checking for only single behavior relation of (x, y)
            for (int i = 0; i < flag_Check.Length; i++)
            {
                if (flag_Check[i] == false)
                    if (final_BhPrfl[i] == true)
                        return true;
            }
            return false;
        }

        private bool[] get_Final_BhPrfl_Acyclic_pureModelChecking(ref GraphVariables.clsGraph graph, int currentN, GraphVariables.clsHWLS clsHWLS, GraphVariables.clsLoop clsLoop, int workLoop,
            GraphVariables.clsSESE clsSESE, int workSESE, int x, int y, bool[] flag_Check)
        {
            bool[] final_BhPrfl = new bool[8];
            bool[] returnBhPrfl = new bool[8];
            //bool[] flag_Check = new bool[8]; //=> Set index = TRUE if want to DISABLE checking relation

            graph.Network[graph.reduceNet] = graph.Network[currentN]; //currentN should be finalNet
            gProAnalyzer.Ultilities.extendGraph.full_extentNetwork(ref graph, graph.reduceNet, 0, 0); //copy to reduceNet                    

            //gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior(graph, graph.reduceNet, x, y, ref returnBhPrfl, flag_Check);
            gProAnalyzer.Ultilities.AnalyseBehavior_InstF.check_InstanceBehavior_Updated(graph, graph.reduceNet, x, y, ref returnBhPrfl, flag_Check);
            
            final_BhPrfl = returnBhPrfl;            
            return final_BhPrfl;
        }
    }
}
