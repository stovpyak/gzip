using System.Collections.Generic;
using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.Queues
{
    /// <summary>
    /// "Очередь" в которой части можно получить по index`у
    /// </summary>
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
            }
            ChangeEvent.Set();
        }

        /// <summary>
        /// Получить filePart из очереди
        /// </summary>
        /// <param name="param">параметром необходимо отправлять index filePart</param>
        /// <returns></returns>
        public FilePart GetPart(object param)
        {
            var index = (int) param;
            lock (LockOn)
            {
                FilePart part;
                if ((_indexToPartDict.Count <= 0) || (!_indexToPartDict.TryGetValue(index, out part)))
                    return null;
                Logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {Name}. Есть элемент c индексом {index} - извлек элемент сразу");
                _indexToPartDict.Remove(index);
                return part;
            }
        }
    }
}
