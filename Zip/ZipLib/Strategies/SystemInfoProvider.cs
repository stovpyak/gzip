using System;

namespace ZipLib.Strategies
{
    public class SystemInfoProvider: ISystemInfoProvider
    {
        public int ProcessorCount => Environment.ProcessorCount;

        public long AvailableMemoryForAppl
        {
            get
            {
                // В 32 - битных программах размер динамически выделяемой памяти ограничен 2 GB, в 64 - битных — 8 TB.
                // todo: потом нужно учтитывать, что оперативной памяти может быть на машине меньше

                long oneGb = 1024 * 1024 * 1024;
                var value = oneGb * 1.6;
                var valueInGb = Convert.ToInt64(value);
                return valueInGb;
            }
        }
    }
}
