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
    public class TestAppend
    {
        [TestMethod]
        public void TestAppendPartTitle()
        {
            var first = new byte[] { 31, 139, 8, 0, 0 };
            var firstBuffer = new BytesBuffer(first, 0, first.Length - 1);
            var portion = new ArhivePortion(firstBuffer);

            var second = new byte[] { 0, 0, 0, 4, 0, 1 };
            var secondBuffer = new BytesBuffer(second, 0, second.Length - 1);
            portion.Append(secondBuffer);

            Assert.IsTrue(portion.IsExistsAllTitle, "порция должна содержать title");
            var titleAndData = portion.ExtractFirstTitleAndData();

            Assert.AreEqual(0, titleAndData.StartPosition, "titleAndData.StartPosition");
            Assert.AreEqual(11, titleAndData.Size, "titleAndData.StartPosition");
            Assert.IsTrue(titleAndData.InnerBuffer.SequenceEqual(new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 }), 
                "неверный titleAndData");

        }
    }
}
