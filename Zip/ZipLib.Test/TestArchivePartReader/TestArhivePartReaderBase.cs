using System.IO;

namespace ZipLib.Test.TestArchivePartReader
{
    public class TestArhivePartReaderBase
    {
        protected Stream MakeInputStream(byte[] input)
        {
            var inputStream = new MemoryStream();
            inputStream.Write(input, 0, input.Length);
            inputStream.Seek(0, SeekOrigin.Begin);

            return inputStream;
        }
    }
}
