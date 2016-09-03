namespace ZipLib.Strategies
{
    /// <summary>
    /// Возвращает информацию о системе
    /// </summary>
    public interface ISystemInfoProvider
    {
        int ProcessorCount { get; }

        long AvailableMemoryForAppl { get; }


    }
}
