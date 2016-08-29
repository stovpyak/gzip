using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Strategies;

namespace ZipLib.Test.TestStrategyStub
{
    [TestClass]
    public class TestByPartCount
    {
        [TestMethod]
        public void TestOnePart()
        {
            var strategy = StrategyStub.MakeByPartCount(1, 1);
            strategy.StartFile(100);
            Assert.AreEqual(1, strategy.GetPartCount(), "part count");

            var part = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть");
            Assert.AreEqual(100, part.SourceSize, "size");
        }

        [TestMethod]
        public void TestTwoParts()
        {
            var strategy = StrategyStub.MakeByPartCount(1, 2);
            strategy.StartFile(100);
            Assert.AreEqual(2, strategy.GetPartCount(), "part count");

            var part1 = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part1);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть1");
            Assert.AreEqual(50, part1.SourceSize, "part1.SourceSize");

            var part2 = new FilePart("dummyPartName");
            resInit = strategy.InitNextFilePart(part2);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть2");
            Assert.AreEqual(50, part2.SourceSize, "part2.SourceSize");
        }

        [TestMethod]
        public void TestThreeParts()
        {
            var strategy = StrategyStub.MakeByPartCount(1, 3);
            strategy.StartFile(100);
            Assert.AreEqual(3, strategy.GetPartCount(), "part count");

            var part1 = new FilePart("dummyPartName");
            var resInit = strategy.InitNextFilePart(part1);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть1");
            Assert.AreEqual(34, part1.SourceSize, "part1.SourceSize");

            var part2 = new FilePart("dummyPartName");
            resInit = strategy.InitNextFilePart(part2);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть2");
            Assert.AreEqual(34, part2.SourceSize, "part2.SourceSize");

            var part3 = new FilePart("dummyPartName");
            resInit = strategy.InitNextFilePart(part3);
            Assert.IsTrue(resInit, "не удалось проиницализировать часть3");
            Assert.AreEqual(32, part3.SourceSize, "part3.SourceSize");
        }
    }
}
