using System;
using System.Collections.Generic;
using System.Text;

namespace Archiver.BinaryArchiver
{
    internal class BinaryArchiver : IArchiver
    {
        public static void Zip(string FilePath)
        {
            FileStream? fs = null; //исходный файл
            FileStream? rs = null; //архив
            string ZipFilePath = FilePath + ".zip";
            try
            {
                if (File.Exists(ZipFilePath))
                    File.Delete(ZipFilePath);
                string Format = FilePath.Substring(FilePath.LastIndexOf('.') + 1); //получаем формат файла
                fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                rs = new FileStream(ZipFilePath, FileMode.CreateNew);
                //сначала сохраним формат исходного файла
                rs.WriteByte((byte)Format.Length);
                for (int i = 0; i < Format.Length; ++i)
                    rs.WriteByte((byte)Format[i]);

                List<byte> Bt = new List<byte>();
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
                            rs.WriteByte((byte)0);
                            rs.WriteByte((byte)255);
                            rs.Write(Bt.ToArray(), 0, 255);
                            Bt.Clear();
                        }
                    }
                    else
                    {
                        //повтор
                        if (Bt.Count != 1)
                        {
                            //в буфере могут быть неповторяющиеся байты
                            //их нужно сохранить
                            rs.WriteByte((byte)0);
                            rs.WriteByte((byte)(Bt.Count - 1));
                            rs.Write(Bt.ToArray(), 0, Bt.Count - 1);
                            Bt.RemoveRange(0, Bt.Count - 1);
                        }
                        Bt.Add(B);
                        while ((B = (byte)fs.ReadByte()) == Bt[0])
                        {
                            //пока идут повторы сохраняем их в буфер
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
                            //если в буфере что-то есть, сохраняем это
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
        public static void UnZip(string ZipFilePath)
        {
            string UnZipedFilePath = ZipFilePath[0..^4];

            if (!File.Exists(ZipFilePath)) return;
            FileStream? fs = null;
            FileStream? rs = null;
            try
            {
                string Format = ".";
                fs = new FileStream(ZipFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                int FormatLen = fs.ReadByte();
                for (int i = 0; i < FormatLen; ++i)
                    Format += (char)fs.ReadByte();
                if (File.Exists(UnZipedFilePath + Format))
                    File.Delete(UnZipedFilePath + Format);
                rs = new FileStream(UnZipedFilePath + Format, FileMode.CreateNew);
                while (fs.Position < fs.Length)
                {
                    int Bt = fs.ReadByte();
                    if (Bt == 0) //различные байты
                    {
                        Bt = fs.ReadByte();
                        for (int j = 0; j < Bt; ++j)
                        {
                            byte b = (byte)fs.ReadByte();
                            rs.WriteByte(b);
                        }
                    }
                    else //повторы
                    {
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
