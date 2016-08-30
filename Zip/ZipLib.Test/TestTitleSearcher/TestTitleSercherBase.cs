using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test.TestTitleSearcher
{
    public abstract class TestTitleSercherBase
    {
        protected void CheckSearchResult(List<TitleSearchResult> res, TitleMode mode, int index)
        {
            Assert.AreEqual(1, res.Count, "res.Count");
            Assert.AreEqual(mode, res[0].Mode, "Mode");
            Assert.AreEqual(index, res[0].IndexStartTitle, "IndexStartTitle");
        }
    }
}
