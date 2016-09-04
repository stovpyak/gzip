namespace ZipLib.Strategies
{
    public interface IDecompressStrategy
    {
        int MaxActivePartCount { get; }
    }
}
