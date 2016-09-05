namespace ZipLib.Strategies
{
    /// <summary>
    /// Возвращает информацию о системе
    /// </summary>
    public interface ISystemInfoProvider
    {
        int ProcessorCount { get; }

        /// <summary>
        /// Сколько памяти приложение занимает в данный момент
        /// </summary>
        long PagedMemorySize64 { get; }

        bool ApplIs64Bit { get; }
    }
}
