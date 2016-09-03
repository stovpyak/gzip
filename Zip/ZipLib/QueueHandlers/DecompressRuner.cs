using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class DecompressRuner : QueueHandlerWithWorkers
    {
        public DecompressRuner(ILogger logger, IQueue sourceQueue, IQueue nextQueue) :
            base(logger, sourceQueue, nextQueue)
        {
            InnerThread = new Thread(Run) {Name = "DecompressRuner"};
            InnerThread.Start();
        }

        private int _workerCount;

        protected override IWorker MakeWorker()
        {
            _workerCount++;
            var name = "DecompressorN" + _workerCount;
            var newWoker = new Decompressor(name, Logger, Statistic, NextQueue);
            return newWoker;
        }
    }
}
