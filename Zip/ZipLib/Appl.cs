using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading;
using ZipLib.QueueHandlers;
using ZipLib.Strategies;

namespace ZipLib
{
    public class Appl
    {
        private readonly IStrategy _strategy;
        private readonly IFileNameProvider _sourceFileNameProvider;
        private readonly IFileNameProvider _targetFileNameProvider;

        private readonly PartQueue _queueEmpty = new PartQueue("Empty");
        private readonly PartQueue _queueForReaders = new PartQueue("ForReaders");
        private readonly PartQueue _queueForArchivers = new PartQueue("ForArchivers");
        private readonly PartQueue _queueForWriter = new PartQueue("ForWriter");

        public Appl(IStrategy strategy, IFileNameProvider sourceFileNameProvider, IFileNameProvider targetFileNameProvider)
        {
            _strategy = strategy;
            _sourceFileNameProvider = sourceFileNameProvider;
            _targetFileNameProvider = targetFileNameProvider;
        }

        private ThreadStop _stopWriter;
        private ThreadStop _stopArchiversRuner;
        private ThreadStop _stopReadersRuner;
        private ThreadStop _stopPartInitializer;

        public void Run()
        {
            Thread.CurrentThread.Name = "Main";

            var sourceFileName = _sourceFileNameProvider.GetFileName();
            var sourceFileInfo = new FileInfo(sourceFileName);

            _stopWriter = new ThreadStop();
            var writer = new Writer(_stopWriter, _targetFileNameProvider.GetFileName(), _queueForWriter, _queueEmpty);

            _stopArchiversRuner = new ThreadStop();
            var archiversRuner = new ArchiversRuner(_stopArchiversRuner, _queueForArchivers, _queueForWriter);

            _stopReadersRuner = new ThreadStop();
            var readersRuner = new ReadersRuner(_stopReadersRuner, _sourceFileNameProvider, _queueForReaders, _queueForArchivers);

            _stopPartInitializer = new ThreadStop();
            var partInitializer = new PartInitializer(_stopPartInitializer, _strategy, _queueEmpty, _queueForReaders);

            _strategy.StartFile(sourceFileInfo.Length);
            var maxActivePartCount = _strategy.GetMaxActivePartCount();
            for (int i = 0; i < maxActivePartCount; i++)
            {
                var part = new FilePart {Name = i.ToString()};
                _queueEmpty.Enqueue(part);
            }
        }

        public void ShowInfo()
        {
            Console.WriteLine($"очередь {_queueEmpty.Name} Count {_queueEmpty.Count}");
            Console.WriteLine($"очередь {_queueForReaders.Name} Count {_queueForReaders.Count}");
            Console.WriteLine($"очередь {_queueForArchivers.Name} Count {_queueForArchivers.Count}");
            Console.WriteLine($"очередь {_queueForWriter.Name} Count {_queueForWriter.Count}");
        }

        public void Stop()
        {
            _stopWriter.IsNeedStop = true;
            _stopArchiversRuner.IsNeedStop = true;
            _stopReadersRuner.IsNeedStop = true;
            _stopPartInitializer.IsNeedStop = true;

            _queueEmpty.Unlock();
            _queueForReaders.Unlock();
            _queueForArchivers.Unlock();
            _queueForWriter.Unlock();
        }
    }
}
