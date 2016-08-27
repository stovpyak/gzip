using System;
using System.Diagnostics;
using System.IO;
using ZipLib;
using ZipLib.Strategies;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // пробуем читать файл несколькими stream
            var sourceFileName = "data_g1.txt";
            var targetFileName = "data_g1.gz";

            //var sourceFileName = "data_mb100.txt";
            //var targetFileName = "data_mb100.gz";

            var sourceFileInfo = new FileInfo(sourceFileName);
            Console.WriteLine("размер файла {0} bite", sourceFileInfo.Length);

            var strategy = new SmartStrategy();
            //var strategy = new StrategyStub(4);
            var sourceFileNameProfiler = new FileNamePrividerStub(sourceFileName);
            var targetFileNameProfiler = new FileNamePrividerStub(targetFileName);

            var appl = new Appl(strategy, sourceFileNameProfiler, targetFileNameProfiler);
            appl.Run();

            var input = Console.ReadLine();
            if (input == "c")
            {
                appl.ShowInfo();
                appl.Stop();
                Console.WriteLine("Для завершения нажмите любую клавишу...");
                Console.ReadLine();
            }
        }
    }
}
