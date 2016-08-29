using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.QueueHandlers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib
{
    public class Appl
    {
        private readonly ILogger _logger;
        private readonly IStrategy _strategy;
        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly IFileNameProvider _targetFileNameProvider;

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly List<IQueueHandler> _queueHandlers = new List<IQueueHandler>();

        private ManualResetEventSlim _stopEvent;

        private Writer _writer;
        private ArchiversRuner _archiversRuner;
        private Reader _reader;
        private PartInitializer _partInitializer;

        public Appl(ILogger logger, IStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            _logger = logger;
            _strategy = strategy;
            _sourceFileNameProvider = sourceFileNameProvider;
            _targetFileNameProvider = targetFileNameProvider;
        }

        public void Execute(ApplMode mode)
        {
            switch (mode)
            {
                case ApplMode.Compress:
                    Compress();
                    break;
                case ApplMode.Decompress:
                    throw new Exception("Режим decompress пока не готов");
                default:
                    throw new Exception("Неизвеcтное значение ApplMode");
            }
        }

        private void Compress()
        {
            Thread.CurrentThread.Name = "Main";
            var sourceFileName = _sourceFileNameProvider.GetFileName();
            if (!File.Exists(sourceFileName))
                throw new FileNotFoundException($"Не найден файл {sourceFileName}");

            // создание очередей
            var loggerForQueue = new LoggerDummy();
            var queueEmpty = new PartQueue("Empty", loggerForQueue);
            _queues.Add(queueEmpty);
            var queueForReader = new PartQueue("ForReader", loggerForQueue);
            _queues.Add(queueForReader);
            var queueForArchivers = new PartQueue("ForArchivers", loggerForQueue);
            _queues.Add(queueForArchivers);
            var queueForWriter = new IndexedParts("ForWriter", loggerForQueue);
            _queues.Add(queueForWriter);

            // создание обработчиков очередей
            _writer = new Writer(_logger, _targetFileNameProvider, queueForWriter, queueEmpty);
            _queueHandlers.Add(_writer);

            _archiversRuner = new ArchiversRuner(_logger, queueForArchivers, queueForWriter);
            _queueHandlers.Add(_archiversRuner);

            _reader = new Reader(_logger, _sourceFileNameProvider, queueForReader, queueForArchivers);
            _queueHandlers.Add(_reader);

            _stopEvent = new ManualResetEventSlim(false);
            _partInitializer = new PartInitializer(_logger, _stopEvent, _strategy,
                queueEmpty, queueForReader);
            _queueHandlers.Add(_partInitializer);

            // вывод отладочной информации
            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");

            _strategy.StartFile(sourceFileInfo.Length);
            var maxActivePartCount = _strategy.GetMaxActivePartCount();
            _logger.Add($"Максимальное кол-во одновременно обрабатываемых частей {maxActivePartCount} шт.");
            _logger.Add($"Всего частей {_strategy.GetPartCount()} шт.");
            _logger.Add("Работа начата...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < maxActivePartCount; i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueEmpty.Add(part);
            }

            // здесь выполнение остановится, пока кто нибудь не просигнализирует об окончании работы
            _stopEvent.Wait();
            stopWatch.Stop();

            Stop();
            ShowInfo();
            _logger.Add($"Работа завершена. Общее время работы {stopWatch.ElapsedMilliseconds} ms");
        }
    
        public void ShowInfo()
        {
            foreach (var queue in _queues)
                _logger.Add($"очередь {queue.Name} Count {queue.Count}");
        }

        public void Stop()
        {
            foreach (var queueHandler in _queueHandlers)
                queueHandler.SetIsNeedStop();

            foreach (var queue in _queues)
                queue.NotifyEndWait();

            foreach (var queueHandler in _queueHandlers)
                queueHandler.Join();
        }
    }
}
