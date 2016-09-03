namespace ZipLib.Strategies
{
    public class CompressStrategyStub : ICompressStrategy
    {
        public CompressStrategyStub(int maxActivePartCount, int partSize)
        {
            MaxActivePartCount = maxActivePartCount;
            PartSize = partSize;
        }

        public int MaxActivePartCount { get; }

        public int PartSize { get; }
    }
}
