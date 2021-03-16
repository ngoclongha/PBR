using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gProAnalyzer.GraphVariables
{
    public class clsEdge
    {
        public struct strEdge
        {
            public int fromNode; //시작 Node
            public int toNode; //끝 Node

            public bool bBackJ; //BackJoin Edge면 true
            public bool bBackS; //BackSplit Edge면 true

            public bool bInstance; //현재 인스턴스에 포함되면 true  .................

            public int orgFromNode; //in case after reduction => have same edge from 1 source to 1 sink
            public int orgToNode;
        }
    }
}
