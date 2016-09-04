namespace ZipLib.Strategies
{
    public class DecompressStrategyStub: IDecompressStrategy
    {
        public DecompressStrategyStub(int maxActivePartCount)
        {
            MaxActivePartCount = maxActivePartCount;
        }

        public int MaxActivePartCount { get; }
    }
}
