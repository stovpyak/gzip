using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Decompress;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Читает части архива
    /// </summary>
    public class ArchiveReader: QueueHandlerBase
    {
        private readonly IFileNameProvider _archiveFileNameProvider;
        private FileStream _archiveStream;

        public ArchiveReader(ILogger logger, IFileNameProvider archiveFileNameProvider, IQueue sourceQueue, IQueue nextQueue) : base(logger, sourceQueue, nextQueue)
        {
            _archiveFileNameProvider = archiveFileNameProvider;

            InnerThread = new Thread(this.Run) { Name = "ArchiveReader" };
            InnerThread.Start();
        }

        readonly Stopwatch _processingStopwatch = new Stopwatch();
        private ArhivePartReader _arhivePartReader;

        protected override bool ProcessPart(FilePart part)
        {
            Logger.Add($"Поток {Thread.CurrentThread.Name} начал читать");
            if (_archiveStream == null)
            {
                _archiveStream = new FileStream(_archiveFileNameProvider.GetFileName(), FileMode.Open, FileAccess.Read);
                _arhivePartReader = new ArhivePartReader(_archiveStream);
            }
            try
            {
                //_processingStopwatch.Reset();
                //_processingStopwatch.Start();

                // делегирукет выполнение _arhivePartReader - так проще было тестировать
                if (_arhivePartReader.ReadPart(part))
                {
                    NextQueue?.Add(part);
                    return true;
                }
                return false;
                //_processingStopwatch.Stop();
                //Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал в часть {part} {count} byte за {_processingStopwatch.ElapsedMilliseconds} ms");
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
