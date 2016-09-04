using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers
{
    public class Reader: QueueHandlerBase
    {
        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly IPartReader _partReader;
        private FileStream _sourceStream;

        public Reader(ILogger logger, ISystemInfoProvider systemInfoProvider, IFileNameProvider sourceFileNameProvider, IPartReader partReader,
            IQueue sourceQueue, IQueue nextQueue): base(logger, systemInfoProvider, sourceQueue, nextQueue)
        {
            _sourceFileNameProvider = sourceFileNameProvider;
            _partReader = partReader;

            InnerThread = new Thread(this.Run) { Name = "Reader" };
            InnerThread.Start();
        }

        readonly Stopwatch _processingStopwatch = new Stopwatch();

        private int _currentPartIndex;
        protected override bool ProcessPart(FilePart part)
        {
            if (_sourceStream == null)
            {
                var fileName = _sourceFileNameProvider.GetFileName();
                _sourceStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                _partReader.Init(_sourceStream, new FileInfo(fileName).Length);
            }
            try
            {
                _processingStopwatch.Reset();
                _processingStopwatch.Start();

                if (_partReader.ReadPart(part))
                {
                    _processingStopwatch.Stop();

                    part.Index = _currentPartIndex;
                    _currentPartIndex++;
                    Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал часть {part} {part.Source.Length} byte за {_processingStopwatch.ElapsedMilliseconds} ms");

                    NextQueue?.Add(part);

                    // часть последняя - сам поток решает, что ему пора остановиться
                    if (part.IsLast)
                        SetIsNeedStop();
                    return true;
                }
                Logger.Add($"!Поток {Thread.CurrentThread.Name} НЕ удалось прочитать часть {part}");
                throw new Exception("Не удалось прочитать часть {part}");
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
            if (_sourceStream != null)
            {
                _sourceStream.Close();
                _sourceStream = null;
            }
        }
    }
}
