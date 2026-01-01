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

Console.WriteLine("\n=== RLE Archiver ===");
RLEArchiver.Compress(filePath);
RLEArchiver.Decompress(filePath+".rle");

Console.WriteLine("\n=== LZ78 Archiver ===");
LZ78Archiver.Compress(filePath);
LZ78Archiver.Decompress(filePath+".lz78");

Console.WriteLine("\n=== Huffman Archiver ===");
HuffmanArchiver.Compress(filePath);
HuffmanArchiver.Decompress(filePath+".haff");

Console.WriteLine("\nВсе архиваторы успешно обработали файл.");