using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Следит за очередью пустых частей
    /// Инициализирует их
    /// </summary>
    public class PartInitializer: QueueHandlerBase
    {
        private readonly ManualResetEventSlim _stopEvent;
        private readonly IStrategy _strategy;

        public PartInitializer(ILogger logger, ManualResetEventSlim stopEvent, IStrategy strategy, IQueue sourceQueue, IQueue nextQueue)
            :base(logger, sourceQueue, nextQueue)
        {
            _stopEvent = stopEvent;
            _strategy = strategy;
  
            InnerThread = new Thread(this.Run) { Name = "PartInitializer" };
            InnerThread.Start();
        }

        private int _removedPartCount;

        protected override bool ProcessPart(FilePart part)
        {
            Logger.Add($"Поток {Thread.CurrentThread.Name} получил из очереди {SourceQueue.Name} part {part}");
            if (_strategy.InitNextFilePart(part))
            {
                Logger.Add($"Поток {Thread.CurrentThread.Name} проинициализировал part {part}");
                NextQueue?.Add(part);
                return true;
            }
            Logger.Add(
                $"!Поток {Thread.CurrentThread.Name} НЕ проинициализировал part {part} - исходный файл прочитан");
            _removedPartCount++;
            if (_removedPartCount == _strategy.GetMaxActivePartCount())
            {
                Logger.Add(
                    $"!Поток {Thread.CurrentThread.Name} выведены все обрабатываемые части {_removedPartCount} шт. - это признак того, что работа завершена");
                _stopEvent.Set();
            }
            return false;
        }
    }
}
