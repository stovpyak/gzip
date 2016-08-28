namespace ZipLib.Strategies
{
    public interface IStrategy
    {
        void StartFile(long fileSize);

        int GetMaxActivePartCount();
        long GetPartCount();

        bool InitNextFilePart(FilePart part);
    }
}
