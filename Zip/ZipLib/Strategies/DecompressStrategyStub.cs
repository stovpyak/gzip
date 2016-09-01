namespace ZipLib.Strategies
{
    public class DecompressStrategyStub: IDecompressStrategy
    {
        private readonly int _maxActivePartCount;

        public DecompressStrategyStub(int maxActivePartCount)
        {
            _maxActivePartCount = maxActivePartCount;
        }

        public int GetMaxActivePartCount()
        {
            return _maxActivePartCount;
        }
    }
}
