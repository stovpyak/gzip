using System.Runtime.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestTitleSearcher
{
    [TestClass]
    public class TestNotTitle: TestTitleSercherBase
    {
        [TestMethod]
        public void TestEmptyInput()
        {
            byte[] input = {};
            Assert.IsNull(TitleSearcher.GetTitlesInfo(input));
        }

        [TestMethod]
        public void TestOneByte()
        {
            byte[] input = { 1 };
            Assert.IsNull(TitleSearcher.GetTitlesInfo(input));
        }

        [TestMethod]
        public void TestTwoBytes()
        {
            byte[] input = { 1, 3 };
            Assert.IsNull(TitleSearcher.GetTitlesInfo(input));
        }

        [TestMethod]
        public void TestBegin1Title()
        {
            byte[] input = { 31, 1 };
            Assert.IsNull(TitleSearcher.GetTitlesInfo(input));
        }

        [TestMethod]
        public void TestBegin2Title()
        {
            byte[] input = { 31, 139, 1 };
            Assert.IsNull(TitleSearcher.GetTitlesInfo(input));
        }

        [TestMethod]
        public void TestBegin9Title()
        {
            byte[] input = { 31, 139, 8, 0, 0, 0, 0, 0, 4, 1 };
            Assert.IsNull(TitleSearcher.GetTitlesInfo(input));
        }
    }
}
