using System;
using ZipLib;
using ZipLib.Loggers;
using ZipLib.Strategies;

namespace GZipTest
{
    // todo
    // 
    //

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
                
                var appl = new Appl(logger);
                switch (param.ApplMode)
                {
                    case ApplMode.Compress:
                        var compressStrategy = new SmartCompressStrategy();
                        //var compressStrategy = CompressStrategyStub.MakeByPartSize(4, 100 * 1024 * 1024);
                        //var compressStrategy = CompressStrategyStub.MakeByPartCount(5, 130);
                        appl.ExecuteCompress(compressStrategy, _sourceFileNameProvider, _targetFileNameProvider);
                        break;
                    case  ApplMode.Decompress:
                        var decompressStrategy = new DecompressStrategyStub(1);
                        appl.ExecuteDecompress(decompressStrategy, _sourceFileNameProvider, _targetFileNameProvider);
                        break;
                }
                return 0;
            }
            catch (Exception e)
            {
                logger.Add("Возникла ошибка при выполнении программы\r\n" + e.Message);
                return 1;
            }
            finally
            {
                logger.Close();
            }
        }
    }
}
