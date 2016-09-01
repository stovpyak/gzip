﻿using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.QueueHandlers
{
    public class Writer: QueueHandlerBase
    {
        private readonly IFileNameProvider _targetFileNameProvider;
        private readonly ManualResetEventSlim _stopEvent;
        private Stream _targetStream;

        public Writer(ILogger logger, IFileNameProvider targetFileNameProvider, ManualResetEventSlim stopEvent,
            IndexedParts sourceQueue, PartQueue nextQueue) : base(logger, sourceQueue, nextQueue)
        {
            _targetFileNameProvider = targetFileNameProvider;
            _stopEvent = stopEvent;

            InnerThread = new Thread(this.Run) {Name = "Writer"};
            InnerThread.Start();
        }

        private int _currentPartIndex;

        protected override bool ProcessPart(FilePart part)
        {
            Logger.Add($"Поток {Thread.CurrentThread.Name} получил part {part}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (_targetStream == null)
                _targetStream = File.Create(_targetFileNameProvider.GetFileName());

            _targetStream.Write(part.Result, 0, part.Result.Length);

            stopWatch.Stop();
            Logger.Add($"Поток {Thread.CurrentThread.Name} записал part {part} за {stopWatch.ElapsedMilliseconds} ms");

            part.Result = null;
            // часть последняя - сообщаем, что работа завершена
            if (part.IsLast)
            {
                Logger.Add($"Поток {Thread.CurrentThread.Name} записал последнюю part - это признак завершения работы");
                _stopEvent.Set();
            }
            else
                NextQueue?.Add(part);

            _currentPartIndex++;
            return true;
        }

        protected override void Close()
        {
            if (_targetStream != null)
            {
                _targetStream.Close();
                _targetStream = null;
            }
        }

        protected override object GetParamForGetPart()
        {
            return _currentPartIndex;
        }
    }
}
