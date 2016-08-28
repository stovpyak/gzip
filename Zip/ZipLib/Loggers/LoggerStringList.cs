using System.Collections.Generic;

namespace ZipLib.Loggers
{
    public class LoggerStringList: ILogger
    {
        private readonly List<string> _items = new List<string>(); 

        public void Add(string msg)
        {
            lock (_items)
            {
                _items.Add(msg);
            }
        }

        public IReadOnlyList<string> Items
        {
            get
            {
                lock (_items)
                {
                    return _items.AsReadOnly();
                }
            }
        }
    }
}
