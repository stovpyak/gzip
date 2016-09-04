using System;

namespace ZipLib.Strategies
{
    public class SystemInfoProviderStub: ISystemInfoProvider
    {
        public SystemInfoProviderStub(int processorCount, long pagedMemorySize64, 
            ulong totalPhysInByte, ulong availPhysInByte, bool applIs64Bit)
        {
            ProcessorCount = processorCount;
            PagedMemorySize64 = pagedMemorySize64;
            TotalPhysInByte = totalPhysInByte;
            AvailPhysInByte = availPhysInByte;
            ApplIs64Bit = applIs64Bit;
        }

        public int ProcessorCount { get; }

        public long PagedMemorySize64 { get; }
        public ulong TotalPhysInByte { get; }
        public double TotalPhysInGB => Convert.ToDouble(TotalPhysInByte) / 1024 / 1024 / 1024;
        public ulong AvailPhysInByte { get; }
        public double AvailPhysInGB => Convert.ToDouble(AvailPhysInByte) / 1024 / 1024 / 1024;

        public bool ApplIs64Bit { get; }
    }
}
