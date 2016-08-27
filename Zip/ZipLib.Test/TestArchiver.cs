using System.IO;
using System.IO.Compression;
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
            //var archiver = new Archiver();

            // execute
            //archiver.Compress(sourceFileName, compressedFileName);
            //Assert.IsTrue(File.Exists(compressedFileName), "compressed file not found");

            //archiver.Decompress(compressedFileName, decompressedNewFileName);
            //Assert.IsTrue(File.Exists(decompressedNewFileName), "decompressed file not found");
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

        [TestMethod]
        public void TestBigVodeoFile()
        {
            var sourceFileName = "..\\..\\..\\TestData\\bigVideoFile.avi";
            var compressedFileName = "..\\..\\..\\TestData\\bigVideoFile.gz";
            var decompressedNewFileName = "..\\..\\..\\TestData\\bigVideoFileNew.avi";

            TestCompressDecompress(sourceFileName, compressedFileName, decompressedNewFileName);
        }

        [TestMethod]
        public void TestExperiments()
        {
            var sourceFileName = "..\\..\\..\\TestData\\bigVideoFile.avi";
            var targetFileName = "..\\..\\..\\TestData\\bigVideoFile.gz";

            FileInfo fileToCompress = new FileInfo(sourceFileName);
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                using (var compressedFileStream = File.Create(targetFileName))
                {
                    var buffer = new byte[64 * 1024];
                    int bytesRead;
                    while ((bytesRead = originalFileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
                            {
                                compressionStream.Write(buffer, 0, bytesRead);
                                memoryStream.CopyTo(compressedFileStream);
                                //var resultPart = memoryStream.ToArray();
                                //compressedFileStream.Write(resultPart, 0, resultPart.Length);
                            }
                        }
                    }
                }
            }
        }
    }
}
