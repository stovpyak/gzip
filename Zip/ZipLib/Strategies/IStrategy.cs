namespace ZipLib.Strategies
{
    public interface IStrategy
    {
        void StartFile(long fileSize);

        int GetMaxActivePartCount();
        long GetPartCount();
        int GetPatrSize();

        bool InitNextFilePart(FilePart part);
    }
}
