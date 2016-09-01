using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Decompress;
using ZipLib.Loggers;

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
                var reader = new ArhivePartReader(new LoggerDummy(), inputStream, 0);
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
        [ExpectedException(typeof(FormatException), 
            "Должно быть исключение FormatException, так как нет title")]
        public void TestWithoutTitle()
        {
            var input = new byte[] {12};
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(new LoggerDummy(), inputStream, input.Length);
                var part = new FilePart("dummyName");
                reader.ReadPart(part);
            }
            finally
            {
                inputStream.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), 
            "Должно быть исключение FormatException, так как поток начинается не с title")]
        public void TestNotFirstTitle()
        {
            var input = new byte[] {1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(new LoggerDummy(), inputStream, input.Length);
                var part = new FilePart("dummyName");
                reader.ReadPart(part);
            }
            finally
            {
                inputStream.Close();
            }
        }

        [TestMethod]
        public void TestOnlyTitleBigBuffer()
        {
            TestOnlyTitle(1000);
        }

        [TestMethod]
        public void TestOnlyTitleSmallBuffer()
        {
            TestOnlyTitle(5);
        }

        public void TestOnlyTitle(int bufferSize)
        {
            var input = new byte[] {31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(new LoggerDummy(), inputStream, input.Length);
                reader.BufferSize = bufferSize;
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
        public void TestTitleAndDataBigBuffer()
        {
            TestOnlyTitle(1000);
        }

        [TestMethod]
        public void TestTitleAndDataSmallBuffer()
        {
            TestOnlyTitle(5);
        }


        public void TestTitleAndData(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(new LoggerDummy(), inputStream, input.Length);
                reader.BufferSize = bufferSize;
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
        public void TestTitleDataAndSecondTitleBigBuffer()
        {
            TestOnlyTitle(1000);
        }

        [TestMethod]
        public void TestTitleDataAndSecondTitleSmallBuffer()
        {
            TestOnlyTitle(5);
        }

        public void TestTitleDataAndSecondTitle(int bufferSize)
        {
            var input = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1, 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
            var first = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 1 };
            var second = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };

            var inputStream = MakeInputStream(input);
            try
            {
                var reader = new ArhivePartReader(new LoggerDummy(), inputStream, input.Length);
                reader.BufferSize = bufferSize;
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
