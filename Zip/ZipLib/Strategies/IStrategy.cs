namespace ZipLib.Strategies
{
    public interface IStrategy
    {
        void StartFile(long fileSize);

        int GetMaxActivePartCount();

        bool InitNextFilePart(FilePart part);
    }
}
