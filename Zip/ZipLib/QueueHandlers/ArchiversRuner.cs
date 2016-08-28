using System;
using System.Diagnostics;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class ArchiversRuner
    {
        private readonly ThreadStop _threadStop;
        private readonly ILogger _logger;

        private readonly IQueue _sourceQueue;
        private readonly IndexedParts _nextQueue;
        private readonly ArchiversStatistic _statistic = new ArchiversStatistic();

        public ArchiversRuner(ThreadStop threadStop, ILogger logger, IQueue sourceQueue, IndexedParts nextQueue)
        {
            _threadStop = threadStop;
            _logger = logger;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;
            var thread = new Thread(this.Run) { Name = "ArchiversRuner" };
            thread.Start();
        }

        private int _archiversCount = 0;
        private long _totalWait;

        private void Run()
        {
            var stopwatchWait = new Stopwatch();
            while (!_threadStop.IsNeedStop)
            {
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                stopwatchWait.Reset();
                stopwatchWait.Start();
                var part = _sourceQueue.GetPart();
                stopwatchWait.Stop();
                if (part != null)
                {
                    _totalWait = _totalWait + stopwatchWait.ElapsedMilliseconds;

                    _logger.Add($"Поток {Thread.CurrentThread.Name} время ожидания {stopwatchWait.ElapsedMilliseconds} ms");
                    _archiversCount++;
                    var archiverName = "ArchiverN" + _archiversCount;
                    var archiver = new Archiver(archiverName, _logger, _statistic, part, _nextQueue);
                    _logger.Add($"Поток {Thread.CurrentThread.Name} отдал part {part} archiver`у {archiverName}");
                    archiver.Start();
                }
            }
            _logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
            _logger.Add($"Поток {Thread.CurrentThread.Name} заархивированно {_archiversCount} частей");
            _logger.Add($"Поток {Thread.CurrentThread.Name} общее время ожидания поступления частей {_totalWait} ms");
            _logger.Add($"Поток {Thread.CurrentThread.Name} среднее время архивирования одной части {_statistic.GetMiddleElapsedTime()} ms");
        }
    }
}
