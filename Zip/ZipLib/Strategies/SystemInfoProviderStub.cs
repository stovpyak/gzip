namespace ZipLib.Strategies
{
    public class SystemInfoProviderStub: ISystemInfoProvider
    {
        public SystemInfoProviderStub(int processorCount, long availableMemoryForAppl)
        {
            ProcessorCount = processorCount;
            AvailableMemoryForAppl = availableMemoryForAppl;
        }

        public int ProcessorCount { get; }

        public long AvailableMemoryForAppl { get; }
    }
}
