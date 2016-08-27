using System;

namespace ZipLib.Strategies
{
    public class StrategyStub : IStrategy
    {
        private int _partCount;

        private int _currentPartIndex;
        private long _remainderFileLength;
        private int _partSize;
        private long _currentStartPosition;

        public StrategyStub(int partCount)
        {
            _partCount = partCount;
        }

        public void StartFile(long fileSize)
        {
            _currentPartIndex = 0;
            _currentStartPosition = 0;
            _remainderFileLength = fileSize;

            var lPartSize = _remainderFileLength / GetMaxActivePartCount();
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
            return _partCount;
        }

        public bool InitNextFilePart(FilePart part)
        {
            if (_remainderFileLength <= 0)
                return false;

            _remainderFileLength = _remainderFileLength - _partSize;
            if (_remainderFileLength < _partSize)
                _partSize = _partSize + (int)_remainderFileLength;

            part.Name = $"FilePart N{_currentPartIndex}";
            part.StartPosition = _currentStartPosition;
            part.Size = _partSize;

            _currentPartIndex++;
            _currentStartPosition = _currentStartPosition + _partSize;

            return true;
        }
    }
}
