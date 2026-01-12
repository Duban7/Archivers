namespace Archivers
{
    public interface IArchiver
    {
        public static abstract void Compress(string FilePath);
        public static abstract void Decompress(string ArchivePath);
    }
}
