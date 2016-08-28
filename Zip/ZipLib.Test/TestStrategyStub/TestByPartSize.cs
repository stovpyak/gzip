using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Strategies;

namespace ZipLib.Test.TestStrategyStub
{
    [TestClass]
    public class TestByPartSize
    {
        [TestMethod]
        public void TestOnePart()
        {
            var strategy = StrategyStub.MakeByPartSize(1, 200);
            strategy.StartFile(100);
            Assert.AreEqual(1, strategy.GetPartCount(), "part count");

            var part = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть");
            Assert.AreEqual(0, part.StartPosition, "start position");
            Assert.AreEqual(100, part.SourceSize, "size");
        }

        [TestMethod]
        public void TestTwoPart()
        {
            var strategy = StrategyStub.MakeByPartSize(1, 50);
            strategy.StartFile(100);
            Assert.AreEqual(2, strategy.GetPartCount(), "part count");

            var part1 = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part1);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть1");
            Assert.AreEqual(0, part1.StartPosition, "part1.StartPosition");
            Assert.AreEqual(50, part1.SourceSize, "part1.SourceSize");

            var part2 = new FilePart("dummyPartName");
            resInit = strategy.InitNextFilePart(part2);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть2");
            Assert.AreEqual(50, part2.StartPosition, "part2.StartPosition");
            Assert.AreEqual(50, part2.SourceSize, "part2.SourceSize");
        }

        [TestMethod]
        public void TestThreeParts()
        {
            var strategy = StrategyStub.MakeByPartSize(1, 34);
            strategy.StartFile(100);
            Assert.AreEqual(3, strategy.GetPartCount(), "part count");

            var part1 = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part1);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть1");
            Assert.AreEqual(0, part1.StartPosition, "part1.StartPosition");
            Assert.AreEqual(34, part1.SourceSize, "part1.SourceSize");

            var part2 = new FilePart("dummyPartName");
            resInit = strategy.InitNextFilePart(part2);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть2");
            Assert.AreEqual(34, part2.StartPosition, "part2.StartPosition");
            Assert.AreEqual(34, part2.SourceSize, "part2.SourceSize");

            var part3 = new FilePart("dummyPartName");
            resInit = strategy.InitNextFilePart(part3);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть3");
            Assert.AreEqual(68, part3.StartPosition, "part3.StartPosition");
            Assert.AreEqual(32, part3.SourceSize, "part3.SourceSize");
        }
    }
}
