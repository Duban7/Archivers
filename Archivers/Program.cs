using Archivers.RLEArchiver;
using Archivers.LZ78Archiver;
using Archivers.HuffmanArchiver;
using System.Diagnostics;

Console.WriteLine("Введите путь к файлу");
Console.WriteLine("Например: C:\\Users\\User\\Documents\\example.txt");

string filePath = Console.ReadLine()!;

// Проверка на пустую строку
if (string.IsNullOrWhiteSpace(filePath))
{
    Console.WriteLine("Путь не может быть пустым.");
    return;
}

// Проверка корректности пути и существования файла
try
{
    Path.GetFullPath(filePath);

    if (!File.Exists(filePath))
    {
        Console.WriteLine("Файл по указанному пути не существует.");
        return;
    }

    Console.WriteLine("Путь корректен, файл существует.\n");
}
catch (Exception ex) when (
    ex is ArgumentException ||
    ex is NotSupportedException ||
    ex is PathTooLongException)
{
    Console.WriteLine("Некорректный путь к файлу.");
    return;
}

// Получение размера исходного файла
long originalSize = GetFileSize(filePath);
Console.WriteLine($"Исходный размер файла: {originalSize} байт\n");

var stopwatch = new Stopwatch();

double rleRatio, lzRatio, huffRatio;

Console.WriteLine("=== RLE Archiver ===");

stopwatch.Restart();
RLEArchiver.Compress(filePath);
stopwatch.Stop();
double rleCompressTime = stopwatch.Elapsed.TotalMilliseconds;

string rlePath = filePath + ".rle";
long rleSize = GetFileSize(rlePath);
rleRatio = CompressionRatio(originalSize, rleSize);

stopwatch.Restart();
RLEArchiver.Decompress(rlePath);
stopwatch.Stop();
double rleDecompressTime = stopwatch.Elapsed.TotalMilliseconds;

Console.WriteLine($"Сжатие: {rleCompressTime:F2} мс");
Console.WriteLine($"Распаковка: {rleDecompressTime:F2} мс");
Console.WriteLine($"Размер архива: {rleSize} байт");
Console.WriteLine($"Коэффициент сжатия: {rleRatio:F3}\n");

Console.WriteLine("=== LZ78 Archiver ===");

stopwatch.Restart();
LZ78Archiver.Compress(filePath);
stopwatch.Stop();
double lzCompressTime = stopwatch.Elapsed.TotalMilliseconds;

string lzPath = filePath + ".lz78";
long lzSize = GetFileSize(lzPath);
lzRatio = CompressionRatio(originalSize, lzSize);

stopwatch.Restart();
LZ78Archiver.Decompress(lzPath);
stopwatch.Stop();
double lzDecompressTime = stopwatch.Elapsed.TotalMilliseconds;

Console.WriteLine($"Сжатие: {lzCompressTime:F2} мс");
Console.WriteLine($"Распаковка: {lzDecompressTime:F2} мс");
Console.WriteLine($"Размер архива: {lzSize} байт");
Console.WriteLine($"Коэффициент сжатия: {lzRatio:F3}\n");

Console.WriteLine("=== Huffman Archiver ===");

stopwatch.Restart();
HuffmanArchiver.Compress(filePath);
stopwatch.Stop();
double huffCompressTime = stopwatch.Elapsed.TotalMilliseconds;

string huffPath = filePath + ".haff";
long huffSize = GetFileSize(huffPath);
huffRatio = CompressionRatio(originalSize, huffSize);

stopwatch.Restart();
HuffmanArchiver.Decompress(huffPath);
stopwatch.Stop();
double huffDecompressTime = stopwatch.Elapsed.TotalMilliseconds;

Console.WriteLine($"Сжатие: {huffCompressTime:F2} мс");
Console.WriteLine($"Распаковка: {huffDecompressTime:F2} мс");
Console.WriteLine($"Размер архива: {huffSize} байт");
Console.WriteLine($"Коэффициент сжатия: {huffRatio:F3}\n");

// Сравнение результатов алгоритмов по коэффициенту сжатия
Console.WriteLine("=== Сравнительный анализ ===");

if (huffRatio < lzRatio && huffRatio < rleRatio)
    Console.WriteLine("Алгоритм Хаффмана показал наилучший коэффициент сжатия.");
else if (lzRatio < rleRatio)
    Console.WriteLine("Алгоритм LZ78 показал наилучший коэффициент сжатия.");
else
    Console.WriteLine("Алгоритм RLE показал наилучший коэффициент сжатия.");

Console.WriteLine("\nВсе архиваторы успешно обработали файл.");

static long GetFileSize(string path)
{
    return new FileInfo(path).Length;
}

static double CompressionRatio(long originalSize, long compressedSize)
{
    return (double)compressedSize / originalSize;
}
