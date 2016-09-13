using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test.TestArchivePartReader
{
    [TestClass]
    public class OnePart: TestArhivePartReaderBase
    {
        /// <summary>
        /// Вся часть попадает в одну порцию
        /// </summary>
        [TestMethod]
        public void TestAllInOnePortion()
        {
            TestTitleAndData(1000);
        }

        /// <summary>
        /// В первую порцию - заголовок и часть данных
        /// Во вторую порцию - остаток данных
        /// </summary>
        [TestMethod]
        public void TestSplitData()
        {
            TestTitleAndData(12);
        }

        /// <summary>
        /// В первую порцию - часть заголовка
        /// Во вторую порцию - остаток заголовка и данные
        /// </summary>
        [TestMethod]
        public void TestSplitTitleAndData()
        {
            TestTitleAndData(7);
        }

        /// <summary>
        /// В первую порцию - только заголовок
        /// Во вторую порцию - данные
        /// </summary>
        [TestMethod]
        public void TestTitleInFirstPortion()
        {
            TestTitleAndData(10);
        }

        public void TestTitleAndData(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 2, 3, 4 };
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
