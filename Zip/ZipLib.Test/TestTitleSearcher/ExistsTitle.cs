using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestTitleSearcher
{
    [TestClass]
    public class ExistsTitle : TestTitleSercherBase
    {
        [TestMethod]
        public void OnlyTitle()
        {
            byte[] input = { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.AllTitle, 0);
        }

        [TestMethod]
        public void TitleAfterOneByte()
        {
            byte[] input = { 3, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.AllTitle, 1);
        }

        [TestMethod]
        public void TitleAfterTwoByte()
        {
            byte[] input = { 31, 31, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.AllTitle, 2);
        }


        [TestMethod]
        public void TitleBeforeOneByte()
        {
            byte[] input = { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.AllTitle, 0);
        }
    }
}
