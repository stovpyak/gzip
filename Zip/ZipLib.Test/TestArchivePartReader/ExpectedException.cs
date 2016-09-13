using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    [TestClass]
    public class ExpectedException: TestArhivePartReaderBase
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException),
        "Должно быть исключение FormatException, так как нет title")]
        public void TestWithoutTitle()
        {
            var input = new byte[] { 12 };
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
                var part = new FilePart("dummyName");
                reader.ReadPart(part);
            }
            finally
            {
                inputStream.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException),
            "Должно быть исключение FormatException, так как поток начинается не с title")]
        public void TestNotFirstTitle()
        {
            var input = new byte[] { 1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
                var part = new FilePart("dummyName");
                reader.ReadPart(part);
            }
            finally
            {
                inputStream.Close();
            }
        }
    }
}
