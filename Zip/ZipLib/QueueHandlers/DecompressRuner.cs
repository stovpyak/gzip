using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class DecompressRuner : QueueHandlerWithWorkers
    {
        public DecompressRuner(ILogger logger, ISystemInfoProvider systemInfoProvider, IQueue sourceQueue, IQueue nextQueue) :
            base(logger, systemInfoProvider, sourceQueue, nextQueue)
        {
            InnerThread = new Thread(Run) {Name = "DecompressRuner"};
            InnerThread.Start();
        }

        private int _workerCount;

        protected override IWorker MakeWorker()
        {
            _workerCount++;
            var name = "DecompressorN" + _workerCount;
            var newWoker = new Decompressor(name, Logger, SystemInfoProvider, Statistic, NextQueue);
            return newWoker;
        }
    }
}
