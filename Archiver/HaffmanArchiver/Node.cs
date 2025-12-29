namespace Archiver.HaffmanArchiver
{
    public class Node
    {
        public readonly byte symbol;
        public readonly int freq;
        public readonly Node? bit0;
        public readonly Node? bit1;

        public Node(byte symbol, int freq)
        {
            this.symbol = symbol;
            this.freq = freq;
        }
        public Node(Node bit0, Node bit1, int freq)
        {
            this.bit0 = bit0;
            this.bit1 = bit1;
            this.freq = freq;
        }
    }
}
