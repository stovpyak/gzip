using System;
using System.Collections.Generic;
using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.Queues
{
    public class PartQueue: IQueue
    {
        private readonly string _name;
        private readonly ILogger _logger;

        private readonly Queue<FilePart> _queue = new Queue<FilePart>();
        private readonly object _lockOn = new object();

        public PartQueue(string name, ILogger logger)
        {
            _name = name;
            _logger = logger;
        }

        public string Name => _name;

        public int Count => _queue.Count;

        public FilePart GetPart()
        {
            lock (_lockOn)
            {
                if (_queue.Count > 0)
                {
                    var part = _queue.Dequeue();
                    _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name}. Есть элементы {_queue.Count} шт. - извлек один элемент сразу");
                    return part;
                }
                _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} нет элементов - вызывает Pulse");
                Monitor.Pulse(_lockOn);
                _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} нет элементов - вызывает Wait");
                Monitor.Wait(_lockOn);
                if (_queue.Count > 0)
                {
                    _logger.Add(
                        $"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - в очереди есть элемент");
                    return _queue.Dequeue();
                }
                _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - а очередь пустая!");
                return null;
            }
        }

        public void Enqueue(FilePart part)
        {
            lock (_lockOn)
            {
                _queue.Enqueue(part);
                _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} поместил в очередь элемент {part}");
                Monitor.Pulse(_lockOn);
            }
        }

        public FilePart Dequeue()
        {
            lock (_lockOn)
            {
                var part = _queue.Dequeue();
                _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} извлёк элемент {part}");
                return part;
            }
        }

        public void Unlock()
        {
            lock (_lockOn)
            {
                Monitor.Pulse(_lockOn);
            }
        }
    }
}
