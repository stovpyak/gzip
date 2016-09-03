using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Strategies;

namespace ZipLib.Test
{
    [TestClass]
    public class TestCompressStrategyStub
    {
        [TestMethod]
        public void TestOnePart()
        {
            var strategy = new CompressStrategyStub(1, 200);
            Assert.AreEqual(1, strategy.MaxActivePartCount, "MaxActivePartCount");
            Assert.AreEqual(200, strategy.PartSize, "PartSize");
        }
    }
}