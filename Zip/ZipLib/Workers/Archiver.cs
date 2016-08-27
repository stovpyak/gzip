using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace ZipLib.Workers
{
    public class Archiver
    {
        private readonly FilePart _part;
        private readonly Thread _thread;
        private readonly PartQueue _nextQueue;

        public Archiver(string name, FilePart part, PartQueue nextQueue)
        {
            _part = part;
            _nextQueue = nextQueue;

            _thread = new Thread(this.Run) { Name = name };
        }

        public void Start()
        {
            _thread.Start();
        }

        private void Run()
        {
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} начал архивировать");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Compress();

            stopWatch.Stop();
            Console.WriteLine($"Поток {Thread.CurrentThread.Name} закончил архивировать part {_part.Name} за {stopWatch.ElapsedMilliseconds} ms");

            _nextQueue?.Enqueue(_part);
        }


        public void Compress()
        {
            var memoryStream = new MemoryStream();
            using (GZipStream gzip = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                gzip.Write(_part.Source, 0 , _part.Source.Length);
            }
            _part.Source = null;
            _part.Result = memoryStream.ToArray();
        }

        public void Compress(string sourceFileName, string targetFileName)
        {
            FileInfo fileToCompress = new FileInfo(sourceFileName);

            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                using (FileStream compressedFileStream = File.Create(targetFileName))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                        CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public void Decompress(string sourceFileName, string targetFileName)
        {
            var fileToDecompress = new FileInfo(sourceFileName);
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                using (FileStream decompressedFileStream = File.Create(targetFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }
    }
}
