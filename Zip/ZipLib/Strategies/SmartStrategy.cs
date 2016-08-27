using System;

namespace ZipLib.Strategies
{
    public class SmartStrategy: IStrategy
    {
        private int _currentPartIndex;

        private int _partSize;
        private long _currentStartPosition;
        private long _remainderFileLength;

        public void StartFile(long fileSize)
        {
            _currentPartIndex = 0;
            _currentStartPosition = 0;
            _remainderFileLength = fileSize;
            InitPartSize();
        }

        private void InitPartSize()
        {
            // делим на 2, так как в одной части присутствует прочитанная часть и заархивированная
            var lPartSize = GetAvailableMemoryForAppl() / GetMaxActivePartCount() / 2;
            if (lPartSize > int.MaxValue)
            {
                _partSize = int.MaxValue;
                Console.WriteLine("размер первой части > int.MaxValue");
            }
            else
                _partSize = (int)lPartSize;
        }

        public int GetMaxActivePartCount()
        {
            return Environment.ProcessorCount;
        }

        private long GetAvailableMemoryForAppl()
        {
            // пока 2 ГБ - отведенные для 32битного приложения.
            // потом нужно учтитывать, что оперативки может быть на машине меньше
            var oneGb = 1024 * 1024 * 1024;
            long valueInGb = oneGb * 1;
            return valueInGb;
        }

        public bool InitNextFilePart(FilePart part)
        {
            if (_remainderFileLength <= 0)
                return false;

            _remainderFileLength = _remainderFileLength - _partSize;
            if (_remainderFileLength < _partSize)
                _partSize = _partSize + (int)_remainderFileLength;

            part.Name = $"FilePartN{_currentPartIndex}";
            part.StartPosition = _currentStartPosition;
            part.Size = _partSize;

            _currentPartIndex++;
            _currentStartPosition = _currentStartPosition + _partSize;

            return true;
        }

        public FilePart[] GetPartsSizes(long sourceFileLength, int partCount)
        {
            var result = new FilePart[partCount];

            var partSize = sourceFileLength / partCount;
            int firstSize;
            if (partSize > int.MaxValue)
            {
                firstSize = int.MaxValue;
                Console.WriteLine("размер первой части > int.MaxValue");
            }
            else
                firstSize = (int)partSize;

            long startPosition = 0;
            for (int i = 0; i < partCount; i++)
            {
                sourceFileLength = sourceFileLength - firstSize;
                if (sourceFileLength < firstSize)
                    firstSize = firstSize + (int)sourceFileLength;

                result[i] = new FilePart();
                result[i].StartPosition = startPosition;
                result[i].Size = firstSize;
                startPosition = startPosition + firstSize;
            }
            return result;
        }
    }
}
