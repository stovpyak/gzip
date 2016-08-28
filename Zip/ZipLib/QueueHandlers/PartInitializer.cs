using System;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers
{
    public class PartInitializer
    {
        private readonly ThreadStop _threadStop;
        private readonly ILogger _logger;
        private readonly StopToken _stopToken;

        private readonly IStrategy _strategy;
        private readonly IQueue _sourceQueue;
        private readonly IQueue _nextQueue;

        public PartInitializer(ThreadStop threadStop, ILogger logger, StopToken stopToken, IStrategy strategy, IQueue sourceQueue, IQueue nextQueue)
        {
            _threadStop = threadStop;
            _logger = logger;
            _stopToken = stopToken;

            _strategy = strategy;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;

            var thread = new Thread(this.Run) { Name = "PartInitializer" };
            thread.Start();
        }

        private int _partCount;
        private int _removedPartCount;

        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                _logger.Add($"Поток {Thread.CurrentThread.Name} запросил элемент из очереди {_sourceQueue.Name}");
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                var part = _sourceQueue.GetPart();
                if (part != null)
                {
                    _logger.Add(
                        $"Поток {Thread.CurrentThread.Name} получил из очереди {_sourceQueue.Name} part {part.Name}");
                    if (_strategy.InitNextFilePart(part))
                    {
                        _logger.Add($"Поток {Thread.CurrentThread.Name} проинициализировал part {part.Name}");
                        _nextQueue.Enqueue(part);
                        _partCount++;
                    }
                    else
                    {
                        _logger.Add($"!Поток {Thread.CurrentThread.Name} НЕ проинициализировал part {part.Name} - исходный файл прочитан");
                        _removedPartCount++;
                        if (_removedPartCount == _strategy.GetMaxActivePartCount())
                        {
                            _logger.Add(
                                $"!Поток {Thread.CurrentThread.Name} Выведены все обрабатываемые части {_removedPartCount} шт. - это признак того, что работа завершена");
                            _stopToken.OnEventEnd();
                        }
                    }
                }
            }
            _logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
            _logger.Add($"Поток {Thread.CurrentThread.Name} обработано частей {_partCount}");
        }
    }
}
