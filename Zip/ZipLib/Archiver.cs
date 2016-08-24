using System.IO;
using System.IO.Compression;

namespace ZipLib
{
    public class Archiver
    {
        public void Compress(string sourceFileName, string targetFileName)
        {
            FileInfo fileToCompress = new FileInfo(sourceFileName);

            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                using (FileStream compressedFileStream = File.Create(targetFileName))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                        CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public void Decompress(string sourceFileName, string targetFileName)
        {
            var fileToDecompress = new FileInfo(sourceFileName);
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                using (FileStream decompressedFileStream = File.Create(targetFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }
    }
}
