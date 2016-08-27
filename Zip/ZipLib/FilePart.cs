using System.IO;

namespace ZipLib
{
    public class FilePart
    {
        public string Name { get; set; }
        public long StartPosition { get; set; }
        public int Size { get; set; }
        public byte[] Source { get; set; }
        public byte[] Result { get; set; }
    }
}
