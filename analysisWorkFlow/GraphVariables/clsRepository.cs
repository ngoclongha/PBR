using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    class clsRepository
    {
        public struct processModel
        {
            public gProAnalyzer.GraphVariables.clsGraph graph;
            public gProAnalyzer.GraphVariables.clsLoop clsLoop;
            public gProAnalyzer.GraphVariables.clsSESE clsSESE;
            public gProAnalyzer.GraphVariables.clsHWLS clsHWLS;
            public gProAnalyzer.GraphVariables.clsHWLS clsHWLS_Untangle;

            public string ID_model;
            public double Time_Index;
        }
        public processModel[] repository;
        public int nModel;

        public clsRepository()
        {
            repository = new processModel[1];            
        }
    }
}
