using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class findIntersection
    {
        public static int[] find_Intersection(int totNum, int[] A, int[] B)
        {
            int cntFind = 0;
            int[] find_Node = new int[totNum]; //totNum = Total number of node

            int[] retSet;

            if (A == null && B == null)
            {
                retSet = null;
            }
            else if (A == null)
            {
                retSet = B; //Must be null??? //Because all of set of node Predecessor of node i are not null => so we can use this commands
            }
            else if (B == null)
            {
                retSet = A; //Must be null???
            }
            else
            {
                for (int i = 0; i < A.Length; i++)
                {
                    for (int j = 0; j < B.Length; j++)
                    {
                        if (A[i] == B[j])
                        {
                            find_Node[cntFind] = A[i];
                            cntFind++;
                            break;
                        }
                    }
                }
                //if (cntFind == 0) retSet = null;
                //else
                //{
                    retSet = new int[cntFind];
                    for (int k = 0; k < cntFind; k++) retSet[k] = find_Node[k];
                //}

            }
            return retSet;
        }
    }
}
