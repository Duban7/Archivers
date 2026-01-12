namespace Archivers.HuffmanArchiver
{
    public class HuffmanArchiver : IArchiver
    {
        // Метод сжатия файла с использованием алгоритма Хаффмана
        public static void Compress(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            byte[] data = File.ReadAllBytes(filePath);

            // Если файл пустой, просто создаем пустой архив
            if (data.Length == 0)
            {
                File.WriteAllBytes(filePath + ".haff", data);
                return;
            }

            byte[] result = CompressBytes(data); // сжатие данных

            File.WriteAllBytes(filePath + ".haff", result);
        }

        // Метод распаковки Huffman-архива
        public static void Decompress(string archivePath)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException(archivePath);

            byte[] arch = File.ReadAllBytes(archivePath);

            // Если архив пустой, просто создаем исходный файл
            if (arch.Length == 0)
            {
                File.WriteAllBytes(archivePath[..^5], arch); // распаковка данных
                return;
            }

            byte[] data = DecompressBytes(arch);

            // Формирование пути для распакованного файла
            string UnarchivedFilePath = archivePath.EndsWith(".haff")
                                             ? archivePath[..^5]
                                             : throw new Exception("Unsupported file format");

            string extension = Path.GetExtension(UnarchivedFilePath);
            string dataFilePath = UnarchivedFilePath.Substring(0, UnarchivedFilePath.Length - extension.Length) + "-HAFFMAN" + extension;

            File.WriteAllBytes(dataFilePath, data);
        }

        // Сжатие данных в массив байт
        private static byte[] CompressBytes(byte[] data)
        {
            int[] freq = CalculateFrequencies(data); // подсчет частот каждого байта

            Node root = BuildTree(freq); // построение дерева Хаффмана

            string[] codes = BuildCodes(root); // генерация кодов для каждого символа

            byte[] bitData = EncodeBits(data, codes); // кодирование исходных данных

            byte[] header = CreateHeader(data.Length, freq); // добавление информации о частотах

            return header.Concat(bitData).ToArray(); // объединение заголовка и данных
        }

        // подсчет частот символов
        private static int[] CalculateFrequencies(byte[] data)
        {
            int[] freq = new int[256];
            foreach (byte b in data) freq[b]++;
            return freq;
        }

        // построение дерева Хаффмана по частотам
        private static Node BuildTree(int[] freq)
        {
            PriorityQueue<Node, int> pq = new();

            for (int i = 0; i < 256; i++)
                if (freq[i] > 0)
                    pq.Enqueue(new Node((byte)i, freq[i]), freq[i]);

            if (pq.Count == 1)
            {
                var single = pq.Dequeue();
                return new Node(single, new Node((byte)0, 0), single.freq);
            }

            while (pq.Count > 1)
            {
                Node a = pq.Dequeue();
                Node b = pq.Dequeue();
                Node parent = new Node(a, b, a.freq + b.freq);
                pq.Enqueue(parent, parent.freq);
            }

            return pq.Dequeue();
        }

        // генерация кодов символов на основе дерева
        private static string[] BuildCodes(Node root)
        {
            string[] codes = new string[256];

            void dfs(Node n, string code)
            {
                if (n.IsLeaf)
                {
                    codes[n.symbol] = code;
                    return;
                }
                dfs(n.left, code + "0");
                dfs(n.right, code + "1");
            }

            dfs(root, "");

            return codes;
        }

        // кодирование данных в битовую последовательность
        private static byte[] EncodeBits(byte[] data, string[] codes)
        {
            List<byte> outBytes = new();

            byte current = 0;
            int bitPos = 0;

            foreach (byte b in data)
            {
                foreach (char c in codes[b])
                {
                    if (c == '1')
                        current |= (byte)(1 << bitPos);

                    bitPos++;

                    if (bitPos == 8)
                    {
                        outBytes.Add(current);
                        current = 0;
                        bitPos = 0;
                    }
                }
            }

            if (bitPos != 0)
                outBytes.Add(current);

            return outBytes.ToArray();
        }

        // формирование заголовка с длиной и частотами
        private static byte[] CreateHeader(int dataLength, int[] freq)
        {
            List<byte> h = new();

            h.AddRange(BitConverter.GetBytes(dataLength));

            for (int i = 0; i < 256; i++)
                h.AddRange(BitConverter.GetBytes(freq[i]));

            return h.ToArray();
        }

        // Распаковка массива байт
        private static byte[] DecompressBytes(byte[] arch)
        {
            ParseHeader(arch, out int dataLength, out int[] freq, out int start);

            Node root = BuildTree(freq);

            return DecodeBits(arch, start, dataLength, root);
        }

        // извлечение длины и частот из заголовка
        private static void ParseHeader(byte[] arch, out int length, out int[] freq, out int startIndex)
        {
            length = BitConverter.ToInt32(arch, 0);

            freq = new int[256];
            int pos = 4;

            for (int i = 0; i < 256; i++)
            {
                freq[i] = BitConverter.ToInt32(arch, pos);
                pos += 4;
            }

            startIndex = pos;
        }

        // восстановление исходных данных из битовой последовательности
        private static byte[] DecodeBits(byte[] arch, int startIndex, int dataLength, Node root)
        {
            List<byte> output = new(dataLength);

            Node node = root;
            int count = 0;

            for (int i = startIndex; i < arch.Length; i++)
            {
                byte b = arch[i];

                for (int bit = 0; bit < 8; bit++)
                {
                    bool one = (b & (1 << bit)) != 0;
                    node = one ? node.right : node.left;

                    if (node.IsLeaf)
                    {
                        output.Add(node.symbol);
                        node = root;
                        count++;
                        if (count == dataLength)
                            return output.ToArray();
                    }
                }
            }

            return output.ToArray();
        }
    }
}
