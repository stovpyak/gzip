using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZipLib.Test
{
    [TestClass]
    public class TestArchiver
    {
        private void TestCompressDecompress(string sourceFileName, 
            string compressedFileName, string decompressedNewFileName)
        {
            // prepare
            if (File.Exists(compressedFileName))
                File.Delete(compressedFileName);
            if (File.Exists(decompressedNewFileName))
                File.Delete(decompressedNewFileName);

            // init
            var archiver = new Archiver();

            // execute
            archiver.Compress(sourceFileName, compressedFileName);
            Assert.IsTrue(File.Exists(compressedFileName), "compressed file not found");

            archiver.Decompress(compressedFileName, decompressedNewFileName);
            Assert.IsTrue(File.Exists(decompressedNewFileName), "decompressed file not found");
        }


        [TestMethod]
        public void TestEmptyFile()
        {
            var sourceFileName = "..\\..\\..\\TestData\\EmptyFile.txt";
            var compressedFileName = "..\\..\\..\\TestData\\EmptyFile.gz";
            var decompressedNewFileName = "..\\..\\..\\TestData\\EmptyFileNew.txt";

            TestCompressDecompress(sourceFileName, compressedFileName, decompressedNewFileName);
        }

        [TestMethod]
        public void TestSimple()
        {
            var sourceFileName = "..\\..\\..\\TestData\\FileToCompress.txt";
            var compressedFileName = "..\\..\\..\\TestData\\FileToCompress.gz";
            var decompressedNewFileName = "..\\..\\..\\TestData\\FileToCompressNew.txt";

            TestCompressDecompress(sourceFileName, compressedFileName, decompressedNewFileName);
         }
    }
}
