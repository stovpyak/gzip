using System.Diagnostics;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Обработчик очереди. Базовый
    /// </summary>
    public abstract class QueueHandlerBase: IQueueHandler
    {
        protected readonly ILogger Logger;
        protected readonly IQueue SourceQueue;
        protected readonly IQueue NextQueue;
        protected Thread InnerThread;

        protected QueueHandlerBase(ILogger logger, IQueue sourceQueue, IQueue nextQueue)
        {
            Logger = logger;
            SourceQueue = sourceQueue;
            NextQueue = nextQueue;
        }

        private int _processedPartCount;

        protected void Run()
        {
            while (!IsNeedStop)
            {
                var part = SourceQueue.GetPart(GetParamForGetPart());
                if (part != null)
                {
                    if (ProcessPart(part))
                        _processedPartCount++;
                }
                else
                    WaitQueue();
            }
            Close();
            AddTotalToLog();
        }

        // todo: если встретится ещё подобная пара, то вынести в отдельную структуру
        private bool _isNeedStop;
        private readonly object _lockOnNeedStop = new object();

        /// <summary>
        /// Установить признак того, что необходимо завершить выполнение
        /// </summary>
        public void SetIsNeedStop()
        {
            lock (_lockOnNeedStop)
            {
                _isNeedStop = true;
            }
        }

        private bool IsNeedStop
        {
            get
            {
                lock (_lockOnNeedStop)
                {
                    return _isNeedStop;
                }
            }
        }

        protected virtual object GetParamForGetPart()
        {
            return null;
        }

        protected virtual bool ProcessPart(FilePart part)
        {
            return true;
        }

        private readonly Stopwatch _stopwatchWait = new Stopwatch();
        protected long TotalWait;

        protected void WaitQueue()
        {
            _stopwatchWait.Reset();
            _stopwatchWait.Start();
            // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
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
            Logger.Add($"Поток {Thread.CurrentThread.Name} общее время ожидания поступления частей {TotalWait} ms");
        }

        public void Join()
        {
            InnerThread.Join();
        }
    }
}
