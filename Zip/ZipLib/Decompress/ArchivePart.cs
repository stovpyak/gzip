using System.Diagnostics;
using System.IO;

namespace ZipLib.Decompress
{
    /// <summary>
    /// Часть архива - полностью пригодная для декомпресси
    /// Заголовок + данные
    /// </summary>
    public class ArchivePart
    {
        private MemoryStream _stream;

        /// <summary>
        /// добавить в часть всю порцию
        /// </summary>
        /// <param name="portion"></param>
        public void AppendAllPortion(ArhivePortion portion)
        {
            if (_stream == null)
                _stream = new MemoryStream();
            var data = portion.ExtractAll();
            Debug.Assert(portion.IsEmpty, "Из порции извлекли всё, а она не пустая");
            _stream.Write(data.InnerBuffer, data.StartPosition, data.Size);
        }

        /// <summary>
        /// записываем в часть все что до следеющего заголовка
        /// </summary>
        public void AppendTitleAndDataBeforeNextTitle(ArhivePortion portion)
        {
            if (_stream == null)
                _stream = new MemoryStream();
            var data = portion.ExtractFirstTitleAndData();
            _stream.Write(data.InnerBuffer, data.StartPosition, data.Size);
        }

        public void AppendDataBeforeTitle(ArhivePortion portion)
        {

        }

        public bool IsEmpty => _stream == null;

        public bool IsNotEmpty => !IsEmpty;

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }
    }
}
