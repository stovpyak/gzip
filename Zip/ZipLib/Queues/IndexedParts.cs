using System;
using System.Collections.Generic;
using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.Queues
{
    public class IndexedParts
    {
        private readonly string _name;
        private readonly ILogger _logger;

        private readonly Dictionary<int, FilePart> _indexToPartDict = new Dictionary<int, FilePart>();
        private readonly object _lockOn = new object();
        

        public IndexedParts(string name, ILogger logger)
        {
            _name = name;
            _logger = logger;
        }

        public string Name => _name;

        public int Count => _indexToPartDict.Count;

        public void Add(FilePart part)
        {
            lock (_lockOn)
            {
                _indexToPartDict.Add(part.Index, part);
                _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} поместил в очередь элемент {part}");
                Monitor.Pulse(_lockOn);
            }
        }

        public FilePart GetPartByIndex(int index)
        {
            lock (_lockOn)
            {
                FilePart part;
                if ((_indexToPartDict.Count > 0) && (_indexToPartDict.TryGetValue(index, out part)))
                {
                    _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name}. Есть элемент c индексом {index} - извлек элемент сразу");
                    _indexToPartDict.Remove(index);
                    return part;
                }
                else
                {
                    _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} нет элементов - вызывает Pulse");
                    Monitor.Pulse(_lockOn);
                    _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} нет элементов - вызывает Wait");
                    Monitor.Wait(_lockOn);
                    if (_indexToPartDict.Count > 0)
                    {
                        if (_indexToPartDict.TryGetValue(index, out part))
                        {
                            _logger.Add(
                                $"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - в очереди есть элемент c индексом {index}");
                            _indexToPartDict.Remove(index);
                            return part;
                        }
                        _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - очередь не пустая, но элемента с индексом {index} нет");
                        return null;
                    }
                    _logger.Add($"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - а очередь пустая!");
                    return null;
                }
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
