using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    [TestClass]
    public class TwoPart: TestArhivePartReaderBase
    {

        /// <summary>
        /// Все части попадают в одну порцию
        /// </summary>
        [TestMethod]
        public void TestAllInOnePortion()
        {
            TestTwoParts(1000);
        }

        /// <summary>
        /// Перавя часть в первую порцию
        /// Вторая часть во вторую порцию
        /// </summary>
        [TestMethod]
        public void TestFirstPartInFirstPortion()
        {
            TestTwoParts(14);
        }

        /// <summary>
        /// В первую порцию попали - первый заголовок, данные, и часть второго заголовка 
        /// Во вторую порцию часть второго заголовка и данные
        /// </summary>
        [TestMethod]
        public void TestSplitSecondTitle()
        {
            TestTwoParts(20);
        }

        /// <summary>
        /// В первую порцию попали - первый заголовок, и часть данных
        /// Во вторую порцию - часть данных, второй заголовк и данные
        /// </summary>
        [TestMethod]
        public void TestSplitFirstData()
        {
            TestTwoParts(13);
        }

        /// <summary>
        /// В первую порцию попали - первый заголовок, данные и второй заголовок
        /// Во вторую порцию - вторые данные
        /// </summary>
        [TestMethod]
        public void TestFirstPartAndSecondTitle()
        {
            TestTwoParts(24);
        }

        /// <summary>
        /// В первую порцию попали - первый заголовок, данные и второй заголовок и часть данных
        /// Во вторую порцию - часть вторых данных
        /// </summary>
        [TestMethod]
        public void TestSplitSecondData()
        {
            TestTwoParts(25);
        }

        public void TestTwoParts(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 2, 3, 4, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 5, 6 };
            var first = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 2, 3, 4 };
            var second = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 5, 6 };

            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
                reader.BufferSize = bufferSize;
                var firstPart = new FilePart("dummyName");
                var res = reader.ReadPart(firstPart);

                Assert.IsTrue(res, "не удалось проинициализировать firstPart");
                Assert.IsNotNull(firstPart.Source, "firstPart.Source = null");
                Assert.IsTrue(first.SequenceEqual(firstPart.Source), "неверный firstPart.Source");
                Assert.IsFalse(firstPart.IsLast, "firstPart.IsLast");

                var secondPart = new FilePart("dummyName");
                res = reader.ReadPart(secondPart);

                Assert.IsTrue(res, "не удалось проинициализировать secondPart");
                Assert.IsNotNull(secondPart.Source, "secondPart.Source = null");
                Assert.IsTrue(second.SequenceEqual(secondPart.Source), "неверный secondPart.Source");
                Assert.IsTrue(secondPart.IsLast, "secondPart.IsLast");
            }
            finally
            {
                inputStream.Close();
            }
        }
    }
}
