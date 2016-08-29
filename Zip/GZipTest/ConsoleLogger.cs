using System;
using ZipLib.Loggers;

namespace GZipTest
{
    /// <summary>
    /// Logger выводит всё на консоль
    /// </summary>
    public class ConsoleLogger: ILogger
    {
        private readonly object _lockOn = new object();

        public void Add(string msg)
        {
            lock (_lockOn)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
