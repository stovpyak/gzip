using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    /// <summary>
    /// Случай, когда в данных есть часть заголовка, но это не заголовок
    /// </summary>
    [TestClass]
    public class OnePartDataWithPartTitle: TestArhivePartReaderBase
    {
        /// <summary>
        /// Вся часть попадает в одну порцию
        /// </summary>
        [TestMethod]
        public void TestAllInOnePortion()
        {
            TestDataWithPartTitle(1000);
        }


        /// <summary>
        /// В первую порцию порцию попал первый заголовой, даные и часть похожая на второй заголовок
        /// вторая порция должна показать, что второй - это не заголовок
        /// </summary>
        [TestMethod]
        public void TestPartSecondTitleInFirstPortion()
        {
            TestDataWithPartTitle(24);
        }

        /// <summary>
        /// В первую порцию порцию попал первый заголовой, и часть даных (2 byte)
        /// Во вторую порцию часть данных (10 byte) и часть второго заголовка (2 byte)
        /// Третья часть должна показать, что второй заголовок - то не заголовок
        /// </summary>
        [TestMethod]
        public void TestPartSecondTitleInSecondPortion()
        {
            TestDataWithPartTitle(12);
        }


        public void TestDataWithPartTitle(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 31, 139, 13, 14, 15 };
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
                reader.BufferSize = bufferSize;
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsTrue(res, "не удалось проинициализировать часть");
                Assert.IsNotNull(part.Source, "part.Source = null");
                Assert.IsTrue(input.SequenceEqual(part.Source), "неверный part.Source");
                Assert.IsTrue(part.IsLast, "part.IsLast");
            }
            finally
            {
                inputStream.Close();
            }
        }
    }
}
