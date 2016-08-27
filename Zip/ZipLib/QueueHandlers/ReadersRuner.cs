using System;
using System.Threading;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class ReadersRuner
    {
        private readonly ThreadStop _threadStop;
        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly PartQueue _sourceQueue;
        private readonly PartQueue _nextQueue;

        public ReadersRuner(ThreadStop threadStop, IFileNameProvider sourceFileNameProvider, PartQueue sourceQueue, PartQueue nextQueue)
        {
            _threadStop = threadStop;
            _sourceFileNameProvider = sourceFileNameProvider;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;

            var thread = new Thread(this.Run) {Name = "ReadersRuner"};
            thread.Start();
        }

        private int _readersCount = 0;
        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                var part = _sourceQueue.GetPart();
                if (part != null)
                {
                    var readerName = "ReaderN" + _readersCount;
                    var reader = new Reader(readerName, _sourceFileNameProvider.GetFileName(), part, _nextQueue);
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} отдал part {part.Name} reader`у {readerName}");
                    _readersCount++;
                    reader.Start();
                }
            }
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} завершил свой run");
        }
    }
}
