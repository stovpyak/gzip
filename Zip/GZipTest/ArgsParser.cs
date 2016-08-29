using System;
using ZipLib;
using ZipLib.Loggers;

namespace GZipTest
{
    public class ArgsParser
    {
        private readonly ILogger _logger;

        public ArgsParser(ILogger logger)
        {
            _logger = logger;
        }

        public ApplParams ParsParams(string[] args)
        {
            var mode = GetMode(args);
            if (mode == null)
                return null;

            string sourceFileName;
            string targetFileName;
            if (!InitFileNames(mode.Value, args, out sourceFileName, out targetFileName))
                return null;

            var param = new ApplParams
            {
                ApplMode = mode.Value,
                SourceFileName = sourceFileName,
                TargetFileName = targetFileName
            };
            return param;
        }

        private ApplMode? GetMode(string[] args)
        {
            if (args == null)
                return null;

            if (args.Length < 1)
            {
                _logger.Add("Не задан режим работы compress/decompress");
                return null;
            }
            var argModeValue = args[0];
            switch (argModeValue)
            {
                case "compress":
                    return ApplMode.Compress;
                case "decompress":
                    return ApplMode.Decompress;
            }
            _logger.Add("Не задан режим работы compress/decompress");
            return null;
        }

        private bool InitFileNames(ApplMode applMode, string[] args, out string sourceFileName, out string targetFileName)
        {
            sourceFileName = "";
            targetFileName = "";

            string sourceFileCaption;
            string targetFileCaption;
            switch (applMode)
            {
                case ApplMode.Compress:
                    {
                        sourceFileCaption = "исходный файл";
                        targetFileCaption = "архив";
                        break;
                    }
                case ApplMode.Decompress:
                    {
                        sourceFileCaption = "архив";
                        targetFileCaption = "распакованный файл";
                        break;
                    }
                default:
                    throw new Exception("Неизвестное значение ApplMode");
            }

            if (args.Length < 2)
            {
                _logger.Add($"Не задан {sourceFileCaption}");
                return false;
            }
            sourceFileName = args[1];
            _logger.Add($"{sourceFileCaption} {sourceFileName}");

            if (args.Length < 3)
            {
                _logger.Add($"Не задан {targetFileCaption}");
                return false;
            }
            targetFileName = args[2];
            _logger.Add($"{targetFileCaption} {targetFileName}");
            return true;
        }
    }
 
}
