using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    [TestClass]
    public class TitleDataAndSecondTitle: TestArhivePartReaderBase
    {
        [TestMethod]
        public void TestAllInOnePortion()
        {
            TestTitleDataAndSecondTitle(1000);
        }

        [TestMethod]
        public void TestSplitFirstTitle()
        {
            TestTitleDataAndSecondTitle(5);
        }

        public void TestTitleDataAndSecondTitle(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            var first = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            var second = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };

            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
                reader.BufferSize = bufferSize;
                var firstPart = new FilePart("dummyName");
                var res = reader.ReadPart(firstPart);

                Assert.IsTrue(res, "не удалось проинициализировать firstPart");
                Assert.IsNotNull(firstPart.Source, "firstPart.Source = null");
                Assert.IsTrue(first.SequenceEqual(firstPart.Source), "неверный firstPart.Source");
                Assert.IsFalse(firstPart.IsLast, "firstPart.IsLast");

                var secondPart = new FilePart("dummyName");
                res = reader.ReadPart(secondPart);

                Assert.IsTrue(res, "не удалось проинициализировать secondPart");
                Assert.IsNotNull(secondPart.Source, "secondPart.Source = null");
                Assert.IsTrue(second.SequenceEqual(secondPart.Source), "неверный secondPart.Source");
                Assert.IsTrue(secondPart.IsLast, "firstPart.IsLast");
            }
            finally
            {
                inputStream.Close();
            }
        }
    }
}
