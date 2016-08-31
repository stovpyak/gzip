using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;

namespace ZipLib.Test
{
    [TestClass]
    public class TestArhivePartReader
    {
        [TestMethod]
        public void TestEmptyStream()
        {
            var inputStream = new MemoryStream();
            try
            {
                var reader = new ArhivePartReader(inputStream);
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsFalse(res, "удалось проинициализировать part из пустого потока");
                Assert.IsNull(part.Source, "у непроинициализированной части source должен быть Null");
            }
            finally
            {
                inputStream.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "Должно быть исключение FormatException")]
        public void TestWithoutTitle()
        {
            var inputStream = MakeInputStream(new byte[] {12});
            try
            {
                var reader = new ArhivePartReader(inputStream);
                var part = new FilePart("dummyName");
                reader.ReadPart(part);
            }
            finally
            {
                inputStream.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "Должно быть исключение FormatException")]
        public void TestNotFirstTitle()
        {
            var inputStream = MakeInputStream(new byte[] { 1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 });
            try
            {
                var reader = new ArhivePartReader(inputStream);
                var part = new FilePart("dummyName");
                reader.ReadPart(part);
            }
            finally
            {
                inputStream.Close();
            }
        }

        [TestMethod]
        public void TestOnlyTitle()
        {
            var input = new byte[] {31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(inputStream);
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsTrue(res, "не удалось проинициализировать часть");
                Assert.IsNotNull(part.Source, "part.Source = null");
                Assert.IsTrue(input.SequenceEqual(part.Source), "неверный part.Source");
            }
            finally
            {
                inputStream.Close();
            }
        }


        [TestMethod]
        public void TestTitleAndData()
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(inputStream);
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsTrue(res, "не удалось проинициализировать часть");
                Assert.IsNotNull(part.Source, "part.Source = null");
                Assert.IsTrue(input.SequenceEqual(part.Source), "неверный part.Source");
            }
            finally
            {
                inputStream.Close();
            }
        }


        [TestMethod]
        public void TestTitleDataAndSecondTitle()
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            var first = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            var second = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };

            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(inputStream);
                var firstPart = new FilePart("dummyName");
                var res = reader.ReadPart(firstPart);

                Assert.IsTrue(res, "не удалось проинициализировать firstPart");
                Assert.IsNotNull(firstPart.Source, "firstPart.Source = null");
                Assert.IsTrue(first.SequenceEqual(firstPart.Source), "неверный firstPart.Source");

                var secondPart = new FilePart("dummyName");
                res = reader.ReadPart(secondPart);

                Assert.IsTrue(res, "не удалось проинициализировать secondPart");
                Assert.IsNotNull(secondPart.Source, "secondPart.Source = null");
                Assert.IsTrue(second.SequenceEqual(secondPart.Source), "неверный secondPart.Source");
            }
            finally
            {
                inputStream.Close();
            }
        }

        private Stream MakeInputStream(byte[] input)
        {
            var inputStream = new MemoryStream();
            inputStream.Write(input, 0, input.Length);
            inputStream.Seek(0, SeekOrigin.Begin);

            return inputStream;
        }


    }
}
