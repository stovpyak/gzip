using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.QueueHandlers;
using ZipLib.QueueHandlers.Readers;
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
            var queueForRead = new PartQueue("ForRead", loggerForQueue);
            _queues.Add(queueForRead);
            var queueForCompress = new PartQueue("ForCompress", loggerForQueue);
            _queues.Add(queueForCompress);
            var queueForWrite = new IndexedParts("ForWrite", loggerForQueue);
            _queues.Add(queueForWrite);

            var stopEvent = new ManualResetEventSlim(false);
            // создание обработчиков очередей
            var writer = new Writer(_logger, targetFileNameProvider, stopEvent, queueForWrite, queueForRead);
            _queueHandlers.Add(writer);

            var archiversRuner = new CompressRuner(_logger, queueForCompress, queueForWrite);
            _queueHandlers.Add(archiversRuner);

            var partReader = new FilePartReader(_logger, strategy);
            var reader = new Reader(_logger, sourceFileNameProvider, partReader, queueForRead, queueForCompress);
            _queueHandlers.Add(reader);

            // вывод отладочной информации
            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");

            var maxActivePartCount = strategy.MaxActivePartCount;
            _logger.Add($"Максимальное кол-во одновременно обрабатываемых частей {maxActivePartCount} шт.");
            _logger.Add($"Размер одной части {strategy.PartSize} byte");
            _logger.Add("Работа начата...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (var i = 0; i < maxActivePartCount; i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueForRead.Add(part);
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
            var queueForRead = new PartQueue("ForRead", loggerForQueue);
            _queues.Add(queueForRead);
            var queueForDecompress = new PartQueue("ForDecompress", loggerForQueue);
            _queues.Add(queueForDecompress);
            var queueForWrite = new IndexedParts("ForWrite", loggerForQueue);
            _queues.Add(queueForWrite);
            
            var stopEvent = new ManualResetEventSlim(false);
            // создание обработчиков очередей
            var writer = new Writer(_logger, targetFileNameProvider, stopEvent, queueForWrite, queueForRead);
            _queueHandlers.Add(writer);

            var decompressRuner = new DecompressRuner(_logger, queueForDecompress, queueForWrite);
            _queueHandlers.Add(decompressRuner);

            var partReader = new ArсhivePartReader(_logger);
            var reader = new Reader(_logger, sourceFileNameProvider, partReader, queueForRead, queueForDecompress);
            _queueHandlers.Add(reader);

            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");
            _logger.Add("Работа начата...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (var i = 0; i < strategy.GetMaxActivePartCount(); i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueForRead.Add(part);
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
