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
    /// <summary>
    /// "Конструирование" приложения из разных частей и запуск его на выполнение
    /// </summary>
    public class Appl
    {
        private readonly ILogger _logger;

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly List<IQueueHandler> _queueHandlers = new List<IQueueHandler>();

        public Appl(ILogger logger)
        {
            _logger = logger;
        }

        public void ExecuteCompress(ICompressStrategy strategy, IFileNameProvider sourceFileNameProvider, 
            IFileNameProvider targetFileNameProvider)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Main";
            Compress(strategy, sourceFileNameProvider, targetFileNameProvider);
        }

        public void ExecuteDecompress(IDecompressStrategy strategy, IFileNameProvider sourceFileNameProvider,
            IFileNameProvider targetFileNameProvider)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Main";
            Decompress(strategy, sourceFileNameProvider, targetFileNameProvider);
        }

        private void Compress(ICompressStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            var sourceFileName = sourceFileNameProvider.GetFileName();
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

            var stopEvent = new ManualResetEventSlim(false);
            // создание обработчиков очередей
            var writer = new Writer(_logger, targetFileNameProvider, stopEvent, queueForWriter, queueEmpty);
            _queueHandlers.Add(writer);

            var archiversRuner = new ArchiversRuner(_logger, queueForArchivers, queueForWriter);
            _queueHandlers.Add(archiversRuner);

            var reader = new Reader(_logger, sourceFileNameProvider, queueForReader, queueForArchivers);
            _queueHandlers.Add(reader);

            
            var partInitializer = new PartInitializer(_logger, strategy, queueEmpty, queueForReader);
            _queueHandlers.Add(partInitializer);

            // вывод отладочной информации
            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");

            strategy.StartFile(sourceFileInfo.Length);
            var maxActivePartCount = strategy.GetMaxActivePartCount();
            _logger.Add($"Максимальное кол-во одновременно обрабатываемых частей {maxActivePartCount} шт.");
            _logger.Add($"Всего частей {strategy.GetPartCount()} шт.");
            _logger.Add($"Размер одной части {strategy.GetPatrSize()} byte");
            _logger.Add("Работа начата...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (var i = 0; i < maxActivePartCount; i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueEmpty.Add(part);
            }

            // здесь выполнение остановится, пока кто нибудь не просигнализирует об окончании работы
            stopEvent.Wait();
            stopWatch.Stop();

            Stop();
            ShowInfo();
            _queueHandlers.Clear();
            _queues.Clear();

            _logger.Add($"Работа завершена. Общее время работы {stopWatch.ElapsedMilliseconds} ms");
        }

        private void Decompress(IDecompressStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            // нужно читать из файла части заархивированные
            // они начинаются с 10 байт (31,139,8,0,0,0,0,0,4,0)
            // эти части по отдельности отдавать на декомпрессию

            var sourceFileName = sourceFileNameProvider.GetFileName();
            if (!File.Exists(sourceFileName))
                throw new FileNotFoundException($"Не найден файл {sourceFileName}");

            // создание очередей
            var loggerForQueue = new LoggerDummy();
            var queueForReader = new PartQueue("ForReader", loggerForQueue);
            _queues.Add(queueForReader);
            var queueForDecompress = new PartQueue("ForDecompress", loggerForQueue);
            _queues.Add(queueForDecompress);
            var queueForWriter = new IndexedParts("ForWriter", loggerForQueue);
            _queues.Add(queueForWriter);

            var stopEvent = new ManualResetEventSlim(false);
            // создание обработчиков очередей
            var writer = new Writer(_logger, targetFileNameProvider, stopEvent, queueForWriter, queueForReader);
            _queueHandlers.Add(writer);

            var decompressRuner = new DecompressRuner(_logger, queueForDecompress, queueForWriter);
            _queueHandlers.Add(decompressRuner);

            var reader = new ArchiveReader(_logger, sourceFileNameProvider, strategy, queueForReader, queueForDecompress);
            _queueHandlers.Add(reader);
           

            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");
            _logger.Add("Работа начата...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (var i = 0; i < strategy.GetMaxActivePartCount(); i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueForReader.Add(part);
            }

            // здесь выполнение остановится, пока кто нибудь не просигнализирует об окончании работы
            stopEvent.Wait();
            stopWatch.Stop();

            Stop();
            ShowInfo();
            _queueHandlers.Clear();
            _queues.Clear();
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
