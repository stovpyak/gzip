using System.Threading;
using ZipLib.Loggers;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers.Readers
{
    public class FilePartReader: PartrReaderBase
    {
        private readonly ICompressStrategy _strategy;

        public FilePartReader(ILogger logger, ICompressStrategy strategy) : base(logger)
        {
            _strategy = strategy;
        }

        private long _totalReadByte;

        public override bool ReadPart(FilePart part)
        {
            part.Source = new byte[_strategy.PartSize];
            var count = SourceStream.Read(part.Source, 0, part.Source.Length);
            part.SourceSize = count;

            _totalReadByte = _totalReadByte + count;
            // прочитали всё - у части выставляем признак, что она последняя
            if (_totalReadByte != SourceFileSize)
                return true;
            part.IsLast = true;
            Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал последнюю часть файла {part} ");
            return true;
        }
    }
}
