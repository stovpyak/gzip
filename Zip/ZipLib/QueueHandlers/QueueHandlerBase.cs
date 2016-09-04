using System.Diagnostics;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Обработчик очереди. Базовый
    /// </summary>
    public abstract class QueueHandlerBase: IQueueHandler
    {
        protected readonly ILogger Logger;
        protected ISystemInfoProvider SystemInfoProvider;
        protected readonly IQueue SourceQueue;
        protected readonly IQueue NextQueue;
        protected Thread InnerThread;

        protected QueueHandlerBase(ILogger logger, ISystemInfoProvider systemInfoProvider, IQueue sourceQueue, IQueue nextQueue)
        {
            Logger = logger;
            SystemInfoProvider = systemInfoProvider;
            SourceQueue = sourceQueue;
            NextQueue = nextQueue;
        }

        private readonly Stopwatch _stopwatchProcess = new Stopwatch();
        protected long TotalProcess;

        private int _processedPartCount;

        protected void Run()
        {
            while (!IsNeedStop)
            {
                var part = SourceQueue.GetPart(GetParamForGetPart());
                if (part != null)
                {
                    Logger.Add($"Поток {Thread.CurrentThread.Name} перед обработкой части {part} приложение занимает в памяти {SystemInfoProvider.PagedMemorySize64} byte");
                    _stopwatchProcess.Reset();
                    _stopwatchProcess.Start();
                    if (ProcessPart(part))
                    {
                        _processedPartCount++;
                    }
                    _stopwatchProcess.Stop();
                    TotalProcess = TotalProcess + _stopwatchProcess.ElapsedMilliseconds;
                    Logger.Add($"Поток {Thread.CurrentThread.Name} время обработки части {part} {_stopwatchWait.ElapsedMilliseconds} ms");
                    Logger.Add($"Поток {Thread.CurrentThread.Name} после обработкой части {part} приложение занимает в памяти {SystemInfoProvider.PagedMemorySize64} byte");
                }
                else
                    WaitQueue();
            }
            Close();
            AddTotalToLog();
        }

        private volatile bool _isNeedStop;

        /// <summary>
        /// Установить признак того, что необходимо завершить выполнение
        /// </summary>
        public void SetIsNeedStop()
        {
            _isNeedStop = true;
        }

        private bool IsNeedStop => _isNeedStop;

        protected virtual object GetParamForGetPart()
        {
            return null;
        }

        protected abstract bool ProcessPart(FilePart part);

        private readonly Stopwatch _stopwatchWait = new Stopwatch();
        protected long TotalWait;

        protected void WaitQueue()
        {
            _stopwatchWait.Reset();
            _stopwatchWait.Start();

            SourceQueue.ChangeEvent.WaitOne();

            _stopwatchWait.Stop();
            TotalWait = TotalWait + _stopwatchWait.ElapsedMilliseconds;
            Logger.Add($"Поток {Thread.CurrentThread.Name} время ожидания {_stopwatchWait.ElapsedMilliseconds} ms");
        }

        protected virtual void Close()
        {
            // empty
        }

        protected virtual void AddTotalToLog()
        {
            Logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
            Logger.Add($"Поток {Thread.CurrentThread.Name} обработано {_processedPartCount} частей");
            Logger.Add($"Поток {Thread.CurrentThread.Name} общее время обработки частей {TotalProcess} ms");
            Logger.Add($"Поток {Thread.CurrentThread.Name} общее время ожидания поступления частей {TotalWait} ms");
        }

        public void Join()
        {
            InnerThread.Join();
        }
    }
}
