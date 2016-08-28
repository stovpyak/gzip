using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class ReadersRuner
    {
        private readonly ThreadStop _threadStop;
        private readonly ILogger _logger;

        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly IQueue _sourceQueue;
        private readonly IQueue _nextQueue;

        public ReadersRuner(ThreadStop threadStop, ILogger logger, IFileNameProvider sourceFileNameProvider, IQueue sourceQueue, IQueue nextQueue)
        {
            _threadStop = threadStop;
            _logger = logger;
            _sourceFileNameProvider = sourceFileNameProvider;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;

            var thread = new Thread(this.Run) {Name = "ReadersRuner"};
            thread.Start();
        }

        private int _readersCount;
        private long _totalWait;
        //private FileStream _fsInput;

        private void Run()
        {
            var stopWatch = new Stopwatch();
            var stopwatchWait = new Stopwatch();
            while (!_threadStop.IsNeedStop)
            {
                // при вызове этого метода исполенение должно остановиться, если очередь пустая. и возобновиться, как только в ней появится part
                stopwatchWait.Reset();
                stopwatchWait.Start();
                var part = _sourceQueue.GetPart();
                stopwatchWait.Stop();
                if (part != null)
                {
                    _totalWait = _totalWait + stopwatchWait.ElapsedMilliseconds;
                    _logger.Add($"Поток {Thread.CurrentThread.Name} время ожидания {stopwatchWait.ElapsedMilliseconds} ms");

                    //_readersCount++;
                    //var readerName = "ReaderN" + _readersCount;
                    //var reader = new Reader(readerName, _logger, _sourceFileNameProvider.GetFileName(), part, _nextQueue);
                    //_logger.Add($"Поток {Thread.CurrentThread.Name} отдал part {part} reader`у {readerName}");
                    //reader.Start();


                    _logger.Add($"Поток {Thread.CurrentThread.Name} начал читать");

                    var fsInput = new FileStream(_sourceFileNameProvider.GetFileName(), FileMode.Open, FileAccess.Read);
                    try
                    {
                        if (part.StartPosition != 0)
                            fsInput.Seek(part.StartPosition, SeekOrigin.Begin);

                        stopWatch.Start();
                        part.Source = new Byte[part.SourceSize];
                        var count = fsInput.Read(part.Source, 0, part.SourceSize);

                        stopWatch.Stop();
                        _logger.Add($"Поток {Thread.CurrentThread.Name} прочитал в часть {part} {count} byte за {stopWatch.ElapsedMilliseconds} ms");
                        stopWatch.Reset();
                    }
                    catch (Exception)
                    {
                        _logger.Add($"Поток {Thread.CurrentThread.Name} - ошибка при чтении");
                        throw;
                    }
                    finally
                    {
                        fsInput.Close();
                    }

                    // сам reader помещает part в следующую очередь
                    _nextQueue?.Add(part);
                    //_logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
                }
            }

            _logger.Add($"Поток {Thread.CurrentThread.Name} завершил свой run");
            _logger.Add($"Поток {Thread.CurrentThread.Name} прочитано {_readersCount} частей");
            _logger.Add($"Поток {Thread.CurrentThread.Name} общее время ожидания поступления частей {_totalWait} ms");
        }
    }
}
