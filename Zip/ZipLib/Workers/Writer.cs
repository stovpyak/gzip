using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.Workers
{
    public class Writer
    {
        private readonly ThreadStop _threadStop;
        private readonly ILogger _logger;

        private readonly IndexedParts _sourceQueue;
        private readonly PartQueue _nextQueue;
        private readonly string _targetFileName;

        public Writer(ThreadStop threadStop, ILogger logger, string targetFileName, IndexedParts sourceQueue, PartQueue nextQueue)
        {
            _threadStop = threadStop;
            _logger = logger;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;
            _targetFileName = targetFileName;

            var thread = new Thread(this.Run) { Name = "Writer" };
            thread.Start();
        }

        private Stream _targetStream;

        private Stream GetOrMakeStream()
        {
            if (_targetStream == null)
            {
                _targetStream = File.Create(_targetFileName);
            }
            return _targetStream;
        }

        private int _partCount;
        private int _currentPartIndex;

        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                // для writera важен порядок/очередность частей!
                var part = _sourceQueue.GetPartByIndex(_currentPartIndex);
                if (part != null)
                {
                    _logger.Add($"Поток {Thread.CurrentThread.Name} получил part {part}");
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    GetOrMakeStream().Write(part.Result, 0, part.Result.Length);
                    _partCount++;

                    stopWatch.Stop();
                    _logger.Add($"Поток {Thread.CurrentThread.Name} записал part {part} за {stopWatch.ElapsedMilliseconds} ms");

                    part.Result = null;
                    _nextQueue.Enqueue(part);

                    _currentPartIndex++;
                }
            }
            GetOrMakeStream().Close();

            _logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
            _logger.Add($"Поток {Thread.CurrentThread.Name} записано {_partCount} частей");
        }
    }
}
