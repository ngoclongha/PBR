using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class findNodeInLoop
    {
        public static bool Node_In_Loop(ref gProAnalyzer.GraphVariables.clsLoop clsLoop, int workLoop, int node, int loop) //node in loop => return true; not in loop => return false
        {
            bool inLoop = false;

            if (node == clsLoop.Loop[workLoop].Loop[loop].header)
            {
                inLoop = true;
            }
            else
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nNode; i++)
                {
                    if (node == clsLoop.Loop[workLoop].Loop[loop].Node[i])
                    {
                        inLoop = true;
                        break;
                    }
                }
            }
            if (!inLoop)
            {
                for (int i = 0; i < clsLoop.Loop[workLoop].Loop[loop].nChild; i++)
                {
                    inLoop = Node_In_Loop(ref clsLoop, workLoop, node, clsLoop.Loop[workLoop].Loop[loop].child[i]);
                    if (inLoop) break;
                }
            }
            return inLoop;
        }
    }
}
