using System;
using System.IO;
using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.QueueHandlers.Readers
{
    public class FilePartReader: PartrReaderBase
    {
        public FilePartReader(ILogger logger) : base(logger)
        {
        }

        private long _totalReadByte;

        public override bool ReadPart(FilePart part)
        {
            part.Source = new Byte[part.SourceSize];
            var count = SourceStream.Read(part.Source, 0, part.SourceSize);

            _totalReadByte = _totalReadByte + count;
            // прочитали всё - у части выставляем признак, что она последняя
            if (_totalReadByte == SourceFileSize)
            {
                part.IsLast = true;
                Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал последнюю часть файла {part} ");
            }
            return true;
        }
    }
}
