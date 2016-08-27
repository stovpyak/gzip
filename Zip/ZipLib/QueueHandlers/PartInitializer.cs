using System;
using System.Threading;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers
{
    public class PartInitializer
    {
        private readonly ThreadStop _threadStop;
        private readonly IStrategy _strategy;
        private readonly PartQueue _sourceQueue;
        private readonly PartQueue _nextQueue;

        public PartInitializer(ThreadStop threadStop, IStrategy strategy, PartQueue sourceQueue, PartQueue nextQueue)
        {
            _threadStop = threadStop;
            _strategy = strategy;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;

            var thread = new Thread(this.Run) { Name = "PartInitializer" };
            thread.Start();
        }

        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                Console.WriteLine($"Поток {Thread.CurrentThread.Name} запросил элемент из очереди {_sourceQueue.Name}");
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                var part = _sourceQueue.GetPart();
                if (part != null)
                {
                    Console.WriteLine(
                        $"Поток {Thread.CurrentThread.Name} получил из очереди {_sourceQueue.Name} part {part.Name}");
                    if (_strategy.InitNextFilePart(part))
                    {
                        Console.WriteLine($"Поток {Thread.CurrentThread.Name} проинициализировал part {part.Name}");
                        _nextQueue.Enqueue(part);
                    }
                    else
                    {
                        Console.WriteLine($"!Поток {Thread.CurrentThread.Name} НЕ проинициализировал part {part.Name} - файл прочитан");
                    }
                }
            }
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} завершил свой run");
        }
    }
}
