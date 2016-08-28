using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.QueueHandlers;
using ZipLib.Queues;
using ZipLib.Strategies;
using ZipLib.Workers;

namespace ZipLib
{
    public class Appl
    {
        private readonly IStrategy _strategy;
        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly IFileNameProvider _targetFileNameProvider;

        private PartQueue _queueEmpty;
        private PartQueue _queueForReaders;
        private PartQueue _queueForArchivers;
        private IndexedParts _queueForWriter;

        private ManualResetEventSlim _stopEvent;

        private ThreadStop _writerStop;
        private LoggerStringList _writerLogger;

        private ThreadStop _archiversRunerStop;
        private LoggerStringList _archiversLogger;

        private ThreadStop _stopReadersRuner;
        private LoggerStringList _readersLogger;

        private ThreadStop _stopPartInitializer;
        private LoggerStringList _partInitializerLogger;

        public Appl(IStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            _strategy = strategy;
            _sourceFileNameProvider = sourceFileNameProvider;
            _targetFileNameProvider = targetFileNameProvider;
        }

        public void Run()
        {
            Thread.CurrentThread.Name = "Main";

            var loggerForQueue = new LoggerDummy();
            _queueEmpty = new PartQueue("Empty", loggerForQueue);
            _queueForReaders = new PartQueue("ForReaders", loggerForQueue);
            _queueForArchivers = new PartQueue("ForArchivers", loggerForQueue);
            _queueForWriter = new IndexedParts("ForWriter", loggerForQueue);

            _writerStop = new ThreadStop();
            _writerLogger = new LoggerStringList();
            var writer = new Writer(_writerStop, _writerLogger, _targetFileNameProvider.GetFileName(), _queueForWriter, _queueEmpty);

            _archiversRunerStop = new ThreadStop();
            _archiversLogger = new LoggerStringList();
            var archiversRuner = new ArchiversRuner(_archiversRunerStop, _archiversLogger, _queueForArchivers, _queueForWriter);

            _stopReadersRuner = new ThreadStop();
            _readersLogger = new LoggerStringList();
            var readersRuner = new ReadersRuner(_stopReadersRuner, _readersLogger, _sourceFileNameProvider, _queueForReaders, _queueForArchivers);

            _stopPartInitializer = new ThreadStop();
            _partInitializerLogger = new LoggerStringList();
            _stopEvent = new ManualResetEventSlim(false);
            var partInitializer = new PartInitializer(_stopPartInitializer, _partInitializerLogger, _stopEvent, _strategy, _queueEmpty, _queueForReaders);

            var sourceFileName = _sourceFileNameProvider.GetFileName();
            var sourceFileInfo = new FileInfo(sourceFileName);
            Console.WriteLine($"Размер файла {sourceFileInfo.Length} byte");

            _strategy.StartFile(sourceFileInfo.Length);
            var maxActivePartCount = _strategy.GetMaxActivePartCount();
            Console.WriteLine($"Максимальное кол-во одновременно обрабатываемых частей {maxActivePartCount} шт.");
            Console.WriteLine($"Всего частей {_strategy.GetPartCount()} шт.");
            Console.WriteLine("Для начала архивирования нажмите любую клавишу...");
            Console.ReadKey();
            Console.WriteLine($"Работа начата...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < maxActivePartCount; i++)
            {
                var part = new FilePart($"FilePart{i+1}");
                _queueEmpty.Add(part);
            }

            // здесь выполнение остановится, пока кто нибудь не просигнализирует об окончании работы
            _stopEvent.Wait();
            stopWatch.Stop();
            
            Stop();
            ShowInfo();
            Console.WriteLine($"Работа завершена. Общее время работы {stopWatch.ElapsedMilliseconds} ms");
        }

        public void ShowInfo()
        {
            Console.WriteLine("Отчет о работе:");
            Console.WriteLine($"очередь {_queueEmpty.Name} Count {_queueEmpty.Count}");
            Console.WriteLine($"очередь {_queueForReaders.Name} Count {_queueForReaders.Count}");
            Console.WriteLine($"очередь {_queueForArchivers.Name} Count {_queueForArchivers.Count}");
            Console.WriteLine($"очередь {_queueForWriter.Name} Count {_queueForWriter.Count}");

            Console.WriteLine("Readers:");
            var items = _readersLogger.Items;
            foreach (var item in items)
                Console.WriteLine(item);

            Console.WriteLine("Archivers:");
            items = _archiversLogger.Items;
            foreach (var item in items)
                Console.WriteLine(item);
            
            //Console.WriteLine("Writer:");
            //items = _writerLogger.Items;
            //foreach (var item in items)
            //    Console.WriteLine(item);
        }

        public void Stop()
        {
            _writerStop.IsNeedStop = true;
            _archiversRunerStop.IsNeedStop = true;
            _stopReadersRuner.IsNeedStop = true;
            _stopPartInitializer.IsNeedStop = true;

            _queueEmpty.NotifyEndWait();
            _queueForReaders.NotifyEndWait();
            _queueForArchivers.NotifyEndWait();
            _queueForWriter.NotifyEndWait();
        }
    }
}
