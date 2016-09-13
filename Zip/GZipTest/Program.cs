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
            var fileLog = new FileLogger("GZipTest.log");
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

                var systemInfoProvider = new SystemInfoProvider();
                var appl = new Appl(logger, systemInfoProvider);
                switch (param.ApplMode)
                {
                    case ApplMode.Compress:
                        var compressStrategy = new SmartCompressStrategy(systemInfoProvider);
                        appl.ExecuteCompress(compressStrategy, _sourceFileNameProvider, _targetFileNameProvider);
                        break;
                    case  ApplMode.Decompress:
                        var decompressStrategy = new SmartDecompressStrategy(systemInfoProvider);
                        appl.ExecuteDecompress(decompressStrategy, _sourceFileNameProvider, _targetFileNameProvider);
                        break;
                }
                return 0;
            }
            catch (Exception ex)
            {
                logger.Add("Произошла ошибка. Выполнение программы будет завершено\r\n" + 
                    ex.Message + "\r\n" + ex.StackTrace);

                AddInnerExceptionToLog(ex, logger);

                return 1;
            }
            finally
            {
                logger.Close();
            }
        }

        private static void AddInnerExceptionToLog(Exception ex, ILogger logger)
        {
            if (ex.InnerException != null)
            {
                logger.Add("Произошла ошибка.\r\n" +
                           ex.InnerException.Message + "\r\n" + ex.InnerException.StackTrace);
                AddInnerExceptionToLog(ex.InnerException, logger);
            }
        }
    }
}
