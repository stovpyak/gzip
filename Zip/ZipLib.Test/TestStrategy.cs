using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Strategies;

namespace ZipLib.Test
{
    [TestClass]
    public class TestStrategy
    {
        [TestMethod]
        public void Test4ProcessorsX86()
        {
            var systemInfoProvider = new SystemInfoProviderStub(4, Convert.ToInt64(1024 * 1024 * 1024 * 1.6));
            var strategy = new SmartCompressStrategy(systemInfoProvider);

            Assert.AreEqual(5, strategy.MaxActivePartCount, "MaxActivePartCount");
            Assert.AreEqual(171798691, strategy.PartSize, "PartSize");
        }

        [TestMethod]
        public void Test4ProcessorsX64_10GB()
        {
            var systemInfoProvider = new SystemInfoProviderStub(4, Convert.ToInt64(1024 * 1024 * 1024 * 10.0));
            var strategy = new SmartCompressStrategy(systemInfoProvider);

            Assert.AreEqual(5, strategy.MaxActivePartCount, "MaxActivePartCount");
            Assert.AreEqual(1073741824, strategy.PartSize, "PartSize");
        }
    }
}
