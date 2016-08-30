namespace ZipLib.Decompress
{
    /// <summary>
    /// Порция прочитанного архива
    /// </summary>
    public class ArhivePortion
    {
        private byte[] _buffer;
        private int _bufferSize;

        public ArhivePortion(byte[] buffer, int bufferSize)
        {
            _buffer = buffer;
            _bufferSize = bufferSize;

            var searchResult = TitleSearcher.GetIndexTitle(buffer);
        }
    }
}
