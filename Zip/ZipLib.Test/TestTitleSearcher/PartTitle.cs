using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestTitleSearcher
{
    [TestClass]
    public class PartTitle: TestTitleSercherBase
    {
        [TestMethod]
        public void FirstOfTitle()
        {
            byte[] input = { 1, 31 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.PartTitle, 1);
        }

        [TestMethod]
        public void TwoByteOfTitle()
        {
            byte[] input = { 1, 2, 31, 139, 31, 139 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.PartTitle, 4);
        }

        [TestMethod]
        public void OnlyPartTitle()
        {
            byte[] input = { 31, 139, 8, 0 };
            CheckSearchResult(TitleSearcher.GetTitlesInfo(input), TitleMode.PartTitle, 0);
        }

    }
}
