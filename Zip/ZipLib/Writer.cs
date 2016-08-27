using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ZipLib
{
    public class Writer
    {
        private readonly ThreadStop _threadStop;
        private readonly PartQueue _sourceQueue;
        private readonly PartQueue _nextQueue;
        private readonly string _targetFileName;

        public Writer(ThreadStop threadStop, string targetFileName, PartQueue sourceQueue, PartQueue nextQueue)
        {
            _threadStop = threadStop;
            _sourceQueue = sourceQueue;
            _nextQueue = nextQueue;
            _targetFileName = targetFileName;

            var thread = new Thread(this.Run) { Name = "Writer" };
            thread.Start();
        }

        private Stream _targetStream;

        private Stream GetOrMakeStream()
        {
            if (_targetStream == null)
            {
                _targetStream = File.Create(_targetFileName);
            }
            return _targetStream;
        }

        private void Run()
        {
            while (!_threadStop.IsNeedStop)
            {
                // для writera важен порядок/очередность частей!
                var part = _sourceQueue.GetPart();
                if (part != null)
                {
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} получил part {part.Name}");
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    var target = GetOrMakeStream();
                    target.Write(part.Result, 0, part.Result.Length);

                    stopWatch.Stop();
                    Console.WriteLine($"Поток {Thread.CurrentThread.Name} записал part {part.Name} за {stopWatch.ElapsedMilliseconds} ms");

                    part.Result = null;
                    _nextQueue.Enqueue(part);
                }
            }
            var target2 = GetOrMakeStream();
            target2.Flush();
            target2.Close();

            Console.WriteLine($"Поток {Thread.CurrentThread.Name} завершил свой run");
        }
    }
}
