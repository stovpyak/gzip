using System;
using ZipLib;
using ZipLib.Loggers;
using ZipLib.Strategies;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // пробуем читать файл несколькими stream
            var sourceFileName = "Model_837330.xml";
            var targetFileName = "Model_837330.gz";

            //var sourceFileName = "data_g1.txt";
            //var targetFileName = "data_g1.gz";

            //var sourceFileName = "Visual_Studio_2015.ISO";
            //var targetFileName = "Visual_Studio_2015.gz";

            //var sourceFileName = "data_mb100.txt";
            //var targetFileName = "data_mb100.gz";

            //var strategy = new SmartStrategy();
            //var strategy = StrategyStub.MakeByPartSize(4, 100 * 1024 * 1024);
            var strategy = StrategyStub.MakeByPartCount(5, 15);
            var sourceFileNameProfiler = new FileNameProviderStub(sourceFileName);
            var targetFileNameProfiler = new FileNameProviderStub(targetFileName);

            var logger = new LoggerStringList();

            var appl = new Appl(logger, strategy, sourceFileNameProfiler, targetFileNameProfiler);
            appl.Run();

            Console.WriteLine("Для завершения нажмите любую клавишу...");
            Console.ReadLine();
        }
    }
}
