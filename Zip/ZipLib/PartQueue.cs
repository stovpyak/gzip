using System;
using System.Collections.Generic;
using System.Threading;

namespace ZipLib
{
    public class PartQueue
    {
        private readonly Queue<FilePart> _queue = new Queue<FilePart>();
        private readonly object _lockOn = new object();
        private readonly string _name;

        public PartQueue(string name)
        {
            _name = name;
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
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name}. Есть элементы {_queue.Count} шт. - извлек один элемент сразу");
                    return part;
                }
                else
                {
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name} нет элементов - вызывает Pulse");
                    Monitor.Pulse(_lockOn);
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name} нет элементов - вызывает Wait");
                    Monitor.Wait(_lockOn);
                    if (_queue.Count > 0)
                    {
                        Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - в очереди есть элемент");
                        return _queue.Dequeue();
                    }
                    else
                    {
                        Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name} дождался unlock - а очередь пустая!");
                        return null;
                    }
                }
            }
        }

        public void Enqueue(FilePart part)
        {
            lock (_lockOn)
            {
                _queue.Enqueue(part);
                Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name}  поместил в очередь элемент {part.Name}");
                Monitor.Pulse(_lockOn);
            }
        }

        public FilePart Dequeue()
        {
            lock (_lockOn)
            {
                var part = _queue.Dequeue();
                Console.WriteLine($"Поток {Thread.CurrentThread.Name} в очереди {_name} извлёк элемент {part.Name}");
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
