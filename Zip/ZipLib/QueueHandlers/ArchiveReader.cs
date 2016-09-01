using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Decompress;
using ZipLib.Loggers;
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
        private readonly IDecompressStrategy _strategy;
        private FileStream _archiveStream;

        public ArchiveReader(ILogger logger, IFileNameProvider archiveFileNameProvider, IDecompressStrategy strategy,
            IQueue sourceQueue, IQueue nextQueue) : 
            base(logger, sourceQueue, nextQueue)
        {
            _archiveFileNameProvider = archiveFileNameProvider;
            _strategy = strategy;

            InnerThread = new Thread(this.Run) { Name = "ArchiveReader" };
            InnerThread.Start();
        }

        readonly Stopwatch _processingStopwatch = new Stopwatch();
        private ArhivePartReader _arhivePartReader;

        private int _currentPartIndex;
        private int _removedPartCount;


        protected override bool ProcessPart(FilePart part)
        {
            Logger.Add($"Поток {Thread.CurrentThread.Name} начал читать");
            if (_archiveStream == null)
            {
                var fileName = _archiveFileNameProvider.GetFileName();
                _archiveStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var sourceFileSize = new FileInfo(fileName).Length;
                _arhivePartReader = new ArhivePartReader(Logger, _archiveStream, sourceFileSize);
            }
            try
            {
                _processingStopwatch.Reset();
                _processingStopwatch.Start();

                // делегирует выполнение _arhivePartReader
                // логика чтения и поиска заголовков получилась "ветиеватая", поэтому отдельный класс
                // с public методами - так проще было тестировать
                if (_arhivePartReader.ReadPart(part))
                {
                    _processingStopwatch.Stop();
                    Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал в часть {part} за {_processingStopwatch.ElapsedMilliseconds} ms");

                    part.Index = _currentPartIndex;
                    _currentPartIndex++;
                    NextQueue?.Add(part);
                    return true;
                }
                // не прочитал часть - архив закончился
                {
                    Logger.Add(
                        $"!Поток {Thread.CurrentThread.Name} НЕ проинициализировал part {part} - архив прочитан");
                    _removedPartCount++;
                    if (_removedPartCount == _strategy.GetMaxActivePartCount())
                    {
                        Logger.Add(
                            $"!Поток {Thread.CurrentThread.Name} выведены все обрабатываемые части {_removedPartCount} шт. - это признак того, что работа завершена");
                    }
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
