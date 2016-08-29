using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipLib;
using ZipLib.Loggers;

namespace GZipTest.Test
{
    [TestClass]
    public class TestArgsParser
    {
        private ArgsParser _parser;

        [TestInitialize]
        public void TestInitialize()
        {
            var logger = new LoggerDummy();
            _parser = new ArgsParser(logger);
        }

        [TestMethod]
        public void TestNullArgs()
        {
            string[] args = null;
            var param = _parser.ParsParams(args);
            Assert.IsNull(param, "параметры должны быть null");
        }

        [TestMethod]
        public void TestNotMode()
        {
            string[] args = { "notMode" };
            var param = _parser.ParsParams(args);
            Assert.IsNull(param, "параметры должны быть null");
        }

        [TestMethod]
        public void TestNormalCompress()
        {
            var source = "sorce.txt";
            var target = "target.gz";
            string[] args = { "compress", source, target };

            var param = _parser.ParsParams(args);

            Assert.IsNotNull(param, "параметры должны быть");
            Assert.AreEqual(ApplMode.Compress, param.ApplMode, "ApplMode");
            Assert.AreEqual(source, param.SourceFileName, "SourceFileName");
            Assert.AreEqual(target, param.TargetFileName, "TargetFileName");
        }

        [TestMethod]
        public void TestNormalDecompress()
        {
            var source = "sorce.txt";
            var target = "target.gz";
            string[] args = { "decompress", source, target };

            var param = _parser.ParsParams(args);

            Assert.IsNotNull(param, "параметры должны быть");
            Assert.AreEqual(ApplMode.Decompress, param.ApplMode, "ApplMode");
            Assert.AreEqual(source, param.SourceFileName, "SourceFileName");
            Assert.AreEqual(target, param.TargetFileName, "TargetFileName");
        }
    }
}
