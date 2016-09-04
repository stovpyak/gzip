using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipLib.Strategies
{
    public class SmartDecompressStrategy: IDecompressStrategy
    {
        private readonly ISystemInfoProvider _systemInfoProvider;

        public SmartDecompressStrategy(ISystemInfoProvider systemInfoProvider)
        {
            _systemInfoProvider = systemInfoProvider;
        }

        public int MaxActivePartCount
        {
            get
            {
                // считаем, что масимально кол-во частей одновременно обрабатываемых в системе 
                //  правильно брать = количеству ядер + 30%
                return Convert.ToInt32(_systemInfoProvider.ProcessorCount * 1.3);
            }
        }
    }
}
