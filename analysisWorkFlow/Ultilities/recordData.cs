using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class recordData
    {
        public static void add_Error(ref gProAnalyzer.GraphVariables.clsError clsError)
        {
            bool bSame = false;
            for (int i = 0; i < clsError.nError; i++)
            {
                if (clsError.Error[i].Node == clsError.Error[clsError.nError].Node && clsError.Error[i].Loop == clsError.Error[clsError.nError].Loop
                    && clsError.Error[i].currentKind == clsError.Error[clsError.nError].currentKind && clsError.Error[i].messageNum == clsError.Error[clsError.nError].messageNum)
                {
                    bSame = true;
                    break;
                }
            }
            if (!bSame) clsError.nError++;
        }
    }
}
