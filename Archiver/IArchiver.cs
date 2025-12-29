namespace Archiver
{
    public interface IArchiver
    {
        public static abstract void Zip(string FilePath);
        public static abstract void UnZip(string FilePath);
    }
}
