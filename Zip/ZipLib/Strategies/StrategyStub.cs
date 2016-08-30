using System;

namespace ZipLib.Strategies
{
    public class StrategyStub : IStrategy
    {
        private readonly int _maxActivePartCount;
        private long _partCount = -1;
        private int _partSize = -1;

        private long _remainderFileLength;
        private int _currentPartIndex;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxActivePartCount">Максимальное кол-во частей, которые могут обрабатываться в системе одновременно</param>
        private StrategyStub(int maxActivePartCount)
        {
            _maxActivePartCount = maxActivePartCount;
        }

        public static IStrategy MakeByPartCount(int maxActivePartCount, int partCount)
        {
            var newInstance = new StrategyStub(maxActivePartCount);
            newInstance.SetPartCount(partCount);
            return newInstance;
        }

        public static IStrategy MakeByPartSize(int maxActivePartCount, int partSize)
        {
            var newInstance = new StrategyStub(maxActivePartCount);
            newInstance.SetPartSize(partSize);
            return newInstance;
        }

        private void SetPartSize(int partSize)
        {
            _partSize = partSize;
        }

        private void SetPartCount(int value)
        {
            _partCount = value;
        }

        public void StartFile(long fileSize)
        {
            _currentPartIndex = 0;
            _remainderFileLength = fileSize;

            // вычисляем _partSize или _partCount в зависимости от того что задано
            if (_partSize != -1)
            {
                _partCount = fileSize/_partSize;
                if (_partCount == 0)
                {
                    _partCount = 1;
                    if (fileSize > int.MaxValue)
                        throw new Exception("fileSize > int.MaxValue");
                    _partSize = (int)fileSize;
                }
                // остаток в дополнительную часть
                else if (fileSize % _partSize > 0)
                    _partCount++;
            }
            else if (_partCount != -1)
            {
                var lPartSize = fileSize / _partCount;

                if (lPartSize > int.MaxValue)
                    throw new Exception("размер части > int.MaxValue");
                _partSize = (int)lPartSize;

                var remainder = _remainderFileLength%_partSize;
                if (remainder > 0)
                    _partSize = _partSize + (int)remainder;
            }
        }
        
        public int GetMaxActivePartCount()
        {
            return _maxActivePartCount;
        }

        public long GetPartCount()
        {
            return _partCount;
        }

        public int GetPatrSize()
        {
            return _partSize;
        }

        public bool InitNextFilePart(FilePart part)
        {
            if (_remainderFileLength <= 0)
                return false;

            part.Index = _currentPartIndex;
            if (_remainderFileLength < _partSize)
                _partSize = (int)_remainderFileLength;
            part.SourceSize = _partSize;

            _remainderFileLength = _remainderFileLength - _partSize;
   
            _currentPartIndex++;
            return true;
        }
    }
}
