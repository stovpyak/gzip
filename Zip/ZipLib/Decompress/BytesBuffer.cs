using System;

namespace ZipLib.Decompress
{
    /// <summary>
    /// Буффер. 
    /// Чтобы не делать копии массива байт, а ссылаться на один массив но с разными значениями "курсоров"
    /// </summary>
    public class BytesBuffer
    {
        private readonly byte[] _buffer;
        private int _endPosition;

        public BytesBuffer(byte[] buffer, int startPosition, int endPosition)
        {
            if (buffer == null)
                throw new ArgumentException("_buffer == null");
            _buffer = buffer;

            if (startPosition > _buffer.Length)
                throw new ArgumentException("startPosition >  _buffer.Length");
            StartPosition = startPosition;

            if (endPosition > _buffer.Length)
                throw new ArgumentException("endPosition >  _buffer.Length");
            _endPosition = endPosition;
        }

        public byte[] InnerBuffer => _buffer;

        public int StartPosition { get; set; }

        public int EndPosition => _endPosition;

        public int Size => _endPosition - StartPosition + 1;
    }
}
