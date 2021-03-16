using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class copySESE
    {
        public static void copy_SESE(ref gProAnalyzer.GraphVariables.clsSESE  clsSESE, int fromSESE, int toSESE)
        {
            clsSESE.SESE[toSESE] = new gProAnalyzer.GraphVariables.clsSESE.strSESE();

            clsSESE.SESE[toSESE].maxDepth = clsSESE.SESE[fromSESE].maxDepth;

            clsSESE.SESE[toSESE].nSESE = clsSESE.SESE[fromSESE].nSESE;
            clsSESE.SESE[toSESE].SESE = new gProAnalyzer.GraphVariables.clsSESE.strSESEInform[clsSESE.SESE[toSESE].nSESE];
            for (int i = 0; i < clsSESE.SESE[toSESE].nSESE; i++)
            {
                clsSESE.SESE[toSESE].SESE[i].depth = clsSESE.SESE[fromSESE].SESE[i].depth;

                clsSESE.SESE[toSESE].SESE[i].parentSESE = clsSESE.SESE[fromSESE].SESE[i].parentSESE;
                clsSESE.SESE[toSESE].SESE[i].nChild = clsSESE.SESE[fromSESE].SESE[i].nChild;
                clsSESE.SESE[toSESE].SESE[i].child = new int[clsSESE.SESE[toSESE].SESE[i].nChild];
                for (int k = 0; k < clsSESE.SESE[toSESE].SESE[i].nChild; k++)
                    clsSESE.SESE[toSESE].SESE[i].child[k] = clsSESE.SESE[fromSESE].SESE[i].child[k];

                clsSESE.SESE[toSESE].SESE[i].Entry = clsSESE.SESE[fromSESE].SESE[i].Entry;
                clsSESE.SESE[toSESE].SESE[i].Exit = clsSESE.SESE[fromSESE].SESE[i].Exit;

                clsSESE.SESE[toSESE].SESE[i].nNode = clsSESE.SESE[fromSESE].SESE[i].nNode;
                clsSESE.SESE[toSESE].SESE[i].Node = new int[clsSESE.SESE[toSESE].SESE[i].nNode];
                for (int k = 0; k < clsSESE.SESE[toSESE].SESE[i].nNode; k++)
                    clsSESE.SESE[toSESE].SESE[i].Node[k] = clsSESE.SESE[fromSESE].SESE[i].Node[k];

            }

        }
    }
}
