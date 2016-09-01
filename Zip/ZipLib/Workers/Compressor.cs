using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.Workers
{
    /// <summary>
    /// Архивирует данные
    /// </summary>
    public class Compressor
    {
        private readonly Thread _thread;
        private readonly ILogger _logger;
        private readonly ProcessStatistic _statistic;

        private readonly FilePart _part;
        private readonly IQueue _nextQueue;

        public Compressor(string name, ILogger logger, ProcessStatistic statistic, FilePart part, IQueue nextQueue)
        {
            _logger = logger;
            _part = part;
            _statistic = statistic;
            _nextQueue = nextQueue;

            _thread = new Thread(this.Run) { Name = name };
        }

        public void Start()
        {
            _thread.Start();
        }

        private void Run()
        {
            _logger.Add($"Поток {Thread.CurrentThread.Name} начал архивировать");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Compress();

            stopWatch.Stop();
            _logger.Add($"Поток {Thread.CurrentThread.Name} закончил архивировать part {_part} за {stopWatch.ElapsedMilliseconds} ms");
            _statistic.Add(_thread.Name, stopWatch.ElapsedMilliseconds);

            _nextQueue?.Add(_part);
        }

        public void Compress()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzip.Write(_part.Source, 0, _part.Source.Length);
                }
                _part.Source = null;
                _part.Result = memoryStream.ToArray(); // todo возможно можно и не преобразовывать в массив
            }
        }
    }
}
