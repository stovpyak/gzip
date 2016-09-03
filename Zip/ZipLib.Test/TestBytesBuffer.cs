using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test
{
    [TestClass]
    public class TestBytesBuffer
    {
        [TestMethod]
        public void TestOneByte()
        {
            var buffer = new BytesBuffer(new byte[] { 1 }, 0, 0);

            Assert.AreEqual(0, buffer.StartPosition, "StartPosition");
            Assert.AreEqual(0, buffer.EndPosition, "EndPosition");
            Assert.AreEqual(1, buffer.Size, "Size");
        }

    }
}
