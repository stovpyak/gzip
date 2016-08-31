using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestArhivePortion
{
    [TestClass]
    public class TestMake
    {
        [TestMethod]
        public void TestOneByte()
        {
            var portion = new ArhivePortion(new BytesBuffer(new byte[] { 1 }, 0, 0));
            Assert.IsFalse(portion.IsEmpty, "IsNotEmpty");
        }
    }
}
