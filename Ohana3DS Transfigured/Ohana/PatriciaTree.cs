using System.Collections.Generic;

namespace Ohana3DS_Transfigured.Ohana
{
    class PatriciaTree
    {
        public class Node
        {
            public int index;
            public int referenceBit;
            public string name;
            public Node left, right;
        }
        public int nodeCount;
        public int maxLength;
        public List<Node> nodes = new List<Node>();
        public Node rootNode = new Node();

        public PatriciaTree(List<string> keys)
        {
            rootNode.left = rootNode;
            rootNode.right = rootNode;
            rootNode.referenceBit = -1;
            foreach (string key in keys) if (key.Length > maxLength) maxLength = key.Length;
            foreach (string key in keys) nodes.Add(Insert(key));
        }

        private Node Insert(string key)
        {
            Node rootNode = this.rootNode;
            Node leftNode = rootNode.left;
            int bit = (maxLength << 3) - 1;
            while (rootNode.referenceBit > leftNode.referenceBit)
            {
                rootNode = leftNode;
                if (GetBit(key, leftNode.referenceBit))
                    leftNode = leftNode.right;
                else
                    leftNode = leftNode.left;
            }
            while (GetBit(leftNode.name, bit) == GetBit(key, bit)) bit--;

            rootNode = this.rootNode;
            leftNode = rootNode.left;
            while ((rootNode.referenceBit > leftNode.referenceBit) && (leftNode.referenceBit > bit))
            {
                rootNode = leftNode;
                if (GetBit(key, leftNode.referenceBit))
                    leftNode = leftNode.right;
                else
                    leftNode = leftNode.left;
            }

            Node output = new Node
            {
                name = key,
                referenceBit = bit
            };
            if (GetBit(key, bit))
            {
                output.left = leftNode;
                output.right = output;
            }
            else
            {
                output.left = output;
                output.right = leftNode;
            }
            output.index = ++nodeCount;
            return output;
        }

        private bool GetBit(string name, int bit)
        {
            int position = bit >> 3;
            int charBit = bit & 7;
            if (name == null || position >= name.Length) return false;
            return ((name[position] >> charBit) & 1) > 0;
        }
    }
}
