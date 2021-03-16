using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.Ultilities
{
    class findUnion
    {
        public static int[] find_Union(int totNum, int[] A, int[] B)
        {
            bool Ok = true;
            int[] retSet = new int[A.Length + B.Length];
            int count = 0;
            for (int i = 0; i < A.Length; i++) retSet[i] = A[i];

            for (int i = 0; i < B.Length; i++)
            {
                for (int j = 0; j < A.Length; j++)
                {
                    if (B[i] == A[j])
                    {
                        Ok = false;
                        break;
                    }
                }
                if (Ok == true)
                {
                    retSet[A.Length + count] = B[i];
                    count++;
                }
                Ok = true;
            }

            int[] finalSet = new int[A.Length + count];
            for (int i = 0; i < (A.Length + count); i++) finalSet[i] = retSet[i];

            return finalSet;
        }
    }
}
