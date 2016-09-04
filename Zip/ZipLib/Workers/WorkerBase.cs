using System;
using System.Diagnostics;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.Workers
{
    public abstract class WorkerBase
    {
        private readonly ILogger _logger;
        private readonly IQueue _nextQueue;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly ProcessStatistic _statistic;

        private FilePart _part;
        private readonly Action<Exception> _exceptionHandler;

        protected WorkerBase(string name, ILogger logger, Action<Exception> exceptionHandler, 
            ISystemInfoProvider systemInfoProvider, ProcessStatistic statistic, IQueue nextQueue)
        {
            Name = name;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
            _systemInfoProvider = systemInfoProvider;
            _statistic = statistic;
            _nextQueue = nextQueue;
        }

        public void ProcessPart(FilePart part)
        {
            try
            {
                _part = part;
                ThreadPool.QueueUserWorkItem(Run);
            }
            catch (Exception ex)
            {
                _exceptionHandler(ex);
            }
        }

        private void Run(object state)
        {
            try
            {
                _logger.Add($"{Name}; ThreadId={Thread.CurrentThread.ManagedThreadId} начал работу");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Execute(_part);

                stopWatch.Stop();
                _logger.Add(
                    $"{Name}; ThreadId={Thread.CurrentThread.ManagedThreadId} закончил заботу с part {_part} за {stopWatch.ElapsedMilliseconds} ms");
                _statistic.Add(Name, stopWatch.ElapsedMilliseconds, _systemInfoProvider.PagedMemorySize64);
                _nextQueue?.Add(_part);
            }
            catch (Exception ex)
            {
                _exceptionHandler(ex);
            }
        }

        protected abstract void Execute(FilePart part);

        public string Name { get; private set; }
    }
}
