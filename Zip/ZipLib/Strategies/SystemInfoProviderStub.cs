namespace ZipLib.Strategies
{
    public class SystemInfoProviderStub: ISystemInfoProvider
    {
        public SystemInfoProviderStub(int processorCount, long pagedMemorySize64, 
            bool applIs64Bit)
        {
            ProcessorCount = processorCount;
            PagedMemorySize64 = pagedMemorySize64;
            ApplIs64Bit = applIs64Bit;
        }

        public int ProcessorCount { get; }

        public long PagedMemorySize64 { get; }

        public bool ApplIs64Bit { get; }
    }
}
