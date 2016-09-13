using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    [TestClass]
    public class OnlyTitle: TestArhivePartReaderBase
    {

        [TestMethod]
        public void TestAllInOnePortion()
        {
            TestOnlyTitle(1000);
        }

        [TestMethod]
        public void TestSplitTitle()
        {
            TestOnlyTitle(5);
        }

        public void TestOnlyTitle(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
                reader.BufferSize = bufferSize;
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsTrue(res, "не удалось проинициализировать часть");
                Assert.IsNotNull(part.Source, "part.Source = null");
                Assert.IsTrue(input.SequenceEqual(part.Source), "неверный part.Source");
                Assert.IsTrue(part.IsLast, "part.IsLast");
            }
            finally
            {
                inputStream.Close();
            }
        }
    }
}
