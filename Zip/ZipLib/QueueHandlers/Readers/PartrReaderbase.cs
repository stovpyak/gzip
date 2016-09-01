using System.IO;
using ZipLib.Loggers;

namespace ZipLib.QueueHandlers.Readers
{
    public abstract class PartrReaderBase: IPartReader
    {
        protected readonly ILogger Logger;
        protected Stream SourceStream;
        protected long SourceFileSize;

        protected PartrReaderBase(ILogger logger)
        {
            Logger = logger;
        }

        public void Init(Stream sourceStream, long fileSize)
        {
            SourceStream = sourceStream;
            SourceFileSize = fileSize;
        }

        public abstract bool ReadPart(FilePart part);
    }
}
