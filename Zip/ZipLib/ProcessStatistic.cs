﻿using System.Collections.Generic;
using System.Linq;

namespace ZipLib
{
    /// <summary>
    /// Собираем статистику работы архиваторов
    /// </summary>
    public class ProcessStatistic
    {
        private readonly List<InfoItem> _items = new List<InfoItem>();
        private readonly object _lockOn = new object();

        public void Add(string name, long elapsedMilliseconds, long currentMemory)
        {
            lock (_lockOn)
            {
                var item = new InfoItem
                {
                    Name = name,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    CurrentMemory = currentMemory
                };
                _items.Add(item);
            }
        }

        public long GetMiddleElapsedTime()
        {
            lock (_lockOn)
            {
                long sum = _items.Aggregate<InfoItem, long>(0, (current, item) => current + item.ElapsedMilliseconds);
                return sum/_items.Count;
            }
        }
        public long GetTotalTime()
        {
            lock (_lockOn)
            {
                if (_items.Any())
                    return _items.Sum(item => item.ElapsedMilliseconds);
                return 0;
            }
        }

        public long MaxMemory
        {
            get
            {
                lock (_lockOn)
                {
                    if (_items.Any())
                        return _items.Max(item => item.CurrentMemory);
                    return 0;
                }
            }
        }

        private class InfoItem
        {
            public string Name { get; set; }
            public long ElapsedMilliseconds { get; set; }
            public long CurrentMemory { get; set; }
        }
    }
}
