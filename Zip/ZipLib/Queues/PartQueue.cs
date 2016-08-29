using System.Collections.Generic;
using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.Queues
{
    public class PartQueue: PartsBase, IQueue
    {
        private readonly Queue<FilePart> _queue = new Queue<FilePart>();

        public PartQueue(string name, ILogger logger) : base(name, logger)
        {
        }

        public override int Count => _queue.Count;

        public FilePart GetPart(object param)
        {
            lock (LockOn)
            {
                if (_queue.Count > 0)
                {
                    var part = _queue.Dequeue();
                    Logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {Name}. Есть элементы {_queue.Count} шт. - извлек один элемент сразу");
                    return part;
                }
                //Monitor.Pulse(LockOn);
                //Monitor.Wait(LockOn);
                //if (_queue.Count > 0)
                //{
                //    Logger.Add(
                //        $"Поток {InnerThread.CurrentThread.Name} в очереди {Name} дождался unlock - в очереди есть элемент");
                //    return _queue.Dequeue();
                //}
                //Logger.Add($"Поток {InnerThread.CurrentThread.Name} в очереди {Name} дождался unlock - а очередь пустая!");
                return null;
            }
        }

        public override void Add(FilePart part)
        {
            lock (LockOn)
            {
                _queue.Enqueue(part);
                Logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {Name} поместил в очередь элемент {part}");
                //Monitor.Pulse(LockOn);
            }
            ChangeEvent.Set();
        }
    }
}
