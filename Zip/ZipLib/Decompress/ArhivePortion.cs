using System.Collections.Generic;
using System.Diagnostics;

namespace ZipLib.Decompress
{
    /// <summary>
    /// Порция прочитанного архива
    /// </summary>
    public class ArhivePortion
    {
        private List<TitleInfo> _titlesInfo;
        private BytesBuffer _bytesBuffer;

        public ArhivePortion(BytesBuffer bytesBuffer)
        {
            _bytesBuffer = bytesBuffer;
            // сразу ищем заголовки
            _titlesInfo = TitleSearcher.GetTitlesInfo(_bytesBuffer.InnerBuffer);
        }

        public BytesBuffer ExtractAll()
        {
            var extracted = _bytesBuffer;
            Clear();
            return extracted;
        }

        /// <summary>
        /// Возвращает первый заголовок и данные после него до следующего заголовка
        /// если следующего заголовка нет, то вернет все - сам останется пустой
        /// </summary>
        /// <returns></returns>
        public BytesBuffer ExtractFirstTitleAndData()
        {
            Debug.Assert(IsNotEmpty, "порция не должена быть пустой");
            Debug.Assert(IsExistsTitle, "для извлечения заголовка и данных нужно чтобы заголовок был");
            Debug.Assert(_titlesInfo[0].Mode == TitleMode.AllTitle, "заголовок не весь, а только часть");

            BytesBuffer bytesBuffer; 

            var indexStart = _titlesInfo[0].IndexStartTitle;
            int indexEnd;
            if (_titlesInfo.Count > 1)
            {
                indexEnd = _titlesInfo[1].IndexStartTitle - 1;
                bytesBuffer = new BytesBuffer(_bytesBuffer.InnerBuffer, indexStart, indexEnd);
                // сдвигаем курсоры у своего - чтобы осталось только то что не отдал
                _bytesBuffer.StartPosition = indexEnd + 1;
                var firstTitleInfo = _titlesInfo[0];
                _titlesInfo.Remove(firstTitleInfo);
            }
            else
            {
                indexEnd = _bytesBuffer.EndPosition;
                bytesBuffer = new BytesBuffer(_bytesBuffer.InnerBuffer, indexStart, indexEnd);
                // отдал все - сам остался пустой
                Clear();
            }
            return bytesBuffer;
        }

        public bool IsExistsTitle => (_titlesInfo != null) && (_titlesInfo.Count > 0);

        public bool IsExistsAllTitle
        {
            get
            {
                if ((IsExistsTitle) && (_titlesInfo[0].Mode == TitleMode.AllTitle))
                    return true;
                return false;
            }
        }

        public bool StartsWithTitle
        {
            get
            {
                if ((IsExistsTitle) && (_titlesInfo[0].IndexStartTitle == _bytesBuffer.StartPosition))
                    return true;
                return false;
            }
        }

        public void Clear()
        {
            _bytesBuffer = null; 
            _titlesInfo = null;
        }

        public bool IsEmpty
        {
            get
            {
                if (_bytesBuffer == null)
                    return true;
                return false;
            }
        }

        public bool IsNotEmpty => !IsEmpty;
    }
}
