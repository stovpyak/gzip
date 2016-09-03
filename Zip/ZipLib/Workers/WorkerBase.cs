using System.Diagnostics;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.Workers
{
    public abstract class WorkerBase
    {
        private readonly ILogger _logger;
        private readonly IQueue _nextQueue;
        private readonly ProcessStatistic _statistic;

        private FilePart _part;
        
        protected WorkerBase(string name, ILogger logger, ProcessStatistic statistic, IQueue nextQueue)
        {
            Name = name;
            _logger = logger;
            _statistic = statistic;
            _nextQueue = nextQueue;
        }

        public void ProcessPart(FilePart part)
        {
            _part = part;
            ThreadPool.QueueUserWorkItem(Run);
        }

        private void Run(object state)
        {
            _logger.Add($"{Name}; ThreadId={Thread.CurrentThread.ManagedThreadId} начал работу");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Execute(_part);

            stopWatch.Stop();
            _logger.Add($"{Name}; ThreadId={Thread.CurrentThread.ManagedThreadId} закончил заботу с part {_part} за {stopWatch.ElapsedMilliseconds} ms");
            _statistic.Add(Name, stopWatch.ElapsedMilliseconds);
            _nextQueue?.Add(_part);
        }

        protected abstract void Execute(FilePart part);

        public string Name { get; private set; }
    }
}
