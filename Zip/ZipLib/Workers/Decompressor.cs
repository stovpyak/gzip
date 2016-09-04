using System.IO;
using System.IO.Compression;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.Workers
{
    public class Decompressor: WorkerBase, IWorker
    {
        public Decompressor(string name, ILogger logger, ISystemInfoProvider systemInfoProvider, ProcessStatistic statistic, IQueue nextQueue): 
            base(name, logger, systemInfoProvider, statistic, nextQueue)
        {
        }

        protected override void Execute(FilePart part)
        {
            Decompress(part);
        }

        public void Decompress(FilePart part)
        {
            var memoryStream = new MemoryStream(part.Source);
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

                    part.Source = null;
                    part.Result = memory.ToArray();
                }
            }
        }
    }
}
