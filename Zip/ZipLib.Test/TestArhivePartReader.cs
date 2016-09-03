using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib.Loggers;
using ZipLib.QueueHandlers.Readers;

namespace ZipLib.Test
{
    [TestClass]
    public class TestArhivePartReader
    {
        [TestMethod]
        public void TestEmptyStream()
        {
            var input = new byte[] {};
            var inputStream = new MemoryStream();
            try
            {
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, 0);
                var part = new FilePart("dummyName");
                var res = reader.ReadPart(part);

                Assert.IsTrue(res, "не удалось проинициализировать part из пустого потока");
                Assert.IsNotNull(part.Source, "у непроинициализированной части source должен быть Null");
                Assert.IsTrue(part.Source.SequenceEqual(input), "part.Source");
                Assert.IsTrue(part.IsLast, "part.IsLast");
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
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
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
                var reader = new ArсhivePartReader(new LoggerDummy());
                reader.Init(inputStream, input.Length);
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
                Assert.IsTrue(secondPart.IsLast, "firstPart.IsLast");
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
