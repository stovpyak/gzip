using System.IO;

namespace ZipLib.QueueHandlers.Readers
{
    public interface IPartReader
    {
        void Init(Stream sourceStream, long fileSize);

        bool ReadPart(FilePart part);
    }
}
