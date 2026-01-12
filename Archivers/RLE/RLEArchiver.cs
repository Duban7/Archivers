namespace Archivers.RLEArchiver
{
    internal class RLEArchiver : IArchiver
    {
        // Метод сжатия файла с помощью алгоритма RLE
        public static void Compress(string FilePath)
        {
            FileStream? fs = null; //исходный файл
            FileStream? rs = null; //архив
            string archiveFilePath = FilePath + ".rle";
            try
            {
                if (File.Exists(archiveFilePath))
                    File.Delete(archiveFilePath);
                fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                rs = new FileStream(archiveFilePath, FileMode.CreateNew);

                List<byte> Bt = new List<byte>(); // буфер текущей последовательности байт
                List<byte> nBt = new List<byte>();
                while (fs.Position < fs.Length)
                {
                    byte B = (byte)fs.ReadByte();
                    if (Bt.Count == 0)
                        Bt.Add(B);
                    else if (Bt[Bt.Count - 1] != B)
                    {
                        //неповторяющиеся байты
                        Bt.Add(B);
                        if (Bt.Count == 255)
                        {
                            // сохраняем блок неповторяющихся байт
                            rs.WriteByte((byte)0);
                            rs.WriteByte((byte)255);
                            rs.Write(Bt.ToArray(), 0, 255);
                            Bt.Clear();
                        }
                    }
                    else
                    {
                        // обработка повторяющихся байт
                        if (Bt.Count != 1)
                        {
                            // сохраняем предыдущие неповторяющиеся байты
                            rs.WriteByte((byte)0);
                            rs.WriteByte((byte)(Bt.Count - 1));
                            rs.Write(Bt.ToArray(), 0, Bt.Count - 1);
                            Bt.RemoveRange(0, Bt.Count - 1);
                        }
                        Bt.Add(B);
                        while ((B = (byte)fs.ReadByte()) == Bt[0])
                        {
                            // продолжаем считать повторяющиеся байты
                            Bt.Add(B);
                            if (Bt.Count == 255)
                            {
                                rs.WriteByte((byte)255);
                                rs.WriteByte(Bt[0]);
                                Bt.Clear();
                                break;
                            }
                        }
                        if (Bt.Count > 0)
                        {
                            // сохраняем повторяющийся блок
                            rs.WriteByte((byte)Bt.Count);
                            rs.WriteByte(Bt[0]);
                            Bt.Clear();
                            Bt.Add(B);
                        }
                    }
                }
                if (Bt.Count > 0)
                {
                    //после просмотра файла у нас может быть буфер с неповторяющимися байтами
                    rs.WriteByte((byte)0);
                    rs.WriteByte((byte)Bt.Count);
                    rs.Write(Bt.ToArray(), 0, Bt.Count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (fs != null) fs.Close();
                if (rs != null) rs.Close();
            }
        }

        // Метод распаковки RLE-архива
        public static void Decompress(string archiveFilePath)
        {
            if (!File.Exists(archiveFilePath))
                throw new FileNotFoundException("File not found", archiveFilePath);

            // формирование пути для распакованного файла
            string UnarchivedFilePath = archiveFilePath.EndsWith(".rle")
                                             ? archiveFilePath[..^4]
                                             : throw new Exception("Unsupported file format");

            string extension = Path.GetExtension(UnarchivedFilePath);
            string dataFilePath = UnarchivedFilePath.Substring(0, UnarchivedFilePath.Length - extension.Length) + "-RLE" + extension;

            FileStream? fs = null;
            FileStream? rs = null;
            try
            {
                fs = new FileStream(archiveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (File.Exists(dataFilePath))
                    File.Delete(dataFilePath);
                rs = new FileStream(dataFilePath, FileMode.CreateNew);
                while (fs.Position < fs.Length)
                {
                    int Bt = fs.ReadByte();
                    if (Bt == 0) //различные байты
                    {
                        // блок неповторяющихся байт
                        Bt = fs.ReadByte();
                        for (int j = 0; j < Bt; ++j)
                        {
                            byte b = (byte)fs.ReadByte();
                            rs.WriteByte(b);
                        }
                    }
                    else
                    {
                        // блок повторяющихся байт
                        int Value = fs.ReadByte();
                        for (int j = 0; j < Bt; ++j)
                            rs.WriteByte((byte)Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (fs != null) fs.Close();
                if (rs != null) rs.Close();
            }
        }

    }
}
