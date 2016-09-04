using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.TeamFoundation.Common.Internal;

namespace ZipLib.Strategies
{
    public class SystemInfoProvider : ISystemInfoProvider
    {
        public int ProcessorCount => Environment.ProcessorCount;

        public ulong AvailableMemoryForAppl
        {
            get
            {
                if (!ApplIs64Bit)
                {
                    // В 32 - битных программах размер динамически выделяемой памяти ограничен 2 GB (на практие 1.75 - 1.6), 
                    const long oneGb = 1024 * 1024 * 1024;
                    const double k = 1.5;
                    var ulongValue = Convert.ToUInt64(oneGb * k);

                    // но нужно учтитывать, сколько сейчас свободно памяти
                    if (AvailPhysInByte < ulongValue)
                        return Convert.ToUInt64(AvailPhysInByte);
                    return ulongValue;
                }
                // в 64 - битных — 8 TB - то есть берем всю свободную
                return AvailPhysInByte;
            }
        }

        public bool ApplIs64Bit => Marshal.SizeOf(typeof (IntPtr)) == 8;

        /// <summary>
        /// Сколько памяти приложение занимает в данный момент
        /// </summary>
        public long PagedMemorySize64
        {
            get
            {
                var proc = Process.GetCurrentProcess();
                return proc.PagedMemorySize64;
            }
        }
        
        /// <summary>
        /// Всего памяти 
        /// </summary>
        public ulong TotalPhysInByte
        {
            get
            {
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    return memStatus.ullTotalPhys;
                }
                throw new Exception("Не удалось определить TotalPhys");
            }
        }

        public double TotalPhysInGB => Convert.ToDouble(TotalPhysInByte) / 1024 / 1024 / 1024;

        /// <summary>
        /// Свободной памяти
        /// </summary>
        public ulong AvailPhysInByte
        {
            get
            {
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    return memStatus.ullAvailPhys;
                }
                throw new Exception("Не удалось определить AvailPhys");
            }
        }

        public double AvailPhysInGB => Convert.ToDouble(AvailPhysInByte) / 1024 / 1024 / 1024;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(NativeMethods.MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}
