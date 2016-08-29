using System;
using ZipLib;
using ZipLib.Loggers;
using ZipLib.Strategies;

namespace GZipTest
{
    class Program
    {
        private static IFileNameProvider _sourceFileNameProvider;
        private static IFileNameProvider _targetFileNameProvider;

        static int Main(string[] args)
        {
            var logger = new CompositeLogger();
            var fileLog = new FileLogger();
            logger.AddChild(fileLog);
            logger.AddChild(new ConsoleLogger());

            try
            {
                var argsParser = new ArgsParser(logger);
                var param = argsParser.ParsParams(args);
                if (param == null)
                    return 1;

                _sourceFileNameProvider = new FileNameProviderStub(param.SourceFileName);
                _targetFileNameProvider = new FileNameProviderStub(param.TargetFileName);

                var strategy = new SmartStrategy();
                //var strategy = StrategyStub.MakeByPartSize(4, 100 * 1024 * 1024);
                //var strategy = StrategyStub.MakeByPartCount(5, 15);
                var appl = new Appl(logger, strategy, _sourceFileNameProvider, _targetFileNameProvider);
                appl.Execute(param.ApplMode);

                return 0;
            }
            catch (Exception e)
            {
                logger.Add("Возникла ошибка при выполнении программы\r\n" + e.Message);
                return 1;
            }
            finally
            {
                fileLog.SaveToFile("GZipTest.log");
            }
        }
    }
}
