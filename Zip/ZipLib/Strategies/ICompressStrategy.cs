namespace ZipLib.Strategies
{
    public interface ICompressStrategy
    {
        void StartFile(long fileSize);

        int GetMaxActivePartCount();
        long GetPartCount();
        int GetPatrSize();

        bool InitNextFilePart(FilePart part);
    }
}
