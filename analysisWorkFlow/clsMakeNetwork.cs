using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gProAnalyzer
{
    class clsMakeNetwork
    {
        private Random imRand;

        public int nNode; // 전체노드수
        public int nNodeF, nNodeT; //Flow당 노드수
        public double rStructure; //Flow내 구조 비율
        public int nSplitF, nSplitT; //구조내 분기수
        public int nForwardF, nForwardT; //구조내 Forward Flow내 노드수
        public int nBackwardF, nBackwardT; //구조내 Forward Flow내 노드수
        public double rOR, rXOR; //XOR gateway 비율

        public struct strNode
        {
            public int Type; //0 : S or E  1: Node  2: structure

            public string Kind; //Node 종류  S:start E: end T:Task AND:And  XOR:OR

        }

        public struct strLink
        {
            public int fromNode; //시작 Node
            public int toNode; //끝 Node
        }

        public struct strNetwork
        {
            public int nNode;
            public strNode[] Node;

            public int nLink;
            public strLink[] Link;
        }
        public strNetwork Network;

        //do nothing
        public clsMakeNetwork()
        {

        }

        public void make_Network()
        {
            imRand = new Random();

            init_Network();

            make_Frame(0, 1);

            do
            {
                int curNode = Network.nNode;
                for (int i = 0; i < curNode; i++)
                {
                    if (Network.nNode >= nNode) Network.Node[i].Type = 1;// Target Node수 맞추기 위해

                    if (Network.Node[i].Type != 2) continue;

                    if (imRand.NextDouble() <= 0.5) make_ForStructure(i);
                    else make_BackStructure(i);
                    Network.Node[i].Type = 1;
                }

                if (Network.nNode >= nNode) break;

                int sNode, tNode;
                do
                {
                    sNode = imRand.Next(2, Network.nNode);
                    tNode = imRand.Next(2, Network.nNode);
                } while (sNode == tNode);

                make_Frame(sNode, tNode);

            } while (true);

            find_NodeInform();
        }

        private void init_Network()
        {
            Network = new strNetwork();
            Network.nNode = 0;
            Network.Node = new strNode[nNode * 2];
            Network.nLink = 0;
            Network.Link = new strLink[nNode * 4];

            Network.nNode = 2;
            Network.Node[0].Type = 0;
            Network.Node[0].Kind = "START";
            Network.Node[1].Type = 0;
            Network.Node[1].Kind = "END";

        }

        private void make_Frame(int sNode, int tNode)
        {
            int nMakeNode = imRand.Next(nNodeF, nNodeT + 1);

            int cntS = 0;
            int makeN = Network.nNode + nMakeNode / 2;
            for (int i = 0; i < nMakeNode; i++)
            {
                if (imRand.NextDouble() <= rStructure)
                {
                    Network.Node[Network.nNode].Type = 2; // structure
                    cntS++;
                }
                else
                {
                    Network.Node[Network.nNode].Type = 1; // node
                }

                if (i == 0) Network.Link[Network.nLink].fromNode = sNode;
                else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
                Network.Link[Network.nLink].toNode = Network.nNode;
                Network.nLink++;

                Network.nNode++;
            }

            Network.Link[Network.nLink].fromNode = Network.nNode - 1;
            Network.Link[Network.nLink].toNode = tNode;
            Network.nLink++;

            //Structure 하나도 없으면
            if (cntS == 0)
            {
                Network.Node[makeN].Type = 2;
            }
        }

        private void make_ForStructure(int sNode)
        {
            int tNode = Network.nNode;
            Network.Node[tNode].Type = 1;
            Network.nNode++;

            for (int i = 0; i < Network.nLink; i++)
            {
                if (Network.Link[i].fromNode == sNode) Network.Link[i].fromNode = tNode;
            }

            int nSplit = imRand.Next(nSplitF, nSplitT + 1);
            for (int k = 0; k < nSplit; k++)
            {
                int nFor = imRand.Next(nForwardF, nForwardT + 1);
                for (int i = 0; i < nFor; i++)
                {
                    Network.Node[Network.nNode].Type = 1; // node

                    if (i == 0) Network.Link[Network.nLink].fromNode = sNode;
                    else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
                    Network.Link[Network.nLink].toNode = Network.nNode;
                    Network.nLink++;

                    Network.nNode++;
                }
                if (nFor == 0) Network.Link[Network.nLink].fromNode = sNode;
                else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
                Network.Link[Network.nLink].toNode = tNode;
                Network.nLink++;
            }

        }

        private void make_BackStructure(int sNode)
        {
            int tNode = Network.nNode;
            Network.Node[tNode].Type = 1;
            Network.nNode++;

            for (int i = 0; i < Network.nLink; i++)
            {
                if (Network.Link[i].fromNode == sNode) Network.Link[i].fromNode = tNode;
            }

            //forward Flow
            int nFor = imRand.Next(nForwardF, nForwardT + 1);
            for (int i = 0; i < nFor; i++)
            {
                Network.Node[Network.nNode].Type = 1; // node

                if (i == 0) Network.Link[Network.nLink].fromNode = sNode;
                else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
                Network.Link[Network.nLink].toNode = Network.nNode;
                Network.nLink++;

                Network.nNode++;
            }
            if (nFor == 0) Network.Link[Network.nLink].fromNode = sNode;
            else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
            Network.Link[Network.nLink].toNode = tNode;
            Network.nLink++;

            //Backward Flow
            int nBack = imRand.Next(nBackwardF, nBackwardT + 1);
            for (int i = 0; i < nBack; i++)
            {
                Network.Node[Network.nNode].Type = 1; // node

                if (i == 0) Network.Link[Network.nLink].fromNode = tNode;
                else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
                Network.Link[Network.nLink].toNode = Network.nNode;
                Network.nLink++;

                Network.nNode++;
            }
            if (nBack == 0) Network.Link[Network.nLink].fromNode = tNode;
            else Network.Link[Network.nLink].fromNode = Network.nNode - 1;
            Network.Link[Network.nLink].toNode = sNode;
            Network.nLink++;

        }

        private void find_NodeInform()
        {
            for (int i = 2; i < Network.nNode; i++)
            {
                int cntPre = 0;
                int cntPost = 0;
                for (int j = 0; j < Network.nLink; j++)
                {
                    if (Network.Link[j].fromNode == i) cntPost++;
                    if (Network.Link[j].toNode == i) cntPre++;
                }

                if (cntPre <= 1 && cntPost <= 1)
                {
                    Network.Node[i].Kind = "TASK";
                }
                else
                {
                    double rand = imRand.NextDouble();
                    if (rand <= rOR) Network.Node[i].Kind = "OR";
                    else if (rand <= rOR + rXOR) Network.Node[i].Kind = "XOR";
                    else Network.Node[i].Kind = "AND";
                }
            }
        }


    }
}
