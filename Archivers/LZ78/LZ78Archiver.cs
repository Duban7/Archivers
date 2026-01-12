namespace Archivers.LZ78Archiver
{
    public class LZ78Archiver : IArchiver
    {
        // Структура для хранения индекса предыдущей последовательности и текущего символа
        private struct Entry
        {
            public int Index;
            public byte Symbol;

            public Entry(int index, byte symbol)
            {
                Index = index;
                Symbol = symbol;
            }
        }

        // Метод сжатия файла с помощью алгоритма LZ78
        public static void Compress(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            byte[] data = File.ReadAllBytes(filePath);
            var dictionary = new Dictionary<string, int>(); // словарь последовательностей
            var entries = new List<Entry>();  // список выходных пар (индекс, символ)

            // Построчно формируем словарь и последовательность кодов
            string current = "";
            int dictIndex = 1;

            foreach (byte b in data)
            {
                string next = current + (char)b;

                if (dictionary.ContainsKey(next))
                {
                    current = next; // последовательность уже есть в словаре
                }
                else
                {
                    int index = current == "" ? 0 : dictionary[current];
                    entries.Add(new Entry(index, b)); // добавляем новую пару

                    dictionary[next] = dictIndex++; // обновляем словарь
                    current = "";
                }
            }

            string outputFile = filePath + ".lz78";

            // Запись сжатых данных в бинарный файл
            using (var fs = new BinaryWriter(File.Open(outputFile, FileMode.Create)))
            {
                foreach (var e in entries)
                {
                    fs.Write(e.Index);
                    fs.Write(e.Symbol);
                }
            }
        }

        // Метод распаковки LZ78-архива
        public static void Decompress(string archivePath)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException("Archive not found", archivePath);

            var entries = new List<Entry>();

            // Чтение сжатых пар из файла
            using (var br = new BinaryReader(File.OpenRead(archivePath)))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    int index = br.ReadInt32();
                    byte symbol = br.ReadByte();

                    entries.Add(new Entry(index, symbol));
                }
            }

            var dictionary = new List<byte[]> { new byte[0] }; // словарь для распаковки
            var output = new List<byte>();

            // Восстановление исходных последовательностей по индексам
            foreach (var e in entries)
            {
                byte[] prefix = dictionary[e.Index];
                byte[] word = new byte[prefix.Length + 1];

                Buffer.BlockCopy(prefix, 0, word, 0, prefix.Length);
                word[word.Length - 1] = e.Symbol;

                dictionary.Add(word);
                output.AddRange(word);
            }

            // Формирование имени распакованного файла
            string UnarchivedFilePath = archivePath.EndsWith(".lz78")
                ? archivePath[..^5]
                : throw new Exception("Unsupported file format");

            string extension = Path.GetExtension(UnarchivedFilePath);
            string dataFilePath = UnarchivedFilePath.Substring(0, UnarchivedFilePath.Length - extension.Length) + "-LZ78" + extension;

            File.WriteAllBytes(dataFilePath, output.ToArray());
        }
    }
}