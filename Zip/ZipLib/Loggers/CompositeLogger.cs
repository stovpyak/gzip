using System.Collections.Generic;

namespace ZipLib.Loggers
{
    /// <summary>
    /// Логгер-композит. Отправляет сообщения нескольким логерам
    /// </summary>
    public class CompositeLogger: ILogger
    {
        private readonly List<ILogger> _children = new List<ILogger>();

        public void AddChild(ILogger child)
        {
            lock (_children)
            {
                _children.Add(child);
            }
        }

        public void Add(string msg)
        {
            lock (_children)
            {
                foreach (var child in _children)
                {
                    child.Add(msg);
                }
            }
        }
    }
}
