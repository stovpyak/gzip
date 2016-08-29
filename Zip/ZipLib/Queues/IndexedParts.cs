using System.Collections.Generic;
using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.Queues
{
    public class IndexedParts: PartsBase, IQueue
    {
        private readonly Dictionary<int, FilePart> _indexToPartDict = new Dictionary<int, FilePart>();

        public IndexedParts(string name, ILogger logger): base(name, logger)
        {
        }

        public override int Count => _indexToPartDict.Count;

        public override void Add(FilePart part)
        {
            lock (LockOn)
            {
                _indexToPartDict.Add(part.Index, part);
                Logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {Name} поместил в очередь элемент {part}");
                //Monitor.Pulse(LockOn);
            }
            ChangeEvent.Set();
        }

        public FilePart GetPart(object param)
        {
            var index = (int) param;
            lock (LockOn)
            {
                FilePart part;
                if ((_indexToPartDict.Count > 0) && (_indexToPartDict.TryGetValue(index, out part)))
                {
                    Logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {Name}. Есть элемент c индексом {index} - извлек элемент сразу");
                    _indexToPartDict.Remove(index);
                    return part;
                }
                //Monitor.Pulse(LockOn);
                //Monitor.Wait(LockOn);
                //if (_indexToPartDict.Count > 0)
                //{
                //    if (_indexToPartDict.TryGetValue(index, out part))
                //    {
                //        Logger.Add(
                //            $"Поток {InnerThread.CurrentThread.Name} в очереди {Name} дождался unlock - в очереди есть элемент c индексом {index}");
                //        _indexToPartDict.Remove(index);
                //        return part;
                //    }
                //    Logger.Add(
                //        $"Поток {InnerThread.CurrentThread.Name} в очереди {Name} дождался unlock - очередь не пустая, но элемента с индексом {index} нет");
                //    return null;
                //}
                //Logger.Add($"Поток {InnerThread.CurrentThread.Name} в очереди {Name} дождался unlock - а очередь пустая!");
                return null;
            }
        }
    }
}
