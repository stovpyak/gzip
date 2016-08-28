using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.Workers
{
    public class Reader
    {
        private readonly ILogger _logger;
        private readonly string _sourceFileName;

        private readonly FilePart _part;
        private readonly Thread _thread;
        private readonly IQueue _nextQueue;

        public Reader(string name, ILogger logger, string sourceFileName, FilePart part, IQueue nextQueue)
        {
            _logger = logger;
            _sourceFileName = sourceFileName;
            _part = part;
            _nextQueue = nextQueue;

            _thread = new Thread(this.Run) {Name = name};
        }

        public void Start()
        {
            _thread.Start();
        }

        private void Run()
        {
            _logger.Add($"Поток {Thread.CurrentThread.Name} начал читать");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fsInput = new FileStream(_sourceFileName, FileMode.Open, FileAccess.Read);
            try
            {
                if (_part.StartPosition != 0)
                    fsInput.Seek(_part.StartPosition, SeekOrigin.Begin);

                _part.Source = new Byte[_part.SourceSize];
                var count = fsInput.Read(_part.Source, 0, _part.SourceSize);

                stopWatch.Stop();
                _logger.Add($"Поток {Thread.CurrentThread.Name} прочитал в часть {_part} {count} byte за {stopWatch.ElapsedMilliseconds} ms");
            }
            catch (Exception)
            {
                _logger.Add($"Поток {Thread.CurrentThread.Name} - ошибка при чтении");
                throw;
            }
            finally
            {
                fsInput.Close();
            }

            // сам reader помещает part в следующую очередь
            _nextQueue?.Add(_part);
            _logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
        }
    }
}
