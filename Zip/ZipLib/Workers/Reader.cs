using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ZipLib.Workers
{
    public class Reader
    { 
        private readonly string _sourceFileName;
        private readonly FilePart _part;
        private readonly Thread _thread;
        private readonly PartQueue _nextQueue;

        public Reader(string name, string sourceFileName, FilePart part, PartQueue nextQueue)
        {
            _sourceFileName = sourceFileName;
            _part = part;
            _nextQueue = nextQueue;

            _thread = new Thread(this.Run) {Name = name};
        }

        public void Start()
        {
            _thread.Start();
        }

        private void Run()
        {
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} начал читать");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            //var start = DateTime.Now;

            var fsInput = new FileStream(_sourceFileName, FileMode.Open, FileAccess.Read);
            try
            {
                if (_part.StartPosition != 0)
                    fsInput.Seek(_part.StartPosition, SeekOrigin.Begin);

                _part.Source = new Byte[_part.Size];
                var count = fsInput.Read(_part.Source, 0, _part.Size);

                stopWatch.Stop();
                //var finish = DateTime.Now;
                Console.WriteLine("{0} прочитал {1} byte за {2} ms", _thread.Name, count, stopWatch.ElapsedMilliseconds);
                //Console.WriteLine("{0} начал в {1}", _thread.Name, start.Ticks);
                //Console.WriteLine("{0} закончил в {1}", _thread.Name, finish.Ticks);
                //Console.WriteLine("{0} время {1}", _thread.Name, finish - start);
            }
            catch (Exception)
            {
                Console.WriteLine($"Поток {Thread.CurrentThread.Name} - ошибка при чтении");
                throw;
            }
            finally
            {
                fsInput.Close();
            }

            // сам reader помещает part в следующую очередь
            _nextQueue?.Enqueue(_part);
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} завершил свой run");
        }
    }
}
