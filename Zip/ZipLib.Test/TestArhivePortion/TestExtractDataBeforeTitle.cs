using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestArhivePortion
{
    [TestClass]
    public class TestExtractDataBeforeTitle
    {
        [TestMethod]
        public void DataBeforePartTitle()
        {
            var input = new byte[] { 1, 31, 139, 8, 0, 0, 0 };
            var buffer = new BytesBuffer(input, 0, input.Length - 1);
            var portion = new ArhivePortion(buffer);
            var extracted = portion.ExtractDataBeforeTitle();

            Assert.IsNotNull(extracted, "extracted == null");
            Assert.AreEqual(0, extracted.StartPosition, "extracted.StartPosition");
            Assert.AreEqual(0, extracted.EndPosition, "extracted.EndPosition");
            Assert.AreEqual(1, extracted.Size, "extracted.Size");

            Assert.IsTrue(portion.IsNotEmpty, "portion.IsNotEmpty");
            Assert.IsTrue(portion.IsExistsTitle, "portion.IsExistsTitle");
        }
    }
}
