using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Следит за очередью частей готовых для архивирования
    /// Запускает потоки для архивации частей
    /// </summary>
    public class CompressRuner: QueueHandlerWithWorkers
    {
        public CompressRuner(ILogger logger, ISystemInfoProvider systemInfoProvider, IQueue sourceQueue, IQueue nextQueue)
            :base(logger, systemInfoProvider, sourceQueue, nextQueue)
        {
            InnerThread = new Thread(Run) { Name = "CompressRuner" };
            InnerThread.Start();
        }
       
        private int _workerCount;

        protected override IWorker MakeWorker()
        {
            _workerCount++;
            var name = "CompressorN" + _workerCount;
            var newWorker = new Compressor(name, Logger, SystemInfoProvider, Statistic, NextQueue);
            return newWorker;
        }
    }
}
