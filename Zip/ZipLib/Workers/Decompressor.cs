using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.Workers
{
    public class Decompressor
    {
        private readonly Thread _thread;
        private readonly ILogger _logger;
        private readonly ProcessStatistic _statistic;

        private readonly FilePart _part;
        private readonly IQueue _nextQueue;

        public Decompressor(string name, ILogger logger, ProcessStatistic statistic, FilePart part, IQueue nextQueue)
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
            _logger.Add($"Поток {Thread.CurrentThread.Name} начал decompress");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Decompress();

            stopWatch.Stop();
            _logger.Add($"Поток {Thread.CurrentThread.Name} закончил decompress part {_part} за {stopWatch.ElapsedMilliseconds} ms");
            _statistic.Add(_thread.Name, stopWatch.ElapsedMilliseconds);

            _nextQueue?.Add(_part);
        }

        
        public void Decompress()
        {
            var memoryStream = new MemoryStream(_part.Source);
            using (var gzip = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = gzip.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);

                    _part.Source = null;
                    _part.Result = memory.ToArray();
                }
            }
        }
    }
}
