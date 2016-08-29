using System.Collections.Generic;
using System.IO;

namespace ZipLib.Loggers
{
    public class FileLogger: ILogger
    {
        private readonly List<string> _items = new List<string>();

        public void Add(string msg)
        {
            lock (_items)
            {
                _items.Add(msg);
            }
        }

        public void SaveToFile(string fileName)
        {
            lock (_items)
            {
                using (var fileStream = File.Create(fileName))
                {
                    using (var fileWriter = new StreamWriter(fileStream))
                    {
                        foreach (var item in _items)
                        {
                            fileWriter.WriteLine(item + "\n");
                        }
                    }
                }
            }
        }
    }
}
