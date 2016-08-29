using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.QueueHandlers;
using ZipLib.Queues;
using ZipLib.Strategies;

namespace ZipLib
{
    /// <summary>
    /// "Конструирование" приложения из разных частей и запуск его на выполнение
    /// </summary>
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
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Main";

            switch (mode)
            {
                case ApplMode.Compress:
                    Compress();
                    break;
                case ApplMode.Decompress:
                    Decompress();
                    break;
                default:
                    throw new Exception("Неизвеcтное значение ApplMode");
            }
        }

        private void Compress()
        {
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

            for (var i = 0; i < maxActivePartCount; i++)
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

        private void Decompress()
        {
            // нужно читать из файла части заархивированные
            // они начинаются с 10 байт (31,139,8,0,0,0,0,0,4,0)
            // эти части по отдельности отдавать на декомпрессию

            // ! это черновой вариант:
            // работает только с архивом, состоящим из одной части
            // все делает в одном потоке
            var sourceFileName = _sourceFileNameProvider.GetFileName();

            if (!File.Exists(sourceFileName))
                throw new FileNotFoundException($"Не найден файл {sourceFileName}");

            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");

            using (var sourceStream = File.OpenRead(sourceFileName))
            {
                using (var gzStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                {
                    using (var targetStream = File.Create(_targetFileNameProvider.GetFileName()))
                    {
                        byte[] buffer = new byte[sourceFileInfo.Length];
                        int nRead;
                        while ((nRead = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            targetStream.Write(buffer, 0, nRead);
                        }
                    }
                }
            }
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
