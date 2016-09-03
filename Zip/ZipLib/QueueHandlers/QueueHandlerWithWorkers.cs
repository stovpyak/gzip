using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public abstract class QueueHandlerWithWorkers: QueueHandlerBase
    {
        protected ProcessStatistic Statistic = new ProcessStatistic();

        protected QueueHandlerWithWorkers(ILogger logger, IQueue sourceQueue, IQueue nextQueue) : 
            base(logger, sourceQueue, nextQueue)
        {
        }

        protected override bool ProcessPart(FilePart part)
        {
            var worker = MakeWorker();
            Logger.Add($"Поток {Thread.CurrentThread.Name} отдал part {part} worker`у {worker.Name}");
            worker.ProcessPart(part);

            // часть последняя - сам поток решает, что ему пора остановиться
            if (part.IsLast)
                SetIsNeedStop();
            return true;
        }

        protected abstract IWorker MakeWorker();

        protected override void AddTotalToLog()
        {
            base.AddTotalToLog();
            Logger.Add($"Поток {Thread.CurrentThread.Name} общее время работы workers {Statistic.GetTotalTime()} ms");
        }
    }
}
