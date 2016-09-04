using System.IO;
using System.IO.Compression;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.Workers
{
    /// <summary>
    /// Архивирует данные
    /// </summary>
    public class Compressor: WorkerBase, IWorker
    {
        public Compressor(string name, ILogger logger, ISystemInfoProvider systemInfoProvide, ProcessStatistic statistic, IQueue nextQueue): 
            base(name, logger, systemInfoProvide, statistic, nextQueue)
        {
        }

        protected override void Execute(FilePart part)
        {
            Compress(part);
        }

        public void Compress(FilePart part)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzip.Write(part.Source, 0, part.SourceSize);
                }
                part.Source = null;
                part.Result = memoryStream.ToArray(); 
            }
        }
    }
}
