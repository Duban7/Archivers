namespace Archiver.HaffmanArchiver
{
    //https://www.youtube.com/watch?v=BKMliUkEZvs
    public class HaffmanArchiver
    {
        public void Zip(string dataFilePath, string zipFileName)
        {
            byte[] data = File.ReadAllBytes(dataFilePath);
            byte[] arch = CompressBytes(data);
            File.WriteAllBytes(zipFileName, arch);
        }

        private byte[] CompressBytes(byte[] data)
        {
            int[] freqs = CalculateFreq(data);
            byte[] head = CreateHeader(data.Length, freqs);
            Node root = CreateHuffmanTree(freqs);
            string[] codes = CreateHuffmanCodes(root);
            byte[] bits = Compress(data, codes);
            return head.Concat(bits).ToArray();
        }

        private byte[] CreateHeader(int dataLength, int[] freqs)
        {
            List<byte> head = new();
            head.Add((byte)(dataLength         & 255));
            head.Add((byte)((dataLength >> 8)  & 255));
            head.Add((byte)((dataLength >> 16) & 255));
            head.Add((byte)((dataLength >> 24) & 255));
            for (int i = 0; i < 256; i++)
                head.Add((byte)freqs[i]);
            return head.ToArray();
        }

        private byte[] Compress(byte[] data, string[] codes)
        {
            List<byte> bits = new();
            byte sum = 0;
            byte bit = 1;
            foreach(byte symbol in data)
                foreach(char c in codes[symbol])
                {
                    if (c == '1')
                        sum |= bit;
                    if (bit < 128)
                        bit <<= 1;
                    else
                    {
                        bits.Add(sum);
                        sum = 0;
                        bit = 1;
                    }
                }
            if(bit>1)
                bits.Add(sum);
            return bits.ToArray();
        }

        private int[] CalculateFreq(byte[] data)
        {
            int[] freqs = new int[256];
            foreach (byte d in data)
                freqs[d]++;
            NormalizeFreqs(freqs);

            return freqs;


            void NormalizeFreqs(int[] freqs)
            {
                int max = freqs.Max();
                if (max <= 255) return;
                for (int i = 0; i < 256;i++)
                    if(freqs[i] > 0)
                        freqs[i] = 1 + freqs[i] * 255 / (max-1);
            }
        }


        private Node CreateHuffmanTree(int[] freqs)
        {
            PriorityQueue<Node> pq = new PriorityQueue<Node>();
            for(int i = 0;i<256;i++)
                if (freqs[i]>0)
                    pq.Enqueue(freqs[i], new Node((byte)i, freqs[i]));

            while (pq.Size() > 1)
            {
                Node bit0 = pq.Dequeue();
                Node bit1 = pq.Dequeue();
                int freq = bit0.freq + bit1.freq;
                Node next = new Node(bit0, bit1, freq);
                pq.Enqueue(freq, next);
            }

            return null;
        }
        private string[] CreateHuffmanCodes(Node root)
        {
            string[] codes = new string[256];
            Next(root, "");
            return codes;

            void Next(Node node, string code)
            {
                if (node.bit0 == null)
                    codes[node.symbol] = code;
                else
                {
                    Next(node.bit0, code + "0");
                    Next(node.bit1, code + "1");
                }
            }
        }


        public void UnZip(string zipFilePath, string dataFileName)
        {
            byte[] arch = File.ReadAllBytes(zipFilePath);
            byte[] data = DecompressBytes(arch);
            File.WriteAllBytes(dataFileName, data);
        }

        private byte[] DecompressBytes(byte[] arch)
        {
            ParseHeader(arch, out int dataLength, out int startIndex, out int[] freqs);
            Node root = CreateHuffmanTree(freqs);
            byte[] data = Decompress(arch, startIndex, dataLength, root);
            return data;
        }

        private byte[] Decompress(byte[] arch, int startIndex, int dataLength, Node root)
        {
            int size = 0;
            Node curr = root;
            List<byte> data = new();
            for(int i = startIndex; i < arch.Length;i++)
                for(int bit = 1; bit <= 128; bit <<= 1)
                {
                    bool zero = (arch[i] & bit) == 0;
                    if (zero)
                        curr = curr.bit0;
                    else
                        curr = curr.bit1;
                    if (curr.bit0 != null)
                        continue;
                    if (size++ < dataLength)
                        data.Add(curr.symbol);
                    curr = root;
                }
            return data.ToArray();  
        }

        private void ParseHeader(byte[] arch, out int dataLength, out int startIndex, out int[] freqs)
        {
            dataLength = arch[0] |
                        (arch[1] << 8)  | 
                        (arch[1] << 16) | 
                        (arch[1] << 24);

            freqs = new int[256];
            for(int i = 0; i< 256; i++) 
                freqs[i] = arch[4 + i];

            startIndex = 4 + 256;
        }
    }
}
