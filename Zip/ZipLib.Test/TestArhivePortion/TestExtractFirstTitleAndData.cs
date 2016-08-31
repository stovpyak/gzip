using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestArhivePortion
{
    [TestClass]
    public class TestExtractFirstTitleAndData
    {
        [TestMethod]
        public void TestOneTitleAndData()
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            var buffer = new BytesBuffer(input, 0, input.Length - 1);
            var portion = new ArhivePortion(buffer);
            var extracted = portion.ExtractFirstTitleAndData();

            Assert.IsNotNull(extracted, "extracted == null");
            Assert.AreEqual(0, extracted.StartPosition, "extracted.StartPosition");
            Assert.AreEqual(10, extracted.EndPosition, "extracted.EndPosition");
            Assert.AreEqual(11, extracted.Size, "extracted.Size");

            Assert.IsTrue(portion.IsEmpty, "portion.IsEmpty");
            Assert.IsFalse(portion.IsExistsTitle, "portion.IsExistsTitle");
        }

        [TestMethod]
        public void TestTwoTitles()
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            var buffer = new BytesBuffer(input, 0, input.Length - 1);
            var portion = new ArhivePortion(buffer);
            var firstExtracted = portion.ExtractFirstTitleAndData();

            Assert.IsNotNull(firstExtracted, "firstExtracted == null");
            Assert.AreEqual(0, firstExtracted.StartPosition, "firstExtracted.StartPosition");
            Assert.AreEqual(10, firstExtracted.EndPosition, "firstExtracted.EndPosition");
            Assert.AreEqual(11, firstExtracted.Size, "firstExtracted.Size");

            Assert.IsFalse(portion.IsEmpty, "portion.IsEmpty");
            Assert.IsTrue(portion.IsExistsTitle, "portion.IsExistsTitle");

            var secondExtracted = portion.ExtractFirstTitleAndData();

            Assert.IsNotNull(secondExtracted, "secondExtracted == null");
            Assert.AreEqual(11, secondExtracted.StartPosition, "secondExtracted.StartPosition");
            Assert.AreEqual(20, secondExtracted.EndPosition, "secondExtracted.EndPosition");
            Assert.AreEqual(10, secondExtracted.Size, "secondExtracted.Size");

            Assert.IsTrue(portion.IsEmpty, "portion.IsEmpty");
            Assert.IsFalse(portion.IsExistsTitle, "portion.IsExistsTitle");

        }
    }
}
