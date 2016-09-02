using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class DecompressRuner: QueueHandlerBase
    {
        private readonly ProcessStatistic _statistic = new ProcessStatistic();

        public DecompressRuner(ILogger logger, IQueue sourceQueue, IQueue nextQueue) : 
            base(logger, sourceQueue, nextQueue)
        {
            InnerThread = new Thread(this.Run) { Name = "DecompressRuner" };
            InnerThread.Start();
        }

        private int _decompressorCount;

        protected override bool ProcessPart(FilePart part)
        {
            _decompressorCount++;
            var decompressorName = "DecomperssorN" + _decompressorCount;
            var decompressor = new Decompressor(decompressorName, Logger, _statistic, part, NextQueue);
            Logger.Add($"Поток {Thread.CurrentThread.Name} отдал part {part} decomperssor`у {decompressorName}");
            decompressor.Start();

            // часть последняя - сам поток решает, что ему пора остановиться
            if (part.IsLast)
                SetIsNeedStop();
            return true;
        }

        protected override void AddTotalToLog()
        {
            base.AddTotalToLog();
            Logger.Add($"Поток {Thread.CurrentThread.Name} общее время decompress {_statistic.GetTotalTime()} ms");
            Logger.Add($"Поток {Thread.CurrentThread.Name} среднее время decompress одной части {_statistic.GetMiddleElapsedTime()} ms");
        }

    }
}
