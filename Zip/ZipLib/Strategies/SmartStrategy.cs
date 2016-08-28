﻿using System;

namespace ZipLib.Strategies
{
    public class SmartStrategy: IStrategy
    {
        private int _currentPartIndex;

        private int _partSize;
        private long _partCount;

        private long _currentStartPosition;
        private long _remainderFileLength;

        public void StartFile(long fileSize)
        {
            _currentPartIndex = 0;
            _currentStartPosition = 0;
            _remainderFileLength = fileSize;
            InitPartSize();
            InitPartCount();
        }

        private void InitPartSize()
        {
            // делим на 2, так как в одной части присутствует прочитанная часть и заархивированная
            var lPartSize = GetAvailableMemoryForAppl() / GetMaxActivePartCount() / 2;
            if (lPartSize > int.MaxValue)
                throw new Exception("размер части > int.MaxValue");
            _partSize = (int)lPartSize;
        }

        private void InitPartCount()
        {
            var count = _remainderFileLength / _partSize;
            if (_remainderFileLength % _partSize > 0)
                count++;
            _partCount = count;
        }

        public int GetMaxActivePartCount()
        {
            return Convert.ToInt32(Environment.ProcessorCount * 1.3);
        }

        public long GetPartCount()
        {
            return _partCount;
        }

        /// <summary>
        /// Возвращает доступный объем памяти для приложения
        /// </summary>
        /// <returns></returns>
        private long GetAvailableMemoryForAppl()
        {
            // В 32 - битных программах размер динамически выделяемой памяти ограничен 2 GB, в 64 - битных — 8 TB.
            // потом нужно учтитывать, что оперативки может быть на машине меньше

            long oneGb = 1024 * 1024 * 1024;
            var value = oneGb * 1.6;
            long valueInGb = Convert.ToInt64(value);
            return valueInGb;
        }

        public bool InitNextFilePart(FilePart part)
        {
            if (_remainderFileLength <= 0)
                return false;

            part.Index = _currentPartIndex;
            part.StartPosition = _currentStartPosition;
            if (_remainderFileLength < _partSize)
                _partSize = (int)_remainderFileLength;
            part.SourceSize = _partSize;

            _remainderFileLength = _remainderFileLength - _partSize;
            _currentStartPosition = _currentStartPosition + _partSize;

            _currentPartIndex++;
            return true;
        }
    }
}
