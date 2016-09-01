using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Decompress;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Читает части архива
    /// </summary>
    public class ArchiveReader: QueueHandlerBase
    {
        private readonly IFileNameProvider _archiveFileNameProvider;
        private FileStream _archiveStream;

        public ArchiveReader(ILogger logger, IFileNameProvider archiveFileNameProvider,
            IQueue sourceQueue, IQueue nextQueue) : 
            base(logger, sourceQueue, nextQueue)
        {
            _archiveFileNameProvider = archiveFileNameProvider;

            InnerThread = new Thread(this.Run) { Name = "ArchiveReader" };
            InnerThread.Start();
        }

        readonly Stopwatch _processingStopwatch = new Stopwatch();
        private IPartReader _partReader;

        private int _currentPartIndex;
        protected override bool ProcessPart(FilePart part)
        {
            if (_archiveStream == null)
            {
                var fileName = _archiveFileNameProvider.GetFileName();
                _archiveStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var sourceFileSize = new FileInfo(fileName).Length;
                var arhivePartReader = new ArсhivePartReader(Logger);
                arhivePartReader.Init(_archiveStream, sourceFileSize);
                _partReader = arhivePartReader;
            }
            try
            {
                _processingStopwatch.Reset();
                _processingStopwatch.Start();

                // делегирует выполнение _arhivePartReader
                // логика чтения и поиска заголовков получилась "ветиеватая", поэтому отдельный класс
                // с public методами - так проще было тестировать
                if (_partReader.ReadPart(part))
                {
                    _processingStopwatch.Stop();
                    part.Index = _currentPartIndex;
                    _currentPartIndex++;
                    Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал часть {part} за {_processingStopwatch.ElapsedMilliseconds} ms");

                    NextQueue?.Add(part);

                    // часть последняя - сам поток решает, что ему пора остановиться
                    if (part.IsLast)
                        SetIsNeedStop();
                    return true;
                }
                // не прочитал часть - архив закончился
                {
                    Logger.Add($"!Поток {Thread.CurrentThread.Name} НЕ прочитал part {part} - архив прочитан");
                }
                return false;
            }
            catch (Exception)
            {
                Logger.Add($"Поток {Thread.CurrentThread.Name} - ошибка при чтении");
                Close();
                throw;
            }
        }

        protected override void Close()
        {
            if (_archiveStream != null)
            {
                _archiveStream.Close();
                _archiveStream = null;
            }
        }
    }
}
