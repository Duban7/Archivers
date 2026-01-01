// See https://aka.ms/new-console-template for more information
using Archiver.BinaryArchiver;
using Archiver.HuffmanArchiver;

Console.WriteLine("Введите путь к файлу:");
string filePath = Console.ReadLine()!;

if (string.IsNullOrWhiteSpace(filePath))
{
    Console.WriteLine("Путь не может быть пустым.");
    return;
}

Console.WriteLine("\n=== Archiver1 ===");
RLEArchiver.Compress(filePath);
RLEArchiver.Decompress(filePath+".rle");

Console.WriteLine("\n=== Archiver2 ===");
LZ78FileCompressor.Compress(filePath);
LZ78FileCompressor.Decompress(filePath+".lz78");

Console.WriteLine("\n=== Archiver3 ===");
HuffmanArchiver.Compress(filePath);
HuffmanArchiver.Decompress(filePath+".haff");

Console.WriteLine("\nВсе архиваторы успешно обработали файл.");