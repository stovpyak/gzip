using System;
using System.Threading;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class ArchiversRuner
    {
        private readonly ThreadStop _threadStop;
        private readonly PartQueue _sourceQueue;
        private readonly PartQueue _nextQueue;
        

        public ArchiversRuner(ThreadStop threadStop, PartQueue sourceQueue, PartQueue nextQueue)
        {
            _threadStop = threadStop;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;
            var thread = new Thread(this.Run) { Name = "ArchiversRuner" };
            thread.Start();
        }

        private int _archiversCount = 0;
        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                var part = _sourceQueue.GetPart();
                if (part != null)
                {
                    var archiverName = "ArchiverN" + _archiversCount;
                    var archiver = new Archiver(archiverName, part, _nextQueue);
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} отдал part {part.Name} archiver`у {archiverName}");
                    _archiversCount++;
                    archiver.Start();
                }
            }
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} завершил свой run");
        }
    }
}
