using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.Strategies;

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
            var appl = new Appl(logger);

            var source = new FileNameProviderStub(sourceFileName);
            var archive = new FileNameProviderStub(compressedFileName);
            var strategy = new SmartCompressStrategy();
            appl.ExecuteCompress(strategy, source, archive);
            Assert.IsTrue(File.Exists(compressedFileName), "Файл архива не обнаружен после архивации");

            var decompress = new FileNameProviderStub(decompressedNewFileName);
            var decompressStrategy = new DecompressStrategyStub(1);
            appl.ExecuteDecompress(decompressStrategy, archive, decompress);
            Assert.IsTrue(File.Exists(compressedFileName), "Файл разархивированный не обнаружен после разархивации");

            IsFilesEquals(sourceFileName, decompressedNewFileName);
        }

        private const string TestDataFolder = "..\\..\\..\\TestData\\";

        [TestMethod]
        public void TestEmptyFile()
        {
            const string sourceFileName = TestDataFolder + "EmptyFile.txt";
            const string compressedFileName = TestDataFolder + "EmptyFile.gz";
            const string decompressedNewFileName = TestDataFolder + "EmptyFileNew.txt";

            TestCompressDecompressCheck(sourceFileName, compressedFileName, decompressedNewFileName);
        }

        [TestMethod]
        public void Test1Byte()
        {
            const string sourceFileName = TestDataFolder + "data_1byte.txt";
            const string compressedFileName = TestDataFolder + "data_1byte.gz";
            const string decompressedNewFileName = TestDataFolder + "data_1byte_new.txt";

            TestCompressDecompressCheck(sourceFileName, compressedFileName, decompressedNewFileName);
         }

        [TestMethod]
        public void Test10Byte()
        {
            const string sourceFileName = TestDataFolder + "data_10byte.txt";
            const string compressedFileName = TestDataFolder + "data_10byte.gz";
            const string decompressedNewFileName = TestDataFolder + "data_10byte_new.txt";

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
