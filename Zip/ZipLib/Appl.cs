using System;
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
        private readonly ISystemInfoProvider _systemInfoProvider;

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly List<IQueueHandler> _queueHandlers = new List<IQueueHandler>();
        private ManualResetEventSlim _stopEvent;
        private readonly Stopwatch _stopWatch = new Stopwatch();

        public Appl(ILogger logger, ISystemInfoProvider systemInfoProvider)
        {
            _logger = logger;
            _systemInfoProvider = systemInfoProvider;
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

        private void AddSystemInfo()
        {
            _logger.Add($"Информация о системе:");
            _logger.Add($"Количество процессоров= {_systemInfoProvider.ProcessorCount} шт.");
            _logger.Add(_systemInfoProvider.ApplIs64Bit ? $"Приложение x64" : $"Приложение x86");
        }

        private void Compress(ICompressStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            _wasException = null;
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

            _stopEvent = new ManualResetEventSlim(false);
            // создание обработчиков очередей
            var writer = new Writer(_logger, _systemInfoProvider, ApplExceptionHandler, targetFileNameProvider, _stopEvent, queueForWrite, queueForRead);
            _queueHandlers.Add(writer);

            var archiversRuner = new CompressRuner(_logger, _systemInfoProvider, ApplExceptionHandler, queueForCompress, queueForWrite);
            _queueHandlers.Add(archiversRuner);

            var partReader = new FilePartReader(_logger, strategy);
            var reader = new Reader(_logger, _systemInfoProvider, ApplExceptionHandler, sourceFileNameProvider, partReader, queueForRead, queueForCompress);
            _queueHandlers.Add(reader);

            // вывод отладочной информации
            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");
            AddSystemInfo();
            var maxActivePartCount = strategy.MaxActivePartCount;
            _logger.Add($"Максимальное кол-во одновременно обрабатываемых частей {maxActivePartCount} шт.");
            _logger.Add($"Размер одной части {strategy.PartSize} byte");
            _logger.Add("Работа начата...");

            _stopWatch.Reset();
            _stopWatch.Start();

            for (var i = 0; i < maxActivePartCount; i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueForRead.Add(part);
            }

            StopEventWait();
        }

        private void Decompress(IDecompressStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            // нужно читать из файла части заархивированные
            // они начинаются с 10 байт (31,139,8,0,0,0,0,0,4,0)
            // эти части по отдельности отдавать на декомпрессию

            _wasException = null;
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
            
            _stopEvent = new ManualResetEventSlim(false);
            // создание обработчиков очередей
            var writer = new Writer(_logger, _systemInfoProvider, ApplExceptionHandler, targetFileNameProvider, _stopEvent, queueForWrite, queueForRead);
            _queueHandlers.Add(writer);

            var decompressRuner = new DecompressRuner(_logger, _systemInfoProvider, ApplExceptionHandler, queueForDecompress, queueForWrite);
            _queueHandlers.Add(decompressRuner);

            var partReader = new ArсhivePartReader(_logger);
            var reader = new Reader(_logger, _systemInfoProvider, ApplExceptionHandler, sourceFileNameProvider, partReader, queueForRead, queueForDecompress);
            _queueHandlers.Add(reader);

            var sourceFileInfo = new FileInfo(sourceFileName);
            _logger.Add($"Размер файла {sourceFileInfo.Length} byte");
            AddSystemInfo();
            _logger.Add("Работа начата...");

            _stopWatch.Reset();
            _stopWatch.Start();

            for (var i = 0; i < strategy.MaxActivePartCount; i++)
            {
                var part = new FilePart($"FilePart{i + 1}");
                queueForRead.Add(part);
            }

            StopEventWait();
        }

        private void StopEventWait()
        {
            // здесь выполнение остановится, пока кто нибудь не просигнализирует об окончании работы
            _stopEvent.Wait();
            _stopWatch.Stop();

            Stop();
            ShowInfo();
            _queueHandlers.Clear();
            _queues.Clear();

            if (_wasException != null)
                throw new Exception("Ошибка в дочернем потоке", _wasException);

            _logger.Add($"Работа завершена. Общее время работы {_stopWatch.ElapsedMilliseconds} ms");
        }

        private Exception _wasException;
        private void ApplExceptionHandler(Exception ex)
        {
            _wasException = ex;
            _stopEvent.Set();
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
