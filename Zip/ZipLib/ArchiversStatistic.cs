using System.Collections.Generic;

namespace ZipLib
{
    /// <summary>
    /// Собираем статистику работы архиваторов
    /// </summary>
    public class ArchiversStatistic
    {
        private readonly List<InfoItem> _items = new List<InfoItem>();
        private readonly object _lockOn = new object();

        public void Add(string name, long elapsedMilliseconds)
        {
            lock (_lockOn)
            {
                var item = new InfoItem
                {
                    Name = name,
                    ElapsedMilliseconds = elapsedMilliseconds
                };
                _items.Add(item);
            }
        }

        public long GetMiddleElapsedTime()
        {
            lock (_lockOn)
            {
                long sum = 0;
                foreach (var item in _items)
                    sum = sum + item.ElapsedMilliseconds;
                return sum/_items.Count;
            }
        }

        private class InfoItem
        {
            public string Name { get; set; }
            public long ElapsedMilliseconds { get; set; }
        }
    }
}
