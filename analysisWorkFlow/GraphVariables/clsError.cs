using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    class clsError
    {
        public void Initialize_All()
        {
            nError = 0;
            //Error = new strError[Network[reduceNet].nNode * 3];
            //initiate_Error(Network[reduceNet].nNode * 3);
        }
        public struct strError
        {
            public string Node; //error Node 번호
            public string Loop; //error Loop 번호
            public string SESE; //new => SESE error
            public string currentKind; //현 Node 종류
            public int messageNum; //error message
            public string TypeOfError; //R, P, D (real, potential, dominance)
        }
        public strError[] Error;
        public int nError;

        //======MICS
        public void initiate_Error(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Error[i].currentKind = "";
                Error[i].Loop = "";
                Error[i].Node = "";
                Error[i].SESE = "";
            }
        }
    }
}
