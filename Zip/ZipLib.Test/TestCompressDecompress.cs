using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.Strategies;
using ZipLib.Workers;

namespace ZipLib.Test
{
    [TestClass]
    public class TestCompressDecompress
    {
        private void TestCompressDecompressCheck(string sourceFileName,
            string compressedFileName, string decompressedNewFileName)
        {
            // prepare
            if (File.Exists(compressedFileName))
                File.Delete(compressedFileName);
            Assert.IsFalse(File.Exists(compressedFileName), "Не удалось удалить архив перед началом теста");

            if (File.Exists(decompressedNewFileName))
                File.Delete(decompressedNewFileName);
            Assert.IsFalse(File.Exists(decompressedNewFileName),
                "Не удалось удалить разархивированный файл перед началом теста");

            // init
            var logger = new LoggerDummy();
            //var strategy = StrategyStub.MakeByPartCount(1, 1);

            var strategy = new SmartStrategy();
            var source = new FileNameProviderStub(sourceFileName);
            var archive = new FileNameProviderStub(compressedFileName);

            var applForCompress = new Appl(logger, strategy, source, archive);
            applForCompress.Execute(ApplMode.Compress);
            Assert.IsTrue(File.Exists(compressedFileName), "Файл архива не обнаружен после архивации");

            var decompress = new FileNameProviderStub(decompressedNewFileName);

            var applForDecompress = new Appl(logger, strategy, archive, decompress);
            applForDecompress.Execute(ApplMode.Decompress);
            Assert.IsTrue(File.Exists(compressedFileName), "Файл разархивированный не обнаружен после разархивации");

            IsFilesEquals(sourceFileName, decompressedNewFileName);
        }


        [TestMethod]
        public void TestEmptyFile()
        {
            var sourceFileName = "..\\..\\..\\TestData\\EmptyFile.txt";
            var compressedFileName = "..\\..\\..\\TestData\\EmptyFile.gz";
            var decompressedNewFileName = "..\\..\\..\\TestData\\EmptyFileNew.txt";

            TestCompressDecompressCheck(sourceFileName, compressedFileName, decompressedNewFileName);
        }

        [TestMethod]
        public void Test1Byte()
        {
            var sourceFileName = "..\\..\\..\\TestData\\data_1byte.txt";
            var compressedFileName = "..\\..\\..\\TestData\\data_1byte.gz";
            var decompressedNewFileName = "..\\..\\..\\TestData\\data_1byte_new.txt";

            TestCompressDecompressCheck(sourceFileName, compressedFileName, decompressedNewFileName);
         }

        private void IsFilesEquals(string firstFileName, string secondFileName)
        {
            var firstMd5 = GetFileMd5(firstFileName);
            var secondMd5 = GetFileMd5(secondFileName);
            var res = firstMd5.SequenceEqual(secondMd5);
            Assert.IsTrue(res, $"Файлы не равны {firstFileName} и {secondFileName}");
        }

        private byte[] GetFileMd5(string fileName)
        {
            byte[] md5;
            using (var firstFileStream = new FileStream(fileName, FileMode.Open))
            {
                var md = MD5.Create();
                md5 = md.ComputeHash(firstFileStream);
            }
            return md5;
        }
    }
}
