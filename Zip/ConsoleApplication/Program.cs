using System;
using ZipLib;
using ZipLib.Strategies;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // пробуем читать файл несколькими stream
            //var sourceFileName = "data_g32.txt";
            //var targetFileName = "data_g32.gz";

            var sourceFileName = "data_g1.txt";
            var targetFileName = "data_g1.gz";

            //var sourceFileName = "data_mb100.txt";
            //var targetFileName = "data_mb100.gz";

            //var strategy = new SmartStrategy();
            //var strategy = StrategyStub.MakeByPartSize(4, 100 * 1024 * 1024);
            var strategy = StrategyStub.MakeByPartCount(5, 8);
            var sourceFileNameProfiler = new FileNameProviderStub(sourceFileName);
            var targetFileNameProfiler = new FileNameProviderStub(targetFileName);

            var appl = new Appl(strategy, sourceFileNameProfiler, targetFileNameProfiler);
            appl.Run();

            Console.WriteLine("Для завершения нажмите любую клавишу...");
            Console.ReadLine();
        }
    }
}
