using System;

namespace ZipLib.Strategies
{
    /// <summary>
    /// Стратегия - на сколько частей разбить файл и какого они долны быть размера
    /// </summary>
    public class SmartCompressStrategy: ICompressStrategy
    {
        private readonly ISystemInfoProvider _systemInfoProvider;
        private int _partSize = -1;

        public SmartCompressStrategy(ISystemInfoProvider systemInfoProvider)
        {
            _systemInfoProvider = systemInfoProvider;
        }

        public int MaxActivePartCount
        {
            get
            {
                // считаем, что масимально кол-во частей одновременно обрабатываемых в системе 
                //  правильно брать = количеству ядер + 30%
                return Convert.ToInt32(_systemInfoProvider.ProcessorCount*1.3);
            }
        }

        public int PartSize
        {
            get
            {
                if (_partSize == -1)
                    _partSize = GetPartSize();
                return _partSize;
            }
        }

        /// <summary>
        /// Размер одной части файла
        /// </summary>
        private int GetPartSize()
        {
            // считаем что все обрабатываемые части должны занимать в памяти не больше 100 МБ (как winrar)
            // на практике будет больше, так как GC будет их уничтожать позже
            const int k = 100;
            const int oneMb = 1024*1024;
            var value = oneMb*k;

            // делим на 2, так как в одной части присутствует и прочитанная часть и обработанная
            // мы заранее не знаем степень сжатия - поэтому 2
            var ulongPartSize = value/MaxActivePartCount/2;
            return (int)ulongPartSize;
        }
    }
}
