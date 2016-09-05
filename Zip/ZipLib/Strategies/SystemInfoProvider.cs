using System;
using System.Runtime.InteropServices;

namespace ZipLib.Strategies
{
    public class SystemInfoProvider : ISystemInfoProvider
    {
        public int ProcessorCount => Environment.ProcessorCount;
       
        public bool ApplIs64Bit => Marshal.SizeOf(typeof (IntPtr)) == 8;

        /// <summary>
        /// Сколько памяти приложение занимает в данный момент
        /// </summary>
        public long PagedMemorySize64
        {
            get
            {
                return GC.GetTotalMemory(false);
            }
        }
    }
}
