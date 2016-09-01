﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.QueueHandlers
{
    public class Reader: QueueHandlerBase
    {
        private readonly IFileNameProvider _sourceFileNameProvider;
        private FileStream _sourceStream;

        public Reader(ILogger logger, IFileNameProvider sourceFileNameProvider, IQueue sourceQueue, IQueue nextQueue):
            base(logger, sourceQueue, nextQueue)
        {
            _sourceFileNameProvider = sourceFileNameProvider;

            InnerThread = new Thread(this.Run) {Name = "Reader"};
            InnerThread.Start();
        }

        readonly Stopwatch _processingStopwatch = new Stopwatch();
        private long _sourceFileSize;
        private long _totalReadByte; 

        protected override bool ProcessPart(FilePart part)
        {
            Logger.Add($"Поток {Thread.CurrentThread.Name} начал читать");
            if (_sourceStream == null)
            {
                var fileName = _sourceFileNameProvider.GetFileName();
                var sourceFileInfo = new FileInfo(fileName);
                _sourceFileSize = sourceFileInfo.Length;
                _sourceStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            try
            {
                _processingStopwatch.Reset();
                _processingStopwatch.Start();

                part.Source = new Byte[part.SourceSize];
                var count = _sourceStream.Read(part.Source, 0, part.SourceSize);

                _processingStopwatch.Stop();
                Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал в часть {part} {count} byte за {_processingStopwatch.ElapsedMilliseconds} ms");

                _totalReadByte = _totalReadByte + count;
                // прочитали всё - у части выставляем признак, что она последняя
                if (_totalReadByte == _sourceFileSize)
                {
                    part.IsLast = true;
                    Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал последнюю часть файла {part} ");
                }
            }
            catch (Exception)
            {
                Logger.Add($"Поток {Thread.CurrentThread.Name} - ошибка при чтении");
                Close();
                throw;
            }
            NextQueue?.Add(part);
            return true;
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
