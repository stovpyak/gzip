using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Strategies;

namespace ZipLib.Test
{
    [TestClass]
    public class TestStrategy
    {
        [TestMethod]
        public void TestOnePart()
        {
            var strategy = new SmartStrategy();
            strategy.StartFile(100);
            Assert.AreEqual(1, strategy.GetPartCount(), "part count");

            var part = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть");
            Assert.AreEqual(100, part.SourceSize, "size");
        }
    }
}
