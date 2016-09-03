namespace ZipLib.Strategies
{
    public interface ICompressStrategy
    {
        int MaxActivePartCount { get; }
        int PartSize { get; }
    }
}
