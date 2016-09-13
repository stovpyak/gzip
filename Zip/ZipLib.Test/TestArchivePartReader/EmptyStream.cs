using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    [TestClass]
    public class EmptyStream: TestArhivePartReaderBase
    {
        [TestMethod]
        public void TestEmptyStream()
        {
            var input = new byte[] {};
            var inputStream = new MemoryStream();
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, 0);
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsTrue(res, "не удалось проинициализировать part из пустого потока");
                Assert.IsNotNull(part.Source, "у непроинициализированной части source должен быть Null");
                Assert.IsTrue(part.Source.SequenceEqual(input), "part.Source");
                Assert.IsTrue(part.IsLast, "part.IsLast");
            }
            finally
            {
                inputStream.Close();
            }
        }

    
    }
}
