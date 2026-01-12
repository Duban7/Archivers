namespace Archivers.HuffmanArchiver
{
    public class Node
    {
        public byte symbol;
        public int freq;
        public Node left;
        public Node right;

        public bool IsLeaf => left == null && right == null;

        public Node(byte symbol, int freq)
        {
            this.symbol = symbol;
            this.freq = freq;
        }

        public Node(Node left, Node right, int freq)
        {
            this.left = left;
            this.right = right;
            this.freq = freq;
        }
    }
}
