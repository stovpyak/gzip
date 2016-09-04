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

        /// <summary>
        /// Всего памяти 
        /// </summary>
        ulong TotalPhysInByte { get; }
        double TotalPhysInGB { get; }

        /// <summary>
        /// Свободной памяти
        /// </summary>
        ulong AvailPhysInByte { get; }
        double AvailPhysInGB { get; }

        bool ApplIs64Bit { get; }
    }
}
