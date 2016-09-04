using System;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class DecompressRuner : QueueHandlerWithWorkers
    {
        public DecompressRuner(ILogger logger, ISystemInfoProvider systemInfoProvider, Action<Exception> applExceptionHandler, 
            IQueue sourceQueue, IQueue nextQueue) : base(logger, systemInfoProvider, applExceptionHandler, sourceQueue, nextQueue)
        {
            InnerThread = new Thread(Run) {Name = "DecompressRuner"};
            InnerThread.Start();
        }

        private int _workerCount;

        protected override IWorker MakeWorker()
        {
            _workerCount++;
            var name = "DecompressorN" + _workerCount;
            var newWoker = new Decompressor(name, Logger, ApplExceptionHandler, SystemInfoProvider, Statistic, NextQueue);
            return newWoker;
        }
    }
}
