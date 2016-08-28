using System;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class ReadersRuner
    {
        private readonly ThreadStop _threadStop;
        private readonly ILogger _logger;

        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly IQueue _sourceQueue;
        private readonly IQueue _nextQueue;

        public ReadersRuner(ThreadStop threadStop, ILogger logger, IFileNameProvider sourceFileNameProvider, IQueue sourceQueue, IQueue nextQueue)
        {
            _threadStop = threadStop;
            _logger = logger;
            _sourceFileNameProvider = sourceFileNameProvider;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;

            var thread = new Thread(this.Run) {Name = "ReadersRuner"};
            thread.Start();
        }

        private int _readersCount;
        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                var part = _sourceQueue.GetPart();
                if (part != null)
                {
                    _readersCount++;
                    var readerName = "ReaderN" + _readersCount;
                    var reader = new Reader(readerName, _logger, _sourceFileNameProvider.GetFileName(), part, _nextQueue);
                    _logger.Add($"Поток {Thread.CurrentThread.Name} отдал part {part.Name} reader`у {readerName}");
                    
                    reader.Start();
                }
            }
            _logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
            _logger.Add($"Поток {Thread.CurrentThread.Name} прочитано {_readersCount} частей");
        }
    }
}
